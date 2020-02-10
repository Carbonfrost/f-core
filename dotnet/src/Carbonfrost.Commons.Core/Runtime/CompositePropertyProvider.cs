//
// Copyright 2020 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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

namespace Carbonfrost.Commons.Core.Runtime {

    sealed class CompositePropertyProvider : IPropertyProvider {

        private readonly IReadOnlyCollection<IPropertyProvider> _items;

        public CompositePropertyProvider(IReadOnlyCollection<IPropertyProvider> items) {
            _items = items;
        }

        public Type GetPropertyType(string property) {
            return PropertyProvider.InferPropertyType(this, property);
        }

        public bool TryGetProperty(string property, Type propertyType, out object value) {
            if (string.IsNullOrEmpty(property)) {
                throw Failure.NullOrEmptyString(nameof(property));
            }
            value = null;

            foreach (var pp in _items) {
                if (pp.TryGetProperty(property, propertyType, out value)) {
                    return true;
                }
            }
            return false;
        }
    }
}
