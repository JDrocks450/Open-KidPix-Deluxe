using KidPix.API.Importer.Mohawk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KidPix.API.Importer
{
    /// <summary>
    /// An object with an exposed <see cref="Stream">
    /// </summary>
    public interface IStreamable
    {
        public Stream DataStream { get; }
    }

    /// <summary>
    /// Base class for all importer resources
    /// </summary>
    public abstract class KidPixResource : IDisposable
    {
        /// <summary>
        /// The chunk type that this <see cref="KidPixResource"/> was found in according to the <see cref="MHWKFile"/> it was read from
        /// </summary>
        public CHUNK_TYPE ResourceChunkType { get; internal set; }

        public ResourceTableEntry ParentEntry
        {
            get;
        }

        internal KidPixResource(ResourceTableEntry ParentEntry)
        {
            this.ParentEntry = ParentEntry;
        }

        public abstract void Dispose();
    }

    /// <summary>
    /// A container for the file stream data of a <see cref="KidPixResource"/> attempted to be imported from a <see cref="MHWKFile"/>
    /// but couldn't be imported as the <see cref="MHWKImporter"/> for that type of data hasn't been implemented yet.
    /// </summary>
    public class GenericKidPixResource : KidPixResource
    {
        public Stream DataStream { get; }

        public GenericKidPixResource(ResourceTableEntry ParentEntry, Stream ResourceDataStream) : base(ParentEntry)
        {
            DataStream = ResourceDataStream;
        }

        public override void Dispose()
        {
            DataStream?.Dispose();
        }
    }
}
