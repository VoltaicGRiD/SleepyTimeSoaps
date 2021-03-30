using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;

namespace SleepyTimeSoaps.Models
{
    public class Product
    {
        public Product _product { get { return this; } }
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
        public bool ProductIsAllergyFriendly { get; set; } = false;
        public List<AttributeModel> _Attributes = new List<AttributeModel>();
        public List<AttributeModel> Attributes { get { return _Attributes; } }
        public bool Naked { get; set; } = false;
        public int Quantity { get; set; } = 1;

        public List<string> _SelectedAttributes = new List<string>();
        public List<string> SelectedAttributes { get { return _SelectedAttributes; } }

        public float FinalProductPrice 
        {
            get
            {
                float tempPrice = 0;

                if (SelectedAttributes.Any(att => att.Contains('$')))
                {
                    foreach (string attribute in SelectedAttributes)
                    {
                        if (attribute.Contains('$'))
                        {
                            tempPrice += float.Parse(attribute.Split('$')[1]);
                        }
                    }
                }

                float ProductPriceWithAttributes = ProductPrice + tempPrice;

                return ProductPriceWithAttributes * Quantity;
            }
        }

        public string ReviewXMLRaw { get; set; }
        public XmlDocument ReviewXML
        {
            get
            {
                if (ReviewXMLRaw != null && !string.IsNullOrWhiteSpace(ReviewXMLRaw))
                {
                    XmlDocument reviewDoc = new XmlDocument();
                    reviewDoc.LoadXml(ReviewXMLRaw);
                    return reviewDoc;
                }
                else
                {
                    return null;
                }
            }
        }
        public List<Review> Reviews
        {
            get
            {
                return GetProductReviews();
            }
        }

        public List<Review> GetProductReviews()
        {
            if (ReviewXML != null)
            {
                List<Review> ReviewList = new List<Review>();

                XmlNodeList reviewNodes = ReviewXML.SelectNodes("/Reviews/Review");

                foreach (XmlNode review in reviewNodes)
                {
                    if (review != null && review.Name == "Review")
                    {
                        Review newReview = new Review();
                        newReview.Name = review.ChildNodes[0].InnerText;
                        newReview.Rating = int.Parse(review.ChildNodes[1].InnerText);
                        newReview.Text = review.ChildNodes[2].InnerText;

                        ReviewList.Add(newReview);
                    }
                }

                return ReviewList;
            }
            else
            {
                return null;
            }
        }

        public int AverageRating
        {
            get
            {
                List<int> AllRatings = new List<int>();

                foreach (Review review in Reviews)
                {
                    AllRatings.Add(review.Rating);
                }

                double Average = AllRatings.Average();
                return int.Parse(Math.Round(Average, 0).ToString());
            }
        }
    }
}