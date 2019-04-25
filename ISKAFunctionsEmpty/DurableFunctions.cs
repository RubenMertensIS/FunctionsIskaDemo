using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PdfSharpCore.Pdf.IO;
using MaandstatenFunctions;
using SendGrid.Helpers.Mail;
using System.Net.Http;

namespace ISKAFunctionsEmpty
{
    //blob maandstaat
    //tekenen
    //doorsturen mail 
    [StorageAccount("storageaccount")]
    public static class DurableFunctions
    {
        [FunctionName(nameof(Http_StartSigning))]
        public static async Task<HttpResponseMessage> Http_StartSigning(
            [HttpTrigger] HttpRequestMessage request,
            [OrchestrationClient] DurableOrchestrationClient client,
            ILogger log
            )
        {
            var name = "Ruben_Mertens_April.pdf";
            var instanceId = await client.StartNewAsync(nameof(OrchestratorSigner), name);
            return client.CreateCheckStatusResponse(request, instanceId);
        }

        [FunctionName(nameof(OrchestratorSigner))]
        public static async Task OrchestratorSigner(
            [OrchestrationTrigger] DurableOrchestrationContext context,
            ILogger log
            )
        {
            var fileName = context.GetInput<string>();
            await context.CallActivityAsync(nameof(QueueTriggerSignAsync), fileName);
            context.SetCustomStatus("Dit is een custom statuus");
            await context.WaitForExternalEvent<bool>("goedkeuren");
            await context.CallActivityAsync(nameof(SendMail), fileName);
        }

        [FunctionName(nameof(QueueTriggerSignAsync))]
        public static async Task QueueTriggerSignAsync(
            [ActivityTrigger] string message,
            [Blob("maandstaten/unsigned/{message}", FileAccess.Read)] Stream unsignedStream,
            [Blob("maandstaten/signed/{message}", FileAccess.Write)] Stream signedStream,
            [Blob("signatures/Ruben_Mertens_Signature.png", FileAccess.Read)] Stream signatureStream,
            ILogger log
            )
        {
            await Task.Delay(5000);
            var pdf = PdfReader.Open(unsignedStream);
            new PdfSigner(log).SignDocument(pdf, () => signatureStream);
            pdf.Save(signedStream, closeStream: true);
        }

        [FunctionName(nameof(SendMail))]
        public static void SendMail(
            [ActivityTrigger] string name,
            [Blob("maandstaten/signed/{name}",FileAccess.Read)] Stream signedStream,
            [SendGrid(ApiKey = "sendgridapikey")] out SendGridMessage message,
            ILogger log
            )
        {
            message = new SendGridMessage();
            message.From = new EmailAddress("toon.jansens@infosupport.com");
            message.Subject = "You've got mail";
            message.PlainTextContent = "In Bijlage de maandstaat";
            message.AddTo("ruben.mertens@infosupport.com");
            using(var save =new MemoryStream())
            {
                signedStream.CopyTo(save);
                message.AddAttachment(name, Convert.ToBase64String(save.ToArray()));
            }
        }
    }
}
