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
using Carbonfrost.Commons.Core.Runtime;

namespace Carbonfrost.Commons.Core {

    public static partial class Extensions {

        public static IServiceContainer AddService<T>(this IServiceContainer serviceContainer) {
            if (serviceContainer == null) {
                throw new ArgumentNullException(nameof(serviceContainer));
            }

            Func<IServiceContainer, Type, object> callback = (c, t) => Activation.CreateInstance(t, serviceProvider: c);
            serviceContainer.AddService(typeof(T), callback);
            return serviceContainer;
        }

        public static IServiceContainer AddService<T>(this IServiceContainer serviceContainer, Func<T> serviceFactory) {
            if (serviceContainer == null) {
                throw new ArgumentNullException(nameof(serviceContainer));
            }

            Func<IServiceContainer, Type, object> callback = (c, t) => serviceFactory();
            serviceContainer.AddService(typeof(T), callback);
            return serviceContainer;
        }

        public static void AddService<T>(this IServiceContainer serviceContainer, T serviceInstance) where T : class {
            if (serviceContainer == null) {
                throw new ArgumentNullException(nameof(serviceContainer));
            }
            if (serviceInstance == null) {
                throw new ArgumentNullException(nameof(serviceInstance));
            }

            serviceContainer.AddService(typeof(T), serviceInstance);
        }

        public static bool TryGetService<T>(this IServiceProvider serviceProvider, out T service) where T : class {
            service = (T) serviceProvider.GetService(typeof(T));
            return !(service is null);
        }

        public static bool TryGetService(this IServiceProvider serviceProvider, Type serviceType, out object service) {
            service = serviceProvider.GetService(serviceType);
            return !(service is null);
        }

    }

}
