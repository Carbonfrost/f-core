//
// Copyright 2012, 2013, 2016, 2019 Carbonfrost Systems, Inc.
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
using System.Linq;
using System.Reflection;

namespace Carbonfrost.Commons.Core.Runtime {

    partial class App {

        public static IProviderInfoDescription DescribeProviders() {
            return ProviderData.Instance;
        }

        public static IEnumerable<IProviderInfo> GetProviderInfos(Type providerType) {
            return DescribeProviders().GetProviderInfos(providerType);
        }

        public static IProviderInfo GetProviderInfo(Type providerType, QualifiedName name) {
            return DescribeProviders().GetProviderInfo(providerType, name);
        }

        public static IProviderInfo GetProviderInfo(Type providerType, string name) {
            return DescribeProviders().GetProviderInfo(providerType, name);
        }

        public static IProviderInfo GetProviderInfo(Type providerType, object criteria) {
            return DescribeProviders().GetProviderInfo(providerType, criteria);
        }

        public static T GetProvider<T>(string name) {
            return DescribeProviders().GetProvider<T>(name);
        }

        public static T GetProvider<T>(QualifiedName name) {
            return DescribeProviders().GetProvider<T>(name);
        }

        public static T GetProvider<T>(object criteria) {
            return DescribeProviders().GetProvider<T>(criteria);
        }

        public static IEnumerable<T> GetProviders<T>(object criteria) {
            return DescribeProviders().GetProviders<T>(criteria);
        }

        public static object GetProvider(Type providerType, string name) {
            return DescribeProviders().GetProvider(providerType, name);
        }

        public static object GetProvider(Type providerType, QualifiedName name) {
            return DescribeProviders().GetProvider(providerType, name);
        }

        public static object GetProvider(Type providerType, object criteria) {
            return DescribeProviders().GetProvider(providerType, criteria);
        }

        public static IEnumerable<T> GetProviders<T>() {
            return DescribeProviders().GetProviders<T>();
        }

        public static IEnumerable<object> GetProviders(Type providerType) {
            return DescribeProviders().GetProviders(providerType);
        }

        public static IEnumerable<object> GetProviders(Type providerType, object criteria) {
            return DescribeProviders().GetProviders(providerType, criteria);
        }

        public static IEnumerable<Type> GetProviderTypes() {
            return DescribeProviders().GetProviderTypes();
        }

        public static MemberInfo GetProviderMember(Type providerType, string name) {
            return DescribeProviders().GetProviderMember(providerType, name);
        }

        public static MemberInfo GetProviderMember(Type providerType, QualifiedName name) {
            return DescribeProviders().GetProviderMember(providerType, name);
        }

        public static MemberInfo GetProviderMember(Type providerType, object criteria) {
            return DescribeProviders().GetProviderMember(providerType, criteria);
        }

        public static IEnumerable<MemberInfo> GetProviderMembers(Type providerType) {
            return DescribeProviders().GetProviderMembers(providerType);
        }

        public static object GetProviderMetadata(object instance) {
            return DescribeProviders().GetProviderMetadata(instance);
        }

        public static QualifiedName GetProviderName(object instance) {
            return DescribeProviders().GetProviderName(instance);
        }

        public static Type GetProviderType(Type providerType, string name) {
            return DescribeProviders().GetProviderType(providerType, name);
        }

        public static Type GetProviderType(Type providerType, QualifiedName name) {
            return DescribeProviders().GetProviderType(providerType, name);
        }

        public static Type GetProviderType(Type providerType, object criteria) {
            return DescribeProviders().GetProviderType(providerType, criteria);
        }

        public static IEnumerable<Type> GetProviderTypes(Type providerType) {
            return DescribeProviders().GetProviderTypes(providerType);
        }

        public static T GetRequiredProvider<T>(string name) {
            return Required(DescribeProviders().GetProvider<T>(name), typeof(T));
        }

        public static T GetRequiredProvider<T>(QualifiedName name) {
            return Required(DescribeProviders().GetProvider<T>(name), typeof(T));
        }

        public static T GetRequiredProvider<T>(object criteria) {
            return Required(DescribeProviders().GetProvider<T>(criteria), typeof(T));
        }

        public static object GetRequiredProvider(Type providerType, string name) {
            return Required(DescribeProviders().GetProvider(providerType, name), providerType);
        }

        public static object GetRequiredProvider(Type providerType, QualifiedName name) {
            return Required(DescribeProviders().GetProvider(providerType, name), providerType);
        }

        public static object GetRequiredProvider(Type providerType, object criteria) {
            return Required(DescribeProviders().GetProvider(providerType, criteria), providerType);
        }

        static T Required<T>(T obj, Type providerType) {
            if (Equals(obj, default(T))) {
                throw RuntimeFailure.ProviderNotFound(providerType);
            }
            return obj;
        }

        public static QualifiedName GetProviderName(Type providerType, object instance) {
            return DescribeProviders().GetProviderName(providerType, instance);
        }

        public static object GetProviderMetadata(Type providerType, object instance) {
            return DescribeProviders().GetProviderMetadata(providerType, instance);
        }

        public static IEnumerable<QualifiedName> GetProviderNames(Type providerType) {
            return DescribeProviders().GetProviderNames(providerType);
        }

        public static IEnumerable<QualifiedName> GetProviderNames(Type providerType, object instance) {
            return DescribeProviders().GetProviderNames(providerType, instance);
        }

        public static IEnumerable<QualifiedName> GetTemplateNames(Type templateType) {
            return DescribeProviders().GetTemplateNames(templateType);
        }

    }
}
