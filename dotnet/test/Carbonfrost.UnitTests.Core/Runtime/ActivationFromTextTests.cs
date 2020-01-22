//
// Copyright 2019, 2020 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using System.Globalization;
using System.Reflection;
using Carbonfrost.Commons.Core;
using Carbonfrost.Commons.Core.Runtime;
using Carbonfrost.Commons.Spec;

namespace Carbonfrost.UnitTests.Core.Runtime {

    public class ActivationFromTextTests : TestClass {

        [Fact]
        public void FromText_should_parse_URI() {
            Assert.Equal(
                new Uri("https://example.com"),
                Activation.FromText<Uri>("https://example.com")
            );
        }

        [Fact]
        public void FromText_should_parse_AssemblyName() {
            Assert.Equal(
                "System, Version=8.0.0.0",
                Activation.FromText<AssemblyName>("System, Version=8.0.0.0").ToString()
            );
        }

        [Fact]
        public void FromText_should_throw_when_cannot_parse() {
            Expect(() => {
                Activation.FromText<Uri>(null);
            }).Will.Throw();
        }

        [Fact]
        public void FromText_should_convert_string() {
            Assert.Same(
                "hello",
                Activation.FromText<string>("hello")
            );
        }

        [Fact]
        public void FromText_should_apply_parse_method() {
            Assert.Equal(
                123,
                Activation.FromText<int>("123")
            );
        }

        [Fact]
        public void FromText_should_bind_parse_method_with_args() {
            var actual = Activation.FromText<PHasParseMethod>("123");
            Assert.True(actual._parsed);
        }

        [Fact]
        public void FromText_should_bind_service_provider_from_parse_method() {
            IServiceProvider expectedServiceProvider = ServiceProvider.Root;

            var actual = Activation.FromText<PHasParseMethod>("123");
            Assert.Same(expectedServiceProvider, actual._serviceProvider);
        }

        [Fact]
        public void FromText_should_bind_culture_from_parse_method() {
            CultureInfo expectedCulture = CultureInfo.CurrentCulture;

            var actual = Activation.FromText<PHasParseMethod>("123", CultureInfo.CurrentCulture);
            Assert.Same(expectedCulture, actual._culture);
        }

        [Fact]
        public void FromText_should_use_current_culture_by_default() {
            var originalCulture = CultureInfo.CurrentCulture;
            try {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");

                var actual = Activation.FromText<double>("123,45");
                Assert.Equal(123.45, actual);
            }
            finally {
                CultureInfo.CurrentCulture = originalCulture;
            }
        }

        [Fact]
        public void FromText_should_use_longest_parse_method() {
            var actual = Activation.FromText<PHasTwoParseMethods>("");
            Assert.True(actual.LongParseWasCalled);
            Assert.False(actual.ShortParseWasCalled);
        }

        [Fact]
        public void FromText_should_apply_streaming_source_conversion() {
            var actual = Activation.FromText<PHasStreamingSource>("");
            Assert.True(actual.UsedStreamingSource);
        }

        [Fact]
        public void FromText_should_throw_when_no_conversion() {
            Assert.Throws<NotSupportedException>(
                () => Activation.FromText<object>("")
            );
        }

        struct PHasParseMethod {
            internal readonly bool _parsed;
            internal readonly IServiceProvider _serviceProvider;
            internal readonly CultureInfo _culture;

            public PHasParseMethod(bool parsed, IServiceProvider sp, CultureInfo cu) {
                _parsed = parsed;
                _serviceProvider = sp;
                _culture = cu;
            }

            public static PHasParseMethod Parse(string text, IServiceProvider s, CultureInfo cu) {
                return new PHasParseMethod(true, s, cu);
            }
        }

        struct PHasTwoParseMethods {
            public readonly bool LongParseWasCalled;
            public readonly bool ShortParseWasCalled;

            public PHasTwoParseMethods(bool longCalled, bool shortCalled) {
                LongParseWasCalled = longCalled;
                ShortParseWasCalled = shortCalled;
            }

            public static PHasTwoParseMethods Parse(string text) {
                return new PHasTwoParseMethods(false, true);
            }

            public static PHasTwoParseMethods Parse(string text, IServiceProvider s, CultureInfo cu) {
                return new PHasTwoParseMethods(true, false);
            }
        }

        [StreamingSource(typeof(PHasStreamingSource.Impl))]
        struct PHasStreamingSource {
            public readonly bool UsedStreamingSource;

            public PHasStreamingSource(bool usedStreamingSource) {
                UsedStreamingSource = usedStreamingSource;
            }

            class Impl : StreamingSource {

                public override object Load(StreamContext s, Type t) {
                    return new PHasStreamingSource(true);
                }

                public override void Save(StreamContext outputTarget, object value) {
                }
            }
        }
    }
}
