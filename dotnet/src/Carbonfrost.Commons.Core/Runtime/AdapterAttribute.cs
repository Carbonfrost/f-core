//
// Copyright 2005, 2006, 2010, 2016, 2019-2020 Carbonfrost Systems, Inc.
// (http://carbonfrost.com)
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

    [AttributeUsage(AttributeTargets.Struct
                    | AttributeTargets.Interface
                    | AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class AdapterAttribute : Attribute {

        private readonly Type _adapterType;
        private readonly string _role;

        public Type AdapterType {
            get {
                return _adapterType;
            }
        }

        public string Role {
            get {
                return _role;
            }
        }

        private string ImpliedRoleName {
            get {
                return Regex.Replace(GetType().Name, "Attribute$" , "");
            }
        }

        public AdapterAttribute(string adapterType) {
            if (string.IsNullOrEmpty(adapterType)) {
                throw Failure.NullOrEmptyString(nameof(adapterType));
            }

            _adapterType = Type.GetType(adapterType);
            _role = ImpliedRoleName;
        }

        public AdapterAttribute(Type adapterType) {
            if (adapterType == null) {
                throw new ArgumentNullException(nameof(adapterType));
            }

            _adapterType = adapterType;
            _role = ImpliedRoleName;
        }

        public AdapterAttribute(string role, string adapterType) {
            role = CheckRole(role);
            if (string.IsNullOrEmpty(adapterType)) {
                throw Failure.NullOrEmptyString(nameof(adapterType));
            }

            _adapterType = Type.GetType(adapterType);
            _role = role;
        }

        public AdapterAttribute(string role, Type adapterType) {
            role = CheckRole(role);

            if (adapterType == null) {
                throw new ArgumentNullException(nameof(adapterType));
            }

            _adapterType = adapterType;
            _role = role;
        }

        private static string CheckRole(string role) {
            if (role == null) {
                throw new ArgumentNullException(nameof(role));
            }

            role = role.Trim();
            if (role.Length == 0) {
                throw Failure.AllWhitespace(nameof(role));
            }
            return role;
        }

    }
}
