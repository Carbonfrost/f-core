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

using System;
using Carbonfrost.Commons.Core.Runtime;
using Carbonfrost.Commons.Spec;
using Carbonfrost.UnitTests.Core.Runtime;

[assembly: Defines("ExampleImplicitRole")]
[assembly: Defines("ExampleImplicitRoleInterface")]
[assembly: Defines("TestRole", AdapterType = typeof(ExampleRole))]

namespace Carbonfrost.UnitTests.Core.Runtime {

    public class ExampleRole {}
    public class ExampleImplicitRole {}
    public interface IExampleImplicitRoleInterface {}

    public class AdapterRoleInfoTests {

        [Theory]
        [InlineData("ExampleImplicitRole", typeof(ExampleImplicitRole))]
        [InlineData("ExampleImplicitRoleInterface", typeof(IExampleImplicitRoleInterface))]
        public void App_GetAdapterRoleInfo_for_role_name_uses_implicit_adapter_type(string name, Type expectedType) {
            Assert.Equal(
                expectedType,
                App.GetAdapterRoleInfo(name).AdapterType
            );
        }

        [Fact]
        public void App_GetAdapterRoleInfo_for_role_name_uses_attribute_configuration() {
            Assert.Equal(
                typeof(StreamingSource),
                App.GetAdapterRoleInfo("StreamingSource").AdapterType
            );
        }

        [Fact]
        public void App_GetAdapterRoleInfo_for_role_type_uses_reverse_lookup() {
            var ari = App.GetAdapterRoleInfo(typeof(StreamingSource));
            Assert.NotNull(ari);
            Assert.Equal("StreamingSource", ari.Name);
        }
    }
}
