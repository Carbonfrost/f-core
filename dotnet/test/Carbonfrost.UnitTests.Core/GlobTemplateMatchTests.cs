//
// Copyright 2020 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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

using System.Linq;
using Carbonfrost.Commons.Core;
using Carbonfrost.Commons.Spec;

namespace Carbonfrost.UnitTests.Core {

    public class GlobTemplateMatchTests {

        [Fact]
        public void ToString_should_equal_filename() {
            var unit = GlobTemplate.Parse("dotnet/src/Carbonfrost.Commons.Core/{name}/{file}.cs");
            var all = unit.EnumerateFiles();
            var assembly = all.Single(t => t.FileName.EndsWith("AssemblyInfo.cs"));

            Assert.Equal(assembly.ToString(), assembly.FileName);
        }
    }
}
