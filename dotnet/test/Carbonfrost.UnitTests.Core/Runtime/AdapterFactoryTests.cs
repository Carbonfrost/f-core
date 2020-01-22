//
// Copyright 2013, 2019 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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

    public class AdapterFactoryTests {

        [Fact]
        public void Compose_zero_implies_null() {
            IAdapterFactory fc = AdapterFactory.Compose();
            Assert.Contains("Null", fc.GetType().FullName);
        }

        [Fact]
        public void Compose_converts_null_to_Null_provider() {
            Assert.Same(AdapterFactory.Null, AdapterFactory.Compose(null, null));
        }

        [Fact]
        public void Compose_will_apply_optimal_composite_on_nulls() {
            var original = AdapterFactory.Default;
            Assert.Same(original, AdapterFactory.Compose(AdapterFactory.Null, original, AdapterFactory.Null));
        }

        [Fact]
        public void GetAdapterType_adapter_factory_t_should_filter_on_role() {
            IAdapterFactory fc = new AdapterFactory<StreamingSource>(AdapterRole.StreamingSource);
            Assert.Null(fc.GetAdapterType(typeof(IProperties), "Builder"));
        }

        [Fact]
        public void GetAdapterType_default_adapter_factory_composed_of_all() {
            IAdapterFactory fc = AdapterFactory.Default;
            Assert.Null(fc.GetAdapterType(typeof(IProperties), "StreamingSource"));
        }

        [Fact]
        public void FromName_provides_Default_provider() {
            Assert.Same(AdapterFactory.Default, AdapterFactory.FromName("Default"));
        }

        [Fact]
        public void GetAssemblyThatDefines_should_obtain_role_assembly() {
            Assert.Equal(typeof(IProperties).GetTypeInfo().Assembly,
                AdapterFactory.GetAssemblyThatDefines("Builder"));
        }

    }
}
