//
// Copyright 2005, 2006, 2010, 2019 Carbonfrost Systems, Inc.
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
using System.Linq;

namespace Carbonfrost.Commons.Core.Runtime {

    sealed class NamespacePropertyProvider : PropertyProviderBase {

        private readonly IDictionary<string, IPropertyProvider> items
            = new Dictionary<string, IPropertyProvider>();

        public NamespacePropertyProvider(
            IEnumerable<KeyValuePair<string, IPropertyProvider>> elements) {

            foreach (var grouping in elements.GroupBy(l => l.Key, l => l.Value)) {
                items.Add(grouping.Key, (IPropertyProvider) PropertyProvider.Compose(grouping));
            }
        }

        protected override bool TryGetPropertyCore(string property, Type propertyType, out object value) {
            if (property == null) {
                throw new ArgumentNullException("property");
            }
            if (property.Length == 0) {
                throw Failure.EmptyString("property");
            }
            string internalKey;
            var result = GetCandidates(property, out internalKey);

            value = null;
            if (result == null) {
                return false;
            }

            return result.TryGetProperty(internalKey, propertyType, out value);
        }

        IPropertyProvider GetCandidates(string key, out string internalKey) {
            // Use the prefix and index method to pick the category
            string[] items = key.Split(new []{':'}, 2);
            string prefix = string.Empty;
            internalKey = string.Empty;

            if (items.Length == 1) {
                prefix = items[0];
            } else {
                internalKey = items[1];
            }

            // Look up the composite provider using the key
            IPropertyProvider pp;
            if (this.items.TryGetValue(prefix, out pp)) {
                return pp;
            }

            return null;
        }
    }
}
