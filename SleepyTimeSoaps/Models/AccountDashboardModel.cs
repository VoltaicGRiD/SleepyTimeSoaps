using System.Collections.Generic;

namespace SleepyTimeSoaps.Models
{
    public class AccountDashboardModel
    {
        public List<CheckoutModel> _Orders = new List<CheckoutModel>();
        public List<CheckoutModel> Orders { get { return _Orders; } }
    }
}