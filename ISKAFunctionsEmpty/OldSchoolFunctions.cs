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
    public static class OldSchoolFunctions
    {
        [FunctionName(nameof(Http_StartSigning))]
        [return: Queue("maandstaten-ready", Connection = "storageaccount")]
        public static string Http_StartSigning(
            [HttpTrigger] HttpRequestMessage request,
            ILogger log
            )
        {
            var name = "Ruben_Mertens_April.pdf";
            return name;
        }

        [FunctionName(nameof(QueueTriggerSignAsync))]
        public static async Task QueueTriggerSignAsync(
            [QueueTrigger("maandstaten-ready")] string message,
            [Blob("maandstaten/unsigned/{queueTrigger}", FileAccess.Read)] Stream unsignedStream,
            [Blob("maandstaten/signed/{queueTrigger}", FileAccess.Write)] Stream signedStream,
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
            [BlobTrigger("maandstaten/signed/{name}")] Stream signedStream,
            string name,
            [SendGrid(ApiKey = "sendgridapikey")] out SendGridMessage message,
            ILogger log
            )
        {
            message = new SendGridMessage();
            message.From = new EmailAddress("someemail@company.com");
            message.Subject = "You've got mail";
            message.PlainTextContent = "In Bijlage de maandstaat";
            message.AddTo("someemail@company.com");
            using(var save =new MemoryStream())
            {
                signedStream.CopyTo(save);
                message.AddAttachment(name, Convert.ToBase64String(save.ToArray()));
            }
        }
    }
}
