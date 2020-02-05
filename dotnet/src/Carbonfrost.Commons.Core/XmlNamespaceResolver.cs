//
// Copyright 2016, 2017, 2019, 2020 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using System.Collections.ObjectModel;
using System.Xml;
using Carbonfrost.Commons.Core.Runtime;

namespace Carbonfrost.Commons.Core {

    class XmlNamespaceResolver : IXmlNamespaceResolver, IReadOnlyCollection<NamespaceUri> {

        private readonly IDictionary<NamespaceUri, string> _xmlnsPrefixes = new Dictionary<NamespaceUri, string>();
        private readonly IDictionary<string, string> _prefixesToXmlns = new Dictionary<string, string>();
        private readonly IEnumerable<IXmlNamespaceResolver> _mergedScope;

        public static readonly IXmlNamespaceResolver Global = AppLoadedAssembliesXmlResolver.Instance;

        public XmlNamespaceResolver() {}

        public XmlNamespaceResolver(IEnumerable<IXmlNamespaceResolver> ancestorScopes) {
            _mergedScope = new Buffer<IXmlNamespaceResolver>(ancestorScopes);
        }

        public string LookupPrefix(NamespaceUri namespaceUri) {
            return this._xmlnsPrefixes.GetValueOrDefault(namespaceUri, string.Empty);
        }

        // IXmlNamespaceResolver implementation
        public IDictionary<string, string> GetNamespacesInScope(XmlNamespaceScope scope) {
            switch (scope) {
                case XmlNamespaceScope.Local:
                    return new ReadOnlyDictionary<string, string>(_prefixesToXmlns);
                case XmlNamespaceScope.All:
                case XmlNamespaceScope.ExcludeXml:
                    return GetMergedScope();
                default:
                    throw Failure.NotDefinedEnum("scope", scope);
            }
        }

        public string LookupNamespace(string prefix) {
            return _prefixesToXmlns.GetValueOrDefault(prefix);
        }

        public string LookupPrefix(string namespaceName) {
            return LookupPrefix(NamespaceUri.Parse(namespaceName));
        }

        public IEnumerator<NamespaceUri> GetEnumerator() {
            return _xmlnsPrefixes.Keys.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public int Count {
            get {
                return _xmlnsPrefixes.Count;
            }
        }

        internal void Add(string prefix, Uri xmlns) {
            NamespaceUri nu = NamespaceUri.Default;
            if (xmlns != null) {
                nu = NamespaceUri.Create(xmlns);
            }
            prefix = prefix ?? string.Empty;
            if (!_xmlnsPrefixes.ContainsKey(nu)) {
                _xmlnsPrefixes.Add(nu, prefix);
            }
            if (!_prefixesToXmlns.ContainsKey(prefix)) {
                _prefixesToXmlns.Add(prefix, nu.ToString());
            }
        }

        private IDictionary<string, string> GetMergedScope() {
            if (_mergedScope == null) {
                return Empty<string, string>.Dictionary;
            }

            var result = new Dictionary<string, string>();
            foreach (var m in _mergedScope) {
                result.AddMany(m.GetNamespacesInScope(XmlNamespaceScope.Local));
            }

            result.AddMany(_prefixesToXmlns);

            return result;
        }
    }
}
