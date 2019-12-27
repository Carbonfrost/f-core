//
// Copyright 2013, 2016 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using System.Collections.ObjectModel;
using System.Reflection;
using Carbonfrost.Commons.Core;
using Carbonfrost.Commons.Core.Runtime;
using Carbonfrost.Commons.Spec;

[assembly: Xmlns("http://ns.example.com/2012", Prefix = "test-sr")]

namespace Carbonfrost.UnitTests.Core.Runtime {

    public class StatusBuilder {
        public int ErrorCode { get; set; }
        public PImmutable Component { get; set; }
        public Version Version { get; set; }
    }

    public class PImmutable {}

    [TemplatingAttribute(TemplatingMode.Copy)]
    public class PTemplatingModeSet {
        public string PreventsBeingImmutable { get; set; }
    }

    [TemplatesAttribute]
    public static class Templates {

        public static readonly Template<StatusBuilder> A = Template.Create((StatusBuilder l) => { l.ErrorCode = 4; });
        public static readonly Template<StatusBuilder> B = Template.Create((StatusBuilder l) => { l.ErrorCode = 5; });

    }

    public class TemplateTests {

        [Fact]
        public void FromName_should_create_from_type_and_qualified_name() {
            Template<StatusBuilder> a = Template.FromName<StatusBuilder>(NamespaceUri.Parse("http://ns.example.com/2012").GetName("A"));
            var sb = a.CreateInstance();

            Assert.Equal(4, sb.ErrorCode);
        }

        [Fact]
        public void GetTemplateNames_should_obtain_names_nominal() {
            IEnumerable<QualifiedName> names = App.GetTemplateNames(typeof(StatusBuilder));
            Assert.Contains(NamespaceUri.Parse("http://ns.example.com/2012") + "A", names);
            Assert.Contains(NamespaceUri.Parse("http://ns.example.com/2012") + "B", names);
        }

        [Fact]
        public void GetTemplateName_should_be_reflexive_from_FromName() {
            var name = NamespaceUri.Parse("http://ns.example.com/2012").GetName("A");
            Template<StatusBuilder> a = Template.FromName<StatusBuilder>(name);
            Assert.Equal(name, Template.GetTemplateName(a));
        }

        [Fact]
        public void FromName_should_create_from_local_name() {
            Template<StatusBuilder> template = Template.FromName<StatusBuilder>("A");
            var sb = template.CreateInstance();
            Assert.Equal(4, sb.ErrorCode);
        }

        [Fact]
        public void Create_initialize_from_instance_nominal() {
            var expected = new PImmutable();
            var t = Template.Create(new StatusBuilder { Component = expected, ErrorCode = 0xdead });

            StatusBuilder sb = new StatusBuilder();
            t.Apply(sb);

            Assert.Equal(0xdead, sb.ErrorCode);
            Assert.Equal(expected, sb.Component);
        }

        [Fact]
        public void Create_initialize_from_instance_uses_CopyFrom() {
            var properties = new Properties {
                { "a", "c" },
                { "b", "d" },
            };

            var t = Template.Create(properties);
            var sb = new Properties();
            t.Apply(sb);

            Assert.Equal("c", sb.GetString("a"));
            Assert.Equal("d", sb.GetString("b"));
        }

        [Fact]
        public void Create_untyped_initializes_from_any_instance() {
            var properties = new Properties {
                { "a", "c" },
                { "b", "d" },
            };

            var sb = (Properties) Template.Copy(properties, new Properties());

            Assert.Equal("c", sb.GetString("a"));
            Assert.Equal("d", sb.GetString("b"));
        }

        [Theory]
        [InlineData(typeof(string))]
        [InlineData(typeof(M))]
        [InlineData(typeof(F))]
        [InlineData(typeof(PTemplatingModeSet))] // Because of TemplatingMode being set
        [InlineData(typeof(ReadOnlyCollection<string>))]
        [InlineData(typeof(IPropertyStore))]
        [InlineData(typeof(object))]
        [InlineData(typeof(Delegate))]
        [InlineData(typeof(MethodInfo))]
        [InlineData(typeof(EventHandler))]
        [InlineData(typeof(char))]
        [InlineData(typeof(Uri))]
        [InlineData(typeof(string[]))]
        [InlineData(typeof(DateTime?))]
        public void IsImmutable_should_be_true_for_immutable_types(Type type) {
            Assert.True(Template.IsImmutable(type),
                        string.Format("Expected to be immutable {0} (mutable property: {1})", type, Template.FirstMutableProperty(type)));
        }

        [Theory]
        [InlineData(typeof(E))]
        [InlineData(typeof(N))]
        [InlineData(typeof(A))]
        [InlineData(typeof(P))]
        [InlineData(typeof(IList<int>))]
        [InlineData(typeof(List<string>))]
        public void IsImmutable_should_be_false_for_mutable_types(Type type) {
            Assert.False(Template.IsImmutable(type), "Expected to be mutable " + type);
        }

        [Fact]
        public void Copy_should_create_a_clone_of_an_object() {
            var properties = new Properties {
                { "a", "c" },
                { "b", "d" },
            };

            var sb = (Properties) Template.Copy(properties);

            Assert.Equal("c", sb.GetString("a"));
            Assert.Equal("d", sb.GetString("b"));
        }

