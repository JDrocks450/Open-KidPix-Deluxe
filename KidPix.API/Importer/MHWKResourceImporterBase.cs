using KidPix.API.Importer.Mohawk;
using System.Reflection;

namespace KidPix.API.Importer
{
    abstract class ChunkTypedSystemAttribute : Attribute
    {
        protected ChunkTypedSystemAttribute(params CHUNK_TYPE[] Type)
        {
            this.TypesAllowed = Type;
        }

        public CHUNK_TYPE[] TypesAllowed { get; }

        public bool Match(CHUNK_TYPE CheckType) => TypesAllowed.Contains(CheckType);
    }

    [System.AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    internal sealed class MHWKImporterAttribute : ChunkTypedSystemAttribute 
    {
        public MHWKImporterAttribute(params CHUNK_TYPE[] Type) : base(Type) { }
    }

    [System.AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    internal sealed class KidPixResourceAttribute : ChunkTypedSystemAttribute
    {
        public KidPixResourceAttribute(params CHUNK_TYPE[] Type) : base(Type) { }
    }

    public abstract class MHWKResourceImporterBase
    {
        static Dictionary<CHUNK_TYPE, MHWKResourceImporterBase> _generalImporters = new();

        /// <summary>
        /// Gets the lifetime importer of the given type.
        /// <para/><i>These importers are reusable and general-use. They should always be preferred as opposed to making a new instance.</i>
        /// <para/>If one for the given type hasn't been instantiated yet, one will be created and added to the store
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        protected static MHWKResourceImporterBase GetDefaultImporter(CHUNK_TYPE Type)
        {
            if (_generalImporters.TryGetValue(Type, out MHWKResourceImporterBase? importer))
                return importer;
            if (importer != null)
                _generalImporters.Remove(Type); // some kind of error, no idea how this would ever happen?
            MHWKResourceImporterBase newImporter = (MHWKResourceImporterBase)Activator.CreateInstance(GetCorrespondingImporterType(Type));
            _generalImporters.Add(Type, newImporter);
            return newImporter;
        }

        /// <summary>
        /// Finds an importer for the given <see cref="CHUNK_TYPE"/> <paramref name="Type"/>
        /// </summary>
        /// <param name="Type"></param>
        /// <returns></returns>
        protected static Type GetCorrespondingImporterType(CHUNK_TYPE Type) =>
            typeof(MHWKResourceImporterBase).Assembly.GetTypes().FirstOrDefault(x => x.GetCustomAttribute<MHWKImporterAttribute>()?.Match(Type) ?? false)
                ?? typeof(MHWKGeneralImporter);
        /// <summary>
        /// Finds a <see cref="KidPixResource"/> that corresponds with the given <see cref="CHUNK_TYPE"/> <paramref name="Type"/>
        /// </summary>
        /// <param name="Type"></param>
        /// <returns></returns>
        protected static Type GetCorrespondingResourceType(CHUNK_TYPE Type) =>
            typeof(MHWKResourceImporterBase).Assembly.GetTypes().FirstOrDefault(x => x.GetCustomAttribute<KidPixResourceAttribute>()?.Match(Type) ?? false)
                ?? typeof(GenericKidPixResource);

        /// <summary>
        /// Finds the default <see cref="MHWKResourceImporterBase"/> for the given <paramref name="Type"/> and returns the imported <see cref="KidPixResource"/>
        /// </summary>
        /// <param name="Type"></param>
        /// <param name="Stream"></param>
        /// <param name="ParentEntry"></param>
        /// <returns></returns>
        internal static KidPixResource? DefaultImport(Stream Stream, ResourceTableEntry ParentEntry, CHUNK_TYPE? FallbackChunkType = default) =>
            GetDefaultImporter(ParentEntry.EnclosingType ?? FallbackChunkType.Value).Import(Stream, ParentEntry); // will crash if both Chunk Types are somehow null

        /// <summary>
        /// When overridden in a derived class, will import the given <see cref="MHWKFile.ReadResourceDataAsync(ResourceTableEntry)"/> data stream
        /// into a container <see cref="KidPixResource"/> and if a specialized <see cref="MHWKResourceImporterBase"/> is implemented, will be of a corresponding type
        /// <para/>If an importer for this type of data is not implemented, a <see cref="GenericKidPixResource"/> will be returned containing the given data
        /// </summary>
        /// <param name="Stream"></param>
        /// <param name="ParentEntry"></param>
        /// <returns></returns>
        public abstract KidPixResource? Import(Stream Stream, ResourceTableEntry ParentEntry);
    }
}
