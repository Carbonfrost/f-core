//
// Copyright 2005, 2006, 2010, 2016 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Carbonfrost.Commons.Core.Runtime {

    partial class Properties {

        class PropertiesDictionaryAdapter<TValue> : IProperties {

            private readonly IReadOnlyDictionary<string, TValue> _value;

            public PropertiesDictionaryAdapter(IReadOnlyDictionary<string, TValue> value) {
                _value = value;
            }

            public event PropertyChangedEventHandler PropertyChanged {
                add {
                }
                remove {
                }
            }

            public void ClearProperties() {
                throw Failure.ReadOnlyCollection();
            }

            public void ClearProperty(string property) {
                throw Failure.ReadOnlyCollection();
            }

            public bool TrySetProperty(string property, object value) {
                return false;
            }

            public void SetProperty(string property, object value) {
                throw Failure.ReadOnlyCollection();
            }

            public IEnumerator<KeyValuePair<string, object>> GetEnumerator() {
                return _value.Select(k => new KeyValuePair<string, object>(k.Key, k.Value)).GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
                return GetEnumerator();
            }

            public Type GetPropertyType(string property) {
                return PropertyProvider.InferPropertyType(this, property);
            }

            public bool TryGetProperty(string property, Type propertyType, out object value) {
                if (property == null) {
                    throw new ArgumentNullException("property");
                }
                if (_value.TryGetValue(property, out TValue result) && propertyType.IsInstanceOfType(result)) {
                    value = result;
                    return true;
                }
                value = null;
                return false;
            }
        }

        private class ReadOnlyProperties : IProperties {

            event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged {
                add {}
                remove {}
            }

            private readonly IPropertyStore _store;

            public ReadOnlyProperties(IPropertyStore _store) {
                this._store = _store;
            }

            void IProperties.ClearProperties() {
                throw Failure.ReadOnlyCollection();
            }

            void IProperties.ClearProperty(string property) {
                throw Failure.ReadOnlyCollection();
            }

            void IProperties.SetProperty(string property, object value) {
                throw Failure.ReadOnlyCollection();
            }

            bool IPropertyProvider.TryGetProperty(string property, Type propertyType, out object value) {
                return _store.TryGetProperty(property, propertyType, out value);
            }

            Type IPropertyProvider.GetPropertyType(string property) {
                return _store.GetPropertyType(property);
            }

            public IEnumerator<KeyValuePair<string, object>> GetEnumerator() {
                return _store.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

            public bool TrySetProperty(string property, object value) {
                return false;
            }

            public override string ToString() {
                return _store.ToString();
            }
        }

        private class NullProperties : IProperties {
            event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged { add {} remove {} }
            void IProperties.ClearProperties() {}
            void IProperties.ClearProperty(string property) {
                PropertyProvider.CheckProperty(property);
            }
            void IProperties.SetProperty(string property, object value) {
                PropertyProvider.CheckProperty(property);
            }

            bool IPropertyProvider.TryGetProperty(string property, Type propertyType, out object value) {
                if (string.IsNullOrEmpty(property)) {
                    throw Failure.NullOrEmptyString(nameof(property));
                }
                value = null;
                return false;
            }
            Type IPropertyProvider.GetPropertyType(string property) {
                PropertyProvider.CheckProperty(property);
                return null;
            }

            public IEnumerator<KeyValuePair<string, object>> GetEnumerator() {
                return ((IEnumerable<KeyValuePair<string, object>>) new KeyValuePair<string, object>[0]).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

            public bool TrySetProperty(string property, object value) {
                PropertyProvider.CheckProperty(property);
                return false;
            }

            public override string ToString() {
                return string.Empty;
            }
        }
    }
}
