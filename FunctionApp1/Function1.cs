using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace FunctionApp1
{
    public static class AcceptOrder
    {
        [FunctionName("AcceptOrder")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req,
            [Queue("outqueue")]ICollector<string> outputQueueItem,
            TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            // Get request body
            dynamic data= await req.Content.ReadAsAsync<object>();
                       
            Order orderDetails = JsonConvert.DeserializeObject<Order>(data.ToString());
            if(data == null || orderDetails.ItemsList == null)
            {
                return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass order items the request body");
            }

            orderDetails.OrderNumber = System.Guid.NewGuid().ToString();
            orderDetails.OrderStatus = "New";
            
            outputQueueItem.Add(JsonConvert.SerializeObject(orderDetails));

            return req.CreateResponse(HttpStatusCode.OK, "Order Received:-" + orderDetails.OrderNumber);
            
        }
    }
}
