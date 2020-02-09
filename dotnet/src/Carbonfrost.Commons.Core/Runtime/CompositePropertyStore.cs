//
// Copyright 2016, 2019-2020 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using System.Linq;

namespace Carbonfrost.Commons.Core.Runtime {

    sealed class CompositePropertyStore : DisposableObject, IPropertyStore {

        private readonly IReadOnlyCollection<IPropertyStore> _items;

        public event PropertyChangedEventHandler PropertyChanged;

        public CompositePropertyStore(IReadOnlyCollection<IPropertyStore> myItems) {
            _items = myItems;
            foreach (var item in _items) {
                item.PropertyChanged += ps_PropertyChanged;
            }
        }

        public Type GetPropertyType(string property) {
            if (TryGetProperty(property, typeof(object), out var result)) {
                return result == null ? typeof(object) : result.GetType();
            }
            return null;
        }

        public bool TryGetProperty(string property, Type propertyType, out object value) {
            PropertyProvider.CheckProperty(property);
            value = null;
            foreach (var pp in _items) {
                if (pp.TryGetProperty(property, propertyType, out value)) {
                    return true;
                }
            }
            return false;
        }

        private void OnPropertyChanged(PropertyChangedEventArgs e) {
            var handler = PropertyChanged;
            if (handler != null) {
                handler(this, e);
            }
        }

        private void ps_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            OnPropertyChanged(e);
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() {
            return _items.SelectMany(t => t).Distinct(new UniqueKeys()).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        protected override void Dispose(bool manualDispose) {
            if (manualDispose) {
                foreach (var item in _items) {
                    item.PropertyChanged -= ps_PropertyChanged;
                }
            }
        }

        sealed class UniqueKeys : IEqualityComparer<KeyValuePair<string, object>> {
            public bool Equals(KeyValuePair<string, object> x, KeyValuePair<string, object> y) {
                return x.Key == y.Key;
            }

            public int GetHashCode(KeyValuePair<string, object> obj) {
                return obj.Key.GetHashCode();
            }
        }
    }
}
