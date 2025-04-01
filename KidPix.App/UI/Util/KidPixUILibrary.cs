using KidPix.API;
using KidPix.API.AppService.Render.CanvasBrushes;
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
    internal class KidPixLibraryDrawingDescriptor
    {
        public System.Windows.Media.Color? TransparentColor { get; set; }
        public System.Windows.Media.Color PrimaryColor { get; set; }
        public KidPix.API.Common.GraphicsExtensions.Opaqueness Opacity { get; set; }
    }

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

        public static async Task<ImageBrush?> PaintabletoBrush(IPaintable Paintable, System.Windows.Media.Color? TransparentColor = default)
        {
            using System.Drawing.Bitmap bmp = Paintable.Paint();
            if (TransparentColor != null && bmp.PixelFormat != System.Drawing.Imaging.PixelFormat.Format8bppIndexed)
                bmp.MakeTransparent(TransparentColor.Value.ToDrawingColor());
            var brush = new ImageBrush(bmp.Convert(TransparentColor != null));
            RenderOptions.SetBitmapScalingMode(brush, BitmapScalingMode.NearestNeighbor);
            return brush;
        }

        public static async Task<ImageBrush?> ResourceToBrush(MHWKIdentifierToken Token, int BMHFrame = -1, KidPixLibraryDrawingDescriptor? Descriptor = default)
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
            if (asset as IPaintable == null) return null;
            if (Descriptor.PrimaryColor != default)
                ((IPaintable)asset).SetPaletteToPrimaryColorPalette(Descriptor.PrimaryColor.ToDrawingColor(), Descriptor.Opacity); 
            return await PaintabletoBrush((IPaintable)asset, Descriptor.TransparentColor);
        }

        public enum KPUtilBrushes
        {
            Pencil,
            Chalk,
            Crayon,
            Highlighter,
            Marker,

        }

        /// <summary>
        /// Creates a pre-created <see cref="KidPixCanvasBrush"/> based on the given tool description
        /// </summary>
        /// <param name="CreateBrush"></param>
        /// <param name="PrimaryColor"></param>
        /// <param name="Radius"></param>
        /// <returns></returns>
        public static async Task<KidPixCanvasBrush?> CreateBrush(KPUtilBrushes CreateBrush, System.Drawing.Color PrimaryColor, double Radius)
        {
            async Task<KidPixImageCanvasBrush?> getImageBrush(ushort AssetID, CHUNK_TYPE AssetType = CHUNK_TYPE.tBMP)
            {
                IPaintable? brushRaster = await TryImportResourceLinked<BMPResource>(new MHWKIdentifierToken(AssetType, AssetID));
                if (brushRaster == null) return null;
                System.Drawing.Bitmap brushImage = brushRaster.Paint();
                return KidPixCanvasBrushes.ImagePaintBrush(CreateBrush.ToString(), brushImage, PrimaryColor, Radius);
            }

            switch (CreateBrush)
            {
                case KPUtilBrushes.Pencil: return KidPixCanvasBrushes.Pencil(PrimaryColor, Radius);
                case KPUtilBrushes.Chalk:
                    var brush = await getImageBrush(3510);
                    brush.Opacity = .05;
                    return brush;
                case KPUtilBrushes.Crayon: return await getImageBrush(3510);
                case KPUtilBrushes.Highlighter: return await getImageBrush(1012);
                case KPUtilBrushes.Marker: return await getImageBrush(1007);
            }
            return null;
        }
    }
}
