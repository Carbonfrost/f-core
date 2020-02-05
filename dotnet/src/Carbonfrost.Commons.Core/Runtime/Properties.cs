//
// Copyright 2005, 2006, 2010, 2019-2020 Carbonfrost Systems, Inc.
// (http://carbonfrost.com)
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
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text;

namespace Carbonfrost.Commons.Core.Runtime {

    [StreamingSource(typeof(PropertiesStreamingSource))]
    public partial class Properties : PropertyStore, IProperties, IMakeReadOnly, IDictionary<string, object> {

        private IDictionary<string, object> _properties = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        private bool _isThisReadOnly;

        internal IDictionary<string, object> InnerMap {
            get {
                return _properties;
            }
        }

        public object this[string property] {
            get {
                return this.GetProperty(property);
            }
            set {
                SetProperty(property, value);
            }
        }

        public Properties() {
        }

        public Properties(IEnumerable<KeyValuePair<string, object>> items) {
            if (items != null) {
                foreach (var kvp in items) {
                    SetProperty(kvp.Key, kvp.Value);
                }
            }
        }

        public Properties(IEnumerable<KeyValuePair<string, string>> items) {
            if (items != null) {
                foreach (var kvp in items) {
                    SetProperty(kvp.Key, kvp.Value);
                }
            }
        }

        public void Add(string property, object value) {
            this.Push(property, value);
        }

        public void CopyFrom(IEnumerable<KeyValuePair<string, object>> other) {
            if (other == null) {
                throw new ArgumentNullException("other");
            }
            foreach (var kvp in other) {
                Add(kvp.Key, kvp.Value);
            }
        }

        public void CopyTo(IProperties other) {
            if (other == null) {
                throw new ArgumentNullException("other");
            }

            foreach (var kvp in this) {
                other.Push(kvp.Key, kvp.Value);
            }
        }

        public void Load(string fileName) {
            if (fileName == null) {
                throw new ArgumentNullException("fileName");
            }
            using (PropertiesReader pr = new PropertiesReader(
                StreamContext.FromFile(fileName))) {
                LoadCore(pr);
            }
        }

        public void Load(StreamContext streamContext, Encoding encoding) {
            if (streamContext == null) {
                throw new ArgumentNullException("streamContext");
            }

            using (PropertiesReader pr = new PropertiesReader(streamContext, encoding)) {
                LoadCore(pr);
            }
        }

        public void Load(Stream stream, Encoding encoding = null) {
            if (stream == null) {
                throw new ArgumentNullException("stream");
            }

            using (StreamReader sr = Utility.MakeStreamReader(stream, encoding)) {
                using (PropertiesReader pr = new PropertiesReader(sr)) {
                    LoadCore(pr);
                }
            }
        }

        public void Save(string fileName) {
            using (var fs = new FileStream(fileName, FileMode.Create)) {
                Save(fs, Encoding.UTF8);
            }
        }

        public void Save(TextWriter writer) {
            using (PropertiesWriter pw = new PropertiesWriter(writer)) {
                SaveCore(pw);
            }
        }

        public void Save(Stream stream, Encoding encoding) {
            using (StreamWriter writer = new StreamWriter(stream, encoding)) {
                using (PropertiesWriter pw = new PropertiesWriter(writer)) {
                    SaveCore(pw);
                }
            }
        }

        public override string ToString() {
            return ToKeyValuePairs(this);
        }

        protected virtual bool IsReadOnlyCore(string property) {
            return false;
        }

        protected virtual void MakeReadOnlyCore() {}

        protected virtual void SetPropertyCore(string property, object value) {
            InnerMap[property] = value;
        }

        protected void ThrowIfReadOnly() {
            if (this._isThisReadOnly)
                throw Failure.ReadOnlyCollection();
        }

        // PropertyStore overrides.
        public override IEnumerator<KeyValuePair<string, object>> GetEnumerator() {
            return InnerMap.GetEnumerator();
        }

        // 'IProperties' implementation.
        public void ClearProperties() {
            ThrowIfReadOnly();
            InnerMap.Clear();
        }

        public void ClearProperty(string property) {
            CheckProperty(property);
            ThrowIfReadOnly();
            ClearPropertyCore(property);
            OnPropertyChanged(new PropertyChangedEventArgs(property));
        }

