using Newtonsoft.Json;
using SendGrid;
using SendGrid.Helpers.Mail;
using SleepyTimeSoaps.CustomAuthentication;
using SleepyTimeSoaps.DataAccess;
using SleepyTimeSoaps.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace SleepyTimeSoaps.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        // GET: Account  
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Login(string ReturnUrl = "")
        {
            if (User.Identity.IsAuthenticated)
            {
                return LogOut();
            }
            ViewBag.ReturnUrl = ReturnUrl;
            return View();
        }

        [HttpPost]
        public ActionResult Login(LoginView loginView, string ReturnUrl = "")
        {
            if (ModelState.IsValid)
            {
                if (Membership.ValidateUser(loginView.Email, loginView.Password))
                {
                    var user = (CustomMembershipUser)Membership.GetUser(loginView.Email, false);
                    if (user != null)
                    {
                        CustomSerializeModel userModel = new CustomSerializeModel()
                        {
                            UserId = user.UserId,
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            RoleName = user.Roles.Select(r => r.RoleName).ToList()
                        };

                        string userData = JsonConvert.SerializeObject(userModel);
                        FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket
                            (
                            1, loginView.Email, DateTime.Now, DateTime.Now.AddMinutes(15), false, userData
                            );

                        string enTicket = FormsAuthentication.Encrypt(authTicket);
                        HttpCookie faCookie = new HttpCookie("stsauth", enTicket);
                        faCookie.Expires = DateTime.Now.Add(new TimeSpan(1, 1, 0, 0));
                        Response.Cookies.Add(faCookie);

                        UpdateCustomerCart(user.Email);
                    }

                    if (Url.IsLocalUrl(ReturnUrl))
                    {
                        return Redirect(ReturnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Products");
                    }
                }
            }

            ModelState.AddModelError("", "Something Wrong: Email or Password is invalid.");
            return View(loginView);
        }

        [HttpGet]
        public ActionResult Registration()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Registration(RegistrationView registrationView)
        {
            bool statusRegistration = false;
            string messageRegistration = string.Empty;

            if (ModelState.IsValid)
            {
                // Email Verification  
                string userName = Membership.GetUserNameByEmail(registrationView.Email);
                if (!string.IsNullOrEmpty(userName))
                {
                    ModelState.AddModelError("", "Sorry: Email already Exists");
                    return View(registrationView);
                }

                Guid ActivationCode = Guid.NewGuid();

                string password = registrationView.Password;
                string confirmPassword = registrationView.ConfirmPassword;

                if (password.Trim() != confirmPassword.Trim())
                {
                    ModelState.AddModelError("", "The passwords do not match.");
                    return View(registrationView);
                }

                //Save User Data   
                using (AuthenticationDB dbContext = new AuthenticationDB())
                {
                    var user = new User()
                    {
                        FirstName = registrationView.FirstName,
                        LastName = registrationView.LastName,
                        Email = registrationView.Email,
                        Password = registrationView.Password,
                        ActivationCode = ActivationCode,
                    };

                    dbContext.Users.Add(user);
                    dbContext.SaveChanges();
                }

                //Verification Email  
                var link = $"https://sleepytimesoaps.com/Account/AccountActivation/{ActivationCode}";

                var apiKey = ConfigurationManager.AppSettings["SendGridAPIKey"];
                var client = new SendGridClient(apiKey);
                var from = new EmailAddress("admin@sleepytimesoaps.com", "SleepyTimeSoaps Account Management");
                var subject = "Verify your account with SleepyTimeSoaps";
                var to = new EmailAddress(registrationView.Email);
                var plainTextContent = "Please go to the following URL in order to activate your account: " + link;
                var htmlContent = "<a href=\"https://sleepytimesoaps.com\"><img src=\"https://sleepytimesoapsdata.blob.core.windows.net/productimages/Company_logo3.png\" width=200px></a> <br/> Please click on the following link in order to activate your account" + "<br/><a href='" + link + "'>Activate Account</a>";
                var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
                var response = Task.Run(async () => await client.SendEmailAsync(msg)).Result;

                //var email = Task.Run(async () => await VerificationEmail(registrationView.Email, ActivationCode.ToString()));
                //email.Wait();
                messageRegistration = "Your account has been created successfully. Please check your email to activate your account.<br/>It may take up to an hour for the email to be delivered. If you don't see the email after that, please send an email to <a href=\"mailto:admin@sleepytimesoaps.com\">'admin@sleepytimesoaps.com</a>'.";
                statusRegistration = true;

                UpdateCustomerCart(registrationView.Email);
            }
            else
            {
                messageRegistration = "An error occurred, please send an email to 'admin@sleepytimesoaps.com' with details.";
            }
            ViewBag.Message = messageRegistration;
            ViewBag.Status = statusRegistration;

            return View(registrationView);
        }

        [HttpGet]
        public ActionResult AccountActivation(string id)
        {
            bool statusAccount = false;
            using (AuthenticationDB dbContext = new DataAccess.AuthenticationDB())
            {
                var userAccount = dbContext.Users.Where(u => u.ActivationCode.ToString().Equals(id)).FirstOrDefault();

                if (userAccount != null)
                {
                    userAccount.IsActive = true;
                    dbContext.SaveChanges();
                    statusAccount = true;
                }
                else
                {
                    ViewBag.Message = "Something Wrong !!";
                }

            }
            ViewBag.Status = statusAccount;
            return View();
        }

        public ActionResult LogOut()
        {
            HttpCookie cookie = new HttpCookie("stsauth", "");
            cookie.Expires = DateTime.Now.AddYears(-1);
            Response.Cookies.Add(cookie);

            Session.Abandon();
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Account", null);
        }


        public async Task VerificationEmail(string email, string activationCode)
        {
            var link = $"https://sleepytimesoaps.com/Account/AccountActivation/{activationCode}";

            var apiKey = ConfigurationManager.AppSettings["SendGridAPIKey"];
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("admin@sleepytimesoaps.com", "SleepyTimeSoaps Account Management");
            var subject = "Verify your account with SleepyTimeSoaps";
            var to = new EmailAddress(email);
            var plainTextContent = "Please go to the following URL in order to activate your account: " + link;
            var htmlContent = "<a href=\"https://sleepytimesoaps.com\"><img src=\"https://sleepytimesoapsdata.blob.core.windows.net/productimages/Company_logo3.png\" width=200px></a> <br/> Please click on the following link in order to activate your account" + "<br/><a href='" + link + "'>Activate Account</a>";
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);

            //var senderEmail = new MailAddress("admin@sleepytimesoaps.com", "SleepyTimeSoaps Admin");

            //var fromEmail = new MailAddress("postmaster@sleepytimesoaps.com", "SleepyTimeSoaps Account Manager");
            //var toEmail = new MailAddress(email);

            //string subject = "Verify your account with SleepyTimeSoaps";

            //string body = "<a href=\"https://sleepytimesoaps.com\"><img src=\"https://sleepytimesoapsdata.blob.core.windows.net/productimages/Company_logo3.png\" width=200px></a> <br/> Please click on the following link in order to activate your account" + "<br/><a href='" + link + "'>Activate Account</a>";

            //var smtp = new SmtpClient
            //{
            //    Host = "smtp.sendgrid.net",
            //    Port = 25,
            //    EnableSsl = true,
            //    DeliveryMethod = SmtpDeliveryMethod.Network,
            //    UseDefaultCredentials = false,
            //    Credentials = new NetworkCredential("apikey", fromEmailPassword)
            //};

            //using (MailMessage message = new MailMessage(fromEmail, toEmail)
            //{
            //    Sender = senderEmail,
            //    Subject = subject,
            //    Body = body,
            //    IsBodyHtml = true
            //})

            //smtp.Send(message);
        }

        [Authorize]
        public ActionResult Dashboard()
        {
            var user = Membership.GetUser(System.Web.HttpContext.Current.User.Identity.Name);
            string email = string.Empty;

            if (user != null)
                email = user.Email;

            SqlConnection oConn = new SqlConnection(AccessModel.SqlConnection);
            oConn.Open();

            string GetCustomerIDCommandText = $"SELECT CustomerID from Customers WHERE CustomerEmail LIKE '%{email}%'";
            SqlCommand GetCustomerIDCommand = new SqlCommand(GetCustomerIDCommandText, oConn);

            string CustomerID = GetCustomerIDCommand.ExecuteScalar().ToString();

            string GetCustomerOrdersCommandText = "SELECT OrderID, Cart, OrderNotes, OrderTotal, ShippingInfo, OrderDiscount, OrderDiscountName FROM Orders WHERE CustomerID=@id AND OrderProcessed=1";
            SqlCommand GetCustomerOrdersCommand = new SqlCommand(GetCustomerOrdersCommandText, oConn);
            GetCustomerOrdersCommand.Parameters.AddWithValue("@id", CustomerID);

            AccountDashboardModel Model = new AccountDashboardModel();

            using (SqlDataReader oReader = GetCustomerOrdersCommand.ExecuteReader())
            {
                while (oReader.HasRows && oReader.Read())
                {
                    CheckoutModel newOrder = new CheckoutModel();

                    newOrder.OrderID = oReader.GetInt32(0);
                    newOrder.Cart = oReader.GetString(1);
                    newOrder.OrderNotes = oReader.IsDBNull(2) ? "None" : oReader.GetString(2);
                    newOrder.OrderTotal = (float)oReader.GetDouble(3);
                    newOrder.ShippingInfo = oReader.GetString(4);
                    newOrder.DiscountPercentage = oReader.IsDBNull(2) ? 0 : oReader.GetInt32(5);
                    newOrder.DiscountName = oReader.IsDBNull(2) ? "None" : oReader.GetString(6);

                    Model._Orders.Add(newOrder);
                }
            }


            foreach (CheckoutModel Order in Model._Orders)
            {
                List<Product> Products = new List<Product>();
                foreach (string s in Order.Cart.Split(';'))
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

                Order._Products = Products;
            }

            oConn.Close();

            return View(Model);
        }

        protected void UpdateCustomerCart(string email)
        {
            string CurrentCart = string.Empty;
            try { CurrentCart = Session["Cart"].ToString(); } catch (NullReferenceException nrexc) { }

            if (!string.IsNullOrWhiteSpace(CurrentCart))
            {
                SqlConnection oConn = new SqlConnection(AccessModel.SqlConnection);
                oConn.Open();

                bool UserExists = false;

                string CheckCustomerExistsCommandText = "SELECT CustomerDBID FROM Customers WHERE CustomerEmail=@email";
                SqlCommand CheckCustomerExistsCommand = new SqlCommand(CheckCustomerExistsCommandText, oConn);

                CheckCustomerExistsCommand.Parameters.AddWithValue("@email", email);

                using (SqlDataReader oReader = CheckCustomerExistsCommand.ExecuteReader())
                {
                    if (oReader.HasRows)
                        UserExists = true;
                }

                if (UserExists == false)
                {
                    string CreateCustomerCommandText = "INSERT INTO Customers (CustomerID, CustomerName, CustomerEmail) VALUES (@id, @name, @email)";
                    SqlCommand CreateCustomerCommand = new SqlCommand(CreateCustomerCommandText, oConn);

                    CreateCustomerCommand.Parameters.AddWithValue("@id", Guid.NewGuid());
                    CreateCustomerCommand.Parameters.AddWithValue("@name", email);
                    CreateCustomerCommand.Parameters.AddWithValue("@email", email);

                    CreateCustomerCommand.ExecuteNonQuery();
                }

                string NewCartCommandText = $"UPDATE Customers SET CustomerCart=@cart WHERE CustomerEmail=@email";
                SqlCommand NewCartCommand = new SqlCommand(NewCartCommandText, oConn);

                NewCartCommand.Parameters.AddWithValue("@cart", CurrentCart);
                NewCartCommand.Parameters.AddWithValue("@email", email);

                NewCartCommand.ExecuteNonQuery();

                oConn.Close();
            }
        }
    }
}