using KidPix.API;
using KidPix.API.Importer;
using KidPix.API.Importer.Graphics;
using KidPix.API.Importer.Mohawk;
using KidPix.API.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace KidPix.App.UI.Util
{
    internal static class KidPixUILibrary
    {
        public static Dictionary<string,MHWKFile> LinkedArchives { get; } = new();
        public static bool LinkResource(params string[] FilePaths)
        {
            bool success = true;
            foreach (var resourceName in FilePaths)
            {
                string path = @"C:\Program Files (x86)\The Learning Company\Kid Pix Deluxe 4\Data\" + resourceName;
                string name = System.IO.Path.GetFileName(path);
                if (LinkedArchives.ContainsKey(name)) continue;
                MHWKFile easelArchive = MHWKImporter.Import(path);
                LinkedArchives.Add(name, easelArchive);
                success = true;
            }
            return success;
        }

        public static async Task<T?> TryImportResourceLinked<T>(MHWKIdentifierToken Token) where T : KidPixResource
        {
            foreach (var archive in LinkedArchives.Values)
            {
                if (!archive.ContainsResourceEntry(Token)) continue;
                var asset = await archive.TryImportResourceAsync(Token) as T;
                return asset;
            }
            return default;
        }

        public static async Task<ImageBrush?> PaintabletoBrush(IPaintable Paintable)
        {
            using System.Drawing.Bitmap bmp = Paintable.Paint();
            bmp.MakeTransparent(System.Drawing.Color.Black);
            var brush = new ImageBrush(bmp.Convert(true));            
            RenderOptions.SetBitmapScalingMode(brush, BitmapScalingMode.NearestNeighbor);
            return brush;
        }

        public static async Task<ImageBrush?> ResourceToBrush(MHWKIdentifierToken Token, int BMHFrame = -1)
        {
            using KidPixResource? asset = await TryImportResourceLinked<KidPixResource>(Token);
            switch (Token.ChunkType)
            {
                case CHUNK_TYPE.tBMP:
                    {
                        break;// :)
                    }
                case CHUNK_TYPE.tBMH:
                    {
                        if (asset is not BMHResource BMH) throw new InvalidOperationException("BMHResource is not of correct type.");
                        BMH.SetCurrentResource(BMHFrame);                        
                    }
                    break;
            }
            return await PaintabletoBrush((IPaintable)asset);
        }
    }
}
