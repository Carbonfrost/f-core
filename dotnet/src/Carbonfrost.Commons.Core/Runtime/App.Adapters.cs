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

namespace Carbonfrost.Commons.Core.Runtime {

    partial class App {

        private static readonly IAdapterRoleInfoDescription _description = new Description();

        public static IAdapterRoleInfoDescription DescribeAdapterRoles() {
            return _description;
        }

        public static IEnumerable<string> GetAdapterRoleNames() {
            return _description.GetAdapterRoleNames();
        }

        public static AdapterRoleInfo GetAdapterRoleInfo(string name) {
            return _description.GetAdapterRoleInfo(name);
        }

        public static AdapterRoleInfo GetAdapterRoleInfo(Type adapterType) {
            return _description.GetAdapterRoleInfo(adapterType);
        }

        public static IEnumerable<AdapterRoleInfo> GetAdapterRoleInfos() {
            return _description.GetAdapterRoleInfos();
        }

        class Description : IAdapterRoleInfoDescription {

            private readonly Buffer<AdapterRoleInfo> _data;

            public Description() {
                _data = new Buffer<AdapterRoleInfo>
                    (AssemblyObserver.Instance.SelectMany(a => a.GetAdapterRoleInfos()));
            }

            public AdapterRoleInfo GetAdapterRoleInfo(string name) {
                return _data.FirstOrDefault(a => string.Equals(a.Name, name, StringComparison.OrdinalIgnoreCase));
            }

            public AdapterRoleInfo GetAdapterRoleInfo(Type adapterType) {
                return _data.FirstOrDefault(a => adapterType == a.AdapterType);
            }


            public IEnumerable<AdapterRoleInfo> GetAdapterRoleInfos() {
                return _data;
            }

            public IEnumerable<string> GetAdapterRoleNames() {
                return _data.Select(a => a.Name).Distinct();
            }

        }

    }
}
