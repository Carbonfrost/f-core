//
// Copyright 2013, 2019, 2020 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using System.Linq;
using System.Reflection;

namespace Carbonfrost.Commons.Core.Runtime {

    sealed class ArrayPropertyProvider : IPropertyProvider, IEnumerable<KeyValuePair<string, object>> {

        private readonly object[] _items;

        public ArrayPropertyProvider(object[] items) {
            _items = items;
        }

        public Type GetPropertyType(string property) {
            PropertyProvider.CheckProperty(property);
            object result;
            if (TryGetProperty(property, typeof(object), out result)) {
                return (result == null) ? _items.GetType().GetElementType() : result.GetType();
            }

            return null;
        }

        public bool TryGetProperty(string property, Type propertyType, out object value) {
            PropertyProvider.CheckProperty(property);
            value = null;
            int index;
            if (!int.TryParse(property, out index) || index < 0 || index >= _items.Length) {
                return false;
            }

            value = _items[index];
            return value == null || propertyType.GetTypeInfo().IsInstanceOfType(value);
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() {
            return _items.Select(
                (t, i) => new KeyValuePair<string, object>(i.ToString(), t)
            ).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public override string ToString() {
            return Properties.ToKeyValuePairs(this);
        }
    }
}
