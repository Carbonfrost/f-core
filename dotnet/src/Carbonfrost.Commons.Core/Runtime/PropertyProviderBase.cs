//
// Copyright 2005, 2006, 2010, 2019 Carbonfrost Systems, Inc.
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
using System.Reflection;

namespace Carbonfrost.Commons.Core.Runtime {

    abstract class PropertyProviderBase : IPropertyProvider {

        protected abstract bool TryGetPropertyCore(string property, Type propertyType, out object value);

        public Type GetPropertyType(string property) {
            CheckProperty(property);

            object objValue;
            if (TryGetProperty(property, typeof(object), out objValue)) {
                if (objValue == null)
                    return typeof(object);
                else
                    return objValue.GetType();
            } else
                throw RuntimeFailure.PropertyNotFound("property", property); // $NON-NLS-1
        }

        public bool TryGetProperty(string property, Type propertyType, out object value) {
            CheckProperty(property);
            object objValue;

            try {
                bool result = TryGetPropertyCore(property, propertyType, out objValue);
                value = objValue;
                return result;

            } catch (TargetInvocationException ex) {
                if (ex.InnerException is KeyNotFoundException
                    || ex.InnerException is ArgumentException) {
                        value = null;
                        return false;
                }

                throw;
            }
        }

        static void CheckProperty(string property) {
            if (property == null) {
                throw new ArgumentNullException("property");
            }
            if (string.IsNullOrEmpty(property)) {
                throw Failure.EmptyString("property");
            }
        }
    }
}
