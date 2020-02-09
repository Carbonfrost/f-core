//
// Copyright 2020 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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

namespace Carbonfrost.Commons.Core.Runtime {

    public sealed partial class TypeReference : IEquatable<TypeReference>, IFormattable {

        private readonly CacheTypeReferenceResolver _resolveThunk;
        private readonly string _originalString;

        public string OriginalString {
            get {
                return _originalString;
            }
        }

        private CacheTypeReferenceResolver Resolver {
            get {
                return _resolveThunk;
            }
        }

        private TypeReference(string originalString,
                              TypeReferenceResolver resolveThunk) {
            if (resolveThunk == null) {
                throw new ArgumentNullException("resolveThunk");
            }

            _resolveThunk = new CacheTypeReferenceResolver(resolveThunk);
            _originalString = originalString;
        }

        public LateBound<T> AsLateBound<T>() {
            return new LateBound<T>(this, ServiceProvider.Root);
        }

        public Type Resolve() {
            var result = Resolver.Resolve();
            if (result == null) {
                // RuntimeWarning.LateBoundTypeFailure(OriginalString, Resolver.ResolveError);
                // throw Resolver.ResolveError ?? RuntimeFailure.TypeMissing(this);
                throw RuntimeFailure.TypeMissing(this);
            }
            return result;
        }

        public Type TryResolve() {
            return Resolver.Resolve();
        }

        public QualifiedName ToQualifiedName() {
            return Resolve().GetQualifiedName();
        }

        public override string ToString() {
            return Resolver.CanonicalString;
        }

        public override int GetHashCode() {
            int hashCode = 0;
            unchecked {
                hashCode += 9 * (_originalString ?? string.Empty).GetHashCode();
                var type = TryResolve();
                if (type != null) {
                    hashCode += 93 * type.GetHashCode();
                }
            }
            return hashCode;
        }

        public override bool Equals(object obj) {
            return Equals(obj as TypeReference);
        }

        public bool Equals(TypeReference other) {
            if (other == null) {
                return false;
            }
            if (this == other) {
                return true;
            }
            return _originalString == other._originalString
                && object.Equals(TryResolve(), other.TryResolve());
        }

        public string ToString(string format) {
            return ToString(format, null);
        }

        public string ToString(string format, IFormatProvider formatProvider) {
            if (string.IsNullOrEmpty(format)) {
                return ToString();
            }
            switch (format[0]) {
                case 'N':
                    return ToString();
                case 'R':
                    return Resolve().AssemblyQualifiedName;
                case 'F':
                    return Resolve().FullName;
                case 'O':
                    return OriginalString ?? string.Empty;
                case 'Q':
                    return ToQualifiedName().ToString();
                case 'q':
                    return GetCanonicalTypeName();
                default:
                    throw new FormatException();
            }
        }

        private string GetCanonicalTypeName() {
            var type = Resolve();
            var assembly = AssemblyInfo.GetAssemblyInfo(type.GetTypeInfo().Assembly);

            string prefix = assembly.GetXmlNamespacePrefix(
                ToQualifiedName().Namespace
            );
            if (string.IsNullOrEmpty(prefix)) {
                return type.Name;
            }

            return string.Concat(prefix, ':', type.Name);
        }
    }

}
