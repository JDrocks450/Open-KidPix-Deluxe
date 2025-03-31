using KidPix.API;
using KidPix.API.Directory;
using KidPix.API.Importer;
using KidPix.API.Importer.Graphics;
using KidPix.API.Importer.kINI;
using KidPix.API.Importer.Mohawk;
using KidPix.API.Importer.tBMP.Decompressor;
using KidPix.API.Importer.tWAV;
using KidPix.ResourceExplorer.Model.Core;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
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

namespace KidPix.ResourceExplorer.Pages.ResourceExplorer
{
    /// <summary>
    /// Interaction logic for ResourceExplorerResPage.xaml
    /// </summary>
    public partial class ResourceExplorerResPage : Page, IResAppPage, INotifyPropertyChanged
    {
        private MHWKFile? CurrentFile { get; set; }
        private ResourceTableEntry CurrentEntry { get; set; }        

        public ResourceExplorerResPage()
        {
            InitializeComponent();

            Title = $"{App.AppName}";
            Loaded += ResourceExplorerResPage_Loaded;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void ResourceExplorerResPage_Loaded(object sender, RoutedEventArgs e)
        {            
            KidPixINIFile ini = INIImporter.Import(@"C:\Program Files (x86)\The Learning Company\Kid Pix Deluxe 4\KP.cfg");
        }

        public void LoadMohawkArchiveFile(FileInfo MohawkArchive, MHWKIdentifierToken? SelectedAsset = default)
        {
            if (MohawkArchive == default) return;

            var file = CurrentFile = MHWKImporter.Import(MohawkArchive.FullName);
            if (file == null) return;

            ResourceExplorer.Items.Clear();
            foreach (var type in file.Resources)
            {                
                MHWKTypeDescription.TryGetTypeName(type.Key, out var name);
                string TagArchiveStr = MHWKTypeDescription.GetChunkTypeStringFromBytes(type.Key);
                ContentControl HeaderControl = new()
                {
                    Content = $"{name ?? "Unsupported"} ({TagArchiveStr})"
                };
                var typeNode = new TreeViewItem()
                {
                    Header = HeaderControl
                };
                ResourceExplorer.Items.Add(typeNode);
                foreach (ResourceTableEntry resource in type.Value)
                {
                    ContentControl LocalHeaderControl = new()
                    {
                        Content = string.IsNullOrWhiteSpace(resource.Name) ? $"{TagArchiveStr}_{resource.Id.ToString()}" : resource.Name
                    };
                    TreeViewItem resNode = new TreeViewItem()
                    { 
                        Header = LocalHeaderControl,
                        IsExpanded = true
                    };
                    typeNode.Items.Add(resNode);
                    resNode.Selected += async delegate { await ResourceSelected(resource); };
                    if (resource.GetIdentifierToken() == SelectedAsset)
                        resNode.IsSelected = true;
                }
            }

            Title = $"{App.AppName} - {System.IO.Path.GetFileName(MohawkArchive.FullName)}";
        }        

        private async Task ResourceSelected(ResourceTableEntry Entry)
        {
            SaveButton.IsEnabled = CurrentFile != null;
            
            if (CurrentFile == default) return;

            ResourceInformation.Breakdown(Entry);

            if (CurrentEntry != Entry)
                BMPRLE16BrushDebug.ClearSession();           

            CurrentEntry = Entry;
            SaveButton.IsEnabled = CurrentEntry != null;

            //**AUTO OPEN
            if (CurrentFile == default || CurrentEntry == default) return;

            //**WAV IMPORT
            KidPixResource? kpResource = default;
            try
            {
                kpResource = await CurrentFile.TryImportResourceAsync(CurrentEntry);                
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message);
                kpResource = new GenericKidPixResource(Entry, new MemoryStream(await CurrentFile.ReadResourceDataAsync(Entry)));
            }    
            if (kpResource != null)
                ResourcePreview.AttachResource(kpResource);        
        }

        public Task ReloadResource() => ResourceSelected(CurrentEntry);

        public IResAppWindow ParentResWindow { get; set; }

        public Task<IResAppNavEventResult> NotifyResAppNavigatingAway() => ((IResAppPage)this).GetNavAccept();

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentFile == default || CurrentEntry == default) { SaveButton.IsEnabled = false; return; }

            string? fileName = App.UserSaveSingleFile("Binary Data File (*.dat)|*.dat");
            if (string.IsNullOrWhiteSpace(fileName)) return;
            using FileStream fileHandle = File.Create(fileName);
            await CurrentFile.ReadResourceDataAsync(fileHandle, CurrentEntry);
        }
    }
}
