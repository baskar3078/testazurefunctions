using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
namespace FunctionApp1
{
    public static class ProcessOrderFunction
    {
        [FunctionName("ProcessOrderFunction")]
        public static void Run([QueueTrigger("outqueue")]string myQueueItem,  [Queue("processqueue")] ICollector<string> ProcessedOrder, TraceWriter log)
        {
            log.Info($"C# Queue trigger function processed: {myQueueItem}");
            
            if(string.IsNullOrWhiteSpace(myQueueItem))
            {
                return;              
            }

            Order orderDetails = JsonConvert.DeserializeObject<Order>(myQueueItem);
            //Apply some business logic to check Stock.
            int productOnStock = 100;            
            decimal OrderTotal = 0;
            foreach(var order in orderDetails.ItemsList)
            {
                if (!string.IsNullOrWhiteSpace(order.Qty) && Convert.ToInt32(order.Qty) > 0)
                {
                    int orderQuantity = Convert.ToInt32(order.Qty);
                    if (orderQuantity > productOnStock)
                    {
                        order.LineStatus = "NotFulfilled";
                    }
                    else 
                    {
                        order.LineStatus = "Fulfilled";
                        OrderTotal = OrderTotal + ((orderQuantity) * (order.Price));
                    }
                }
                else
                {
                    order.LineStatus = "NotFulfilled";
                }
            }

            orderDetails.OrderTotal = OrderTotal;
            var checkstatus = orderDetails.ItemsList.Where(x => x.LineStatus.Equals("NotFulfilled"));
            if(checkstatus.Count() > 0)
            {
                orderDetails.OrderStatus = "NotFulfilled";
            }
            else
            {
                orderDetails.OrderStatus = "Completed";
            }
            ProcessedOrder.Add(JsonConvert.SerializeObject(orderDetails));
            log.Info($"C# Queue trigger function processed order: {orderDetails.OrderNumber}");
        }
    }
}
