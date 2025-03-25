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
                    { typeof(GenericKidPixResource),new HexEditorResourcePreview() }
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

            Dispose();

            if (Resource is BMHResource bmh)
            {
                var frameSource = bmh.Table[0];
                var treeView = new ListBox()
                {
                    ItemsSource = frameSource.Select(x => $"Frame {x}")
                };
                Window hWnd = new()
                {
                    Content = treeView,
                    SizeToContent = SizeToContent.WidthAndHeight
                };
                BMHFrameInfo GetSelectedFrame() => frameSource[treeView.SelectedIndex];
                treeView.SelectionChanged += delegate
                {
                    if (treeView.SelectedIndex <= -1 || treeView.SelectedIndex >= frameSource.Count) return;
                    hWnd.Close();
                };                               
                hWnd.ShowDialog();
                Resource = bmh.ImportFrame(GetSelectedFrame());
            }

            ContentFrame.Content = _controls[Resource.GetType()];
            _controls[Resource.GetType()].AttachResource(Resource);

            ResourceContent = Resource;
            ResourceInformationBlock.Breakdown(Resource);
        }

        public void Dispose()
        {
            if (ContentFrame.Content != null && ContentFrame.Content is IResourcePreviewControl control)
                control.Dispose();
        }
    }
}
