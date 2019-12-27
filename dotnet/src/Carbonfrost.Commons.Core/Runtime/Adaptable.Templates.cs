//
// Copyright 2005, 2006, 2010, 2014, 2016, 2019 Carbonfrost Systems, Inc.
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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Carbonfrost.Commons.Core.Runtime {

    public static partial class Adaptable {

        public static TemplatingMode GetTemplatingMode(this PropertyInfo property) {
            if (property == null) {
                throw new ArgumentNullException("property");
            }

            var attr = property.GetCustomAttribute<TemplatingAttribute>();
            if (attr == null) {
                // Check for some built-in templating modes
                if (IsTemplatingHidden(property)) {
                    return TemplatingMode.Hidden;
                }

                return Template.IsImmutable(property.PropertyType)
                    ? TemplatingMode.Copy
                    : TemplatingMode.Default;
            }

            return attr.Mode;
        }

        public static TemplatingMode GetTemplatingMode(this Type type) {
            if (type == null) {
                throw new ArgumentNullException("type");
            }

            var attr = type.GetTypeInfo().GetCustomAttribute<TemplatingAttribute>() ?? TemplatingAttribute.Default;
            return attr.Mode;
        }

        static bool IsTemplatingHidden(PropertyInfo property) {
            // Properties like List<T>.Capacity should not be templated
            if (property.Name == "Capacity") {
                return typeof(ICollection).GetTypeInfo().IsAssignableFrom(property.DeclaringType)
                    || property.DeclaringType.GetTypeInfo().GetInterfaces().Select(t => t.GetTypeInfo())
                    .Any(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>));
            }
            return false;
        }
    }
}
