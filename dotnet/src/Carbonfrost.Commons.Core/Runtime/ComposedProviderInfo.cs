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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Carbonfrost.Commons.Core.Runtime {

    class ComposedProviderInfo : ProviderValueSource {

        private readonly List<ProviderValueSource> _items;

        public ComposedProviderInfo(ProviderValueSource existing, ProviderValueSource item)
            : base(item.ProviderType, existing.Name)
        {
            _items = new List<ProviderValueSource> { existing, item };
        }

        public override object Activate(IEnumerable<KeyValuePair<string, object>> arguments, IServiceProvider services) {
            return _items[0].Activate(arguments, services);
        }

        public override object GetValue() {
            return _items[0].GetValue();
        }

        public override bool IsValue(object instance) {
            return _items[0].IsValue(instance);
        }

        public override Type ValueType {
            get {
                return _items[0].ValueType;
            }
        }

        public override IReadOnlyList<QualifiedName> Names {
            get {
                return _items.Select(t => t.Name).ToList();
            }
        }

        public override MemberInfo Member {
            get {
                return _items[0].Member;
            }
        }

        public override ProviderValueSource AppendOne(ProviderValueSource item) {
            _items.Add(item);
            return this;
        }

        public override ResultAndCriteria DoMatchCriteria(object criteria) {
            double maxCriteriaScore = _items.Max(t => t.DoMatchCriteria(criteria).criteria);
            return new ResultAndCriteria
            {
                result = this,
                criteria = maxCriteriaScore,
            };
        }

        public override bool PreciseMatch(object instance) {
            return _items.Any(t => t.PreciseMatch(instance));
        }

        public override bool IsMatchLocalName(string localName) {
            return _items.Any(t => t.IsMatchLocalName(localName));
        }
    }
}
