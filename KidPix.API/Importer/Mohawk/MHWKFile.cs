using KidPix.API.Importer.tBMP;
using KidPix.API.Importer.tWAV;

namespace KidPix.API.Importer.Mohawk
{
    /// <summary>
    /// An interface for reading data from a *.MHK file imported using the <see cref="MHWKImporter"/>
    /// </summary>
    public class MHWKFile
    {
        /// <summary>
        /// The name of this file on the disk
        /// </summary>
        public string FileName { get; set; }
        public long FileSize { get; }
        public uint MHWKChunkPayloadSize { get; set; }

        public MHWKFileTable Files { get; internal set; }
        public MHWKResourceTable Resources { get; internal set; }

        internal MHWKFile(string fName, long fSize)
        {
            FileName = fName;
            FileSize = fSize;
        }

        public ResourceTableEntry GetEntryByID(MHWKIdentifierToken AssetID) => Resources.GetEntryByID(AssetID);

        /// <summary>
        /// Basic error-catching for typical errors that can happen while reading.
        /// </summary>
        private void EnsureFileSafeToRead(FileStream FileHandle)
        {
            if (FileSize != FileHandle.Length) // modified size
                throw new InvalidOperationException("The file is a different size than when it was imported -- it cannot be read and should be imported again.");

        }

        /// <summary>
        /// Reads the given <paramref name="AssetID"/> from the current <see cref="MHWKFile"/>
        /// <para/>This data is streamed from the file on the disk -- it cannot have moved to a new name on the disk, it cannot be currently 
        /// locked by another process, it is not expected for it to have been externally modified.
        /// Doing so will cause an exception or undefined behavior that is expected to be handled by the caller.
        /// <para/>If the file moves, it will need to have its <see cref="FileName"/> property modified to the new name.
        /// </summary>
        /// <param name="Stream"></param>
        /// <param name="ResourceDefinition"></param>
        /// <returns></returns>
        public Task ReadResourceDataAsync(Stream Stream, MHWKIdentifierToken AssetID) => ReadResourceDataAsync(Stream,GetEntryByID(AssetID));

        /// <summary>
        /// Reads the given <paramref name="ResourceDefinition"/> from the current <see cref="MHWKFile"/>
        /// <para/>This data is streamed from the file on the disk -- it cannot have moved to a new name on the disk, it cannot be currently 
        /// locked by another process, it is not expected for it to have been externally modified.
        /// Doing so will cause an exception or undefined behavior that is expected to be handled by the caller.
        /// <para/>If the file moves, it will need to have its <see cref="FileName"/> property modified to the new name.
        /// </summary>
        /// <param name="Stream"></param>
        /// <param name="ResourceDefinition"></param>
        /// <returns></returns>
        public async Task ReadResourceDataAsync(Stream Stream, ResourceTableEntry ResourceDefinition)
        {
            long pos = Stream.Position;
            var data = await ReadResourceDataAsync(ResourceDefinition);
            await Stream.WriteAsync(data);
            Stream.Seek(pos, SeekOrigin.Begin);
        }
        /// <summary>
        /// Reads the given <paramref name="ResourceDefinition"/> from the current <see cref="MHWKFile"/>
        /// <para/>This data is streamed from the file on the disk -- it cannot have moved to a new name on the disk, it cannot be currently 
        /// locked by another process, it is not expected for it to have been externally modified.
        /// Doing so will cause an exception or undefined behavior that is expected to be handled by the caller.
        /// <para/>If the file moves, it will need to have its <see cref="FileName"/> property modified to the new name.
        /// </summary>
        /// <param name="Stream"></param>
        /// <param name="ResourceDefinition"></param>
        /// <returns></returns>
        public async Task<byte[]> ReadResourceDataAsync(ResourceTableEntry ResourceDefinition)
        {
            using FileStream fileStream = File.OpenRead(FileName);
            EnsureFileSafeToRead(fileStream);
            fileStream.Seek(ResourceDefinition.Offset, SeekOrigin.Begin);
            byte[] buffer = new byte[ResourceDefinition.Size];
            await fileStream.ReadAsync(buffer, 0, (int)ResourceDefinition.Size);
            return buffer;
        }

        public async Task<KidPixResource?> TryImportResourceAsync(ResourceTableEntry ResourceDefinition)
        {
            MemoryStream stream = new MemoryStream();
            await ReadResourceDataAsync(stream, ResourceDefinition);
            return MHWKResourceImporterBase.DefaultImport(stream, ResourceDefinition);
        }
    }
}
