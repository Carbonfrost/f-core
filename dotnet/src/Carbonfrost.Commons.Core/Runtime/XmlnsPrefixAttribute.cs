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

namespace Carbonfrost.Commons.Core.Runtime {

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class XmlnsPrefixAttribute : Attribute {

        public string Xmlns {
            get;
            private set;
        }

        public string Prefix {
            get;
            private set;
        }

        public XmlnsPrefixAttribute(string xmlns, string prefix) {
            if (string.IsNullOrEmpty(xmlns)) {
                throw Failure.NullOrEmptyString(nameof(xmlns));
            }
            if (string.IsNullOrEmpty(prefix)) {
                throw Failure.NullOrEmptyString(nameof(prefix));
            }

            Xmlns = xmlns;
            Prefix = prefix;
        }
    }
}
