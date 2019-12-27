//
// Copyright 2005, 2006, 2010, 2019 Carbonfrost Systems, Inc.
// (http://carbonfrost.com)
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
using System.Linq;
using System.Reflection;

namespace Carbonfrost.Commons.Core.Runtime {

    public partial class ServiceContainer : DisposableObject, IServiceContainer, IActivationFactory {

        private readonly Dictionary<Type, ServiceDictionary> _services = CreateTypeDictionary();
        private readonly IServiceProvider _parentProvider;
        private readonly IActivationFactory _activationFactory;

        private ServiceDictionary GetServiceDictionary(Type serviceType, bool create) {
            if (serviceType == null) {
                throw new ArgumentNullException("serviceType");
            }

            var unwrappedServiceType = UnwrapServiceType(serviceType);
            ServiceDictionary result;
            if (_services.TryGetValue(unwrappedServiceType, out result)) {
                return result;
            }

            if (create) {
                return _services[unwrappedServiceType] = new ServiceDictionary(this, serviceType);
            }

            return null;
        }

        // IActivationFactory implementation
        public virtual object CreateInstance(
            Type type,
            IEnumerable<KeyValuePair<string, object>> values = null,
            IServiceProvider serviceProvider = null,
            params Attribute[] attributes)
        {
            IServiceProvider actualServiceProvider;
            if (serviceProvider == this) {
                actualServiceProvider = serviceProvider;
            } else if (serviceProvider == null) {
                actualServiceProvider = this;
            } else {
                actualServiceProvider = ServiceProvider.Compose(serviceProvider, this);
            }

            return _activationFactory.CreateInstance(type, values, actualServiceProvider, attributes);
        }

        public void AddService(Type serviceType, object serviceInstance) {
            if (serviceType == null) {
                throw new ArgumentNullException("serviceType");
            }
            if (serviceInstance == null) {
                throw new ArgumentNullException("serviceInstance");
            }

            GetServiceDictionary(serviceType, true).Add(null, serviceInstance);
        }

        public void AddService(Type serviceType, Func<IServiceContainer, Type, object> callback) {
            if (serviceType == null) {
                throw new ArgumentNullException("serviceType");
            }
            if (callback == null) {
                throw new ArgumentNullException("callback");
            }

            GetServiceDictionary(serviceType, true).Add(null, callback);
        }

        public void RemoveService(Type serviceType) {
            if (serviceType == null) {
                throw new ArgumentNullException("serviceType");
            }

            var dict = GetServiceDictionary(serviceType, false);
            if (dict != null) {
                dict.Clear();
            }
        }

        public object GetService(Type serviceType) {
            if (serviceType == null)
                throw new ArgumentNullException("serviceType");

            // Process self
            if (serviceType.GetTypeInfo().IsInstanceOfType(this)) {
                return this;
            }

            var dict = GetServiceDictionary(serviceType, false);
            var result = dict == null ? null : dict.Get(serviceType).FirstOrDefault();

            return result ?? ParentProvider.GetService(serviceType);
        }

        protected override void Dispose(bool manualDispose) {
            if (manualDispose) {
                foreach (var item in _services.Values) {
                    item.Dispose();
                }
            }
            base.Dispose(manualDispose);
        }

        private static Dictionary<Type, ServiceDictionary> CreateTypeDictionary() {
            return new Dictionary<Type, ServiceDictionary>(Utility.EquivalentComparer);
        }

        private static Type UnwrapServiceType(Type serviceType) {
            var unwrappedServiceType = serviceType;
            if (serviceType.GetTypeInfo().IsGenericType) {
                var def = serviceType.GetGenericTypeDefinition();
                if (def == typeof(Lazy<>) || def == typeof(Func<>)) {
                    unwrappedServiceType = serviceType.GetGenericArguments()[0];
                }
            }
            return unwrappedServiceType;
        }
    }
}
