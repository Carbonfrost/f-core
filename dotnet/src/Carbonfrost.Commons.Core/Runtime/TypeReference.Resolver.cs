//
// Copyright 2012, 2016, 2020 Carbonfrost Systems, Inc. (https://carbonfrost.com)
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
using System.IO;
using System.Linq;
using System.Reflection;

namespace Carbonfrost.Commons.Core.Runtime {

    partial class TypeReference {

        abstract class TypeReferenceResolver {
            public abstract string CanonicalString { get; }
            public abstract Type Resolve();
        }

        class CacheTypeReferenceResolver : TypeReferenceResolver {
            private readonly TypeReferenceResolver _inner;
            private Exception _resolveError;
            private Type _result;

            public CacheTypeReferenceResolver(TypeReferenceResolver inner) {
                _inner = inner;
            }

            public override string CanonicalString {
                get {
                    return _inner.CanonicalString;
                }
            }

            public Exception ResolveError {
                get {
                    return _resolveError;
                }
            }

            public override Type Resolve() {
                var result = EnsureResult();
                if (result == typeof(FailedToResolve)) {
                    return null;
                }
                return result;
            }

            private Type EnsureResult() {
                if (_result == null) {
                    try {
                        _result = _inner.Resolve();
                        if (_result == null) {
                            _result = typeof(FailedToResolve);
                        }

                    } catch (Exception ex) {
                        _resolveError = ex;
                        _result = typeof(FailedToResolve);
                    }
                }
                return _result;
            }

            class FailedToResolve {}
        }

        class QualifiedNameResolver : TypeReferenceResolver {

            private QualifiedName _qn;

            public QualifiedNameResolver(QualifiedName qn) {
                _qn = qn;
            }

            public override string CanonicalString {
                get {
                    return _qn.ToString();
                }
            }

            public override Type Resolve() {
                string cleanName = _qn.LocalName.Replace('.', '+').Replace('-', '`');

                foreach (var a in App.Assemblies) {
                    AssemblyInfo ai = AssemblyInfo.GetAssemblyInfo(a);
                    foreach (string clrns in ai.GetClrNamespaces(_qn.Namespace)) {
                        Type result = a.GetType(CombinedTypeName(clrns, cleanName));
                        if (result != null) {
                            return result;
                        }
                    }
                }

                return null;
            }

            static string CombinedTypeName(string clrns, string name) {
                if (clrns.Length == 0) {
                    return name;
                }
                return string.Concat(clrns, ".", name);
            }

        }

        class TrivialResolver : TypeReferenceResolver {

            private readonly Type type;

            public TrivialResolver(Type type) {
                this.type = type;
            }

            public override string CanonicalString {
                get {
                    return (type == null) ? string.Empty : type.AssemblyQualifiedName;
                }
            }

            public override Type Resolve() {
                return type;
            }
        }

        class SlowResolver : TypeReferenceResolver {

            private readonly string typeName;

            public SlowResolver(string typeName) {
                this.typeName = typeName;
            }

            public override string CanonicalString {
                get {
                    return typeName;
                }
            }

            public override Type Resolve() {
                foreach (var an in App.DescribeAssemblies()) {
                    Type type = an.GetType(typeName);
                    if (type != null) {
                        return type;
                    }
                }

                if (!typeName.Contains(".")) {
                    return typeof(object).GetTypeInfo().Assembly.GetType("System." + typeName);
                }

                return null;
            }
        }

        class DefaultResolver : TypeReferenceResolver {

            private readonly AssemblyName assemblyName;
            private readonly string typeName;

            public DefaultResolver(AssemblyName assemblyName, string typeName) {
                this.assemblyName = assemblyName;
                this.typeName = typeName;
            }

            public override string CanonicalString {
                get {
                    return typeName + ", " + assemblyName;
                }
            }

            public override Type Resolve() {
                Assembly assembly = null;
                try {
                    assembly = Assembly.Load(assemblyName);
                    return FindType(assembly, typeName);

                } catch (FileNotFoundException) {
                    // Ignore assembly loading problems
                }

                // Slow search for an assembly
                var results = App.Assemblies.Where(a => a.GetName().Name == assemblyName.Name);
                var pkt = assemblyName.GetPublicKeyToken();
                if (pkt != null) {
                    results = results.Where(a => a.GetName().GetPublicKeyToken().SequenceEqual(pkt));
                }
                if (assemblyName.CultureName != null) {
                    results = results.Where(a => a.GetName().CultureName == assemblyName.CultureName);
                }
                return FindType(results.FirstOrDefault(), typeName);
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
        }
    }
}
