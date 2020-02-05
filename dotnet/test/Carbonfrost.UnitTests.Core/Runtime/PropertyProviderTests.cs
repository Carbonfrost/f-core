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
using System.Linq;
using Carbonfrost.Commons.Core.Runtime;
using Carbonfrost.Commons.Spec;

namespace Carbonfrost.UnitTests.Core {

    public class PropertyProviderTests {

        [Fact]
        public void Compose_converts_null_to_Null_provider() {
            Assert.Same(PropertyProvider.Null, PropertyProvider.Compose(null, null));
        }

        [Fact]
        public void Compose_will_apply_optimal_composite_on_nulls() {
            var original = new Properties();
            Assert.Same(original, PropertyProvider.Compose(PropertyProvider.Null, original, PropertyProvider.Null));
        }

        [Fact]
        public void Format_parse_and_render_string_format() {
            var pp = PropertyProvider.FromValue(new {
                                                    planet = "Phazon",
                                                });
            Assert.Equal("Hello, Phazon", PropertyProvider.Format("Hello, ${planet}", pp));
        }

        [Fact]
        public void Format_implicitly_converts_to_adapter() {
            Assert.Equal("Hello, Phazon", PropertyProvider.Format("Hello, ${planet}", new {
                planet = "Phazon",
            }));
        }

        [Fact]
        public void FromArray_can_convert_property_to_index() {
            var pp = PropertyProvider.FromArray("a", "c");
            Assert.Equal("a", pp.GetProperty("0"));
            Assert.Equal(typeof(string), pp.GetPropertyType("0"));
        }

        [Theory]
        [InlineData("-1", Name = "out of range")]
        [InlineData("33", Name = "out of range positive")]
        [InlineData("text", Name = "not a number")]
        public void FromArray_adapter_can_use_invalid_properties(string name) {
            var pp = PropertyProvider.FromArray("a", "c");
            Assert.Null(pp.GetProperty(name));
            Assert.Null(pp.GetPropertyType(name));
            Assert.False(pp.TryGetProperty(name, typeof(object), out _));
        }

        class A {}
        class B: A {}

        [Fact]
        public void FromArray_uses_property_type_of_array_for_null_items() {
            var pp = PropertyProvider.FromArray(new A(), new B(), null);
            // Due to it being null, we use the prevailing type of the array which is A[]
            // Assert.Equal(typeof(A), pp.GetPropertyType("2"));

            // Use the specific type of the value
            Assert.Equal(typeof(A), pp.GetPropertyType("0"));
            Assert.Equal(typeof(B), pp.GetPropertyType("1"));
        }

        [Fact]
        public void FromArray_allows_property_names_not_an_index() {
            var pp = PropertyProvider.FromArray("a", "c");
            Assert.Null(pp.GetProperty("a"));
        }

        [Fact]
        public void FromArray_can_return_Null_for_empty_array() {
            var pp = PropertyProvider.FromArray();
            Assert.Same(PropertyProvider.Null, pp);

            pp = PropertyProvider.FromArray(null);
            Assert.Same(PropertyProvider.Null, pp);
        }

        [Fact]
        public void FromFactory_can_create_instance() {
            bool invoked = false;
            var pp = PropertyProvider.FromFactory(() => {
                invoked = true;
                return PropertyProvider.Null;
            });
            Assert.Equal(null, pp.GetProperty("s"));
            Assert.True(invoked);
        }

        [Fact]
        public void FromFactory_can_create_instance_from_container() {
            var container = new PPropertiesContainer();
            bool invoked = false;
            var pp = PropertyProvider.FromFactory(() => {
                invoked = true;
                return container;
            });

            Assert.Equal("hello", pp.GetProperty("s"));
            Assert.True(invoked);
        }

        [Fact]
        public void FromValue_can_format_as_strings() {
            var pp = PropertyProvider.FromArray("a", "c");
            Assert.Equal("0=a;1=c", pp.ToString());
        }

        [Fact]
        public void FromValue_can_enumerate_key_value_pairs() {
            var pp = PropertyProvider.FromArray("a", "c");
            Assert.Equal(new [] {
                KeyValuePair.Create("0", (object) "a"),
                KeyValuePair.Create("1", (object) "c"),
             }, ((IEnumerable<KeyValuePair<string, object>>) pp).ToArray());
        }

        [Fact]
        public void FromValue_property_provider_should_obtain_values() {
            var pp = PropertyProvider.FromValue(new {
                                                    planet = "Terra",
                                                });
            object home;
            Assert.True(pp.TryGetProperty("Planet", typeof(string), out home));
            Assert.Equal("Terra", home);
        }

        [Fact]
        public void FromValue_null_should_return_null_instance() {
            var pp = PropertyProvider.FromValue(null);
            Assert.Same(PropertyProvider.Null, pp);
        }

        [Fact]
        public void FromValue_should_return_self_if_already_property_provider() {
            var existing = new Properties();
            var pp = PropertyProvider.FromValue(existing);
            Assert.Same(existing, pp);
        }

        [Fact]
        public void FromValue_can_handle_any_KeyValuePair_enumerable() {
            var items = new [] {
                new KeyValuePair<string, int>("a", 420),
                new KeyValuePair<string, int>("b", 500),
            };
            var pp = PropertyProvider.FromValue(items);
            Assert.Equal(420, pp.GetProperty("a"));
            Assert.IsInstanceOf<IPropertyStore>(pp);
        }

        class PPropertiesContainer : IPropertiesContainer {
            private readonly Properties _properties = new Properties {
                { "s", "hello" }
            };

            public IProperties Properties {
                get {
                    return _properties;;
                }
            }
        }
    }
}
