using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SleepyTimeSoaps.Models
{
    public class AdminDashboardModel
    {
        public int RegisteredUsers { get; set; }
        public int RegisteredCustomers { get; set; }
        public int ProductCount { get; set; }
        public int OrderCount { get; set; }
    }
}