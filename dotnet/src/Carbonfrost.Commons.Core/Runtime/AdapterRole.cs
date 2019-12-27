//
// Copyright 2005, 2006, 2010, 2016, 2019 Carbonfrost Systems, Inc.
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
using System.Linq;
using System.Reflection;

namespace Carbonfrost.Commons.Core.Runtime {

    internal static class AdapterRole {

        internal const string Builder = "Builder";
        internal const string StreamingSource = "StreamingSource";
        internal const string ActivationProvider = "ActivationProvider";
        internal const string Null = "Null";
        internal const string Template = "Template";

        internal static bool IsActivationProviderType(Type type) {
            return typeof(IActivationProvider).GetTypeInfo().IsAssignableFrom(type);
        }

        internal static bool IsTemplateType(Type type) {
            return typeof(ITemplate).GetTypeInfo().IsAssignableFrom(type);
        }

        internal static bool IsBuilderType(Type builderType, Type typeToBuild) {
            // Can't itself be a builder type, must define Build
            if (!builderType.GetTypeInfo().IsDefined(typeof(BuilderAttribute), false)) {
                return builderType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Any(m => m.Name == "Build"
                         && IsBuildReturnType(m, builderType, typeToBuild) && IsBuildParameters(m));
            }

            return false;
        }

        static bool IsBuildParameters(MethodInfo t) {
            var pi = t.GetParameters();
            return pi.Length == 0 || (pi.Length == 1 && pi[0].ParameterType == typeof(IServiceProvider));
        }

        static bool IsBuildReturnType(MethodInfo t, Type type, Type typeToBuild) {
            if (type == null) {
                return false;
            }

            var returnType = t.ReturnType;
            if (returnType != null) {
                // HBuilder.Build():H
                // -or- HBuilder.Build():object
                // -or- HBuilder.Build():G -- if H extends G
                return returnType == typeof(object)
                    || returnType.IsAssignableFrom(typeToBuild);
            }

            return false;
        }

        internal static bool IsStreamingSourceType(this Type type) {
            return typeof(StreamingSource).GetTypeInfo().IsAssignableFrom(type);
        }

    }
}
