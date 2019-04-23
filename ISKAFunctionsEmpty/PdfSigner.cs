using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using PdfSharpCore.Drawing;
using PdfSharpCore.Fonts;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;

namespace MaandstatenFunctions
{
    public class PdfSigner
    {
        private readonly ILogger _logger;

        public PdfSigner(ILogger logger)
        {
            this._logger = logger;
            this._logger.LogInformation("creating the pdfsigner");
            try
            {
                this._logger.LogInformation("trying to set the font resolver");
                GlobalFontSettings.FontResolver = new VerdanaFontResolver();
            }
            catch (Exception e)
            {
                this._logger.LogError(e.Message);
            }

        }

        public void SignDocument(PdfDocument document, Func<Stream> imageStreamGetter)
        {
            this._logger.LogInformation("getting first page from document");
            var firstPage = document.Pages[0];
            SignPage(firstPage, imageStreamGetter);
        }

        public void SignPage(PdfPage page, Func<Stream> imageStreamGetter)
        {
            this._logger.LogInformation("resolving font");
            var font = new XFont("Verdana", 10, XFontStyle.Bold);
            _logger.LogInformation("making string formatting");
            var format = new XStringFormat
            {
                Alignment = XStringAlignment.Near,
                LineAlignment = XLineAlignment.Near
            };
            this._logger.LogInformation("getting gfx from page");
            using (var gfx = XGraphics.FromPdfPage(page))
            {
                //var rect = new XRect(new XPoint(x1, y1), new XPoint(x2, y2));
                var rect = new XRect(new XPoint(70, 435), new XSize(10, 10));
                var formattedDate = DateTime.Now.ToString("yyyy/MM/dd");
                this._logger.LogInformation("formatted string is " + formattedDate);
                this._logger.LogInformation("drawing string on page");
                gfx.DrawString(formattedDate, font, XBrushes.Black, rect, format);
                this._logger.LogInformation("loading the image");
                //image should be of size (180,90) for correct fitting in box
                var image = XImage.FromStream(imageStreamGetter);
                var imagePoint = new XPoint(40, 340);
                this._logger.LogInformation("drawing the image on page");
                gfx.DrawImage(image, imagePoint);
                this._logger.LogInformation("Done drawing image on page");
            }
        }


        public void SignEmbedded()
        {
            var assembly = Assembly.GetExecutingAssembly();

            const string name = "MaandstatenFunctions.Maandstaat.pdf";
            using (var stream = assembly.GetManifestResourceStream(name))
            {
                var document = PdfReader.Open(stream);
                assembly.GetManifestResourceStream("MaandstatenFunctions.Signature.png");
                var client = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("maandstatenStorage")).CreateCloudBlobClient();
                var reference = client
                    .GetContainerReference("signatures")
                    .GetBlobReference("scaled/Ruben_Mertens_Signature.png");


                SignDocument(document, () => reference.OpenReadAsync().GetAwaiter().GetResult());
                document.Save("D:/test.pdf");
                this._logger.LogInformation("this is where we would normally save our document, but all is well if you reach this point");
            }
        }
    }
}