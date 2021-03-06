//
// Copyright 2015, 2016, 2020 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using Carbonfrost.Commons.Spec;
using _Safely = Carbonfrost.Commons.Core.Safely;

namespace Carbonfrost.UnitTests.Core {

    public class SafelyTests {

        class C : IDisposable {

            public event EventHandler Disposed;
            public bool DisposedCalled;

            public void Dispose() {
                var handler = Disposed;
                if (handler != null) {
                    handler(this, EventArgs.Empty);
                }
                DisposedCalled = true;
            }
        }

        [Fact]
        public void Dispose_should_be_called() {
            var c = new C();
            _Safely.Dispose(c);
            Assert.True(c.DisposedCalled);
        }

        [Fact]
        public void OnDisposed_should_call_event_on_disposal() {
            var c = new C();
            int count = 0;
            _Safely.OnDisposed(c, (_sender,_e) => count++);
            c.Dispose();

            Assert.Equal(1, count);
        }

        [Fact]
        public void OnDisposed_should_fire_only_once() {
            // In other words, if the event is fired, we should fire handler only once
            var c = new C();
            int count = 0;
            _Safely.OnDisposed(c, (_sender,_e) => count++);
            c.Dispose();
            c.Dispose();

            Assert.Equal(1, count);
        }
    }
}


