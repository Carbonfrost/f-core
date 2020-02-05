//
// Copyright 2005, 2006, 2010, 2020 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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

namespace Carbonfrost.Commons.Core.Runtime {

    internal sealed class ReflectionPropertiesUsingIndexer : PropertiesImpl {

        private readonly object _objectContext;
        private readonly PropertyInfo _indexer;
        private readonly Lazy<IEnumerator<KeyValuePair<string, object>>> _getEnumerator;

        public ReflectionPropertiesUsingIndexer(object objectContext, PropertyInfo indexer) : base(objectContext) {
            _objectContext = objectContext;
            _indexer = indexer;
             _getEnumerator = new Lazy<IEnumerator<KeyValuePair<string, object>>>(FindEnumerator);
        }

        private IEnumerator<KeyValuePair<string, object>> FindEnumerator() {
            foreach (var mi in _objectContext.GetType().GetTypeInfo().GetRuntimeMethods()) {
                if (mi.Name != "GetEnumerator") {
                    continue;
                }

                if (mi.ReturnType == typeof(IEnumerator<KeyValuePair<string, object>>)) {
                    return (IEnumerator<KeyValuePair<string, object>>) mi.Invoke(_objectContext, Array.Empty<object>());
                }
            }
            return EmptyEnumerator;
        }

        protected override void SetPropertyCore(string key, object defaultValue) {
            _indexer.SetValue(_objectContext, defaultValue, new object[] { key });
        }

        protected override bool TryGetPropertyCore(string key, Type requiredType, out object value) {
            try {
                value = _indexer.GetValue(_objectContext, new object[] { key });
                return true;

            } catch (TargetInvocationException ex) {
                if (ex.InnerException is KeyNotFoundException
                    || ex.InnerException is ArgumentException) {
                        value = null;
                        return false;
                }

                throw;
            }
        }

        public override IEnumerator<KeyValuePair<string, object>> GetEnumerator() {
            return _getEnumerator.Value;
        }

        public override void ClearProperties() {
        }

        static IEnumerator<KeyValuePair<string, object>> EmptyEnumerator {
            get {
                yield break;
            }
        }
    }
}
