//
// Copyright 2014, 2019 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using Carbonfrost.Commons.Core.Runtime;

namespace Carbonfrost.Commons.Core {

    class RootServiceProvider : DisposableObject, IServiceContainer {

        private readonly IEnumerator<Type> _startClasses;
        private ServiceContainer _services = new ServiceContainer();

        public IServiceProvider Parent {
            get {
                return null;
            }
        }

        internal RootServiceProvider() {
            var all = App.DescribeAssemblies();
            _startClasses = all.SelectMany(t => t.GetStartClasses("ServiceRegistration")).GetEnumerator();

            if (AppDomain.CurrentDomain.IsDefaultAppDomain()) {
                AppDomain.CurrentDomain.DomainUnload += delegate {
                    Dispose();
                };
            }
        }

        protected override void Dispose(bool manualDispose) {
            if (manualDispose) {
                _services.Dispose();
            }
            base.Dispose(manualDispose);
        }

        private void ApplyStartClasses(Func<bool> until) {
            while (_startClasses.MoveNext()) {
                var start = _startClasses.Current;
                InvokeStartClass(start);

                if (until()) {
                    return;
                }
            }
        }

        public object CreateInstance(Type type,
                                     IEnumerable<KeyValuePair<string, object>> values = null,
                                     IServiceProvider serviceProvider = null,
                                     params Attribute[] attributes) {
            return _services.CreateInstance(type, values, serviceProvider, attributes);
        }

        public void AddService(Type serviceType, object serviceInstance) {
            _services.AddService(serviceType, serviceInstance);
        }

        public void AddService(Type serviceType, Func<IServiceContainer, Type, object> callback) {
            _services.AddService(serviceType, callback);
        }

        public void RemoveService(Type serviceType) {
            _services.RemoveService(serviceType);
        }

        public object GetService(Type serviceType) {
            if (serviceType == null) {
                throw new ArgumentNullException("serviceType");
            }

            if (serviceType.IsInstanceOfType(this)) {
                return this;
            }

            return _services.GetService(serviceType) ?? GetServiceUsingDiscovery(serviceType);
        }

        private object GetServiceUsingDiscovery(Type serviceType) {
            // TODO If this service is subsequently removed, then it is possible that
            // next GetService call will return a different instance or that
            // AppDomain will synchronize (performance, design)

            ApplyStartClasses(() => _services.TryGetService(serviceType, out _));
            return _services.GetService(serviceType);
        }

        private void InvokeStartClass(Type startClass) {
            // TODO Support fields and properties
            foreach (var mi in startClass.GetMethods(BindingFlags.Public | BindingFlags.Static)) {
                var args = mi.GetParameters()
                    .Select(t => GetService(t.ParameterType)).ToArray();

                try {
                    mi.Invoke(null, args);

                } catch (TargetException) {
                }
            }
        }
    }
}
