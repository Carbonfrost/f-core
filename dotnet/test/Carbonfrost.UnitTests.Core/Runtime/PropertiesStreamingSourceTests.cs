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

using System.IO;
using Carbonfrost.Commons.Core.Runtime;
using Carbonfrost.Commons.Spec;

namespace Carbonfrost.UnitTests.Core.Runtime {

    public class PropertiesStreamingSourceTests {

        [Fact]
        public void Load_should_initialize_properties_existing() {
            var sc = StreamingSource.Properties;
            var properties = new Properties();
            sc.Load(StreamContext.FromText("a=b\nc=d"), properties);
            Assert.Equal("b", properties["a"]);
            Assert.Equal("d", properties["c"]);
        }

        [Fact]
        public void Load_should_initialize_from_text_reader() {
            var sc = (TextSource) StreamingSource.Properties;
            var properties = new Properties();
            sc.Load(new StringReader("a=b\nc=d"), properties);
            Assert.Equal("b", properties["a"]);
            Assert.Equal("d", properties["c"]);
        }

        [Fact]
        public void Provider_metadata_should_enumerate_content_types_and_extensions() {
            var props = StreamingSource.Properties;
            var me = (StreamingSourceUsageAttribute) App.GetProviderMetadata(typeof(StreamingSource), props);

            Assert.SetEqual(new [] { "text/x-properties", "text/x-ini" }, me.EnumerateContentTypes());
            Assert.SetEqual(new [] { ".cfg", ".conf", ".ini", ".properties" }, me.EnumerateExtensions());
        }
    }
}
