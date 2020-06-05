//
// Copyright 2016, 2020 Carbonfrost Systems, Inc. (https://carbonfrost.com)
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
using Carbonfrost.Commons.Core.Runtime;

namespace Carbonfrost.Commons.Core {

    public static partial class ServiceProvider {

        public static T GetRequiredService<T>(this IServiceProvider serviceProvider) {
            if (serviceProvider == null) {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            T t = (T) serviceProvider.GetService(typeof(T));
            if (object.Equals(t, default(T))) {
                throw RuntimeFailure.ServiceNotFound(typeof(T));
            }

            return t;
        }

        public static T GetServiceOrDefault<T>(this IServiceProvider serviceProvider, T defaultService = default(T)) {
            if (serviceProvider == null) {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            T result = (T) serviceProvider.GetService(typeof(T));
            if (object.Equals(result, default(T))) {
                return defaultService;
            }

            return result;
        }

        public static T GetService<T>(this IServiceProvider serviceProvider) {
            if (serviceProvider == null) {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            return (T) serviceProvider.GetService(typeof(T));
        }
    }
}
