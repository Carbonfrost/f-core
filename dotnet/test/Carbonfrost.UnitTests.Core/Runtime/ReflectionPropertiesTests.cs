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

namespace Carbonfrost.UnitTests.Core {

    public class ReflectionPropertiesTests : TestClass {

        public class PValues {
            public string A { get; set; }
            public int B { get; set; }
        }

        public IProperties Subject { get; set; }
        public PValues Value { get; set; }

        protected override void BeforeTest(Commons.Spec.ExecutionModel.TestUnit test) {
            Value = new PValues();
            Subject = new ReflectionProperties(Value);
        }

        [Fact]
        public void SetProperty_will_update_property_value() {
            Subject.SetProperty("A", "bc");
            Assert.Equal("bc", Value.A);
        }

        [Fact]
        public void TrySetProperty_will_update_property_value() {
            Subject.TrySetProperty("A", "bc");
            Assert.Equal("bc", Value.A);
        }

        [Fact]
        public void ClearProperty_will_update_property_value_to_default() {
            Value.B = 20;
            Subject.ClearProperty("B");
            Assert.Equal(0, Value.B);
        }

        [Fact]
        public void ClearProperties_will_update_property_value_to_default() {
            Value.A = "wrong";
            Value.B = 20;
            Subject.ClearProperties();
            Assert.Null(Value.A);
            Assert.Equal(0, Value.B);
        }

        [Fact]
        public void SetProperty_will_throw_on_missing_property() {
            Expect(() => Subject.SetProperty("R", "bc")).Will.Throw();
        }

        [Fact]
        public void TrySetProperty_will_not_throw_on_missing_property() {
            Expect(() => { Subject.TrySetProperty("R", "bc"); }).Will.Not.Throw();
            Expect(() => Subject.TrySetProperty("R", "bc")).ToBe.False();
        }
    }
}
