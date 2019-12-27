//
// Copyright 2012 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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

    [AttributeUsage(AttributeTargets.Class
                    | AttributeTargets.Struct
                    | AttributeTargets.Field
                    | AttributeTargets.Method
                    | AttributeTargets.Interface, AllowMultiple = true)]
    public class ProviderAttribute : Attribute, IProviderMetadata {

        public Type ProviderType { get; private set; }
        public string Name { get; set; }

        ProviderValueSource IProviderMetadata.Source { get; set; }

        object IProviderMetadata.Value {
            get { return this; } }

        public ProviderAttribute(Type providerType) {
            if (providerType == null)
                throw new ArgumentNullException("providerType");

            this.ProviderType = providerType;
        }

        protected virtual int MatchCriteriaCore(object criteria) {
            return ProviderMetadataWrapper.MemberwiseEquals(this, criteria);
        }

        public int MatchCriteria(object criteria) {
            return MatchMemberCriteria(criteria) + MatchCriteriaCore(criteria);
        }

        private int MatchMemberCriteria(object criteria) {
            return ProviderMetadataWrapper.MatchMemberCriteria(
                ((IProviderMetadata) this).Source, criteria);
        }

        protected virtual IEnumerable<string> GetDefaultProviderNames(Type type) {
            return null;
        }

        internal IEnumerable<QualifiedName> GetNames(Type type) {
            var qn = type.GetQualifiedName();
            var ns = qn.Namespace;
            var names =
                SelectNames()
                .Concat(GetDefaultProviderNames(type) ?? Empty<string>.Array)
                .Where(t => !string.IsNullOrEmpty(t))
                .Select(t => ns + t);

            return names
                .Concat((new[] { qn }))
                .Distinct(QualifiedNameComparer.IgnoreCaseLocalName);
        }

        IEnumerable<string> SelectNames() {
            if (string.IsNullOrEmpty(this.Name))
                return Empty<string>.Array;

            IEnumerable<string> names = this.Name.Split(
                new [] {
                    ' ', ',', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return names;
        }

        internal IEnumerable<QualifiedName> GetNames(Type declaringType, string fieldOrProperty) {
            var qn = declaringType.GetQualifiedName();
            qn = qn.ChangeLocalName(Utility.Camel(fieldOrProperty));

            var ns = qn.Namespace;
            var names = SelectNames().Select(t => ns + t);

            return (new[] { qn })
                .Concat(names)
                .Distinct(QualifiedNameComparer.IgnoreCaseLocalName);
        }
    }
}
