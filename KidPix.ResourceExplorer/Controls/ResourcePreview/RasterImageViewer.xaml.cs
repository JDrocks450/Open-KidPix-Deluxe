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
        private HexEditorResourcePreview _debugger => _debuggerWindow.Content as HexEditorResourcePreview;
        private Window _debuggerWindow;

        public RasterImageViewer()
        {
            InitializeComponent();

            Unloaded += delegate
            {
                Dispose();
            };
            Reset();
            _debuggerWindow = new()
            {
                Width = 800,
                Height = 600,
                Title = "Debug Window",
                Content = new HexEditorResourcePreview()
            };
        }

        ~RasterImageViewer()
        {
            Dispose();
        }

        private bool IsSafe() => _currentResource != null;

        public void Reset()
        {
            ResetPaletteUI();
            _currentResource?.Dispose();
            _currentResource = null;
        }

        public void AttachResource(KidPixResource? Resource)
        {
            Reset();

            if (Resource is not BMPResource bmpResource) return;
            _currentResource = bmpResource;
            _debugger.AttachStream(_currentResource.ImageStream);

            OnDisplay();
        }

        private void OnDisplay()
        {
            if (!IsSafe()) return;

            CodeRunLinePreviewBox.Foreground = Brushes.Black;
            CodeRunLinePreviewBox.ToolTip = null;

            PreviewImage.Source = _currentResource.BitmapImage.Convert(true);
            if (API.Importer.tBMP.Decompressor.BMPRLE16Brush.DEBUG_LAST_CALL != null)
            {                
                CodeRunLinePreviewBox.Text = API.Importer.tBMP.Decompressor.BMPRLE16Brush.DEBUG_LAST_CALL.ToString();
                _debugger.HexEditorControl.SelectionStart = API.Importer.tBMP.Decompressor.BMPRLE16Brush.DEBUG_LAST_CALL.OFFSET;
                _debugger.HexEditorControl.SelectionStop = (API.Importer.tBMP.Decompressor.BMPRLE16Brush.DEBUG_LAST_CALL.LENGTH + API.Importer.tBMP.Decompressor.BMPRLE16Brush.DEBUG_LAST_CALL.OFFSET)-1;
                _debuggerWindow.Show();
                if (API.Importer.tBMP.Decompressor.BMPRLE16Brush.DEBUG_LAST_ERROR != null)
                {
                    CodeRunLinePreviewBox.Foreground = Brushes.Red;
                    CodeRunLinePreviewBox.ToolTip = API.Importer.tBMP.Decompressor.BMPRLE16Brush.DEBUG_LAST_ERROR;
                }
            }           
            MakePaletteUI(_currentResource?.BitmapImage?.Palette);
        }

        private record PaletteEntryDescription(int PaletteIndex);

        /// <summary>
        /// Show the palette pane if the given color palette has any entries
        /// </summary>
        /// <param name="palette"></param>
        private void MakePaletteUI(System.Drawing.Imaging.ColorPalette? palette)
        {
            ResetPaletteUI();
            if (palette == default || !palette.Entries.Any()) return;

            bool present = false;
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
                present = true;
            }
            if (present)
            {
                PaletteRule.Width = new GridLength(1, GridUnitType.Star);
                CollapsablePalettePane.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Hide the palette pane and destroy all palette entry controls
        /// </summary>
        private void ResetPaletteUI()
        {
            //deregister all
            foreach(UIElement rectControl in PaletteEntriesGrid.Children)            
                rectControl.MouseLeftButtonUp -= OnPaletteSelection;
            PaletteEntriesGrid.Children.Clear();
            PaletteRule.Width = new GridLength(0, GridUnitType.Star);
            CollapsablePalettePane.Visibility = Visibility.Collapsed;
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
            API.Importer.tBMP.Decompressor.BMPRLE16Brush.DEBUG_MAX_COMMANDS++;
            (Application.Current.MainWindow as MainWindow)?.CurrentResExplorerPage?.ReloadResource();
        }

        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            API.Importer.tBMP.Decompressor.BMPRLE16Brush.DEBUG_MAX_COMMANDS=int.MaxValue;
            (Application.Current.MainWindow as MainWindow)?.CurrentResExplorerPage?.ReloadResource();
        }

        private void NextLineButton_Click(object sender, RoutedEventArgs e)
        {
            API.Importer.tBMP.Decompressor.BMPRLE16Brush.DEBUG_RUNNING_UNTIL_NEXT_SCAN = true;
            (Application.Current.MainWindow as MainWindow)?.CurrentResExplorerPage?.ReloadResource();
        }

        public void Dispose()
        {
            _currentResource?.Dispose();
            _currentResource = null;
        }
    }
}
