//
// Copyright 2005, 2006, 2010 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public sealed class ActivationProviderAttribute : AdapterAttribute {

        public ActivationProviderAttribute(Type adapterType)
            : base(adapterType, AdapterRole.ActivationProvider) {
            CheckType();
        }

        public ActivationProviderAttribute(string adapterType)
            : base(adapterType, AdapterRole.ActivationProvider) {
            CheckType();
        }

        void CheckType() {
            if (this.AdapterType == null || !typeof(IActivationProvider).GetTypeInfo().IsAssignableFrom(this.AdapterType))
                throw Failure.NotInstanceOf("adapterType", AdapterType, typeof(IActivationProvider));
        }
    }
}
