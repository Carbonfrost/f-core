//
// Copyright 2019 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using Carbonfrost.Commons.Core;
using Carbonfrost.Commons.Core.Runtime;

namespace Carbonfrost.Commons.Core {

    public sealed partial class AssemblyInfo {

        private string[] _nsCache;
        private static readonly IDictionary<Assembly, AssemblyInfo> map = new Dictionary<Assembly, AssemblyInfo>();
        private readonly Assembly assembly;
        private readonly IAssemblyInfoXmlNamespaceResolver _resolver;

        private readonly ICustomAttributeProvider attributes;
        private readonly SharedRuntimeOptionsAttribute options;

        internal static readonly IEnumerable<AssemblyName> ALL;

        public IEnumerable<AssemblyInfo> ReferencedAssemblies {
            get {
                return this.Assembly.GetReferencedAssemblies().Select(AssemblyInfo.GetAssemblyInfo);
            }
        }

        public IReadOnlyList<string> ClrNamespaces {
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
            get { return Scannable && options.Templates; }
        }

        internal bool ScanForAdapters {
            get { return Scannable && options.Adapters; }
        }

        internal bool ScanForProviders {
            get { return Scannable && options.Providers; }
        }

        internal bool Scannable {
            get; private set;
        }

        public Assembly Assembly { get { return assembly; } }

        private AssemblyInfo(Assembly a) {
            this.assembly = a;
            this.attributes = assembly;

            var sc = CustomAttributeProvider.GetCustomAttribute<SharedRuntimeOptionsAttribute>(this.attributes, false);
            this.options = sc ?? SharedRuntimeOptionsAttribute.Default;
            this.Scannable = Utility.IsScannableAssembly(a);

            if (this.Scannable) {
                _resolver = new AssemblyInfoXmlNamespaceResolver(this);
            } else {
                _resolver = AssemblyInfoXmlNamespaceResolver.Null;
            }
        }

        static AssemblyInfo() {
            ALL = App.DescribeAssemblies(
                a => new [] { a.GetName() });
        }

        public IEnumerable<string> GetNamespaces(string pattern) {
            return new NamespaceFilter(pattern).Filter(AllNamespaces());
        }

        public static AssemblyInfo GetAssemblyInfo(AssemblyName assemblyName) {
            if (assemblyName == null)
                throw new ArgumentNullException("assemblyName");

            return GetAssemblyInfo(Assembly.Load(assemblyName));
        }

        public static AssemblyInfo GetAssemblyInfo(Assembly assembly) {
            if (assembly == null)
                throw new ArgumentNullException("assembly"); // $NON-NLS-1

            AssemblyInfo result;
            if (!map.TryGetValue(assembly, out result)) {
                result = map[assembly] = new AssemblyInfo(assembly);
            }

            return result;
        }

        public IEnumerable<string> GetClrNamespaces(NamespaceUri namespaceUri) {
            return _resolver.GetClrNamespaces(namespaceUri);
        }

        public string GetClrNamespace(NamespaceUri namespaceUri) {
            if (namespaceUri == null)
                throw new ArgumentNullException("namespaceUri"); // $NON-NLS-1

            return GetClrNamespaces(namespaceUri).SingleOrThrow(RuntimeFailure.MultipleNamespaces);
        }

        public NamespaceUri GetXmlNamespace(string clrNamespace) {
            return _resolver.GetXmlNamespace(clrNamespace);
        }

        public string GetXmlNamespacePrefix(NamespaceUri namespaceUri) {
            if (namespaceUri == null)
                throw new ArgumentNullException("namespaceUri");

            return _resolver.LookupPrefix(namespaceUri);
        }

        private string[] AllNamespaces() {
            if (_nsCache == null) {
                _nsCache = this.assembly.GetTypesHelper()
                    .Select(t => t.Namespace)
                    .Where(t => !string.IsNullOrEmpty(t))
                    .Distinct()
                    .ToArray();
            }

            return _nsCache;
        }

        internal IProviderRegistration GetProviderRegistration() {
            if (!ScanForProviders)
                return ProviderRegistration.None;

            var sc = (ProviderRegistrationAttribute[])
                assembly.GetCustomAttributes(typeof(ProviderRegistrationAttribute), false);

            var items = sc.Select(t => t.Registration).ToArray();
            if (items.Length == 0)
                return ProviderRegistration.Default;
            else if (items.Length == 1)
                return items[0];
            else
                return new CompositeProviderRegistration(items);
        }
    }

}
