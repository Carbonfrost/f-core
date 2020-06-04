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
using Carbonfrost.Commons.Core;
using Carbonfrost.Commons.Core.Runtime;
using System.Collections.Generic;

namespace Carbonfrost.UnitTests.Core.Runtime {

    public class RootServiceProviderTests {

        public class PServiceRegistration {
            public static void ErrorRegistrationMethod() {
                throw new Exception();
            }
        }

        [Fact]
        public void RootServiceProvider_will_create_log_on_failure() {
            var rsp = new RootServiceProvider(new List<Type> { typeof(PServiceRegistration) }.GetEnumerator());
            using (var record = new LateBoundLogRecord()) {
                rsp.GetService(typeof(string));

                Assert.Equal(
                    "Problem executing service registration method `Carbonfrost.UnitTests.Core.Runtime.RootServiceProviderTests+PServiceRegistration.ErrorRegistrationMethod'",
                    record.LastFailure.Message
                );
            }
        }

    }
}
