using SleepyTimeSoaps.Models;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using System.Xml;

namespace SleepyTimeSoaps.Controllers
{
    public class ProductsController : Controller
    {
        // GET: Products
        public ActionResult Index(string id = "")
        {
            switch (id)
            {
                case "w=0":
                    Response.Write("<script> alert ('You have no items in your wishlist, please add some items before going to review it.') </script>");
                    break;
                case "c=0":
                    Response.Write("<script> alert ('You have no items in your bag, please add some items before going to review it.') </script>");
                    break;
                case "c=1":
                    ViewBag.Response = "Successfully added to your bag.";
                    break;
                default:
                    break;
            }

            string Tag = string.Empty;
            string SearchText = string.Empty;
            string Category = string.Empty;

            if (id.StartsWith("tag="))
            {
                Tag = id.Split('=')[1];
            }
            else if (id.StartsWith("search="))
            {
                SearchText = id.Split('=')[1];
            }
            else if (id.StartsWith("category="))
            {
                Category = id.Split('=')[1];
            }

            ProductsModel returnedModel = null;

            if (!string.IsNullOrWhiteSpace(Tag))
            {
                returnedModel = GatherProductsByTag(Tag);
                ViewBag.Message = $"Gathered using tag: <b><i>{Tag}</b></i>";
            }
            else if (!string.IsNullOrWhiteSpace(SearchText))
            {
                //returnedModel = Search(SearchText);
                ViewBag.Message = $"Products like <b><i>{SearchText}</i></b>";
            }
            else if (!string.IsNullOrWhiteSpace(Category))
            {
                ViewBag.Message = $"Products like <b><i>{SearchText}</i></b>";

                switch (Category)
                {
                    case "soap":
                        returnedModel = GatherProductsByCategory("Specialty Soap");
                        returnedModel.SelectedCategory = ProductsModel.Categories.soap;
                        break;
                    case "waxmelts":
                        returnedModel = GatherProductsByCategory("Wax Melts");
                        returnedModel.SelectedCategory = ProductsModel.Categories.waxmelts;
                        break;
                    case "bathsalts":
                        returnedModel = GatherProductsByCategory("Bath Salts");
                        returnedModel.SelectedCategory = ProductsModel.Categories.bathsalts;
                        break;
                    default:
                        break;
                }
            }
            else if (id == "Recommended")
            {
                returnedModel = GatherRecommendedProducts();
                ViewBag.Message = "These are our recommended products";
            }
            else
            {
                ViewBag.Message = "All available products";
                returnedModel = GatherAllProducts();
            }

            return View(returnedModel);
        }

        public ActionResult ShopForThem(string id = "")
        {
            switch (id)
            {
                case "w=0":
                    Response.Write("<script> alert ('You have no items in your wishlist, please add some items before going to review it.') </script>");
                    break;
                case "c=0":
                    Response.Write("<script> alert ('You have no items in your bag, please add some items before going to review it.') </script>");
                    break;
                case "c=1":
                    ViewBag.Response = "Successfully added to your bag.";
                    break;
                default:
                    break;
            }

            string Tag = string.Empty;
            string SearchText = string.Empty;
            string Category = string.Empty;

            if (id.StartsWith("tag="))
            {
                Tag = id.Split('=')[1];
            }
            else if (id.StartsWith("search="))
            {
                SearchText = id.Split('=')[1];
            }
            else if (id.StartsWith("category="))
            {
                Category = id.Split('=')[1];
            }

            ProductsModel returnedModel = null;

            if (!string.IsNullOrWhiteSpace(Tag))
            {
                returnedModel = GatherProductsByTag(Tag);
                ViewBag.Message = $"Gathered using tag: <b><i>{Tag}</b></i>";
            }
            else if (!string.IsNullOrWhiteSpace(SearchText))
            {
                //returnedModel = Search(SearchText);
                ViewBag.Message = $"Products like <b><i>{SearchText}</i></b>";
            }
            else if (!string.IsNullOrWhiteSpace(Category))
            {
                ViewBag.Message = $"Products like <b><i>{SearchText}</i></b>";

                switch (Category)
                {
                    case "soap":
                        returnedModel = GatherProductsByCategory("Specialty Soap");
                        returnedModel.SelectedCategory = ProductsModel.Categories.soap;
                        break;
                    case "waxmelts":
                        returnedModel = GatherProductsByCategory("Wax Melts");
                        returnedModel.SelectedCategory = ProductsModel.Categories.waxmelts;
                        break;
                    case "bathsalts":
                        returnedModel = GatherProductsByCategory("Bath Salts");
                        returnedModel.SelectedCategory = ProductsModel.Categories.bathsalts;
                        break;
                    default:
                        break;
                }
            }
            else if (id == "Recommended")
            {
                returnedModel = GatherRecommendedProducts();
                ViewBag.Message = "These are our recommended products";
            }
            else
            {
                ViewBag.Message = "All available products";
                returnedModel = GatherAllProducts();
            }

            return View("Index", returnedModel);
        }

