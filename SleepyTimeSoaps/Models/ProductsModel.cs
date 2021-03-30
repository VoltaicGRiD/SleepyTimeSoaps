using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SleepyTimeSoaps.Models
{
    public class ProductsModel
    {
        public List<Product> _Products = new List<Product>();
        public List<Product> Products { get { return _Products; } }

        public enum Categories
        {
            soap = 1,
            waxmelts = 2,
            bathsalts = 3
        }
        public Categories SelectedCategory { get; set; }
    }
}