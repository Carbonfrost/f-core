//
// Copyright 2012, 2016, 2019 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using System.Reflection;
using Carbonfrost.Commons.Core;
using Carbonfrost.Commons.Core.Runtime;
using Carbonfrost.Commons.Spec;

namespace Carbonfrost.UnitTests.Core.Runtime {

    public class ConcreteClassProviderAttributeTests {

        [ConcreteClassImpl]
        interface IP<T> {}
        class P : IP<P>, IR {}

        [ConcreteClassProvider(typeof(MyConcreteClassProvider))]
        interface IR {}

        [ConcreteClassProvider(typeof(MyConcreteClassProvider))]
        interface IS {} // P doesn't implement IS as required

        class MyConcreteClassProvider : IConcreteClassProvider {

            public Type GetConcreteClass(Type sourceType, IServiceProvider serviceProvider) {
                return typeof(P);
            }
        }

        sealed class ConcreteClassImplAttribute : Attribute, IConcreteClassProvider {

            public Type GetConcreteClass(Type sourceType, IServiceProvider serviceProvider) {
                return sourceType.GetTypeInfo().GetGenericArguments()[0];
            }
        }

        [Fact]
        public void GetConcreteClass_test_concrete_class_provider_custom_impl() {
            Assert.Equal(typeof(P), typeof(IP<P>).GetConcreteClass());
            Assert.IsInstanceOf<ConcreteClassImplAttribute>(typeof(IP<P>).GetConcreteClassProvider());
        }

        [Fact]
        public void GetConcreteClass_test_concrete_class_provider_via_parameter() {
            Assert.Equal(typeof(P), typeof(IR).GetConcreteClass());
            Assert.IsInstanceOf<ConcreteClassProviderAttribute>(typeof(IR).GetConcreteClassProvider());

            var a = (ConcreteClassProviderAttribute ) typeof(IR).GetConcreteClassProvider();
            Assert.IsInstanceOf<MyConcreteClassProvider>(a.Value);
        }

        [Fact]
        public void Constructor_should_throw_on_non_implementer() {
            Assert.Throws<ArgumentException>(() => new ConcreteClassProviderAttribute(typeof(Uri)));
        }

        [Fact]
        public void GetConcreteClass_should_throw_on_non_implementer() {
            Assert.Throws<FormatException>(() => typeof(IS).GetConcreteClass());
        }
    }
}
