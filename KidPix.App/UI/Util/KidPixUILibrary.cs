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
        public static HashSet<MHWKFile> LinkedArchives { get; } = new();

        public static async Task<T?> TryImportResourceLinked<T>(MHWKIdentifierToken Token) where T : KidPixResource
        {
            foreach (var archive in LinkedArchives)
            {
                var asset = await archive.TryImportResourceAsync(Token) as T;
                if (asset == null) continue;
                return asset;
            }
            return default;
        }

        public static async Task<ImageBrush?> PaintabletoBrush(IPaintable Paintable)
        {
            using System.Drawing.Bitmap bmp = Paintable.Paint();
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
