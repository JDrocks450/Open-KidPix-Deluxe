using KidPix.API.Importer;
using KidPix.API.Importer.Graphics;
using KidPix.API.Importer.tBMP.Decompressor;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace KidPix.ResourceExplorer.Controls.ResourcePreview
{
    internal class RasterImageDebugProvider
    {
        public HexEditorResourcePreview _debugger => _debuggerWindow.Content as HexEditorResourcePreview;
        public Window _debuggerWindow;

        public StackPanel _callstackStackpanel;
        public Window _brushRLECallstackWindow;

        public void CreateSession(Brush BackgroundBrush)
        {
            //ENSURE OLD SESSION CLOSED
            CloseSession();

            //**DEBUG
            var bgBrush = BackgroundBrush;
            _debuggerWindow = new()
            {
                Background = bgBrush,
                Width = 800,
                Height = 600,
                Title = "Debug Window",
                Content = new HexEditorResourcePreview()
            };
            _callstackStackpanel = new StackPanel();
            _brushRLECallstackWindow = new()
            {
                Background = bgBrush,
                Width = 400,
                Height = 600,
                Title = "Callstack Window",
                Content = new ScrollViewer() { Content = _callstackStackpanel }
            };
        }

        public void CloseSession()
        {
            _debuggerWindow?.Close();
            _brushRLECallstackWindow?.Close();
            _debuggerWindow = null;
            _brushRLECallstackWindow = null;
        }

        internal void EvaluateDebugWindows(KidPixResource Resource, TextBox OutputBox)
        {
            _debuggerWindow.Hide();
            _brushRLECallstackWindow.Hide();

            if ((((IPaintable)Resource)?.Header?.DrawAlgorithm ?? BitmapDrawCompression.kDrawRaw) is not BitmapDrawCompression.kDrawRLE) 
                return; // Is this a BrushRLE16 image? These debug tools aren't indended for anything else
            if (Resource is IStreamable)
                _debuggerWindow.Show(); // SHOW STREAM
            if (API.Importer.tBMP.Decompressor.BMPRLE16BrushDebug.DEBUG_LAST_CALL is not null)
            {
                //HEX EDITOR DEBUG WINDOW
                OutputBox.Text = API.Importer.tBMP.Decompressor.BMPRLE16BrushDebug.DEBUG_LAST_CALL.ToString();
                if (false) // unsupported 03/25/25
                    Debug_JumpToDrawCallOffset16(API.Importer.tBMP.Decompressor.BMPRLE16BrushDebug.DEBUG_LAST_CALL);
                if (API.Importer.tBMP.Decompressor.BMPRLE16BrushDebug.DEBUG_LAST_ERROR != null)
                { // ENDED WITH AN ERROR
                    OutputBox.Foreground = Brushes.Red;
                    OutputBox.ToolTip = API.Importer.tBMP.Decompressor.BMPRLE16BrushDebug.DEBUG_LAST_ERROR;
                }
            }

            //CALLSTACK DBG WINDOW
            _callstackStackpanel.Children.Clear();
            foreach (KeyValuePair<int, List<API.Importer.tBMP.Decompressor.BMPRLE16Brush.RLEDrawCall>>
                scanGroup in API.Importer.tBMP.Decompressor.BMPRLE16BrushDebug.DEBUG_DrawCallsByRow)
            {
                int scanLine = scanGroup.Key;

                var expander = new Expander()
                {
                    Foreground = Brushes.White,
                    Header = $"Scan{scanLine}",
                };
                expander.Expanded += delegate
                { // Load new Scan Line frame
                    List<API.Importer.tBMP.Decompressor.BMPRLE16Brush.RLEDrawCall> drawCallsList = scanGroup.Value;
                    var listBox = new ListBox()
                    {
                        ItemsSource = drawCallsList
                    };
                    expander.Content = listBox;
                    listBox.SelectionChanged += (object sender, SelectionChangedEventArgs e) =>
                    { // Invoke Hex Control to move to offset of selected DrawCall
                        Debug_JumpToDrawCallOffset16(listBox.SelectedValue as API.Importer.tBMP.Decompressor.BMPRLE16Brush.RLEDrawCall);
                    };
                };
                expander.Collapsed += delegate
                { // unload Scan Line Frame
                    expander.Content = null;
                };
                _callstackStackpanel.Children.Add(expander);
            }
            _brushRLECallstackWindow.Show();
        }

        internal void AttachStream(Stream dataStream) => _debugger.AttachStream(dataStream);

        private void Debug_JumpToDrawCallOffset16(API.Importer.tBMP.Decompressor.BMPRLE16Brush.RLEDrawCall? DrawCall)
        {
            if (DrawCall == null) return;
            _debugger.HexEditorControl.SelectionStart = DrawCall.OFFSET;
            _debugger.HexEditorControl.SelectionStop = (DrawCall.LENGTH + DrawCall.OFFSET) - 1;
            _debuggerWindow.Show();
            _debuggerWindow.Focus();
        }
    }

    /// <summary>
    /// Interaction logic for RasterImageViewer.xaml
    /// </summary>
    public partial class RasterImageViewer : UserControl, IResourcePreviewControl
    {
        const int MAX_PALETTE_ENTRIES = 512, MAX_CALLSTACK_RESOLUTION = 100;

        private RasterImageDebugProvider _debugProvider = new();

        public event EventHandler OnPushResourceInfoUpdate;

        private KidPixResource? _currentResource
        {
            get;
            set;
        }
        private IPaintable? _paintableResource => _currentResource as IPaintable;        

        public RasterImageViewer()
        {
            InitializeComponent();

            Unloaded += delegate
            {
                Dispose();
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

            if (Resource is null) return; // Resource is not null
            if (Resource is not BMPResource bmpResource && Resource is not BMHResource bmhResource) return; // supported types are BMP and BMH            

            _currentResource = Resource;

            DebugPanel.Visibility = Visibility.Collapsed;
            if (BMPRLE16BrushDebug.DEBUGGING_ENABLED)
            {
                DebugPanel.Visibility = Visibility.Visible;
                _debugProvider.CreateSession((Brush)FindResource("GeneralWindowBackgroundColorBrush"));
                if (Resource is IStreamable streamable) // Has an exposed data stream            
                    _debugProvider.AttachStream(streamable.DataStream);
            }

            OnDisplay();

            //BMH files only -- will close pane if not BMH
            UpdateBMHExplorerUIPane();
        }

        private void OnDisplay()
        {
            if (!IsSafe()) return;

            CodeRunLinePreviewBox.Foreground = Brushes.Black;
            CodeRunLinePreviewBox.ToolTip = null;

            try
            { // attempt to display image
                using System.Drawing.Bitmap? bmp = _paintableResource?.Paint();
                PreviewImage.Source = bmp?.Convert(true);
                if (bmp == null) return;

                if (BMPRLE16BrushDebug.DEBUGGING_ENABLED)
                    _debugProvider.EvaluateDebugWindows(_currentResource, CodeRunLinePreviewBox);

                MakePaletteUI(bmp.Palette);
            }
            catch (InvalidDataException ox)
            { // Could not load this resource exception
                PreviewImage.Source = null;
            }
            catch (Exception e)
            { // all other exceptions
                MessageBox.Show(e.Message);
            }
            //update the resource explorer's preview field thingy
            OnPushResourceInfoUpdate?.Invoke(this, new());
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
            PaletteEntriesGrid.Children.Clear();
            PaletteRule.Width = new GridLength(0, GridUnitType.Star);
            CollapsablePalettePane.Visibility = Visibility.Collapsed;
        }

        private void ChangeColorButton_Click(object sender, RoutedEventArgs e)
        {
            if (_paintableResource == null) return;
            System.Windows.Forms.ColorDialog dlg = new()
            {
                AllowFullOpen = true,
                AnyColor = true,
                SolidColorOnly = true
            };
            if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            _paintableResource.SetPaletteToPrimaryColorPalette(dlg.Color, API.Common.GraphicsExtensions.Opaqueness.SemiOpaque);
            OnDisplay();
        }

        private void StepOverButton_Click(object sender, RoutedEventArgs e)
        {
            API.Importer.tBMP.Decompressor.BMPRLE16BrushDebug.DEBUG_MAX_COMMANDS++;
            (Application.Current.MainWindow as MainWindow)?.CurrentResExplorerPage?.ReloadResource();
        }

        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            API.Importer.tBMP.Decompressor.BMPRLE16BrushDebug.DEBUG_MAX_COMMANDS = int.MaxValue;
            (Application.Current.MainWindow as MainWindow)?.CurrentResExplorerPage?.ReloadResource();
        }

        private void NextLineButton_Click(object sender, RoutedEventArgs e)
        {
            API.Importer.tBMP.Decompressor.BMPRLE16BrushDebug.DEBUG_RUNNING_UNTIL_NEXT_SCAN = true;
            (Application.Current.MainWindow as MainWindow)?.CurrentResExplorerPage?.ReloadResource();
        }                

        private void HideBMHExplorerPane()
        {
            CollapsableBMHPane.Visibility = Visibility.Collapsed;
            BMHRule.Width = new GridLength(0);
        }

        private void UpdateBMHExplorerUIPane()
        {
            HideBMHExplorerPane();
            if (_currentResource is not BMHResource bmh) return;
            
            BMHResourceListBox.ItemsSource = bmh.Table.Resources.Values;
            CollapsableBMHPane.Visibility = Visibility.Visible;
            BMHRule.Width = new GridLength(1, GridUnitType.Star);
        }

        private void SelectNewBMHFrame(int ResID)
        {
            if (_currentResource is not BMHResource bmh) return;
            bmh.SetCurrentResource(ResID);            
            OnDisplay();
        }

        private void BMHResourceListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_currentResource is not BMHResource bmh) return;
            if (BMHResourceListBox.SelectedValue == null) return;
            if (BMHResourceListBox.SelectedValue is not BMHFrameInfo info) return;            
            SelectNewBMHFrame(info.ResourceID);
        }

        private void CopyImageClipboardMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetImage((BitmapSource)PreviewImage.Source);
        }        

        public object? GetResourceInformationContext() => _paintableResource?.Header;

        public void Dispose()
        {
            _currentResource?.Dispose();
            _currentResource = null;
            _debugProvider.CloseSession();
        }        
    }
}
