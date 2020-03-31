//
// Copyright 2013, 2020 Carbonfrost Systems, Inc. (https://carbonfrost.com)
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

    class TemplateData : ITemplateInfoDescription {

        private readonly BufferDictionary<TemplateKey, ProviderField> _providers;

        // TODO Optimize by memoizing lookups on local names

        internal static readonly TemplateData Instance = new TemplateData();

        private TemplateData() {
            _providers = new BufferDictionary<TemplateKey, ProviderField>(
                App.DescribeAssemblies(t => ExtractFromTypes(t)));
        }

        private IEnumerable<KeyValuePair<TemplateKey, ProviderField>> ExtractFromTypes(Assembly a) {
            if (!AssemblyInfo.GetAssemblyInfo(a).ScanForTemplates)
                yield break;

            foreach (TypeInfo type in a.GetTypesHelper()) {
                if (type.IsDefined(typeof(TemplatesAttribute))) {

                    foreach (FieldInfo field in type.GetFields()) {
                        if (!field.IsStatic) {
                            continue;
                        }

                        if (typeof(ITemplate).IsAssignableFrom(field.FieldType)) {
                            var fieldResult = new ProviderField(field);
                            yield return new KeyValuePair<TemplateKey, ProviderField>(fieldResult.Key, fieldResult);
                        }
                    }
                }
            }
        }

        public IEnumerable<QualifiedName> GetTemplateNames(Type templateType) {
            if (templateType == null) {
                throw new ArgumentNullException(nameof(templateType));
            }

            return _providers.Where(t => t.Key.TemplateType == templateType)
                .Select(t => t.Key.Name);
        }

        public ITemplate GetTemplate(Type templateType, QualifiedName name) {
            if (templateType == null) {
                throw new ArgumentNullException(nameof(templateType));
            }

            ProviderField f = _providers[new TemplateKey(templateType, name)];
            if (f == null) {
                return null;
            }
            return (ITemplate) f.GetValue();
        }

        public IEnumerable<ITemplate> GetTemplates(Type templateType, string localName) {
            if (templateType == null) {
                throw new ArgumentNullException(nameof(templateType));
            }
            return GetTemplatesByLocalName(templateType, localName);
        }

        internal IEnumerable<ITemplate> GetTemplatesByLocalName(Type templateType, string localName) {
            return _providers.Where(t => t.Key.TemplateType == templateType
                                     && t.Key.Name.LocalName == localName)
                             .Select(t => t.Value.GetValue());
        }

        internal class ProviderField {

            private readonly FieldInfo _field;
            private ITemplate _templateCache;

            public ProviderField(FieldInfo field) {
                _field = field;
            }

            internal TemplateKey Key {
                get {
                    return new TemplateKey(TemplateType, QualifiedName);
                }
            }

            public QualifiedName QualifiedName {
                get {
                    return _field.DeclaringType.GetQualifiedName().ChangeLocalName(_field.Name);
                }
            }

            public string Name {
                get {
                    return _field.Name;
                }
            }

            public Type TemplateType {
                get {
                    return _field.FieldType.GetGenericArguments()[0];
                }
            }

            public ITemplate GetValue() {
                if (_templateCache == null) {
                    _templateCache = (ITemplate) Activator.CreateInstance(
                        typeof(ReflectedTemplate), _field.GetValue(null), QualifiedName);
                }
                return _templateCache;
            }
        }

        class ReflectedTemplate : ITemplate {

            private readonly ITemplate _template;
            private readonly QualifiedName _name;

            public ReflectedTemplate(ITemplate value, QualifiedName name) {
                _template = value;
                _name = name;
            }

            void ITemplate.Apply(object value) {
                _template.Apply(value);
            }

            public QualifiedName QualifiedName { get { return _name; } }
        }

        internal struct TemplateKey {
            public readonly Type TemplateType;
            public readonly QualifiedName Name;

            public TemplateKey(Type templateType, QualifiedName name) {
                TemplateType = templateType;
                Name = name;
            }

            public override bool Equals(object obj) {
                return (obj is TemplateKey) && Equals((TemplateKey)obj);
            }

            public bool Equals(TemplateKey other) {
                return TemplateType == other.TemplateType
                    && Name == other.Name;
            }

            public override int GetHashCode() {
                unchecked {
                    return 37 * TemplateType.GetHashCode() + 17 * Name.GetHashCode();
                }
            }
        }
    }
}
