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
using System.Reflection;

namespace Carbonfrost.Commons.Core.Runtime {

    public abstract partial class PropertyProvider : IPropertyProvider {

        private readonly ReflectionPropertyProvider _reflection;

        protected PropertyProvider() {
            _reflection = new ReflectionPropertyProvider(this);
        }

        protected virtual bool TryGetPropertyCore(
            string property, Type requiredType, out object value) {

            PropertyInfo pd = _GetProperty(property);
            requiredType = requiredType ?? typeof(object);
            value = null;

            return pd != null
                && TryGetValue(pd, out object tempValue)
                && TryCoerceValue(pd, tempValue, requiredType, out value)
                && requiredType.IsInstanceOfType(value);
        }

        protected virtual bool TryGetValue(PropertyInfo property, out object value) {
            if (property == null) {
                throw new ArgumentNullException(nameof(property));
            }
            return _reflection.TryGetValue(property, out value);
        }

        protected virtual bool TryCoerceValue(
            PropertyInfo property, object value, Type requiredType, out object result) {
            if (property == null) {
                throw new ArgumentNullException(nameof(property));
            }
            return _reflection.TryCoerceValue(property, value, requiredType, out result);
        }

        public virtual Type GetPropertyType(string property) {
            CheckProperty(property);
            var result = InferPropertyType(this, property);
            if (result != null) {
                return result;
            }
            return null;
        }

        public bool TryGetProperty(string property, Type propertyType, out object value) {
            CheckProperty(property);
            return TryGetPropertyCore(property, propertyType, out value);
        }

        private PropertyInfo _GetProperty(string property) {
            var result = _reflection._GetProperty(property);
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
