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

using Carbonfrost.Commons.Core.Runtime;
using Carbonfrost.Commons.Spec;

namespace Carbonfrost.UnitTests.Core {

    public class PropertyProviderExtensionsTests {

        [Fact]
        public void GetDateTimeOffset_will_throw_on_problem_parsing() {
            var pp = PropertyProvider.FromValue(new { a = "nope" });
            Assert.Throws<FormatException>(() => pp.GetDateTimeOffset("a"));
        }

        [Fact]
        public void GetBoolean_will_throw_on_problem_parsing() {
            var pp = PropertyProvider.FromValue(new { a = "nope" });
            Assert.Throws<FormatException>(() => pp.GetBoolean("a"));
        }

        [Theory]
        [InlineData("true", true)]
        [InlineData("false", false)]
        [InlineData("TRUE", true)]
        [InlineData("FALSE", false)]
        public void GetBoolean_will_convert_from_text(string text, bool expected) {
            var pp = PropertyProvider.FromValue(new { a = text });
            Assert.Equal(expected, pp.GetBoolean("a"));
        }
    }
}

