# testazurefunctions
Samples built using Azure Functions for Columbus Code Camp

The samples here include three different types of azure functions.
1. Accept Order function uses a Http trigger to receive an Order and responds with Order Number and adds the received order to a queue.
2. ProcessOrderFunction uses a Queue Trigger and will be triggered when new order is added to queue 
   and performs order processing and adds the processed order to processed orders queue.
3. SendOrderConfirmation function uses a Timer Trigger and it executes every 5 minutes to poll the processed order queue and picks 5 orders
from the queue and sends an order confirmation email using SendGrid API.

Configuration for storage account and Send Grid API key has to be updated in local.settings.json file after downloading the solution.
They are currently not included in the solution.

Use the azure function request to post to the http function. You need to use an valid email address to receive the email from Azure Function using SendGrid.

Below is the content to go inside local.settings.json
{
    "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "",
    "AzureWebJobsDashboard": "",
    "SendGrid": ""
  }
}
