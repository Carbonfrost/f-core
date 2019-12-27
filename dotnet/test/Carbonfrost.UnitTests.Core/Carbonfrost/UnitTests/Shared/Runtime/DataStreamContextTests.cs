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
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Carbonfrost.Commons.Core.Runtime;
using Carbonfrost.Commons.Spec;

namespace Carbonfrost.UnitTests.Core.Runtime {

    public class DataStreamContextTests {

        [Fact]
        public void DataStreamContext_sanity_checks() {
            byte[] data = new byte[32];
            RandomNumberGenerator.Create().GetBytes(data);
            string bytes = Convert.ToBase64String(data);
            Uri u = new Uri("data:application/octet-stream;base64," + bytes);

            Assert.Equal("application/octet-stream;base64," + bytes, u.PathAndQuery);
        }

        [Fact]
        public void Write_data_should_update_uri() {
            // URI is updated when we write to stream
            StreamContext sc = StreamContext.FromSource(new Uri("data:application/octet-stream;base64,"));
            byte[] data = { 0x42, 0x20, 0xFF };
            sc.OpenWrite().Write(data, 0, 3);

            Assert.Equal(new Uri("data:application/octet-stream;base64," + Convert.ToBase64String(data)),
                         sc.Uri);
        }

        [Fact]
        public void ReadAllBytes_should_get_data_uri() {
            byte[] data = new byte[32];
            RandomNumberGenerator.Create().GetBytes(data);

            string bytes = Convert.ToBase64String(data);
            StreamContext sc = StreamContext.FromSource(new Uri("data:application/octet-stream;base64," + bytes));

            Assert.Equal(Convert.FromBase64String(bytes), sc.OpenRead().ReadAllBytes());
        }

        [Fact]
        public void ReadAllBytes_should_get_data_uri_empty_() {
            byte[] data = new byte[0];
            string bytes = Convert.ToBase64String(data);
            StreamContext sc = StreamContext.FromSource(new Uri("data:application/octet-stream;base64," + bytes));

            Assert.Equal(Convert.FromBase64String(bytes), sc.OpenRead().ReadAllBytes());
        }

        [Fact]
        public void ReadAllBytes_should_get_data_uri_empty_string() {
            byte[] data = new byte[0];
            string bytes = Convert.ToBase64String(data);
            StreamContext sc = StreamContext.FromSource(new Uri("data:application/octet-stream;base64,"));

            Assert.Equal(Convert.FromBase64String(bytes), sc.OpenRead().ReadAllBytes());
        }

        [Fact]
        public void ContentType_should_parse_content_type() {
            byte[] data = new byte[0];
            const string CONTENT_TYPE = "application/x-carbonfrost-hwd";
            string bytes = Convert.ToBase64String(data);
            StreamContext sc = StreamContext.FromSource(new Uri("data:" + CONTENT_TYPE + ";base64,"));
            Assert.Equal(CONTENT_TYPE, sc.ContentType.ToString());
            Assert.Contains(";base64", sc.Uri.ToString());
        }

        [Fact]
        public void RealAllText_should_get_read_stream_twice() {
            string bytes = Convert.ToBase64String(Encoding.UTF8.GetBytes("abc"));
            var sc = StreamContext.FromSource(new Uri("data:text/plain;base64," + bytes));
            Assert.Equal("abc", sc.ReadAllText());
            Assert.Equal("abc", sc.ReadAllText());
        }

        [Fact]
        public void RealAllText_should_get_from_text() {
            StreamContext sc = StreamContext.FromText("abc");
            Assert.Equal("text/plain; charset=utf-8", sc.ContentType.ToString());
            Assert.Throws<KeyNotFoundException>(() => sc.ContentType.Parameters["base64"]);
            Assert.Equal("abc", sc.ReadAllText());
        }

        [Fact]
        public void RealAllText_should_allow_optional_content_type_and_uri_encoding() {
            var sc = StreamContext.FromSource(new Uri("data:,A%20brief%20note"));
            Assert.Equal("text/plain", sc.ContentType.ToString());
            Assert.Equal("A brief note", sc.ReadAllText());
        }

        [Fact]
        public void RealAllText_should_allow_uri_encoding() {
            var sc = StreamContext.FromSource(new Uri("data:text/p,A%20brief%20note"));
            Assert.Equal("text/p", sc.ContentType.ToString());
            Assert.Equal("A brief note", sc.ReadAllText());
            Assert.Equal("data:text/p,A%20brief%20note", sc.Uri.OriginalString);
        }

        [Fact]
        public void RealAllText_should_allow_non_standard_non_uri() {
            var sc = StreamContext.FromSource(new Uri("data:text/html,Encode s p aces"));
            Assert.Equal("Encode s p aces", sc.ReadAllText());
            Assert.Equal("data:text/html,Encode%20s%20p%20aces", sc.Uri.OriginalString);
        }

    }
}
