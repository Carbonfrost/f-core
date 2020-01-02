//
// Copyright 2016, 2019 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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

using Carbonfrost.Commons.Core;
using Carbonfrost.Commons.Core.Runtime;

namespace Carbonfrost.Commons.Core.Runtime {

    public abstract class Builder<T> {

        public string Provider { get; set; }
        public string Implementation { get; set; }
        public virtual TypeReference Type { get; set; }

        public virtual T Build(IServiceProvider serviceProvider = null) {
            serviceProvider = serviceProvider ?? ServiceProvider.Root;
            Type activatedType = GetActivatedType(serviceProvider);
            return CreateInstanceCore(activatedType, serviceProvider);
        }

        protected virtual Type GetActivatedType(IServiceProvider serviceProvider) {
            string effectiveImpl = GetEffectiveImplementation(serviceProvider);
            Type resolvedType = null;
            if (Type == null) {
                resolvedType = GetProviderType() ?? typeof(T);
            } else {
                resolvedType = Type.Resolve();
            }

            // Use implementations
            if (!string.IsNullOrEmpty(effectiveImpl)) {
                // TypeReference myRef = (resolvedType ?? typeof(T)).GetExtensionImplementationType(effectiveImpl);
                // return (myRef == null) ? resolvedType : myRef.Resolve();
            }

            return resolvedType;
        }

        private Type GetProviderType() {
            if (typeof(T).IsProviderType()) {
                if (!string.IsNullOrWhiteSpace(Provider)) {
                    return App.GetProviderType(typeof(T), Provider);
                }

                var qn = GetQName();
                if (qn != null) {
                    return App.GetProviderType(typeof(T), qn);
                }

                var ion = GetName();
                if (!string.IsNullOrEmpty(ion)) {
                    return App.GetProviderType(typeof(T), ion);
                }
            }

            return null;
        }

        protected virtual string GetEffectiveImplementation(IServiceProvider serviceProvider) {
            return Implementation;
        }

        protected virtual IServiceProvider GetServiceProvider(IServiceProvider parent) {
            return parent ?? ServiceProvider.Root;
        }

        protected virtual T CreateInstanceCore(Type activatedType,
                                               IServiceProvider serviceProvider) {
            if (!typeof(T).GetTypeInfo().IsAssignableFrom(activatedType)) {
                throw Failure.NotAssignableFrom("activatedType", typeof(T), activatedType);
            }

            var values = Properties.FromValue(this);
            var sc = GetServiceProvider(serviceProvider);

            var attributes = GetActivationAttributes(activatedType, serviceProvider) ?? Enumerable.Empty<Attribute>();
            return (T) Activation.CreateInstance(activatedType, values, sc, attributes.ToArray());
        }

        protected virtual IEnumerable<Attribute> GetActivationAttributes(Type activatedType, IServiceProvider serviceProvider) {
            return Array.Empty<Attribute>();
        }

        private string GetName() {
            try {
                PropertyInfo pi = typeof(T).GetProperty("Name");
                if (pi != null) {
                    return Convert.ToString(pi.GetValue(this, null));
                }

            } catch (AmbiguousMatchException) {
            }

            return null;
        }

        private QualifiedName GetQName() {
            try {
                PropertyInfo pi = typeof(T).GetProperty("Name");
                if (pi != null && pi.PropertyType == typeof(QualifiedName)) {
                    return (QualifiedName) pi.GetValue(this, null);
                }

            } catch (AmbiguousMatchException) {
            }

            return null;
        }
    }
}
