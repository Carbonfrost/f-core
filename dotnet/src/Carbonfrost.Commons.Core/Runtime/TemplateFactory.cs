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
using System.Reflection;

namespace Carbonfrost.Commons.Core.Runtime {

    public class TemplateFactory : AdapterFactory<ITemplate> {

        private static readonly TemplateFactory _default = new TemplateFactory(AdapterFactory.Default);

        public TemplateFactory(IAdapterFactory implementation) : base(AdapterRole.Template, implementation) {
        }

        public static TemplateFactory Default {
            get {
                return _default;
            }
        }

        public static TemplateFactory FromAssembly(Assembly assembly) {
            return new TemplateFactory(AdapterFactory.FromAssembly(assembly));
        }

        public Type GetTemplateType(object adaptee) {
            return GetAdapterType(adaptee);
        }

        public Type GetTemplateType(Type adapteeType) {
            return GetAdapterType(adapteeType);
        }
    }
}



