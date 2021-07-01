namespace SleepyTimeSoaps.Models
{
    public class CustomerModel
    {
        public string CustomerID { get; set; }
        public string CustomerIP { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerAddress { get; set; }
        public string CustomerCart { get; set; }
        public string CustomerWishlist { get; set; }
        public string CustomerOrders { get; set; }
        public int ActiveOrder { get; set; }
    }
}