        public ActionResult ShopForHer(string id = "")
        {
            switch (id)
            {
                case "w=0":
                    Response.Write("<script> alert ('You have no items in your wishlist, please add some items before going to review it.') </script>");
                    break;
                case "c=0":
                    Response.Write("<script> alert ('You have no items in your bag, please add some items before going to review it.') </script>");
                    break;
                case "c=1":
                    ViewBag.Response = "Successfully added to your bag.";
                    break;
                default:
                    break;
            }

            string Tag = string.Empty;
            string SearchText = string.Empty;
            string Category = string.Empty;

            if (id.StartsWith("tag="))
            {
                Tag = id.Split('=')[1];
            }
            else if (id.StartsWith("search="))
            {
                SearchText = id.Split('=')[1];
            }
            else if (id.StartsWith("category="))
            {
                Category = id.Split('=')[1];
            }

            ProductsModel returnedModel = null;

            if (!string.IsNullOrWhiteSpace(Tag))
            {
                returnedModel = GatherProductsByTag(Tag);
                ViewBag.Message = $"Gathered using tag: <b><i>{Tag}</b></i>";
            }
            else if (!string.IsNullOrWhiteSpace(SearchText))
            {
                //returnedModel = Search(SearchText);
                ViewBag.Message = $"Products like <b><i>{SearchText}</i></b>";
            }
            else if (!string.IsNullOrWhiteSpace(Category))
            {
                ViewBag.Message = $"Products like <b><i>{SearchText}</i></b>";

                switch (Category)
                {
                    case "soap":
                        returnedModel = GatherProductsByCategory("Specialty Soap");
                        returnedModel.SelectedCategory = ProductsModel.Categories.soap;
                        break;
                    case "waxmelts":
                        returnedModel = GatherProductsByCategory("Wax Melts");
                        returnedModel.SelectedCategory = ProductsModel.Categories.waxmelts;
                        break;
                    case "bathsalts":
                        returnedModel = GatherProductsByCategory("Bath Salts");
                        returnedModel.SelectedCategory = ProductsModel.Categories.bathsalts;
                        break;
                    default:
                        break;
                }
            }
            else if (id == "Recommended")
            {
                returnedModel = GatherRecommendedProducts();
                ViewBag.Message = "These are our recommended products";
            }
            else
            {
                ViewBag.Message = "All available products";
                returnedModel = GatherAllProductsForHer();
            }

            return View("Index", returnedModel);
        }

        public ActionResult ShopForHim(string id = "")
        {
            switch (id)
            {
                case "w=0":
                    Response.Write("<script> alert ('You have no items in your wishlist, please add some items before going to review it.') </script>");
                    break;
                case "c=0":
                    Response.Write("<script> alert ('You have no items in your bag, please add some items before going to review it.') </script>");
                    break;
                case "c=1":
                    ViewBag.Response = "Successfully added to your bag.";
                    break;
                default:
                    break;
            }

            string Tag = string.Empty;
            string SearchText = string.Empty;
            string Category = string.Empty;

            if (id.StartsWith("tag="))
            {
                Tag = id.Split('=')[1];
            }
            else if (id.StartsWith("search="))
            {
                SearchText = id.Split('=')[1];
            }
            else if (id.StartsWith("category="))
            {
                Category = id.Split('=')[1];
            }

            ProductsModel returnedModel = null;

            if (!string.IsNullOrWhiteSpace(Tag))
            {
                returnedModel = GatherProductsByTag(Tag);
                ViewBag.Message = $"Gathered using tag: <b><i>{Tag}</b></i>";
            }
            else if (!string.IsNullOrWhiteSpace(SearchText))
            {
                //returnedModel = Search(SearchText);
                ViewBag.Message = $"Products like <b><i>{SearchText}</i></b>";
            }
            else if (!string.IsNullOrWhiteSpace(Category))
            {
                ViewBag.Message = $"Products like <b><i>{SearchText}</i></b>";

                switch (Category)
                {
                    case "soap":
                        returnedModel = GatherProductsByCategory("Specialty Soap");
                        returnedModel.SelectedCategory = ProductsModel.Categories.soap;
                        break;
                    case "waxmelts":
                        returnedModel = GatherProductsByCategory("Wax Melts");
                        returnedModel.SelectedCategory = ProductsModel.Categories.waxmelts;
                        break;
                    case "bathsalts":
                        returnedModel = GatherProductsByCategory("Bath Salts");
                        returnedModel.SelectedCategory = ProductsModel.Categories.bathsalts;
                        break;
                    default:
                        break;
                }
            }
            else if (id == "Recommended")
            {
                returnedModel = GatherRecommendedProducts();
                ViewBag.Message = "These are our recommended products";
            }
            else
            {
                ViewBag.Message = "All available products";
                returnedModel = GatherAllProductsForHim();
            }

            return View("Index", returnedModel);
        }

