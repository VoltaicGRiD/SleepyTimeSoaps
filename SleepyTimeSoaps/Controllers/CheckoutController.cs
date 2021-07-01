﻿using SleepyTimeSoaps.Models;
using Square;
using Square.Apis;
using Square.Exceptions;
using Square.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Security;
using SendGrid;
using SendGrid.Helpers;
using SendGrid.Helpers.Mail;
using System.Configuration;
using System.Diagnostics;

namespace SleepyTimeSoaps.Controllers
{
    public class CheckoutController : Controller
    {

        public static CheckoutModel Model = new CheckoutModel();

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

            string CommandText = $"SELECT CustomerID, CustomerCart, CustomerAddress, CustomerOrders, ActiveOrder FROM Customers WHERE CustomerEmail LIKE '%{email}%'";
            SqlCommand oCommand = new SqlCommand(CommandText, oConn);

            CustomerModel CurrentCustomer = new CustomerModel();

            bool UserExists = false;

            using (SqlDataReader oReader = oCommand.ExecuteReader())
            {
                if (oReader.HasRows)
                {
                    UserExists = true;

                    while (oReader.Read())
                    {
                        CurrentCustomer.CustomerID = oReader.GetGuid(0).ToString();
                        CurrentCustomer.CustomerCart = oReader.GetString(1);
                        CurrentCustomer.CustomerAddress = oReader.IsDBNull(2) ? string.Empty : oReader.GetString(2);
                        CurrentCustomer.CustomerOrders = oReader.IsDBNull(3) ? string.Empty : oReader.GetString(3);
                        CurrentCustomer.ActiveOrder = oReader.IsDBNull(4) ? 0 : oReader.GetInt32(4);
                    }
                }
            }

            if (UserExists == false)
            {
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrWhiteSpace(CurrentCustomer.CustomerCart))
            {
                TempData["response"] = "You have no items in your bag. You need to add items before you can review your cart.";

                CheckoutModel PM = new CheckoutModel();

                return RedirectToAction("ReviewCart", "Cart", PM);
            }

