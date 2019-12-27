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
using System.Reflection;
using System.Reflection.Emit;

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

        class ServiceDictionary : DisposableObject {

            private readonly Type _type;
            private readonly ServiceContainer _container;
            private readonly List<object> _services = new List<object>();

            private Type FuncType {
                get {
                    return typeof(Func<>).MakeGenericType(_type);
                }
            }

            private Type LazyType {
                get {
                    return typeof(Lazy<>).MakeGenericType(_type);
                }
            }

            private Type OutputHelperType {
                get {
                    return typeof(OutputHelper<>).MakeGenericType(_type);
                }
            }

            public ServiceDictionary(ServiceContainer container, Type type) {
                _type = type;
                _container = container;
            }

            protected override void Dispose(bool manualDispose) {
                foreach (var item in _services) {
                    _container.ReleaseService(item);
                }
            }

            public void Add(IProviderMetadata serviceMetadata, object serviceInstanceOrFactory) {
                DemandType(serviceInstanceOrFactory);
                _services.Add(serviceInstanceOrFactory);
            }

            public void Clear() {
                _services.Clear();
            }

            public IEnumerable<object> Get(Type outputType) {
                Func<object, object> convertForOutput = t => t;
                if (outputType.GetTypeInfo().IsAssignableFrom(_type)) {
                    convertForOutput = t => t;

                } else if (FuncType == outputType) {

                    convertForOutput = t => {
                        var helper = Activator.CreateInstance(OutputHelperType, t);
                        return EmitFunc(helper);
                    };

                } else if (LazyType == outputType) {

                    convertForOutput = t => {
                        var helper = Activator.CreateInstance(OutputHelperType, t);
                        return Activator.CreateInstance(LazyType, EmitFunc(helper));
                    };

                } else {
                    throw new NotImplementedException();
                }

                foreach (var instanceOrFactory in _services) {
                    var cache = Unwrap(instanceOrFactory);
                    yield return convertForOutput(cache);
                }
            }

            private Delegate EmitFunc(object closure) {
                var method = closure.GetType().GetMethod("Convert");
                return method.CreateDelegate(FuncType, closure);
            }

            private object Unwrap(object service) {
                var callback = service as Func<IServiceContainer, Type, object>;
                if (callback != null) {
                    service = callback(_container, _type);
                    if (service != null && !_type.IsInstanceOfType(service)) {
                        service = null;
                    }
                }

                return service;
            }

            private void DemandType(object serviceInstance) {
                if (_type.IsInstanceOfType(serviceInstance))
                    return;
                if (serviceInstance is Func<IServiceContainer, Type, object>)
                    return;
                if (LazyType.IsInstanceOfType(serviceInstance))
                    return;
                if (FuncType.IsInstanceOfType(serviceInstance))
                    return;

                throw new NotImplementedException();
            }
        }
    }

}
