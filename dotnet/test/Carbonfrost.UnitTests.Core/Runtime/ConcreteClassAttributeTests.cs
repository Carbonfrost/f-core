//
// Copyright 2012 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using Carbonfrost.Commons.Core.Runtime;
using Carbonfrost.Commons.Spec;

namespace Carbonfrost.UnitTests.Core.Runtime {

    public class ConcreteClassAttributeTests {

        [ConcreteClass(typeof(A1))]
        abstract class A {}

        class A1 : A {}

        // Invalid due to target being abstract
        [ConcreteClass(typeof(A))]
        abstract class B {}

        // Invalid due to target not extending base class
        [ConcreteClass(typeof(A1))]
        abstract class C {}

        [Fact]
        public void GetConcreteClass_test_concrete_class_nominal() {
            Assert.Equal(typeof(A1), typeof(A).GetConcreteClass());
        }

        [Fact]
        public void GetConcreteClass_test_concrete_class_error_abstract_target() {
            Assert.Throws<FormatException>(() => typeof(B).GetConcreteClass());
        }

        [Fact]
        public void GetConcreteClass_test_concrete_class_error_not_base_class() {
            Assert.Throws<FormatException>(() => typeof(C).GetConcreteClass());
        }
    }
}