            else
            {
                List<Product> Products = new List<Product>();
                foreach (string s in CurrentCustomer.CustomerCart.Split(';'))
                {
                    Product newProduct = new Product();

                    List<string> innerData = new List<string>();
                    innerData = s.Replace('}', '\0').Replace('{', '\0').Trim().Split(',').ToList();

                    if (!string.IsNullOrWhiteSpace(innerData[0]))
                    {
                        newProduct.ProductID = int.Parse(innerData[0].Split('=')[1].Trim());
                        newProduct.Quantity = int.Parse(innerData[1].Split('=')[1].Trim());
                        newProduct.Naked = innerData[2].Split('=')[1].Trim().Contains("Naked") ? true : false;

                        if (innerData.Count > 3)
                        {
                            for (int i = 3; i < innerData.Count; i++)
                            {
                                newProduct._SelectedAttributes.Add(innerData[i]);
                            }
                        }

                        Products.Add(newProduct);
                    }
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

                foreach (Product p in Products)
                {
                    if (p.Quantity > p.ProductStock)
                    {
                        TempData["response"] = $"We currently only have {p.ProductStock} stock of {p.ProductName}. Please purchase less than that quantity.";

                        CheckoutModel PM = new CheckoutModel();

                        return RedirectToAction("ReviewCart", "Cart", PM);
                    }
                }

                Model._Products = Products;

                string GetActiveDiscountsCommandText = "SELECT * FROM Discounts WHERE AlwaysActive=1";
                SqlCommand GetActiveDiscountsCommand = new SqlCommand(GetActiveDiscountsCommandText, oConn);

                using (SqlDataReader oReader = GetActiveDiscountsCommand.ExecuteReader())
                {
                    while (oReader.HasRows && oReader.Read())
                    {
                        Model.DiscountApplied = true;
                        Model.DiscountName = oReader.GetString(1);
                        Model.DiscountPercentage = oReader.GetInt32(3);
                    }
                }

                int ReturnedOrderID = 0;

                if (!Model.DiscountApplied)
                {
                    string InsertOrderCommandText = "INSERT INTO Orders (CustomerID, ProductCount, Cart, OrderProcessed, OrderProcessor, OrderTotal) OUTPUT INSERTED.OrderID VALUES (@id, @count, @cart, @processed, @processor, @total);";
                    SqlCommand InsertOrderCommand = new SqlCommand(InsertOrderCommandText, oConn);
                    InsertOrderCommand.Parameters.AddWithValue("@id", CurrentCustomer.CustomerID);
                    InsertOrderCommand.Parameters.AddWithValue("@count", Products.Count);
                    InsertOrderCommand.Parameters.AddWithValue("@cart", CurrentCustomer.CustomerCart);
                    InsertOrderCommand.Parameters.AddWithValue("@processed", 0);
                    InsertOrderCommand.Parameters.AddWithValue("@processor", "Square");
                    InsertOrderCommand.Parameters.AddWithValue("@total", Model.CartTotal);

                    ReturnedOrderID = int.Parse(InsertOrderCommand.ExecuteScalar().ToString());
                }
                else
                {
                    string InsertOrderCommandText = "INSERT INTO Orders (CustomerID, ProductCount, Cart, OrderProcessed, OrderProcessor, OrderTotal, OrderDiscount, OrderDiscountName) OUTPUT INSERTED.OrderID VALUES (@id, @count, @cart, @processed, @processor, @total, @discount, @discountname);";
                    SqlCommand InsertOrderCommand = new SqlCommand(InsertOrderCommandText, oConn);
                    InsertOrderCommand.Parameters.AddWithValue("@id", CurrentCustomer.CustomerID);
                    InsertOrderCommand.Parameters.AddWithValue("@count", Products.Count);
                    InsertOrderCommand.Parameters.AddWithValue("@cart", CurrentCustomer.CustomerCart);
                    InsertOrderCommand.Parameters.AddWithValue("@processed", 0);
                    InsertOrderCommand.Parameters.AddWithValue("@processor", "Square");
                    InsertOrderCommand.Parameters.AddWithValue("@total", Model.CartTotal);
                    InsertOrderCommand.Parameters.AddWithValue("@discount", Model.DiscountPercentage);
                    InsertOrderCommand.Parameters.AddWithValue("@discountname", Model.DiscountName);

                    ReturnedOrderID = int.Parse(InsertOrderCommand.ExecuteScalar().ToString());
                }

                string UpdateActiveOrderCommandText = $"UPDATE Customers SET ActiveOrder=@orderid WHERE CustomerEmail LIKE '%{email}%'";
                SqlCommand UpdateActiveOrderCommand = new SqlCommand(UpdateActiveOrderCommandText, oConn);
                UpdateActiveOrderCommand.Parameters.AddWithValue("@orderid", ReturnedOrderID);

                UpdateActiveOrderCommand.ExecuteNonQuery();

                Model.OrderID = ReturnedOrderID;

                oConn.Close();

                return View("Checkout", Model);
            }
        }

