//
// Copyright 2013, 2019-2020 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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

    [Providers]
    public abstract partial class AdapterFactory : IAdapterFactory {

        private static readonly IDictionary<Assembly, IAdapterFactory> cache
            = new Dictionary<Assembly, IAdapterFactory>();

        public static readonly IAdapterFactory Default = new DefaultAdapterFactoryImpl();

        protected AdapterFactory() {}

        public virtual object GetAdapter(object adaptee, string adapterRoleName) {
            if (adaptee == null) {
                throw new ArgumentNullException("adaptee");
            }
            if (string.IsNullOrEmpty(adapterRoleName)) {
                throw Failure.NullOrEmptyString(nameof(adapterRoleName));
            }

            var type = GetAdapterType(adaptee, adapterRoleName);
            if (type == null) {
                return null;
            }
            return NewInstance(adaptee, type);
        }

        public virtual Type GetAdapterType(object adaptee, string adapterRoleName) {
            if (adaptee == null) {
                throw new ArgumentNullException("adaptee");
            }
            if (adapterRoleName == null) {
                throw new ArgumentNullException("adapterRoleName");
            }
            if (adapterRoleName.Length == 0) {
                throw Failure.EmptyString("adapterRoleName");
            }

            return GetAdapterType(adaptee.GetType(), adapterRoleName);
        }

        public static IAdapterFactory FromName(string name) {
            return App.GetProvider<IAdapterFactory>(name);
        }

        public abstract Type GetAdapterType(Type adapteeType, string adapterRoleName);

        public static IAdapterFactory Compose(params IAdapterFactory[] items) {
            return Utility.OptimalComposite(items,
                                            i => new CompositeAdapterFactoryImpl(i),
                                            Null);
        }

        public static IAdapterFactory Compose(IEnumerable<IAdapterFactory> items) {
            return Utility.OptimalComposite(items,
                                            i => new CompositeAdapterFactoryImpl(i),
                                            Null);
        }

        public static IAdapterFactory FromAssembly(Assembly assembly) {
            if (assembly == null) {
                throw new ArgumentNullException("assembly");
            }

            return cache.GetValueOrCache(assembly, () => FromAssemblyInternal(assembly));
        }

        internal static object NewInstance(object adaptee, Type type) {
            var props = Properties.FromArray(adaptee);
            return Activation.CreateInstance(type, props, ServiceProvider.Current);
        }

        internal static Assembly GetAssemblyThatDefines(string role) {
            return App.DescribeAdapterRoles().GetAdapterRoleInfo(role).DefiningAssembly;
        }

        static IAdapterFactory FromAssemblyInternal(Assembly assembly) {
            if (!AssemblyInfo.GetAssemblyInfo(assembly).ScanForAdapters) {
                return Null;
            }
            // This will contain all factories that are declared.  If any factory has an explicit role, then
            // all adapters in that role must be made available from it (otherwise, the optimization is pointless since we
            // have to fall back to a full scan).

            var all = (AdapterFactoryAttribute[]) assembly.GetCustomAttributes(typeof(AdapterFactoryAttribute));
            var except = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var genericFactories = new List<IAdapterFactory>();  // [assembly: AdapterFactory(typeof(H))]
            var roleFactories = new List<IAdapterFactory>();  // [assembly: StreamingSourceFactory(typeof(H))]

            if (all.Length == 0) {
                return ReflectedAdapterFactory.Create(assembly, Array.Empty<string>());
            }

            foreach (var t in all) {
                var inst = (IAdapterFactory) Activator.CreateInstance(t.AdapterFactoryType);
                if (t.Role == null) {
                    genericFactories.Add(inst);
                } else {
                    except.Add(t.Role); // Don't consider role because it has a factory
                    roleFactories.Add(inst);
                }
            }

            // If no generic factories were defined, then fallback available
            if (genericFactories.Count == 0) {
                genericFactories.Add(ReflectedAdapterFactory.Create(assembly, except));
            }

            // Consider role factories before generic ones
            return Compose(roleFactories.Concat(genericFactories));
        }

    }
}
