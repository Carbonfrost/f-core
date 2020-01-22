//
// Copyright 2013, 2019 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using System.Reflection;
using System.Xml;

using Carbonfrost.Commons.Core;
using Carbonfrost.Commons.Core.Runtime;
using Carbonfrost.Commons.Spec;

namespace Carbonfrost.UnitTests.Core.Runtime {

    public class TypeReferenceTests {

        [Fact]
        public void Resolve_simple_name_resolves() {
            TypeReference tr = TypeReference.Parse("String");
            Assert.Equal(typeof(string), tr.Resolve());
        }

        [Fact]
        public void Resolve_builtin_identifiers_resolve() {
            TypeReference tr = TypeReference.Parse("uint");
            Assert.Equal(typeof(UInt32), tr.Resolve());

            tr = TypeReference.Parse("long");
            Assert.Equal(typeof(long), tr.Resolve());

            tr = TypeReference.Parse("decimal");
            Assert.Equal(typeof(decimal), tr.Resolve());
        }

        [Fact]
        public void Resolve_resolves_type_FromType_nominal() {
            TypeReference tr = TypeReference.FromType(typeof(int));
            Assert.Equal(typeof(int), tr.Resolve());
        }

        [Fact]
        public void Resolve_name_resolves_without_assembly() {
            TypeReference tr = TypeReference.Parse("System.Uri");
            Assert.Equal(typeof(Uri), tr.Resolve());
        }

        [Fact]
        public void Resolve_qualified_name_resolves() {
            string fullName = string.Format("{{{0}}} TypeReference", Xmlns.Core2008);

            TypeReference tr = TypeReference.Parse(fullName);
            Assert.Equal(typeof(TypeReference), tr.Resolve());
        }

        [Theory]
        [InlineData("http")]
        [InlineData("https")]
        public void Resolve_qualified_name_equivalent_https(string scheme) {
            string fullName = "{" + scheme + "://ns.carbonfrost.com/commons/core} Glob";

            TypeReference tr = TypeReference.Parse(fullName);
            Assert.Equal(typeof(Glob), tr.Resolve());
        }

        [Fact]
        public void Resolve_qualified_name_using_prefix_resolves() {
            ServiceContainer services = new ServiceContainer();
            XmlNameTable table = new NameTable();
            XmlNamespaceManager mgr = new XmlNamespaceManager(table);
            services.AddService(typeof(IXmlNamespaceResolver), mgr);
            mgr.AddNamespace("f", Xmlns.Core2008);

            TypeReference tr = TypeReference.Parse("f:TypeReference", services);
            Assert.Equal(typeof(TypeReference), tr.Resolve());
        }

        [Fact]
        public void Resolve_default_qualified_name_using_prefix_resolves() {
            ServiceContainer services = new ServiceContainer();
            XmlNameTable table = new NameTable();
            XmlNamespaceManager mgr = new XmlNamespaceManager(table);
            services.AddService(typeof(IXmlNamespaceResolver), mgr);
            mgr.AddNamespace("", Xmlns.Core2008);

            TypeReference tr = TypeReference.Parse(":TypeReference", services);
            Assert.Equal(typeof(TypeReference), tr.Resolve());
        }

        [Fact]
        public void Resolve_missing_type_should_throw() {
            TypeReference tr = TypeReference.Parse("System.Glob");
            Assert.Throws(typeof(InvalidOperationException), () => tr.Resolve());
        }

        [Fact]
        public void Resolve_missing_type_should_return_null() {
            TypeReference tr = TypeReference.Parse("System.Glob");
            Assert.Null(tr.TryResolve());
        }

        [Fact]
        public void Parse_assembly_qualified_name_should_get_original_name_and_string() {
            string fullName = "Carbonfrost.Commons.Core.Runtime.TypeReference, Carbonfrost.Commons.Core, PublicKeyToken=d09aaf34527fe3e6";

            TypeReference tr = TypeReference.Parse(fullName);
            Assert.Equal(tr.OriginalString, tr.OriginalString);
            Assert.Matches(@"^Carbonfrost\.Commons\.Core\.Runtime\.TypeReference,", tr.ToString());
            Assert.Contains("Carbonfrost.Commons.Core,", tr.ToString());
            Assert.Contains("PublicKeyToken=d09aaf34527fe3e6", tr.ToString());
        }

        [Fact]
        public void Resolve_assembly_qualified_name() {
            string fullName = "Carbonfrost.Commons.Core.Runtime.TypeReference, Carbonfrost.Commons.Core, PublicKeyToken=d09aaf34527fe3e6";

            TypeReference tr = TypeReference.Parse(fullName);
            Assert.Equal(typeof(TypeReference), tr.Resolve());
        }

        [Theory]
        [InlineData("Carbonfrost.UnitTests.Core, PublicKeyToken=d09aaf34527fe3e6")]
        [InlineData("Carbonfrost.UnitTests.Core")]
        public void Resolve_assembly_qualified_name_dynamically_loaded(string asmName) {
            string fullName = string.Format("Carbonfrost.UnitTests.Core.Runtime.TypeReferenceTests, {0}",
                asmName);

            TypeReference tr = TypeReference.Parse(fullName);
            Assert.Equal(typeof(TypeReferenceTests), tr.Resolve());
        }
    }
}
