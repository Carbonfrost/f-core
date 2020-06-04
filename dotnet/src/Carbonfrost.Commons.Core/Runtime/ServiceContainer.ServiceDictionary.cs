//
// Copyright 2014, 2019, 2020 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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

    partial class ServiceContainer {

        // Provides a way to emit a closure that will capture the service
        class OutputHelper<T> {

            private readonly object _value;

            public OutputHelper(object val) {
                this._value = val;
            }

            public T Convert() {
                return (T) _value;
            }
        }

        interface IServiceDictionary : IDisposable {
            IEnumerable<object> Get(Type outputType);
            void Clear();
            void Add(IProviderMetadata serviceMetadata, object serviceInstanceOrFactory);
        }

        static class ServiceDictionary {

            public static IServiceDictionary Create(ServiceContainer sc, Type serviceType) {
                return (IServiceDictionary) Activator.CreateInstance(
                    typeof(ServiceDictionary<>).MakeGenericType(serviceType),
                    new [] { sc }
                );
            }

        }

        class ServiceDictionary<T> : DisposableObject, IServiceDictionary {

            private readonly ServiceContainer _container;
            private readonly List<ServiceDescriptor> _services = new List<ServiceDescriptor>();

            private Type FuncType {
                get {
                    return typeof(Func<T>);
                }
            }

            private Type LazyType {
                get {
                    return typeof(Lazy<T>);
                }
            }

            private Type ServiceType {
                get {
                    return typeof(T);
                }
            }

            public ServiceDictionary(ServiceContainer container) {
                _container = container;
            }

            protected override void Dispose(bool manualDispose) {
                foreach (var item in _services) {
                    item.Release(_container);
                }
            }

            public void Add(IProviderMetadata serviceMetadata, object serviceInstanceOrFactory) {
                _services.Add(DemandType(serviceInstanceOrFactory));
            }

            public void Clear() {
                _services.Clear();
            }

            public IEnumerable<object> Get(Type outputType) {
                Func<object, object> convertForOutput = null;
                if (outputType.GetTypeInfo().IsAssignableFrom(ServiceType)) {
                    convertForOutput = t => t;

                } else if (FuncType == outputType) {

                    convertForOutput = t => {
                        var helper = new OutputHelper<T>(t);
                        return EmitFunc(helper);
                    };

                } else if (LazyType == outputType) {

                    convertForOutput = t => {
                        var helper = new OutputHelper<T>(t);
                        return new Lazy<T>(EmitFunc(helper));
                    };

                } else {
                    throw new NotImplementedException();
                }

                foreach (var instanceOrFactory in _services) {
                    var cache = instanceOrFactory.Unwrap(_container);
                    yield return convertForOutput(cache);
                }
            }

            private Func<T> EmitFunc(OutputHelper<T> closure) {
                return () => closure.Convert();
            }

            private ServiceDescriptor DemandType(object serviceInstance) {
                if (serviceInstance is T) {
                    return ServiceDescriptor.Singleton(serviceInstance);
                }
                if (serviceInstance is Func<IServiceContainer, Type, object> ff) {
                    return ServiceDescriptor.Factory(typeof(T), ff);
                }
                if (serviceInstance is Lazy<T> l) {
                    return ServiceDescriptor.Lazy(l);
                }
                if (serviceInstance is Func<T> fac) {
                    return ServiceDescriptor.Factory(fac);
                }
                throw RuntimeFailure.ServiceContainerAddInvalidServiceDescriptor(typeof(T));
            }
        }
    }

}
