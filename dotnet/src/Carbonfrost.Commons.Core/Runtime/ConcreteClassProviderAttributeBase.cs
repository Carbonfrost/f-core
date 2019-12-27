//
// Copyright 2012, 2014 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using System.Reflection;

namespace Carbonfrost.Commons.Core.Runtime {

    public abstract class ConcreteClassProviderAttributeBase : Attribute, IConcreteClassProvider {

        protected ConcreteClassProviderAttributeBase() {}

        public Type GetConcreteClass(Type sourceType, IServiceProvider serviceProvider) {
            if (sourceType == null)
                throw new ArgumentNullException("sourceType");

            serviceProvider = serviceProvider ?? ServiceProvider.Null;
            Type result = GetConcreteClassCore(sourceType, serviceProvider);
            return VerifyConcreteClass(sourceType, result);
        }

        internal static Type VerifyConcreteClass(Type sourceType, Type resultType) {
            if (resultType == null) {
                return null;
            }
            var result = resultType.GetTypeInfo();
            if (result != null && (result.IsAbstract || result.IsInterface || !sourceType.GetTypeInfo().IsAssignableFrom(result))) {
                throw RuntimeFailure.ConcreteClassError(resultType);
            }
            return resultType;
        }

        protected abstract Type GetConcreteClassCore(Type sourceType, IServiceProvider serviceProvider);
    }
}
