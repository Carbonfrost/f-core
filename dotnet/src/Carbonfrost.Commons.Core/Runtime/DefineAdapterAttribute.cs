//
// Copyright 2012, 2020 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class DefineAdapterAttribute : Attribute {

        static IDictionary<string, DefineAdapterAttribute[]> cache;

        public Type AdapteeType {
            get;
            private set;
        }

        public Type AdapterType {
            get;
            private set;
        }

        public string Role {
            get;
            private set;
        }

        private string ImpliedRoleName {
            get {
                var result = Regex.Replace(GetType().Name, "Attribute$" , "");
                return Regex.Replace(result, "^Define" , "");
            }
        }

        public DefineAdapterAttribute(string role, Type adapteeType, Type adapterType) {
            Initialize(role, adapteeType, adapterType);
        }

        public DefineAdapterAttribute(string role, Type adapteeType, string adapterType) {
            Initialize(role, adapteeType, adapterType);
        }

        protected DefineAdapterAttribute(Type adapteeType, Type adapterType) {
            Initialize(ImpliedRoleName, adapteeType, adapterType);
        }

        protected DefineAdapterAttribute(Type adapteeType, string adapterType) {
            Initialize(ImpliedRoleName, adapteeType, adapterType);
        }

        internal static IEnumerable<Type> GetAdapterTypes(
            Type adapteeType, string adapterRoleName, bool inherit)
        {
            EnsureCache();

            Func<DefineAdapterAttribute, bool> predicate;
            if (inherit) {
                predicate = t => (t.AdapteeType.GetTypeInfo().IsAssignableFrom(adapteeType));
            } else {
                predicate = t => (adapteeType == t.AdapteeType);
            }

            return cache.GetValueOrDefault(adapterRoleName, Array.Empty<DefineAdapterAttribute>())
                .Where(predicate)
                .Select(t => t.AdapterType);
        }

        static void EnsureCache() {
            if (cache == null) {
                var allItems = App.DescribeAssemblies(
                    t => (DefineAdapterAttribute[]) t.GetCustomAttributes(typeof(DefineAdapterAttribute), false));

                cache = allItems.GroupBy(t => t.Role).ToDictionary(t => t.Key, t => t.ToArray());
            }
        }

        private void Initialize(string role, Type adapteeType, Type adapterType) {
            if (string.IsNullOrEmpty(role)) {
                throw Failure.NullOrEmptyString(nameof(role));
            }
            if (adapteeType == null) {
                throw new ArgumentNullException(nameof(adapteeType));
            }
            if (adapterType == null) {
                throw new ArgumentNullException(nameof(adapterType));
            }

            Role = role;
            AdapteeType = adapteeType;
            AdapterType = adapterType;
        }

        private void Initialize(string role, Type adapteeType, string adapterType) {
            if (string.IsNullOrEmpty(role)) {
                throw Failure.NullOrEmptyString(nameof(role));
            }
            if (adapteeType == null) {
                throw new ArgumentNullException(nameof(adapteeType));
            }
            if (string.IsNullOrEmpty(adapterType)) {
                throw Failure.NullOrEmptyString(nameof(adapterType));
            }

            Role = role;
            AdapteeType = adapteeType;
            AdapterType = Type.GetType(adapterType);
        }

    }
}
