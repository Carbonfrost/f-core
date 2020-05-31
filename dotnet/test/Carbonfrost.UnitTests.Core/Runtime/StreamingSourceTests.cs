//
// Copyright 2013, 2020 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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

namespace Carbonfrost.UnitTests.Core.Runtime {

    public class StreamingSourceTests {

        [Theory]
        [InlineData("xmlFormatter")]
        [InlineData("text")]
        [InlineData("properties")]
        public void FromName_test_known_nominal(string name) {
            Assert.NotNull(StreamingSource.FromName(name));
        }

        [Theory]
        [InlineData("xmlFormatter")]
        [InlineData("text")]
        [InlineData("properties")]
        public void FromName_test_known_qualified_name(string name) {
            var qn = NamespaceUri.Create(Xmlns.Core2008) + name;
            Assert.NotNull(StreamingSource.FromName(qn));
        }

        [Theory]
        [InlineData(KnownStreamingSource.XmlFormatter, typeof(XmlFormatterStreamingSource))]
        [InlineData(KnownStreamingSource.Properties, typeof(PropertiesStreamingSource))]
        [InlineData(KnownStreamingSource.Text, typeof(TextStreamingSource))]
        public void GetStreamingSourceType_from_known_produces_correct_type(KnownStreamingSource ks, Type expected) {
            Assert.Equal(
                expected,
                StreamingSource.GetStreamingSourceType(ks)
            );

            Assert.IsInstanceOf(
                expected,
                StreamingSource.Create(ks)
            );
        }


        [Fact]
        public void AppProviderNames_test_known_adapter_names() {
            Assert.SetEqual(
                new [] {
                        "xmlFormatter",
                        "text",
                        "properties",
                    },
                App.GetProviderNames(typeof(StreamingSource)).Select(t => t.LocalName)
            );
        }

        [Fact]
        public void LoadByHydration_should_initialize_Properties() {
            var sc = StreamingSource.Properties;
            var properties = new Properties();
            sc.LoadByHydration(StreamContext.FromText("a=b\nc=d"), properties);

            Assert.Equal("b", properties["a"]);
            Assert.Equal("d", properties["c"]);
        }

        [Theory]
        [InlineData("text/x-properties")]
        [InlineData("text/x-ini")]
        public void FromCriteria_should_acquire_known_content_type(string contentType) {
            Assert.NotNull(StreamingSource.FromCriteria( new { contentType }));
        }

        [StreamingSource(typeof(OverrideStreamingSource))]
        public class OverrideProperties : Properties {
            public string A { get; set; }
        }

        public class OverrideStreamingSource : StreamingSource {

            public override object Load(StreamContext inputSource, Type instanceType) {
                return null;
            }

            public override void Save(StreamContext outputTarget, object value) {}
        }

        [Fact]
        public void Create_should_apply_to_derived_types() {
            Assert.IsInstanceOf<PropertiesStreamingSource>(StreamingSource.Create(typeof(PropertiesTests.MyProperties)));
        }

        [Fact]
        public void Create_should_apply_to_derived_types_with_override_metadata() {
            Assert.IsInstanceOf<OverrideStreamingSource>(StreamingSource.Create(typeof(OverrideProperties)));
        }

    }

}
