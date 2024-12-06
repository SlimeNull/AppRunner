using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppRunner.Helpers
{
    public class DictionaryProxy<TKey, TValue> : IDictionary<TKey, TValue?>
        where TKey : notnull
    {
        private readonly Dictionary<TKey, TValue?> _storage;
        private IDictionary<TKey, TValue?>? _fallback;
        public TValue? DefaultValue { get; set; }

        public ICollection<TKey> Keys => ((IDictionary<TKey, TValue?>)_storage).Keys;

        public ICollection<TValue?> Values => ((IDictionary<TKey, TValue?>)_storage).Values;

        public int Count => ((ICollection<KeyValuePair<TKey, TValue?>>)_storage).Count;

        public bool IsReadOnly => ((ICollection<KeyValuePair<TKey, TValue?>>)_storage).IsReadOnly;

        public TValue? this[TKey key]
        {
            get
            {
                if (_storage.TryGetValue(key, out var value))
                {
                    return value;
                }

                if (_fallback is not null &&
                    _fallback.TryGetValue(key, out value))
                {
                    this[key] = value;
                    return value;
                }

                return DefaultValue;
            }
            set => ((IDictionary<TKey, TValue?>)_storage)[key] = value;
        }

        public DictionaryProxy(IDictionary<TKey, TValue?>? fallback)
        {
            _storage = new Dictionary<TKey, TValue?>();
            _fallback = fallback;
        }

        public void Add(TKey key, TValue? value) => ((IDictionary<TKey, TValue?>)_storage).Add(key, value);
        public bool ContainsKey(TKey key) => ((IDictionary<TKey, TValue?>)_storage).ContainsKey(key);
        public bool Remove(TKey key) => ((IDictionary<TKey, TValue?>)_storage).Remove(key);
        public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue? value)
        {
            if (_storage.TryGetValue(key, out value))
            {
                return true;
            }

            if (_fallback != null &&
                _fallback.TryGetValue(key, out value))
            {
                this[key] = value;
                return true;
            }

            value = DefaultValue;
            this[key] = value;
            return true;
        }

        public void Add(KeyValuePair<TKey, TValue?> item) => ((ICollection<KeyValuePair<TKey, TValue?>>)_storage).Add(item);
        public void Clear() => ((ICollection<KeyValuePair<TKey, TValue?>>)_storage).Clear();
        public bool Contains(KeyValuePair<TKey, TValue?> item) => ((ICollection<KeyValuePair<TKey, TValue?>>)_storage).Contains(item);
        public void CopyTo(KeyValuePair<TKey, TValue?>[] array, int arrayIndex) => ((ICollection<KeyValuePair<TKey, TValue?>>)_storage).CopyTo(array, arrayIndex);
        public bool Remove(KeyValuePair<TKey, TValue?> item) => ((ICollection<KeyValuePair<TKey, TValue?>>)_storage).Remove(item);
        public IEnumerator<KeyValuePair<TKey, TValue?>> GetEnumerator() => ((IEnumerable<KeyValuePair<TKey, TValue?>>)_storage).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_storage).GetEnumerator();
    }
}
