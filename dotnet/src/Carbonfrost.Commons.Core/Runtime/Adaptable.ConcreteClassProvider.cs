//
// Copyright 2014, 2016, 2019 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using System.Linq;
using System.Reflection;

namespace Carbonfrost.Commons.Core.Runtime {

    public static partial class Adaptable {

        public static IConcreteClassProvider GetConcreteClassProvider(this Type type) {
            if (type == null) {
                throw new ArgumentNullException("type");
            }

            return type.GetTypeInfo().GetCustomAttributes(false).OfType<IConcreteClassProvider>().FirstOrDefault();
        }

        public static IConcreteClassProvider GetConcreteClassProvider(this PropertyInfo property) {
            if (property == null) {
                throw new ArgumentNullException("property");
            }

            return property.GetCustomAttributes().OfType<IConcreteClassProvider>().FirstOrDefault();
        }

        public static Type GetConcreteClass(this PropertyInfo property, IServiceProvider serviceProvider = null) {
            if (property == null)
                throw new ArgumentNullException("property"); // $NON-NLS-1

            var cca = GetConcreteClassProvider(property);
            var type = property.PropertyType.GetTypeInfo();
            if (cca == null) {
                return (type.IsAbstract || type.IsInterface) ? null : type.AsType();
            }

            return VerifyConcreteClass(property.PropertyType, cca.GetConcreteClass(property.PropertyType, serviceProvider));
        }

        public static Type GetConcreteClass(this Type type, IServiceProvider serviceProvider = null) {
            if (type == null) {
                throw new ArgumentNullException("type");
            }

            var cca = GetConcreteClassProvider(type);
            var tt = type.GetTypeInfo();
            if (cca == null) {
                return (tt.IsAbstract || tt.IsInterface) ? null : type;
            }

            return VerifyConcreteClass(type, cca.GetConcreteClass(type, serviceProvider));
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
    }
}
