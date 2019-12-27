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
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Carbonfrost.Commons.Core.Runtime {

    class StartClassInfo {

        private static IDictionary<Assembly, StartClassInfo> _cache = new Dictionary<Assembly, StartClassInfo>();
        private IEnumerable<Type> _all;

        public IEnumerable<Type> StaticClasses {
            get {
                if (!(_all is Array)) {
                    _all = _all.ToArray();
                }

                return _all;
            }
        }

        public StartClassInfo(Assembly asm) {
            _all = asm.GetTypesHelper().Where(t => t.IsSealed && t.IsAbstract).Select(t => t.AsType());
        }

        public static StartClassInfo Get(Assembly assembly) {
            return _cache.GetValueOrCache(assembly, () => new StartClassInfo(assembly));
        }

        public IEnumerable<Type> GetByName(string className) {
            if (className.Contains("*")) {
                var regex = new Regex(className.Replace("*", ".*"));
                return StaticClasses.Where(t => regex.IsMatch(t.Name));
            }
            return StaticClasses.Where(t => t.Name == className);
        }

        public static IEnumerable<TValue> FindStartFields<TValue>(IEnumerable<Type> types) {
            return types.SelectMany(t => t.GetTypeInfo().GetFields(BindingFlags.Static | BindingFlags.Public))
                .Where(f => typeof(TValue).GetTypeInfo().IsAssignableFrom(f.FieldType))
                .Select(f => f.GetValue(null))
                .Cast<TValue>();
        }
    }
}
