//
// Copyright 2016 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using System.Collections.ObjectModel;

namespace Carbonfrost.Commons.Core.Runtime {

    public static partial class App {

        public static IEnumerable<Assembly> Assemblies {
            get {
                return DescribeAssemblies();
            }
        }

        public static IReadOnlyList<Assembly> LoadedAssemblies {
            get {
                return LoadedAssembliesImpl();
            }
        }

        public static string BasePath {
            get {
                return AppContext.BaseDirectory;
            }
        }

        public static IEnumerable<string> GetAdapterRoleNames() {
            return DescribeAssemblies(t => t.GetAdapterRoleNames());
        }

        public static IEnumerable<Type> GetStartClasses(string className) {
            return DescribeAssemblies().SelectMany(a => a.GetStartClasses(className));
        }

        public static IEnumerable<TValue> GetStartFields<TValue>(string className) {
            return StartClassInfo.FindStartFields<TValue>(DescribeAssemblies().SelectMany(a => a.GetStartClasses(className)));
        }

        public static Type GetTypeByQualifiedName(QualifiedName name, bool throwOnError) {
            if (name == null) {
                throw new ArgumentNullException("name");
            }
            var tr = TypeReference.FromQualifiedName(name);
            return throwOnError ? tr.Resolve() : tr.TryResolve();
        }

        public static Type GetTypeByQualifiedName(QualifiedName name) {
            return GetTypeByQualifiedName(name, false);
        }

        public static IEnumerable<Type> FilterTypes(Func<Type, bool> predicate) {
            return DescribeTypes(a => (predicate(a) ? new[] { a } : null));
        }

        public static IEnumerable<Assembly> FilterAssemblies(
            Func<Assembly, bool> predicate) {

            return DescribeAssemblies(a => (predicate(a) ? new[] { a } : null));
        }

        public static IEnumerable<Assembly> DescribeAssemblies() {
            return AssemblyObserver.Instance;
        }

        public static IEnumerable<TValue> DescribeAssemblies<TValue>(
            Func<Assembly, IEnumerable<TValue>> selector) {

            if (selector == null) {
                throw new ArgumentNullException("selector");
            }

            return new Buffer<TValue>(
                AssemblyObserver.Instance.SelectMany(t => selector(t) ?? Array.Empty<TValue>()));
        }

        public static IReadOnlyDictionary<TKey, TValue> DescribeAssemblies<TKey, TValue>(
            Func<Assembly, IEnumerable<KeyValuePair<TKey, TValue>>> selector)
        {
            if (selector == null) {
                throw new ArgumentNullException("selector");
            }

            return new BufferDictionary<TKey, TValue>(
                AssemblyObserver.Instance.SelectMany(t => selector(t)));
        }

        public static IEnumerable<Type> DescribeTypes() {
            return new Buffer<Type>(DescribeAssemblies()
                                    .SelectMany(a => a.GetTypesHelper())
                                    .Select(t => t.AsType()));
        }

        public static IEnumerable<TValue> DescribeTypes<TValue>(
            Func<Type, IEnumerable<TValue>> selector)
        {
            if (selector == null) {
                throw new ArgumentNullException("selector");
            }

            var s = AssemblyThunk(selector);
            return AssemblyObserver.Instance.SelectMany(s);
        }

        public static IReadOnlyDictionary<TKey, TValue> DescribeTypes<TKey, TValue>(
            Func<Type, IEnumerable<KeyValuePair<TKey, TValue>>> selector)
        {
            if (selector == null)
                throw new ArgumentNullException("selector");

            var func = AssemblyThunk(selector);
            return new BufferDictionary<TKey, TValue>(AssemblyObserver.Instance.SelectMany(func));
        }

        static Func<Assembly, IEnumerable<TValue>> AssemblyThunk<TValue>(Func<Type, IEnumerable<TValue>> selector) {
            return (a) => (a.GetTypesHelper().SelectMany(s => selector(s.AsType())) ?? Array.Empty<TValue>());
        }

        static IReadOnlyList<Assembly> LoadedAssembliesImpl() {
            return AppDomain.CurrentDomain.GetAssemblies();
        }
    }
}
