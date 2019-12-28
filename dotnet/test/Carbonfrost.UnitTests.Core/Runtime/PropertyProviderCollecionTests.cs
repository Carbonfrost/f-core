//
// Copyright 2014, 2019 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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

namespace Carbonfrost.UnitTests.Core.Runtime {

    public class PropertyProviderCollectionTests {

        class C {
            public string Id { get { return "0"; } }
        }

        [Fact]
        public void AddNew_should_set_by_name() {
            var unit = new PropertyProviderCollection();
            unit.AddNew("a", new C());

            Assert.NotNull(unit["a"]);
            Assert.NotNull(unit["a"].GetProperty("Id"));
        }
    }
}
