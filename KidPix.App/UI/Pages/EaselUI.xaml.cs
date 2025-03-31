using KidPix.API.AppService.Sessions;
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
        //The session this easel will pull data from
        public KidPixSession MySession { get; }

        string[] MyReferencedArchives =
        {
            "Easel.MHK", "Tools\\TP.MHK", "Tools\\Icons.MHK"
        };

        public EaselUI()
        {
            MySession = KidPixSessionManager.ActiveSession;
            if (MySession == null)
                throw new NullReferenceException("There is no currently active KidPixSession for this Easel to function off of");
            KidPixUILibrary.LinkResource(MyReferencedArchives);

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
