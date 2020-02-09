//
// Copyright 2014, 2020 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using System.Diagnostics;
using System.Reflection;

namespace Carbonfrost.Commons.Core.Runtime {

    sealed class ReflectionPropertyProviderUsingIndexer : IPropertyProvider {

        private readonly object _objectContext;
        private readonly PropertyInfo _indexer;

        public ReflectionPropertyProviderUsingIndexer(object objectContext, PropertyInfo indexer) {
            _objectContext = objectContext;
            _indexer = indexer;
        }

        public static ReflectionPropertyProviderUsingIndexer TryCreate(object context) {
            Debug.Assert(context != null);
            var indexer = ReflectionPropertyProvider.FindIndexerProperty(context.GetType());
            if (indexer != null) {
                return new ReflectionPropertyProviderUsingIndexer(context, indexer);
            }
            return null;
        }

        public Type GetPropertyType(string property) {
            PropertyProvider.CheckProperty(property);
            return _indexer.PropertyType;
        }

        public bool TryGetProperty(string property, Type propertyType, out object value) {
            PropertyProvider.CheckProperty(property);
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
