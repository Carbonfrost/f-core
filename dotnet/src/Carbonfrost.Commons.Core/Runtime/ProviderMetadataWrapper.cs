//
// Copyright 2013, 2016 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using System.Linq;
using System.Reflection;

namespace Carbonfrost.Commons.Core.Runtime {

    sealed class ProviderMetadataWrapper : IProviderMetadata {

        readonly object value;

        public ProviderMetadataWrapper(object value) {
            this.value = value;
        }

        public ProviderValueSource Source { get; set; }

        public int MatchCriteria(object criteria) {
            return MatchMemberCriteria(this.Source, criteria)
                + ProviderMetadataWrapper.MemberwiseEquals(value, criteria);
        }

        internal static int MatchMemberCriteria(ProviderValueSource source, object criteria) {
            if (criteria == null)
                return 0;
            var memberInfo = criteria as MemberInfo;
            if (memberInfo != null) {
                return MemberCriteria(memberInfo, source);
            }
            var assembly = criteria as Assembly;
            if (assembly != null) {
                return AssemblyCriteria(assembly, source);
            }

            var cpp = Properties.FromValue(criteria);

            int result = 0;

            // Match member and assembly
            var asm = criteria as Assembly ?? cpp.GetProperty("Assembly", (Assembly) null);
            var mem = criteria as MemberInfo ?? cpp.GetProperty("Member", (MemberInfo) null);

            if (asm != null) {

                result += AssemblyCriteria(asm, source);
            }
            if (mem != null) {
                result += MemberCriteria(mem, source);
            }

            return result;
        }

        static int MemberCriteria(MemberInfo mem, IProviderInfo source) {
            return mem == source.Member ? 1 : 0;
        }

        static int AssemblyCriteria(Assembly asm, IProviderInfo source) {
            return asm.GetName().FullName
                == source.Assembly.GetName().FullName ? 1 : 0;
        }

        internal static int MemberwiseEquals(object criteria, object other) {
            var pp = (other == null ? PropertyProvider.Null : PropertyProvider.FromValue(other));
            int score = 0;

            foreach (var m in Properties.FromValue(criteria)) {
                var comparand = pp.GetProperty(m.Key);
                if (comparand is Type && m.Value is Type) {
                    if (((Type) m.Value).GetTypeInfo().IsAssignableFrom((Type) comparand)) {
                        score++;
                    }
                }

                if (object.Equals(comparand, m.Value)) {
                    score++;
                }
            }
            return score;
        }

        public static IProviderMetadata Create(object metadata) {
            var pm = metadata as IProviderMetadata;
            if (pm == null)
                return new ProviderMetadataWrapper(metadata ?? new object());
            else
                return pm;
        }

        object IProviderMetadata.Value {
            get { return this.value; } }

        sealed class NullProviderMetadata : IProviderMetadata {

            object IProviderMetadata.Value {
                get { return null; } }

            ProviderValueSource IProviderMetadata.Source {
                get { return null; }
                set {} }

            public int MatchCriteria(object criteria) {
                return 0;
            }
        }
    }
}
