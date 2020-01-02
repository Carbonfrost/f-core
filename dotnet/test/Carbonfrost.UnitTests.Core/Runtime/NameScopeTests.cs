 //
// Copyright 2016, 2019 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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

    public class NameScopeTests {

        [Fact]
        public void AsPropertyProvider_should_return_added_item() {
            var parent = new NameScope();
            var instance = new object();
            parent.RegisterName("c", instance);
            var pp = parent.AsPropertyProvider();
            Assert.Same(instance, pp.GetProperty("c"));
        }

        [Fact]
        public void FindName_should_traverse_to_parent_scope() {
            var parent = new NameScope();
            var instance = new object();
            parent.RegisterName("c", instance);
            var ns = new NameScope(parent);
            Assert.Same(instance, ns.FindName("c"));
        }

        [Fact]
        public void ReadOnly_should_provide_read_only_adapter() {
            var ns = new NameScope();
            var ex = Record.Exception(
                () => NameScope.ReadOnly(ns).RegisterName("s", new object())
            );
            Assert.IsInstanceOf<InvalidOperationException>(ex);
            Assert.Contains("read-only", ex.Message);
        }

        [Fact]
        public void ReadOnly_should_return_Empty_when_Empty_is_arg() {
            Assert.Same(NameScope.Empty, NameScope.ReadOnly(NameScope.Empty));
        }
    }
}