        public ActionResult RenderProduct(int id)
        {
            Product newProduct = new Product();

            SqlConnection oConn = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
            oConn.Open();

            string CommandText = "SELECT ProductID, ProductName, ProductScentProfile, ProductPrice, ProductPrimaryImageUrl, ProductDescription, ProductIsRecommended, ProductIsClearance, ProductIsAllergyFriendly, ProductHasAttributes, ProductReviews, ProductIsBundle, ProductBundle FROM Products WHERE ProductIsReleased=1 AND ProductID=@id";

            SqlCommand oCommand = new SqlCommand(CommandText, oConn);
            oCommand.Parameters.AddWithValue("@id", id);

            using (SqlDataReader oReader = oCommand.ExecuteReader())
            {
                while (oReader.HasRows && oReader.Read())
                {
                    newProduct.ProductID = oReader.GetInt32(0);
                    newProduct.ProductName = oReader.GetString(1);
                    newProduct.ProductScentProfile = oReader.GetString(2);
                    newProduct.ProductPrice = float.Parse(oReader.GetValue(3).ToString());
                    newProduct.ProductPrimaryImageUrl = oReader.IsDBNull(4) ? "https://sleepytimesoapsdata.blob.core.windows.net/productimages/Company_logo2.png" : oReader.GetString(4);
                    newProduct.ProductDescription = oReader.GetString(5);
                    newProduct.ProductIsRecommended = oReader.GetBoolean(6);
                    newProduct.ProductIsClearance = oReader.GetBoolean(7);
                    newProduct.ProductIsAllergyFriendly = oReader.GetBoolean(8);
                    newProduct.ProductHasAttributes = oReader.GetBoolean(9);
                    newProduct.ReviewXMLRaw = oReader.IsDBNull(10) ? string.Empty : oReader.GetString(10);
                    newProduct.ProductIsBundle = oReader.GetBoolean(11);
                    newProduct.ProductBundle = oReader.IsDBNull(12) ? string.Empty : oReader.GetString(12);
                }
            }

            oConn.Close();

            return PartialView("ProductPartial", newProduct);
        }

        private ProductsModel GatherProductsByCategory(string Category)
        {
            ProductsModel Model = new ProductsModel();

            SqlConnection oConn = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
            oConn.Open();

#if DEBUG
            string CommandText = $"SELECT ProductID, ProductName, ProductScentProfile, ProductPrice, ProductPrimaryImageUrl, ProductDescription, ProductIsRecommended, ProductIsClearance, ProductIsAllergyFriendly, ProductHasAttributes, ProductReviews, ProductIsBundle, ProductBundle FROM Products WHERE ProductType=@category";
            SqlCommand oCommand = new SqlCommand(CommandText, oConn);
#endif
#if (!DEBUG)
            string CommandText = $"SELECT ProductID, ProductName, ProductScentProfile, ProductPrice, ProductPrimaryImageUrl, ProductDescription, ProductIsRecommended, ProductIsClearance, ProductIsAllergyFriendly, ProductHasAttributes, ProductReviews, ProductIsBundle, ProductBundle FROM Products WHERE ProductIsReleased=1 AND ProductType=@category";
            SqlCommand oCommand = new SqlCommand(CommandText, oConn);
#endif

            oCommand.Parameters.AddWithValue("@category", Category);

            using (SqlDataReader oReader = oCommand.ExecuteReader())
            {
                while (oReader.HasRows && oReader.Read())
                {
                    Product newProduct = new Product();
                    newProduct.ProductID = oReader.GetInt32(0);
                    newProduct.ProductName = oReader.GetString(1);
                    newProduct.ProductScentProfile = oReader.GetString(2);
                    newProduct.ProductPrice = float.Parse(oReader.GetValue(3).ToString());
                    newProduct.ProductPrimaryImageUrl = oReader.IsDBNull(4) ? "https://sleepytimesoapsdata.blob.core.windows.net/productimages/Company_logo2.png" : oReader.GetString(4);
                    newProduct.ProductDescription = oReader.GetString(5);
                    newProduct.ProductIsRecommended = oReader.GetBoolean(6);
                    newProduct.ProductIsClearance = oReader.GetBoolean(7);
                    newProduct.ProductIsAllergyFriendly = oReader.GetBoolean(8);
                    newProduct.ProductHasAttributes = oReader.GetBoolean(9);
                    newProduct.ReviewXMLRaw = oReader.IsDBNull(10) ? string.Empty : oReader.GetString(10);
                    newProduct.ProductIsBundle = oReader.GetBoolean(11);
                    newProduct.ProductBundle = oReader.IsDBNull(12) ? string.Empty : oReader.GetString(12);

                    Model.Products.Add(newProduct);
                }
            }

            oConn.Close();

            return Model;
        }

