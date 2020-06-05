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

using Carbonfrost.Commons.Core.Runtime;
using Carbonfrost.Commons.Spec;

namespace Carbonfrost.UnitTests.Core.Runtime {

    public class AppProviderTests {

        [Fact]
        public void GetProviderMember_when_no_match_returns_null() {
            Assert.Null(
                App.GetProviderMember(typeof(StreamingSource), "nomatch")
            );
        }

        [Fact]
        public void GetProviderMember_when_no_match_for_criteria_returns_null() {
            Assert.Null(
                App.GetProviderMember(typeof(StreamingSource), new { Extension = ".none" })
            );
        }

        [Fact]
        public void GetProviderMember_when_not_provider_type_returns_null() {
            Assert.Null(
                App.GetProviderMember(typeof(StreamContext), "L")
            );
        }

        [Fact]
        public void GetStartClasses_apply_wildcard() {
            var all = App.GetStartClasses("*ExampleStartClass");
            Assert.Equal(
                new [] {
                    typeof(P1ExampleStartClass),
                    typeof(P2ExampleStartClass),
                },
                all
            );
        }
    }

    static class P1ExampleStartClass {}
    static class P2ExampleStartClass {}
}
