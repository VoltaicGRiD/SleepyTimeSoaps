using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SleepyTimeSoaps.Models
{
    public class CheckoutModel
    {
        public CheckoutModel _Model { get { return this; } }

        public List<Product> _Products = new List<Product>();
        public List<Product> Products { get { return _Products; } }

        //Put 'Free Colorado shipping' or '$30+ free shipping' or 'Shipping sub total' here
        public string ShippingNote { get; set; }

        public int OrderID { get; set; }

        // firstname lastname
        // address
        // address2
        // city, state zip
        public string ShippingInfo { get; set; }
        private List<string> GetShippingLines()
        {
            if (!string.IsNullOrWhiteSpace(ShippingInfo))
            {
                List<string> ShippingLines = new List<string>();
                
                foreach (string s in ShippingInfo.Split('\n'))
                {
                    ShippingLines.Add(s);
                }
                
                return ShippingLines;
            }
            
            else
            {
                return null;
            }
        }

        public string firstname { get { return string.IsNullOrWhiteSpace(ShippingInfo) ? "" : GetShippingLines()[0].Split(' ')[0].Trim(); } }
        public string lastname { get { return string.IsNullOrWhiteSpace(ShippingInfo) ? "" : GetShippingLines()[0].Split(' ')[1].Trim(); } }
        public string address { get { return string.IsNullOrWhiteSpace(ShippingInfo) ? "" : GetShippingLines()[1].Trim(); } }
        public string address2 { get { return string.IsNullOrWhiteSpace(ShippingInfo) ? "" : GetShippingLines()[2].Trim(); } }
        public string city { get { return string.IsNullOrWhiteSpace(ShippingInfo) ? "" : GetShippingLines()[3].Split(',')[0].Trim(); } }
        public string state { get { return string.IsNullOrWhiteSpace(ShippingInfo) ? "" : GetShippingLines()[3].Split(',')[1].Trim().Split(' ')[0].Trim(); } }
        public string zip { get { return string.IsNullOrWhiteSpace(ShippingInfo) ? "" : GetShippingLines()[3].Split(',')[1].Trim().Split(' ')[1].Trim(); } }


        public string cardname { get; set; }
        public string cardnumber { get; set; }
        public string expmonth { get; set; }
        public string expyear { get; set; }
        public string cvv { get; set; }

        public float ProductSubTotal()
        {
            float Total = 0;

            foreach (Product p in Products)
            {
                Total += p.ProductPrice;
            }

            return Total;
        }

        public float ShippingSubtotal()
        {
            if (state == "CO")
            {
                ShippingNote = "Free Colorado shipping";
                return 0;
            }
            else if (ProductSubTotal() >= 30f)
            {
                ShippingNote = "$30+ free shipping";
                return 0;
            }
            else
            {
                ShippingNote = "Flat-rate shipping";
                return 5 + (1 * _Products.Count);
            }
        }


        public float CartTotal { get { return ProductSubTotal() + ShippingSubtotal(); } }

    }
}