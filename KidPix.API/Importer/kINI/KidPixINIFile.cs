using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace KidPix.API.Importer.kINI
{
    public class KidPixINIFile : IDictionary<string, INISection>
    {
        public INISection Root => this[""];

        public KidPixINIFile()
        {
            Add("", new(""));
        }

        #region IDICTIONARY
        public INISection this[string key] { get => ((IDictionary<string, INISection>)_sections)[key]; set => ((IDictionary<string, INISection>)_sections)[key] = value; }

        public ICollection<string> Keys => ((IDictionary<string, INISection>)_sections).Keys;

        public ICollection<INISection> Values => ((IDictionary<string, INISection>)_sections).Values;

        public int Count => ((ICollection<KeyValuePair<string, INISection>>)_sections).Count;

        public bool IsReadOnly => ((ICollection<KeyValuePair<string, INISection>>)_sections).IsReadOnly;

        private Dictionary<string, INISection> _sections { get; } = new();

        public void Add(string key, INISection value)
        {
            if (_sections.ContainsKey(key)) _sections.Remove(key);
            ((IDictionary<string, INISection>)_sections).Add(key, value);
        }

        public void Add(KeyValuePair<string, INISection> item)
        {
            ((ICollection<KeyValuePair<string, INISection>>)_sections).Add(item);
        }

        public void Clear()
        {
            ((ICollection<KeyValuePair<string, INISection>>)_sections).Clear();
        }

        public bool Contains(KeyValuePair<string, INISection> item)
        {
            return ((ICollection<KeyValuePair<string, INISection>>)_sections).Contains(item);
        }

        public bool ContainsKey(string key)
        {
            return ((IDictionary<string, INISection>)_sections).ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, INISection>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<string, INISection>>)_sections).CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<string, INISection>> GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<string, INISection>>)_sections).GetEnumerator();
        }

        public bool Remove(string key)
        {
            return ((IDictionary<string, INISection>)_sections).Remove(key);
        }

        public bool Remove(KeyValuePair<string, INISection> item)
        {
            return ((ICollection<KeyValuePair<string, INISection>>)_sections).Remove(item);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out INISection value)
        {
            return ((IDictionary<string, INISection>)_sections).TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_sections).GetEnumerator();
        }
        #endregion
    }
}
