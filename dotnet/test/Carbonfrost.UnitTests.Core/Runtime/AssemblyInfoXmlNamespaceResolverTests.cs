//
// Copyright 2014, 2016, 2020 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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

using System.Reflection;
using System.Xml;
using Carbonfrost.Commons.Core;
using Carbonfrost.Commons.Core.Runtime;
using Carbonfrost.Commons.Spec;

namespace Carbonfrost.UnitTests.Core {

    public class AssemblyInfoXmlNamespaceResolverTests {

        [Fact]
        public void GetNamespacesInScope_should_contain_all_xmlns() {
            var ai = AssemblyInfo.GetAssemblyInfo(typeof(TypeReference).GetTypeInfo().Assembly);
            var xr = (AssemblyInfoXmlNamespaceResolver) ai.XmlNamespaceResolver;

            var all = xr.GetNamespacesInScope(XmlNamespaceScope.Local);
            Assert.Equal(2, all.Count);
            Assert.Contains("runtime", all.Keys);
            Assert.Contains("core", all.Keys);
            Assert.Equal(Xmlns.Core2008, all["runtime"]);
        }

        [Fact]
        public void GetNamespacesInScope_should_contain_all_inherited_xmlns() {
            var ai = AssemblyInfo.GetAssemblyInfo(typeof(ServiceProviderTests).GetTypeInfo().Assembly);
            Assert.True(ai.Scannable);

            var all = ai.XmlNamespaceResolver.GetNamespacesInScope(XmlNamespaceScope.All);
            Assert.Equal(3, all.Count);
            Assert.Contains("test-sr", all.Keys);
            Assert.Contains("runtime", all.Keys);
            Assert.Contains("core", all.Keys);
        }

        // TODO The resolver should resolve prefixes in the correct order if
        // another assembly defines a new prefix (uncommon)

        [Fact]
        public void LookupNamespace_should_return_reflected_prefix_name() {
            var ai = AssemblyInfo.GetAssemblyInfo(typeof(TypeReference).GetTypeInfo().Assembly);
            var xr = ai.XmlNamespaceResolver;

            var all = xr.LookupNamespace("runtime");
            Assert.Equal(Xmlns.Core2008, all);
        }

        [Fact]
        public void LookupPrefix_should_return_reflected_ns_name() {
            var ai = AssemblyInfo.GetAssemblyInfo(typeof(TypeReference).GetTypeInfo().Assembly);
            var xr = (AssemblyInfoXmlNamespaceResolver) ai.XmlNamespaceResolver;

            var all = xr.LookupPrefix(Xmlns.Core2008);
            Assert.Equal("runtime", all);
        }

        [Fact]
        public void LookupPrefix_should_account_for_XmlnsPrefixAttribute() {
            var ai = AssemblyInfo.GetAssemblyInfo(typeof(TypeReference).GetTypeInfo().Assembly);
            var xr = ai.XmlNamespaceResolver;
            var all = xr.LookupNamespace("core");
            Assert.Equal(Xmlns.Core2008, all);
        }

    }
}
