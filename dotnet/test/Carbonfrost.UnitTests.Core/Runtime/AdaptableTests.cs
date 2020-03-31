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
using System.ComponentModel;
using System.Reflection;

using Carbonfrost.Commons.Core;
using Carbonfrost.Commons.Core.Runtime;
using Carbonfrost.Commons.Spec;

namespace Carbonfrost.UnitTests.Core.Runtime {

    public class AdaptableTests {

        [Theory]
        [InlineData(typeof(List<int>), "Capacity")]
        public void GetTemplatingMode_should_hide_certain_properties(Type type, string property) {
            var pi = type.GetTypeInfo().GetProperty(property);
            Assert.Equal(TemplatingMode.Hidden, pi.GetTemplatingMode());
        }

        [Fact]
        public void GetDefaultValue_should_be_equal_to_uninitialized() {
            Assert.Null(typeof(string).GetDefaultValue());
            Assert.Equal(0, typeof(int).GetDefaultValue());
            Assert.Equal(default(DateTime), typeof(DateTime).GetDefaultValue());
        }

        class N {
          [DefaultValue("S")]
          public string S { get; set; }
          public int T { get; set; }
          [DefaultValue(typeof(Properties), "hello=world")]
          public Properties U { get; set; }
        }

        [Fact]
        public void GetDefaultValue_should_be_equal_to_value_of_attribute() {
            var value = typeof(N).GetTypeInfo().GetProperty("S").GetDefaultValue();
            Assert.Equal("S", value);
        }

        [Fact]
        public void GetDefaultValue_should_parse_string_if_special_type() {
            var value = typeof(N).GetTypeInfo().GetProperty("U").GetDefaultValue();
            Assert.IsInstanceOf<Properties>(value);
            Assert.Equal("hello=world", value.ToString());
        }

        [Fact]
        public void GetDefaultValue_should_be_equal_to_default_value_of_property_Type() {
            var value = typeof(N).GetTypeInfo().GetProperty("T").GetDefaultValue();
            Assert.Equal(0, value);
        }

        [Fact]
        public void ApplyProperties_should_apply_to_instance_methods() {
            var method = typeof(S).GetMethod("A");
            var props = Properties.FromValue(new { arg0 = "_string_", arg1 = 300 });
            Assert.Equal("instance_string_300", method.ApplyProperties(new S { V = "instance" }, props));
        }

        [Fact]
        public void ApplyProperties_instance_method_should_verify_instance_argument_type() {
            var method = typeof(S).GetMethod("A");
            Assert.Throws<TargetException>(() => method.ApplyProperties(string.Empty, Properties.Empty));
        }

        [Fact]
        public void ApplyProperties_should_apply_to_extension_methods() {
            var method = typeof(S).GetMethod("B");
            var props = Properties.FromValue(new { arg0 = "_string" });
            Assert.Equal("instance_string", method.ApplyProperties(new S { V = "instance" }, props));
        }

        [Fact]
        public void ApplyProperties_requires_matching_argument_thisInstance_on_static_methods() {
            var method = typeof(S).GetMethod("CInvalid");
            var expectedMessage = RuntimeFailure.ThisArgumentIncorrectType(typeof(S)).Message;
            var ex = Record.Exception(() => method.ApplyProperties(new S(), Properties.Empty));
            Assert.NotNull(ex);
            Assert.Equal(expectedMessage, ex.Message);
        }

        [Fact]
        public void ApplyProperties_requires_at_least_one_argument() {
            var method = typeof(S).GetMethod("CInvalidNoArgs");
            var expectedMessage = RuntimeFailure.ApplyPropertiesStaticMethodRequiresArg("method").Message;
            var ex = Record.Exception(() => method.ApplyProperties(new S(), Properties.Empty));
            Assert.NotNull(ex);
            Assert.Equal(expectedMessage, ex.Message);
        }

        [Fact]
        public void GetActivationConstructor_gets_implicit_default_public_constructor() {
            Assert.NotNull(Adaptable.GetActivationConstructor(typeof(S)));
            Assert.Equal(1, typeof(S).GetConstructors(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Length);
        }

        [Theory]
        [InlineData(typeof(IUriContext))]
        [InlineData(typeof(Exception))]
        public void IsServiceType_should_detect_eligible_service_types(Type serviceType) {
            Assert.True(Adaptable.IsServiceType(serviceType));
        }

        [Theory]
        [InlineData(typeof(UriKind), Name = "Enums")]
        [InlineData(typeof(Adaptable), Name = "Static classes")]
        [InlineData(typeof(int), Name = "Primitives")]
        public void IsServiceType_should_detect_ineligible_service_types(Type serviceType) {
            Assert.False(Adaptable.IsServiceType(serviceType));
        }

        [Fact]
        public void TryAdapt_obtains_registered_adapter_type() {
            Properties p = new Properties();
            var ps = p.TryAdapt("StreamingSource");
            var pp = p.TryAdapt("Builder");

            Assert.IsInstanceOf<PropertiesStreamingSource>(ps);
            Assert.Null(pp);
        }

        [Fact]
        public void TryAdapt_will_use_the_adapter_role_corresponding_to_adapter_type() {
            Properties p = new Properties();
            var pp = p.TryAdapt<StreamingSource>();

            Assert.IsInstanceOf<PropertiesStreamingSource>(pp);
        }

        class S {

            public string V { get; set; }

            public string A(string arg0, int arg1) {
                return V + arg0 + arg1;
            }

            public static string B(S instance, string arg0) {
                return instance.V + arg0;
            }

            public static string CInvalid(string arg0) {
                return null;
            }

            public static void CInvalidNoArgs() {}
        }
    }
}
