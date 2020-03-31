//
// Copyright 2020 Carbonfrost Systems, Inc. (https://carbonfrost.com)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     https://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.CSharp.RuntimeBinder;

namespace Carbonfrost.Commons.Core.Runtime {

    using CallSiteFunc = Func<CallSite, object, object>;

    class DynamicPropertyProvider : IPropertyProvider {

        private readonly object _value;
        private readonly Dictionary<string, CallSite<CallSiteFunc>> _callSiteCache = new Dictionary<string, CallSite<CallSiteFunc>>();

        public DynamicPropertyProvider(dynamic value) {
            _value = value;
        }

        public Type GetPropertyType(string property) {
            object result;
            if (TryGetProperty(property, typeof(object), out result)) {
                if (result == null) {
                    return typeof(object);
                }
                return result.GetType();
            }
            return null;
        }

        public bool TryGetProperty(string property, Type propertyType, out object value) {
            // Cache the callsite for performance
            var callSite = _callSiteCache.GetValueOrCache(
                property,
                () => {
                    var binder = Binder.GetMember(
                        CSharpBinderFlags.None,
                        property,
                        _value.GetType(),
                        new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }
                    );
                    return CallSite<Func<CallSite, object, object>>.Create(binder);
                }
            );
            try {
                value = callSite.Target(callSite, _value);
                return propertyType.IsInstanceOfType(value);

            } catch (RuntimeBinderException) {
                value = null;
                return false;
            }
        }
    }
}
