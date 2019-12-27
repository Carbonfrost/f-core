//
// Copyright 2005, 2006, 2010, 2013 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using System.Reflection;

namespace Carbonfrost.Commons.Core.Runtime {

    internal sealed class ReflectionProperties : ReflectionPropertyProvider, IProperties {

        private PropertyChangedEventHandler PropertyChangedValue;

        public ReflectionProperties(object component) : base(component) {
        }

        void OnPropertyChanged(PropertyChangedEventArgs e) {
            if (PropertyChangedValue != null)
                PropertyChangedValue(this, e);
        }

        public override string ToString() {
            return Properties.ToKeyValuePairs(this);
        }

        // IProperties implementation
        public event PropertyChangedEventHandler PropertyChanged {
            add {
                EnsureEventHandler();
                PropertyChangedValue += value;
            }
            remove {
                EnsureEventHandler();
                PropertyChangedValue -= value;
            }
        }

        public void ClearProperties() {
            foreach (PropertyInfo pd in _EnsureProperties().Values)
                _ResetProperty(pd);
        }

        public void ClearProperty(string property) {
            PropertyInfo pd = _GetProperty(property);
            if (pd != null)
                _ResetProperty(pd);
        }

        public bool TrySetProperty(string property, object value) {
            PropertyInfo pd = _GetProperty(property);
            if (pd != null && pd.GetValue(ObjectContext) != value) {
                pd.SetValue(ObjectContext, value);
                return true;
            }
            return false;
        }

        public void SetProperty(string property, object value) {
            PropertyInfo pd = _GetProperty(property);
            if (pd != null && pd.GetValue(ObjectContext) != value) {
                pd.SetValue(ObjectContext, value);
            }
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() {
            foreach (PropertyInfo pd in _EnsureProperties().Values) {
                yield return new KeyValuePair<string, object>(pd.Name, pd.GetValue(ObjectContext));
            }
        }

        // object overrides
        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        private void _ResetProperty(PropertyInfo pd) {
            // TODO May be a better way of resetting without TypeDescriptor
            pd.SetValue(ObjectContext, null);
            OnPropertyChanged(new PropertyChangedEventArgs(pd.Name));
        }

        private void EnsureEventHandler() {
            INotifyPropertyChanged change = this.ObjectContext as INotifyPropertyChanged;
            if (change != null) {
                change.PropertyChanged += (sender, e) => { OnPropertyChanged(e); };
                return;
            }

            var events = ObjectContext.GetType().GetEvents(BindingFlags.Public | BindingFlags.Instance);
            var properties = _EnsureProperties();
            foreach (var ed in events) {
                if (ed.EventHandlerType == typeof(EventHandler) && ed.Name.EndsWith("Changed", StringComparison.Ordinal)) {
                    string property = ed.Name.Substring(0, ed.Name.Length - "Changed".Length);

                    if (property.Length > 0 && properties.ContainsKey(property)) {
                        EventHandler handler = (sender, e) => {
                            OnPropertyChanged(new PropertyChangedEventArgs(property));
                        };

                        ed.AddEventHandler(this.ObjectContext, handler);
                    }
                }
            }

        }

    }
}
