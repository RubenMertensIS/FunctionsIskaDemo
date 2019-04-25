# AzureFunctionsIskaDemo

## getting started

Since it can contain senstive information you should not commit your local.settings.json file. However it should look something like this:

```json
{
   "IsEncrypted": false,
 "Values": {
   "AzureWebJobsStorage": "UseDevelopmentStorage=true",
   "FUNCTIONS_WORKER_RUNTIME": "dotnet",
   "storageaccount": "STORAGE KEY HERE",
   "sendgridapikey": "SENDGRID KEY HERE"
 }
}

```
