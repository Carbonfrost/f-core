//
// Copyright 2013, 2016, 2019 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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

    internal abstract class ProviderValueSource : IProviderInfo {

        public abstract object GetValue();
        public abstract object Activate(IEnumerable<KeyValuePair<string, object>> arguments,
                                        IServiceProvider services);

        public abstract Type ValueType { get; }
        public abstract MemberInfo Member { get; }

        public virtual Assembly Assembly {
            get {
                return Member.DeclaringType.GetTypeInfo().Assembly;
            }
        }

        public Type ProviderType { get; private set; }
        public IProviderMetadata Metadata { get; set; }

        protected ProviderValueSource(Type providerType, QualifiedName key) {
            this.ProviderType = providerType;
            this.Name = key;
        }

        public virtual ProviderValueSource AppendOne(ProviderValueSource item) {
            return new ComposedProviderInfo(this, item);
        }

        public virtual ResultAndCriteria DoMatchCriteria(object criteria) {
            return new ResultAndCriteria
            {
                result = this,
                criteria = Metadata == null ? 0 : Metadata.MatchCriteria(criteria)
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

        public QualifiedName Name { get; private set; }

        public virtual IReadOnlyList<QualifiedName> Names {
            get {
                return new[] { Name };
            }
        }

        public Type Type { get { return ValueType; } }

        object IProviderInfo.Metadata {
            get {
                return Metadata.Value;
            }
        }

        public override string ToString() {
            return string.Format("{0:C} {1}", Name.ToString("C"), ProviderType.Name);
        }

    }
}
