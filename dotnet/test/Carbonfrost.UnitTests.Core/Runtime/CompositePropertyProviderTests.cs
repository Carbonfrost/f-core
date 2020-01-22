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

using Carbonfrost.Commons.Core.Runtime;
using Carbonfrost.Commons.Spec;

namespace Carbonfrost.UnitTests.Core {

    public class CompositePropertyProviderTests {

        [Fact]
        public void TryGetProperty_checks_each() {
            var composite = PropertyProvider.Compose(
                PropertyProvider.FromValue(new { a = "1", b = "2" }),
                PropertyProvider.FromValue(new { c = "1", d = "2" })
            );

            object result;
            Assert.True(composite.TryGetProperty("c", out result));
            Assert.Equal("1", result);
        }

        [Fact]
        public void GetPropertyType_checks_each() {
            var composite = PropertyProvider.Compose(
                PropertyProvider.FromValue(new { a = 1, b = 2.0 }),
                PropertyProvider.FromValue(new { c = "1", d = 2.0m })
            );

            Assert.Equal(typeof(string), composite.GetPropertyType("c"));
        }
    }
}

