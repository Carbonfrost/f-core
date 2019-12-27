//
// Copyright 2019 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using Carbonfrost.Commons.Core;
using Carbonfrost.Commons.Spec;

namespace Carbonfrost.UnitTests.Core {

    public class NamespaceUriTests : TestClass {

        [Theory]
        [InlineData("T", "tag:www.w3.org,2000:/xmlns/")]
        [InlineData("G", NamespaceUri.XmlnsPrefixNamespace)]
        [InlineData("F", NamespaceUri.XmlnsPrefixNamespace)]
        [InlineData("B", "{http://www.w3.org/2000/xmlns/}")]
        public void ToString_should_have_format_strings(string format, string expected) {
            Assert.Equal(expected, NamespaceUri.Xmlns.ToString(format));
            Assert.Equal(expected, NamespaceUri.Xmlns.ToString(format.ToLowerInvariant()));
        }

        [Theory]
        [InlineData(
            "http://ns.example.com/2012-01-01/etc",
            "tag:ns.example.com,2012-01-01:/etc",
            Name = "Long date"
        )]
        [InlineData(
            "http://ns.example.com/2012-01-01",
            "tag:ns.example.com,2012-01-01:/",
            Name = "Rooted")]
        [InlineData(
            "http://ns.example.com/2012-01/etc",
            "tag:ns.example.com,2012-01:/etc",
            Name = "Year and month")]
        public void ToString_should_support_various_tag_formats(string ns, string expected) {
            NamespaceUri nu = NamespaceUri.Parse(ns);
            Assert.Equal(expected, nu.ToString("t"));
        }
    }
}
