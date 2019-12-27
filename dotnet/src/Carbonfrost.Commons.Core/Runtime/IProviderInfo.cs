//
// Copyright 2014 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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

    public interface IProviderInfo {

        // TODO Consider pushing up GetValue() and IsValue()

        object Activate(IEnumerable<KeyValuePair<string, object>> arguments,
                        IServiceProvider services);

        QualifiedName Name { get; }
        IReadOnlyList<QualifiedName> Names { get; }

        Type Type { get; }
        MemberInfo Member { get; }
        object Metadata { get; }

        Assembly Assembly { get; }
        Type ProviderType { get; }

    }
}
