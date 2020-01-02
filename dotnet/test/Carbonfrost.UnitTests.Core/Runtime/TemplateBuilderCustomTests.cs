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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Carbonfrost.Commons.Core.Runtime;
using Carbonfrost.Commons.Spec;

namespace Carbonfrost.UnitTests.Core.Runtime {

    public class TemplateBuilderCustomTests {

        class E {
            private readonly List<E> _items = new List<E>();
            public string A { get; set; }
            public E T { get; set; }
            public IList<E> Items { get { return _items; } }
        }

        class E1 : E {}

        class D : IConfigurable<D> {
            private readonly Properties _c = new Properties();
            private readonly List<D> _configurations = new List<D>();
            public string A { get; set; }
            public int B { get; set; }
            public Properties Properties { get { return _c; } }
            public IList<D> Configurations {
                get { return _configurations; }
            }
        }

        class C {
            public D D { get; set; }
        }

        interface IConfigurable<T> {
            IList<T> Configurations { get; }
        }

        class ConfigurationTemplate : Template {

            public ConfigurationTemplate(object obj) : base(obj, new ConfigurationTemplateBuilder()) {}

            internal static bool IsConfigurable(Type type) {
                foreach (var i in type.GetTypeInfo().GetInterfaces()) {
                    var inf = i.GetTypeInfo();
                    if (inf.IsGenericType && inf.GetGenericTypeDefinition() == typeof(IConfigurable<>)) {
                        return true;
                    }
                }
                return false;
            }

            private static bool IsClosedGenericOf(Type self, Type genericDefinition) {
                return self.GetTypeInfo().IsGenericType
                    && self.GetTypeInfo().GetGenericTypeDefinition() == genericDefinition;
            }

            internal static IEnumerable GetConfigurations(object any) {
                var inf = any.GetType().GetTypeInfo().GetInterfaces()
                    .FirstOrDefault(t => IsClosedGenericOf(t, typeof(IConfigurable<>)));

                var prop = inf.GetTypeInfo().GetProperty("Configurations");
                return (IEnumerable) prop.GetValue(any);
            }

            class ConfigurationTemplateBuilder : TemplateBuilder {

                protected override void CopyPropertyOverride() {
                    base.CopyPropertyOverride();

                    // Apply configurations to properties which are IConfigurable
                    var type = CurrentContext.Property.PropertyType;
                    if (IsConfigurable(type)) {
                        var configs = GetConfigurations(CurrentContext.PropertyValue);
                        foreach (var c in configs) {
                            CopyObject(c);
                        }
                    }
                }

                protected override void CopyObjectOverride() {
                    var type = CurrentContext.Object.GetType();
                    if (IsConfigurable(type)) {
                        if (TryCopyFromMethod()) {
                        } else {
                            TryCopyContent();
                            // all properties except Configuration
                            foreach (var p in GetTemplateProperties(CurrentContext.Object)) {
                                if (p.Name == "Configurations") {
                                    continue;
                                }
                                CopyProperty(p);
                            }
                        }
                        var configs = GetConfigurations(CurrentContext.Object);
                        foreach (var c in configs) {
                            CopyObject(c);
                        }

                    } else {
                        // copy all properties
                        base.CopyObjectOverride();
                    }
                }
            }
        }

        [Fact]
        public void GetConfigurations_should_find_the_collection() {
            var d = new D();
            Assert.Same(d.Configurations, ConfigurationTemplate.GetConfigurations(d));
        }

        [Fact]
        public void IsConfigurable_should_apply() {
            Assert.True(ConfigurationTemplate.IsConfigurable(typeof(D)));
            Assert.False(ConfigurationTemplate.IsConfigurable(typeof(C)));
        }

        [Fact]
        public void Apply_should_invoke_custom_nested_templates() {
            var d = new D {
                Properties = { { "1", "10" }, { "2", "20" } },
                A = "nope",
                B = 80,
                Configurations = {
                    new D {
                        A = "yes",
                        Properties = { { "3", "30" } },
                    },
                    new D {
                        B = 7800,
                    }
                }
            };
            var result = new D();
            new ConfigurationTemplate(d).Apply(result);
            Assert.Equal("yes", result.A);
            Assert.Equal(7800, result.B);
            Assert.Equal(3, result.Properties.Count);
            Assert.Equal("30", result.Properties["3"]);
        }

        [Fact]
        public void Apply_should_invoke_custom_nested_templates_recurse() {
            // Because C.D is _settable_, the template would normally write
            // to it.  But in this template builder, we still want it to template.
            var c = new C {
                D = new D {
                    A = "nope",
                    B = 30,
                    Configurations = {
                      new D { A = "x" }
                    }
                },
            };
            var result = new C();
            new ConfigurationTemplate(c).Apply(result);
            Assert.Equal("x", result.D.A);
            Assert.Equal(30, result.D.B);
        }

        [Fact]
        public void Apply_Create_should_be_nonreentrant() {
            var e = new E {
                A = "recursion",
            };
            e.T = e; // references self
            var result = new E();
            Template.Create(e).Apply(result);
            Assert.Equal("recursion", e.A);
        }

        [Fact]
        public void Apply_Create_should_be_nonrentrant_in_list() {
            var e = new E {
                A = "recursion",
            };
            e.Items.Add(e); // references self
            var result = new E();
            Template.Create(e).Apply(result);
            Assert.Equal("recursion", e.A);
            Assert.Empty(result.Items);
        }

    }
}
