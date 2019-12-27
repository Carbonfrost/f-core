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
using System.Reflection;
using Carbonfrost.Commons.Core.Runtime;
using Carbonfrost.Commons.Spec;

namespace Carbonfrost.UnitTests.Core.Runtime {

    public class StartClassInfoTests {

        [Fact]
        public void StaticClasses_should_get_known_static_class_nominal() {
            var sci = StartClassInfo.Get(typeof(StartClassInfo).GetTypeInfo().Assembly);
            var all = sci.StaticClasses;
            Assert.Contains(typeof(Adaptable), all);
        }

        [Fact]
        public void GetByName_should_apply_wildcards() {
            var sci = StartClassInfo.Get(typeof(StartClassInfo).GetTypeInfo().Assembly);
            Assert.Contains(typeof(Activation), sci.GetByName("Activation*"));
        }

        [Fact]
        public void FindStartFields_should_scan_for_public_static_fields() {
            var sci = StartClassInfo.FindStartFields<IActivationFactory>(
                new [] { typeof(ActivationFactory) });

            Assert.SetEqual(new [] {
                    ActivationFactory.Build, ActivationFactory.Default }, sci);
        }
    }
}
