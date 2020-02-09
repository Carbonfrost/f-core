//
// Copyright 2013, 2016, 2019-2020 Carbonfrost Systems, Inc. (https://carbonfrost.com)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     https://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using Carbonfrost.Commons.Core.Runtime;
using Carbonfrost.Commons.Spec;

namespace Carbonfrost.UnitTests.Core {

    public class PropertiesTests : TestClass {

        public IEnumerable<IProperties> ReadOnlyPropertiesAdapters {
            get {
                var p = new Properties { { "a", "bc123" } };
                var dict = new Dictionary<string, string> { { "a", "bc123" } };

                yield return Properties.ReadOnly(p);
                yield return Properties.ReadOnly(dict);
            }
        }

        public IEnumerable<IProperties> StrictProperties {
            get {
                return new IProperties[] {
                    new Properties(),
                    Properties.Null,
                    Properties.Empty,
                    Properties.FromArray(3),
                    Properties.FromValue(new { a = "" }),
                    Properties.FromValue(new Dictionary<string,string> { ["a"] = "" }),
                    Properties.FromValue(new NameValueCollection()),
                };
            }
        }

        [Fact]
        public void Parse_key_value_pairs_whitespace_rules() {
            Properties p = Properties.Parse("a=;b;c=");
            Assert.Equal(3, p.InnerMap.Count);

            Assert.Equal("", p.GetProperty("a"));
            Assert.Equal("", p.GetProperty("b"));
            Assert.Equal("", p.GetProperty("c"));
        }

        [Fact]
        public void FromFile_will_read_file() {
            Properties p = Properties.FromFile(TestContext.GetFullPath("alpha.properties"));
            Assert.ContainsKeyWithValue("Bash", "Escape\nTwo lines", p);
        }

        [Fact]
        public void FromStreamContext_will_read_file() {
            Properties p = Properties.FromStreamContext(
                StreamContext.FromFile(TestContext.GetFullPath("alpha.properties"))
            );
            Assert.ContainsKeyWithValue("Bash", "Escape\nTwo lines", p);
        }

        [Fact]
        public void Parse_key_value_pairs() {
            Properties p = Properties.Parse("a=a;b=b;c=true");
            Assert.Equal(3, p.InnerMap.Count);

            Assert.Equal("a", p.GetProperty("a"));
            Assert.Equal("b", p.GetProperty("b"));
            Assert.Equal("true", p.GetProperty("c"));
        }

        [Fact]
        public void Parse_escape_sequences() {
            Properties p = Properties.Parse(@"a=\t\n\r\u27F5;c=\x20\b\f\0");

            Assert.Equal("\t\n\r⟵", p.GetProperty("a"));
            Assert.Equal(" \b\f\0", p.GetProperty("c"));
        }

        [Fact]
        public void FromValue_convert_key_value_pairs() {
            IProperties p = Properties.FromValue(new { a = "a", b = "b", c = true });
            Assert.Equal("a=a;b=b;c=True", p.ToString());
        }

        [Fact]
        public void FromValue_convert_key_value_pairs_escaping() {
            IProperties p = Properties.FromValue(new { a = "a    ", b = "\"quotat ; ions\"", c = "carriage returns\r\n", d = ";;; '' ;;;" });
            Assert.Equal("a='a    ';b='\"quotat ; ions\"';c='carriage returns\r\n';d=';;; \\'\\' ;;;'", p.ToString());
        }

        [Fact]
        public void FromValues_can_handle_any_KeyValuePair_enumerable() {
            var items = new [] {
                new KeyValuePair<string, int>("a", 420),
                new KeyValuePair<string, int>("b", 500),
            };
            var pp = Properties.FromValues(items);
            Assert.Equal(420, pp.GetProperty("a"));
            Assert.IsInstanceOf<IPropertyStore>(pp);
        }

        [Fact]
        public void Parse_convert_key_value_pairs_unescaping() {
            Properties p = Properties.Parse("a='a    ';b='\"quotat ; ions\"';c='carriage returns\r\n';d=';;; \\'\\' ;;;'");
            Assert.Equal("a    ", p.GetProperty("a"));
            Assert.Equal("\"quotat ; ions\"", p.GetProperty("b"));
            Assert.Equal("carriage returns\r\n", p.GetProperty("c"));
            Assert.Equal(";;; '' ;;;", p.GetProperty("d"));
        }

        [Fact]
        public void ToString_conversion_nominal() {
            IProperties p = Properties.FromValue(new {
                                                     a = 420, b = "cool", c = false
                                                 });
            Assert.Equal("a=420;b=cool;c=False", p.ToString());
        }

        [Fact]
        public void StreamingSource_Create_should_provide_correct_streaming_source_adapter() {
            Assert.IsInstanceOf<PropertiesStreamingSource>(StreamingSource.Create(typeof(Properties)));
        }

        [Fact]
        public void FromValue_should_return_null_instance() {
            Assert.Same(Properties.Null, Properties.FromValue(null));
        }

