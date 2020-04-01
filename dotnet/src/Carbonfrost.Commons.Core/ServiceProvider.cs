//
// Copyright 2005, 2006, 2010, 2014, 2019 Carbonfrost Systems, Inc.
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
using System.Threading;
using Carbonfrost.Commons.Core.Runtime;

namespace Carbonfrost.Commons.Core {

    public static partial class ServiceProvider {

        public static readonly IServiceProvider Null = new NullServiceProvider();
        public static readonly IServiceProvider Root = new RootServiceProvider();

        static readonly ThreadLocal<Stack<IServiceProvider>> _current
            = new ThreadLocal<Stack<IServiceProvider>>(() => new Stack<IServiceProvider>());

        public static IServiceProvider Current {
            get {
                if (_current.Value.Count == 0) {
                    return ServiceProvider.Root;
                }

                return _current.Value.Peek();
            }
        }

        public static IServiceContainer AddService<T>(this IServiceContainer serviceContainer) {
            if (serviceContainer == null) {
                throw new ArgumentNullException("serviceContainer");
            }

            Func<IServiceContainer, Type, object> callback = (c, t) => Activation.CreateInstance(t, serviceProvider: c);
            serviceContainer.AddService(typeof(T), callback);
            return serviceContainer;
        }

        public static IServiceContainer AddService<T>(this IServiceContainer serviceContainer, Func<T> serviceFactory) {
            if (serviceContainer == null) {
                throw new ArgumentNullException("serviceContainer");
            }

            Func<IServiceContainer, Type, object> callback = (c, t) => serviceFactory();
            serviceContainer.AddService(typeof(T), callback);
            return serviceContainer;
        }

        public static void AddService<T>(this IServiceContainer serviceContainer, T serviceInstance) where T : class {
            if (serviceContainer == null) {
                throw new ArgumentNullException("serviceContainer");
            }
            if (serviceInstance == null) {
                throw new ArgumentNullException("serviceInstance");
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

        public static IServiceProvider FromValue(object value) {
            if (value == null) {
                return Null;
            }

            IServiceProvider s = value as IServiceProvider;
            if (s == null) {
                return new ServiceProviderValue(value);
            }

            return s;
        }

        public static IServiceProvider Filtered(IServiceProvider serviceProvider, Func<Type, bool> typeFilter) {
            if (serviceProvider == null || serviceProvider == Null) {
                return Null;
            }
            if (typeFilter == null) {
                return serviceProvider;
            }

            return new FilteredServiceProvider(typeFilter, serviceProvider);
        }

        public static IServiceProvider Filtered(IServiceProvider serviceProvider, params Type[] types) {
            if (serviceProvider == null || serviceProvider == Null) {
                return Null;
            }
            if (types == null || types.Length == 0) {
                return serviceProvider;
            }

            ICollection<Type> myTypes;
            if (types.Length > 8) {
                myTypes = new HashSet<Type>(types);
            }
            else {
                myTypes = new List<Type>(types);
            }

            return Filtered(serviceProvider, t => myTypes.Contains(t));
        }

        public static IServiceProvider FromValue(object value, params Type[] types) {
            if (value == null) {
                throw new ArgumentNullException("value");
            }

            return Filtered(FromValue(value), types);
        }

        public static IServiceProvider Compose(IEnumerable<IServiceProvider> items) {
            return Utility.OptimalComposite(items,
                                            i => new CompositeServiceProvider(i),
                                            Null);
        }

        public static IServiceProvider Compose(params IServiceProvider[] items) {
            return Utility.OptimalComposite(items,
                                            i => new CompositeServiceProvider(i),
                                            Null);
        }

        class NullServiceProvider : IServiceProvider {

            public object GetService(Type serviceType) {
                if (serviceType == null) {
                    throw new ArgumentNullException("serviceType");
                }
                return null;
            }
        }

        class CompositeServiceProvider : IServiceProvider {

            private readonly IReadOnlyCollection<IServiceProvider> _items;

            public CompositeServiceProvider(IReadOnlyCollection<IServiceProvider> items) {
                _items = items;
            }

            public object GetService(Type serviceType) {
                if (serviceType == null) {
                    throw new ArgumentNullException("serviceType");
                }

                foreach (var s in _items) {
                    object result = s.GetService(serviceType);
                    if (result != null) {
                        return result;
                    }
                }
                return null;
            }
        }

        sealed class ServiceProviderValue : IServiceProvider {

            private readonly object _value;

            public ServiceProviderValue(object value) {
                _value = value;
            }

            public object GetService(Type serviceType) {
                if (serviceType == null) {
                    throw new ArgumentNullException("serviceType");
                }

                return _value.TryAdapt(serviceType, null);
            }
        }

        sealed class FilteredServiceProvider : IServiceProvider {

            private readonly Func<Type, bool> _types;
            private readonly IServiceProvider _sp;

            public FilteredServiceProvider(Func<Type, bool> types, IServiceProvider sp) {
                _sp = sp;
                _types = types;
            }

            public object GetService(Type serviceType) {
                if (serviceType == null) {
                    throw new ArgumentNullException("serviceType");
                }
                if (_types(serviceType)) {
                    return _sp.GetService(serviceType);
                }

                return null;
            }
        }

        internal static void PushCurrent(IServiceProvider serviceProvider) {
            _current.Value.Push(serviceProvider);
        }

        internal static void PopCurrent() {
            _current.Value.Pop();
        }
    }
}
