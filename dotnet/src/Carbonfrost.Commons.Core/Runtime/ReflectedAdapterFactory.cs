//
// Copyright 2013, 2016 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using System.Linq;
using System.Reflection;

namespace Carbonfrost.Commons.Core.Runtime {

    [DebuggerDisplayAttribute("{assembly}")]
    sealed class ReflectedAdapterFactory : AdapterFactory {

        private static readonly IDictionary<Assembly, ReflectedAdapterFactory> items
            = new Dictionary<Assembly, ReflectedAdapterFactory>();

        private readonly Assembly _assembly;
        private readonly ICollection<string> _except;

        private ReflectedAdapterFactory(Assembly assembly, ICollection<string> except) {
            _assembly = assembly;
            _except = except;
        }

        public static ReflectedAdapterFactory Create(Assembly assembly, ICollection<string> except) {
            return items.GetValueOrCache(assembly,
                                         () => new ReflectedAdapterFactory(assembly, except));
        }

        public override Type GetAdapterType(Type adapteeType, string adapterRoleName) {
            if (adapteeType == null) {
                throw new ArgumentNullException("adapteeType");
            }
            if (adapterRoleName == null) {
                throw new ArgumentNullException("adapterRoleName");
            }
            if (adapterRoleName.Length == 0) {
                throw Failure.EmptyString("adapterRoleName");
            }

            if (adapteeType.GetTypeInfo().Assembly != _assembly) {
                return null;
            }

            if (_except.Contains(adapterRoleName)) {
                return null;
            }

            var attrs = ((AdapterAttribute[]) adapteeType.GetCustomAttributes(typeof(AdapterAttribute), true));
            foreach (var attr in attrs) {
                if (string.Equals(attr.Role, adapterRoleName, StringComparison.OrdinalIgnoreCase)) {
                    return attr.AdapterType;
                }
            }

            return DefineAdapterAttribute.GetAdapterTypes(adapteeType, adapterRoleName, false).FirstOrDefault();
        }

    }
}
