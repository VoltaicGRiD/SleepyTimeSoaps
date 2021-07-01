using SendGrid;
using SendGrid.Helpers.Mail;
using SleepyTimeSoaps.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SleepyTimeSoaps.Controllers
{
    public class AdminController : Controller
    {
        // GET: Admin
        public ActionResult Dashboard()
        {
            AdminDashboardModel Model = GetDashboardData();

            return View(Model);
        }

        public AdminDashboardModel GetDashboardData()
        {
            AdminDashboardModel Model = new AdminDashboardModel();

            SqlConnection oConn = new SqlConnection(AccessModel.SqlConnection);

            oConn.Open();

            string CommandText = @"DECLARE @ProductCount INT
                                   DECLARE @UserCount INT
                                   DECLARE @CustomerCount INT
                                   DECLARE @OrderCount INT
                                   DECLARE @ProcessedOrders INT
                                    
                                   SELECT @ProductCount = COUNT(*) FROM Products;
                                   SELECT @UserCount = COUNT(*) FROM Users;
                                   SELECT @CustomerCount = COUNT(*) FROM Customers;
                                   SELECT @OrderCount = COUNT(*) FROM Orders;
                                   SELECT @ProcessedOrders = COUNT(*) FROM Orders WHERE OrderProcessed=1;

                                   SELECT @ProductCount, @UserCount, @CustomerCount, @OrderCount, @ProcessedOrders;";
            SqlCommand oCommand = new SqlCommand(CommandText, oConn);

            using (SqlDataReader oReader = oCommand.ExecuteReader())
            {
                while (oReader.HasRows && oReader.Read())
                {
                    Model.ProductCount = oReader.GetInt32(0);
                    Model.RegisteredUsers = oReader.GetInt32(1);
                    Model.RegisteredCustomers = oReader.GetInt32(2);
                    Model.OrderCount = oReader.GetInt32(3);
                    Model.CompletedOrders = oReader.GetInt32(4);
                }
            }

            string GetOrdersCommandText = "SELECT OrderID, CustomerID, Cart, ShippingInfo, OrderDiscount, OrderStatus FROM Orders WHERE OrderProcessed=1";
            SqlCommand GetOrdersCommand = new SqlCommand(GetOrdersCommandText, oConn);

            using (SqlDataReader oReader = GetOrdersCommand.ExecuteReader())
            {
                while (oReader.HasRows && oReader.Read())
                {
                    CheckoutModel newOrder = new CheckoutModel();

                    newOrder.OrderID = oReader.GetInt32(0);
                    newOrder.CustomerID = oReader.GetGuid(1).ToString();
                    newOrder.Cart = oReader.GetString(2);
                    newOrder.ShippingInfo = oReader.GetString(3);
                    newOrder.DiscountPercentage = oReader.IsDBNull(4) ? 0 : oReader.GetInt32(4);
                    newOrder.OrderStatus = (CheckoutModel._status)Enum.Parse(typeof(CheckoutModel._status), oReader.GetString(5));

                    Model._Orders.Add(newOrder);
                }
            }

            foreach (CheckoutModel order in Model.Orders)
            {
                string GetCustomerEmailCommandText = "SELECT CustomerEmail FROM Customers WHERE CustomerID=@id";
                SqlCommand GetCustomerEmailCommand = new SqlCommand(GetCustomerEmailCommandText, oConn);

                GetCustomerEmailCommand.Parameters.AddWithValue("@id", order.CustomerID);

                order.Customer = GetCustomerEmailCommand.ExecuteScalar().ToString();

                List<Product> Products = new List<Product>();
                foreach (string s in order.Cart.Split(';'))
                {
                    if (!string.IsNullOrWhiteSpace(s))
                    {
                        Product newProduct = new Product();

                        List<string> innerData = new List<string>();
                        innerData = s.Replace('}', '\0').Replace('{', '\0').Trim().Split(',').ToList();

                        newProduct.ProductID = int.Parse(innerData[0].Split('=')[1].Trim());
                        newProduct.Quantity = int.Parse(innerData[1].Split('=')[1].Trim());
                        newProduct.Naked = innerData[2].Split('=')[1].Trim().Contains("Naked") ? true : false;

                        if (innerData.Count > 3)
                        {
                            for (int i = 3; i < innerData.Count; i++)
                            {
                                newProduct.SelectedAttributes.Add(innerData[i]);
                            }
                        }

                        Products.Add(newProduct);
                    }
                }

                string ProductDataCommandText = "SELECT ProductID, ProductName, ProductPrice, ProductStock FROM Products WHERE ProductID=@id";
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
                            p.ProductPrice = float.Parse(oReader.GetValue(2).ToString());
                            p.ProductStock = oReader.GetInt32(3);
                        }
                    }
                }

                order._Products = Products;
            }

            oConn.Close();

            return Model;
        }

        public ActionResult AddProduct(string ProductName, float ProductPrice, int ProductStock, string ProductCategory, string ProductScent, string ProductTags, string ProductPrimaryImageURL, string ProductImages, string ProductType, string ProductDescription, string ProductIngredients)
        {
            AdminDashboardModel Model = GetDashboardData();

            SqlConnection oConn = new SqlConnection(AccessModel.SqlConnection);
            oConn.Open();

            string CommandText = "INSERT INTO Products (ProductName, ProductReleaseDate, ProductIsReleased, ProductDescription, ProductIngredients, ProductPrice, ProductStock, ProductCategory, ProductScentProfile, ProductTags, ProductPrimaryImageURL, ProductImages, ProductType) VALUES (@name, @releasedate, @released, @description, @ingredients, @price, @stock, @category, @scent, @tags, @primaryurl, @images, @type)";
            SqlCommand oCommand = new SqlCommand(CommandText, oConn);

            oCommand.Parameters.AddWithValue("@name", ProductName);
            oCommand.Parameters.AddWithValue("@price", ProductPrice);
            oCommand.Parameters.AddWithValue("@releaseDate", DateTime.Now.ToString());
            oCommand.Parameters.AddWithValue("@released", 1);
            oCommand.Parameters.AddWithValue("@description", ProductDescription);
            oCommand.Parameters.AddWithValue("@ingredients", ProductIngredients);
            oCommand.Parameters.AddWithValue("@stock", ProductStock);
            oCommand.Parameters.AddWithValue("@category", ProductCategory);
            oCommand.Parameters.AddWithValue("@scent", ProductScent);
            oCommand.Parameters.AddWithValue("@tags", ProductTags);
            oCommand.Parameters.AddWithValue("@primaryurl", ProductPrimaryImageURL);
            oCommand.Parameters.AddWithValue("@images", ProductImages);
            oCommand.Parameters.AddWithValue("@type", ProductType);

            oCommand.ExecuteNonQuery();

            oConn.Close();

            ViewBag.Response = "Product added successfully";
            return View("Dashboard", Model);
        }

        public ActionResult Orders()
        {
            AdminDashboardModel Model = GetDashboardData();

            var NewOrders = Model.Orders.Where(c => c.OrderStatus == CheckoutModel._status.Received);

            ViewBag.Title = $"({NewOrders.Count()}) Orders";
            if (Session["Response"] != null)
            {
                ViewBag.Response = Session["Response"].ToString();
            }
            if (Session["Tracking"] != null)
            {
                ViewBag.Tracking = Session["Tracking"].ToString();
            }

            Response.AddHeader("Refresh", "300");
            return View(Model);
        }

        public ActionResult Acknowledged(int id)
        {
            SqlConnection oConn = new SqlConnection(AccessModel.SqlConnection);
            oConn.Open();

            string UpdateCommandText = "UPDATE Orders SET OrderStatus='Acknowledged' WHERE OrderID=@id";
            SqlCommand UpdateCommand = new SqlCommand(UpdateCommandText, oConn);
            UpdateCommand.Parameters.AddWithValue("@id", id);

            UpdateCommand.ExecuteNonQuery();

            oConn.Close();

            Session["Response"] = "Successfully updated order status to 'Acknowledged'.";

            return RedirectToAction("Orders");
        }

        public ActionResult Refund(int id)
        {
            SqlConnection oConn = new SqlConnection(AccessModel.SqlConnection);
            oConn.Open();

            string UpdateCommandText = "UPDATE Orders SET OrderStatus='Refunded' WHERE OrderID=@id";
            SqlCommand UpdateCommand = new SqlCommand(UpdateCommandText, oConn);
            UpdateCommand.Parameters.AddWithValue("@id", id);

            UpdateCommand.ExecuteNonQuery();

            oConn.Close();

            Session["Response"] = "Successfully updated order status to 'Refunded'.";

            return RedirectToAction("Orders");
        }

        public ActionResult Cancel(int id)
        {
            SqlConnection oConn = new SqlConnection(AccessModel.SqlConnection);
            oConn.Open();

            string UpdateCommandText = "UPDATE Orders SET OrderStatus='Cancelled' WHERE OrderID=@id";
            SqlCommand UpdateCommand = new SqlCommand(UpdateCommandText, oConn);
            UpdateCommand.Parameters.AddWithValue("@id", id);

            UpdateCommand.ExecuteNonQuery();

            oConn.Close();

            Session["Response"] = "Successfully updated order status to 'Cancelled'.";

            return RedirectToAction("Orders");
        }

        public ActionResult Packed(int id)
        {
            SqlConnection oConn = new SqlConnection(AccessModel.SqlConnection);
            oConn.Open();

            string UpdateCommandText = "SELECT CustomerID, Cart FROM Orders WHERE OrderID=@id; UPDATE Orders SET OrderStatus='Packed' WHERE OrderID=@id";
            SqlCommand UpdateCommand = new SqlCommand(UpdateCommandText, oConn);
            UpdateCommand.Parameters.AddWithValue("@id", id);

            string Cart = string.Empty;
            string CustomerID = string.Empty;

            using (SqlDataReader oReader = UpdateCommand.ExecuteReader())
            {
                while (oReader.HasRows && oReader.Read())
                {
                    CustomerID = oReader.GetGuid(0).ToString();
                    Cart = oReader.GetString(1);
                }
            }

            string GetOrderEmailCommandText = "SELECT CustomerEmail FROM Customers WHERE CustomerID=@id";
            SqlCommand GetOrderEmailCommand = new SqlCommand(GetOrderEmailCommandText, oConn);
            GetOrderEmailCommand.Parameters.AddWithValue("@id", CustomerID);

            string CustomerEmail = GetOrderEmailCommand.ExecuteScalar().ToString();

            List<Product> Products = new List<Product>();
            foreach (string s in Cart.Split(';'))
            {
                if (!string.IsNullOrWhiteSpace(s))
                {
                    Product newProduct = new Product();

                    List<string> innerData = new List<string>();
                    innerData = s.Replace('}', '\0').Replace('{', '\0').Trim().Split(',').ToList();

                    newProduct.ProductID = int.Parse(innerData[0].Split('=')[1].Trim());
                    newProduct.Quantity = int.Parse(innerData[1].Split('=')[1].Trim());
                    newProduct.Naked = innerData[2].Split('=')[1].Trim().Contains("Naked") ? true : false;

                    if (innerData.Count > 3)
                    {
                        for (int i = 3; i < innerData.Count; i++)
                        {
                            newProduct.SelectedAttributes.Add(innerData[i]);
                        }
                    }

                    Products.Add(newProduct);
                }
            }

            string ProductDataCommandText = "SELECT ProductID, ProductName, ProductPrice, ProductStock FROM Products WHERE ProductID=@id";
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
                        p.ProductPrice = float.Parse(oReader.GetValue(2).ToString());
                        p.ProductStock = oReader.GetInt32(3);
                    }
                }
            }

            oConn.Close();

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<a href=https://sleepytimesoaps.com\"><img src=\"https://sleepytimesoapsdata.blob.core.windows.net/productimages/Company_logo3.png\" width=200px></a>");
            sb.AppendLine("<br/>");
            sb.AppendLine("<p style=\"font-size: 14pt\">Your recent order containing the following has been packed: <br/>");
            foreach (Product p in Products)
            {
                sb.AppendLine($"<span style=\"font-size: 12pt; font-weight: 500;\">{p.ProductName}</span><br/>");
                sb.AppendLine($"<small>Quantity: {p.Quantity}</small><br/>");
                if (p.Naked)
                    sb.AppendLine($"<small>Naked</small><br/>");
                else
                    sb.AppendLine($"<small>Wrapped</small><br/>");
                if (p.SelectedAttributes.Count > 0)
                {
                    foreach (string s in p.SelectedAttributes)
                    {
                        sb.AppendLine($"<small>{s}</small><br/>");
                    }
                }
            }
            sb.AppendLine("<br/>Your order will be shipped soon, and an email containing a tracking number will be provided.");
            sb.AppendLine("<br/>You can review your order at any time here: <a href=\"https://sleepytimesoaps.com/Account/Dashboard\">Account Dashboard</a></p>");

            var link = $"https://sleepytimesoaps.com/Account/Dashboard";
            var apiKey = ConfigurationManager.AppSettings["SendGridAPIKey"];
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("admin@sleepytimesoaps.com", "SleepyTimeSoaps Order Management");
            var subject = "Order status updated.";
            var to = new EmailAddress(CustomerEmail);
            var plainTextContent = "Your recent order has had it's status updated to 'Packed'. Your order will be shipped soon, and an email containing a tracking number will be provided. You can view your order at this address: " + link;
            var htmlContent = sb.ToString(); ;
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = Task.Run(async () => await client.SendEmailAsync(msg)).Result;

            Session["Response"] = "Successfully updated order status to 'Packed'.";

            return RedirectToAction("Orders");
        }

        public ActionResult Shipped(int id)
        {
            Session["Tracking"] = id;

            return RedirectToAction("Orders");
        }

        public ActionResult SubmitShipped(string order, string tracking)
        {
            SqlConnection oConn = new SqlConnection(AccessModel.SqlConnection);
            oConn.Open();

            string UpdateCommandText = "SELECT CustomerID, Cart FROM Orders WHERE OrderID=@id; UPDATE Orders SET OrderStatus='Shipped' WHERE OrderID=@id";
            SqlCommand UpdateCommand = new SqlCommand(UpdateCommandText, oConn);
            UpdateCommand.Parameters.AddWithValue("@id", order);

            string Cart = string.Empty;
            string CustomerID = string.Empty;

            using (SqlDataReader oReader = UpdateCommand.ExecuteReader())
            {
                while (oReader.HasRows && oReader.Read())
                {
                    CustomerID = oReader.GetGuid(0).ToString();
                    Cart = oReader.GetString(1);
                }
            }

            string GetOrderEmailCommandText = "SELECT CustomerEmail FROM Customers WHERE CustomerID=@id";
            SqlCommand GetOrderEmailCommand = new SqlCommand(GetOrderEmailCommandText, oConn);
            GetOrderEmailCommand.Parameters.AddWithValue("@id", CustomerID);

            string CustomerEmail = GetOrderEmailCommand.ExecuteScalar().ToString();

            List<Product> Products = new List<Product>();
            foreach (string s in Cart.Split(';'))
            {
                if (!string.IsNullOrWhiteSpace(s))
                {
                    Product newProduct = new Product();

                    List<string> innerData = new List<string>();
                    innerData = s.Replace('}', '\0').Replace('{', '\0').Trim().Split(',').ToList();

                    newProduct.ProductID = int.Parse(innerData[0].Split('=')[1].Trim());
                    newProduct.Quantity = int.Parse(innerData[1].Split('=')[1].Trim());
                    newProduct.Naked = innerData[2].Split('=')[1].Trim().Contains("Naked") ? true : false;

                    if (innerData.Count > 3)
                    {
                        for (int i = 3; i < innerData.Count; i++)
                        {
                            newProduct.SelectedAttributes.Add(innerData[i]);
                        }
                    }

                    Products.Add(newProduct);
                }
            }

            string ProductDataCommandText = "SELECT ProductID, ProductName, ProductPrice, ProductStock FROM Products WHERE ProductID=@id";
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
                        p.ProductPrice = float.Parse(oReader.GetValue(2).ToString());
                        p.ProductStock = oReader.GetInt32(3);
                    }
                }
            }

            oConn.Close();

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<a href=https://sleepytimesoaps.com\"><img src=\"https://sleepytimesoapsdata.blob.core.windows.net/productimages/Company_logo3.png\" width=200px></a>");
            sb.AppendLine("<br/>");
            sb.AppendLine("<p style=\"font-size: 14pt\">Your recent order containing the following has been shipped: <br/>");
            foreach (Product p in Products)
            {
                sb.AppendLine($"<span style=\"font-size: 12pt; font-weight: 500;\">{p.ProductName}</span><br/>");
                sb.AppendLine($"<small>Quantity: {p.Quantity}</small><br/>");
                if (p.Naked)
                    sb.AppendLine($"<small>Naked</small><br/>");
                else
                    sb.AppendLine($"<small>Wrapped</small><br/>");
                if (p.SelectedAttributes.Count > 0)
                {
                    foreach (string s in p.SelectedAttributes)
                    {
                        sb.AppendLine($"<small>{s}</small><br/>");
                    }
                }
            }
            sb.AppendLine($"<br/>You can view tracking information by searching this tracking number: {tracking}");
            sb.AppendLine("<br/>You can review your order at any time here: <a href=\"https://sleepytimesoaps.com/Account/Dashboard\">Account Dashboard</a></p>");

            var link = $"https://sleepytimesoaps.com/Account/Dashboard";
            var apiKey = ConfigurationManager.AppSettings["SendGridAPIKey"];
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("admin@sleepytimesoaps.com", "SleepyTimeSoaps Order Management");
            var subject = "Order status updated.";
            var to = new EmailAddress(CustomerEmail);
            var plainTextContent = "Your recent order has had it's status updated to 'Packed'. Your order will be shipped soon, and an email containing a tracking number will be provided. You can view your order at this address: " + link;
            var htmlContent = sb.ToString(); ;
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = Task.Run(async () => await client.SendEmailAsync(msg)).Result;

            oConn.Close();

            Session["Tracking"] = null;
            Session["Response"] = "Successfully updated order status to 'Shipped'.";

            return RedirectToAction("Orders");
        }

        public ActionResult Products()
        {
            ProductsModel oModel = new ProductsModel();
            List<Product> Products = new List<Product>();

            SqlConnection oConn = new SqlConnection(AccessModel.SqlConnection);
            oConn.Open();

            string CommandText = "SELECT ProductID,ProductName,ProductDescription,ProductIngredients,ProductType,ProductCategory,ProductTags,ProductPrice FROM Products";
            SqlCommand oCommand = new SqlCommand(CommandText, oConn);

            using (SqlDataReader oReader = oCommand.ExecuteReader())
            {
                while (oReader.Read() && oReader.HasRows)
                {
                    Product newProduct = new Product();

                    newProduct.ProductID = oReader.GetInt32(0);
                    newProduct.ProductName = oReader.GetString(1);
                    newProduct.ProductDescription = oReader.GetString(2);
                    newProduct.ProductIngredients = oReader.GetString(3);
                    newProduct.ProductType = oReader.GetString(4);
                    newProduct.ProductCategory = oReader.GetString(5);
                    newProduct._ProductTags = oReader.GetString(6).Split(',').ToList();
                    newProduct.ProductPrice = float.Parse(oReader.GetValue(7).ToString());

                    Products.Add(newProduct);
                }
            }

            oConn.Close();

            oModel._Products = Products;

            return View(oModel);
        }

        public ActionResult DownloadImages(string id)
        {
            string PrimaryImage = string.Empty;
            string ProductImages = string.Empty;
            List<string> Images = new List<string>();

            SqlConnection oConn = new SqlConnection(AccessModel.SqlConnection);
            oConn.Open();

            string CommandText = "SELECT ProductPrimaryImageUrl, ProductImages FROM Products WHERE ProductID=@id";
            SqlCommand oCommand = new SqlCommand(CommandText, oConn);
            oCommand.Parameters.AddWithValue("@id", id);

            using (SqlDataReader oReader = oCommand.ExecuteReader())
            {
                PrimaryImage = oReader.GetString(0);
                ProductImages = oReader.GetString(1);
            }

            Images = ProductImages.Split(',').ToList();

            oConn.Close();

            using (WebClient client = new WebClient())
            {
                //
            }

            return null;
        }

        public ActionResult AddBlogPost(FormCollection input)
        {
            string PostTitle = input["BlogTitle"];
            string PostText = input["BlogText"];
            string PostImage = input["BlogImageUrl"];
            string PostButton = input["BlogButtonText"];
            string PostButtonUrl = input["BlogButtonText"];

            try
            {
                SqlConnection oConn = new SqlConnection(AccessModel.SqlConnection);
                oConn.Open();

                string CommandText = "INSERT INTO BlogPosts (BlogTitle, BlogText, BlogImageUrl, BlogPosted, BlogButtonText, BlogButtonHref) VALUES (@title, @text, @image, @posted, @buttontext, @buttonhref)";
                SqlCommand oCommand = new SqlCommand(CommandText, oConn);

                oCommand.Parameters.AddWithValue("@title", PostTitle);
                oCommand.Parameters.AddWithValue("@text", PostText);
                oCommand.Parameters.AddWithValue("@image", PostImage);
                oCommand.Parameters.AddWithValue("@posted", DateTime.Today.ToString());
                oCommand.Parameters.AddWithValue("@buttontext", PostButton);
                oCommand.Parameters.AddWithValue("@buttonhref", PostButtonUrl);

                oCommand.ExecuteNonQuery();

                oConn.Close();

                ViewBag["Response"] = "Success, blog post added.";
                return View("Dashboard");
            }
            catch (Exception exc)
            {
                ViewBag["Response"] = "ERR: " + exc.Message;
                return View("Dashboard");
            }
        }
    }
}