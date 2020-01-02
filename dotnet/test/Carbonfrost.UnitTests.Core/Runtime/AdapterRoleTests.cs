//
// Copyright 2019 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using Carbonfrost.Commons.Spec;
using AdapterRole = Carbonfrost.Commons.Core.Runtime.AdapterRole;

namespace Carbonfrost.UnitTests.Core.Runtime {

    public class AdapterRoleTests {

        class A {}
        class ABuilder {
            public A Build() {
                return new A();
            }
        }

        class WithArgs {}
        class WithArgsBuilder {
            public WithArgs Build(IServiceProvider serviceProvider) {
                return new WithArgs();
            }
        }

        class WithDerivedReturnType {}
        class PDerived : WithDerivedReturnType {}
        class PDerivedBuilder {
            public WithDerivedReturnType Build() {
                return new PDerived();
            }
        }

        [Theory]
        [InlineData(typeof(ABuilder), typeof(A))]
        [InlineData(typeof(WithArgsBuilder), typeof(WithArgs))]
        [InlineData(typeof(PDerivedBuilder), typeof(WithDerivedReturnType))]
        public void IsBuilderType_applies_to_valid_builder_types(Type type, Type adaptee) {
            Assert.True(AdapterRole.IsBuilderType(type, adaptee));
        }
    }
}
