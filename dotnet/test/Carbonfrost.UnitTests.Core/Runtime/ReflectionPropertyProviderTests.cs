//
// Copyright 2014, 2016, 2020 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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

using Carbonfrost.Commons.Core.Runtime;
using Carbonfrost.Commons.Spec;

namespace Carbonfrost.UnitTests.Core {

    public class ReflectionPropertyProviderTests {

        class A {
            public string B { get { return "S"; } }
            public string C { get { return null; } }
            public N D { get { return new N(); } }
        }

        class L {}
        class N : L {}
        class O : L {}

        [Fact]
        public void TryGetProperty_should_apply_to_nulls() {
            var pp = new ReflectionPropertyProvider(new A());
            object actual;
            Assert.True(pp.TryGetProperty("C", typeof(string), out actual));
            Assert.Null(actual);
        }

        [Fact]
        public void TryGetProperty_should_filter_on_property_type() {
            var pp = new ReflectionPropertyProvider(new A());
            object actual;
            Assert.False(pp.TryGetProperty("B", typeof(int), out actual));
        }

        [Fact]
        public void TryGetProperty_should_filter_on_property_type_polymorphic() {
            var pp = new ReflectionPropertyProvider(new A());
            object actual;
            Assert.True(pp.TryGetProperty("D", typeof(object), out actual));
            Assert.True(pp.TryGetProperty("D", typeof(L), out actual));
            Assert.True(pp.TryGetProperty("D", typeof(N), out actual));
            Assert.False(pp.TryGetProperty("D", typeof(O), out actual));
        }

        [Fact]
        public void GetPropertyType_should_obtain_value_nominal() {
            var pp = new ReflectionPropertyProvider(new A());
            Assert.Equal(typeof(string), pp.GetPropertyType("B"));
            Assert.Null(pp.GetPropertyType("Z"));
        }

        [Fact]
        public void ToString_calls_underlying_string() {
            var pp = new ReflectionPropertyProvider(new A());
            Assert.Equal(new A().ToString(), pp.ToString());
        }

    }
}


