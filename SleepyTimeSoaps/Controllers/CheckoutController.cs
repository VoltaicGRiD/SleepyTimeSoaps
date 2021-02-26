using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Square;
using Square.Models;
using SleepyTimeSoaps.Models;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using Square.Exceptions;
using System.Diagnostics;
using System.Net;
using System.Data.SqlClient;
using PayPal.Api;
using System.Web.Security;
using Square.Apis;

namespace SleepyTimeSoaps.Controllers
{
    public class CheckoutController : Controller
    {
        // GET: Checkout
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult SquareResponse()
        {
            string code = Request.QueryString["code"];

            return null;
        }

        [Authorize]
        public ActionResult NewCheckout()
        {
            SqlConnection oConn = new SqlConnection(AccessModel.SqlConnection);
            oConn.Open();

            var user = Membership.GetUser(System.Web.HttpContext.Current.User.Identity.Name);
            string email = string.Empty;

            if (user != null)
                email = user.Email;
            else
                return RedirectToAction("Login", "Account");

            string CommandText = $"SELECT CustomerCart FROM Customers WHERE CustomerEmail LIKE '%{email}%'";
            SqlCommand oCommand = new SqlCommand(CommandText, oConn);

            string CurrentCart = string.Empty;

            bool UserExists = false;

            using (SqlDataReader oReader = oCommand.ExecuteReader())
            {
                if (oReader.HasRows)
                {
                    UserExists = true;

                    while (oReader.Read())
                    {
                        CurrentCart = oReader.GetString(0).TrimStart(';').TrimEnd(';').Trim();
                    }
                }
            }

            if (UserExists == false)
            {
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrWhiteSpace(CurrentCart))
            {
                ViewBag.Response = "You have no items in your bag. You need to add items before you can review your cart.";

                ProductsModel PM = new ProductsModel();

                return View("ReviewCart", PM);
            }

            else
            {
                List<Product> Products = new List<Product>();
                foreach (string s in CurrentCart.Split(';'))
                {
                    Product newProduct = new Product();

                    List<string> innerData = new List<string>();
                    innerData = s.Replace('}', '\0').Replace('{', '\0').Trim().Split(',').ToList();

                    newProduct.ProductID = int.Parse(innerData[0].Split('=')[1].Trim());
                    newProduct.Quantity = int.Parse(innerData[1].Split('=')[1].Trim());
                    newProduct.Naked = innerData[2].Split('=')[1].Trim().Contains("Naked") ? true : false;

                    Products.Add(newProduct);
                }

                string ProductDataCommandText = "SELECT ProductID, ProductName, ProductDescription, ProductPrice, ProductStock FROM Products WHERE ProductID=@id";
                SqlCommand ProductDataCommand = new SqlCommand(ProductDataCommandText, oConn);

                foreach (Product p in Products)
                {
                    ProductDataCommand.Parameters.Clear();
                    ProductDataCommand.Parameters.AddWithValue("@id", p.ProductID);

                    using (SqlDataReader oReader = ProductDataCommand.ExecuteReader())
                    {
                        while (oReader.HasRows && oReader.Read())
                        {
                            p.ProductID = oReader.GetInt32(0);
                            p.ProductName = oReader.GetString(1);
                            p.ProductDescription = oReader.GetString(2);
                            p.ProductPrice = float.Parse(oReader.GetValue(3).ToString());
                            p.ProductStock = oReader.GetInt32(4);
                        }
                    }
                }

                //string InsertOrderCommandText = "INSERT INTO Orders";

                CheckoutModel Model = new CheckoutModel();
                Model._Products = Products;

                return View("Checkout", Model);
            }
        }

