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
using System.Reflection;

namespace Carbonfrost.Commons.Core.Runtime {

    public abstract class ProviderInfo {

        public abstract QualifiedName Name { get; }

        public virtual IReadOnlyList<QualifiedName> Names {
            get {
                return new[] { Name };
            }
        }

        public abstract Type Type { get; }
        public abstract MemberInfo Member { get; }
        public abstract object Metadata { get; }

        public virtual Assembly Assembly {
            get {
                return Member.DeclaringType.GetTypeInfo().Assembly;
            }
        }

        public abstract Type ProviderType { get; }

        public abstract object Activate(IEnumerable<KeyValuePair<string, object>> arguments, IServiceProvider services);

        private protected ProviderInfo() {}
    }
}
