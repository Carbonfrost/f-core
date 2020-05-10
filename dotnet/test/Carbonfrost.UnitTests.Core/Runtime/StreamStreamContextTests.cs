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
using System.IO;
using Carbonfrost.Commons.Core.Runtime;
using Carbonfrost.Commons.Spec;

namespace Carbonfrost.UnitTests.Core.Runtime {

    public class StreamStreamContextTests {

        [Fact]
        public void FromStream_stream_context_from_stream_nominal() {
            MemoryStream ms = new MemoryStream();
            ms.Write(new byte[] { 0, 1, 2, 3, 4, 5}, 0, 6);

            var sc = StreamContext.FromStream(ms);
            Assert.Equal("stream:///", sc.Uri.ToString());
        }

        [Fact]
        public void FromStream_when_file_stream_should_have_Uri() {
            var sc = StreamContext.FromStream(new FileStream(".gitignore", FileMode.Append));
            string expected = "file://" + Path.GetFullPath(".gitignore");
            Assert.Equal(expected, sc.Uri.ToString());
        }

        [Fact]
        public void FromStream_when_nullstream_should_have_Uri() {
            var sc = StreamContext.FromStream(Stream.Null);
            Assert.Equal("null:///", sc.Uri.ToString());
        }

        [Fact]
        public void ChangePath_should_apply_relative_path() {
            var sc = StreamContext.FromStream(new FileStream(".gitignore", FileMode.Append));
            sc = sc.ChangePath("./hello");
            string expected = "file://" + Path.GetFullPath("hello");
            Assert.Equal(expected, sc.Uri.ToString());
        }

        [Fact]
        public void ChangePath_should_throw_by_default() {
            var sc = StreamContext.FromStream(new MemoryStream());
            Assert.Throws<NotSupportedException>(() => sc.ChangePath("./hello"));
        }
    }
}
