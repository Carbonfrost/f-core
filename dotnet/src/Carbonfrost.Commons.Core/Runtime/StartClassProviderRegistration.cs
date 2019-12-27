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
using System.Linq;
using System.Reflection;

namespace Carbonfrost.Commons.Core.Runtime {

    sealed class StartClassProviderRegistration : ProviderRegistration {

        public static readonly ProviderRegistration Instance = new StartClassProviderRegistration();

        public override void RegisterProviderTypes(ProviderRegistrationContext context) {
            var types = context.Assembly.GetStartClasses("ProviderRegistration");

            foreach (var type in types) {
                // Look for Register methods
                // Register(ProviderRegistrationContext)

                foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static)) {
                    var pms = method.GetParameters();

                    if (method.ReturnType == null && pms.Length == 1 && pms[0].ParameterType == typeof(ProviderRegistrationContext)) {
                        method.Invoke(null, new object[] { context });
                    }
                }

                // Also treat like a [Providers] class
                SlowProviderRegistration.ExtractProvidersClass(context, type.GetTypeInfo());
            }
        }

    }
}
