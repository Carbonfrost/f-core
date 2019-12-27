//
// Copyright 2005, 2006, 2010, 2016, 2019 Carbonfrost Systems, Inc.
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

        public AdapterAttribute(string adapterType, string role) {
            if (adapterType == null) {
                throw new ArgumentNullException("adapterType");
            }

            adapterType = adapterType.Trim();
            if (adapterType.Length == 0) {
                throw Failure.AllWhitespace("adapterType");
            }

            if (role == null) {
                throw new ArgumentNullException("role");
            }

            role = role.Trim();
            if (role.Length == 0) {
                throw Failure.AllWhitespace("role");
            }

            _adapterType = Type.GetType(adapterType);
            _role = role;
        }

        public AdapterAttribute(Type adapterType, string role) {
            if (adapterType == null) {
                throw new ArgumentNullException("adapterType");
            }

            if (role == null) {
                throw new ArgumentNullException("role");
            }

            role = role.Trim();
            if (role.Length == 0) {
                throw Failure.AllWhitespace("role");
            }

            _adapterType = adapterType;
            _role = role;
        }

    }
}
