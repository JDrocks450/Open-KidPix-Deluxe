using KidPix.API.Importer.Graphics;
using KidPix.API.Importer.Mohawk;
using KidPix.API.Resources;
using KidPix.App.UI.Util;
using System;
using System.Collections.Generic;
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

namespace KidPix.App.UI.Pages
{
    /// <summary>
    /// Interaction logic for Easel.xaml
    /// </summary>
    public partial class Easel : Page
    {
        public Easel()
        {
            MHWKFile easelArchive = MHWKImporter.Import(@"C:\Program Files (x86)\The Learning Company\Kid Pix Deluxe 4\Data\Easel.MHK");
            KidPixUILibrary.LinkedArchives.Add(easelArchive);
            
            InitializeComponent();            
            Loaded += Easel_Loaded;
        }

        private async void Easel_Loaded(object sender, RoutedEventArgs e)
        {
            //**LOAD INI

            //**LOAD CONTENT            
            Background = await KidPixUILibrary.ResourceToBrush(new API.MHWKIdentifierToken(CHUNK_TYPE.tBMP, (ushort)Easel_BMPResources.EaselBackground));
        }
    }
}
