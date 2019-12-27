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
using System.Linq;

namespace Carbonfrost.Commons.Core.Runtime {

    class BufferDictionary<TKey, TValue> : Buffer<KeyValuePair<TKey, TValue>>, IReadOnlyDictionary<TKey, TValue> {

        private readonly Dictionary<TKey, TValue> _cache = new Dictionary<TKey, TValue>();

        public BufferDictionary(IEnumerable<KeyValuePair<TKey, TValue>> e) : base(e) {
        }

        public bool ContainsKey(TKey key) {
            return _cache.ContainsKey(key) || Keys.Contains(key);
        }

        public bool TryGetValue(TKey key, out TValue value) {
            ContainsKey(key);
            return _cache.TryGetValue(key, out value);
        }

        public TValue this[TKey key] {
            get {
                ContainsKey(key);
                return _cache[key];
            }
        }

        // TODO Allow these to be computed without buffering (performance)

        public IEnumerable<TKey> Keys {
            get {
                MoveToEnd();
                return _cache.Keys;
            }
        }

        public IEnumerable<TValue> Values {
            get {
                MoveToEnd();
                return _cache.Values;
            }
        }

        protected override void OnCacheValue(KeyValuePair<TKey, TValue> current) {
            _cache.Add(current.Key, current.Value);
        }
    }
}


