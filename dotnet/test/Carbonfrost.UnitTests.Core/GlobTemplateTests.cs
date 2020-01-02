//
// Copyright 2016 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using Carbonfrost.Commons.Spec;

namespace Carbonfrost.UnitTests.Core {

    public class GlobTemplateTests {

        [Fact]
        public void Parse_should_generate_expected_values() {
            var unit = GlobTemplate.Parse("/var/{dir}/Suite");

            Assert.Equal("^.*/var/(?<dir>.*?)/Suite$", unit.Pattern.ToString());
            Assert.Contains("dir", unit.Variables);
            Assert.Equal(Glob.Parse("/var/*/Suite"), unit.Glob);
        }

        [InlineData("./{dir}/Suite", "^.*/(?<dir>.*?)/Suite$")]
        [InlineData("./{dir}-Suite", "^.*/(?<dir>.*?)-Suite$")]
        [InlineData("././{dir}/./Suite", "^.*/(?<dir>.*?)/Suite$")]
        [InlineData("./hello/./{dir}/./Suite", "^.*/hello/(?<dir>.*?)/Suite$")]
        [Theory]
        public void Parse_should_generate_from_pwd_dot(string text, string expected) {
            var unit = GlobTemplate.Parse(text);
            Assert.Equal(expected, unit.Pattern.ToString());
        }

        [Theory]
        [InlineData("dotnet/src/Carbonfrost.Commons.Core/{name}/{file}.cs")]
        [InlineData("dotnet/src/././Carbonfrost.Commons.Core/./{name}/{file}.cs")]
        public void EnumerateFiles_should_match_files_nominal(string text) {
            var unit = GlobTemplate.Parse(text);
            var all = unit.EnumerateFiles();

            var assembly = all.Single(t => t.FileName.EndsWith("AssemblyInfo.cs"));
            Assert.Equal("Properties", assembly["name"]);
            Assert.Equal("AssemblyInfo", assembly["file"]);
        }
    }

}
