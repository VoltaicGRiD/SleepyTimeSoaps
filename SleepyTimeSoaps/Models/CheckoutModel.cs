using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public string Cart { get; set; }

        public string CustomerID { get; set; }

        public string Customer { get; set; }

        public enum _status
        {
            Refunded = -2,
            Cancelled = -1,
            Received = 0,
            Acknowledged = 1,
            Packed = 2,
            Shipped = 3
        }
        public _status OrderStatus { get; set; }

        public string OrderNotes { get; set; }

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

        public float ProductSubTotal()
        {
            float Total = 0;

            foreach (Product p in Products)
            {
                Total += p.FinalProductPrice;
            }

            if (DiscountApplied)
            {
                float Percentage = (float)DiscountPercentage / 100;
                Total -= (Total * Percentage);
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

        public float OrderTotal { get; set; }

        //public string ProcessedCart 
        //{
        //    get
        //    {
        //        if (!string.IsNullOrWhiteSpace(Cart))
        //        {
        //            foreach (string s in Cart.Split(';'))
        //            {

        //                Product newProduct = new Product();

        //                List<string> innerData = new List<string>();
        //                innerData = s.Replace('}', '\0').Replace('{', '\0').Trim().Split(',').ToList();


        //                newProduct.ProductID = int.Parse(innerData[0].Split('=')[1].Trim());
        //                newProduct.Quantity = int.Parse(innerData[1].Split('=')[1].Trim());
        //                newProduct.Naked = innerData[2].Split('=')[1].Trim().Contains("Naked") ? true : false;

        //                if (innerData.Count > 3)
        //                {
        //                    for (int i = 3; i < innerData.Count; i++)
        //                    {
        //                        newProduct.SelectedAttributes.Add(innerData[i]);
        //                    }
        //                }

        //                Products.Add(newProduct);

        //            }
        //        }
        //    }
        //}

        public bool DiscountApplied { get; set; }
        public string DiscountName { get; set; }
        public int DiscountPercentage { get; set; }
    }
}