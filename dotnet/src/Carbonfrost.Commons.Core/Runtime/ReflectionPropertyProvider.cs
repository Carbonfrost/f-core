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
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Carbonfrost.Commons.Core.Runtime {

    internal class ReflectionPropertyProvider : IPropertyProvider {

        internal readonly TryCoerceValueCallback DEFAULT_COERCION = StaticDefaultCoercion;

        internal delegate bool TryCoerceValueCallback(
            PropertyInfo property, object value, Type requiredType, out object result
        );

        protected readonly object ObjectContext;

        public ReflectionPropertyProvider(object component) {
            Debug.Assert(component != null);
            ObjectContext = component;
        }

        public override string ToString() {
            return ObjectContext.ToString();
        }

        public Type GetPropertyType(string property) {
            PropertyProvider.CheckProperty(property);

            PropertyInfo descriptor = _GetProperty(property);
            if (descriptor == null) {
                return null;
            }

            else return descriptor.PropertyType;
        }

        public bool TryGetProperty(string property, Type requiredType, out object value) {
            PropertyProvider.CheckProperty(property);

            PropertyInfo pd = _GetProperty(property);
            requiredType = requiredType ?? typeof(object);
            value = null;

            if (pd != null && TryGetValue(pd, out object tempValue)) {
                if (tempValue == null) {
                    return true;
                }
                return TryCoerceValue(pd, tempValue, requiredType, out value)
                    && requiredType.IsInstanceOfType(value);
            }
            return false;
        }

        internal bool TryGetValue(PropertyInfo property, out object value) {
            value = property.GetValue(ObjectContext);
            return true;
        }

        internal bool TryCoerceValue(PropertyInfo property, object value, Type requiredType, out object result) {
            if (requiredType.IsAssignableFrom(property.PropertyType)) {
                result = value;
                return true;
            }

            return StaticDefaultCoercion(property, value, requiredType, out result);
        }

        internal PropertyInfo _GetProperty(string property) {
            return _EnsureProperties().GetValueOrDefault(property);
        }

        protected IDictionary<string, PropertyInfo> _EnsureProperties() {
             return Template.GetPropertyCache(ObjectContext);
        }

        internal static PropertyInfo FindIndexerProperty(Type type) {
            foreach (PropertyInfo pi in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)) {
                var parameters = pi.GetIndexParameters();
                if (parameters.Length == 1 && parameters[0].ParameterType == typeof(string))
                    return pi;
            }
            return null;
        }

        private static bool StaticDefaultCoercion(PropertyInfo property, object value, Type requiredType, out object result) {
            if (value is string str) {
                try {
                    result = Activation.FromText(requiredType, str, null, null);
                    return true;
                } catch {
                    // Type conversion problem
                }
            }
            result = null;
            return false;
        }
    }
}