        public ActionResult SubmitDiscountCode(string code, int OrderID)
        {
            string promocode = code.Trim();

            SqlConnection oConn = new SqlConnection(AccessModel.SqlConnection);
            oConn.Open();

            string CommandText = "SELECT DiscountName, DiscountPercentage FROM Discounts WHERE DiscountCode=@code";
            SqlCommand oCommand = new SqlCommand(CommandText, oConn);
            oCommand.Parameters.AddWithValue("@code", promocode);

            DiscountModel newDiscount = new DiscountModel();

            bool DiscountValid = false;

            using (SqlDataReader oReader = oCommand.ExecuteReader())
            {
                while (oReader.HasRows && oReader.Read())
                {
                    DiscountValid = true;

                    newDiscount.DiscountName = oReader.GetString(0);
                    newDiscount.DiscountPercentage = oReader.GetInt32(1);

                    Model.DiscountApplied = true;
                    Model.DiscountName = oReader.GetString(0);
                    Model.DiscountPercentage = oReader.GetInt32(1);
                }
            }

            if (DiscountValid)
            {
                string UpdateOrderCommandText = "UPDATE Orders SET OrderDiscount=@percent, OrderDiscountName=@name WHERE OrderID=@id";
                SqlCommand UpdateOrderCommand = new SqlCommand(UpdateOrderCommandText, oConn);
                UpdateOrderCommand.Parameters.AddWithValue("@percent", newDiscount.DiscountPercentage);
                UpdateOrderCommand.Parameters.AddWithValue("@name", newDiscount.DiscountName);
                UpdateOrderCommand.Parameters.AddWithValue("@id", OrderID);

                UpdateOrderCommand.ExecuteNonQuery();
            }
            else
            {
                TempData["response"] = "Discount code is invalid, you may have typed it wrong.\nIf the issue continues, please contact support at support@sleepytimesoaps.com";
            }

            oConn.Close();

            return RedirectToAction("Checkout", Model);
        }

