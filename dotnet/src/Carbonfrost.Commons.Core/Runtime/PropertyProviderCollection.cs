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

    public class PropertyProviderCollection
        : NamedObjectCollection<IPropertyProvider>, IPropertyProvider {

        public IPropertyProvider AddNew(string name, object value = null, TypeReference type = null) {
            if (value == null && type == null) {
                throw RuntimeFailure.DataProviderTypeOrValueNotBoth();
            }

            if (value != null) {
                return AddOne(name, PropertyProvider.FromValue(value));
            }

            return AddOne(name, PropertyProvider.LateBound(type));
        }

        private IPropertyProvider AddOne(string name, IPropertyProvider item) {
            Add(name, item);
            return item;
        }

        Type IPropertyProvider.GetPropertyType(string property) {
            if (property == null)
                throw new ArgumentNullException("property");
            if (string.IsNullOrEmpty(property))
                throw Failure.EmptyString("property");

            foreach (var p in this) {
                var result = p.GetPropertyType(property);
                if (result != null)
                    return result;
            }

            return null;
        }

        bool IPropertyProvider.TryGetProperty(string property, Type propertyType, out object value) {
            value = null;

            if (property == null)
                throw new ArgumentNullException("property");
            if (string.IsNullOrEmpty(property))
                throw Failure.EmptyString("property");

            foreach (var p in this) {
                bool result =  p.TryGetProperty(property, propertyType, out value);
                if (result)
                    return true;
            }

            return false;
        }

    }

}
