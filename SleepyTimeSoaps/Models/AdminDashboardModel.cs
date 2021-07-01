using System.Collections.Generic;

namespace SleepyTimeSoaps.Models
{
    public class AdminDashboardModel
    {
        public int RegisteredUsers { get; set; }
        public int RegisteredCustomers { get; set; }
        public int ProductCount { get; set; }
        public int OrderCount { get; set; }
        public int CompletedOrders { get; set; }

        public List<CheckoutModel> _Orders = new List<CheckoutModel>();
        public List<CheckoutModel> Orders { get { return _Orders; } }
    }
}