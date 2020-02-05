//
// Copyright 2016, 2020 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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

    class DefaultAdapterFactoryImpl : IAdapterFactory {

        static readonly IDictionary<string, string> Conventions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
            { "Builder", "-Builder" },
            { "StreamingSource", "-Source" }, // sic
            { "ActivationProvider", "-ActivationProvider" },
            { "Template", "-Template" },
            { "Null", "Null-" },
        };

        // TODO Providers and configured adapters

        public object GetAdapter(object adaptee, string adapterRoleName) {
            if (adaptee == null) {
                throw new ArgumentNullException("adaptee");
            }
            if (string.IsNullOrEmpty(adapterRoleName)) {
                throw Failure.NullOrEmptyString(nameof(adapterRoleName));
            }
            object result;
            if (TryBasicFactories(adaptee.GetType(), adapterRoleName, t => t.GetAdapter(adaptee, adapterRoleName), out result)) {
                return result;
            }

            var convo = GetConventionAdapterType(adaptee.GetType(), adapterRoleName);
            if (convo == null) {
                return null;
            }
            return AdapterFactory.NewInstance(adaptee, convo);
        }

        public Type GetAdapterType(object adaptee, string adapterRoleName) {
            if (adaptee == null) {
                throw new ArgumentNullException("adaptee");
            }
            if (string.IsNullOrEmpty(adapterRoleName)) {
                throw Failure.NullOrEmptyString(nameof(adapterRoleName));
            }
            Type result;
            if (TryBasicFactories(adaptee.GetType(), adapterRoleName, t => t.GetAdapterType(adaptee, adapterRoleName), out result)) {
                return result;
            }

            return GetConventionAdapterType(adaptee.GetType(), adapterRoleName);
        }

        public Type GetAdapterType(Type adapteeType, string adapterRoleName) {
            if (adapteeType == null) {
                throw new ArgumentNullException("adapteeType");
            }
            if (string.IsNullOrEmpty(adapterRoleName)) {
                throw Failure.NullOrEmptyString(nameof(adapterRoleName));
            }
            Type result;
            if (TryBasicFactories(adapteeType, adapterRoleName, t => t.GetAdapterType(adapteeType, adapterRoleName), out result)) {
                return result;
            }

            return GetConventionAdapterType(adapteeType, adapterRoleName);
        }

        private bool TryBasicFactories<T>(Type type, string adapterRoleName, Func<IAdapterFactory, T> func, out T result) {
            var roleAsm = AdapterFactory.GetAssemblyThatDefines(adapterRoleName);
            if (roleAsm == null) {
                throw RuntimeFailure.AdapterRoleNotDefined("adapterRoleName", adapterRoleName);
            }
            // Try assembly factory, factory where the role is defined, then provider factories
            var asm = AdapterFactory.FromAssembly(type.GetTypeInfo().Assembly) ?? AdapterFactory.Null;
            result = func(asm);
            if (result != null) {
                return true;
            }

            result = func(AdapterFactory.FromAssembly(roleAsm));
            if (result != null) {
                return true;
            }

            // Exclude self, otherwise would be recursive
            var right = App.GetProviders<IAdapterFactory>().Where(t => t != this);
            foreach (var m in right) {
                result = func(m);
                if (result != null) {
                    return true;
                }
            }

            return false;
        }

        private static Type GetConventionAdapterType(Type adapteeType, string adapterRoleName) {
            if (IsSupportedAdapter(adapterRoleName)) {
                string qualifierName = adapteeType.Namespace + ".";
                if (string.IsNullOrEmpty(adapteeType.Namespace)) {
                    qualifierName = null;
                }
                if (adapteeType.GetTypeInfo().DeclaringType != null) {
                    qualifierName = adapteeType.DeclaringType.FullName + "+";
                }

                // Get the type which is named by convention for this adapter role.
                // Replacing the - in the conventions map gives us the lookup
                string conventionType = qualifierName + Conventions[adapterRoleName].Replace("-", adapteeType.Name);
                var result = adapteeType.GetTypeInfo().Assembly.GetType(conventionType);
                if (result == null) {
                    return null;
                }
                switch (adapterRoleName) {
                    case "Builder":
                        return AdapterRole.IsBuilderType(result, adapteeType) ? result : null;
                    case "StreamingSource":
                        return AdapterRole.IsStreamingSourceType(result) ? result : null;
                    case "ActivationProvider":
                        return AdapterRole.IsActivationProviderType(result) ? result : null;
                    case "Template":
                        return AdapterRole.IsTemplateType(result) ? result : null;
                    case "Null":
                        return result;
                }
            }
            return null;
        }

        private static bool IsSupportedAdapter(string adapterRoleName) {
            return Conventions.ContainsKey(adapterRoleName);
        }
    }
}
