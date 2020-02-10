//
// Copyright 2014, 2016 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using System.Xml;
using Carbonfrost.Commons.Core.Runtime;

namespace Carbonfrost.Commons.Core {

    class AssemblyInfoXmlNamespaceResolver : IAssemblyInfoXmlNamespaceResolver, IReadOnlyCollection<NamespaceUri> {

        private readonly AssemblyInfo _asm;
        private readonly XmlNamespaceResolver _resolver;
        private readonly SortedList<string, NamespaceUri> xmlns = new SortedList<string, NamespaceUri>();

        public static readonly IAssemblyInfoXmlNamespaceResolver Null = new NullImpl();

        public AssemblyInfoXmlNamespaceResolver(AssemblyInfo asm) {
            _asm = asm;
            _resolver = new XmlNamespaceResolver(_asm.ReferencedAssemblies.Select(t => t.XmlNamespaceResolver));
            foreach (XmlnsAttribute attr in asm.Assembly.GetCustomAttributes(typeof(XmlnsAttribute), false)) {
                AddXmlns(attr.Prefix, attr.Namespace, attr.Xmlns);
            }
            foreach (XmlnsPrefixAttribute attr in asm.Assembly.GetCustomAttributes(typeof(XmlnsPrefixAttribute), false)) {
                _resolver.Add(attr.Prefix, attr.Xmlns);
            }
        }

        public NamespaceUri GetXmlNamespace(string clrNamespace) {
            NamespaceUri ns;
            if (xmlns.TryGetValue(clrNamespace ?? string.Empty, out ns))
                return ns;
            else
                return null;
        }

        public IEnumerable<string> GetClrNamespaces(NamespaceUri namespaceUri) {
            if (namespaceUri == null)
                throw new ArgumentNullException("namespaceUri"); // $NON-NLS-1

            return xmlns.Where(t => namespaceUri.Equals(t.Value)).Select(t => t.Key);
        }

        public string LookupPrefix(NamespaceUri namespaceUri) {
            return _resolver.LookupPrefix(namespaceUri);
        }

        public IDictionary<string, string> GetNamespacesInScope(XmlNamespaceScope scope) {
            return _resolver.GetNamespacesInScope(scope);
        }

        public string LookupNamespace(string prefix) {
            return _resolver.LookupNamespace(prefix);
        }

        public string LookupPrefix(string namespaceName) {
            return LookupPrefix(NamespaceUri.Parse(namespaceName));
        }

        public IEnumerator<NamespaceUri> GetEnumerator() {
            return _resolver.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public int Count {
            get {
                return _resolver.Count;
            }
        }

        private void AddXmlns(string prefix, string clrNamespacePattern, string xmlns) {
            NamespaceUri nu = NamespaceUri.Create(xmlns);

            var allNamespaces = _asm.Namespaces;
            foreach (var m in new NamespaceFilter(clrNamespacePattern).Filter(allNamespaces)) {
                if (!this.xmlns.ContainsKey(m ?? string.Empty)) {
                    this.xmlns.Add(m ?? string.Empty, nu);
                }
            }

            prefix = string.IsNullOrEmpty(prefix) ? null : prefix;
            _resolver.Add(prefix, xmlns);
        }

        class NullImpl : IAssemblyInfoXmlNamespaceResolver {

            public IDictionary<string, string> GetNamespacesInScope(XmlNamespaceScope scope) {
                return Empty<string, string>.Dictionary;
            }

            public NamespaceUri GetXmlNamespace(string clrNamespace) {
                return null;
            }

            public string LookupNamespace(string prefix) {
                return null;
            }

            public string LookupPrefix(string namespaceName) {
                return null;
            }

            public IEnumerator<NamespaceUri> GetEnumerator() {
                yield break;
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
                return GetEnumerator();
            }

            public int Count {
                get {
                    return 0;
                }
            }

            public IEnumerable<string> GetClrNamespaces(NamespaceUri namespaceUri) {
                if (namespaceUri == null) {
                    throw new ArgumentNullException("namespaceUri");
                }
                return Array.Empty<string>();
            }

            public string LookupPrefix(NamespaceUri namespaceUri) {
                if (namespaceUri == null)
                    throw new ArgumentNullException("namespaceUri");
                return null;
            }
        }
    }
}

