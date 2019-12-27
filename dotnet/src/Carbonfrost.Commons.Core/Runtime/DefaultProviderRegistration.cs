//
// Copyright 2013 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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

namespace Carbonfrost.Commons.Core.Runtime {

    sealed class DefaultProviderRegistration : ProviderRegistration {

        public override void RegisterProviderTypes(ProviderRegistrationContext context) {
            var startClasses = context.Assembly.GetStartClasses("ProviderRegistration");

            ProviderRegistration registrar;
            if (startClasses.Any())
                registrar = StartClassProviderRegistration.Instance;
            else
                registrar = SlowProviderRegistration.Instance;

            registrar.RegisterProviderTypes(context);
        }

        public override void RegisterRootProviderTypes(ProviderRegistrationContext context) {
            SlowProviderRegistration.Instance.RegisterRootProviderTypes(context);
        }

    }
}