        private ProductsModel GatherRecommendedProducts()
        {
            ProductsModel Model = new ProductsModel();

            SqlConnection oConn = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
            oConn.Open();

#if DEBUG
            string CommandText = $"SELECT ProductID, ProductName, ProductScentProfile, ProductPrice, ProductPrimaryImageUrl, ProductDescription, ProductIsRecommended, ProductIsClearance, ProductIsAllergyFriendly, ProductHasAttributes, ProductReviews, ProductIsBundle, ProductBundle FROM Products WHERE ProductIsRecommended=1";
            SqlCommand oCommand = new SqlCommand(CommandText, oConn);
#endif
#if (!DEBUG)
            string CommandText = $"SELECT ProductID, ProductName, ProductScentProfile, ProductPrice, ProductPrimaryImageUrl, ProductDescription, ProductIsRecommended, ProductIsClearance, ProductIsAllergyFriendly, ProductHasAttributes, ProductReviews, ProductIsBundle, ProductBundle FROM Products WHERE ProductIsReleased=1 AND ProductIsRecommended=1";
            SqlCommand oCommand = new SqlCommand(CommandText, oConn);
#endif

            using (SqlDataReader oReader = oCommand.ExecuteReader())
            {
                while (oReader.HasRows && oReader.Read())
                {
                    Product newProduct = new Product();
                    newProduct.ProductID = oReader.GetInt32(0);
                    newProduct.ProductName = oReader.GetString(1);
                    newProduct.ProductScentProfile = oReader.GetString(2);
                    newProduct.ProductPrice = float.Parse(oReader.GetValue(3).ToString());
                    newProduct.ProductPrimaryImageUrl = oReader.IsDBNull(4) ? "https://sleepytimesoapsdata.blob.core.windows.net/productimages/Company_logo2.png" : oReader.GetString(4);
                    newProduct.ProductDescription = oReader.GetString(5);
                    newProduct.ProductIsRecommended = oReader.GetBoolean(6);
                    newProduct.ProductIsClearance = oReader.GetBoolean(7);
                    newProduct.ProductIsAllergyFriendly = oReader.GetBoolean(8);
                    newProduct.ProductHasAttributes = oReader.GetBoolean(9);
                    newProduct.ReviewXMLRaw = oReader.IsDBNull(10) ? string.Empty : oReader.GetString(10);
                    newProduct.ProductIsBundle = oReader.GetBoolean(11);
                    newProduct.ProductBundle = oReader.IsDBNull(12) ? string.Empty : oReader.GetString(12);

                    Model.Products.Add(newProduct);
                }
            }

            oConn.Close();

            return Model;
        }

        private ProductsModel GatherProductsByTag(string tag)
        {
            ProductsModel Model = new ProductsModel();

            SqlConnection oConn = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
            oConn.Open();

#if DEBUG
            string CommandText = $"SELECT ProductID, ProductName, ProductScentProfile, ProductPrice, ProductPrimaryImageUrl, ProductDescription, ProductIsRecommended, ProductIsClearance, ProductIsAllergyFriendly, ProductHasAttributes, ProductReviews, ProductIsBundle, ProductBundle FROM Products WHERE ProductTags LIKE '%{tag}%'";
            SqlCommand oCommand = new SqlCommand(CommandText, oConn);
#endif
#if (!DEBUG)
            string CommandText = $"SELECT ProductID, ProductName, ProductScentProfile, ProductPrice, ProductPrimaryImageUrl, ProductDescription, ProductIsRecommended, ProductIsClearance, ProductIsAllergyFriendly, ProductHasAttributes, ProductReviews, ProductIsBundle, ProductBundle FROM Products WHERE ProductIsReleased=1 AND ProductTags LIKE '%{tag}%'";
            SqlCommand oCommand = new SqlCommand(CommandText, oConn);
#endif

            using (SqlDataReader oReader = oCommand.ExecuteReader())
            {
                while (oReader.HasRows && oReader.Read())
                {
                    Product newProduct = new Product();
                    newProduct.ProductID = oReader.GetInt32(0);
                    newProduct.ProductName = oReader.GetString(1);
                    newProduct.ProductScentProfile = oReader.GetString(2);
                    newProduct.ProductPrice = float.Parse(oReader.GetValue(3).ToString());
                    newProduct.ProductPrimaryImageUrl = oReader.IsDBNull(4) ? "https://sleepytimesoapsdata.blob.core.windows.net/productimages/Company_logo2.png" : oReader.GetString(4);
                    newProduct.ProductDescription = oReader.GetString(5);
                    newProduct.ProductIsRecommended = oReader.GetBoolean(6);
                    newProduct.ProductIsClearance = oReader.GetBoolean(7);
                    newProduct.ProductIsAllergyFriendly = oReader.GetBoolean(8);
                    newProduct.ProductHasAttributes = oReader.GetBoolean(9);
                    newProduct.ReviewXMLRaw = oReader.IsDBNull(10) ? string.Empty : oReader.GetString(10);
                    newProduct.ProductIsBundle = oReader.GetBoolean(11);
                    newProduct.ProductBundle = oReader.IsDBNull(12) ? string.Empty : oReader.GetString(12);

                    Model.Products.Add(newProduct);
                }
            }

            oConn.Close();

            return Model;
        }

