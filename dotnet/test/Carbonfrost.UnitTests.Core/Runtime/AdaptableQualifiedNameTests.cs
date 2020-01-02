//
// Copyright 2013 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using Carbonfrost.Commons.Core;
using Carbonfrost.Commons.Core.Runtime;
using Carbonfrost.Commons.Spec;

namespace Carbonfrost.UnitTests.Core.Runtime {

    public class AdaptableQualifiedNameTests {

        [Fact]
        public void GetTypeByQualifiedName_get_type_from_qualified_name_nominal() {
            string fullName = string.Format("{{{0}}} TypeReference", Xmlns.Core2008);
            Assert.Equal(typeof(TypeReference), App.GetTypeByQualifiedName(QualifiedName.Parse(fullName)));

            TypeReference tr = TypeReference.Parse(fullName);
            Assert.Equal(typeof(TypeReference), tr.Resolve());
        }

        [Fact]
        public void GetQualifiedName_get_type_from_qualified_name_nested_types() {
            var nested = typeof(TypeReference).GetNestedType("TrivialResolver", BindingFlags.NonPublic);

            string fullName = string.Format("{{{0}}} TypeReference.TrivialResolver", Xmlns.Core2008);
            Assert.Equal(fullName, nested.GetQualifiedName().ToString());
            Assert.Equal(nested, App.GetTypeByQualifiedName(QualifiedName.Parse(fullName)));
        }

        [Fact]
        public void GetTypeByQualifiedName_get_type_from_qualified_name_open_generic_type() {
            var open = typeof(AdapterFactory<>);

            string fullName = string.Format("{{{0}}} AdapterFactory-1", Xmlns.Core2008);
            Assert.Equal(fullName, open.GetQualifiedName().ToString());
            Assert.Equal(open, App.GetTypeByQualifiedName(QualifiedName.Parse(fullName)));
        }

        [Fact]
        public void GetQualifiedName_from_closed_generic_type_throws() {
            var closed = typeof(Template<BindingFlags>);

            Assert.Throws<ArgumentException>(() => { closed.GetQualifiedName().ToString(); });
        }
    }
}
