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
using Carbonfrost.Commons.Core;
using Carbonfrost.Commons.Core.Runtime;
using Carbonfrost.Commons.Spec;

namespace Carbonfrost.UnitTests.Core.Runtime {

    public class BuilderTests : TestClass {
        class PObject {}

        [Fact]
        public void Build_creates_new_instance_nominal() {
            var builder = new Builder<PObject>();
            Assert.IsInstanceOf<PObject>(builder.Build());
        }

        class PObjectServiceConstructor {
            public readonly IServiceProvider S;
            public readonly bool ActivationConstructorCalled;

            public PObjectServiceConstructor() {}

            [ActivationConstructor]
            public PObjectServiceConstructor(IServiceProvider s) {
                S = s;
                ActivationConstructorCalled = true;
            }
        }

        [Fact]
        public void Build_invokes_service_provider_binding_on_construction() {
            var builder = new Builder<PObjectServiceConstructor>();
            var actual = builder.Build();
            
            Assert.True(actual.ActivationConstructorCalled);
            Assert.Same(ServiceProvider.Root, actual.S);
        }
    }
}
