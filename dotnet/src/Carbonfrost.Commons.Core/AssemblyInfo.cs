//
// Copyright 2019, 2020 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using System.Linq;
using System.Reflection;
using System.Xml;
using Carbonfrost.Commons.Core.Runtime;

namespace Carbonfrost.Commons.Core {

    public sealed partial class AssemblyInfo {

        private string[] _nsCache;
        private static readonly IDictionary<Assembly, AssemblyInfo> _map = new Dictionary<Assembly, AssemblyInfo>();
        private readonly Assembly _assembly;
        private readonly IAssemblyInfoXmlNamespaceResolver _resolver;
        private readonly Lazy<IProperties> _metadata;
        private static readonly NamespaceUri ShareNamespace = NamespaceUri.Create(Xmlns.ShareableCodeMetadata2011);

        private readonly SharedRuntimeOptionsAttribute _options;

        public IEnumerable<AssemblyInfo> ReferencedAssemblies {
            get {
                return Assembly.GetReferencedAssemblies().Select(AssemblyInfo.GetAssemblyInfo);
            }
        }

        public IReadOnlyList<Assembly> RelatedAssemblies {
            get {
                return Assembly.GetRelatedAssemblies();
            }
        }

        public IReadOnlyList<string> Namespaces {
            get {
                return AllNamespaces();
            }
        }

        public IReadOnlyCollection<NamespaceUri> XmlNamespaces {
            get {
                return _resolver;
            }
        }

        public IXmlNamespaceResolver XmlNamespaceResolver {
            get {
                return _resolver;
            }
        }

        internal bool ScanForTemplates {
            get { return Scannable && _options.Templates; }
        }

        internal bool ScanForAdapters {
            get { return Scannable && _options.Adapters; }
        }

        internal bool ScanForProviders {
            get { return Scannable && _options.Providers; }
        }

        internal bool Scannable {
            get;
            private set;
        }

        public Assembly Assembly {
            get {
                return _assembly;
            }
        }

        public DateTimeOffset BuildDate {
            get {
                return Metadata.GetDateTimeOffset((ShareNamespace + "buildDate").ToString());
            }
        }

        public IPropertyStore Metadata {
            get {
                return _metadata.Value;
            }
        }

        private AssemblyInfo(Assembly a) {
            _assembly = a;

            var sc = (SharedRuntimeOptionsAttribute) Attribute.GetCustomAttribute(_assembly, typeof(SharedRuntimeOptionsAttribute));
            _options = sc ?? SharedRuntimeOptionsAttribute.Default;
            Scannable = Utility.IsScannableAssembly(a);

            if (Scannable) {
                _resolver = new AssemblyInfoXmlNamespaceResolver(this);
            } else {
                _resolver = AssemblyInfoXmlNamespaceResolver.Null;
            }

            _metadata = new Lazy<IProperties>(() => {
                var props = new Properties();
                var globalScope = Carbonfrost.Commons.Core.XmlNamespaceResolver.Global;

                // We provide "share" by convention, and using our own resolver this should also help
                // avoid probing the full app
                var services = ServiceProvider.FromValue(new XmlNamespaceResolver(new [] { globalScope }) {
                    { "share", Xmlns.ShareableCodeMetadata2011 },
                });
                foreach (AssemblyMetadataAttribute am in Attribute.GetCustomAttributes(_assembly, typeof(AssemblyMetadataAttribute))) {
                    if (string.IsNullOrEmpty(am.Key)) {
                        continue;
                    }
                    if (QualifiedName.TryParse(am.Key, services, out QualifiedName qn)) {
                        props.Add(qn.ToString(), am.Value);
                    }
                }
                return Properties.ReadOnly(props);
            });
        }

        public IEnumerable<string> GetNamespaces(string pattern) {
            return new NamespaceFilter(pattern).Filter(Namespaces);
        }

        public static AssemblyInfo GetAssemblyInfo(AssemblyName assemblyName) {
            if (assemblyName == null) {
                throw new ArgumentNullException("assemblyName");
            }

            return GetAssemblyInfo(Assembly.Load(assemblyName));
        }

        public static AssemblyInfo GetAssemblyInfo(Assembly assembly) {
            if (assembly == null) {
                throw new ArgumentNullException("assembly");
            }

            AssemblyInfo result;
            if (!_map.TryGetValue(assembly, out result)) {
                result = _map[assembly] = new AssemblyInfo(assembly);
            }

            return result;
        }

        public IEnumerable<string> GetClrNamespaces(NamespaceUri namespaceUri) {
            return _resolver.GetClrNamespaces(namespaceUri);
        }

        public string GetClrNamespace(NamespaceUri namespaceUri) {
            if (namespaceUri == null) {
                throw new ArgumentNullException("namespaceUri");
            }

            return GetClrNamespaces(namespaceUri).SingleOrThrow(RuntimeFailure.MultipleNamespaces);
        }

        public NamespaceUri GetXmlNamespace(string clrNamespace) {
            return _resolver.GetXmlNamespace(clrNamespace);
        }

        public string GetXmlNamespacePrefix(NamespaceUri namespaceUri) {
            if (namespaceUri == null) {
                throw new ArgumentNullException("namespaceUri");
            }

            return _resolver.LookupPrefix(namespaceUri);
        }

        private string[] AllNamespaces() {
            if (_nsCache == null) {
                _nsCache = _assembly.GetTypesHelper()
                    .Select(t => t.Namespace)
                    .Where(t => !string.IsNullOrEmpty(t))
                    .Distinct()
                    .ToArray();
                Array.Sort(_nsCache);
            }

            return _nsCache;
        }

        internal IProviderRegistration GetProviderRegistration() {
            if (!ScanForProviders) {
                return ProviderRegistration.None;
            }

            var sc = (ProviderRegistrationAttribute[])
                _assembly.GetCustomAttributes(typeof(ProviderRegistrationAttribute), false);

            var items = sc.Select(t => t.Registration).ToArray();
            if (items.Length == 0) {
                return ProviderRegistration.Default;
            }
            if (items.Length == 1) {
                return items[0];
            }
            return new CompositeProviderRegistration(items);
        }
    }

}
