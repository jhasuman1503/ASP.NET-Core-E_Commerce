using System.ComponentModel.DataAnnotations;

namespace E_Commerce_Website.Models
{
    public class Order
    {
        [Key]
        public int order_id { get; set; }
        public int prod_id { get; set; }
        public int cust_id { get; set; }
        public int quantity { get; set; }
        public decimal total_price { get; set; }
        public DateTime order_date { get; set; }
        public string order_status { get; set; } 
    }
}
