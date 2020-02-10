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

using Carbonfrost.Commons.Core.Runtime;
using Carbonfrost.Commons.Spec;

namespace Carbonfrost.UnitTests.Core {

    public class PropertyProviderCollectionTests {

        [Fact]
        public void GetProperty_will_access_item_by_name() {
            var pp = new PropertyProviderCollection();
            pp["name"] = PropertyProvider.FromArray(0, 1);

            Assert.Equal(1, pp.GetProperty("1"));
            Assert.Equal(typeof(int), ((IPropertyProvider) pp).GetPropertyType("1"));
        }

        [Fact]
        public void Indexer_set_will_replace_item_by_index() {
            var pp = new PropertyProviderCollection();
            var prop1 = PropertyProvider.FromArray(0);
            var prop2 = PropertyProvider.FromArray(0);
            pp.AddNew("name", prop1);

            pp[0] = prop2;

            Assert.DoesNotContain(prop1, pp);
            Assert.Same(prop2, pp[0]);
        }
    }
}

