using KidPix.API.Importer;
using KidPix.API.Importer.Mohawk;
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

            Loaded += ResourceExplorerResPage_Loaded;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void ResourceExplorerResPage_Loaded(object sender, RoutedEventArgs e)
        {
            ((IResAppPage)this).TryRegisterMenuItem("File\\Open", delegate { PromptUserLoadFile(); }, out _);

            LoadFile(App.AutoOpen);
        }

        public void LoadFile(string FileName)
        {
            var file = CurrentFile = MHWKImporter.Import(FileName);
            if (file == null) return;

            ResourceTypeExplorer.Items.Clear();
            foreach (var type in file.Resources)
            {
                MHWKTypeDescription.TryGetTypeName(type.Key, out var name);
                string TagArchiveStr = new string(Encoding.UTF8.GetString(BitConverter.GetBytes((uint)type.Key)).Reverse().ToArray());
                var typeNode = new TreeViewItem()
                {
                    Header = $"{name ?? "Unsupported"} ({TagArchiveStr})"
                };
                ResourceTypeExplorer.Items.Add(typeNode);
                foreach (ResourceTableEntry resource in type.Value)
                {
                    TreeViewItem resNode = new TreeViewItem()
                    { 
                        Header = string.IsNullOrWhiteSpace(resource.Name) ? $"{TagArchiveStr}_{resource.Id.ToString()}" : resource.Name 
                    };
                    typeNode.Items.Add(resNode);
                    resNode.Selected += delegate { ResourceSelected(resource); };
                }
            }
        }

        private void PromptUserLoadFile()
        {
            OpenFileDialog dialog = new()
            {
                Title = "Open a *.MHK File",
                Filter = "Mohawk Archive File Format (*.MHK)|*.MHK",
                InitialDirectory = @"C:\Program Files (x86)\The Learning Company\Kid Pix Deluxe 4\Data",//Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                RestoreDirectory = true,
                CheckFileExists = true,
                Multiselect = false
            };
            if (!dialog.ShowDialog() ?? true || string.IsNullOrWhiteSpace(dialog.FileName))
                return;
            LoadFile(dialog.FileName);
        }

        private async Task ResourceSelected(ResourceTableEntry Entry)
        {
            SaveButton.IsEnabled = CurrentFile != null;
            
            if (CurrentFile == default) return;

            ResourceInformation.Breakdown(Entry);

            CurrentEntry = Entry;
            SaveButton.IsEnabled = CurrentEntry != null;

            //**AUTO OPEN
            if (CurrentFile == default || CurrentEntry == default) return;

            //**WAV IMPORT
            try
            {
                var kpResource = await CurrentFile.TryImportResourceAsync(CurrentEntry);
                ResourcePreview.AttachResource(kpResource);
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message);
                return;
            }            
        }

        public Task ReloadResource() => ResourceSelected(CurrentEntry);

        public IResAppWindow ParentResWindow { get; set; }

        public Task<IResAppNavEventResult> NotifyResAppNavigatingAway() => ((IResAppPage)this).GetNavAccept();

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentFile == default || CurrentEntry == default) { SaveButton.IsEnabled = false; return; }

            SaveFileDialog saveFileDialog = new()
            {
                CheckFileExists = false,
                AddExtension = true,
                Filter = "Binary Data File (*.dat)|*.dat",
                Title = "Save to Destination",
                DefaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                RestoreDirectory = true,
                OverwritePrompt = true
            };
            if (!saveFileDialog.ShowDialog() ?? true || string.IsNullOrWhiteSpace(saveFileDialog.FileName))
                return;
            using FileStream fileHandle = File.Create(saveFileDialog.FileName);
            await CurrentFile.ReadResourceDataAsync(fileHandle,CurrentEntry);
        }

        private async void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
