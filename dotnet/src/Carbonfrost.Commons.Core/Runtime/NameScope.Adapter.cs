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
using System.Reflection;

namespace Carbonfrost.Commons.Core.Runtime {

    partial class NameScope {

        sealed class NameScopePropertyProvider : IPropertyProvider {
            private readonly NameScope _nameScope;

            public NameScopePropertyProvider(NameScope nameScope) {
                _nameScope = nameScope;
            }

            public Type GetPropertyType(string property) {
                if (property == null) {
                    throw new ArgumentNullException("property");
                }
                if (string.IsNullOrEmpty(property)) {
                    throw Failure.EmptyString("property");
                }
                var value = _nameScope.FindName(property);
                return value == null ? null : value.GetType();
            }

            public bool TryGetProperty(string property, Type propertyType, out object value) {
                if (property == null) {
                    throw new ArgumentNullException("property");
                }
                if (string.IsNullOrEmpty(property)) {
                    throw Failure.EmptyString("property");
                }

                value = _nameScope.FindName(property);
                return propertyType == null || propertyType.GetTypeInfo().IsInstanceOfType(value);
            }
        }

        private abstract class ReadOnlyAdapterBase : INameScope {
            public abstract object FindName(string name);
            public void RegisterName(string name, object item) {
                if (name == null) {
                    throw new ArgumentNullException("name");
                }
                if (string.IsNullOrEmpty(name)) {
                    throw Failure.EmptyString("name");
                }
                if (item == null) {
                    throw new ArgumentNullException("item");
                }
                throw Failure.ReadOnlyCollection();
            }

            public void UnregisterName(string name) {
                if (name == null) {
                    throw new ArgumentNullException("name");
                }
                if (string.IsNullOrEmpty(name)) {
                    throw Failure.EmptyString("name");
                }
                throw Failure.ReadOnlyCollection();
            }
        }

        private class ReadOnlyAdapter : ReadOnlyAdapterBase {

            private readonly INameScope _other;

            public ReadOnlyAdapter(INameScope other) {
                _other = other;
            }

            public override object FindName(string name) {
                return _other.FindName(name);
            }
        }

        class EmptyImpl : ReadOnlyAdapterBase {
            public override object FindName(string name) {
                return null;
            }
        }
    }
}
