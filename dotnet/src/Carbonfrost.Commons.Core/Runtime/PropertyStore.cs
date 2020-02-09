//
// Copyright 2016, 2020 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using System.Collections.Generic;
using System.ComponentModel;

namespace Carbonfrost.Commons.Core.Runtime {

    public abstract class PropertyStore : PropertyProvider, IPropertyStore {

        public event PropertyChangedEventHandler PropertyChanged;

        private bool _raiseEvents = true;

        public bool RaiseEvents {
            get { return _raiseEvents; }
            set { _raiseEvents = value; }
        }

        public static IPropertyStore Compose(IEnumerable<IPropertyStore> items) {
            return Utility.OptimalComposite(
                items,
                myItems => new CompositePropertyStore(myItems),
                Properties.Null
            );
        }

        public static IPropertyStore Compose(params IPropertyStore[] items) {
            return Compose((IEnumerable<IPropertyStore>) items);
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e) {
            var handler = PropertyChanged;
            if (RaiseEvents && handler != null) {
                handler(this, e);
            }
        }

        public abstract IEnumerator<KeyValuePair<string, object>> GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}