        public ActionResult Clearance()
        {
            ProductsModel Model = new ProductsModel();

            SqlConnection oConn = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
            oConn.Open();

#if DEBUG
            string CommandText = $"SELECT ProductID, ProductName, ProductScentProfile, ProductPrice, ProductPrimaryImageUrl, ProductDescription, ProductIsRecommended, ProductIsClearance, ProductIsAllergyFriendly, ProductHasAttributes, ProductReviews, ProductIsBundle, ProductBundle FROM Products WHERE ProductIsClearance=1";
            SqlCommand oCommand = new SqlCommand(CommandText, oConn);
#endif
#if (!DEBUG)
            string CommandText = $"SELECT ProductID, ProductName, ProductScentProfile, ProductPrice, ProductPrimaryImageUrl, ProductDescription, ProductIsRecommended, ProductIsClearance, ProductIsAllergyFriendly, ProductHasAttributes, ProductReviews, ProductIsBundle, ProductBundle FROM Products WHERE ProductIsReleased=1 AND ProductIsClearance=1";
            SqlCommand oCommand = new SqlCommand(CommandText, oConn);
#endif

            using (SqlDataReader oReader = oCommand.ExecuteReader())
            {
                while (oReader.HasRows && oReader.Read())
                {
                    Product newProduct = new Product();
                    newProduct.ProductID = oReader.GetInt32(0);
                    newProduct.ProductName = oReader.GetString(1);
                    newProduct.ProductScentProfile = oReader.GetString(2);
                    newProduct.ProductPrice = float.Parse(oReader.GetValue(3).ToString());
                    newProduct.ProductPrimaryImageUrl = oReader.IsDBNull(4) ? "https://sleepytimesoapsdata.blob.core.windows.net/productimages/Company_logo2.png" : oReader.GetString(4);
                    newProduct.ProductDescription = oReader.GetString(5);
                    newProduct.ProductIsRecommended = oReader.GetBoolean(6);
                    newProduct.ProductIsClearance = oReader.GetBoolean(7);
                    newProduct.ProductIsAllergyFriendly = oReader.GetBoolean(8);
                    newProduct.ProductHasAttributes = oReader.GetBoolean(9);
                    newProduct.ReviewXMLRaw = oReader.IsDBNull(10) ? string.Empty : oReader.GetString(10);
                    newProduct.ProductIsBundle = oReader.GetBoolean(11);
                    newProduct.ProductBundle = oReader.IsDBNull(12) ? string.Empty : oReader.GetString(12);

                    Model.Products.Add(newProduct);
                }
            }

            oConn.Close();

            ViewBag.Message = "Clearance";
            return View("Index", Model);
        }

