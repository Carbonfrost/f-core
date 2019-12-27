//
// Copyright 2005, 2006, 2010, 2019 Carbonfrost Systems, Inc.
// (http://carbonfrost.com)
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
using System.Diagnostics;
using System.Threading;

namespace Carbonfrost.Commons.Core {

    public abstract partial class DisposableObject : IDisposable {

        public static readonly IDisposable Null = new NullDisposable();

        // Keeps track of when the object is disposed.  Zero when the object is live, negative for disposal
        private int _isDisposed;

        protected DisposableObject() {
        }

        ~DisposableObject() {
            // Call disposal, only unmanaged resources
            Dispose(false);
        }

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Advanced)]
        protected bool IsDisposed {
            [DebuggerStepThrough]
            get {
                return _isDisposed < 0;
            }
        }

        protected void ThrowIfDisposed() {
            if (IsDisposed) {
                throw Failure.Disposed(this.GetType().ToString());
            }
        }

        protected virtual void Dispose(bool manualDispose) {}

        public static void Dispose(object instance) {
            Safely.Dispose(instance);
        }

        public void Dispose() {
            // Dispose and suppress finalization
            if (_isDisposed == 0) {
                // Only do client disposal the first time
                System.GC.SuppressFinalize(this);
                try {
                    Dispose(true);
                }
                finally {
                    Interlocked.Decrement(ref _isDisposed);
                }
            }
        }

        sealed class NullDisposable : IDisposable {
            public void Dispose() {}
        }
    }
}
