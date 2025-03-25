using Microsoft.Win32;
using System.Configuration;
using System.Data;
using System.Windows;

namespace KidPix.ResourceExplorer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public const string AutoOpen = @"C:\Program Files (x86)\The Learning Company\Kid Pix Deluxe 4\Data\Splash.MHK";
        public const string KidPix4DeluxeFilePath = @"C:\Program Files (x86)\The Learning Company\Kid Pix Deluxe 4\Data";
        public const string AppName = "Kid Pix Deluxe 4 Resource Explorer";

        internal static string? UserOpenSingleFile(string Title, string Filter, ref OpenFileDialog Dlg, string? InitialDirectory = default)
        {
            if (InitialDirectory == default) InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            OpenFileDialog dialog = Dlg; 
            if(dialog == null)
                dialog = Dlg = new()
                {
                    Title = Title,
                    Filter = Filter,
                    InitialDirectory = InitialDirectory,
                    DefaultDirectory = InitialDirectory,
                    CheckFileExists = true,
                    Multiselect = false
                };

            if (!dialog.ShowDialog() ?? true || string.IsNullOrWhiteSpace(dialog.FileName))
                return null;
            return dialog.FileName;
        }
        internal static string? UserOpenSingleFile(string Title, string Filter, string? InitialDirectory = default)
        {
            OpenFileDialog dlg = null;
            return UserOpenSingleFile(Title, Filter, ref dlg, InitialDirectory);
        }
        internal static string? UserOpenDirectory(string Title, string? DefaultDirectory = default)
        {
            OpenFolderDialog ofd = new()
            {
                Title = Title,
                Multiselect = false,
                DefaultDirectory = DefaultDirectory,
                DereferenceLinks = true,
                InitialDirectory = DefaultDirectory,                
            };
            if (!ofd.ShowDialog() ?? true || string.IsNullOrWhiteSpace(ofd.FolderName)) return default;
            return ofd.FolderName;
        }

        internal static string? UserSaveSingleFile(string FileExtensionFilter, string Title = "Save as...", string? DefaultDirectory = default)
        {
            SaveFileDialog saveFileDialog = new()
            {
                CheckFileExists = false,
                AddExtension = true,
                Filter = FileExtensionFilter,
                Title = Title,
                DefaultDirectory = DefaultDirectory ?? Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                RestoreDirectory = true,
                OverwritePrompt = true
            };
            if (!saveFileDialog.ShowDialog() ?? true || string.IsNullOrWhiteSpace(saveFileDialog.FileName))
                return default;
            return saveFileDialog.FileName;
        }
    }

}
