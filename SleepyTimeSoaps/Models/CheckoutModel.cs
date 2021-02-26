using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SleepyTimeSoaps.Models
{
    public class CheckoutModel
    {
        public List<Product> _Products = new List<Product>();
        public List<Product> Products { get { return _Products; } }

        public float CartTotal()
        {
            float Total = 0;

            foreach (Product p in Products)
            {
                Total += p.ProductPrice;
            }

            return Total;
        }

        public string firstname { get; set; }
        public string email { get; set; }
        public string address { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string zip { get; set; }

        public string cardname { get; set; }
        public string cardnumber { get; set; }
        public string expmonth { get; set; }
        public string expyear { get; set; }
        public string cvv { get; set; }
    }
}