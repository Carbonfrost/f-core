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
using Carbonfrost.Commons.Core.Runtime;
using Carbonfrost.Commons.Spec;

namespace Carbonfrost.UnitTests.Core {

    public class PropertyProviderTests {

        [Fact]
        public void Format_parse_and_render_string_format() {
            var pp = PropertyProvider.FromValue(new {
                                                    planet = "Phazon",
                                                });
            Assert.Equal("Hello, Phazon", PropertyProvider.Format("Hello, ${planet}", pp));
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
        public void FromValue_can_handle_any_KeyValuePair_enumerable() {
            var items = new [] {
                new KeyValuePair<string, int>("a", 420),
                new KeyValuePair<string, int>("b", 500),
            };
            var pp = PropertyProvider.FromValue(items);
            Assert.Equal(420, pp.GetProperty("a"));
            Assert.IsInstanceOf<IPropertyStore>(pp);
        }
    }
}

