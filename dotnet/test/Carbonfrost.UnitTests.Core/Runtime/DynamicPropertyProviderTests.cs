//
// Copyright 2020 Carbonfrost Systems, Inc. (https://carbonfrost.com)
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
using System.Collections.Generic;
using System.Dynamic;
using Carbonfrost.Commons.Spec;
using Carbonfrost.Commons.Core.Runtime;

namespace Carbonfrost.UnitTests.Core.Runtime {

    public class DynamicPropertyProviderTests : TestClass {

        public IEnumerable<TestData> DynamicObjects {
            get {
                yield return new TestData(new { a = "hello" }).WithName("static type");

                dynamic expando = new ExpandoObject();
                expando.a = "hello";
                yield return new TestData(expando).WithName("expando object");
            }
        }

        [Theory]
        [PropertyData(nameof(DynamicObjects))]
        public void TryGetMember_is_true_when_it_can_bind(dynamic subject) {
            var pp = new DynamicPropertyProvider(subject);
            object actual;
            Assert.True(pp.TryGetProperty("a", typeof(object), out actual));
            Assert.Equal("hello", actual);
        }

        [Theory]
        [PropertyData(nameof(DynamicObjects))]
        public void GetPropertyType_is_correct_Type(dynamic subject) {
            var pp = new DynamicPropertyProvider(subject);
            Assert.Equal(typeof(string), pp.GetPropertyType("a"));
        }

        [Fact]
        public void TryGetMember_is_false_when_it_is_different_return_type() {
            dynamic subject = new { a = "hello" };
            var pp = new DynamicPropertyProvider(subject);
            Assert.False(pp.TryGetProperty("a", typeof(int), out _));
        }

        [Theory]
        [PropertyData(nameof(DynamicObjects))]
        public void TryGetMember_is_false_when_it_cannot_bind(dynamic subject) {
            var pp = new DynamicPropertyProvider(subject);
            Assert.False(pp.TryGetProperty("missing", typeof(object), out _));
        }
    }
}
