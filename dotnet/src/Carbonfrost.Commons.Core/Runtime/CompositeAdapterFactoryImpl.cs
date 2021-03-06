//
// - CompositeAdapterFactoryImpl.cs -
//
// Copyright 2013, 2020 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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

    class CompositeAdapterFactoryImpl : IAdapterFactory {

        private readonly IEnumerable<IAdapterFactory> items;

        public CompositeAdapterFactoryImpl(IEnumerable<IAdapterFactory> items) {
            this.items = items;
        }

        public object GetAdapter(object adaptee, string adapterRoleName) {
            if (adaptee == null)
                throw new ArgumentNullException("adaptee");
            if (string.IsNullOrEmpty(adapterRoleName)) {
                throw Failure.NullOrEmptyString(nameof(adapterRoleName));
            }

            return FirstResult(e => e.GetAdapter(adaptee, adapterRoleName));
        }

        public Type GetAdapterType(Type adapteeType, string adapterRoleName) {
            if (adapteeType == null)
                throw new ArgumentNullException("adapteeType");
            if (string.IsNullOrEmpty(adapterRoleName)) {
                throw Failure.NullOrEmptyString(nameof(adapterRoleName));
            }

            return FirstResult(e => e.GetAdapterType(adapteeType, adapterRoleName));
        }

        public Type GetAdapterType(object adaptee, string adapterRoleName) {
            if (adaptee == null)
                throw new ArgumentNullException("adaptee");
            if (string.IsNullOrEmpty(adapterRoleName)) {
                throw Failure.NullOrEmptyString(nameof(adapterRoleName));
            }

            return FirstResult(e => e.GetAdapterType(adaptee, adapterRoleName));
        }

        private T FirstResult<T>(Func<IAdapterFactory, T> func) {
            foreach (var e in items) {
                T result = func(e);
                if (!object.Equals(result, null))
                    return (T) result;
            }
            return default(T);
        }
    }
}
