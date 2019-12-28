//
// Copyright 2016 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using System.Xml;
using Carbonfrost.Commons.Core;
using Carbonfrost.Commons.Spec;

namespace Carbonfrost.UnitTests.Core {

    public class XmlNamespaceResolverTests {

        [Fact]
        public void GetNamespacesInScope_should_contain_all_xmlns() {
            var xr = new XmlNamespaceResolver {
                { "runtime", new Uri(Xmlns.Core2008) },
                { "shareable", new Uri(Xmlns.ShareableCodeMetadata2011) },
            };

            var all = xr.GetNamespacesInScope(XmlNamespaceScope.Local);
            Assert.Equal(2, all.Count);
            Assert.Contains("runtime", all.Keys);
            Assert.Equal(Xmlns.Core2008, all["runtime"]);
            Assert.Contains("shareable", all.Keys);
            Assert.Equal(Xmlns.ShareableCodeMetadata2011, all["shareable"]);
        }

        // TODO The resolver should resolve prefixes in the correct order if
        // another assembly defines a new prefix (uncommon)
        [Fact]
        public void LookupNamespace_should_return_prefix_name() {
            var xr = new XmlNamespaceResolver {
                { "runtime", new Uri(Xmlns.Core2008) },
                { "shareable", new Uri(Xmlns.ShareableCodeMetadata2011) },
            };
            var all = xr.LookupNamespace("runtime");
            Assert.Equal(Xmlns.Core2008, all);
        }

        [Fact]
        public void LookupPrefix_should_return_ns_name() {
            var xr = new XmlNamespaceResolver {
                { "runtime", new Uri(Xmlns.Core2008) },
                { "shareable", new Uri(Xmlns.ShareableCodeMetadata2011) },
            };
            var all = xr.LookupPrefix(Xmlns.Core2008);
            Assert.Equal("runtime", all);
        }

        [Fact]
        public void Add_should_allow_and_apply_duplicates() {
            var xr = new XmlNamespaceResolver {
                { "runtime", new Uri(Xmlns.Core2008) },
                { "runtime", new Uri(Xmlns.Core2008 + "S") },
            };

            var all = xr.GetNamespacesInScope(XmlNamespaceScope.Local);
            var prefix = xr.LookupPrefix(Xmlns.Core2008 + "S");
            Assert.Contains("runtime", all.Keys);
            Assert.Equal(1, all.Keys.Count);
            Assert.Equal(Xmlns.Core2008, all["runtime"]);
            Assert.Equal("runtime", prefix);
        }
    }
}


