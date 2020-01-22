//
// Copyright 2013, 2019 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using Carbonfrost.Commons.Core.Runtime;
using Carbonfrost.Commons.Spec;

namespace Carbonfrost.UnitTests.Core.Runtime {

    public class ActivationProviderAttributeTests {

        [Fact]
        public void Constructor_should_require_correct_type() {
            Assert.Throws<ArgumentException>(() => {
                new ActivationProviderAttribute(typeof(ActivationProviderAttributeTests));
            });
        }

        [Fact]
        public void Constructor_should_require_correct_type_string_argument() {
            Assert.Throws<ArgumentException>(() => {
                new ActivationProviderAttribute("System.String");
            });
        }

        [Fact]
        public void Constructor_should_convert_from_string() {
            Assert.Equal(
                typeof(UriContextActivationProvider),
                new ActivationProviderAttribute(
                    "Carbonfrost.Commons.Core.Runtime.UriContextActivationProvider, Carbonfrost.Commons.Core, PublicKeyToken=d09aaf34527fe3e6"
                ).AdapterType
            );
        }
    }
}
