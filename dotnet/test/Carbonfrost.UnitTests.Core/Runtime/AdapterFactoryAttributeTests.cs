//
// Copyright 2019 Carbonfrost Systems, Inc. (https://carbonfrost.com)
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
using Carbonfrost.Commons.Core.Runtime;
using Carbonfrost.Commons.Spec;

namespace Carbonfrost.UnitTests.Core.Runtime {

    public class AdapterFactoryAttributeTests {

        class HelloRoleAttribute : AdapterFactoryAttribute {
            public HelloRoleAttribute() : base(typeof(string)) {}
        }

        class HelloRoleFactoryAttribute : AdapterFactoryAttribute {
            public HelloRoleFactoryAttribute() : base(typeof(string)) {}
        }

        [Fact]
        public void Role_should_be_implied_when_unspecified() {
            Assert.Equal("HelloRole", new HelloRoleAttribute().Role);
            Assert.Equal("HelloRole", new HelloRoleFactoryAttribute().Role);
        }

        [Fact]
        public void Constructor_should_initialize_nominal() {
            var attr = new AdapterFactoryAttribute("null", typeof(string));
            Assert.Equal("null", attr.Role);
            Assert.Equal(typeof(string), attr.AdapterFactoryType);
        }

        [Fact]
        public void Constructor_string_type_should_initialize_nominal() {
            var attr = new AdapterFactoryAttribute("null", "System.String");
            Assert.Equal("null", attr.Role);
            Assert.Equal(typeof(string), attr.AdapterFactoryType);
        }

        [Fact]
        public void Constructor_should_throw_on_required_role_argument() {
            Assert.Throws<ArgumentNullException>(
                () => new AdapterFactoryAttribute(null, typeof(string))
            );

            Assert.Throws<ArgumentNullException>(
                () => new AdapterFactoryAttribute(null, "System.String")
            );
        }

        [Fact]
        public void Constructor_should_throw_on_empty_role_argument() {
            Assert.Throws<ArgumentException>(
                () => new AdapterFactoryAttribute("", typeof(string))
            );

            Assert.Throws<ArgumentException>(
                () => new AdapterFactoryAttribute("", "System.String")
            );
        }
    }
}
