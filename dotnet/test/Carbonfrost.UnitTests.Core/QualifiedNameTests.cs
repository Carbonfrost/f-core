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
using System.Collections.Generic;
using System.Xml;
using Carbonfrost.Commons.Core;
using Carbonfrost.Commons.Spec;

namespace Carbonfrost.UnitTests.Core {

    public class QualifiedNameTests {

        const string MUSHROOM_KINGDOM = "http://ns.example.com/mushroom-kingdom";

        [Fact]
        public void ToString_should_format() {
            QualifiedName qn = QualifiedName.Parse("{http://ns.example.com/mushroom-kingdom} Mario");
            Assert.Equal("{http://ns.example.com/mushroom-kingdom} Mario", qn.ToString());
            Assert.Equal("{http://ns.example.com/mushroom-kingdom} Mario", qn.ToString("F"));
            Assert.Equal("Mario", qn.ToString("S"));
            Assert.Equal("http://ns.example.com/mushroom-kingdom", qn.ToString("N"));
            Assert.Equal("{http://ns.example.com/mushroom-kingdom}", qn.ToString("m"));
        }

        [Fact]
        public void ToString_should_apply_format_curie() {
            QualifiedName qn = QualifiedName.Create(Xmlns.Core2008, "Glob");
            Assert.Equal("[s:Glob]", qn.ToString("c", new FakeFormatProvider()));
        }

        [Fact]
        public void ToString_should_apply_format_curie_global_registrations() {
            QualifiedName qn = QualifiedName.Create(Xmlns.Core2008, "Glob");
            Assert.Equal("[runtime:Glob]", qn.ToString("c"));
        }

        [Fact]
        public void Parse_should_parse_curie_syntax() {
            var resolver = ServiceProvider.FromValue(new FakeXmlNamespaceResolver());
            QualifiedName qn = QualifiedName.Parse("[example:a]", resolver);
            Assert.Equal("a", qn.LocalName);
            Assert.Equal("http://ns.example.com", qn.NamespaceName);
        }

        [Fact]
        public void TryParse_should_detect_non_registered_prefixes() {
            QualifiedName qn;
            Assert.False(QualifiedName.TryParse("nonexistant:a", out qn));
        }

        [Fact]
        public void Parse_should_parse_default_ns() {
            QualifiedName qn = QualifiedName.Parse("Mario");
            Assert.Equal("Mario", qn.LocalName);
        }

        [Fact]
        public void Parse_should_parse_expanded_names() {
            QualifiedName qn = QualifiedName.Parse("{http://ns.example.com/mushroom-kingdom} Mario");
            Assert.Equal("Mario", qn.LocalName);
            Assert.Equal("http://ns.example.com/mushroom-kingdom", qn.NamespaceName);
        }

        [Fact]
        public void Parse_should_expand_using_prefix_lookup() {
            IDictionary<string, string> lookup = new Dictionary<string, string> {
                { "mk", MUSHROOM_KINGDOM }
            };

            QualifiedName qn = QualifiedName.Parse("mk:Mario", MakeResolver(lookup));
            Assert.Equal(MUSHROOM_KINGDOM, qn.Namespace.NamespaceName);
            Assert.Equal("Mario", qn.LocalName);
        }

        [Fact]
        public void Parse_should_expand_empty_prefix() {
            IDictionary<string, string> lookup = new Dictionary<string, string>();
            lookup.Add("", null);
            QualifiedName qn = QualifiedName.Parse(":Mario", MakeResolver(lookup));

            Assert.Equal(NamespaceUri.Default.NamespaceName, qn.Namespace.NamespaceName);
            Assert.Equal("Mario", qn.LocalName);
        }

        [Fact]
        public void Parse_should_throw_on_missing_prefix() {
            Assert.Throws<ArgumentException>(() => { QualifiedName.Parse("mk:Mario", ServiceProvider.Null); });
        }

        [Fact]
        public void Parse_should_throw_oninvalid_names() {
            Assert.Throws<ArgumentException>(() => { QualifiedName.Parse("*&Ma^^rio"); });
            Assert.Throws<ArgumentException>(() => { QualifiedName.Parse(""); });
            Assert.Throws<ArgumentException>(() => { QualifiedName.Parse("[unclosed:curie"); });
            Assert.Throws<ArgumentException>(() => { QualifiedName.Parse("unclosed:curie]"); });
        }

        [Fact]
        public void Create_local_name_is_required() {
            Assert.Throws<ArgumentException>(() => { QualifiedName.Create(NamespaceUri.Default, ""); });
            Assert.Throws<ArgumentNullException>(() => { QualifiedName.Create(NamespaceUri.Default, null); });
        }

        [Fact]
        public void Equals_operator_should_apply() {
            QualifiedName n = QualifiedName.Create(NamespaceUri.Default, "default");
            QualifiedName m = n;
            Assert.False(n == null);
            Assert.True(n != null);
            Assert.False(null == n);
            Assert.True(null != n);

            Assert.True(m == n);
            Assert.False(m != n);
        }

        [Fact]
        public void Equals_qualified_names_equals_equatable() {
            QualifiedName n = QualifiedName.Create(NamespaceUri.Default, "default");
            Assert.False(n.Equals(null));
            Assert.True(n.Equals(n));
        }

        [Fact]
        public void ChangeNamespace_should_change_nominal() {
            QualifiedName n = QualifiedName.Create(NamespaceUri.Default, "default");
            NamespaceUri nu = NamespaceUri.Create("https://example.com");
            n = n.ChangeNamespace(nu);
            Assert.Same(nu, n.Namespace);
        }

        [Fact]
        public void ChangeLocalName_should_change_nominal() {
            QualifiedName n = QualifiedName.Create(NamespaceUri.Default, "default");
            n = n.ChangeLocalName("name");
            Assert.Same("name", n.LocalName);
        }

        static IServiceProvider MakeResolver(IDictionary<string, string> prefixesToNS) {
            var r = new XmlNamespaceResolver();
            foreach (var kvp in prefixesToNS) {
                if (string.IsNullOrEmpty(kvp.Value)) {
                    r.Add(kvp.Key, null);
                } else {
                    r.Add(kvp.Key, new Uri(kvp.Value));
                }
            }
            return ServiceProvider.FromValue(r);
        }

        class FakeFormatProvider : IFormatProvider {

            public object GetFormat(Type formatType) {
                if (formatType == typeof(IXmlNamespaceResolver)) {
                    return new FakeXmlNamespaceResolver();
                }
                return null;
            }
        }

        class FakeXmlNamespaceResolver : IXmlNamespaceResolver {

            public IDictionary<string, string> GetNamespacesInScope(XmlNamespaceScope scope) {
                return Empty<string, string>.Dictionary;
            }

            public string LookupNamespace(string prefix) {
                if (prefix == "example") {
                    return "http://ns.example.com";
                }
                return null;
            }

            public string LookupPrefix(string namespaceName) {
                if (namespaceName == Xmlns.Core2008) {
                    return "s";
                }
                return null;
            }
        }
    }
}
