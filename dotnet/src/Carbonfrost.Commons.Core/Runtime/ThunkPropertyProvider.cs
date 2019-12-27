//
// Copyright 2016, 2019 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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

    class ThunkPropertyProvider : IPropertyProvider {

        private readonly Func<IPropertyProvider> _valueFactory;

        public ThunkPropertyProvider(Func<IPropertyProvider> valueFactory) {
            _valueFactory = valueFactory;
        }

        public Type GetPropertyType(string property) {
            return _valueFactory().GetPropertyType(property);
        }

        public bool TryGetProperty(string property, Type propertyType, out object value) {
            return _valueFactory().TryGetProperty(property, propertyType, out value);
        }
    }
}