        public bool TrySetProperty(string property, object value) {
            CheckProperty(property);
            if (_isThisReadOnly || IsReadOnlyCore(property)) {
                return false;
            }
            SetProperty(property, value);
            return true;
        }

        public void SetProperty(string property, object value) {
            CheckProperty(property);
            ThrowIfReadOnly();

            object currentValue;
            bool hasCurrentValue = TryGetPropertyCore(property, typeof(object), out currentValue);

            if (!hasCurrentValue || !object.Equals(value, currentValue)) {
                SetPropertyCore(property, value);
                OnPropertyChanged(new PropertyChangedEventArgs(property));
            }
        }

        // `IDictionary<,>' implementation
        bool IDictionary<string, object>.ContainsKey(string key) {
            return InnerMap.ContainsKey(key);
        }

        bool IDictionary<string, object>.Remove(string key) {
            if (string.IsNullOrEmpty(key)) {
                throw Failure.NullOrEmptyString(nameof(key));
            }

            bool hasKey = InnerMap.ContainsKey(key);
            ClearProperty(key);
            return hasKey;
        }

        bool IDictionary<string, object>.TryGetValue(string key, out object value) {
            return InnerMap.TryGetValue(key, out value);
        }

        ICollection<string> IDictionary<string, object>.Keys {
            get {
                return InnerMap.Keys;
            }
        }

        ICollection<object> IDictionary<string, object>.Values {
            get {
                return InnerMap.Values;
            }
        }

        void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item) {
            if (string.IsNullOrEmpty(item.Key)) {
                throw RuntimeFailure.CannotSpecifyNullKey("item");
            }
            SetProperty(item.Key, item.Value);
        }

        void ICollection<KeyValuePair<string, object>>.Clear() {
            InnerMap.Clear();
        }

        bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item) {
            return InnerMap.Contains(item);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) {
            InnerMap.CopyTo(array, arrayIndex);
        }

        bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item) {
            if (string.IsNullOrEmpty(item.Key)) {
                throw RuntimeFailure.CannotSpecifyNullKey("item");
            }

            object result;
            if (InnerMap.TryGetValue(item.Key, out result) && Equals(result, item.Value)) {
                ClearProperty(item.Key);
                return true;
            }
            return false;
        }

        bool ICollection<KeyValuePair<string, object>>.IsReadOnly {
            get {
                return _isThisReadOnly;
            }
        }

        public int Count {
            get {
                return InnerMap.Count;
            }
        }

        // 'IMakeReadOnly' implementation.
        bool IMakeReadOnly.IsReadOnly { get { return _isThisReadOnly; } }

        public void MakeReadOnly() {
            if (!this._isThisReadOnly) {
                MakeReadOnlyCore();
                this._isThisReadOnly = true;
            }
        }

        protected virtual void ClearPropertyCore(string property) {
            InnerMap.Remove(property);
        }

        public override Type GetPropertyType(string property) {
            CheckProperty(property);

            // Must reimplement this to take advantage of parenting and to
            // check chaining modes
            object objValue;
            if (InnerMap.TryGetValue(property, out objValue)) {
                if (objValue == null)
                    return typeof(object);
                else
                    return objValue.GetType();
            }

            return null;
        }

        protected override bool TryGetPropertyCore(string property, Type requiredType, out object value) {
            // Review the local storage to determine if it is contained
            requiredType = requiredType ?? typeof(object);
            if (InnerMap.TryGetValue(property, out value)) {
                if (requiredType.GetTypeInfo().IsInstanceOfType(value) || value == null) {
                    return true;
                }

                // Type coercion
                var str = value as string;
                if (str != null) {
                    value = Activation.FromText(requiredType, str, null, null);
                    return true;
                }
            }

            value = null;
            return base.TryGetPropertyCore(property, requiredType, out value);
        }

        private void LoadCore(PropertiesReader reader) {
            try {
                RaiseEvents = false;
                while (reader.MoveToProperty()) {
                    this.SetProperty(reader.QualifiedKey, reader.Value);
                }
            } finally {
                RaiseEvents = true;
            }
        }

        private void SaveCore(PropertiesWriter writer) {
            foreach (KeyValuePair<string, object> prop in InnerMap) {
                writer.WriteProperty(prop.Key, prop.Value);
            }
        }
    }
}
