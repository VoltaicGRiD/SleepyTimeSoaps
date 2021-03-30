using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SleepyTimeSoaps.Models
{
    public class DiscountModel
    {
        public int DiscountID { get; set; }
        public string DiscountName { get; set; }
        public string DiscountCode { get; set; }
        public int DiscountPercentage { get; set; }
    }
}