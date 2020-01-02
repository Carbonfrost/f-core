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
using System.Collections.Generic;
using System.Reflection;
using Carbonfrost.Commons.Core;
using Carbonfrost.Commons.Core.Runtime;
using Carbonfrost.Commons.Spec;

namespace Carbonfrost.UnitTests.Core.Runtime {

    public class ProviderAttributeTests {

        [Fact]
        public void GetNames_should_include_default_type_name() {
            ProviderAttribute pa = new ProviderAttribute(typeof(StreamContext));
            Assert.Equal(new[] {
                                   typeof(StreamContext).GetQualifiedName()
                               },
                        pa.GetNames(typeof(StreamContext)));
        }

        [Fact]
        public void GetNames_should_generate_names_with_generated_ns() {
            ProviderAttribute pa = new ProviderAttribute(typeof(StreamContext)) {
                Name = "a\t\t\r\nb,c d"
            };

            var ns = typeof(StreamContext).GetQualifiedName().Namespace;
            Assert.Equal(new[] {
                                   ns + "a",
                                   ns + "b",
                                   ns + "c",
                                   ns + "d",
                                   ns + "StreamContext",
                               },
                        pa.GetNames(typeof(StreamContext)));
        }

        [Fact]
        public void GetNames_should_supplement_default_name_generation() {
            ProviderAttribute pa = new CustomProviderAttribute(typeof(StreamContext));

            // Note that StreamContext must always be generated
            var ns = typeof(StreamContext).GetQualifiedName().Namespace;
            Assert.SetEqual(new[] {
                                ns + "StreamContext",
                                ns + "StreamContextular",
                            },
                            pa.GetNames(typeof(StreamContext)),
                            QualifiedNameComparer.IgnoreCaseLocalName);
        }

        [Fact]
        public void GetNames_should_be_distinct_by_ignoring_case() {
            ProviderAttribute pa = new ProviderAttribute(typeof(StreamContext)) {
                Name = "streamContext b b"
            };

            var ns = typeof(StreamContext).GetQualifiedName().Namespace;
            Assert.SetEqual(new[] {
                                ns + "streamContext",
                                ns + "b",
                            },
                            pa.GetNames(typeof(StreamContext)),
                            QualifiedNameComparer.IgnoreCaseLocalName);
        }

        [Fact]
        public void GetNames_works_even_if_GetDefaultProviderNames_returns_empty_string_or_null_ignored() {
            ProviderAttribute pa = new ReturnNullProviderAttribute(typeof(StreamContext)) {
                Name = "streamContext b b"
            };

            var ns = typeof(StreamContext).GetQualifiedName().Namespace;
            Assert.SetEqual(new[] {
                               ns + "streamContext",
                               ns + "b",
                           }, pa.GetNames(typeof(StreamContext)));
        }

        class ReturnNullProviderAttribute : ProviderAttribute {

            public ReturnNullProviderAttribute(Type providerType) : base(providerType) {}

            protected override IEnumerable<string> GetDefaultProviderNames(Type type) {
                yield return null;
                yield return "";
            }
        }

        class CustomProviderAttribute : ProviderAttribute {

            public CustomProviderAttribute(Type providerType) : base(providerType) {}

            protected override IEnumerable<string> GetDefaultProviderNames(Type type) {
                yield return type.Name + "ular";
            }
        }
    }
}