        public ActionResult Checkout()
        {
            try
            {
                if (TempData["response"] != null || !string.IsNullOrWhiteSpace(TempData["response"].ToString()))
                    ViewBag.Response = TempData["response"].ToString();
            }
            catch (NullReferenceException exc)
            {

            }

            SqlConnection oConn = new SqlConnection(AccessModel.SqlConnection);
            oConn.Open();

            var user = Membership.GetUser(System.Web.HttpContext.Current.User.Identity.Name);
            string email = string.Empty;

            if (user != null)
                email = user.Email;
            else
                return RedirectToAction("Login", "Account");

            string CommandText = $"SELECT CustomerID, CustomerCart, CustomerAddress, CustomerOrders, ActiveOrder FROM Customers WHERE CustomerEmail LIKE '%{email}%'";
            SqlCommand oCommand = new SqlCommand(CommandText, oConn);

            CustomerModel CurrentCustomer = new CustomerModel();

            bool UserExists = false;

            using (SqlDataReader oReader = oCommand.ExecuteReader())
            {
                if (oReader.HasRows)
                {
                    UserExists = true;

                    while (oReader.Read())
                    {
                        CurrentCustomer.CustomerID = oReader.GetGuid(0).ToString();
                        CurrentCustomer.CustomerCart = oReader.GetString(1);
                        CurrentCustomer.CustomerAddress = oReader.IsDBNull(2) ? string.Empty : oReader.GetString(2);
                        CurrentCustomer.CustomerOrders = oReader.IsDBNull(3) ? string.Empty : oReader.GetString(3);
                        CurrentCustomer.ActiveOrder = oReader.IsDBNull(4) ? 0 : oReader.GetInt32(4);
                    }
                }
            }

            if (UserExists == false)
            {
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrWhiteSpace(CurrentCustomer.CustomerCart))
            {
                TempData["response"] = "You have no items in your bag. You need to add items before you can review your cart.";

                ProductsModel PM = new ProductsModel();

                return RedirectToAction("ReviewCart", "Cart", PM);
            }

            else
            {
                List<Product> Products = new List<Product>();
                foreach (string s in CurrentCustomer.CustomerCart.Split(';'))
                {
                    Product newProduct = new Product();

                    List<string> innerData = new List<string>();
                    innerData = s.Replace('}', '\0').Replace('{', '\0').Trim().Split(',').ToList();

                    if (!string.IsNullOrWhiteSpace(innerData[0]))
                    {
                        newProduct.ProductID = int.Parse(innerData[0].Split('=')[1].Trim());
                        newProduct.Quantity = int.Parse(innerData[1].Split('=')[1].Trim());
                        newProduct.Naked = innerData[2].Split('=')[1].Trim().Contains("Naked") ? true : false;

                        if (innerData.Count > 3)
                        {
                            for (int i = 3; i < innerData.Count; i++)
                            {
                                newProduct._SelectedAttributes.Add(innerData[i]);
                            }
                        }

                        Products.Add(newProduct);
                    }
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

                foreach (Product p in Products)
                {
                    if (p.Quantity > p.ProductStock)
                    {
                        TempData["response"] = $"We currently only have {p.ProductStock} stock of {p.ProductName}. Please purchase less than that quantity.";

                        ProductsModel PM = new ProductsModel();

                        return RedirectToAction("ReviewCart", "Cart", PM);
                    }
                }

                Model._Products = Products;

                string OrderCommandText = "SELECT ShippingInfo FROM Orders WHERE OrderID=@orderid";
                SqlCommand OrderCommand = new SqlCommand(OrderCommandText, oConn);
                OrderCommand.Parameters.AddWithValue("@orderid", CurrentCustomer.ActiveOrder);

                using (SqlDataReader oReader = OrderCommand.ExecuteReader())
                {
                    while (oReader.HasRows && oReader.Read())
                    {
                        Model.ShippingInfo = oReader.IsDBNull(0) ? string.Empty : oReader.GetString(0);
                    }
                }

                return View(Model);
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

        public ActionResult UpdateShipping(CheckoutModel Model, int OrderID, string firstname, string lastname, string email, string address, string address2, string city, string state, string zip)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(firstname + " " + lastname);
            sb.AppendLine(address);
            sb.AppendLine(address2);
            sb.AppendLine(city + ", " + state + " " + zip);

            SqlConnection oConn = new SqlConnection(AccessModel.SqlConnection);
            oConn.Open();

            string CommandText = "UPDATE Orders SET ShippingInfo=@shipping WHERE OrderID=@orderid";
            SqlCommand oCommand = new SqlCommand(CommandText, oConn);
            oCommand.Parameters.AddWithValue("@shipping", sb.ToString());
            oCommand.Parameters.AddWithValue("@orderid", OrderID);

            oCommand.ExecuteNonQuery();

            oConn.Close();

            TempData["response"] = "Shipping information successfully updated.";
            return RedirectToAction("Checkout", Model);
        }

        public ActionResult SubmitPayment(CheckoutModel _Model, float CartTotal, int OrderID, string ShippingInfo)
        {
            if (string.IsNullOrWhiteSpace(ShippingInfo))
            {
                TempData["response"] = "You need to provide shipping information first. Please ensure you've selected 'Update Shipping Information' before proceeding to payment.";
                return RedirectToAction("Checkout", _Model);
            }

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

            int FinalTotal = int.Parse((CartTotal * 100).ToString());

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
                .Amount(FinalTotal)
                .Currency("USD")
                .Build();

            // To learn more about splitting payments with additional recipients,
            // see the Payments API documentation on our [developer site]
            // (https://developer.squareup.com/docs/payments-api/overview).
            CreatePaymentRequest createPaymentRequest = new CreatePaymentRequest.Builder(nonce, uuid, amount)
                .Note("<p>You'll receive the following via email:<br/>confirmation of order recieved, <br/>an invoice, <br/>and status updates as your order is prepared and shipped.</p>")
                .Build();

            try
            {
                CreatePaymentResponse response = PaymentsApi.CreatePayment(createPaymentRequest);

                SqlConnection oConn = new SqlConnection(AccessModel.SqlConnection);
                oConn.Open();

                string CommandText = "UPDATE Orders SET OrderProcessed=1 WHERE OrderID=@orderid";
                SqlCommand oCommand = new SqlCommand(CommandText, oConn);

                oCommand.Parameters.AddWithValue("@orderid", OrderID);
                oCommand.ExecuteNonQuery();

                var user = Membership.GetUser(System.Web.HttpContext.Current.User.Identity.Name);
                string email = string.Empty;

                if (user != null)
                    email = user.Email;
                else
                    return RedirectToAction("Login", "Account");

                // TODO :: Update this function to properly work if var email is empty / unassigned
                if (!string.IsNullOrWhiteSpace(email))
                {
                    string ClearCartCommandText = $"UPDATE Customers SET CustomerCart='Empty' WHERE CustomerEmail=@email";
                    SqlCommand ClearCartCommand = new SqlCommand(ClearCartCommandText, oConn);

                    ClearCartCommand.Parameters.AddWithValue("@email", email);

                    ClearCartCommand.ExecuteNonQuery();
                }

                oConn.Close();

                ViewBag.CheckoutResponse = "<h4>Payment complete!<h4> <br/>" + response.Payment.Note;
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

        public ActionResult NewMarketCheckout()
        {
            SqlConnection oConn = new SqlConnection(AccessModel.SqlConnection);
            oConn.Open();

            var user = Membership.GetUser(System.Web.HttpContext.Current.User.Identity.Name);
            string email = string.Empty;

            if (user != null)
                email = user.Email;
            else
                return RedirectToAction("Login", "Account");

            string CommandText = $"SELECT CustomerID, CustomerCart, CustomerAddress, CustomerOrders, ActiveOrder FROM Customers WHERE CustomerEmail LIKE '%{email}%'";
            SqlCommand oCommand = new SqlCommand(CommandText, oConn);

            CustomerModel CurrentCustomer = new CustomerModel();

            bool UserExists = false;

            using (SqlDataReader oReader = oCommand.ExecuteReader())
            {
                if (oReader.HasRows)
                {
                    UserExists = true;

                    while (oReader.Read())
                    {
                        CurrentCustomer.CustomerID = oReader.GetGuid(0).ToString();
                        CurrentCustomer.CustomerCart = oReader.GetString(1);
                        CurrentCustomer.CustomerAddress = oReader.IsDBNull(2) ? string.Empty : oReader.GetString(2);
                        CurrentCustomer.CustomerOrders = oReader.IsDBNull(3) ? string.Empty : oReader.GetString(3);
                        CurrentCustomer.ActiveOrder = oReader.IsDBNull(4) ? 0 : oReader.GetInt32(4);
                    }
                }
            }

            if (UserExists == false)
            {
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrWhiteSpace(CurrentCustomer.CustomerCart))
            {
                TempData["response"] = "You have no items in your bag. You need to add items before you can review your cart.";

                CheckoutModel PM = new CheckoutModel();

                return RedirectToAction("ReviewCart", "Cart", PM);
            }

            else
            {
                List<Product> Products = new List<Product>();
                foreach (string s in CurrentCustomer.CustomerCart.Split(';'))
                {
                    Product newProduct = new Product();

                    List<string> innerData = new List<string>();
                    innerData = s.Replace('}', '\0').Replace('{', '\0').Trim().Split(',').ToList();

                    if (!string.IsNullOrWhiteSpace(innerData[0]))
                    {
                        newProduct.ProductID = int.Parse(innerData[0].Split('=')[1].Trim());
                        newProduct.Quantity = int.Parse(innerData[1].Split('=')[1 ].Trim());
                        newProduct.Naked = innerData[2].Split('=')[1].Trim().Contains("Naked") ? true : false;

                        if (innerData.Count > 3)
                        {
                            for (int i = 3; i < innerData.Count; i++)
                            {
                                newProduct._SelectedAttributes.Add(innerData[i]);
                            }
                        }

                        Products.Add(newProduct);
                    }
                }

                string ProductDataCommandText = "SELECT ProductID, ProductName, ProductDescription, ProductPrice, ProductStock, MktBuy FROM Products WHERE ProductID=@id";
                SqlCommand ProductDataCommand = new SqlCommand(ProductDataCommandText, oConn);

                List<Product> NonMarketProducts = new List<Product>();

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

                            if (oReader.GetBoolean(5) == false)
                            {
                                NonMarketProducts.Add(p);
                            }
                        }
                    }
                }

                if (NonMarketProducts.Count > 0)
                {
                    CheckoutModel PM = new CheckoutModel();

                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("<p>The following products are not applicable for market pickup:<br/>");
                    
                    foreach (Product p in NonMarketProducts)
                    {
                        sb.AppendLine(p.ProductName + "<br/>");
                    }

                    sb.AppendLine("<br/>If you have any questions, please visit our booth at the Laramar County Farmers' Market.</p>");

                    TempData["response"] = sb.ToString();

                    return RedirectToAction("ReviewCart", "Cart", PM);
                }

                foreach (Product p in Products)
                {
                    if (p.Quantity > p.ProductStock)
                    {
                        TempData["response"] = $"We currently only have {p.ProductStock} stock of {p.ProductName}. Please purchase less than that quantity.";

                        CheckoutModel PM = new CheckoutModel();

                        return RedirectToAction("ReviewCart", "Cart", PM);
                    }
                }

                Model._Products = Products;

                string GetActiveDiscountsCommandText = "SELECT * FROM Discounts WHERE AlwaysActive=1";
                SqlCommand GetActiveDiscountsCommand = new SqlCommand(GetActiveDiscountsCommandText, oConn);

                using (SqlDataReader oReader = GetActiveDiscountsCommand.ExecuteReader())
                {
                    while (oReader.HasRows && oReader.Read())
                    {
                        Model.DiscountApplied = true;
                        Model.DiscountName = oReader.GetString(1);
                        Model.DiscountPercentage = oReader.GetInt32(3);
                    }
                }

                int ReturnedOrderID = 0;

                if (!Model.DiscountApplied)
                {
                    string InsertOrderCommandText = "INSERT INTO Orders (CustomerID, ProductCount, Cart, OrderProcessed, OrderProcessor, OrderTotal) OUTPUT INSERTED.OrderID VALUES (@id, @count, @cart, @processed, @processor, @total);";
                    SqlCommand InsertOrderCommand = new SqlCommand(InsertOrderCommandText, oConn);
                    InsertOrderCommand.Parameters.AddWithValue("@id", CurrentCustomer.CustomerID);
                    InsertOrderCommand.Parameters.AddWithValue("@count", Products.Count);
                    InsertOrderCommand.Parameters.AddWithValue("@cart", CurrentCustomer.CustomerCart);
                    InsertOrderCommand.Parameters.AddWithValue("@processed", 0);
                    InsertOrderCommand.Parameters.AddWithValue("@processor", "Square");
                    InsertOrderCommand.Parameters.AddWithValue("@total", Model.CartTotal);

                    ReturnedOrderID = int.Parse(InsertOrderCommand.ExecuteScalar().ToString());
                }
                else
                {
                    string InsertOrderCommandText = "INSERT INTO Orders (CustomerID, ProductCount, Cart, OrderProcessed, OrderProcessor, OrderTotal, OrderDiscount, OrderDiscountName) OUTPUT INSERTED.OrderID VALUES (@id, @count, @cart, @processed, @processor, @total, @discount, @discountname);";
                    SqlCommand InsertOrderCommand = new SqlCommand(InsertOrderCommandText, oConn);
                    InsertOrderCommand.Parameters.AddWithValue("@id", CurrentCustomer.CustomerID);
                    InsertOrderCommand.Parameters.AddWithValue("@count", Products.Count);
                    InsertOrderCommand.Parameters.AddWithValue("@cart", CurrentCustomer.CustomerCart);
                    InsertOrderCommand.Parameters.AddWithValue("@processed", 0);
                    InsertOrderCommand.Parameters.AddWithValue("@processor", "Square");
                    InsertOrderCommand.Parameters.AddWithValue("@total", Model.CartTotal);
                    InsertOrderCommand.Parameters.AddWithValue("@discount", Model.DiscountPercentage);
                    InsertOrderCommand.Parameters.AddWithValue("@discountname", Model.DiscountName);

                    ReturnedOrderID = int.Parse(InsertOrderCommand.ExecuteScalar().ToString());
                }

                string UpdateActiveOrderCommandText = $"UPDATE Customers SET ActiveOrder=@orderid WHERE CustomerEmail LIKE '%{email}%'";
                SqlCommand UpdateActiveOrderCommand = new SqlCommand(UpdateActiveOrderCommandText, oConn);
                UpdateActiveOrderCommand.Parameters.AddWithValue("@orderid", ReturnedOrderID);

                UpdateActiveOrderCommand.ExecuteNonQuery();

                Model.OrderID = ReturnedOrderID;

                oConn.Close();

                return View("MarketCheckout", Model);
            }
        }

        public ActionResult MarketCheckout()
        {
            try
            {
                if (TempData["response"] != null || !string.IsNullOrWhiteSpace(TempData["response"].ToString()))
                    ViewBag.Response = TempData["response"].ToString();
            }
            catch (NullReferenceException exc)
            {

            }

            SqlConnection oConn = new SqlConnection(AccessModel.SqlConnection);
            oConn.Open();

            var user = Membership.GetUser(System.Web.HttpContext.Current.User.Identity.Name);
            string email = string.Empty;

            if (user != null)
                email = user.Email;
            else
                return RedirectToAction("Login", "Account");

            string CommandText = $"SELECT CustomerID, CustomerCart, CustomerAddress, CustomerOrders, ActiveOrder FROM Customers WHERE CustomerEmail LIKE '%{email}%'";
            SqlCommand oCommand = new SqlCommand(CommandText, oConn);

            CustomerModel CurrentCustomer = new CustomerModel();

            bool UserExists = false;

            using (SqlDataReader oReader = oCommand.ExecuteReader())
            {
                if (oReader.HasRows)
                {
                    UserExists = true;

                    while (oReader.Read())
                    {
                        CurrentCustomer.CustomerID = oReader.GetGuid(0).ToString();
                        CurrentCustomer.CustomerCart = oReader.GetString(1);
                        CurrentCustomer.CustomerAddress = oReader.IsDBNull(2) ? string.Empty : oReader.GetString(2);
                        CurrentCustomer.CustomerOrders = oReader.IsDBNull(3) ? string.Empty : oReader.GetString(3);
                        CurrentCustomer.ActiveOrder = oReader.IsDBNull(4) ? 0 : oReader.GetInt32(4);
                    }
                }
            }

            List<Product> Products = new List<Product>();

            if (string.IsNullOrWhiteSpace(CurrentCustomer.CustomerCart))
            {
                TempData["response"] = "You have no items in your bag. You need to add items before you can review your cart.";

                ProductsModel PM = new ProductsModel();

                return RedirectToAction("ReviewCart", "Cart", PM);
            }

            else
            {
                foreach (string s in CurrentCustomer.CustomerCart.Split(';'))
                {
                    Product newProduct = new Product();

                    List<string> innerData = new List<string>();
                    innerData = s.Replace('}', '\0').Replace('{', '\0').Trim().Split(',').ToList();

                    if (!string.IsNullOrWhiteSpace(innerData[0]))
                    {
                        newProduct.ProductID = int.Parse(innerData[0].Split('=')[1].Trim());
                        newProduct.Quantity = int.Parse(innerData[1].Split('=')[1].Trim());
                        newProduct.Naked = innerData[2].Split('=')[1].Trim().Contains("Naked") ? true : false;

                        if (innerData.Count > 3)
                        {
                            for (int i = 3; i < innerData.Count; i++)
                            {
                                newProduct._SelectedAttributes.Add(innerData[i]);
                            }
                        }

                        Products.Add(newProduct);
                    }
                }
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

            foreach (Product p in Products)
            {
                if (p.Quantity > p.ProductStock)
                {
                    TempData["response"] = $"We currently only have {p.ProductStock} stock of {p.ProductName}. Please purchase less than that quantity.";

                    ProductsModel PM = new ProductsModel();

                    return RedirectToAction("ReviewCart", "Cart", PM);
                }
            }

            Model._Products = Products;
            Model.IsMarketPurchase = true;

            string OrderCommandText = "SELECT ShippingInfo FROM Orders WHERE OrderID=@orderid";
            SqlCommand OrderCommand = new SqlCommand(OrderCommandText, oConn);
            OrderCommand.Parameters.AddWithValue("@orderid", CurrentCustomer.ActiveOrder);

            using (SqlDataReader oReader = OrderCommand.ExecuteReader())
            {
                while (oReader.HasRows && oReader.Read())
                {
                    Model.ShippingInfo = oReader.IsDBNull(0) ? string.Empty : oReader.GetString(0);
                }
            }

            return View(Model);
        }


        public ActionResult CashCheckout(int OrderID)
        {
            string ShippingInfo = string.Empty;
            string Cart = string.Empty;
            List<Product> Products = new List<Product>();

            SqlConnection oConn = new SqlConnection(AccessModel.SqlConnection);
            oConn.Open();

            string CommandText = "SELECT Cart, ShippingInfo FROM Orders WHERE OrderID = @id";
            SqlCommand oCommand = new SqlCommand(CommandText, oConn);
            oCommand.Parameters.Add("@id", OrderID);

            using (SqlDataReader oReader = oCommand.ExecuteReader())
            {
                while (oReader.HasRows && oReader.Read())
                {
                    Cart = oReader.GetString(0);
                    ShippingInfo = oReader.GetString(1);
                }
            }

            oConn.Close();

            string firstname = ShippingInfo.Split('\n')[0].Split(' ')[0].Trim();
            string lastname = ShippingInfo.Split('\n')[0].Split(' ')[1].Trim();

            foreach (string s in Cart.Split(';'))
            {
                Product newProduct = new Product();

                List<string> innerData = new List<string>();
                innerData = s.Replace('}', '\0').Replace('{', '\0').Trim().Split(',').ToList();

                if (!string.IsNullOrWhiteSpace(innerData[0]))
                {
                    newProduct.ProductID = int.Parse(innerData[0].Split('=')[1].Trim());
                    newProduct.Quantity = int.Parse(innerData[1].Split('=')[1].Trim());
                    newProduct.Naked = innerData[2].Split('=')[1].Trim().Contains("Naked") ? true : false;

                    if (innerData.Count > 3)
                    {
                        for (int i = 3; i < innerData.Count; i++)
                        {
                            newProduct._SelectedAttributes.Add(innerData[i]);
                        }
                    }

                    Products.Add(newProduct);
                }
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("An order was placed for market pickup: " + Model.firstname + " " + Model.lastname);
            sb.AppendLine("Products ordered: ");
            foreach (Product p in Model.Products)
            {
                sb.AppendLine(p.Quantity + "x " + p.ProductName);
            }

            var link = $"https://sleepytimesoaps.com/Account/Dashboard";
            var apiKey = ConfigurationManager.AppSettings["SendGridAPIKey"];
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("admin@sleepytimesoaps.com", "SleepyTimeSoaps Order Management");
            var to = new EmailAddress("7195516779@tmomail.net");
            var plainTextContent = sb.ToString();
            var htmlContent = sb.ToString();
            var msg = MailHelper.CreateSingleEmail(from, to, "Order", plainTextContent, htmlContent);
            var response = Task.Run(async () => await client.SendEmailAsync(msg)).Result;

            return RedirectToAction("Confirmation", "Checkout");
        }

        public ActionResult UpdateMarketContact(CheckoutModel Model, int OrderID, string firstname, string lastname, string email)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(firstname + " " + lastname);
            sb.AppendLine("MARKET");
            sb.AppendLine("PICKUP");
            sb.AppendLine("NULL, NULL 00000");

            SqlConnection oConn = new SqlConnection(AccessModel.SqlConnection);
            oConn.Open();

            string CommandText = "UPDATE Orders SET ShippingInfo=@shipping WHERE OrderID=@orderid";
            SqlCommand oCommand = new SqlCommand(CommandText, oConn);
            oCommand.Parameters.AddWithValue("@shipping", sb.ToString());
            oCommand.Parameters.AddWithValue("@orderid", OrderID);

            oCommand.ExecuteNonQuery();

            oConn.Close();

            foreach (Product p in Model.Products)
            {
                Debug.WriteLine(p.ProductName);
            }

            Model.ShippingInfo = sb.ToString();

            TempData["response"] = "Market contact information successfully updated.";
            return RedirectToAction("MarketCheckout", Model);
        }
    }
}