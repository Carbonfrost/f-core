//
// Copyright 2013, 2016 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using System.Linq;
using System.Reflection;
using Carbonfrost.Commons.Core;
using Carbonfrost.Commons.Core.Runtime;
using Carbonfrost.Commons.Spec;

[assembly: AssemblyMetadata("[shareable:BuildDate]", "1970-01-01T00:00 +0800")]
[assembly: AssemblyMetadata("BuildLabel", "abc")]
[assembly: AssemblyMetadata("", "abc")] // invalid - empty string

namespace Carbonfrost.UnitTests.Core {

    public class AssemblyInfoTests {

        [Fact]
        public void GetXmlNamespace_should_get_xmlns_from_clrnamespaces() {
            AssemblyInfo ai = AssemblyInfo.GetAssemblyInfo(typeof(TypeReference).GetTypeInfo().Assembly);
            Assert.Equal(NamespaceUri.Parse(Xmlns.Core2008), ai.GetXmlNamespace("Carbonfrost.Commons.Core"));
            Assert.Equal(NamespaceUri.Parse(Xmlns.Core2008), ai.GetXmlNamespace("Carbonfrost.Commons.Core.Runtime"));
        }

        [Fact]
        public void GetXmlNamespaces_should_get_clr_namespaces_from_xmlns() {
            AssemblyInfo ai = AssemblyInfo.GetAssemblyInfo(typeof(TypeReference).GetTypeInfo().Assembly);
            var all = ai.GetClrNamespaces(NamespaceUri.Parse(Xmlns.Core2008));
            Assert.Contains("Carbonfrost.Commons.Core", all);
            Assert.Contains("Carbonfrost.Commons.Core.Runtime", all);

            Assert.DoesNotContain("Carbonfrost.Commons.ComponentModel.Annotations", all);
        }

        [Fact]
        public void Namespaces_should_get_known_namespaces() {
            Assembly a = typeof(TypeReference).GetTypeInfo().Assembly;
            var info = AssemblyInfo.GetAssemblyInfo(a);

            // Notice that default ns isn't included
            // Exclude System (and coverlet when coverage is running)
            Assert.SetEqual(new [] {
                            "Carbonfrost.Commons.Core.Resources",
                            "Carbonfrost.Commons.Core",
                            "Carbonfrost.Commons.Core.Runtime",
                        }, info.Namespaces.Except(new[] { "System", "Coverlet.Core.Instrumentation.Tracker" }));
        }

        [Fact]
        public void XmlNamespaces_should_get_known_namespaces() {
            Assembly a = typeof(TypeReference).GetTypeInfo().Assembly;
            var info = AssemblyInfo.GetAssemblyInfo(a);

            Assert.Equal(new [] {
                            NamespaceUri.Parse(Xmlns.Core2008)
                        }, info.XmlNamespaces.ToArray());
        }
    }
}
