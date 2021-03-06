//
// Copyright 2016 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using System.Reflection;

namespace Carbonfrost.Commons.Core.Runtime {

    sealed class AssemblyNameComparer : IEqualityComparer<Assembly>, IEqualityComparer<AssemblyName> {

        public bool Equals(Assembly x, Assembly y) {
            return x.FullName.Equals(y.FullName);
        }

        public bool Equals(AssemblyName x, AssemblyName y) {
            return x.FullName.Equals(y.FullName);
        }

        public int GetHashCode(Assembly obj) {
            return obj.FullName.GetHashCode();
        }

        public int GetHashCode(AssemblyName obj) {
            return obj.FullName.GetHashCode();
        }
    }

}
