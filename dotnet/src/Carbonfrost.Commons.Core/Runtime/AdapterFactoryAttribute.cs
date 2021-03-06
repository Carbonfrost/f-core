//
// Copyright 2013, 2019 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using System.Text.RegularExpressions;

namespace Carbonfrost.Commons.Core.Runtime {

    [AttributeUsage(AttributeTargets.Assembly)]
    public class AdapterFactoryAttribute : Attribute {

        public Type AdapterFactoryType {
            get;
            private set;
        }

        public string Role {
            get;
            private set;
        }

        private string ImpliedRoleName {
            get {
                return Regex.Replace(GetType().Name, "(Factory)?Attribute$" , "");
            }
        }

        protected AdapterFactoryAttribute(Type adapterFactoryType) {
            AdapterFactoryType = adapterFactoryType;
            Role = ImpliedRoleName;
        }

        protected AdapterFactoryAttribute(string adapterFactoryType) {
            AdapterFactoryType = Type.GetType(adapterFactoryType);
            Role = ImpliedRoleName;
        }

        public AdapterFactoryAttribute(string role, Type adapterFactoryType) {
            CheckRole(role);

            if (adapterFactoryType == null) {
                throw new ArgumentNullException(nameof(adapterFactoryType));
            }

            AdapterFactoryType = adapterFactoryType;
            Role = role;
        }

        public AdapterFactoryAttribute(string role, string adapterFactoryType) {
            CheckRole(role);

            if (adapterFactoryType == null) {
                throw new ArgumentNullException(nameof(adapterFactoryType));
            }

            AdapterFactoryType = Type.GetType(adapterFactoryType);
            Role = role;
        }

        private static void CheckRole(string role) {
            if (role == null) {
                throw new ArgumentNullException(nameof(role));
            }
            if (role.Length == 0) {
                throw Failure.EmptyString(nameof(role));
            }
        }
    }
}
