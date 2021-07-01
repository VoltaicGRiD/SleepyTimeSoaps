using System;

namespace SleepyTimeSoaps.Models
{
    public class Blog
    {
        public string Title { get; set; }
        public string Text { get; set; }
        public string ImageUrl { get; set; }
        public DateTime Posted { get; set; }
        public string PostedReadable { get { return Posted.ToString("MMMM dd, yyyy"); } }
        public string ButtonText { get; set; }
        public string ButtonHref { get; set; }
        public int RecommendedProductID { get; set; }
        public Product RecommendedProduct { get; set; }
    }
}