using System;
using PdfSharpCore.Fonts;

namespace MaandstatenFunctions
{
    public class VerdanaFontResolver : IFontResolver
    {
        public string DefaultFontName => throw new NotImplementedException();

        public byte[] GetFont(string faceName)
        {
            if (faceName == "Verdana#")
            {
                var assembly = typeof(VerdanaFontResolver).Assembly;
                
                using (var stream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.Verdana.ttf"))
                {
                    var count = (int)stream.Length;
                    byte[] data = new byte[count];
                    stream.Read(data, 0, count);
                    return data;
                }
            }
            return new byte[0];
        }

        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            switch (familyName)
            {
                case "Verdana":
                    return new FontResolverInfo("Verdana#");
            }
            return null;
        }
    }
}