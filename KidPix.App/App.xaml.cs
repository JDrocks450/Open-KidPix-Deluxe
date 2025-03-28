using System.Configuration;
using System.Data;
using System.Windows;

namespace KidPix.App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App() : base()
        {
            //**CREATE NEW SESSION
            KidPix.API.AppService.Sessions.KidPixSessionManager.CreateSession(true);
        }
    }
}
