//
// Copyright 2020 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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

namespace Carbonfrost.Commons.Core.Runtime {

    partial class ServiceContainer {

        abstract class ServiceDescriptor {

            internal static ServiceDescriptor Singleton(object serviceInstance) {
                return new SingletonDescriptor(serviceInstance);
            }

            internal static ServiceDescriptor Factory(Type serviceType, Func<IServiceContainer, Type, object> ff) {
                return new CreatorCallbackDescriptor(ff, serviceType);
            }

            internal static ServiceDescriptor Factory<T>(Func<T> fac) {
                return new LazyDescriptor<T>(new Lazy<T>(fac));
            }

            internal static ServiceDescriptor Lazy<T>(Lazy<T> lazy) {
                return new LazyDescriptor<T>(lazy);
            }

            public abstract object Unwrap(IServiceContainer parent);

            public virtual void Release(ServiceContainer parent) {
            }
        }

        private class SingletonDescriptor : ServiceDescriptor {
            private readonly object _serviceInstance;

            public SingletonDescriptor(object serviceInstance) {
                _serviceInstance = serviceInstance;
            }

            public override object Unwrap(IServiceContainer parent) {
                return _serviceInstance;
            }

            public override void Release(ServiceContainer parent) {
                parent.ReleaseService(_serviceInstance);
            }
        }

        private class CreatorCallbackDescriptor : ServiceDescriptor {
            private readonly Func<IServiceContainer, Type, object> _ff;
            private readonly Type _serviceType;
            private bool _invoked;
            private object _cachedValue;

            public CreatorCallbackDescriptor(Func<IServiceContainer, Type, object> ff, Type serviceType) {
                _ff = ff;
                _serviceType = serviceType;
            }

            public override object Unwrap(IServiceContainer parent) {
                if (!_invoked) {
                    _invoked = true;
                    _cachedValue = _ff(parent, _serviceType);
                }
                return _cachedValue;
            }

            public override void Release(ServiceContainer parent) {
                if (_cachedValue == null) {
                    return;
                }
                parent.ReleaseService(_cachedValue);
            }
        }

        private class LazyDescriptor<T> : ServiceDescriptor {
            private readonly Lazy<T> _lazy;

            public LazyDescriptor(Lazy<T> lazy) {
                _lazy = lazy;
            }

            public override object Unwrap(IServiceContainer parent) {
                return _lazy.Value;
            }

            public override void Release(ServiceContainer parent) {
                if (_lazy.IsValueCreated) {
                    parent.ReleaseService(_lazy.Value);
                }
            }
        }
    }
}
