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

using System.Collections.Generic;
using Carbonfrost.Commons.Core.Runtime;
using Carbonfrost.Commons.Spec;

namespace Carbonfrost.UnitTests.Core {

    public class NamespacePropertyProviderTests {

        public IPropertyProvider Subject {
            get {
                return PropertyProvider.Compose(new Dictionary<string, object> {
                    { "env", new { a = "hello", b = "goodbye", c = "target" } },
                    { "sys", new { a = "os", b = "environ", c = "stdout" } },
                });
            }
        }

        [Theory]
        [InlineData("sys:a", "os")]
        [InlineData("env:c", "target")]
        public void GetProperty_will_lookup_by_namespace_prefix(string prop, string expected) {
            Assert.Equal(expected, Subject.GetProperty(prop));
            Assert.Equal(typeof(string), Subject.GetPropertyType(prop));
        }

        [Theory]
        [InlineData("unqualified")]
        [InlineData("sys:z")]
        [InlineData("missingPrefix:c")]
        public void GetProperty_will_return_null_for_missing_key(string prop) {
            Assert.Null(Subject.GetProperty(prop));
            Assert.Null(Subject.GetPropertyType(prop));
        }
    }

}
