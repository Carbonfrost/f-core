//
// Copyright 2016, 2020 Carbonfrost Systems, Inc. (https://carbonfrost.com)
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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Carbonfrost.Commons.Core.Runtime {

    public class ProviderInfoCollection : ICollection<ProviderInfo> {

        private readonly Dictionary<MemberInfo, ProviderValueSource> _valueSources = new Dictionary<MemberInfo, ProviderValueSource>();
        private readonly List<ProviderInfo> _items = new List<ProviderInfo>();

        public void DefineProvider(QualifiedName name,
                                   Type providerType,
                                   Type providerInstanceType,
                                   object metadata = null) {
            if (providerType == null) {
                throw new ArgumentNullException("providerType");
            }
            if (providerInstanceType == null) {
                throw new ArgumentNullException("providerInstanceType");
            }
            if (providerInstanceType.GetTypeInfo().IsAbstract || !providerType.GetTypeInfo().IsAssignableFrom(providerInstanceType)) {
                throw RuntimeFailure.InvalidProviderInstanceType("providerInstanceType");
            }

            var qn = GetName(name, providerInstanceType, providerInstanceType.Name);

            var tr = new ProviderType(providerInstanceType, providerType, qn);
            tr.ProviderMetadata = ProviderMetadataWrapper.Create(ApplyCompleter(providerInstanceType, providerType, metadata));
            tr.ProviderMetadata.Source = tr;
            AppendResult(tr);
        }

        public void DefineProvider(QualifiedName name,
                                   Type providerType,
                                   FieldInfo field,
                                   object metadata = null) {
            if (providerType == null) {
                throw new ArgumentNullException("providerType");
            }
            if (field == null) {
                throw new ArgumentNullException("field");
            }
            if (!field.IsStatic
                || !providerType.IsAssignableFrom(field.FieldType)) {
                throw RuntimeFailure.InvalidProviderFieldOrMethod("field");
            }

            var qn = GetName(name, field.DeclaringType, field.Name);
            var fieldResult = new ProviderField(field, providerType, qn);
            fieldResult.ProviderMetadata = ProviderMetadataWrapper.Create(metadata);
            fieldResult.ProviderMetadata.Source = fieldResult;
            AppendResult(fieldResult);
        }

        public void DefineProvider(QualifiedName name,
                                   Type providerType,
                                   MethodInfo factoryMethod,
                                   object metadata = null) {
            if (providerType == null) {
                throw new ArgumentNullException("providerType");
            }
            if (factoryMethod == null) {
                throw new ArgumentNullException("factoryMethod");
            }
            if (!factoryMethod.IsStatic
                || !providerType.IsAssignableFrom(factoryMethod.ReturnType)) {
                throw RuntimeFailure.InvalidProviderFieldOrMethod("factoryMethod");
            }

            var qn = GetName(name, factoryMethod.DeclaringType, factoryMethod.Name);
            var methodResult = new ProviderMethod(factoryMethod,
                                                  providerType,
                                                  qn);
            methodResult.ProviderMetadata = ProviderMetadataWrapper.Create(metadata);
            methodResult.ProviderMetadata.Source = methodResult;
            AppendResult(methodResult);
        }

        static QualifiedName GetName(QualifiedName userName,
                                     Type declaringType,
                                     string memberName) {
            return userName
                ?? declaringType.GetQualifiedName()
                .ChangeLocalName(Utility.Camel(memberName));
        }

        static object ApplyCompleter(Type providerInstanceType, Type providerType, object metadata) {
            string expectedName = string.Concat("Complete", providerType.Name, "Provider");
            var completer = providerInstanceType.GetTypeInfo().GetMethods(
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
                .FirstOrDefault(m => IsCompleterMethod(m, expectedName, metadata.GetType()));
            if (completer != null) {
                completer.Invoke(null, new[] { metadata });
            }
            return metadata;
        }

        static bool IsCompleterMethod(MethodInfo method, string expectedName, Type metadata) {
            if (method.Name != expectedName) {
                return false;
            }
            var pms = method.GetParameters();
            return pms.Length == 1 && pms[0].ParameterType == metadata;
        }

        void AppendResult(ProviderValueSource item) {
            lock (_valueSources) {
                ProviderValueSource existing;
                var member = item.Member;
                if (_valueSources.TryGetValue(member, out existing)) {
                    _valueSources[member] = existing.AppendOne(item);
                } else {
                    _valueSources[member] = item;
                }
            }
        }

        public void Add(ProviderInfo item) {
            if (item == null) {
                throw new ArgumentNullException("item");
            }
            _items.Add(item);
        }

        public void Clear() {
            _items.Clear();
            _valueSources.Clear();
        }

        public bool Contains(ProviderInfo item) {
            if (_items.Contains(item)) {
                return true;
            }

            ProviderValueSource vs;
            if (_valueSources.TryGetValue(item.Member, out vs)) {
                return vs == item;
            }
            return false;
        }

        public void CopyTo(ProviderInfo[] array, int arrayIndex) {
            _valueSources.Values.Concat(_items).ToList().CopyTo(array, arrayIndex);
        }

        public bool Remove(ProviderInfo item) {
            throw new NotImplementedException();
        }

        public int Count {
            get {
                return _valueSources.Count + _items.Count;
            }
        }

        public bool IsReadOnly {
            get {
                return false;
            }
        }

        public IEnumerator<ProviderInfo> GetEnumerator() {
            return _valueSources.Values.Concat(_items).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        sealed class ProviderField : ProviderValueSource {

            private readonly FieldInfo field;

            public ProviderField(FieldInfo field,
                                 Type providerType,
                                 QualifiedName key)
                : base(providerType, key) {
                this.field = field;
            }

            public override MemberInfo Member {
                get {
                    return this.field;
                }
            }

            public override Type ValueType {
                get {
                    return GetValue().GetType();
                }
            }

            public override object GetValue() {
                return field.GetValue(null);
            }

            public override bool IsValue(object instance) {
                return field.IsInitOnly && object.ReferenceEquals(instance, GetValue());
            }

            public override object Activate(IEnumerable<KeyValuePair<string, object>> arguments,
                                            IServiceProvider services) {
                object result = GetValue();
                Activation.Initialize(result, arguments, services);
                return result;
            }
        }

        sealed class ProviderMethod : ProviderValueSource {

            private readonly MethodInfo _method;
            private ActivationHelper helper;

            public ProviderMethod(MethodInfo method,
                                  Type providerType,
                                  QualifiedName key)
                : base(providerType, key) {
                _method = method;
            }

            public override MemberInfo Member {
                get {
                    return _method;
                }
            }

            public override Type ValueType {
                get {
                    return _method.ReturnType;
                }
            }

            public override bool IsValue(object instance) {
                // Non-singletons
                return false;
            }

            public override object GetValue() {
                return Activate(null, null);
            }

            public override object Activate(IEnumerable<KeyValuePair<string, object>> arguments,
                                            IServiceProvider services) {
                if (helper == null)
                    helper = new ActivationHelper(_method);

                var instance = helper.CreateInstance(_method.ReturnType, arguments, services);
                return instance;
            }

            sealed class ActivationHelper : DefaultActivationFactory {

                // We have a method, so we can't use pure Activation
                private readonly MethodInfo method;

                public ActivationHelper(MethodInfo method) {
                    this.method = method;
                }

                protected override MethodBase GetActivationConstructor(Type type) {
                    return method;
                }
            }
        }

        sealed class ProviderType : ProviderValueSource {

            private readonly Type type;

            public ProviderType(Type type,
                                Type providerType,
                                QualifiedName key)
                : base(providerType, key) {
                this.type = type;
            }

            public override Type ValueType {
                get {
                    return type;
                }
            }

            public override MemberInfo Member {
                get {
                    return type.GetTypeInfo();
                }
            }

            public override Assembly Assembly {
                get {
                    return type.GetTypeInfo().Assembly;
                }
            }

            public override bool IsValue(object instance) {
                return ValueType == instance.GetType();
            }

            public override object GetValue() {
                return Activation.CreateInstance(type);
            }

            public override object Activate(IEnumerable<KeyValuePair<string, object>> arguments,
                                            IServiceProvider services) {
                return Activation.CreateInstance(type, arguments, services);
            }
        }

    }
}
