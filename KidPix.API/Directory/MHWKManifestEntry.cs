using KidPix.API.Importer.Mohawk;

namespace KidPix.API.Directory
{
    /// <summary>
    /// An entry in a <see cref="MHWKManifestFile"/>
    /// </summary>
    /// <param name="ArchiveLocalPath"> The localized path to the <see cref="MHWKFile"/> that contains this resource </param>
    /// <param name="AssetID"></param>
    /// <param name="AssetName"> Gets (or sets) the name given to the asset
    /// <para/>This will only persist in this Manifest file -- it will not permutate to the original <see cref="MHWKFile"/>
    /// this asset belongs to </param>
    /// <param name="ChunkType"> The type of data this entry contains </param>
    public record MHWKManifestEntry(string ArchiveLocalPath, ushort AssetID, string AssetName, CHUNK_TYPE ChunkType)
    {
        private string _archiveName = ArchiveLocalPath;

        public string ArchiveLocalPath { get => _archiveName.Trim('\\'); set => _archiveName = value; }
        
        /// <summary>
        /// Gets the name of the enclosing Mohawk Archive
        /// </summary>
        public string ArchiveName => Path.GetFileNameWithoutExtension(ArchiveLocalPath);
    }
}
