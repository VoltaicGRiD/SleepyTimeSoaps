using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SleepyTimeSoaps.Models
{
    public class AccountDashboardModel
    {
        public List<CheckoutModel> _Orders = new List<CheckoutModel>();
        public List<CheckoutModel> Orders { get { return _Orders; } }
    }
}