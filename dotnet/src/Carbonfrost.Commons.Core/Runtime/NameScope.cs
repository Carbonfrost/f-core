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
using System.Collections.Generic;

namespace Carbonfrost.Commons.Core.Runtime {

    public partial class NameScope : INameScope, IEnumerable<KeyValuePair<string, object>> {

        private readonly IDictionary<string, object> _items = new Dictionary<string, object>(
            StringComparer.OrdinalIgnoreCase);
        private readonly INameScope _parent;

        public static readonly INameScope Empty = new EmptyImpl();

        protected IDictionary<string, object> Items {
            get {
                return _items;
            }
        }

        public INameScope Parent {
            get {
                return _parent;
            }
        }

        public NameScope() {}

        public NameScope(INameScope parent) {
            _parent = parent;
        }

        public static INameScope ReadOnly(INameScope other) {
            if (other == null) {
                throw new ArgumentNullException("other");
            }
            if (other == Empty) {
                return Empty;
            }
            return new ReadOnlyAdapter(other);
        }

        public void Add(string name, object item) {
            RegisterName(name, item);
        }

        public IPropertyProvider AsPropertyProvider() {
            return new NameScopePropertyProvider(this);
        }

        public object FindName(string name) {
            if (name == null) {
                throw new ArgumentNullException("name");
            }
            if (string.IsNullOrEmpty(name)) {
                throw Failure.EmptyString("name");
            }
            var result = _items.GetValueOrDefault(name);
            if (result == null && _parent != null) {
                result = _parent.FindName(name);
            }
            return result;
        }

        public void RegisterName(string name, object item) {
            if (name == null) {
                throw new ArgumentNullException("name");
            }
            if (string.IsNullOrEmpty(name)) {
                throw Failure.EmptyString("name");
            }
            if (item == null) {
                throw new ArgumentNullException("item");
            }
            _items.Add(name, item);
        }

        public void UnregisterName(string name) {
            if (name == null) {
                throw new ArgumentNullException("name");
            }
            if (string.IsNullOrEmpty(name)) {
                throw Failure.EmptyString("name");
            }
            _items.Remove(name);
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() {
            return _items.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}
