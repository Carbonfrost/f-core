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
using System.Linq;
using System.Reflection;

namespace Carbonfrost.Commons.Core.Runtime {

    partial class Adaptable {

        public static IEnumerable<string> GetAdapterRoleNames(this Assembly assembly) {
            return assembly.GetCustomAttributes(typeof(DefinesAttribute))
                .Select(t => ((DefinesAttribute) t).AdapterRole)
                .Distinct();
        }

        public static IEnumerable<AdapterRoleInfo> GetAdapterRoleInfos(this Assembly assembly) {
            return assembly.GetCustomAttributes(typeof(DefinesAttribute))
                .Select(t => CreateAdapterRoleInfoFrom((DefinesAttribute) t, assembly));
        }

        private static AdapterRoleInfo CreateAdapterRoleInfoFrom(DefinesAttribute t, Assembly assembly) {
            return new AdapterRoleInfo(
                t.AdapterRole,
                t.AdapterType ?? ImplicitRole(assembly, t.AdapterRole),
                assembly
            );
        }

        private static Type ImplicitRole(Assembly assembly, string adapterRole) {
            string interfaceName = "I" + adapterRole;

            return assembly.ExportedTypes.FirstOrDefault(
                t => t.Name == adapterRole
            ) ?? assembly.ExportedTypes.FirstOrDefault(
                t => string.Equals(t.Name, interfaceName) && t.IsInterface
            );
        }
    }
}
