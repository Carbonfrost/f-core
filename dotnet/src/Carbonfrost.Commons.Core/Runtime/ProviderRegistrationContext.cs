//
// Copyright 2013, 2016, 2020 Carbonfrost Systems, Inc. (https://carbonfrost.com)
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

    public class ProviderRegistrationContext {

        readonly ProviderInfoCollection _result = new ProviderInfoCollection();
        readonly List<Type> roots = new List<Type>();

        public Assembly Assembly {
            get;
            private set;
        }

        // For tests only
        internal virtual IEnumerable<Type> StartClasses {
            get {
                return Assembly.GetStartClasses("ProviderRegistration");
            }
        }

        internal ProviderRegistrationContext(Assembly assembly) {
            Assembly = assembly;
        }

        // TODO Validate root providers

        // TODO Custom client implementations could register providers and root providers out of order (uncommon)

        internal IEnumerable<Type> EnumerateRoots() {
            return roots;
        }

        public void DefineRootProvider(Type providerType) {
            if (providerType == null) {
                throw new ArgumentNullException(nameof(providerType));
            }

            lock (roots) {
                roots.Add(providerType);
            }
            Traceables.DefineRootProvider(providerType);
        }

        public void DefineProvider(QualifiedName name,
                                   Type providerType,
                                   Type providerInstanceType,
                                   object metadata = null)
        {
            _result.DefineProvider(name, providerType, providerInstanceType, metadata);
            Traceables.DefineProvider(name, providerType);
        }

        public void DefineProvider(QualifiedName name,
                                   Type providerType,
                                   FieldInfo field,
                                   object metadata = null)
        {
            _result.DefineProvider(name, providerType, field, metadata);
            Traceables.DefineProvider(name, providerType);
        }

        public void DefineProvider(QualifiedName name,
                                   Type providerType,
                                   MethodInfo factoryMethod,
                                   object metadata = null)
        {
            _result.DefineProvider(name, providerType, factoryMethod, metadata);
            Traceables.DefineProvider(name, providerType);
        }

        internal IEnumerable<ProviderValueSource> EnumerateValueSources() {
            return _result.Cast<ProviderValueSource>();
        }
    }
}
