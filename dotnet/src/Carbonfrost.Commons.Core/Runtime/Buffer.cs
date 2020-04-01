//
// Copyright 2013, 2019 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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

using System.Collections;
using System.Collections.Generic;

namespace Carbonfrost.Commons.Core.Runtime {

    class Buffer<TElement> : IReadOnlyCollection<TElement> {

        private IEnumerator<TElement> _source;
        private readonly List<TElement> _cache;

        public int Count {
            get {
                MoveToEnd();
                return _cache.Count;
            }
        }

        public Buffer(IEnumerable<TElement> e) {
            // Optimization if already a collection
            if (e is ICollection<TElement> c) {
                _source = null;
                _cache = new List<TElement>(c);
                return;
            }
            _source = e.GetEnumerator();
            _cache = new List<TElement>();
        }

        public void MoveToEnd() {
            while (MoveNext()) {
            }
        }

        private bool MoveNext() {
            if (_source == null) {
                return false;
            }

            lock (_cache) {
                if (_source.MoveNext()) {
                    _cache.Add(_source.Current);
                    OnCacheValue(_source.Current);
                    return true;
                }

                _source = null;
                _cache.TrimExcess();
            }

            OnCacheComplete();
            return false;
        }

        protected virtual void OnCacheComplete() {}
        protected virtual void OnCacheValue(TElement current) {}

        public IEnumerator<TElement> GetEnumerator() {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        struct Enumerator : IEnumerator<TElement> {

            private readonly Buffer<TElement> _source;
            private int _index;

            public Enumerator(Buffer<TElement> source) {
                _source = source;
                _index = -1;
            }

            public TElement Current {
                get {
                    if (_index < 0 || _index >= _source._cache.Count) {
                        throw Failure.OutsideEnumeration();
                    }

                    return _source._cache[_index];
                }
            }

            object IEnumerator.Current {
                get {
                    return Current;
                }
            }

            public void Dispose() {
            }

            public bool MoveNext() {
                _index++;

                if (_source._cache.Count == _index) {
                    return _source.MoveNext();
                }

                return true;
            }

            public void Reset() {
                _index = -1;
            }
        }
    }
}
