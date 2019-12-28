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

using Carbonfrost.Commons.Core.Runtime;
using Carbonfrost.Commons.Spec;

namespace Carbonfrost.UnitTests.Core.Runtime {

    public class AdaptableBuilderTests {

        [Builder(typeof(ABuilder3))]
        [Adapter(typeof(ABuilder4), "builder")]
        class A {}

        [Adapter(typeof(ABuilder3), "Builder")]
        class B {}
        class ABuilder3 {
            public object Build() { return null; }
        }
        class ABuilder4 {}

        [Builder(typeof(C))] // Not allowed to use self
        class C {
            public object Build() { return null; }
        }

        [Builder(typeof(A))]
        class D {} // Not allowed since A specifies a Builder

        class E {}
        class EBuilder { // Its implicit builder
            public E Build(IServiceProvider serviceProvider) { return default(E); }
        }

        class F {}

        [Builder(typeof(ABuilder3))]
        class FBuilder {} // Would be the implicit builder except
        // that it itself specifies a builder

        class G {}
        class GBuilder {
            public virtual G Build() { return null; }
        }

        class H : G {}
        class HBuilder : GBuilder {
            public override G Build() { return null; }
        }

        [Fact]
        public void GetBuilderType_should_apply_implicitly_builder() {
            Assert.Equal(typeof(EBuilder), Adaptable.GetBuilderType(typeof(E)));
        }

        [Fact]
        public void GetAdapterType_works_with_builder_role_name() {
            Assert.Equal(typeof(EBuilder), Adaptable.GetAdapterType(typeof(E), "Builder"));
        }

        [Fact]
        public void GetBuilderType_prevents_builder_type_that_specifies_builder_Type() {
            var result = Adaptable.GetBuilderType(typeof(D));
            Assert.Null(result);
        }

        [Fact]
        public void GetBuilderType_ensures_cannot_use_self_as_builder_adapter() {
            var result = Adaptable.GetBuilderType(typeof(C));
            Assert.Null(result);
        }

        [Fact]
        public void GetBuilderType_can_use_derived_builder_class() {
            var result = Adaptable.GetBuilderType(typeof(H));
            Assert.Equal(typeof(HBuilder), result);
        }

        [Fact]
        public void GetAdapterType_role_names_are_case_insensitive() {
            Type correct = typeof(ABuilder3);

            Assert.Equal(correct, Adaptable.GetAdapterType(typeof(A), "Builder"));
            Assert.Equal(correct, Adaptable.GetAdapterType(typeof(A), "builder"));
        }

        [Fact]
        public void GetAdapterType_should_be_equivalent_to_name_adapters_or_use_builder_attribute() {
            Type correct = typeof(ABuilder3);
            // Using the BuilderAttribute
            Assert.Equal(correct, Adaptable.GetBuilderType(typeof(A)));
            Assert.Equal(correct, Adaptable.GetAdapterType(typeof(A), "Builder"));

            // Using the AdapterAttribute
            Assert.Equal(correct, Adaptable.GetBuilderType(typeof(B)));
            Assert.Equal(correct, Adaptable.GetAdapterType(typeof(B), "Builder"));
        }

    }
}
