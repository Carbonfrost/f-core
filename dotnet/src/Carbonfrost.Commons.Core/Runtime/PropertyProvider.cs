//
// Copyright 2005, 2006, 2010, 2016, 2019-2020 Carbonfrost Systems, Inc.
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
using System.Reflection;

namespace Carbonfrost.Commons.Core.Runtime {

    public abstract partial class PropertyProvider : IPropertyProvider {

        private readonly IPropertyProvider _reflection;

        protected PropertyProvider() {
            _reflection = PropertyProvider.FromValue(this);
        }

        protected virtual bool TryGetPropertyCore(
            string property, Type requiredType, out object value) {
            requiredType = requiredType ?? typeof(object);

            PropertyInfo pd = _GetProperty(property);
            if (pd == null || !requiredType.IsAssignableFrom(pd.PropertyType)) {
                value = null;
                return false;
            }

            value = pd.GetValue(this);
            return requiredType.IsInstanceOfType(value);
        }

        public virtual Type GetPropertyType(string property) {
            CheckProperty(property);

            object objValue;
            if (TryGetProperty(property, typeof(object), out objValue)) {
                return objValue == null ? typeof(object) : objValue.GetType();
            }

            throw RuntimeFailure.PropertyNotFound("property", property);
        }

        public bool TryGetProperty(string property, Type propertyType, out object value) {
            CheckProperty(property);
            object objValue;

            if (TryGetPropertyCore(property, propertyType, out objValue)) {
                value = objValue;
                return true;
            }

            value = null;
            return false;
        }

        private PropertyInfo _GetProperty(string property) {
            var result = Template.GetPropertyCache(this).GetValueOrDefault(property);
            if (result == null) {
                return null;
            }
            if (result.DeclaringType == typeof(PropertyStore) || result.DeclaringType == typeof(Properties)) {
                return null;
            }

            return result;
        }

        internal static void CheckProperty(string property) {
            if (string.IsNullOrEmpty(property)) {
                throw Failure.NullOrEmptyString(nameof(property));
            }
        }

        internal static Type InferPropertyType(IPropertyProvider self, string property) {
            if (self.TryGetProperty(property, typeof(object), out object value)) {
                return value == null ? typeof(object) : value.GetType();
            }
            return null;
        }
    }
}
