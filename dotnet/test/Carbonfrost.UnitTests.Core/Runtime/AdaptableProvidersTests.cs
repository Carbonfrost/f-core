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
using System.Linq.Expressions;
using System.Reflection;
using Carbonfrost.Commons.Core;
using Carbonfrost.Commons.Core.Runtime;
using Carbonfrost.Commons.Spec;
using Carbonfrost.UnitTests.Core.Runtime;

[assembly: Provides(typeof(TestProvider))]

namespace Carbonfrost.UnitTests.Core.Runtime {

    public class TestProvider {}

    [Provider(typeof(TestProvider), Name = "A B C")]
    public class ATestProvider : TestProvider {

        public readonly string A;

        public ATestProvider(string a) {
            A = a;
        }
    }

    public class AdaptableProvidersTests {

        [Fact]
        public void GetProviderInfo_using_local_name_should_obtain_one() {
            var cs = App.GetProviderInfo(
                typeof(IActivationFactory),
                "default"
            );

            Assert.Same(ActivationFactory.Default, cs.Activate(null, null));
        }

        [Fact]
        public void GetProviderInfo_should_have_ToString_using_name_and_curie() {
            var cs = App.GetProviderInfo(
                typeof(IActivationFactory),
                "default"
            );

            Assert.Equal("[runtime:default] IActivationFactory", cs.ToString());
        }

        [Fact]
        public void GetProviderInfo_using_type_criteria_should_find_names_and_in_order() {
            var cs = App.GetProviderInfo(typeof(TestProvider), typeof(ATestProvider));

            // Proper order is:
            // - explicitly specified Name= name
            // - names returned from ProviderAttribute
            // - name apparent from type and namespace
            Assert.Equal("A", cs.Name.LocalName);
            Assert.Equal(new[] { "A", "B", "C", "ATestProvider" }, cs.Names.Select(t => t.LocalName).ToArray());
        }

        [Fact]
        public void GetProviderInfo_using_member_criteria_should_obtain_one() {
            var defaultMember = typeof(ActivationFactory).GetTypeInfo().GetField("Default");
            var cs = App.GetProviderInfo(
                typeof(IActivationFactory),
                defaultMember);

            Assert.Same(ActivationFactory.Default, cs.Activate(null, null));
        }

        [Fact]
        public void GetProviderTypes_should_contain_known_root_providers() {
            var cs = App.GetProviderTypes();

            Assert.Contains(typeof(IActivationFactory), cs);
            Assert.Contains(typeof(IAdapterFactory), cs);
            Assert.Contains(typeof(StreamingSource), cs);
        }

        [Fact]
        public void GetProviders_should_handle_non_existent_provider_type() {
            var cs = App.GetProviders<ITemplate>();
            Assert.Empty(cs);
        }

        [Fact]
        public void GetProvider_should_handle_non_existent_provider_type_by_name() {
            var cs = App.GetProvider<StreamingSource>("J");
            Assert.Null(cs);
        }

        [Fact]
        public void GetProvider_should_handle_provider_names_case_insensitive() {
            var cs1 = App.GetProvider<StreamingSource>("properties");
            var cs2 = App.GetProvider<StreamingSource>("Properties");
            Assert.Same(StreamingSource.Properties, cs2);
            Assert.Same(StreamingSource.Properties, cs1);
        }

        [Fact]
        public void GetProviderName_should_lookup_nominal() {
            var cs = App.GetProviderName(typeof(StreamingSource), StreamingSource.Properties);
            Assert.Equal("properties", cs.LocalName);
        }

        [Fact]
        public void GetProviderName_should_work_with_a_member_specification() {
            var field = typeof(StreamingSource).GetTypeInfo().GetField("Properties");
            var cs = App.GetProviderName(typeof(StreamingSource), field);
            Assert.Equal("properties", cs.LocalName);
        }

        [Fact]
        public void GetProviderNames_should_provide_all_names() {
            var cs = App.GetProviderNames(typeof(StreamingSource), StreamingSource.Properties);
            Assert.Equal(1, cs.Count());
            Assert.Equal("properties", cs.First().LocalName);
        }

        [Fact]
        public void GetProviderNames_should_provide_rollup_names() {
            var cs = App.GetProviderNames(typeof(TestProvider), new ATestProvider(null));
            Assert.Equal(new [] { "A", "B", "C", "ATestProvider" }, cs.Select(t => t.LocalName).ToArray());
        }

        [Fact]
        public void GetProviderNames_should_provide_all_names_via_member_criteria() {
            var field = typeof(StreamingSource).GetTypeInfo().GetField("Properties");
            var cs = App.GetProviderNames(typeof(StreamingSource),
                                                              field);
            Assert.NotEmpty(cs);
            Assert.Equal("properties", cs.First().LocalName);
        }

        [Fact]
        public void GetProviders_should_get_provider_using_assembly_criteria() {
            var cs = App.GetProviders(typeof(StreamingSource),
                                                          new { Assembly = typeof(object).GetTypeInfo().Assembly });
            Assert.Empty(cs);

            cs = App.GetProviders(typeof(StreamingSource),
                                                      new { Assembly = typeof(StreamingSource).GetTypeInfo().Assembly }).ToList();
            Assert.Equal(3, cs.Count());
        }

        [Fact]
        public void GetProvider_gets_provider_names_by_qualified_name() {
            var name = NamespaceUri.Create(Xmlns.Core2008) + "properties";
            var cs1 = App.GetProvider<StreamingSource>(name);
            Assert.Same(StreamingSource.Properties, cs1);
        }

        [Fact]
        public void GetProviders_gets_providers_using_static_fields_component_store() {
            var cs = App.GetProviders<StreamingSource>();

            Assert.Contains(StreamingSource.Properties, cs);
            Assert.Contains(StreamingSource.XmlFormatter, cs);
        }

        [Fact]
        public void GetProvider_gets_provider_names_by_name_repeatable() {
            var name = "properties";
            var cs1 = App.GetProvider<StreamingSource>(name);
            var cs2 = App.GetProvider<StreamingSource>(name);

            Assert.Same(StreamingSource.Properties, cs1);
            Assert.Same(StreamingSource.Properties, cs2);
        }
    }
}
