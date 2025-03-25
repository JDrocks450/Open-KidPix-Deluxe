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
using KidPix.ResourceExplorer.Pages.DirectoryExplorer;
using KidPix.API.Directory;
using System.IO;
using KidPix.API;

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
            //**VITAL ITEMS**
            EnsureVitalMenuItems();
            //**MENU ITEMS
            ((IResAppWindow)this).TryRegisterMenuItem("File\\Open Mohawk Archive (*.MHK, *.KPA, etc.)", async delegate { await InvokeOpenResourceExplorer(); }, out _);
            ((IResAppWindow)this).TryRegisterMenuItem("Edit\\Build Kid Pix Directory Manifest", delegate
            { // REBUILD MANIFEST MENU ITEM CALLBACK
                string? fName = App.UserOpenDirectory("Select where Kid Pix 4 Deluxe is installed", App.KidPix4DeluxeFilePath);
                if (string.IsNullOrWhiteSpace(fName)) return;
                MHWKManifestFile manifest = MHWKManifestor.CreateManifestDirectory(new DirectoryInfo(fName));
                manifest.Save("E:\\manifest.json");
            }, out _);
            ((IResAppWindow)this).TryRegisterMenuItem("File\\Open Kid Pix Directory Manifest", async delegate
            { // LOAD MANIFEST MENU ITEM CALLBACK
                string? fName = App.UserOpenSingleFile("Select a Manifest", "Manifest File (*.json)|*.json", App.KidPix4DeluxeFilePath);
                if (string.IsNullOrWhiteSpace(fName)) return;
                MHWKManifestFile? manifest = await MHWKManifestor.LoadManifestFile(new FileInfo(fName));
            }, out _);

            //**default page
            await NavigateNewPrimaryPageAsync(new DirectoryManifestExplorer());
        }

        private void EnsureVitalMenuItems()
        {
            TryRegisterMenuItem("File\\Exit", delegate { Application.Current.Shutdown(); }, out _);
            TryRegisterMenuItem("File\\!-", null, out _);            
        }

        public async Task InvokeOpenResourceExplorer(FileInfo? MohawkArchiveFileInfo = default, MHWKIdentifierToken? SelectedAsset = default)
        {
            if (MohawkArchiveFileInfo == null)
            { // PROMPT USER SELECT FILE
                string? FileName = App.UserOpenSingleFile("Open a *.MHK File", "Mohawk Archive (*.MHK)|*.MHK", App.KidPix4DeluxeFilePath);
                if (string.IsNullOrWhiteSpace(FileName)) return;
                MohawkArchiveFileInfo = new(FileName);
            }
            var resPage = new ResourceExplorerResPage();
            await NavigateNewPrimaryPageAsync(resPage);            
            resPage.LoadMohawkArchiveFile(MohawkArchiveFileInfo, SelectedAsset);
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
                if (searchToken == "!-") // separator
                {
                    searchCollection.Insert(0, new Separator());
                    continue;
                }

                currentItem = searchCollection.OfType<MenuItem>().FirstOrDefault(x => x.Header.ToString() == searchToken);

                if (currentItem == null)
                {
                    currentItem = new MenuItem()
                    {
                        Header = searchToken
                    };
                    searchCollection.Insert(0,currentItem);
                }

                searchCollection = currentItem.Items;                
            }

            if (currentItem != null && ActionCallback != null)
                currentItem.Click += ActionCallback;

            CreatedItem = currentItem;
            return currentItem != null;
        }

        #endregion
    }
}