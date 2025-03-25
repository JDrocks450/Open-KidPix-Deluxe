using KidPix.API.Importer.Mohawk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace KidPix.API.Directory
{
    /// <summary>
    /// Scrubs a directory structure for all <see cref="MHWKFile"/> and add them to a map of resources to load them easily
    /// </summary>
    public class MHWKManifestor
    {
        /// <summary>
        /// Loads a <see cref="MHWKManifestFile"/> saved using the <see cref="MHWKManifestFile.Save(string?)"/> method
        /// </summary>
        /// <param name="ManifestFileInfo"></param>
        /// <returns></returns>
        public static async Task<MHWKManifestFile?> LoadManifestFile(FileInfo ManifestFileInfo)
        {
            using FileStream fs = File.OpenRead(ManifestFileInfo.FullName);
            var manifest = await JsonSerializer.DeserializeAsync<MHWKManifestFile>(fs);
            if (manifest == null) return null;
            manifest.DirectoryLocation = ManifestFileInfo.DirectoryName;
            return manifest;
        }

        /// <summary>
        /// Creates a new <see cref="MHWKManifestFile"/> from the specified <paramref name="Directory"/>
        /// </summary>
        /// <param name="Directory"></param>
        /// <param name="PatternMatch"></param>
        /// <returns></returns>
        public static MHWKManifestFile CreateManifestDirectory(DirectoryInfo Directory, string PatternMatch = "*.mhk")
        {
            MHWKManifestFile manifestFile = new(Directory.FullName);
            void ScrubDir(DirectoryInfo LocalDirectory)
            {
                foreach(DirectoryInfo directory in LocalDirectory.EnumerateDirectories())
                {
                    ScrubDir(directory);
                }
                foreach (FileInfo file in LocalDirectory.EnumerateFiles(PatternMatch))
                {
                    string myLocalPath = file.FullName.Remove(0, Directory.FullName.Length); // maybe dangerous? maybe awesome? B)
                    MHWKFile mArchive = MHWKImporter.Import(file.FullName);
                    foreach (var newEntry in mArchive.Resources.SelectMany(x => x.Value).Select(y =>
                                            new MHWKManifestEntry(myLocalPath, y.Id, y.Name, y.EnclosingType)))
                    {
                        MHWKIdentifierToken myID = new MHWKIdentifierToken(newEntry.ChunkType, newEntry.AssetID);
                        if (!manifestFile.Entries.Add(newEntry))
                            throw new Exception("Error adding an entry to the manifest.");
                    }
                }
            }
            ScrubDir(Directory);
            return manifestFile;
        }
    }
}
