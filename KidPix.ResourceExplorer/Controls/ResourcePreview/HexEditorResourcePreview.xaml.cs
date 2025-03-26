using KidPix.API.Importer;
using KidPix.API.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfHexaEditor.Properties;

namespace KidPix.ResourceExplorer.Controls.ResourcePreview
{
    /// <summary>
    /// Interaction logic for HexEditorResourcePreview.xaml
    /// </summary>
    public partial class HexEditorResourcePreview : UserControl, IResourcePreviewControl
    {
        KidPixResource? _currentResource = null;

        public event EventHandler OnPushResourceInfoUpdate;

        public HexEditorResourcePreview()
        {
            InitializeComponent();

            Unloaded += delegate
            {
                Dispose();
            };            
        }

        public void AttachResource(KidPixResource? Resource)
        {
            AttachStream((Resource as GenericKidPixResource)?.DataStream);
        }
        public void AttachStream(Stream? DataStream) => HexEditorControl.Stream = DataStream;
        private void HexEditorControl_SelectionChanged(object sender, EventArgs e)
        {
            byte[] bytesSelected = HexEditorControl.GetCopyData(HexEditorControl.SelectionStart, HexEditorControl.SelectionStop, false);
            if (bytesSelected != null)
             RebuildDataInspector((LittleEndianRadio?.IsChecked ?? false) ? Endianness.LittleEndian : Endianness.BigEndian, bytesSelected);
        }
        private void RebuildDataInspector( Endianness ByteOrder, params byte[] Inspect)
        {
            void Register(string Descriptor, Func<string> ValueStrFormFunc)
            {
                int MARGIN = 5;
                DataInspectorGrid.Children.Add(new TextBlock()
                {
                    Margin = new Thickness(MARGIN),
                    Text = Descriptor,
                    FontWeight = FontWeights.Bold
                });
                string evalutation = "";
                try
                {
                    evalutation = ValueStrFormFunc.Invoke();
                }
                catch { }
                DataInspectorGrid.Children.Add(new TextBox()
                {
                    Background = null,
                    IsReadOnly = true,
                    Foreground = Brushes.White,
                    Margin = new Thickness(MARGIN),
                    Text = evalutation,
                });
            }
            void InspectData(byte[] Bytes, Endianness Endian = Endianness.BigEndian, string? Format = default)
            {
                using MemoryStream ms = new(Bytes);
                EndianBinaryReader reader = new EndianBinaryReader(ms)
                {
                    DisallowAdvance = true
                };

                //**SBYTE
                Register("sbyte (Int8)", () => reader.ReadInt8().ToString(Format));
                //BYTE
                Register("byte (UInt8)", () => reader.ReadByte().ToString(Format));
                //SHORT
                Register("short (Int16)", () => reader.ReadInt16(Endian).ToString(Format));
                //USHORT
                Register("ushort (UInt16)", () => reader.ReadUInt16(Endian).ToString(Format));
                //INT
                Register("int (Int32)", () => reader.ReadInt32(Endian).ToString(Format));
                //UINT
                Register("uint (UInt32)", () => reader.ReadUInt32(Endian).ToString(Format));
                //LONG
                Register("long (Int64)", () => reader.ReadInt64(Endian).ToString(Format));
                //ULONG
                Register("ulong (UInt64)", () => reader.ReadUInt64(Endian).ToString(Format));
                Register("SELECT", () => Bytes.Length.ToString());
                Register("SELECT (0x)", () => Bytes.Length.ToString("X8"));
            }
            DataInspectorGrid.Children.Clear();
            InspectData(Inspect,ByteOrder);
        }

        public void Dispose()
        {
            _currentResource?.Dispose();
        }

        private void LittleEndianRadio_Checked(object sender, RoutedEventArgs e)
        {
            HexEditorControl_SelectionChanged(sender, null);
        }

        public object? GetResourceInformationContext() => _currentResource;
    }
}
