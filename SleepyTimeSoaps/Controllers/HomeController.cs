using SleepyTimeSoaps.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SleepyTimeSoaps.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            BlogsModel Model = new BlogsModel();

            SqlConnection oConn = new SqlConnection(AccessModel.SqlConnection);
            oConn.Open();

            string CommandText = "SELECT * FROM BlogPosts ORDER BY BlogPosted DESC";
            SqlCommand oCommand = new SqlCommand(CommandText, oConn);

            using (SqlDataReader oReader = oCommand.ExecuteReader())
            {
                while (oReader.HasRows && oReader.Read())
                {
                    Model.BlogPosts.Add(new Blog()
                    {
                        Title = oReader.GetString(1),
                        Text = oReader.GetString(2),
                        ImageUrl = oReader.GetString(3),
                        Posted = oReader.GetDateTime(4),
                        ButtonText = oReader.GetString(5),
                        ButtonHref = oReader.GetString(6),
                        RecommendedProductID = oReader.IsDBNull(7) ? 0 : oReader.GetInt32(7)
                    });
                }
            }

            foreach (Blog post in Model.BlogPosts)
            {
                if (post.RecommendedProductID != 0)
                {
                    string GetProductCommandText = "SELECT ProductName, ProductPrimaryImageURL FROM Products WHERE ProductID=@id";
                    SqlCommand GetProductCommand = new SqlCommand(GetProductCommandText, oConn);

                    GetProductCommand.Parameters.AddWithValue("@id", post.RecommendedProductID);

                    Product recProduct = new Product();

                    using (SqlDataReader oReader = GetProductCommand.ExecuteReader())
                    {
                        while (oReader.HasRows && oReader.Read())
                        {
                            recProduct.ProductName = oReader.GetString(0);
                            recProduct.ProductPrimaryImageUrl = oReader.GetString(1);
                        }
                    }

                    post.RecommendedProduct = recProduct;
                }
            }

            oConn.Close();

            return View(Model);
        }

        public ActionResult Categories()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}