//
// Copyright 2016, 2019 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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

    public class PlatformMacOSXSwversVersionFinderTests {

        [Theory]
        [InlineData("Mac OS X", "10.12", "16A320", "MacOSX/10.12 (Build 16A320; \"macOS Sierra\")")]
        [InlineData("Mac OS X", "10.11", "15A820", "MacOSX/10.11 (Build 15A820; \"Mac OS X El Capitan\")")]
        [InlineData("Mac OS X", "10.15.1", "19B88", "MacOSX/10.15.1 (Build 19B88; \"macOS Catalina\") macOS/10.15.1")]
        [InlineData("macOS", "11.0", "19B88", "macOS/11.0 (Build 19B88; \"macOS Big Sur\"; like MacOSX)")]
        public void ParseResult_should_generate_correct_result(string productName, string productVersion, string buildVersion, string expected) {
            var lines = new string[] {
                string.Format("ProductName:    {0}", productName),
                string.Format("ProductVersion: {0}", productVersion),
                string.Format("BuildVersion:   {0}", buildVersion),
            };
            var unit = new Platform.MacOSXSwversVersionFinder();
            var results = unit.ParseResult(lines).ToList();

            Assert.Equal(expected, string.Join(" ", results));
            Assert.Equal("MacOSX", results[0].PlatformFamily);
        }

        [Fact]
        public void ParseResult_should_return_nothing_empty_lines() {
            var lines = new string[0];
            var unit = new Platform.MacOSXSwversVersionFinder();
            var results = unit.ParseResult(lines).ToList();
            Assert.Empty(results);
        }
    }
}
