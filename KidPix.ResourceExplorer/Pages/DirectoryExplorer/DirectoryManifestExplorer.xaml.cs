using KidPix.API;
using KidPix.API.Directory;
using KidPix.API.Importer.Mohawk;
using KidPix.ResourceExplorer.Model.Core;
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

namespace KidPix.ResourceExplorer.Pages.DirectoryExplorer
{
    /// <summary>
    /// Interaction logic for DirectoryManifestExplorer.xaml
    /// </summary>
    public partial class DirectoryManifestExplorer : Page, IResAppPage
    {
        enum ViewMode
        {
            Files,
            Types
        }

        private MHWKManifestFile _currentManifest;
        public IResAppWindow ParentResWindow { get; set; }

        public DirectoryManifestExplorer()
        {
            InitializeComponent();

            Loaded += async delegate
            {
                LoadManifest(await MHWKManifestor.LoadManifestFile(new FileInfo(@"C:\Program Files (x86)\The Learning Company\Kid Pix Deluxe 4\Data\manifest.json")));
            };            
        }

        public void LoadManifest(MHWKManifestFile ManifestFile)
        {
            _currentManifest = ManifestFile;
            if (_currentManifest == null) return;

            ShowItems((ViewMode)TypeTabs.SelectedIndex);
        }

        private void ShowItems(ViewMode Mode)
        {
            ListViewItem selectedControl = null;
            void AddControl(string Name, Action? OnClick, Action? OnDoubleClick)
            {
                ListViewItem ctrl = new()
                {
                    Content = new TextBlock()
                    {
                        Text = Name
                    }
                };
                ctrl.MouseLeftButtonUp += delegate
                {
                    if (selectedControl != null)
                        selectedControl.IsSelected = false;
                    ctrl.IsSelected = true;
                    OnClick?.Invoke();
                    selectedControl = ctrl;
                };
                ctrl.MouseDoubleClick += delegate { OnDoubleClick?.Invoke(); };
                ArchiveExplorer.Children.Add(ctrl);
            }

            ArchiveExplorer.Children.Clear();
            if (_currentManifest == null) return;

            switch (Mode)
            {
                case ViewMode.Files:
                    foreach (var archiveFile in _currentManifest.Files)
                    {
                        AddControl(System.IO.Path.GetFileName(archiveFile.Key),
                            () => HiveSelected(archiveFile.Value),
                            async () => await ArchiveOpenRequested(_currentManifest.GetFilePath(archiveFile.Key)));
                    }
                    break;
                case ViewMode.Types:                    
                    foreach (var typeGroup in _currentManifest.EntriesByType)
                    {                        
                        AddControl(MHWKTypeDescription.GetTypeName(typeGroup.Key),
                            () => HiveSelected(typeGroup.Value), null);
                    }
                    break;
            }
        }

        private void HiveSelected(IEnumerable<MHWKManifestEntry> Entries)
        {
            ArchiveTree.Items.Clear();
            Dictionary<CHUNK_TYPE, TreeViewItem> groups = new();

            foreach (var resourceEntry in Entries)
            {
                if (!groups.TryGetValue(resourceEntry.ChunkType, out TreeViewItem parent))
                {
                    parent = new()
                    {
                        Header = MHWKTypeDescription.GetTypeName(resourceEntry.ChunkType)
                    };
                    ArchiveTree.Items.Add(parent);
                    groups.Add(resourceEntry.ChunkType, parent);
                }
                var newItem = new TreeViewItem()
                {
                    Header = resourceEntry.AssetName ?? $"{MHWKTypeDescription.GetChunkTypeStringFromBytes(resourceEntry.ChunkType)}_{resourceEntry.AssetID}"
                };
                newItem.Selected += async delegate
                {
                    await ArchiveOpenRequested(_currentManifest.GetFilePath(resourceEntry.ArchiveLocalPath), new(resourceEntry.ChunkType, resourceEntry.AssetID));
                };
                parent.Items.Add(newItem);
            }
        }

        private async Task ArchiveOpenRequested(string MohawkArchiveFileName, MHWKIdentifierToken? AssetID = default)
        {
            await ((MainWindow)ParentResWindow).InvokeOpenResourceExplorer(new (MohawkArchiveFileName), AssetID);
        }

        public Task<IResAppNavEventResult> NotifyResAppNavigatingAway()
        {
            return ((IResAppPage)this).GetNavAccept();
        }

        private void TypeTabs_SelectionChanged(object sender, SelectionChangedEventArgs e) => ShowItems((ViewMode)TypeTabs.SelectedIndex);
    }
}