        [Authorize]
        public ActionResult SquareCheckout()
        {
            ////https://connect.squareup.com/oauth2/authorize?client_id=sq0idp-lzuru296bDZusOTtDxaHww

#if DEBUG
            string _locationID = "LCRKWECB5YE7A";

            SquareClient squareClient = new SquareClient.Builder()
                .Environment(Square.Environment.Sandbox)
                .AccessToken("EAAAEPpSBAov7l9FzuIAlLh9MBJHEV7GdH-oHaR16kYJRAq0fW9bFllz3FAIdvNR")
                .Build();
#endif
#if (!DEBUG)
            string _locationID = "L39ZKZNWZ1J2M";

            SquareClient squareClient = new SquareClient.Builder()
                .Environment(Square.Environment.Production)
                .AccessToken("EAAAEPmiyCPbe9lZN9x47KvqI8WvZhLtovnOBW0qIs1FkhSjQkxF1kLUUT6kkQyx")
                .Build();
#endif

            SqlConnection oConn = new SqlConnection(AccessModel.SqlConnection);
            oConn.Open();

            var user = Membership.GetUser(System.Web.HttpContext.Current.User.Identity.Name);
            string email = string.Empty;

            if (user != null)
                email = user.Email;

            string CommandText = $"SELECT CustomerCart FROM Customers WHERE CustomerEmail LIKE '%{email}%'";
            SqlCommand oCommand = new SqlCommand(CommandText, oConn);

            string CurrentCart = string.Empty;

            bool UserExists = false;

            using (SqlDataReader oReader = oCommand.ExecuteReader())
            {
                if (oReader.HasRows)
                {
                    UserExists = true;

                    while (oReader.Read())
                    {
                        CurrentCart = oReader.GetString(0);
                    }
                }
            }

            if (UserExists == false)
            {
                return RedirectToAction("Login", "Account");
            }

            List<Product> Products = new List<Product>();
            foreach (string s in CurrentCart.Split(';'))
            {
                Product newProduct = new Product();

                List<string> innerData = new List<string>();
                innerData = s.Replace('}', '\0').Replace('{', '\0').Trim().Split(',').ToList();

                newProduct.ProductID = int.Parse(innerData[0].Split('=')[1].Trim());
                newProduct.Quantity = int.Parse(innerData[1].Split('=')[1].Trim());
                newProduct.Naked = innerData[2].Split('=')[1].Trim().Contains("Naked") ? true : false;

                Products.Add(newProduct);
            }

            string ProductDataCommandText = "SELECT ProductName, ProductPrice, ProductStock FROM Products WHERE ProductID=@id";
            SqlCommand ProductDataCommand = new SqlCommand(ProductDataCommandText, oConn);

            foreach (Product p in Products)
            {
                ProductDataCommand.Parameters.Clear();
                ProductDataCommand.Parameters.AddWithValue("@id", p.ProductID);

                using (SqlDataReader oReader = ProductDataCommand.ExecuteReader())
                {
                    while (oReader.HasRows && oReader.Read())
                    {
                        p.ProductName = oReader.GetString(0);
                        p.ProductPrice = float.Parse(oReader.GetValue(1).ToString());
                        p.ProductStock = oReader.GetInt32(2);
                    }
                }
            }

            string SquareIDCommandText = "SELECT ProductSquareId FROM Products WHERE ProductId=@id";
            SqlCommand SquareIDCommand = new SqlCommand(SquareIDCommandText, oConn);

            var lineItems = new List<OrderLineItem>();

            foreach (Product product in Products)
            {
                SquareIDCommand.Parameters.Clear();
                SquareIDCommand.Parameters.AddWithValue("@id", product.ProductID);

                string ProductSquareID = SquareIDCommand.ExecuteScalar().ToString();

                string Naked = product.Naked ? "Naked" : "Wrapped";

                var orderLineItem = new OrderLineItem.Builder(quantity: product.Quantity.ToString()).CatalogObjectId(ProductSquareID).Note(product.ProductName + " : " + Naked).Build();
                lineItems.Add(orderLineItem);
            }

            var order1 = new Square.Models.Order.Builder(locationId: _locationID)
              .LineItems(lineItems)
              .Build();

            var order = new CreateOrderRequest.Builder()
              .Order(order1)
              .LocationId(_locationID)
              .IdempotencyKey(Guid.NewGuid().ToString())
              .Build();

            var checkoutBody = new CreateCheckoutRequest.Builder(idempotencyKey: Guid.NewGuid().ToString(), order: order)
              .AskForShippingAddress(true)
              .Build();

            CreateCheckoutResponse CheckoutResponse = null;

            try
            {
                CheckoutResponse = Task.Run(async () => await squareClient.CheckoutApi.CreateCheckoutAsync(locationId: _locationID, body: checkoutBody)).Result;
            }
            catch (ApiException e)
            {
                Console.WriteLine("Failed to make the request");
                Console.WriteLine($"Response Code: {e.ResponseCode}");
                Console.WriteLine($"Exception: {e.Message}");
            }

            return Redirect(CheckoutResponse.Checkout.CheckoutPageUrl);
        }

        public ActionResult UpdateShipping(string firstname, string email, string address, string city, string state, string zip, string cardname, string cardnumber, string expmonth, string expyear, string cvv)
        {


            return null;
        }

