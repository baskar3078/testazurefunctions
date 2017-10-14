using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text;
namespace FunctionApp1
{
    public static class SendOrderConfirmation
    {
        [FunctionName("SendOrderConfirmation")]
        public static void Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, TraceWriter log)
        {
            log.Info($"C# Timer trigger function executed at: {DateTime.Now}");
            string ConnectionString = System.Configuration.ConfigurationManager.AppSettings["AzureWebJobsStorage"].ToString();
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConnectionString);
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

            CloudQueue processedQueue = queueClient.GetQueueReference("processqueue");
            List<CloudQueueMessage> ordermessage = (List<CloudQueueMessage>)processedQueue.GetMessages(5);

            if(ordermessage !=null && ordermessage.Count >0)
            {
                foreach(var message in ordermessage)
                {
                    Order orderDetails = JsonConvert.DeserializeObject<Order>(message.AsString);
                    
                    StringBuilder htmlContent = new StringBuilder();
                    if (string.IsNullOrWhiteSpace(orderDetails.EmailAddress))
                    {
                        log.Info($"C# Queue Timer trigger function Email Address Not Found for Order: {orderDetails.OrderNumber}");
                        return;
                    }
                    if(orderDetails.OrderStatus.Equals("Completed"))
                    {

                        htmlContent.Append(string.Format("<p>Your Order with Order Number - {0} has been received and Shipped</p></br>", orderDetails.OrderNumber));
                     
                        htmlContent.Append("<table>");
                        htmlContent.Append("<tr>");
                        htmlContent.Append(string.Format("<th>{0}</th><th>{1}</th><th>{2}</th><th>{3}</th><th>{4}</th></tr>", "ItemId", "Name", "Qty", "Price", "Status"));
                        foreach (var itemdetails in orderDetails.ItemsList)
                        {
                            htmlContent.Append("<tr>");
                            htmlContent.Append(string.Format("<th>{0}</th><th>{1}</th><th>{2}</th><th>{3}</th><th>{4}</th></tr>", itemdetails.ItemId.ToString(), itemdetails.ItemName
                                , itemdetails.Qty, itemdetails.Price, itemdetails.LineStatus));
                        }
                        htmlContent.Append("</table>");
                        SendOrderConfirmationEmail(orderDetails.EmailAddress, htmlContent.ToString()).Wait();
                    }
                    else
                    {
                        string htmlMessage = string.Format("<p>Please contact customer care for your order number-{0}</p>", orderDetails.OrderNumber);
                        SendOrderConfirmationEmail(orderDetails.EmailAddress, htmlMessage).Wait();
                    }
                    
                    processedQueue.DeleteMessage(message);
                }
               
            }                      
        }

        static async Task SendOrderConfirmationEmail(string toEmailAddress, string htmlContent)
        {
            var apiKey = System.Configuration.ConfigurationManager.AppSettings["SendGrid"].ToString();
            var fromaddress = System.Configuration.ConfigurationManager.AppSettings["SendGridFromAddress"].ToString();
            var subject = System.Configuration.ConfigurationManager.AppSettings["SendGridSubject"].ToString();
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(fromaddress); //Update the from address before publishing to azure.
            var to = new EmailAddress(toEmailAddress);            
            var msg = MailHelper.CreateSingleEmail(from, to, subject, string.Empty, htmlContent);
            var response = await client.SendEmailAsync(msg);
        }
    }
}
