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
using System.Collections.Generic;
using System.Linq;
using Carbonfrost.Commons.Core;
using Carbonfrost.Commons.Spec;

namespace Carbonfrost.UnitTests.Core {

    public class NamespaceFilterTests {

        static string[] NAMES =  {
            "System",
            "System.Collections",
            "System.Collections.Generic",
            "System.Linq",
            "System.Linq.Expressions",
            "Carbonfrost.Commons.Core",
            "Carbonfrost.Commons.ComponentModel.PropertyTrees.Expressions"
        };

        [Fact]
        public void Filter_should_apply_reflexively_to_wildcard_nominal() {
            Assert.Equal(NAMES, FilterNamespaces(NAMES, "*").ToArray());
        }

        [Fact]
        public void Filter_should_apply_to_literal_name() {
            Assert.SetEqual(new string[] {
                                "System"
                            }, FilterNamespaces(NAMES, "System"));
        }

        [Fact]
        public void Filter_should_apply_to_prefix_wildcard() {
            string[] systemNames =  {
                "System",
                "System.Collections",
                "System.Collections.Generic",
                "System.Linq",
                "System.Linq.Expressions"
            };
            Assert.SetEqual(systemNames, FilterNamespaces(NAMES, "System.*"));
        }

        [Fact]
        public void Filter_should_apply_to_suffix_wildcard() {
            string[] expressionNames =  {
                "System.Linq.Expressions",
                "Carbonfrost.Commons.ComponentModel.PropertyTrees.Expressions"
            };
            Assert.SetEqual(expressionNames, FilterNamespaces(expressionNames, "*.Expressions").ToArray());
        }

        [Fact]
        public void Filter_should_match_ns_names_when_wildcard_embedded() {
            string[] expressionNames2 =  {
                "System.Linq.Expressions"
            };
            Assert.SetEqual(expressionNames2, FilterNamespaces(NAMES, "System.*.Expressions"));
        }

        static IEnumerable<string> FilterNamespaces(IEnumerable<string> names, string pattern) {
            return new NamespaceFilter(pattern).Filter(names);
        }
    }
}