        public ActionResult SubmitPayment()
        {
#if DEBUG
            string _locationID = "LCRKWECB5YE7A";

            SquareClient squareClient = new SquareClient.Builder()
                .Environment(Square.Environment.Sandbox)
                .AccessToken("EAAAEPpSBAov7l9FzuIAlLh9MBJHEV7GdH-oHaR16kYJRAq0fW9bFllz3FAIdvNR")
                .Build();
#endif
#if (!DEBUG)
            string _locationID = "L39ZKZNWZ1J2M";

            SquareClient squareClient = new SquareClient.Builder()
                .Environment(Square.Environment.Production)
                .AccessToken("EAAAEPmiyCPbe9lZN9x47KvqI8WvZhLtovnOBW0qIs1FkhSjQkxF1kLUUT6kkQyx")
                .Build();
#endif

            string nonce = Request.Form["nonce"];

            IPaymentsApi PaymentsApi = squareClient.PaymentsApi;
            // Every payment you process with the SDK must have a unique idempotency key.
            // If you're unsure whether a particular payment succeeded, you can reattempt
            // it with the same idempotency key without worrying about double charging
            // the buyer.
            string uuid = Guid.NewGuid().ToString();

            // Monetary amounts are specified in the smallest unit of the applicable currency.
            // This amount is in cents. It's also hard-coded for $1.00,
            // which isn't very useful.
            Money amount = new Money.Builder()
                .Amount(520)
                .Currency("USD")
                .Build();

            // To learn more about splitting payments with additional recipients,
            // see the Payments API documentation on our [developer site]
            // (https://developer.squareup.com/docs/payments-api/overview).
            CreatePaymentRequest createPaymentRequest = new CreatePaymentRequest.Builder(nonce, uuid, amount)
                .Note("From Square Sample Csharp App")
                .Build();

            try
            {
                CreatePaymentResponse response = PaymentsApi.CreatePayment(createPaymentRequest);

                ViewBag.Response = "Payment complete! " + response.Payment.Note;
                return View("Confirmation");
            }
            catch (ApiException e)
            {
                ViewBag.Response = e.Message;
                return View("Confirmation");
            }
        }

        public ActionResult Confirmation()
        {
            return View();
        }

        public ActionResult PaypalCheckout()
        {
            //HttpCookie cartCookie = Request.Cookies["cartCookie"];
            //if (cartCookie != null)
            //{
            //    string cartValues = cartCookie.Value;

            //    var apiContext = new APIContext(AccessModel.PaypalAppAccessCode);

            //    apiContext.Config = ConfigManager.Instance.GetProperties();
            //    apiContext.Config["connectionTimeout"] = "1000";

            //    if (apiContext.HTTPHeaders == null)
            //    {
            //        apiContext.HTTPHeaders = new Dictionary<string, string>();
            //    }

            //    SqlConnection oConn = new SqlConnection(AccessModel.SqlConnection);
            //    oConn.Open();

            //    string CommandText = "SELECT ProductName,ProductPrice FROM Products WHERE ProductId=@id";
            //    SqlCommand oCommand = new SqlCommand(CommandText, oConn);

            //    List<Item> items = new List<Item>();
            //    foreach (string productid in cartValues.Split(','))
            //    {
            //        oCommand.Parameters.Clear();
            //        oCommand.Parameters.AddWithValue("@id", productid);

            //        using (SqlDataReader oReader = oCommand.ExecuteReader())
            //        {
            //            while (oReader.HasRows && oReader.Read())
            //            {
            //                items.Add(new Item()
            //                {
            //                    name = oReader.GetString(0),
            //                    currency = "USD",
            //                    price = oReader.GetValue(1).ToString(),
            //                    quantity = "1"
            //                });
            //            }
            //        }
            //    }

            //    string payerId = Request.Params["PayerID"];

            //    //if (string.IsNullOrWhiteSpace(payerId))
            //    //{
            //    var ItemList = new ItemList()
            //    {
            //        items = items
            //    };

            //    var payer = new Payer() { payment_method = "paypal" };

            //    var baseURI = Request.Url.Scheme + "://" + Request.Url.Authority + "/Checkout/PaypalSuccess";
            //    var guid = Convert.ToString((new Random()).Next(100000));
            //    var redirectUrl = baseURI + "guid=" + guid;
            //    var redirUrls = new RedirectUrls()
            //    {
            //        cancel_url = redirectUrl + "&cancel=true",
            //        return_url = redirectUrl
            //    };

            //    var details = new Details()
            //    {
            //        tax = "15",
            //        shipping = "10",
            //        subtotal = "75"
            //    };

            //    var amount = new Amount()
            //    {
            //        currency = "USD",
            //        total = "100.00", // Total must be equal to sum of shipping, tax and subtotal.
            //        details = details
            //    };

            //    var transactionList = new List<PayPal.Api.Transaction>();

            //    transactionList.Add(new PayPal.Api.Transaction()
            //    {
            //        description = "Transaction description.",
            //        invoice_number = Common.GetRandomInvoiceNumber(),
            //        amount = amount,
            //        item_list = ItemList
            //    });

            //    var payment = new PayPal.Api.Payment()
            //    {
            //        intent = "sale",
            //        payer = payer,
            //        transactions = transactionList,
            //        redirect_urls = redirUrls
            //    };

            //    var createdPayment = payment.Create(apiContext);

            //    var links = createdPayment.links.GetEnumerator();
            //    while (links.MoveNext())
            //    {
            //        var link = links.Current;
            //        if (link.rel.ToLower().Trim().Equals("approval_url"))
            //        {
            //            this.flow.RecordRedirectUrl("Redirect to PayPal to approve the payment...", link.href);
            //        }
            //    }
            //    Session.Add(guid, createdPayment.id);
            //    Session.Add("flow-" + guid, this.flow);4

            //    //}

            return null;
        }
    }
}