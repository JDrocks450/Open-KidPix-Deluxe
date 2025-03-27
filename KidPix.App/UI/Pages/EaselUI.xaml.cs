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
    public partial class EaselUI : Page
    {
        string[] MyReferencedArchives =
        {
            "Easel.MHK", "Tools\\TP.MHK"
        };
        public EaselUI()
        {
            foreach (var resource in MyReferencedArchives)
            {
                MHWKFile easelArchive = MHWKImporter.Import(@"C:\Program Files (x86)\The Learning Company\Kid Pix Deluxe 4\Data\" + resource);
                KidPixUILibrary.LinkedArchives.Add(easelArchive);
            }

            InitializeComponent();            
            Loaded += Easel_Loaded;
        }

        private async void Easel_Loaded(object sender, RoutedEventArgs e)
        {
            //**LOAD INI

            //**LOAD CONTENT            
        }
    }
}
