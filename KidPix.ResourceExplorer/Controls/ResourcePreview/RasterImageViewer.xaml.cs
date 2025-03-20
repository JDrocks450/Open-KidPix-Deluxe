using KidPix.API.Importer;
using KidPix.API.Importer.tBMP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KidPix.ResourceExplorer.Controls.ResourcePreview
{
    /// <summary>
    /// Interaction logic for RasterImageViewer.xaml
    /// </summary>
    public partial class RasterImageViewer : UserControl, IResourcePreviewControl
    {
        const int MAX_PALETTE_ENTRIES = 512;

        private BMPResource? _currentResource;

        public RasterImageViewer()
        {
            InitializeComponent();

            Unloaded += delegate
            {
                Dispose();
            };
            Reset();
        }

        ~RasterImageViewer()
        {
            Dispose();
        }

        private bool IsSafe() => _currentResource != null;

        public void Reset()
        {
            ResetPaletteUI();
        }

        public void AttachResource(KidPixResource? Resource)
        {
            if (Resource is not BMPResource bmpResource) return;
            _currentResource = bmpResource;

            OnDisplay();
        }

        private void OnDisplay()
        {
            if (!IsSafe()) return;

            PreviewImage.Source = _currentResource.BitmapImage.Convert(true);
            if (API.Importer.tBMP.Decompressor.BMPRLEBrush.DEBUG_LAST_CALL != null)            
                CodeRunLinePreviewBox.Text = API.Importer.tBMP.Decompressor.BMPRLEBrush.DEBUG_LAST_CALL.ToString();            
            MakePaletteUI(_currentResource?.BitmapImage?.Palette);
        }

        private record PaletteEntryDescription(int PaletteIndex);

        private void MakePaletteUI(System.Drawing.Imaging.ColorPalette? palette)
        {
            ResetPaletteUI();
            if (palette == default) return;

            for(int i = 0; i < Math.Min(palette.Entries.Length, MAX_PALETTE_ENTRIES); i++)
            {
                System.Drawing.Color paltEntry = palette.Entries[i];
                Border paletteControl = new Border()
                {
                    Background = new SolidColorBrush(Color.FromArgb(paltEntry.A, paltEntry.R, paltEntry.G, paltEntry.B)),
                    Height = 15,
                    BorderThickness = new Thickness(0,0,1,1),
                    BorderBrush = Brushes.Black,
                    Tag = new PaletteEntryDescription(i)
                };
                PaletteEntriesGrid.Children.Add(paletteControl);
            }
        }

        private void ResetPaletteUI()
        {
            //deregister all
            foreach(UIElement rectControl in PaletteEntriesGrid.Children)            
                rectControl.MouseLeftButtonUp -= OnPaletteSelection;
            PaletteEntriesGrid.Children.Clear();            
        }

        private void OnPaletteSelection(object sender, MouseButtonEventArgs e)
        {
            if (sender is not Border borderControl) return;
            if (borderControl.Tag is not PaletteEntryDescription desc) return;
            if (_currentResource?.BitmapImage?.Palette == null) return;

            var palt = _currentResource?.BitmapImage?.Palette;

            System.Drawing.Color paltEntry = palt.Entries[desc.PaletteIndex];
        }

        private void StepOverButton_Click(object sender, RoutedEventArgs e)
        {
            API.Importer.tBMP.Decompressor.BMPRLEBrush.DEBUG_MAX_COMMANDS++;
            (Application.Current.MainWindow as MainWindow)?.CurrentResExplorerPage?.ReloadResource();
        }

        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            API.Importer.tBMP.Decompressor.BMPRLEBrush.DEBUG_MAX_COMMANDS=int.MaxValue;
            (Application.Current.MainWindow as MainWindow)?.CurrentResExplorerPage?.ReloadResource();
        }

        public void Dispose()
        {
            _currentResource?.Dispose();
            _currentResource = null;
        }
    }
}
