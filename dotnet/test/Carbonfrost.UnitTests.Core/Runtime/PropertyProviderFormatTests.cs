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
using Carbonfrost.Commons.Core.Runtime;
using Carbonfrost.Commons.Spec;

namespace Carbonfrost.UnitTests.Core {

    public class PropertyProviderFormatTests {

        [Fact]
        public void Parse_should_handle_literal_nominal() {
            var f = PropertyProviderFormat.Parse("just a literal");
            Assert.Equal("just a literal", f.ToString());
        }

        [Theory]
        [InlineData("$expr literal")]
        [InlineData("${expr} literal")]
        public void Parse_should_handle_expansions(string text) {
            var f = PropertyProviderFormat.Parse(text);
            Assert.Equal("${expr} literal", f.ToString());
        }

        [Theory]
        [InlineData("$AllowedIdentifier", "AllowedIdentifier")]
        [InlineData("$aws:Allowed.Identifier.Do_ts", "aws:Allowed.Identifier.Do_ts")]
        public void Parse_should_handle_inline_expansions(string text, string id) {
            var f = PropertyProviderFormat.Parse(text);
            Assert.Equal("${" + id + "}", f.ToString());
        }

        [Fact]
        public void Parse_should_escape_double_dollar() {
            var f = PropertyProviderFormat.Parse("$$");
            Assert.Equal("$$", f.ToString());
        }

        [Theory]
        [InlineData("$ dollar", "$$ dollar")]
        [InlineData("ending $", "ending $$")]
        public void Parse_should_handle_unescaped_dollar_sign(string text, string canonical) {
            // gets doubled in the canonical representation
            var f = PropertyProviderFormat.Parse(text);
            Assert.Equal(canonical, f.ToString());
        }

        [Theory]
        [InlineData("${unclosed_brace")]
        [InlineData("also unclosed ${")]
        [InlineData("empty ${}")]
        [InlineData("${spaces not allowed in key}")]
        public void TryParse_should_return_false_on_malformed(string text) {
            PropertyProviderFormat s;
            Assert.False(PropertyProviderFormat.TryParse(text, out s));
        }

        [Fact]
        public void Format_should_fill_nominal() {
            var f = PropertyProviderFormat.Parse("Hello, ${planet}");
            var pp = PropertyProvider.FromValue(new {
                                                    planet = "Phazon",
                                                });
            Assert.Equal("Hello, Phazon", f.Format(pp));
        }

        [Fact]
        public void Format_should_invoke_delegates() {
            var f = PropertyProviderFormat.Parse("Hello, ${planet}");
            Func<string> thunk = () => "Phazon";
            var pp = PropertyProvider.FromValue(new {
                                                    planet = thunk,
                                                });
            Assert.Equal("Hello, Phazon", f.Format(pp));
        }

        [Fact]
        public void Format_should_invoke_delegates_with_arg() {
            var f = PropertyProviderFormat.Parse("Hello, ${Planet}");
            Func<string, string> thunk2 = k => (k + " Phazon");
            var pp = PropertyProvider.FromValue(new {
                                                    planet = thunk2,
                                                });
            Assert.Equal("Hello, Planet Phazon", f.Format(pp));
        }

    }
}
