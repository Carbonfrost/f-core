//
// Copyright 2013, 2016, 2019-2020 Carbonfrost Systems, Inc. (https://carbonfrost.com)
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

namespace Carbonfrost.Commons.Core.Runtime {

    internal abstract class ProviderValueSource : ProviderInfo {

        // TODO Consider pushing up GetValue() and IsValue()

        private readonly Type _providerType;
        private readonly QualifiedName _name;

        public abstract Type ValueType { get; }

        public sealed override Type ProviderType {
            get {
                return _providerType;
            }
        }

        protected ProviderValueSource(Type providerType, QualifiedName key) {
            _providerType = providerType;
            _name = key;
        }

        public virtual ProviderValueSource AppendOne(ProviderValueSource item) {
            return new ComposedProviderInfo(this, item);
        }

        public virtual ResultAndCriteria DoMatchCriteria(object criteria) {
            return new ResultAndCriteria {
                result = this,
                criteria = Metadata == null ? 0 : ProviderMetadata.MatchCriteria(criteria)
            };
        }

        public virtual bool PreciseMatch(object instance) {
            return object.ReferenceEquals(Member, instance) || IsValue(instance);
        }

        internal struct ResultAndCriteria {
            public ProviderValueSource result;
            public double criteria;
        }

        // Gets whether the specified instance corresponds to this (if it is
        // a singleton provider value source, not a factory one)
        public abstract bool IsValue(object instance);

        public virtual bool IsMatchLocalName(string localName) {
            return string.Equals(Name.LocalName, localName, StringComparison.OrdinalIgnoreCase);
        }

        public sealed override QualifiedName Name {
            get {
                return _name;
            }
        }

        public sealed override Type Type { get { return ValueType; } }

        public IProviderMetadata ProviderMetadata { get; set; }

        public sealed override object Metadata {
            get {
                return ProviderMetadata.Value;
            }
        }

        public abstract object GetValue();

        public override string ToString() {
            return string.Format("{0:C} {1}", Name.ToString("C"), ProviderType.Name);
        }

    }
}