        private ProductsModel GatherAllProducts()
        {
            ProductsModel Model = new ProductsModel();

            SqlConnection oConn = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
            oConn.Open();

#if DEBUG
            string CommandText = "SELECT ProductID, ProductName, ProductScentProfile, ProductPrice, ProductPrimaryImageUrl, ProductDescription, ProductIsRecommended, ProductIsClearance, ProductIsAllergyFriendly, ProductHasAttributes, ProductReviews, ProductIsBundle, ProductBundle FROM Products";
            SqlCommand oCommand = new SqlCommand(CommandText, oConn);
#endif
#if (!DEBUG)
            string CommandText = "SELECT ProductID, ProductName, ProductScentProfile, ProductPrice, ProductPrimaryImageUrl, ProductDescription, ProductIsRecommended, ProductIsClearance, ProductIsAllergyFriendly, ProductHasAttributes, ProductReviews, ProductIsBundle, ProductBundle FROM Products WHERE ProductIsReleased=1";
            SqlCommand oCommand = new SqlCommand(CommandText, oConn);
#endif

            using (SqlDataReader oReader = oCommand.ExecuteReader())
            {
                while (oReader.HasRows && oReader.Read())
                {
                    Product newProduct = new Product();
                    newProduct.ProductID = oReader.GetInt32(0);
                    newProduct.ProductName = oReader.GetString(1);
                    newProduct.ProductScentProfile = oReader.GetString(2);
                    newProduct.ProductPrice = float.Parse(oReader.GetValue(3).ToString());
                    newProduct.ProductPrimaryImageUrl = oReader.IsDBNull(4) ? "https://sleepytimesoapsdata.blob.core.windows.net/productimages/Company_logo2.png" : oReader.GetString(4);
                    newProduct.ProductDescription = oReader.GetString(5);
                    newProduct.ProductIsRecommended = oReader.GetBoolean(6);
                    newProduct.ProductIsClearance = oReader.GetBoolean(7);
                    newProduct.ProductIsAllergyFriendly = oReader.GetBoolean(8);
                    newProduct.ProductHasAttributes = oReader.GetBoolean(9);
                    newProduct.ReviewXMLRaw = oReader.IsDBNull(10) ? string.Empty : oReader.GetString(10);
                    newProduct.ProductIsBundle = oReader.GetBoolean(11);
                    newProduct.ProductBundle = oReader.IsDBNull(12) ? string.Empty : oReader.GetString(12);

                    Model.Products.Add(newProduct);
                }
            }

            oConn.Close();

            return Model;
        }

        private ProductsModel GatherAllProductsForHim()
        {
            ProductsModel Model = new ProductsModel();

            SqlConnection oConn = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
            oConn.Open();

#if DEBUG
            string CommandText = "SELECT ProductID, ProductName, ProductScentProfile, ProductPrice, ProductPrimaryImageUrl, ProductDescription, ProductIsRecommended, ProductIsClearance, ProductIsAllergyFriendly, ProductHasAttributes, ProductReviews, ProductIsBundle, ProductBundle FROM Products WHERE ProductGender=2 OR ProductGender=4";
            SqlCommand oCommand = new SqlCommand(CommandText, oConn);
#endif
#if (!DEBUG)
            string CommandText = "SELECT ProductID, ProductName, ProductScentProfile, ProductPrice, ProductPrimaryImageUrl, ProductDescription, ProductIsRecommended, ProductIsClearance, ProductIsAllergyFriendly, ProductHasAttributes, ProductReviews, ProductIsBundle, ProductBundle FROM Products WHERE ProductIsReleased=1 AND (ProductGender=2 OR ProductGender=4)";
            SqlCommand oCommand = new SqlCommand(CommandText, oConn);
#endif

            using (SqlDataReader oReader = oCommand.ExecuteReader())
            {
                while (oReader.HasRows && oReader.Read())
                {
                    Product newProduct = new Product();
                    newProduct.ProductID = oReader.GetInt32(0);
                    newProduct.ProductName = oReader.GetString(1);
                    newProduct.ProductScentProfile = oReader.GetString(2);
                    newProduct.ProductPrice = float.Parse(oReader.GetValue(3).ToString());
                    newProduct.ProductPrimaryImageUrl = oReader.IsDBNull(4) ? "https://sleepytimesoapsdata.blob.core.windows.net/productimages/Company_logo2.png" : oReader.GetString(4);
                    newProduct.ProductDescription = oReader.GetString(5);
                    newProduct.ProductIsRecommended = oReader.GetBoolean(6);
                    newProduct.ProductIsClearance = oReader.GetBoolean(7);
                    newProduct.ProductIsAllergyFriendly = oReader.GetBoolean(8);
                    newProduct.ProductHasAttributes = oReader.GetBoolean(9);
                    newProduct.ReviewXMLRaw = oReader.IsDBNull(10) ? string.Empty : oReader.GetString(10);
                    newProduct.ProductIsBundle = oReader.GetBoolean(11);
                    newProduct.ProductBundle = oReader.IsDBNull(12) ? string.Empty : oReader.GetString(12);

                    Model.Products.Add(newProduct);
                }
            }

            oConn.Close();

            return Model;
        }

