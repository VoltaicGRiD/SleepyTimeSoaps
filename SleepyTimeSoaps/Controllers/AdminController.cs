using SleepyTimeSoaps.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
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
                                    
                                   SELECT @ProductCount = COUNT(*) FROM Products;
                                   SELECT @UserCount = COUNT(*) FROM Users;
                                   SELECT @CustomerCount = COUNT(*) FROM Customers;
                                   SELECT @OrderCount = COUNT(*) FROM Orders;

                                   SELECT @ProductCount, @UserCount, @CustomerCount, @OrderCount";
            SqlCommand oCommand = new SqlCommand(CommandText, oConn);

            using (SqlDataReader oReader = oCommand.ExecuteReader())
            {
                while (oReader.HasRows && oReader.Read())
                {
                    Model.ProductCount = oReader.GetInt32(0);
                    Model.RegisteredUsers = oReader.GetInt32(1);
                    Model.RegisteredCustomers = oReader.GetInt32(2);
                    Model.OrderCount = oReader.GetInt32(3);
                }
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
    }
}