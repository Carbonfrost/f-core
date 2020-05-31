//
// Copyright 2013 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using System.Linq;

using Carbonfrost.Commons.Core;
using Carbonfrost.Commons.Core.Runtime;
using Carbonfrost.Commons.Spec;

namespace Carbonfrost.UnitTests.Core.Runtime {

    public class ServiceContainerTests {

        class ServiceA {}
        class ServiceB {}
        class ServiceC : ServiceB {}

        [Fact]
        public void AddService_should_throw_ArgumentNullException_on_serviceInstance_null() {
            ServiceContainer c = new ServiceContainer();
            Assert.Throws<ArgumentNullException>(() => c.AddService((object) null));
        }

        [Fact]
        public void AddService_should_allow_lazy_value_on_service_add() {
            ServiceContainer c = new ServiceContainer();
            c.AddService(typeof(PService), new Lazy<PService>());

            Assert.IsInstanceOf<PService>(c.GetService(typeof(PService)));
        }

        [Fact]
        public void AddService_should_unwrap_creator_callback_instances() {
            ServiceContainer c = new ServiceContainer();
            ServiceA a = null;
            Func<IServiceContainer, Type, object> callback = (container, t) => (a = new ServiceA());
            c.AddService(typeof(ServiceA), callback);

            Assert.NotNull(c.GetService(typeof(ServiceA)));
            Assert.Same(a, c.GetService(typeof(ServiceA)));
        }

        [Fact]
        public void GetService_should_retrieve_singleton_service() {
            ServiceContainer c = new ServiceContainer();
            ServiceA a = new ServiceA();
            c.AddService(typeof(ServiceA), a);

            Assert.Same(a, c.GetRequiredService<ServiceA>());
            Assert.Same(a, c.GetService(typeof(ServiceA)));
        }

        [Fact]
        public void GetService_should_obtain_service_container_trivially() {
            ServiceContainer c = new ServiceContainer();
            Assert.Same(c, c.GetService(typeof(IServiceContainer)));
            Assert.Same(c, c.GetService(typeof(IServiceProvider)));
        }

        [Fact]
        public void GetService_should_obtain_service_container_extension_trivially() {
            ServiceContainer c = new ServiceContainer();
            Assert.Same(c, c.GetService(typeof(IServiceContainer)));
            Assert.Same(c, c.GetService(typeof(ServiceContainer)));
        }

        class PService {}

        [Fact]
        public void GetService_should_cache_service_creator_instance() {
            ServiceContainer c = new ServiceContainer();
            c.AddService(() => new PService());

            var firstRetrieval = c.GetService<PService>();
            var secondRetrieval = c.GetService<PService>();
            Assert.Same(firstRetrieval, secondRetrieval);
        }

        [Fact]
        public void GetService_Lazy_should_wrap_service() {
            var c = new ServiceContainer();
            var a = new ServiceA();
            c.AddService(typeof(ServiceA), a);

            var func = c.GetService(typeof(Lazy<ServiceA>)) as Lazy<ServiceA>;
            Assert.NotNull(func);
            Assert.Same(a, func.Value);
        }

        [Fact]
        public void GetService_Func_should_wrap_service() {
            var c = new ServiceContainer();
            var a = new ServiceA();
            c.AddService(typeof(ServiceA), a);

            var func = c.GetService(typeof(Func<ServiceA>)) as Func<ServiceA>;
            Assert.NotNull(func);
            Assert.Same(a, func());
        }
    }
}
