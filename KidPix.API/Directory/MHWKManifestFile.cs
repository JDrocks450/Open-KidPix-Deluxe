using KidPix.API.Importer.Mohawk;
using System.Text.Json;

namespace KidPix.API.Directory
{
    /// <summary>
    /// A listing of all resources in a directory or group for quickly loading assets
    /// </summary>
    [Serializable]
    public class MHWKManifestFile
    {
        public const uint CURRENT_VERSION = 1;
        private Dictionary<string, IEnumerable<MHWKManifestEntry>> _map;
        private Dictionary<CHUNK_TYPE, IEnumerable<MHWKManifestEntry>> _typeMap;

        public uint Version { get; set; } = CURRENT_VERSION;
        public string DirectoryLocation { get; set; }
        
        public MHWKManifestFile(string directoryLocation)
        {
            DirectoryLocation = directoryLocation;
        }

        public HashSet<MHWKManifestEntry> Entries { get; set; } = new();

        public Dictionary<string, IEnumerable<MHWKManifestEntry>> Files
        {
            get
            {
                if (_map == null) return ReevaluateFileMap();
                if (_map.Count != Entries.Count) return ReevaluateFileMap();
                return _map;
            }
        }
        public Dictionary<CHUNK_TYPE, IEnumerable<MHWKManifestEntry>> EntriesByType => _typeMap;        

        /// <summary>
        /// Refreshes the <see cref="EntriesByType"/> and <see cref="Files"/> maps
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, IEnumerable<MHWKManifestEntry>> ReevaluateFileMap()
        {
            _map = new();
            _typeMap = new();
            foreach (var entry in Entries)
            {
                //**FILE MAP
                if(_map.TryGetValue(entry.ArchiveLocalPath, out IEnumerable<MHWKManifestEntry> list))                
                    (list as HashSet<MHWKManifestEntry>).Add(entry);
                else
                {
                    HashSet<MHWKManifestEntry> entriesList = new();
                    _map.Add(entry.ArchiveLocalPath, entriesList);
                    entriesList.Add(entry);
                }
                //**TYPE MAP
                if (_typeMap.TryGetValue(entry.ChunkType, out list))
                    (list as HashSet<MHWKManifestEntry>).Add(entry);
                else
                {
                    HashSet<MHWKManifestEntry> entriesList = new();
                    _typeMap.Add(entry.ChunkType, entriesList);
                    entriesList.Add(entry);
                }
            }
            return _map;
        }

        /// <summary>
        /// Saves this manifest file to the disk at the specified location
        /// </summary>
        /// <param name="SavePath"></param>
        /// <exception cref="DirectoryNotFoundException"></exception>
        public void Save(string? SavePath = default)
        {
            string DefaultName() => Path.Combine(DirectoryLocation, "manifest.json");
            string fName = SavePath ?? DefaultName();
            if (!System.IO.Directory.Exists(Path.GetDirectoryName(fName)))
                throw new DirectoryNotFoundException("The directory we're supposedly saving in doesn't exist.");
            using FileStream fs = File.Create(fName);
            var myDoc = JsonSerializer.SerializeToDocument(this);
            using var writer = new Utf8JsonWriter(fs);
            myDoc.WriteTo(writer);
        }

        public string GetFilePath(MHWKManifestEntry Entry) => GetFilePath(Entry.ArchiveLocalPath);
        public string GetFilePath(string LocalFileName) => System.IO.Path.Combine(DirectoryLocation, LocalFileName);
    }
}