        private ProductsModel GatherAllProductsForHer()
        {
            ProductsModel Model = new ProductsModel();

            SqlConnection oConn = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
            oConn.Open();

#if DEBUG
            string CommandText = "SELECT ProductID, ProductName, ProductScentProfile, ProductPrice, ProductPrimaryImageUrl, ProductDescription, ProductIsRecommended, ProductIsClearance, ProductIsAllergyFriendly, ProductHasAttributes, ProductReviews, ProductIsBundle, ProductBundle FROM Products WHERE ProductGender=1 OR ProductGender=4";
            SqlCommand oCommand = new SqlCommand(CommandText, oConn);
#endif
#if (!DEBUG)
            string CommandText = "SELECT ProductID, ProductName, ProductScentProfile, ProductPrice, ProductPrimaryImageUrl, ProductDescription, ProductIsRecommended, ProductIsClearance, ProductIsAllergyFriendly, ProductHasAttributes, ProductReviews, ProductIsBundle, ProductBundle FROM Products WHERE ProductIsReleased=1 AND (ProductGender=1 OR ProductGender=4)";
            SqlCommand oCommand = new SqlCommand(CommandText, oConn);
#endif

            using (SqlDataReader oReader = oCommand.ExecuteReader())
            {
                while (oReader.HasRows && oReader.Read())
                {
                    Product newProduct = new Product();
                    newProduct.ProductID = oReader.GetInt32(0);
                    newProduct.ProductName = oReader.GetString(1);
                    newProduct.ProductScentProfile = oReader.GetString(2);
                    newProduct.ProductPrice = float.Parse(oReader.GetValue(3).ToString());
                    newProduct.ProductPrimaryImageUrl = oReader.IsDBNull(4) ? "https://sleepytimesoapsdata.blob.core.windows.net/productimages/Company_logo2.png" : oReader.GetString(4);
                    newProduct.ProductDescription = oReader.GetString(5);
                    newProduct.ProductIsRecommended = oReader.GetBoolean(6);
                    newProduct.ProductIsClearance = oReader.GetBoolean(7);
                    newProduct.ProductIsAllergyFriendly = oReader.GetBoolean(8);
                    newProduct.ProductHasAttributes = oReader.GetBoolean(9);
                    newProduct.ReviewXMLRaw = oReader.IsDBNull(10) ? string.Empty : oReader.GetString(10);
                    newProduct.ProductIsBundle = oReader.GetBoolean(11);
                    newProduct.ProductBundle = oReader.IsDBNull(12) ? string.Empty : oReader.GetString(12);

                    Model.Products.Add(newProduct);
                }
            }

            oConn.Close();

            return Model;
        }

        public ActionResult Search(string SearchText)
        {
            ProductsModel Model = new ProductsModel();

            SqlConnection oConn = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
            oConn.Open();

#if DEBUG
            string CommandText = $"SELECT ProductID, ProductName, ProductScentProfile, ProductPrice, ProductPrimaryImageUrl, ProductDescription, ProductIsRecommended, ProductIsClearance, ProductIsAllergyFriendly, ProductHasAttributes, ProductReviews, ProductIsBundle, ProductBundle FROM Products WHERE ProductName LIKE '%{SearchText}%' OR ProductCategory LIKE '%{SearchText}%' OR ProductTags LIKE '%{SearchText}%' OR ProductScentProfile LIKE '%{SearchText}%'";
            SqlCommand oCommand = new SqlCommand(CommandText, oConn);
#endif
#if (!DEBUG)
            string CommandText = $"SELECT ProductID, ProductName, ProductScentProfile, ProductPrice, ProductPrimaryImageUrl, ProductDescription, ProductIsRecommended, ProductIsClearance, ProductIsAllergyFriendly, ProductHasAttributes, ProductReviews, ProductIsBundle, ProductBundle FROM Products WHERE ProductIsReleased=1 AND (ProductName LIKE '%{SearchText}%' OR ProductCategory LIKE '%{SearchText}%' OR ProductTags LIKE '%{SearchText}%' OR ProductScentProfile LIKE '%{SearchText}%')";
            SqlCommand oCommand = new SqlCommand(CommandText, oConn);
#endif

            using (SqlDataReader oReader = oCommand.ExecuteReader())
            {
                while (oReader.HasRows && oReader.Read())
                {
                    Product newProduct = new Product();
                    newProduct.ProductID = oReader.GetInt32(0);
                    newProduct.ProductName = oReader.GetString(1);
                    newProduct.ProductScentProfile = oReader.GetString(2);
                    newProduct.ProductPrice = float.Parse(oReader.GetValue(3).ToString());
                    newProduct.ProductPrimaryImageUrl = oReader.IsDBNull(4) ? "https://sleepytimesoapsdata.blob.core.windows.net/productimages/Company_logo2.png" : oReader.GetString(4);
                    newProduct.ProductDescription = oReader.GetString(5);
                    newProduct.ProductIsRecommended = oReader.GetBoolean(6);
                    newProduct.ProductIsClearance = oReader.GetBoolean(7);
                    newProduct.ProductIsAllergyFriendly = oReader.GetBoolean(8);
                    newProduct.ProductHasAttributes = oReader.GetBoolean(9);
                    newProduct.ReviewXMLRaw = oReader.IsDBNull(10) ? string.Empty : oReader.GetString(10);
                    newProduct.ProductIsBundle = oReader.GetBoolean(11);
                    newProduct.ProductBundle = oReader.IsDBNull(12) ? string.Empty : oReader.GetString(12);

                    Model.Products.Add(newProduct);
                }
            }

            oConn.Close();

            ViewBag.Message = $"Products like \"<b><i>{SearchText}</i></b>\"";
            return View("Index", Model);
        }

