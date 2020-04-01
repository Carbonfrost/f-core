//
// Copyright 2014, 2016 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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

namespace Carbonfrost.UnitTests.Core {

    public class ServiceProviderTests {

        [Fact]
        public void Compose_should_select_first_result_in_GetService() {
            var value1 = new Exception();
            var value2 = new Exception();
            var sp1 = ServiceProvider.FromValue(value1);
            var sp2 = ServiceProvider.FromValue(value2);
            var sp = ServiceProvider.Compose(sp1, sp2);

            Assert.Same(value1, sp.GetService(typeof(Exception)));
        }

        [Fact]
        public void Compose_should_return_null_on_empty() {
            var sp = ServiceProvider.Compose(Enumerable.Empty<IServiceProvider>());
            Assert.Same(ServiceProvider.Null, sp);
        }

        [Fact]
        public void Compose_should_return_null_on_empty_list() {
            var sp = ServiceProvider.Compose(Enumerable.Empty<IServiceProvider>());
            Assert.Same(ServiceProvider.Null, sp);
        }

        [Fact]
        public void Compose_should_return_null_on_null_instances() {
            var sp = ServiceProvider.Compose(null, null, null);
            Assert.Same(ServiceProvider.Null, sp);
        }

        [Fact]
        public void FromValue_returns_Null_when_argument_null() {
            var actual = ServiceProvider.FromValue(null);
            Assert.Same(ServiceProvider.Null, actual);
        }

        [Fact]
        public void Root_should_invoke_start_classes() {
            Assert.Equal(MyService.Instance, ServiceProvider.Root.GetServiceOrDefault<MyService>());
        }

        [Fact]
        public void Root_should_be_service_container() {
            var root = ServiceProvider.Root.GetRequiredService<IServiceContainer>();
            Assert.Same(ServiceProvider.Root, root);
        }

        [Fact]
        public void AddService_Lazy_should_activate_a_service() {
            var c = new ServiceContainer();
            c.AddService<A>();

            var b = c.GetService(typeof(A));
            Assert.NotNull(b);
        }

        [Fact]
        public void AddService_Lazy_should_activate_a_service_chained() {
            var c = new ServiceContainer();
            c.AddService<C>();
            c.AddService<B>();
            c.AddService<A>();

            var result = (C) c.GetService(typeof(C));
            Assert.NotNull(result);
            Assert.NotNull(result.B);
            Assert.NotNull(result.B.A);
        }

        [Fact]
        public void AddService_extension_should_add_on_apparent_type() {
            var c = new ServiceContainer();
            var myComponent = new A();
            c.AddService(myComponent);

            Assert.Same(myComponent, c.GetRequiredService<A>());
        }

        class A {
        }

        class B {
            public A A { get; private set; }
            public B(A a) { A = a; }
        }

        class C {
            public B B { get; private set; }
            public C(B b) { B = b; }
        }
    }

    class MyService {
        public static readonly MyService Instance = new MyService();
        private MyService() {}
    }

    class MyService2 {
        public static readonly MyService2 Instance = new MyService2();
        private MyService2() {}
    }

    static class ServiceRegistration {

        public static void RegisterServices(IServiceContainer container) {
            container.AddService(typeof(MyService), MyService.Instance);
        }
    }
}
