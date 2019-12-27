//
// Copyright 2015 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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

namespace Carbonfrost.Commons.Core.Runtime {

    // Implements an enumerator where you can add distinct items (or deferrals) and they
    // will be enumerated from the underlying list.

    class BufferEnumerator<T> : IEnumerator<T> {

        // Buffer of items that have been calculated
        private readonly List<T> _buffer;
        private readonly HashSet<T> _bufferUnique;
        private readonly Func<T[]> _lastChance;
        private bool _canTryLastChance = true;
        private int _index;
        private readonly object _sync = new object();

        // Items and deferrals to yield and then buffer
        private readonly Queue<object> _pending = new Queue<object>();

        public T Current {
            get {
                if (_index < 0) {
                    throw Failure.OutsideEnumeration();
                }
                if (_index < _buffer.Count) {
                    return _buffer[_index];
                }

                throw Failure.OutsideEnumeration();
            }
        }

        public BufferEnumerator()
            : this(EqualityComparer<T>.Default, null) {}

        private BufferEnumerator(BufferEnumerator<T> other) {
            _buffer = other._buffer;
            _bufferUnique = other._bufferUnique;
            _pending = other._pending;
            _lastChance = other._lastChance;
            Reset();
        }

        public BufferEnumerator(IEqualityComparer<T> comparer, Func<T[]> lastChance) {
            _buffer = new List<T>();
            _bufferUnique = new HashSet<T>(comparer);
            _lastChance = lastChance;
            Reset();
        }

        public void AddRange(params T[] items) {
            foreach (var o in items) {
                Add(o);
            }
        }

        public void Add(T item) {
            _pending.Enqueue(item);
        }

        public void Add(Func<T> item) {
            _pending.Enqueue(item);
        }

        public BufferEnumerator<T> Clone() {
            return new BufferEnumerator<T>(this);
        }

        public bool MoveNext() {
            _index++;
            if (_index < _buffer.Count) {
                return true;
            }
            object result;
            while (_pending.Count > 0 || TryLastChance()) {
                result = _pending.Dequeue();
                var addOne = Unwrap(result);

                if (!ReferenceEquals(addOne, null) && _bufferUnique.Add(addOne)) {
                    _buffer.Add(addOne);
                    _canTryLastChance = true;
                    return true;
                }
            }

            _index--;
            return false;
        }

        bool TryLastChance() {
            if (_canTryLastChance && _lastChance != null) {
                AddRange(_lastChance());
                _canTryLastChance = false;
                return _pending.Count > 0;
            }

            return false;
        }

        public void Reset() {
            _index = -1;
        }

        T Unwrap(object result) {
            if (result is T) {
                return (T) result;
            }
            return ((Func<T>) result)();
        }

        object System.Collections.IEnumerator.Current {
            get {
                return Current;
            }
        }

        public void Dispose() {
        }
    }
}