        public ActionResult ProductPage(int id, string response = "null")
        {
            if (response == "cs")
                Response.Write("<script> AddToCartSuccess(); </script>");
            else if (response == "ws")
                Response.Write("<script> alert ('Item added to your wishlist!') </script>");

            Product selectedProduct = new Product();

            SqlConnection oConn = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
            oConn.Open();

            string CommandText = "SELECT * FROM Products WHERE ProductID=@id";
            SqlCommand oCommand = new SqlCommand(CommandText, oConn);
            oCommand.Parameters.AddWithValue("@id", id);

            using (SqlDataReader oReader = oCommand.ExecuteReader())
            {
                while (oReader.HasRows && oReader.Read())
                {
                    selectedProduct.ProductID = oReader.GetInt32(0);
                    selectedProduct.ProductName = oReader.GetString(1);
                    selectedProduct.ProductReleaseDate = oReader.GetDateTime(2);
                    selectedProduct.isReleased = oReader.GetBoolean(3);
                    selectedProduct.ProductDescription = oReader.GetString(4);
                    selectedProduct.ProductIngredients = oReader.GetString(5);
                    selectedProduct.ProductType = oReader.GetString(6);
                    selectedProduct.ProductCategory = oReader.GetString(7);
                    selectedProduct._ProductTags = oReader.GetString(8).Split(',').ToList();
                    selectedProduct.ProductPrimaryImageUrl = oReader.IsDBNull(9) ? "" : oReader.GetString(9);
                    if (!oReader.IsDBNull(10))
                        selectedProduct._ProductImages = oReader.GetString(10).Split(',').ToList();
                    selectedProduct.ProductPrice = float.Parse(oReader.GetValue(11).ToString());
                    selectedProduct.ProductHasAttributes = oReader.GetBoolean(12);
                    selectedProduct.ProductAttributeIDs = oReader.IsDBNull(13) ? "" : oReader.GetString(13);
                    selectedProduct.ProductScentProfile = oReader.GetString(14);
                    selectedProduct.ProductStock = oReader.GetInt32(15);
                    selectedProduct.ReviewXMLRaw = oReader.IsDBNull(20) ? string.Empty : oReader.GetString(20);
                    selectedProduct.ProductIsBundle = oReader.GetBoolean(22);
                    selectedProduct.ProductBundle = oReader.IsDBNull(23) ? string.Empty : oReader.GetString(23);
                }
            }

            if (selectedProduct.ProductHasAttributes)
            {
                string AttributeCommandText = "SELECT AttributeName, AttributeData FROM Attributes WHERE AttributeID=@id";
                SqlCommand AttributeCommand = new SqlCommand(AttributeCommandText, oConn);

                List<AttributeModel> newAttributes = new List<AttributeModel>();

                foreach (string attid in selectedProduct.ProductAttributeIDs.Split(','))
                {
                    AttributeCommand.Parameters.Clear();
                    AttributeCommand.Parameters.AddWithValue("@id", attid);

                    using (SqlDataReader oReader = AttributeCommand.ExecuteReader())
                    {
                        while (oReader.HasRows && oReader.Read())
                        {
                            AttributeModel newAttribute = new AttributeModel();
                            XmlDocument newXmlDoc = new XmlDocument();
                            newAttribute.AttributeName = oReader.GetString(0);
                            newXmlDoc.LoadXml(oReader.GetString(1));
                            newAttribute.BaseData = newXmlDoc;

                            newAttributes.Add(newAttribute);
                        }
                    }
                }

                foreach (AttributeModel attribute in newAttributes)
                {
                    XmlNodeList attributeNodes = attribute.BaseData.SelectNodes("/attribute/data/attribute");

                    foreach (XmlNode attributeNode in attributeNodes)
                    {
                        attribute._Values.Add(attributeNode.ChildNodes[0].InnerText);
                        attribute._Descriptions.Add(attributeNode.ChildNodes[1].InnerText);
                        attribute._Prices.Add(int.Parse(attributeNode.ChildNodes[2].InnerText));
                    }

                    selectedProduct._Attributes.Add(attribute);
                }
            }

            oConn.Close();

            return View(selectedProduct);
        }


    }


}