using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
namespace FunctionApp1
{
    public class Order
    {
        
        public string OrderNumber { get; set; }
        
        public List<OrderItems> ItemsList { get; set; }
       
        public decimal OrderTotal { get; set; }
        
        public string OrderStatus { get; set; }

        public string EmailAddress { get; set; }
    }

    public class OrderItems
    {
        public Int32 ItemId { get; set; }
        public string ItemName { get; set; }
        public string Qty { get; set; }
        public decimal Price { get; set; }
        public string LineStatus { get; set; }
    }
}
