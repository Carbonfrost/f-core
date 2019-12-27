//
// Copyright 2015, 2016 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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

    public class FileSystemStreamContextTests {

        public string RootDirectory {
            get {
                return "dotnet/test/Carbonfrost.UnitTests.Core/";
            }
        }

        [Fact]
        public void Uri_should_contain_fragment_from_source() {
            var ss = StreamContext.FromSource(new Uri("file:///hello.txt#s"));
            Assert.Equal("file:///hello.txt#s", ss.Uri.AbsoluteUri);
        }

        [Fact]
        public void Open_should_open_a_file_nominal() {
            var ss = StreamContext.FromFile(RootDirectory + "Content/alpha.properties");
            Assert.NotNull(ss.Open());
        }
    }
}
