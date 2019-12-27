//
// Copyright 2005, 2006, 2010 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Carbonfrost.Commons.Core.Runtime {

    public sealed partial class TypeReference : IEquatable<TypeReference> {

        private readonly string originalString;
        private readonly TypeReferenceResolver resolveThunk;

        public static readonly TypeReference Null = new TypeReference(null, new TrivialResolver(null));

        static readonly IDictionary<string, Type> builtInNames = new Dictionary<string, Type>() {
            { "string", typeof(string) },
            { "sbyte", typeof(sbyte) },
            { "byte", typeof(byte) },
            { "short", typeof(short) },
            { "int", typeof(int) },
            { "long", typeof(long) },
            { "ushort", typeof(ushort) },
            { "uint", typeof(uint) },
            { "ulong", typeof(ulong) },
            { "decimal", typeof(decimal) },
            { "double", typeof(double) },
            { "float", typeof(float) },
            { "timespan", typeof(TimeSpan) },
            { "bool", typeof(bool) },
            { "char", typeof(char) },
        };

        private Exception _typeResolveError;
        private bool triedToResolveType;
        private Type innerType;

        public string OriginalString {
            get { return this.originalString; }
        }

        internal Type Type {
            get {
                EnsureTypeResolved(false);
                return this.innerType;
            }
        }

        private TypeReference(string originalString,
                              TypeReferenceResolver resolveThunk) {

            this.resolveThunk = resolveThunk;
            this.originalString = originalString;
        }

        static Exception _TryParse(string text,
                                   IServiceProvider serviceProvider,
                                   out TypeReference result) {
            result = null;
            if (text == null)
                return new ArgumentNullException("type");  // $NON-NLS-1
            text = text.Trim();

            if (text.Length == 0)
                return Failure.AllWhitespace("text"); // $NON-NLS-1

            serviceProvider = serviceProvider ?? ServiceProvider.Root;

            Type builtIn;
            if (builtInNames.TryGetValue(text, out builtIn)) {
                result = FromType(builtIn);
                return null;
            }

            if (text.Contains(":") || text.Contains("{")) {
                try {

                    QualifiedName qn = QualifiedName.Parse(text, serviceProvider);
                    result = new TypeReference(text, new QualifiedNameResolver(qn));
                    return null;

                } catch (FormatException f) {
                    return Failure.NotParsable("text", typeof(TypeReference), f);
                }
            }

            string[] s = text.Split(new [] { ',' } , 2);
            string typeName = s[0];

            if (s.Length == 2) {

                AssemblyName assemblyName = null;
                try {
                    assemblyName = new AssemblyName(s[1]);

                } catch (Exception ex) {
                    return Failure.NotParsable("text", typeof(TypeReference), ex);
                }

                var resolveThunk = CreateDefaultResolveThunk(assemblyName, typeName);
                result = new TypeReference(text, resolveThunk);
                return null;

            } else {
                var resolveThunk = CreateSlowResolveThunk(typeName);
                result = new TypeReference(text, resolveThunk);
                return null;
            }
        }

        static TypeReferenceResolver CreateSlowResolveThunk(string typeName) {
            return new SlowResolver(typeName);
        }

        static TypeReferenceResolver CreateDefaultResolveThunk(AssemblyName assemblyName, string typeName) {
            return new DefaultResolver(assemblyName, typeName);
        }

        static Type FindType(Assembly assembly, string typeName) {
            if (assembly == null) {
                return null;
            }
            Type c = assembly.GetType(typeName);
            if (c == null && !typeName.Contains(".")) {
                var result = assembly.GetTypesHelper().FirstOrDefault(u => u.Name == typeName);
                return result == null ? null : result.AsType();

            } else
                return c;
        }

        // N.B.: If Parse provides parameters, they must be IServiceProvider or CultureInfo
        public static TypeReference Parse(string text, IServiceProvider serviceProvider = null) {
            TypeReference result;
            Exception ex = _TryParse(text, serviceProvider, out result);
            if (ex == null)
                return result;
            else
                throw ex;
        }

        public static bool TryParse(string text, IServiceProvider serviceProvider, out TypeReference result) {
            return _TryParse(text, serviceProvider, out result) != null;
        }

        public static TypeReference FromType(Type type) {
            if (type == null)
                throw new ArgumentNullException("type"); // $NON-NLS-1

            return new TypeReference(type.AssemblyQualifiedName, new TrivialResolver(type));
        }

        public LateBound<T> AsLateBound<T>() {
            return new LateBound<T>(this, ServiceProvider.Root);
        }

        public Type Resolve() {
            EnsureTypeResolved(true);
            return this.innerType;
        }

        public Type TryResolve() {
            EnsureTypeResolved(false);
            return this.innerType;
        }

        internal string ConvertToString() {
            return this.resolveThunk.CanonicalString;
        }

        // 'Object' overrides.
        public override string ToString() {
            return ConvertToString();
        }

        public override int GetHashCode() {
            int hashCode = 0;
            unchecked {
                this.EnsureTypeResolved(false);
                if (originalString != null) hashCode += 1000000009 * originalString.GetHashCode();
                if (innerType != null) hashCode += 1000000093 * innerType.GetHashCode();
            }
            return hashCode;
        }

        public override bool Equals(object obj) {
            return Equals(obj as TypeReference);
        }

        // 'IEquatable' implementation.
        public bool Equals(TypeReference other) {
            if (other == null) { return false; }
            if (this == other) return true;
            EnsureTypeResolved();
            other.EnsureTypeResolved();
            return this.originalString == other.originalString
                && this.innerType.Equals(other.innerType);
        }

        // Ensures that the type has had a chance to resolve
        private void EnsureTypeResolved(bool throwOnError) {
            if (this.innerType == null) {
                // Don't execute this more than once
                if (!triedToResolveType) {
                    triedToResolveType = true;
                    Traceables.TypeReferenceResolvingType(originalString);

                    try {
                        this.innerType = this.resolveThunk.Resolve();

                        if (this.innerType == null) {
                            _typeResolveError = RuntimeFailure.TypeMissing(this);
                        }
                    } catch (Exception ex) {
                        RuntimeWarning.LateBoundTypeFailure(this.originalString, ex);
                        Traceables.TypeReferenceResolveError(ex);

                        this.innerType = null;
                        _typeResolveError = ex;
                    }
                }
            }

            if (throwOnError && _typeResolveError != null) {
                Console.WriteLine(_typeResolveError);
                throw _typeResolveError;
            }
        }

        // Ensures that the type has had a chance to resolve
        private void EnsureTypeResolved() {
            EnsureTypeResolved(false);
        }

    }

}
