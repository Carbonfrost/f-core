//
// Copyright 2020 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using System.Collections;
using System.Collections.Generic;
using Carbonfrost.Commons.Core.Runtime;
using Carbonfrost.Commons.Spec;

namespace Carbonfrost.UnitTests.Core {

    public class ArrayPropertyProviderTests {

        private readonly A[] _items = { new A("a"), new A("c"), new B("e"), null };

        public IPropertyProvider Subject {
            get {
                return PropertyProvider.FromArray(_items);
            }
        }

        class A {
            public readonly string Value;
            public A(string value) { Value = value; }

            public override bool Equals(object obj) {
                return obj is A && ((A) obj).Value == Value;
            }

            public override int GetHashCode() {
                return Value.GetHashCode();
            }
        }

        class B: A {
            public B(string value): base(value) {}
        }

        [Fact]
        public void GetProperty_can_convert_property_to_index() {
            Assert.Equal(new A("a"), Subject.GetProperty("0"));
        }

        [Theory]
        [InlineData("0", typeof(A))]
        [InlineData("2", typeof(B))]
        public void GetPropertyType_can_provide_correct_type(string prop, Type expected) {
            Assert.Equal(expected, Subject.GetPropertyType(prop));
        }

        [Fact]
        public void GetPropertyType_will_infer_array_type_for_null_value() {
            Assert.Equal(typeof(A), Subject.GetPropertyType("3"));
        }

        [Theory]
        [InlineData("-1", Name = "out of range")]
        [InlineData("33", Name = "out of range positive")]
        [InlineData("text", Name = "not a number")]
        public void GetProperty_can_use_invalid_properties(string name) {
            Assert.Null(Subject.GetProperty(name));
            Assert.Null(Subject.GetPropertyType(name));
            Assert.False(Subject.TryGetProperty(name, typeof(object), out _));
        }

        [Fact]
        public void GetEnumerator_will_generate_values() {
            int index = 0;
            foreach (KeyValuePair<string, object> value in (IEnumerable) Subject) {
                Assert.Equal(index.ToString(), value.Key);
                Assert.Equal(_items[index], value.Value);
                index++;
            }
        }
    }
}

