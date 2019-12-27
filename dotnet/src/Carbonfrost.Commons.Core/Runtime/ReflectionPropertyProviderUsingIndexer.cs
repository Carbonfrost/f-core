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

using System.Reflection;

namespace Carbonfrost.Commons.Core.Runtime {

    class ReflectionPropertyProviderUsingIndexer : PropertyProviderBase {

        private readonly object _objectContext;
        private readonly PropertyInfo _indexer;

        public ReflectionPropertyProviderUsingIndexer(object objectContext, PropertyInfo indexer) {
            _objectContext = objectContext;
            _indexer = indexer;
        }

        protected override bool TryGetPropertyCore(string property, Type propertyType, out object value) {
            value = null;

            try {
                value = _indexer.GetValue(_objectContext, new object[] { property });

            } catch (TargetInvocationException ex) {
                if (ex.InnerException is KeyNotFoundException
                    || ex.InnerException is ArgumentException) {
                    return false;
                }

                throw;
            }

            return true;
        }
    }
}
