//
// Copyright 2014 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using System.Linq;
using System.Reflection;
using Carbonfrost.Commons.Core.Runtime;
using Carbonfrost.Commons.Spec;

namespace Carbonfrost.UnitTests.Core {

    public class ReflectionPropertyProviderUsingIndexerTests {

        private class A {

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
            }
        }

        [Fact]
        public void GetProperty_should_use_indexer_nominal() {
            var pp = PropertyProvider.FromValue(new A());
            Assert.Equal("U", pp.GetProperty("R"));
        }

        [Fact]
        public void TryGetProperty_should_return_false_on_null() {
            var pp = PropertyProvider.FromValue(new A());
            Assert.Null(pp.GetProperty("ZZZ"));
        }

        [Fact]
        public void TryGetProperty_should_return_false_on_KeyNotFoundException() {
            var pp = PropertyProvider.FromValue(new A());
            object dummy;
            Assert.False(pp.TryGetProperty("S", out dummy));
        }

        [Fact]
        public void TryGetProperty_should_surface_certain_apparent_exceptions() {
            var pp = PropertyProvider.FromValue(new A());
            object dummy;

            // Throws on exceptions which aren't ArgumentException-derived
            Assert.Throws<TargetInvocationException>(() => pp.TryGetProperty("U", out dummy));

            Assert.False(pp.TryGetProperty("T", out dummy));
        }
    }
}

