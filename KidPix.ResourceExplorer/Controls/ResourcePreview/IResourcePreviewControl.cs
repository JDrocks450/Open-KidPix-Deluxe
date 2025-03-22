using KidPix.API.Importer;

namespace KidPix.ResourceExplorer.Controls.ResourcePreview
{
    public interface IResourcePreviewControl : IDisposable
    {
        /// <summary>
        /// Previews the given <paramref name="Resource"/> in the control
        /// </summary>
        /// <param name="Resource"></param>
        public void AttachResource(KidPixResource? Resource);
    }
}
