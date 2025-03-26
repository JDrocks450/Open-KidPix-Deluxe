using KidPix.API.Importer;
using KidPix.API.Importer.Graphics;
using KidPix.API.Importer.tWAV;
using KidPix.ResourceExplorer.Controls.ResourcePreview;
using NAudio.Wave;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace KidPix.ResourceExplorer.Controls
{
    /// <summary>
    /// Interaction logic for ResourcePreviewControl.xaml
    /// </summary>
    public partial class ResourcePreviewControl : ContentControl, INotifyPropertyChanged, IResourcePreviewControl
    {
        private static Dictionary<Type, IResourcePreviewControl> _controls;

        private KidPixResource? _resourceSelected;

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler OnPushResourceInfoUpdate;

        public Object? ResourceContent
        {
            get => _resourceSelected;
            private set
            {
                _resourceSelected = value as KidPixResource;
                if (_resourceSelected != null)
                    PropertyChanged?.Invoke(this, new(nameof(ResourceContent)));
            }
        }

        public ResourcePreviewControl()
        {
            InitializeComponent();

            if (_controls == null)
            { // INIT CONTROLS
                _controls = new()
                {
                    { typeof(WAVResource),new AudioPlayer() },
                    { typeof(BMPResource),new RasterImageViewer() },
                    { typeof(BMHResource),new RasterImageViewer() },
                    { typeof(GenericKidPixResource),new HexEditorResourcePreview() }
                };
            }
        }

        public void AttachResource(KidPixResource? Resource)
        {
            if (Resource == null) return;

            Dispose();

            IResourcePreviewControl previewControl = _controls[Resource.GetType()];
            ContentFrame.Content = previewControl;
            previewControl.AttachResource(Resource);

            ResourceContent = Resource;            
            ResourceInformationBlock.Breakdown(previewControl.GetResourceInformationContext());
            previewControl.OnPushResourceInfoUpdate += PreviewControl_OnPushResourceInfoUpdate;
        }

        private void PreviewControl_OnPushResourceInfoUpdate(object? sender, EventArgs e)
        {
            if (ContentFrame.Content is not IResourcePreviewControl control) return;
            ResourceInformationBlock.Breakdown(control.GetResourceInformationContext());
        }

        public void Dispose()
        {
            if (ContentFrame.Content != null && ContentFrame.Content is IResourcePreviewControl control)
            {
                control.OnPushResourceInfoUpdate -= PreviewControl_OnPushResourceInfoUpdate;
                control.Dispose();
            }
        }

        public object? GetResourceInformationContext() => null;
    }
}
