//
// Copyright 2014, 2016, 2020 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using System.Linq.Expressions;
using Carbonfrost.Commons.Core.Runtime;
using Carbonfrost.Commons.Spec;

namespace Carbonfrost.UnitTests.Core.Runtime {

    public class SharedRuntimeOptionsAttributeTests {

        [Theory]
        [InlineData(typeof(object))]
        [InlineData(typeof(Uri))]
        [InlineData(typeof(Console))]
        [InlineData(typeof(Expression))]
        public void Providers_scanning_ignored_for_system_assemblies(Type type) {
            Assembly asm = type.GetTypeInfo().Assembly;
            Assert.False(SharedRuntimeOptionsAttribute.GetSharedRuntimeOptions(asm).Providers);
            Assert.False(SharedRuntimeOptionsAttribute.GetSharedRuntimeOptions(asm).Templates);
            Assert.False(SharedRuntimeOptionsAttribute.GetSharedRuntimeOptions(asm).Adapters);
        }

        [Fact]
        public void Providers_scanning_for_this_assembly_enabled() {
            Assembly shared = typeof(Adaptable).GetTypeInfo().Assembly;
            Assert.True(SharedRuntimeOptionsAttribute.GetSharedRuntimeOptions(shared).Providers);

            Assembly self = GetType().GetTypeInfo().Assembly;
            Assert.True(SharedRuntimeOptionsAttribute.GetSharedRuntimeOptions(self).Providers);
        }

        [Fact]
        public void Adapters_scanning_for_this_assembly_enabled() {
            Assembly shared = typeof(Adaptable).GetTypeInfo().Assembly;
            Assert.True(SharedRuntimeOptionsAttribute.GetSharedRuntimeOptions(shared).Adapters);

            Assembly self = GetType().GetTypeInfo().Assembly;
            Assert.True(SharedRuntimeOptionsAttribute.GetSharedRuntimeOptions(self).Adapters);
        }
    }
}
