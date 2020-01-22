//
// Copyright 2013, 2016, 2019 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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

    public class ActivationTests {

        struct UriContext : IUriContext {
            private Uri uri;

            public UriContext(string uri) {
                this.uri = new Uri(uri);
            }
            public Uri BaseUri {
                get { return this.uri; }
                set { }
            }
        }

        class PHasUriContext : IUriContext {
            public Uri BaseUri { get; set; }
        }


        class B {

            private int c;
            public string CSetBy;

            public B(int a, int c) {
                this.A = a;
                this.c = c;
                CSetBy = "parameter";
            }

            public int A { get; set; }
            public int C {
                get { return c; }
                set {
                    this.c = value;
                    CSetBy = "property";
                }
            }
        }

        class K {

            public int A { get; set; }
            public bool B { get; set; }
        }

        class I {
            public string A { get; set; }
            public virtual string B { get; set; }
        }

        class J : I {
            public new string A { get; set; }
            public override string B { get; set; }
        }

        class C {

            private B b;
            private PHasUriContext a;

            public B B { get { return b; } }
            public PHasUriContext A { get { return a; } }

            [ActivationConstructor]
            public C(B b, PHasUriContext a) {
                if (b == null) {
                    throw new ArgumentNullException("b");
                }
                if (a == null) {
                    throw new ArgumentNullException("a");
                }

                this.b = b;
                this.a = a;
            }
        }

        class PHasReadOnlyProperty {
            public string ReadOnly { get { return null; } }
        }

        class PHasServiceProvider {
            internal IServiceProvider captured;

            public PHasServiceProvider() {
                captured = ServiceProvider.Current;
            }
        }


        [Fact]
        public void CreateInstance_should_cause_parameters_and_properties_to_activate_once() {
            IDictionary<string, object> values = new Dictionary<string, object>();
            values.Add("c", 50);

            // C should be activated once via parameter, not property
            B b = Activation.CreateInstance<B>(values);
            Assert.Equal("parameter", b.CSetBy);
        }

        [Fact]
        public void CreateInstance_should_activate_using_array() {
            B b = Activation.CreateInstance<B>(Properties.FromArray(4, 20));
            Assert.Equal(4, b.A);
            Assert.Equal(20, b.C);
        }

        // TODO Tests: - missing ctor args, - optonal ctor args, - duplicated key-value pair

        [Fact]
        public void CreateInstance_should_apply_uri_context() {
            ServiceContainer sc = new ServiceContainer();
            sc.AddService(typeof(IUriContext), new UriContext("http://carbonfrost.com"));

            var a = Activation.CreateInstance<PHasUriContext>((IServiceProvider) sc);
            Assert.Equal(new Uri("http://carbonfrost.com"), a.BaseUri);
        }

        [Fact]
        public void CreateInstance_should_resolve_qualified_name() {
            Assert.IsInstanceOf(typeof(Properties),
                Activation.CreateInstance(QualifiedName.Create(Xmlns.Core2008, "Properties"))
            );

            Assert.IsInstanceOf(typeof(Properties),
                Activation.CreateInstance<Properties>(QualifiedName.Create(Xmlns.Core2008, "Properties"))
            );
        }

        [Fact]
        public void FromFile_should_apply_text_conversion() {
            var actual = (Properties) Activation.FromFile(typeof(Properties), TestContext.Current.GetFullPath("alpha.properties"));
            Assert.Equal("bar", actual.GetProperty("Foo"));
        }

        [Fact]
        public void CreateInstanceOfT_nominal() {
            Assert.IsInstanceOf(typeof(Properties),
                Activation.CreateInstance<Properties>()
            );
        }

        [Fact]
        public void CreateInstance_should_obtain_service_provider_current() {
            ServiceContainer sc = new ServiceContainer();

            var e = Activation.CreateInstance<PHasServiceProvider>((IServiceProvider) sc);
            Assert.Same(sc, e.captured);
        }

        [Fact]
        public void FromProvider_should_create_provider_instance_and_initialize() {
            var props = new Properties { { "a", "s" } };
            var pro = Activation.FromProvider<TestProvider>("a", props);
            Assert.IsInstanceOf<ATestProvider>(pro);
            Assert.Equal("s", ((ATestProvider) pro).A);
        }

        [Fact]
        public void CreateInstance_parameter_matching_is_case_insensitive() {
            C c = (C) Activation.CreateInstance(typeof(C), Properties.FromValue(new { A = new PHasUriContext(), B = new B(4, 4) }), null, null);
        }

        [Fact]
        public void CreateInstance_should_ignore_readonly_properties() {
            var d = (PHasReadOnlyProperty) Activation.CreateInstance(typeof(PHasReadOnlyProperty), Properties.FromValue(new { ReadOnly = "b" }), null, null);
            Assert.Null(d.ReadOnly);
        }

        [Fact]
        public void CreateInstance_should_ignore_duplicated_properties_first_wins() {
            var props = new [] {
                new KeyValuePair<string, object>("A", 123),
                new KeyValuePair<string, object>("A", 420),
            };
            K d = (K) Activation.CreateInstance(typeof(K), props, null, null);
            Assert.Equal(123, d.A);
        }

        [Fact]
        public void CreateInstance_should_apply_to_shadowed_or_derived_properties_first_and_only() {
            var props = new [] {
                new KeyValuePair<string, object>("A", "a"),
                new KeyValuePair<string, object>("B", "b"),
            };
            Template.GetProperties(typeof(J));
            J d = (J) Activation.CreateInstance(typeof(J), props, null, null);

            Assert.Equal("a", d.A);
            Assert.Null(((I) d).A);
            Assert.Equal("b", d.B);
        }

        [Fact]
        public void Initialize_should_apply_type_conversions_from_string() {
            var props = new [] {
                new KeyValuePair<string, object>("A", "420"),
                new KeyValuePair<string, object>("B", "true"),
            };
            var one = new K();
            var t = one.GetType().GetProperties();
            Assert.NotEmpty(t);
            Activation.Initialize(one, props);

            Assert.Equal(420, one.A);
            Assert.True(one.B);
        }

        [Fact]
        public void Initialize_should_merge_collections() {
            var c = new L { Items = { "0", "1" }};
            var d = new L { Items = { "a", "b" }};

            Activation.Initialize(d, Properties.FromValue(c));
            Assert.Equal(new [] { "a", "b", "0", "1" }, d.Items);
        }

        [Fact]
        public void Initialize_should_copy_from_nominals() {
            Assume.True(Template.IsImmutable(typeof(PImmutable)));
            var asm = new PImmutable();

            Dictionary<string, object> values =  new Dictionary<string, object>() {
                { "Component", asm },
                { "ErrorCode", 0xdead },
            };

            StatusBuilder sb = new StatusBuilder();
            Activation.Initialize(sb, values);

            Assert.Equal(0xdead, sb.ErrorCode);
            Assert.Equal(asm, sb.Component);
        }

        class L {

            private readonly List<string> _items = new List<string>();

            public List<string> Items { get { return _items; } }

            public IList<string> PrivateSet { get; private set; }

            public string S { get; set; }

            public L() {
                PrivateSet = new List<string>();
            }
        }

        class DumbActivationFactory : IActivationFactory {
            public bool CreateInstanceWasCalled;
            public object CreateInstance(Type type,
                                         IEnumerable<KeyValuePair<string, object>> values,
                                         IServiceProvider serviceProvider, params Attribute[] attributes) {
                CreateInstanceWasCalled = true;
                return null;
            }
        }

        [Fact]
        public void CreateInstance_will_request_service_provider_activation_factory() {
            var factory = new DumbActivationFactory();
            Activation.CreateInstance(typeof(K), serviceProvider: ServiceProvider.FromValue(factory));
            Assert.True(factory.CreateInstanceWasCalled);
        }

        [Builder(typeof(ABuilder))]
        class PHasABuilder {
            public bool ViaBuilder;
        }

        class ABuilder {

            public PHasABuilder Build(IServiceProvider serviceProvider) {
                return new PHasABuilder { ViaBuilder = true };
            }
        }

        [Fact]
        public void Build_should_use_builder_when_available() {
            var a = Activation.Build<PHasABuilder>();
            Assert.True(a.ViaBuilder);
        }

        [Fact]
        public void Build_should_work_even_when_builder_not_available() {
            Assert.DoesNotThrow(() => {
                Activation.Build<K>();
            });
        }

        // TODO Check non-reentrancy in activation providers
    }
}
