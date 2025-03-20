using KidPix.API.Importer;
using KidPix.API.Importer.tBMP;
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
    public partial class ResourcePreviewControl : ContentControl, INotifyPropertyChanged
    {
        private static Dictionary<Type, IResourcePreviewControl> _controls;

        private KidPixResource? _resourceSelected;

        public event PropertyChangedEventHandler? PropertyChanged;

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
                    { typeof(BMPResource),new RasterImageViewer() }
                };
            }
        }

        public void AttachResource(KidPixResource? Resource)
        {
            if (Resource == null) return;

            if (Resource is BMPResource bmpResource)
            {
                if (bmpResource?.BitmapImage == null) return;
                bmpResource.BitmapImage.Save("test.bmp");
            }

            ContentFrame.Content = _controls[Resource.GetType()];
            _controls[Resource.GetType()].AttachResource(Resource);

            ResourceContent = Resource;
            ResourceInformationBlock.Breakdown(Resource);
        }        
    }
}
