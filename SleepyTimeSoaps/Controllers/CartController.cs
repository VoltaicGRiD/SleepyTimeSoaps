﻿using SleepyTimeSoaps.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace SleepyTimeSoaps.Controllers
{
    public class CartController : Controller
    {
        // GET: Cart
        //int ProductID, int quantity = 1, string wrapped = "Wrapped", List<Attribute> attributes = null
        public ActionResult AddToCart(FormCollection formCollection)
        {
            List<string> _SelectedAttributes = new List<string>();

            foreach (string s in formCollection.Keys)
            {
                if (s.Contains("attribute"))
                {
                    _SelectedAttributes.Add(formCollection[s]);
                }
            }

            SqlConnection oConn = new SqlConnection(AccessModel.SqlConnection);
            oConn.Open();

            var user = Membership.GetUser(System.Web.HttpContext.Current.User.Identity.Name);
            string email = string.Empty;

            if (user != null)
            {
                email = user.Email;
            }

            if (!string.IsNullOrWhiteSpace(email))
            {
                string CommandText = $"SELECT CustomerCart FROM Customers WHERE CustomerEmail=@email";
                SqlCommand oCommand = new SqlCommand(CommandText, oConn);

                string CurrentCart = string.Empty;
                oCommand.Parameters.AddWithValue("@email", email);

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

                string QuantityString = formCollection["quantity"].ToString();
                string NewCart = string.Empty;


                if (CurrentCart == "Empty")
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append($"ProductID = {formCollection["ProductID"]},");
                    sb.Append($"Quantity = {QuantityString},");
                    sb.Append($"Wrapped = {formCollection["wrapped"]}");
                    if (_SelectedAttributes.Count > 0)
                    {
                        foreach (string s in _SelectedAttributes)
                        {
                            sb.Append("," + s);
                        }
                    }
                    NewCart = sb.ToString();
                }
                else
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(CurrentCart + ";");
                    sb.Append($"ProductID = {formCollection["ProductID"]},");
                    sb.Append($"Quantity = {QuantityString},");
                    sb.Append($"Wrapped = {formCollection["wrapped"]}");
                    if (_SelectedAttributes.Count > 0)
                    {
                        foreach (string s in _SelectedAttributes)
                        {
                            sb.Append("," + s);
                        }
                    }
                    NewCart = sb.ToString();
                }

                string NewCartCommandText = $"UPDATE Customers SET CustomerCart=@cart WHERE CustomerEmail=@email";
                SqlCommand NewCartCommand = new SqlCommand(NewCartCommandText, oConn);

                NewCartCommand.Parameters.AddWithValue("@cart", NewCart);
                NewCartCommand.Parameters.AddWithValue("@email", email);

                NewCartCommand.ExecuteNonQuery();

                oConn.Close();

                return RedirectToAction("ReviewCart", "Cart");
            }

            else
            {
                string CurrentCart = string.Empty;
                try { CurrentCart = Session["Cart"].ToString(); } catch (NullReferenceException nrexc) { }
                string QuantityString = string.Empty;
                try
                {
                    QuantityString = formCollection["quantity"].ToString();
                } catch (NullReferenceException NRE)
                {
                    QuantityString = "1";
                }
                string NewCart = string.Empty;

                if (string.IsNullOrWhiteSpace(CurrentCart))
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append($"ProductID = {formCollection["ProductID"]},");
                    sb.Append($"Quantity = {QuantityString},");
                    sb.Append($"Wrapped = Wrapped");
                    NewCart = sb.ToString();
                }
                else
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(CurrentCart + ";");
                    sb.Append($"ProductID = {formCollection["ProductID"]},");
                    sb.Append($"Quantity = {QuantityString},");
                    sb.Append($"Wrapped = Wrapped");
                    NewCart = sb.ToString();
                }

                Session.Remove("Cart");
                Session.Add("Cart", NewCart);
            }

            return RedirectToAction("ReviewCart", "Cart");
        }

        public ActionResult AddToCartSimple(string id)
        {
            SqlConnection oConn = new SqlConnection(AccessModel.SqlConnection);
            oConn.Open();

            var user = Membership.GetUser(System.Web.HttpContext.Current.User.Identity.Name);
            string email = string.Empty;

            if (user != null)
            {
                email = user.Email;
            }

            if (!string.IsNullOrWhiteSpace(email))
            {
                string CommandText = $"SELECT CustomerCart FROM Customers WHERE CustomerEmail=@email";
                SqlCommand oCommand = new SqlCommand(CommandText, oConn);

                string CurrentCart = string.Empty;
                oCommand.Parameters.AddWithValue("@email", email);

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
                    StringBuilder sb = new StringBuilder();
                    sb.Append($"ProductID = {id},");
                    sb.Append($"Quantity = {QuantityString},");
                    sb.Append($"Wrapped = Wrapped");
                    NewCart = sb.ToString();
                }
                else
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(CurrentCart + ";");
                    sb.Append($"ProductID = {id},");
                    sb.Append($"Quantity = {QuantityString},");
                    sb.Append($"Wrapped = Wrapped");
                    NewCart = sb.ToString();
                }

                string NewCartCommandText = $"UPDATE Customers SET CustomerCart=@cart WHERE CustomerEmail=@email";
                SqlCommand NewCartCommand = new SqlCommand(NewCartCommandText, oConn);

                NewCartCommand.Parameters.AddWithValue("@cart", NewCart);
                NewCartCommand.Parameters.AddWithValue("@email", email);

                NewCartCommand.ExecuteNonQuery();

                oConn.Close();

                return RedirectToAction("Index", "Products", new { id = "c=1" });
            }

            else
            {
                string CurrentCart = string.Empty;
                try { CurrentCart = Session["Cart"].ToString(); } catch (NullReferenceException nrexc) { }
                string QuantityString = "1";
                string NewCart = string.Empty;

                if (string.IsNullOrWhiteSpace(CurrentCart))
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append($"ProductID = {id},");
                    sb.Append($"Quantity = {QuantityString},");
                    sb.Append($"Wrapped = Wrapped");
                    NewCart = sb.ToString();
                }
                else
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(CurrentCart + ";");
                    sb.Append($"ProductID = {id},");
                    sb.Append($"Quantity = {QuantityString},");
                    sb.Append($"Wrapped = Wrapped");
                    NewCart = sb.ToString();
                }

                Session.Remove("Cart");
                Session.Add("Cart", NewCart);
            }

            return RedirectToAction("Index", "Products", new { id = "c=1" });
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
            try
            {
                if (ViewBag.Response == null)
                {
                    if (string.IsNullOrWhiteSpace(ViewBag.Response))
                    {
                        ViewBag.Response = TempData["response"];
                    }
                }
            }
            catch (Exception exc) { }
            { }

            SqlConnection oConn = new SqlConnection(AccessModel.SqlConnection);
            oConn.Open();

            var user = Membership.GetUser(System.Web.HttpContext.Current.User.Identity.Name);
            string email = string.Empty;

            if (user != null)
                email = user.Email;

            if (!string.IsNullOrWhiteSpace(email))
            {
                string CommandText = $"SELECT CustomerCart FROM Customers WHERE CustomerEmail=@email";
                SqlCommand oCommand = new SqlCommand(CommandText, oConn);

                string CurrentCart = string.Empty;
                oCommand.Parameters.AddWithValue("@email", email);

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
                    CurrentCart = Session["Cart"].ToString();
                }

                if (CurrentCart == "Empty")
                {
                    ViewBag.Response = "You have no items in your bag. You need to add items before you can review your cart.";

                    CheckoutModel PM = new CheckoutModel();

                    return View(PM);
                }

                else
                {
                    List<Product> Products = new List<Product>();
                    foreach (string s in CurrentCart.Split(';'))
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

                    CheckoutModel PM = new CheckoutModel();
                    PM._Products = Products;

                    return View(PM);
                }
            }

            else
            {
                string CurrentCart = string.Empty;
                try { CurrentCart = Session["Cart"].ToString(); } catch (NullReferenceException nrexc) { }

                List<Product> Products = new List<Product>();
                if (!string.IsNullOrWhiteSpace(CurrentCart))
                {
                    foreach (string s in CurrentCart.Split(';'))
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
                }
                else
                {
                    return RedirectToAction("Index", "Products", new { id = "c=0" });
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

                CheckoutModel PM = new CheckoutModel();
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

            if (!string.IsNullOrWhiteSpace(email))
            {
                string CommandText = $"SELECT CustomerCart FROM Customers WHERE CustomerEmail=@email";
                SqlCommand oCommand = new SqlCommand(CommandText, oConn);

                string CurrentCart = string.Empty;
                oCommand.Parameters.AddWithValue("@email", email);

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

                var ProductToRemove = Products.First(product => product.ProductID == id);
                Products.Remove(ProductToRemove);

                string NewCart = string.Empty;

                foreach (Product product in Products)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append($"ProductID = {product.ProductID},");
                    sb.Append($"Quantity = {product.Quantity},");
                    sb.Append($"Wrapped = {(product.Naked ? "Naked" : "Wrapped")}");
                    if (product._SelectedAttributes.Count > 0)
                    {
                        foreach (string s in product._SelectedAttributes)
                        {
                            sb.Append("," + s);
                        }
                    }
                    NewCart += sb.ToString() + ";";
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

                string NewCartCommandText = $"UPDATE Customers SET CustomerCart=@cart WHERE CustomerEmail=@email";
                SqlCommand NewCartCommand = new SqlCommand(NewCartCommandText, oConn);

                NewCartCommand.Parameters.AddWithValue("@cart", NewCart);
                NewCartCommand.Parameters.AddWithValue("@email", email);

                NewCartCommand.ExecuteNonQuery();

                oConn.Close();

                return RedirectToAction("ReviewCart", "Cart");
            }

            else
            {
                string CurrentCart = string.Empty;
                try { CurrentCart = Session["Cart"].ToString(); } catch (NullReferenceException nrexc) { }

                List<Product> Products = new List<Product>();
                foreach (string s in CurrentCart.Split(';'))
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

                var ProductToRemove = Products.First(product => product.ProductID == id);
                Products.Remove(ProductToRemove);

                string NewCart = string.Empty;

                foreach (Product product in Products)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append($"ProductID = {product.ProductID},");
                    sb.Append($"Quantity = {product.Quantity},");
                    sb.Append($"Wrapped = {(product.Naked ? "Naked" : "Wrapped")}");
                    if (product._SelectedAttributes.Count > 0)
                    {
                        foreach (string s in product._SelectedAttributes)
                        {
                            sb.Append("," + s);
                        }
                    }
                    NewCart += sb.ToString() + ";";
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

                string NewCartCommandText = $"UPDATE Customers SET CustomerCart=@cart WHERE CustomerEmail=@email";
                SqlCommand NewCartCommand = new SqlCommand(NewCartCommandText, oConn);

                NewCartCommand.Parameters.AddWithValue("@cart", NewCart);
                NewCartCommand.Parameters.AddWithValue("@email", email);

                NewCartCommand.ExecuteNonQuery();

                oConn.Close();

                return RedirectToAction("ReviewCart", "Cart");
            }
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

            string CommandText = $"SELECT CustomerCart FROM Customers WHERE CustomerEmail=@email";
            SqlCommand oCommand = new SqlCommand(CommandText, oConn);

            string CurrentCart = string.Empty;
            oCommand.Parameters.AddWithValue("@email", email);

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

                CheckoutModel PM = new CheckoutModel();

                return View("ReviewCart", PM);
            }

            else
            {
                List<Product> Products = new List<Product>();
                foreach (string s in CurrentCart.Split(';'))
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

        public ActionResult UpdateQuantity(int id, int quantity)
        {
            SqlConnection oConn = new SqlConnection(AccessModel.SqlConnection);
            oConn.Open();

            var user = Membership.GetUser(System.Web.HttpContext.Current.User.Identity.Name);
            string email = string.Empty;

            if (user != null)
                email = user.Email;
            else
                return RedirectToAction("Login", "Account");

            string CommandText = $"SELECT CustomerCart FROM Customers WHERE CustomerEmail=@email";
            SqlCommand oCommand = new SqlCommand(CommandText, oConn);

            string CurrentCart = string.Empty;
            oCommand.Parameters.AddWithValue("@email", email);

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

            var ProductToUpdate = Products.First(product => product.ProductID == id);
            ProductToUpdate.Quantity = quantity;

            string NewCart = string.Empty;

            foreach (Product product in Products)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append($"ProductID = {product.ProductID},");
                sb.Append($"Quantity = {product.Quantity},");
                sb.Append($"Wrapped = {(product.Naked ? "Naked" : "Wrapped")}");
                if (product._SelectedAttributes.Count > 0)
                {
                    foreach (string s in product._SelectedAttributes)
                    {
                        sb.Append("," + s);
                    }
                }
                NewCart += sb.ToString() + ";";
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

            string NewCartCommandText = $"UPDATE Customers SET CustomerCart=@cart WHERE CustomerEmail=@email";
            SqlCommand NewCartCommand = new SqlCommand(NewCartCommandText, oConn);

            NewCartCommand.Parameters.AddWithValue("@cart", NewCart);
            NewCartCommand.Parameters.AddWithValue("@email", email);

            NewCartCommand.ExecuteNonQuery();

            oConn.Close();

            return RedirectToAction("ReviewCart", "Cart");
        }
    }
}

