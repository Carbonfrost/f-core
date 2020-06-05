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
using Carbonfrost.Commons.Spec;
using Carbonfrost.Commons.Core.Runtime;
using System.Collections.Generic;
using System.Reflection;

namespace Carbonfrost.UnitTests.Core.Runtime {

    public class StartClassProviderRegistrationTests {

        public static class PStartProviderRegistrationClass {
            public static void ErrorRegistrationMethod(ProviderRegistrationContext _) {
                throw new Exception();
            }
        }

        class ProviderRegistrationContextImpl : ProviderRegistrationContext {

            internal ProviderRegistrationContextImpl() : base(null) {
            }

            internal override IEnumerable<Type> StartClasses {
                get {
                    return new [] { typeof(PStartProviderRegistrationClass) };
                }
            }
        }

        [Fact]
        public void RegisterProviderTypes_will_create_log_if_failure() {
            using (var record = new LateBoundLogRecord()) {
                var sc = new StartClassProviderRegistration();
                sc.RegisterProviderTypes(new ProviderRegistrationContextImpl());

                Assert.Equal(
                    "Problem executing provider registration method `Carbonfrost.UnitTests.Core.Runtime.PStartProviderRegistrationClass+ErrorRegistrationMethod'",
                    record.LastFailure.Message
                );
            }
        }
    }
}