        [Theory]
        [PropertyData(nameof(ReadOnlyPropertiesAdapters))]
        public void ReadOnly_should_generate_InvalidOperationException_on_edits(IProperties ro) {
            Assert.Throws<InvalidOperationException>(() => ro.SetProperty("a", "x"));
            Assert.Throws<InvalidOperationException>(() => ro.SetProperty("v", "d"));
        }

        [Theory]
        [PropertyData(nameof(ReadOnlyPropertiesAdapters))]
        public void ReadOnly_should_not_generate_InvalidOperationException_on_TrySetProperty(IProperties ro) {
            Assert.DoesNotThrow(() => ro.TrySetProperty("v", "d"));
        }

        [Theory]
        [PropertyData(nameof(ReadOnlyPropertiesAdapters))]
        public void ReadOnly_should_generate_InvalidOperationException_on_clears(IProperties ro) {
            Assert.Throws<InvalidOperationException>(() => ro.ClearProperty("a"));
            Assert.Throws<InvalidOperationException>(() => ro.ClearProperties());
        }

        [Theory]
        [PropertyData(nameof(ReadOnlyPropertiesAdapters))]
        public void ReadOnlyAdapters_should_apply_GetProperty(IProperties p) {
            Assert.Equal("bc123", p.GetProperty("a"));
            Assert.Equal(typeof(string), p.GetPropertyType("a"));
        }

        [Theory]
        [PropertyData(nameof(ReadOnlyPropertiesAdapters))]
        public void ReadOnlyAdapters_should_have_enumerator(IProperties p) {
            Assert.ContainsKeyWithValue("a", "bc123", p);

            foreach (KeyValuePair<string, object> kvp in (System.Collections.IEnumerable) p) {
                Assert.Equal(new KeyValuePair<string, object>("a", "bc123"), kvp);
            }
        }

        [Fact]
        public void TrySetProperty_should_return_false_on_read_only() {
            var p = new Properties { { "a", "bc123" } };
            var ro = Properties.ReadOnly(p);
            Assert.False(ro.TrySetProperty("a", "x"));
        }

        [Fact]
        public void ReadOnly_should_generate_null_instances() {
            Assert.Same(Properties.Null, Properties.ReadOnly(Properties.Null));
            Assert.Same(Properties.Null, Properties.ReadOnly((IPropertyStore) null));
            Assert.Same(Properties.Null, Properties.ReadOnly((IReadOnlyDictionary<string, object>) null));
        }

        [Fact]
        public void RaiseEvents_and_other_properties_should_not_be_a_reflected_property() {
            var pp = new Properties();
            const BindingFlags flags = BindingFlags.Static
                | BindingFlags.Instance
                | BindingFlags.Public
                | BindingFlags.NonPublic;

            Assert.False(pp.HasProperty("RaiseEvents"));
            foreach (var prop in pp.GetType().GetProperties(flags)) {
                Assert.False(pp.HasProperty(prop.Name), "Unexpected property: " + prop.Name);
            }
            Assert.Equal(0, pp.Count);
        }

        [Fact]
        public void GetProperty_should_apply_text_conversion_using_parse() {
            var pp = new Properties { { "A", "420" }};
            Assert.Equal(420, pp.GetProperty<int>("A"));
        }

        [Fact]
        public void GetProperty_should_be_case_insensitive() {
            var pp = new Properties { { "A", "420" } };
            Assert.Equal("420", pp.GetProperty<string>("a"));
        }

        public class MyProperties : Properties {
            public string A { get; set; }
        }

        [Fact]
        public void Properties_derived_classes_should_expose_reflected_properties() {
            var pp = new MyProperties() { A = "a" };
            Assert.True(pp.HasProperty("A"));
            Assert.Equal("a", pp.GetProperty("A"));
        }

        [Fact]
        public void Properties_is_the_concrete_class_for_IProperties() {
            Assert.Equal(typeof(Properties), typeof(IProperties).GetConcreteClass());
        }

        [Theory]
        [PropertyData(nameof(StrictProperties))]
        public void GetProperty_will_throw_on_invalid_property_name(IProperties pp) {
            Assert.Throws<ArgumentException>(() => pp.GetProperty(""));
            Assert.Throws<ArgumentException>(() => pp.GetProperty(null));
            Assert.Throws<ArgumentException>(() => pp.GetPropertyType(""));
            Assert.Throws<ArgumentException>(() => pp.GetPropertyType(null));

            Assert.Throws<ArgumentException>(() => pp.TryGetProperty("", typeof(object), out _));
            Assert.Throws<ArgumentException>(() => pp.TryGetProperty(null, typeof(object), out _));
        }

        [Theory]
        [PropertyData(nameof(StrictProperties))]
        public void ClearProperty_will_throw_on_invalid_property_name(IProperties pp) {
            Assert.Throws<ArgumentException>(() => pp.ClearProperty(""));
            Assert.Throws<ArgumentException>(() => pp.ClearProperty(null));
        }
    }
}
