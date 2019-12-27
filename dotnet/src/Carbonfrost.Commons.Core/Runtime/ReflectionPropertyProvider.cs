//
// Copyright 2013, 2019 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using System.Reflection;

namespace Carbonfrost.Commons.Core.Runtime {

    internal class ReflectionPropertyProvider : IPropertyProvider {

        protected readonly object ObjectContext;

        public ReflectionPropertyProvider(object component) {
            if (component == null) {
                throw new ArgumentNullException("component");
            }

            ObjectContext = component;
        }

        public override string ToString() {
            return ObjectContext.ToString();
        }

        public Type GetPropertyType(string property) {
            PropertyInfo descriptor = _GetProperty(property);
            if (descriptor == null) {
                return null;
            }

            else return descriptor.PropertyType;
        }

        public bool TryGetProperty(string property, Type propertyType, out object value) {
            PropertyInfo pd = _GetProperty(property);
            if (pd == null) {
                value = null;
                return false;
            }

            value = pd.GetValue(ObjectContext);
            return value == null || propertyType.IsInstanceOfType(value);
        }

        protected PropertyInfo _GetProperty(string property) {
            return _EnsureProperties().GetValueOrDefault(property);
        }

        protected IDictionary<string, PropertyInfo> _EnsureProperties() {
             return Template.GetPropertyCache(ObjectContext);
        }
    }
}
