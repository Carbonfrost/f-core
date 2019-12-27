//
// Copyright 2013, 2016 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using System.IO;
using Carbonfrost.Commons.Core.Runtime;
using Carbonfrost.Commons.Spec;

namespace Carbonfrost.UnitTests.Core {

    public class PropertiesReaderTests : TestClass {

        [Fact]
        public void Parse_from_file_read_nominal() {
            Properties p = ReadAlpha();

            Assert.Equal(5, p.InnerMap.Count);
            Assert.Equal("bar", p.GetProperty("Foo"));
            Assert.Equal("Continued on multiple lines (prefix whitespace removed). Another line. Another.", p.GetProperty("Baz"));
        }

        [Fact]
        public void Parse_from_file_parse_escape_sequences() {
            Properties p = ReadAlpha();
            Assert.Equal("Escape\nTwo lines", p.GetProperty("Bash"));
        }

        // TODO Url syntax test

        private Properties ReadAlpha() {
            Stream source = TestContext.OpenRead("alpha.properties");
            Properties p = Properties.FromStream(source);
            return p;
        }
    }
}
