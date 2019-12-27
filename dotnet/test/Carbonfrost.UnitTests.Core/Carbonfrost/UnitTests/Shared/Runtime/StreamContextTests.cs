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
using System.Text;
using Carbonfrost.Commons.Core.Runtime;
using Carbonfrost.Commons.Spec;

namespace Carbonfrost.UnitTests.Core.Runtime {

    public class StreamContextTests {

        [Fact]
        public void Uri_encoding_including_text_encoding() {
            string chars = Convert.ToBase64String(Encoding.UTF32.GetBytes("h"));

            StreamContext sc = StreamContext.FromText("h", Encoding.UTF32);
            Assert.Equal("data:text/plain; charset=utf-32;base64," + chars, sc.Uri.ToString());
        }

        [Fact]
        public void FromSource_when_relative_uri_treat_as_file() {
            var s = StreamContext.FromSource(new Uri("./Build", UriKind.Relative));
            Assert.IsInstanceOf<FileSystemStreamContext>(s);
            Assert.Equal(new Uri("./Build", UriKind.Relative), s.Uri);
        }

        [Fact]
        public void ReadAllText_should_get_read_stream_twice_from_text() {
            StreamContext sc = StreamContext.FromText("abc");

            Assert.Equal("abc", sc.ReadAllText());
            Assert.Equal("abc", sc.ReadAllText());
        }

        [Fact]
        public void FromSource_data_uri_with_no_base64_encoding() {
            StreamContext sc = StreamContext.FromSource(new Uri("data:text/plain,some text"));
            Assert.Equal("some text", sc.ReadAllText());
        }

        [Fact]
        public void FromText_data_uri_should_have_no_Base64_encoding() {
            StreamContext sc = StreamContext.FromText("some text");
            Assert.Equal("data:text/plain; charset=utf-8,some text", sc.Uri.ToString());
        }

        [Fact]
        public void FromText_data_uri_should_have_Base64_encoding_if_nonutf8() {
            StreamContext sc = StreamContext.FromText("0", Encoding.UTF32);
            Assert.Equal("data:text/plain; charset=utf-32;base64,MAAAAA==", sc.Uri.ToString());
        }

        [Fact]
        public void FromFile_should_use_rooted_path() {
            var sc = StreamContext.FromFile("/var/e/t");
            Assert.Equal(new Uri("file:///var/e/t"), sc.Uri);
        }

        [Fact]
        public void Extension_match_extension_absolute_path() {
            var sc = StreamContext.FromSource(new Uri("https://example.com/download.csv#row-3"));
            Assert.Equal(".csv", sc.Extension);
        }
    }
}
