using KidPix.ResourceExplorer.Model.Core;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Linq.Expressions;
using KidPix.ResourceExplorer.Pages.ResourceExplorer;

namespace KidPix.ResourceExplorer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IResAppWindow
    {
        private IResAppPage? PrimaryPage => PrimaryResPageFrame.Content as IResAppPage;

        public ResourceExplorerResPage? CurrentResExplorerPage => PrimaryPage as ResourceExplorerResPage;

        public MainWindow()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ClearAllMenuItems();
            EnsureVitalMenuItems();

            //**default page
            await NavigateNewPrimaryPageAsync(new ResourceExplorerResPage());
        }

        private void EnsureVitalMenuItems()
        {
            TryRegisterMenuItem("File\\Exit", delegate { Application.Current.Shutdown(); }, out _);
        }

        #region ResApp Interface

        public void ClearAllMenuItems()
        {
            AppMainMenuStrip.Items.Clear();
        }

        public async Task<IResAppNavEventResult> NavigateNewPrimaryPageAsync<T>(T NewPage) where T : Page, IResAppPage
        {
            IResAppNavEventResult result = IResAppNavEventResult.Navigated;
            if (PrimaryPage != null)
                result = await PrimaryPage.NotifyResAppNavigatingAway();
            if (result != IResAppNavEventResult.Navigated || result != IResAppNavEventResult.Navigated)
                return result;
            PrimaryResPageFrame.Content = NewPage;
            NewPage.ParentResWindow = this;
            return IResAppNavEventResult.Navigated;
        }

        public bool TryDeregisterMenuItem(MenuItem Item)
        {
            if (Item.Parent == null) return false;
            if (Item.Parent is MenuItem parentItem)
            {
                parentItem.Items.Remove(Item);
                return true;
            }
            return false;
        }

        public bool TryRegisterMenuItem(string Path, RoutedEventHandler ActionCallback, out MenuItem? CreatedItem)
        {
            string[] itemsPath = Path.Split("\\");
            CreatedItem = null;

            if (!itemsPath.Any()) return false;
            MenuItem? currentItem = default;
            ItemCollection searchCollection = AppMainMenuStrip.Items;

            for (int i = 0; i < itemsPath.Length; i++)
            {                
                string searchToken = itemsPath[i];
                currentItem = searchCollection.OfType<MenuItem>().FirstOrDefault(x => x.Header.ToString() == searchToken);

                if (currentItem == null)
                {
                    currentItem = new MenuItem()
                    {
                        Header = searchToken
                    };
                    searchCollection.Add(currentItem);
                }

                searchCollection = currentItem.Items;                
            }

            if (currentItem != null)
                currentItem.Click += ActionCallback;

            CreatedItem = currentItem;
            return currentItem != null;
        }

        #endregion
    }
}