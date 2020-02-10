//
// Copyright 2010, 2019, 2020 Carbonfrost Systems, Inc. (http://carbonfrost.com)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;


namespace Carbonfrost.Commons.Core.Runtime {

    public class NamedObjectCollection<T> : Collection<T> where T : class {

        private readonly IEqualityComparer<string> _comparer;
        private readonly IDictionary<string, T> _dict;

        public ICollection<string> Names {
            get {
                return Dictionary.Keys;
            }
        }

        protected IDictionary<string, T> Dictionary {
            get {
                return _dict;
            }
        }

        public NamedObjectCollection()
            : this(StringComparer.OrdinalIgnoreCase) {}

        public NamedObjectCollection(IEqualityComparer<string> comparer) {
            _comparer = comparer ?? StringComparer.OrdinalIgnoreCase;
            _dict = new Dictionary<string, T>(comparer);
        }

        public IEnumerable<string> GetNames(T item) {
            if (item == null) {
                throw new ArgumentNullException("item");
            }

            foreach (var kvp in Dictionary) {
                if (kvp.Value == item) {
                    yield return kvp.Key;
                }
            }
        }

        public void Add(string name, T item) {
            Add(item);
            AddKey(name, item);
        }

        protected void ChangeItemKey(T item, string newKey) {
            if (!this.ContainsItem(item)) {
                throw Failure.CollectionElementMissing("item");
            }

            string keyForItem = GetNameForItem(item);
            if (!CompareNames(keyForItem, newKey)) {
                if (newKey != null) {
                    this.AddKey(newKey, item);
                }

                if (keyForItem != null) {
                    this.RemoveKey(keyForItem);
                }
            }
        }

        bool RemoveKey(string key) {
            return (_dict.ContainsKey(key) && base.Remove(_dict[key]));
        }

        void AddKey(string key, T item) {
            if (key == null) {
                key = item.GetType().FullName;
                if (_dict.ContainsKey(key))
                    return;
            }

            _dict.Add(key, item);
        }

        protected virtual string GetNameForItem(T item) {
            string name = Utility.LateBoundProperty<string>(item, "Name");
            if (name == null) {
                return null;
            }
            return name.ToString();
        }

        protected override void InsertItem(int index, T item) {
            if (item == null) {
                throw new ArgumentNullException("item");
            }

            string keyForItem = GetNameForItem(item);
            AddKey(keyForItem, item);
            Items.Insert(index, item);
        }

        protected override void ClearItems() {
            _dict.Clear();
            base.ClearItems();
        }

        protected override void SetItem(int index, T item) {
            if (item == null) {
                throw new ArgumentNullException("item");
            }

            string name = GetNameForItem(item);
            string existing = GetNameForItem(Items[index]);

            if (_comparer.Equals(existing, name)) {
                if (name != null) {
                    _dict[name] = item;
                }
            } else {
                AddKey(name, item);

                if (existing != null) {
                    RemoveKey(existing);
                }
            }

            Items[index] = item;
        }

        protected override void RemoveItem(int index) {
            string name = GetNameForItem(Items[index]);
            if (name == null)
                Items.RemoveAt(index);
            else
                RemoveKey(name);
        }

        public T this[string name] {
            get {
                if (name == null) {
                    throw new ArgumentNullException(nameof(name));
                }

                return Dictionary.GetValueOrDefault(name);
            }
            set {
                if (name == null) {
                    throw new ArgumentNullException(nameof(name));
                }
                if (value == null) {
                    throw new ArgumentNullException(nameof(value));
                }
                Remove(name);
                Add(name, value);
            }
        }

        public bool Contains(string name) {
            if (name == null)
                throw new ArgumentNullException("name");

            return _dict.ContainsKey(name);
        }

        public bool TryGetValue(string key, out T value) {
            return Dictionary.TryGetValue(key, out value);
        }

        public bool Remove(string key) {
            if (key == null) {
                throw new ArgumentNullException("key");
            }

            return _dict.ContainsKey(key) && base.Remove(_dict[key]);
        }

        private bool ContainsItem(T item) {
            string name = GetNameForItem(item);
            T result;

            if (name == null)
                return this.Items.Contains(item);
            else
                return _dict.TryGetValue(name, out result) && EqualityComparer<T>.Default.Equals(result, item);
        }

        private bool CompareNames(string a, string b) {
            return _comparer.Equals(a, b);
        }

        internal void _ChangeItemKey(T item, string newKey) {
            this.ChangeItemKey(item, newKey);
        }
    }
}
