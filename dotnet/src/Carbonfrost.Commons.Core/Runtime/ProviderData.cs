//
// Copyright 2012, 2020 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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

    class ProviderData : IProviderInfoDescription {

        private readonly IDictionary<Type, IEnumerable<ProviderValueSource>> _all
            = new Dictionary<Type, IEnumerable<ProviderValueSource>>();

        private readonly ProviderRootBuffer _providerRoots;
        private readonly Buffer<ProviderValueSource> _providers;

        internal static readonly ProviderData Instance = new ProviderData();

        private ProviderData() {
            _providers = new Buffer<ProviderValueSource>(
                AssemblyObserver.Instance.SelectMany(a => ExtractFromTypes(a)));
            _providerRoots = new ProviderRootBuffer(
                AssemblyObserver.Instance.SelectMany(a => GetRootProviderTypes(a)));
        }

        private static IEnumerable<ProviderValueSource> ExtractFromTypes(Assembly a) {
            IProviderRegistration registration = AssemblyInfo.GetAssemblyInfo(a).GetProviderRegistration();
            ProviderRegistrationContext context = new ProviderRegistrationContext(a);
            registration.RegisterProviderTypes(context);

            return context.EnumerateValueSources();
        }

        static IEnumerable<Type> GetRootProviderTypes(Assembly a) {
            IProviderRegistration registration = AssemblyInfo.GetAssemblyInfo(a).GetProviderRegistration();
            ProviderRegistrationContext context = new ProviderRegistrationContext(a);
            registration.RegisterRootProviderTypes(context);

            return context.EnumerateRoots();
        }

        private static IEnumerable<Type> GetAllProviderRootTypes() {
            return Instance._providerRoots;
        }

        private static T GetProvider<T>(
            Type providerType,
            QualifiedName name,
            Func<ProviderValueSource, T> selector) {

            return WhereByName(Instance.GetProviderData(providerType), name)
                .Select(selector)
                .FirstOrDefault();
        }

        internal static IEnumerable<T> GetProvidersByLocalName<T>(
            Type providerType,
            string localName,
            Func<ProviderValueSource, T> selector) {

            // TODO Inheritance -- Get by IAdapterFactory should return all derived ones

            return Instance.GetProviderData(providerType)
                .Where(t => t.IsMatchLocalName(localName))
                .Select(selector);
        }

        internal static Type GetProviderType(
            Type providerType,
            QualifiedName name) {

            // TODO Convert from linear time to O(1) if possible
            var r = WhereByName(Instance.GetProviderData(providerType), name).FirstOrDefault();
            if (r == null)
                return null;

            return r.ValueType;
        }

        static IEnumerable<ProviderValueSource> WhereByName(IEnumerable<ProviderValueSource> source, QualifiedName name) {
            return source.Where(t => t.Name.EqualsIgnoreCase(name));
        }

        internal static IEnumerable<Type> GetProviderTypes(Type providerType) {
            return Instance.GetProviderData(providerType).Select(t => t.ValueType);
        }

        private Type SingleProviderType(object instance) {
            if (instance == null)
                throw new ArgumentNullException("instance");

            // TODO Might be possible for a provider to implement a type that it isn't a provider for
            // via registration or define exports (rare)

            return this._providerRoots.Where(
                t => t.IsInstanceOfType(instance))
                .SingleOrThrow(RuntimeFailure.MultipleProviderTypes);
        }

        internal static IEnumerable<QualifiedName> GetProviderNames(Type providerType) {
            return Instance.GetProviderData(providerType).SelectMany(t => t.Names);
        }

        private static IEnumerable<object> GetProviders(Type providerType) {
            return GetProviders<object>(providerType, t => t.GetValue());
        }

        private static IEnumerable<T> GetProviders<T>(Type providerType, Func<ProviderValueSource, T> selector) {
            return Instance.GetProviderData(providerType).Select(selector);
        }

        private IEnumerable<ProviderValueSource> GetProviderData(Type providerType) {
            if (!ContainsRootProviderType(providerType)) {
                return Enumerable.Empty<ProviderValueSource>();
            }
            IEnumerable<ProviderValueSource> result;
            if (_all.TryGetValue(providerType, out result)) {
                return result;
            }

            _all[providerType] = result = new Buffer<ProviderValueSource>(
                _providers.Where(p => p.ProviderType == providerType));
            return result;
        }

        private bool ContainsRootProviderType(Type providerType) {
            // Enumerating the AssemblyObserver will load new assemblies
            foreach (var asm in AssemblyObserver.Instance) {
                if (_providerRoots.Contains(providerType)) {
                    return true;
                }
            }
            return false;
        }

        private static IEnumerable<T> GetProvidersUsingCriteria<T>(Type providerType,
                                                                   object criteria,
                                                                   Func<ProviderValueSource, T> selector)
        {
            return Instance.GetProviderData(providerType)
                .Select(t => t.DoMatchCriteria(criteria))
                .OrderByDescending(t => t.criteria)
                .Where(t => t.criteria > 0)
                .Select(t => selector(t.result));
        }

        private static T GetProviderMetadata<T>(Type providerType,
                                                Func<ProviderValueSource, bool> filter,
                                                Func<ProviderValueSource, T> selector)
        {
            return GetProviderMetadataMany<T>(providerType, filter, selector).FirstOrDefault();
        }

        private static IEnumerable<T> GetProviderMetadataMany<T>(Type providerType,
                                                                 Func<ProviderValueSource, bool> filter,
                                                                 Func<ProviderValueSource, T> selector)
        {
            return Instance.GetProviderData(providerType).Where(filter).Select(selector);
        }

        // IProviderInfoDescription implementation

        IEnumerable<MemberInfo> IProviderInfoDescription.GetProviderMembers(Type providerType) {
            return ProviderData.GetProviders( providerType, t => t.Member);
        }

        IEnumerable<object> IProviderInfoDescription.GetProviders(Type providerType) {
            if (providerType == null)
                throw new ArgumentNullException("providerType");

            return ProviderData.GetProviders(providerType);
        }

        IEnumerable<object> IProviderInfoDescription.GetProviders(Type providerType, object criteria) {
            if (providerType == null)
                throw new ArgumentNullException("providerType");

            return ProviderData.GetProvidersUsingCriteria(providerType, criteria, t => t.GetValue());
        }

        IEnumerable<QualifiedName> IProviderInfoDescription.GetProviderNames(Type providerType) {
            if (providerType == null) {
                throw new ArgumentNullException("providerType");
            }

            return ProviderData.GetProviderNames(providerType);
        }

        IEnumerable<QualifiedName> IProviderInfoDescription.GetTemplateNames(Type templateType) {
            if (templateType == null)
                throw new ArgumentNullException("templateType");

            return TemplateData.GetTemplateNames(templateType);
        }

        IEnumerable<T> IProviderInfoDescription.GetProviders<T>() {
            return ProviderData.GetProviders(typeof(T)).Cast<T>();
        }

        IEnumerable<T> IProviderInfoDescription.GetProviders<T>(object criteria) {
            return ((IProviderInfoDescription) this).GetProviders(typeof(T), criteria).Cast<T>();
        }

        IEnumerable<Type> IProviderInfoDescription.GetProviderTypes() {
            return ProviderData.GetAllProviderRootTypes();
        }

        IEnumerable<Type> IProviderInfoDescription.GetProviderTypes(Type providerType) {
            return ProviderData.GetProviderTypes(providerType);
        }

        MemberInfo IProviderInfoDescription.GetProviderMember(Type providerType, QualifiedName name) {
            return ProviderData.GetProvider(providerType, name, t => t.Member);
        }

        MemberInfo IProviderInfoDescription.GetProviderMember(Type providerType, string name) {
            return ProviderData.GetProvidersByLocalName(providerType, name, t => t.Member).SingleOrThrow(RuntimeFailure.MultipleProviders);
        }

        MemberInfo IProviderInfoDescription.GetProviderMember(Type providerType, object criteria) {
            if (providerType == null) {
                throw new ArgumentNullException("providerType");
            }
            if (criteria == null) {
                throw new ArgumentNullException("criteria");
            }

            string s = criteria as string;
            if (s != null)
                return ((IProviderInfoDescription) this).GetProviderMember(providerType, s);

            QualifiedName t = criteria as QualifiedName;
            if (t != null)
                return ((IProviderInfoDescription) this).GetProviderMember(providerType, t);

            return ProviderData.GetProvidersUsingCriteria(providerType, criteria, u => u.Member)
                .SingleOrThrow(RuntimeFailure.MultipleProviders);
        }

        object IProviderInfoDescription.GetProviderMetadata(object instance) {
            return ((IProviderInfoDescription) this).GetProviderMetadata(SingleProviderType(instance), instance);
        }

        QualifiedName IProviderInfoDescription.GetProviderName(object instance) {
            return ((IProviderInfoDescription) this).GetProviderName(SingleProviderType(instance), instance);
        }

        object IProviderInfoDescription.GetProvider(Type providerType, object criteria) {
            if (providerType == null) {
                throw new ArgumentNullException("providerType");
            }
            if (criteria == null) {
                throw new ArgumentNullException("criteria");
            }

            string s = criteria as string;
            if (s != null) {
                return ((IProviderInfoDescription)this).GetProvider(providerType, s);
            }

            QualifiedName t = criteria as QualifiedName;
            if (t != null) {
                return ((IProviderInfoDescription)this).GetProvider(providerType, t);
            }

            return ProviderData.GetProvidersUsingCriteria(providerType, criteria, u => u.GetValue()).FirstOrDefault();
        }

        object IProviderInfoDescription.GetProvider(Type providerType, QualifiedName name) {
            if (providerType == null) {
                throw new ArgumentNullException("providerType");
            }

            return ProviderData.GetProvider(providerType, name, t => t.GetValue());
        }

        object IProviderInfoDescription.GetProvider(Type providerType, string name) {
            if (providerType == null) {
                throw new ArgumentNullException("providerType");
            }

            return ProviderData.GetProvidersByLocalName(providerType, name, t => t.GetValue()).SingleOrThrow(RuntimeFailure.MultipleProviders);
        }

        object IProviderInfoDescription.GetProviderMetadata(Type providerType, object instance) {
            if (providerType == null) {
                throw new ArgumentNullException("providerType");
            }
            if (instance == null) {
                throw new ArgumentNullException("instance");
            }

            return ProviderData.GetProviderMetadata(providerType,
                                                    t => t.PreciseMatch(instance),
                                                    t => t.Metadata);
        }

        QualifiedName IProviderInfoDescription.GetProviderName(Type providerType, object instance) {
            if (providerType == null) {
                throw new ArgumentNullException("providerType");
            }
            if (instance == null) {
                throw new ArgumentNullException("instance");
            }

            return ProviderData.GetProviderMetadata(providerType,
                                                    t => t.PreciseMatch(instance),
                                                    t => t.Name);
        }

        IEnumerable<QualifiedName> IProviderInfoDescription.GetProviderNames(Type providerType, object instance) {
            if (providerType == null) {
                throw new ArgumentNullException("providerType");
            }
            if (instance == null) {
                throw new ArgumentNullException("instance");
            }

            return ProviderData.GetProviderMetadataMany(providerType,
                                                        t => t.PreciseMatch(instance),
                                                        t => t.Names)
                .SelectMany(t => t);
        }

        T IProviderInfoDescription.GetProvider<T>(object criteria) {
            return (T) ProviderData.GetProvidersUsingCriteria(typeof(T), criteria, t => t.GetValue()).FirstOrDefault();
        }

        T IProviderInfoDescription.GetProvider<T>(QualifiedName name) {
            return (T) ProviderData.GetProvider(typeof(T), name, t => t.GetValue());
        }

        Type IProviderInfoDescription.GetProviderType(Type providerType, object criteria) {
            if (criteria == null)
                throw new ArgumentNullException("criteria");

            string s = criteria as string;
            if (s != null) {
                return ((IProviderInfoDescription)this).GetProviderType(providerType, s);
            }

            QualifiedName t = criteria as QualifiedName;
            if (t != null) {
                return ((IProviderInfoDescription)this).GetProviderType(providerType, t);
            }

            return ProviderData.GetProvidersUsingCriteria(providerType, criteria, u => u.ValueType).FirstOrDefault();
        }

        Type IProviderInfoDescription.GetProviderType(Type providerType, QualifiedName name) {
            return ProviderData.GetProviderType(providerType, name);
        }

        Type IProviderInfoDescription.GetProviderType(Type providerType, string name) {
            return ProviderData.GetProvidersByLocalName(providerType, name, t => t.ValueType).SingleOrThrow(RuntimeFailure.MultipleProviders);
        }

        T IProviderInfoDescription.GetProvider<T>(string name) {
            return (T) ProviderData.GetProvidersByLocalName(
                typeof(T), name, t => t.GetValue()).SingleOrThrow(RuntimeFailure.MultipleProviders);
        }

        IProviderInfo IProviderInfoDescription.GetProviderInfo(Type type, QualifiedName name) {
            if (name == null) {
                throw new ArgumentNullException("name");
            }

            return ProviderData.GetProvider(type, name, t => t);
        }

        IProviderInfo IProviderInfoDescription.GetProviderInfo(Type type, string name) {
            if (string.IsNullOrEmpty(name)) {
                throw Failure.NullOrEmptyString(nameof(name));
            }

            return ProviderData.GetProvidersByLocalName(
                type, name, t => t).SingleOrThrow(RuntimeFailure.MultipleProviders);
        }

        IProviderInfo IProviderInfoDescription.GetProviderInfo(Type type, object criteria) {
            if (criteria == null) {
                throw new ArgumentNullException("criteria");
            }

            var them = ProviderData.GetProvidersUsingCriteria(
                type, criteria, t => t).ToList();
            return them.SingleOrThrow(RuntimeFailure.MultipleProviders);
        }

        IEnumerable<IProviderInfo> IProviderInfoDescription.GetProviderInfos(Type providerType) {
            return ProviderData.GetProviders(providerType, t => t);
        }

        class ProviderRootBuffer : Buffer<Type> {

            public readonly HashSet<Type> Items = new HashSet<Type>();
            public ProviderRootBuffer(IEnumerable<Type> types)
                : base(types) {}

            protected override void OnCacheValue(Type current) {
                Items.Add(current);
                base.OnCacheValue(current);
            }
        }
    }
}