        [Fact]
        public void Create_should_skip_immutable_or_hidden_properties() {
            var e = new M();

            // Because M.C is read-only and an immutable property type,
            // there is no need to access the property when building the
            // template
            var pi = Template.Create(e);
            Assert.Equal(0, e._cAccess);
            Assert.Equal(0, e._hAccess);
        }

        [Fact]
        public void Copy_should_generate_template_that_applies_copy_property() {
            var e = new P();
            var f = (P) Template.Copy(e);
            Assert.Same(e.C, f.C);
        }

        class P {
            public Uri C { get; set;}
        }

        class M {
            internal int _cAccess;
            internal int _hAccess;
            public PImmutable C {
                get {
                    _cAccess++;
                    return null;
                }
            }

            [Templating(TemplatingMode.Hidden)]
            public string H {
                get {
                    _hAccess++;
                    return null;
                }
                set {
                    _hAccess++;
                }
            }
        }

        class N {

            public IList<string> R { get { return null; } }
        }

        public class A {
            public string RO { get { return "r"; } }
            public int Zero { get; set; }
        }

        public class B {
            private readonly Properties _p = new Properties();
            private readonly Properties _q = new Properties();
            public Properties P { get { return _p; } }
            public Properties Q { get { return _q; } }
        }

        [Fact]
        public void Create_should_ignore_read_only_properties() {
            var result = Template.Create(new A());
            Assert.Null(Record.Exception(() => result.Apply(new A())));
        }

        [Fact]
        public void Create_should_implement_a_filter_on_properties() {
            var name = new StatusBuilder {
                ErrorCode = 2,
                Component = new PImmutable(),
            };

            var result = Template.Create(name, pd => pd.Name != "Version")
                .CreateInstance();

            Assert.Null(result.Version);
            Assert.NotNull(result.Component);
        }

        class E {

            private readonly List<string> _items = new List<string>();

            public List<string> Items { get { return _items; } }

            public IList<string> PrivateSet { get; private set; }

            public string S { get; set; }

            public E() {
                PrivateSet = new List<string>();
            }
        }

        class F {

            private readonly IList<string> _items = new List<string>();

            [Templating(TemplatingMode.Hidden)]
            public IList<string> Items { get { return _items; } }

            [Templating(TemplatingMode.Hidden)]
            public string S { get; set; }
        }

        class G {

            private readonly E _e = new E();

            public E E { get { return _e; } }

        }

        class H {

            private readonly List<A> _items = new List<A>();

            public IList<A> Items { get { return _items; } }
        }

        [Fact]
        public void Copy_should_copy_lists_of_items() {
            var e = new E() { Items = { "a", "b", "c" } };
            var to = new E();
            Template.Copy(e, to);

            Assert.Equal(3, to.Items.Count);
        }

        [Fact]
        public void Apply_should_copy_list_contents() {
            var c = new E { Items = { "0", "1" }};
            var d = new E { Items = { "a", "b" }};

            var template = new Template<E>(c);
            template.Apply(d);
            Assert.Equal(new [] { "a", "b", "0", "1" }, d.Items);
        }

        [Fact]
        public void Apply_should_ignore_list_capacities() {
            var result = new E { Items = { "a", "a", "a", "a", } };
            var template = new E { Items = { "a" }, };
            template.Items.Capacity = 1; // this Capacity _should not_ be templated

            Template.Copy(template, result);
            Assert.Equal(5, result.Items.Count);
            Assert.NotEqual(1, result.Items.Capacity);
        }

        [Fact]
        public void Copy_should_ignore_hidden_elements() {
            var e = new F() { Items = { "a", "b", "c" }, S = "ex" };
            var to = new F();
            Template.Copy(e, to);

            Assert.Equal(0, to.Items.Count);
            Assert.Null(to.S);
        }

        public class D {

            public D Other { get; set; }

        }

        [Fact]
        public void Copy_should_prevent_reentrancy() {
            var parent = new D();
            var child = new D { Other = parent };
            parent.Other = child;

            Assert.Null(Record.Exception(() => Template.Copy(child, new D())));
        }

        [Fact]
        public void Copy_should_create_clones_of_templating_objects_in_lists() {
            var source = new H {
                Items = { new A {} }
            };
            var clone = (H) Template.Copy(source);
            Assert.NotSame(clone.Items[0], source.Items[0]);
        }

        [Fact]
        public void Copy_should_treat_defaults_as_skip_commands() {
            var result = new A { Zero = 10 };
            var template = new A { Zero = 0 };

            // Note that Zero=0 implies it won't be overwritten
            Template.Copy(template, result);
            Assert.Equal(10, result.Zero);
        }

        [Fact]
        public void Copy_should_apply_to_read_only_properties() {
            var result = new G();
            var template = new G { E = { Items = { "a" }, PrivateSet = { "b" }, S = "text" } };

            Template.Copy(template, result);
            Assert.Equal("text", result.E.S);
            Assert.Equal(1, result.E.Items.Count);
            Assert.Equal(1, result.E.PrivateSet.Count);

            Assert.Equal("a", result.E.Items[0]);
            Assert.Equal("b", result.E.PrivateSet[0]);
        }

        [Fact]
        public void Copy_should_apply_CopyFrom_on_readonly_properties() {
            var clone = (B) Template.Copy(
                new B {
                    P = { { "s", "s" } }
                });
            Assert.Equal("s", clone.P["s"]);
        }

        [Fact]
        public void GetTemplateType_should_obtain_default_template_instance() {
            Assert.Equal(typeof(Template), Template.GetTemplateType(typeof(Properties)));
        }

    }
}
