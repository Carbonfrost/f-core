//
// Copyright 2014 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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

namespace Carbonfrost.Commons.Core.Runtime {

    sealed class LateBoundPropertyProvider : IPropertyProvider {

        private readonly Lazy<IPropertyProvider> _item;

        public LateBoundPropertyProvider(TypeReference type) {
            _item = new Lazy<IPropertyProvider>(
                () => PropertyProvider.FromValue(Activation.CreateInstance(type.Resolve()))
            );
        }

        public Type GetPropertyType(string property) {
            return _item.Value.GetPropertyType(property);
        }

        public bool TryGetProperty(string property, Type propertyType, out object value) {
            return _item.Value.TryGetProperty(property, propertyType, out value);
        }
    }
}
