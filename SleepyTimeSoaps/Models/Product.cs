using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SleepyTimeSoaps.Models
{
    public class Product
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public DateTime ProductReleaseDate { get; set; }
        public string ProductReleaseDateReadable { get { return ProductReleaseDate.ToString("d"); } }
        public bool isReleased { get; set; }
        public string ProductDescription { get; set; }
        public string ProductIngredients { get; set; }
        public string ProductType { get; set; }
        public string ProductCategory { get; set; }
        public List<string> _ProductTags { get; set; }
        public List<string> ProductTags { get { return _ProductTags; } }
        public string ProductPrimaryImageUrl { get; set; }
        public List<string> _ProductImages { get; set; }
        public List<string> ProductImages { get { return _ProductImages; } }
        public float ProductPrice { get; set; } = 0;
        public string ProductScentProfile { get; set; }
        public bool ProductHasAttributes { get; set; }
        public string ProductAttributeIDs { get; set; }
        public int ProductStock { get; set; }
        public bool ProductIsRecommended { get; set; } = false;
        public bool ProductIsClearance { get; set; } = false;
        public List<AttributeModel> _Attributes = new List<AttributeModel>();
        public List<AttributeModel> Attributes { get { return _Attributes; } }
        public bool Naked { get; set; } = false;
        public int Quantity { get; set; } = 1;
        
        public List<string> _SelectedAttributes = new List<string>();
        public List<string> SelectedAttributes { get { return _SelectedAttributes; } }
    }
}