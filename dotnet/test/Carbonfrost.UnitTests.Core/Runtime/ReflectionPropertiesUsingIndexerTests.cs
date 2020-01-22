//
// Copyright 2020 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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

using System.Reflection;
using Carbonfrost.Commons.Core.Runtime;
using Carbonfrost.Commons.Spec;

namespace Carbonfrost.UnitTests.Core.Runtime {

    public class ReflectionPropertiesUsingIndexerTests {

        private class A : IEnumerable<KeyValuePair<string, object>> {

            public readonly IDictionary<string, object> SetItems = new Dictionary<string, object>();

            public object this[string key] {
                get {
                    if (key == "R")
                        return "U";
                    else if (key == "S")
                        throw new KeyNotFoundException();
                    else if (key == "T")
                        throw new ArgumentException();
                    else if (key == "U")
                        throw new InvalidCastException();
                    else
                        return null;
                }
                set {
                    if (key == "S")
                        throw new KeyNotFoundException();
                    else if (key == "T")
                        throw new ArgumentException();
                    else if (key == "U")
                        throw new InvalidCastException();
                    SetItems[key] = value;
                }
            }

            public IEnumerator<KeyValuePair<string, object>> GetEnumerator() {
                return new Enumerator();
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return GetEnumerator();
            }

            internal struct Enumerator : IEnumerator<KeyValuePair<string, object>> {
                public KeyValuePair<string, object> Current {
                    get {
                        return default;
                    }
                }

                object IEnumerator.Current {
                    get {
                        return Current;
                    }
                }

                public void Dispose() {}
                public bool MoveNext() {
                    return false;
                }
                public void Reset() {}
            }
        }

        private class B : IEnumerable<KeyValuePair<string, Uri>> {

            public readonly List<KeyValuePair<string, Uri>> Items = new List<KeyValuePair<string, Uri>> {
                KeyValuePair.Create("Hello", new Uri("https://example.com")),
            };

            public IEnumerator<KeyValuePair<string, Uri>> GetEnumerator() {
                return Items.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return GetEnumerator();
            }
        }

        [Fact]
        public void GetProperty_should_use_indexer_nominal() {
            var pp = Properties.FromValue(new A());
            Assert.Equal("U", pp.GetProperty("R"));
        }

        [Fact]
        public void TryGetProperty_should_return_false_on_null() {
            var pp = Properties.FromValue(new A());
            Assert.Null(pp.GetProperty("ZZZ"));
        }

        [Fact]
        public void TryGetProperty_should_return_false_on_KeyNotFoundException() {
            var pp = Properties.FromValue(new A());
            object dummy;
            Assert.False(pp.TryGetProperty("S", out dummy));
        }

        [Fact]
        public void TryGetProperty_should_surface_certain_apparent_exceptions() {
            var pp = Properties.FromValue(new A());
            object dummy;

            // Throws on exceptions which aren't ArgumentException-derived
            Assert.Throws<TargetInvocationException>(() => pp.TryGetProperty("U", out dummy));

            Assert.False(pp.TryGetProperty("T", out dummy));
        }

        [Fact]
        public void SetProperty_should_use_indexer_nominal() {
            var a = new A();
            var pp = Properties.FromValue(a);
            pp.SetProperty("A", "1");
            Assert.ContainsKeyWithValue("A", (object) "1", a.SetItems);
        }

        [Fact]
        public void TrySetProperty_should_return_true_nominal() {
            var a = new A();
            var pp = Properties.FromValue(a);

            Assert.True(pp.TrySetProperty("A", "1"));
            Assert.ContainsKeyWithValue("A", (object) "1", a.SetItems);
        }

        [Fact]
        public void TrySetProperty_should_return_false_on_KeyNotFoundException() {
            var pp = Properties.FromValue(new A());

            Assert.False(pp.TrySetProperty("S", "_"));
        }

        [Fact]
        public void TrySetProperty_should_surface_certain_apparent_exceptions() {
            var pp = Properties.FromValue(new A());

            // Throws on exceptions which aren't ArgumentException-derived
            Assert.Throws<TargetInvocationException>(() => pp.TrySetProperty("U", "_"));

            Assert.False(pp.TrySetProperty("T", "_"));
        }

        [Fact]
        public void GetEnumerator_should_delegate() {
            var pp = Properties.FromValue(new A());
            Assert.IsInstanceOf<A.Enumerator>(
                pp.GetEnumerator()
            );
        }
    }
}

