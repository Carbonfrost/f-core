//
// Copyright 2014, 2020 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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

namespace Carbonfrost.Commons.Core {

    abstract class QualifiedNameComparer : IEqualityComparer<QualifiedName>, IComparer<QualifiedName> {

        public static readonly QualifiedNameComparer IgnoreCaseLocalName
            = new IgnoreCaseLocalNameImpl();

        public abstract bool Equals(QualifiedName x, QualifiedName y);
        public abstract int GetHashCode(QualifiedName obj);
        public abstract int Compare(QualifiedName x, QualifiedName y);

        class IgnoreCaseLocalNameImpl : QualifiedNameComparer {

            public override bool Equals(QualifiedName x, QualifiedName y) {
                if (x == null || y == null) {
                    return x == y;
                }

                return x.EqualsIgnoreCase(y);
            }

            public override int GetHashCode(QualifiedName obj) {
                if (obj == null) {
                    return -37;
                }

                unchecked {
                    return obj.LocalName.ToLowerInvariant().GetHashCode() << 0x18
                        ^ obj.Namespace.GetHashCode();
                }
            }

            public override int Compare(QualifiedName x, QualifiedName y) {
                return StringComparer.OrdinalIgnoreCase.Compare(x.LocalName, y.LocalName);
            }
        }
    }
}
