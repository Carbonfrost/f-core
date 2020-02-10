//
// Copyright 2020 Carbonfrost Systems, Inc. (https://carbonfrost.com)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     https://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//


using System;
using System.Collections.Generic;
using System.Linq;

namespace Carbonfrost.Commons.Core.Runtime {

    sealed class NamespacePropertyProvider : IPropertyProvider {

        private readonly IDictionary<string, IPropertyProvider> _items;

        public NamespacePropertyProvider(IEnumerable<KeyValuePair<string, IPropertyProvider>> elements) {
            _items = elements.GroupBy(l => l.Key, l => l.Value).ToDictionary(
                grouping => grouping.Key,
                grouping => PropertyProvider.Compose(grouping)
            );
        }

        public Type GetPropertyType(string property) {
            return PropertyProvider.InferPropertyType(this, property);
        }

        public bool TryGetProperty(string property, Type propertyType, out object value) {
            PropertyProvider.CheckProperty(property);

            value = null;
            if (!property.Contains(':')) {
                return false;
            }

            string[] items = property.Split(new []{':'}, 2);
            string prefix = items[0];
            string myProp = items[1];
            var pp = _items.GetValueOrDefault(prefix) ?? PropertyProvider.Null;
            return pp.TryGetProperty(myProp, propertyType, out value);
        }
    }
}
