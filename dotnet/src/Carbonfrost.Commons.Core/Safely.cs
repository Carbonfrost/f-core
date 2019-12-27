//
// Copyright 2014-2015, 2019 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using System.Linq;

namespace Carbonfrost.Commons.Core {

    static class Safely {

        public static void Dispose(object instance) {
            var d = instance as IDisposable;
            if (d == null) {
                return;
            }

            try {
                d.Dispose();

            } catch (Exception ex) {
                if (Failure.IsCriticalException(ex)) {
                    throw;
                }
            }
        }

        public static void OnDisposed(object instance, EventHandler handler) {
            if (instance == null) {
                return;
            }

            var evt = instance.GetType().GetTypeInfo().GetEvents().FirstOrDefault(e => e.Name == "Disposed" && e.EventHandlerType == typeof(EventHandler));
            if (evt != null) {
                EventHandler removeHandler = (sender, e) => evt.RemoveEventHandler(instance, handler);
                evt.AddEventHandler(instance, Delegate.Combine(handler, removeHandler));
            }
        }
    }
}
