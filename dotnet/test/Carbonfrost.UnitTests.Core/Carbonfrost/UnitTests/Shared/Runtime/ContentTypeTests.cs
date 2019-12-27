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
using Carbonfrost.Commons.Core.Runtime;
using Carbonfrost.Commons.Spec;

namespace Carbonfrost.UnitTests.Core.Runtime {

    public class ContentTypeTests {

        [Fact]
        public void Parse_should_support_expected_property_values() {
            ContentType type = ContentType.Parse("application/propertytrees+xml");
            Assert.Equal("application/propertytrees+xml", type.MediaType);
            Assert.Equal("application", type.Type);
            Assert.Equal("propertytrees+xml", type.Subtype);

            Assert.Equal("application/propertytrees+xml", type.ToString());
        }

        [Fact]
        public void Parse_should_support_expected_parameters_values() {
            ContentType type = ContentType.Parse("application/propertytrees+xml;charset=utf-8;schema=http://example.com");
            Assert.Equal("utf-8", type.Parameters["charset"]);
            Assert.Equal("http://example.com", type.Parameters["schema"]);
        }

        [Fact]
        public void Parent_should_get_parent_content_type() {
            ContentType type = ContentType.Parse("application/propertytrees+xml");
            Assert.Equal("application/xml", type.Parent.MediaType);
        }
    }

}
