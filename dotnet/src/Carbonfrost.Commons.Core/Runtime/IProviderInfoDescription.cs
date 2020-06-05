//
// Copyright 2014, 2020 Carbonfrost Systems, Inc. (https://carbonfrost.com)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     https://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Carbonfrost.Commons.Core.Runtime {

    public interface IProviderInfoDescription {

        ProviderInfo GetProviderInfo(Type providerType, object criteria);
        ProviderInfo GetProviderInfo(Type providerType, string name);
        ProviderInfo GetProviderInfo(Type providerType, QualifiedName name);
        IEnumerable<ProviderInfo> GetProviderInfos(Type providerType);

        IEnumerable<MemberInfo> GetProviderMembers(Type providerType);
        IEnumerable<object> GetProviders(Type providerType);
        IEnumerable<object> GetProviders(Type providerType, object criteria);
        IEnumerable<QualifiedName> GetProviderNames(Type providerType);
        IEnumerable<T> GetProviders<T>();
        IEnumerable<T> GetProviders<T>(object criteria);
        IEnumerable<Type> GetProviderTypes();
        IEnumerable<Type> GetProviderTypes(Type providerType);
        MemberInfo GetProviderMember(Type providerType, QualifiedName name);
        MemberInfo GetProviderMember(Type providerType, string name);
        MemberInfo GetProviderMember(Type providerType, object criteria);
        object GetProvider(Type providerType, object criteria);
        object GetProvider(Type providerType, QualifiedName name);
        object GetProvider(Type providerType, string name);
        object GetProviderMetadata(Type providerType, object instance);
        QualifiedName GetProviderName(Type providerType, object instance);
        IEnumerable<QualifiedName> GetProviderNames(Type providerType, object instance);
        object GetProviderMetadata(object instance);
        QualifiedName GetProviderName(object instance);
        T GetProvider<T>(object criteria);
        T GetProvider<T>(QualifiedName name);
        Type GetProviderType(Type providerType, object criteria);
        Type GetProviderType(Type providerType, QualifiedName name);
        Type GetProviderType(Type providerType, string name);
        T GetProvider<T>(string name);
    }
}
