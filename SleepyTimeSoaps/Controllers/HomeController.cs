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

            string CommandText = "SELECT * FROM BlogPosts";
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
                        ButtonHref = oReader.GetString(6)
                    });
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