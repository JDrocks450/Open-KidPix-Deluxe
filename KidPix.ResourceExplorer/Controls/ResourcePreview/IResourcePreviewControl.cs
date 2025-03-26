using KidPix.API.Importer;

namespace KidPix.ResourceExplorer.Controls.ResourcePreview
{
    public interface IResourcePreviewControl : IDisposable
    {
        event EventHandler OnPushResourceInfoUpdate;

        /// <summary>
        /// Previews the given <paramref name="Resource"/> in the control
        /// </summary>
        /// <param name="Resource"></param>
        public void AttachResource(KidPixResource? Resource);
        /// <summary>
        /// Gets the information shown by the Resource Information panel
        /// </summary>
        /// <returns></returns>
        public object? GetResourceInformationContext();
    }
}
