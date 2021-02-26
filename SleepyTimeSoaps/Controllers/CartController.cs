using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using SleepyTimeSoaps.Models;
using SleepyTimeSoaps.CustomAuthentication;
using System.Security.Principal;

namespace SleepyTimeSoaps.Controllers
{
    public class CartController : Controller
    {
        // GET: Cart
        [Authorize]
        public ActionResult AddToCart(int ProductID, int quantity = 1, string wrapped = "Wrapped")
        {
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
                string CreateCustomerCommandText = "INSERT INTO Customers (CustomerID, CustomerName, CustomerEmail) VALUES (@id, @name, @email)";
                SqlCommand CreateCustomerCommand = new SqlCommand(CreateCustomerCommandText, oConn);

                CreateCustomerCommand.Parameters.AddWithValue("@id", Guid.NewGuid());
                CreateCustomerCommand.Parameters.AddWithValue("@name", user.UserName);
                CreateCustomerCommand.Parameters.AddWithValue("@email", email);

                CreateCustomerCommand.ExecuteNonQuery();

                CurrentCart = "Empty";
            }

            string QuantityString = quantity.ToString();
            string NewCart = string.Empty;


            if (CurrentCart == "Empty")
            {
                NewCart = string.Join(",", new { ProductID, QuantityString, wrapped });
            }
            else
            {
                NewCart = CurrentCart + ";" + string.Join(",", new { ProductID, QuantityString, wrapped });
            }

            string NewCartCommandText = $"UPDATE Customers SET CustomerCart=@cart WHERE CustomerEmail LIKE '%{email}%'";
            SqlCommand NewCartCommand = new SqlCommand(NewCartCommandText, oConn);

            NewCartCommand.Parameters.AddWithValue("@cart", NewCart);

            NewCartCommand.ExecuteNonQuery();

            oConn.Close();

            return RedirectToAction("ReviewCart", "Cart");
        }

        [Authorize]
        public ActionResult AddToCartSimple(string id)
        {
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
                string CreateCustomerCommandText = "INSERT INTO Customers (CustomerID, CustomerName, CustomerEmail) VALUES (@id, @name, @email)";
                SqlCommand CreateCustomerCommand = new SqlCommand(CreateCustomerCommandText, oConn);

                CreateCustomerCommand.Parameters.AddWithValue("@id", Guid.NewGuid());
                CreateCustomerCommand.Parameters.AddWithValue("@name", user.UserName);
                CreateCustomerCommand.Parameters.AddWithValue("@email", email);

                CreateCustomerCommand.ExecuteNonQuery();

                CurrentCart = "Empty";
            }

            string QuantityString = "1";
            string NewCart = string.Empty;


            if (CurrentCart == "Empty")
            {
                NewCart = string.Join(",", new { ProductID = id, QuantityString, Naked = "Wrapped" });
            }
            else
            {
                NewCart = CurrentCart + ";" + string.Join(",", new { ProductID = id, QuantityString, Naked = "Wrapped" });
            }

            string NewCartCommandText = $"UPDATE Customers SET CustomerCart=@cart WHERE CustomerEmail LIKE '%{email}%'";
            SqlCommand NewCartCommand = new SqlCommand(NewCartCommandText, oConn);

            NewCartCommand.Parameters.AddWithValue("@cart", NewCart);

            NewCartCommand.ExecuteNonQuery();

            oConn.Close();

            return RedirectToAction("ReviewCart", "Cart");
        }

        public ActionResult AddToWishlist(int id)
        {
            HttpCookie cartCookie = Request.Cookies["wishlistCookie"];
            if (cartCookie == null)
            {

                cartCookie = new HttpCookie("wishlistCookie");
                cartCookie.Value = id.ToString();
                cartCookie.Expires = DateTime.Now.AddDays(7);
                Response.Cookies.Add(cartCookie);

            }
            else
            {

                string cookieValue = cartCookie.Value;
                cookieValue += "," + id.ToString();
                cartCookie.Value = cookieValue;
                Response.Cookies.Add(cartCookie);

            }

            return RedirectToAction("ProductPage", "Products", new { id = id, response = "ws" });
        }

        public ActionResult ReviewWishlist()
        {
            HttpCookie cartCookie = Request.Cookies["wishlistCookie"];
            if (cartCookie == null)
            {


                return RedirectToAction("Index", "Products", new { id = "w=0" });

            }
            else
            {

                string ProductsInCart = cartCookie.Value;
                List<string> Products = new List<string>();
                Products = ProductsInCart.Split(',').ToList();

                ProductsModel PM = new ProductsModel();

                SqlConnection oConn = new SqlConnection(AccessModel.SqlConnection);
                oConn.Open();

                string CommandText = "SELECT ProductName, ProductPrice, ProductID FROM Products WHERE ProductID=@id";
                SqlCommand oCommand = new SqlCommand(CommandText, oConn);

                foreach (string s in Products)
                {
                    if (oCommand.Parameters.Count == 0)
                        oCommand.Parameters.AddWithValue("@id", s);
                    else
                    {
                        oCommand.Parameters.RemoveAt(0);
                        oCommand.Parameters.AddWithValue("@id", s);
                    }

                    using (SqlDataReader oReader = oCommand.ExecuteReader())
                    {
                        while (oReader.HasRows && oReader.Read())
                        {
                            PM._Products.Add(new Product()
                            {
                                ProductName = oReader.GetString(0),
                                ProductPrice = float.Parse(oReader.GetValue(1).ToString()),
                                ProductID = oReader.GetInt32(2)
                            });
                        }
                    }
                }

                return View(PM);

            }
        }

        public ActionResult ReviewCart()
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

                return View(PM);
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

                ProductsModel PM = new ProductsModel();
                PM._Products = Products;

                return View(PM);
            }
        }


        [Authorize]
        public ActionResult RemoveFromCart(int id)
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

            var ProductToRemove = Products.First(product => product.ProductID == id);
            Products.Remove(ProductToRemove);

            string NewCart = string.Empty;

            foreach (Product product in Products)
            {
                NewCart = NewCart + ";" + string.Join(",", new { product.ProductID, product.Quantity, Naked = product.Naked ? "Naked" : "Wrapped" });
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

            string NewCartCommandText = $"UPDATE Customers SET CustomerCart=@cart WHERE CustomerEmail LIKE '%{email}%'";
            SqlCommand NewCartCommand = new SqlCommand(NewCartCommandText, oConn);

            NewCartCommand.Parameters.AddWithValue("@cart", NewCart);

            NewCartCommand.ExecuteNonQuery();

            oConn.Close();

            return RedirectToAction("ReviewCart", "Cart");
        }

        [Authorize]
        public ActionResult Checkout()
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

                CheckoutModel Model = new CheckoutModel();
                Model._Products = Products;

                return View(Model);
            }
        }
    }
}

