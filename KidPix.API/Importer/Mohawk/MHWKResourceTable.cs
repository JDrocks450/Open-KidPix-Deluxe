using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace KidPix.API.Importer.Mohawk
{
    public class MHWKResourceTable : IDictionary<CHUNK_TYPE,HashSet<ResourceTableEntry>>
    {
        #region DICTIONARY FUNCTIONS
        private Dictionary<CHUNK_TYPE, HashSet<ResourceTableEntry>> _map = new();

        public HashSet<ResourceTableEntry> this[CHUNK_TYPE key] { get => ((IDictionary<CHUNK_TYPE, HashSet<ResourceTableEntry>>)_map)[key]; set => ((IDictionary<CHUNK_TYPE, HashSet<ResourceTableEntry>>)_map)[key] = value; }

        public ICollection<CHUNK_TYPE> Keys => ((IDictionary<CHUNK_TYPE, HashSet<ResourceTableEntry>>)_map).Keys;

        public ICollection<HashSet<ResourceTableEntry>> Values => ((IDictionary<CHUNK_TYPE, HashSet<ResourceTableEntry>>)_map).Values;

        public int Count => ((ICollection<KeyValuePair<CHUNK_TYPE, HashSet<ResourceTableEntry>>>)_map).Count;

        public bool IsReadOnly => ((ICollection<KeyValuePair<CHUNK_TYPE, HashSet<ResourceTableEntry>>>)_map).IsReadOnly;

        private void EnsureChunkTypePresentOnResourceTableEntry(CHUNK_TYPE key, params ResourceTableEntry[] value)
        {
            foreach (var item in value) 
                item.EnclosingType = key;
        }

        public void Add(CHUNK_TYPE key, ResourceTableEntry value)
        {
            EnsureChunkTypePresentOnResourceTableEntry(key, value);
            if (_map.TryGetValue(key, out var list)) list.Add(value);
            else _map.Add(key, new() { value });
        }

        public void Add(KeyValuePair<CHUNK_TYPE, HashSet<ResourceTableEntry>> item)
        {
            EnsureChunkTypePresentOnResourceTableEntry(item.Key, item.Value.ToArray());
            ((ICollection<KeyValuePair<CHUNK_TYPE, HashSet<ResourceTableEntry>>>)_map).Add(item);
        }

        public void Add(CHUNK_TYPE key, HashSet<ResourceTableEntry> value)
        {
            EnsureChunkTypePresentOnResourceTableEntry(key, value.ToArray());
            ((IDictionary<CHUNK_TYPE, HashSet<ResourceTableEntry>>)_map).Add(key, value);
        }

        public void Clear()
        {
            ((ICollection<KeyValuePair<CHUNK_TYPE, HashSet<ResourceTableEntry>>>)_map).Clear();
        }

        public bool Contains(KeyValuePair<CHUNK_TYPE, HashSet<ResourceTableEntry>> item)
        {
            return ((ICollection<KeyValuePair<CHUNK_TYPE, HashSet<ResourceTableEntry>>>)_map).Contains(item);
        }

        public bool ContainsKey(CHUNK_TYPE key)
        {
            return ((IDictionary<CHUNK_TYPE, HashSet<ResourceTableEntry>>)_map).ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<CHUNK_TYPE, HashSet<ResourceTableEntry>>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<CHUNK_TYPE, HashSet<ResourceTableEntry>>>)_map).CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<CHUNK_TYPE, HashSet<ResourceTableEntry>>> GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<CHUNK_TYPE, HashSet<ResourceTableEntry>>>)_map).GetEnumerator();
        }

        public bool Remove(CHUNK_TYPE key)
        {
            return ((IDictionary<CHUNK_TYPE, HashSet<ResourceTableEntry>>)_map).Remove(key);
        }

        public bool Remove(KeyValuePair<CHUNK_TYPE, HashSet<ResourceTableEntry>> item)
        {
            return ((ICollection<KeyValuePair<CHUNK_TYPE, HashSet<ResourceTableEntry>>>)_map).Remove(item);
        }

        public bool TryGetValue(CHUNK_TYPE key, [MaybeNullWhen(false)] out HashSet<ResourceTableEntry> value)
        {
            return ((IDictionary<CHUNK_TYPE, HashSet<ResourceTableEntry>>)_map).TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_map).GetEnumerator();
        }

        #endregion
    }
}
