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
using System.Collections.Generic;
using Carbonfrost.Commons.Core;
using Carbonfrost.Commons.Core.Runtime;
using Carbonfrost.Commons.Spec;

namespace Carbonfrost.UnitTests.Core.Runtime {

    public class ActivationFactoryTests {

        [Theory]
        [InlineData("Default")]
        [InlineData("Build")]
        public void FromName_provides_expected_names(string name) {
            Assert.NotNull(
                ActivationFactory.FromName(name)
            );
        }

        [Fact]
        public void CreateInstance_applies_concrete_class() {
            var inst = ActivationFactory.Default.CreateInstance(typeof(PAbstract));
            Assert.IsInstanceOf(typeof(PConcrete), inst);

            inst = ActivationFactory.Default.CreateInstance(typeof(PConcrete));
            Assert.IsInstanceOf(typeof(PConcrete), inst);
        }

        [Fact]
        public void CreateInstance_when_activation_provider_throws_can_resume_via_ExceptionHandler_service() {
            var inst = new BActivationFactory();
            bool called = false;
            ExceptionHandler handler = (_sender, _) => { called = true; };

            var provider = ServiceProvider.FromValue(handler);
            Assert.DoesNotThrow(() => { inst.CreateInstance(typeof(PConcrete), serviceProvider: provider); });
            Assert.True(called);
        }

        [ConcreteClass(typeof(PConcrete))]
        abstract class PAbstract {}
        class PConcrete : PAbstract {}

        class PThrowsActivationProvider : IActivationProvider {
            public object ActivateComponent(IServiceProvider serviceProvider, object component) {
                throw new InvalidOperationException();
            }
        }

        class BActivationFactory : ActivationFactory {
            protected override IEnumerable<IActivationProvider> GetActivationProviders(Type type, object component) {
                return new [] {
                    new PThrowsActivationProvider(),
                };
            }
        }
    }
}
