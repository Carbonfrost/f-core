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

namespace Carbonfrost.Commons.Core.Runtime {

    class FilterPropertyProvider : IPropertyProvider {

        private readonly Func<string, bool> _predicate;
        private readonly IPropertyProvider _propertyProvider;

        public FilterPropertyProvider(IPropertyProvider propertyProvider, Func<string, bool> predicate) {
            _propertyProvider = propertyProvider;
            _predicate = predicate;
        }

        public Type GetPropertyType(string property) {
            if (FilteredOut(property)) {
                return null;
            }
            return _propertyProvider.GetPropertyType(property);
        }

        public bool TryGetProperty(string property, Type propertyType, out object value) {
            if (FilteredOut(property)) {
                value = null;
                return false;
            }
            return _propertyProvider.TryGetProperty(property, propertyType, out value);
        }

        private bool FilteredOut(string property) {
            return !_predicate(property);
        }
    }
}
