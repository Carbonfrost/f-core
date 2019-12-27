//
// Copyright 2016, 2017, 2019 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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

    class AppLoadedAssembliesXmlResolver : IXmlNamespaceResolver {

        internal static readonly AppLoadedAssembliesXmlResolver Instance = new AppLoadedAssembliesXmlResolver();
        private XmlNamespaceResolver _cache;

        public IDictionary<string, string> GetNamespacesInScope(XmlNamespaceScope scope) {
            throw new NotImplementedException();
        }

        public string LookupNamespace(string prefix) {
            return EnsureCache().LookupNamespace(prefix);
        }

        public string LookupPrefix(string namespaceName) {
            return EnsureCache().LookupPrefix(namespaceName);
        }

        private XmlNamespaceResolver EnsureCache() {
            if (_cache == null) {
                // We have to use a separate cache here because XMLNS lookups could be
                // needed by AssemblyMetadataAttribute-based relationship data.  If we
                // used AssemblyInfo.XmlNamespaceResolver directly, we'd end up in a circular
                // dependency
                _cache = new XmlNamespaceResolver();
                var attrs = App.DescribeAssemblies()
                    .SelectMany(a => a.GetCustomAttributes(typeof(XmlnsAttribute), false))
                    .Cast<XmlnsAttribute>();
                foreach (var attr in attrs) {
                    _cache.Add(attr.Prefix, attr.Xmlns);
                }

            }
            return _cache;
        }
    }
}
