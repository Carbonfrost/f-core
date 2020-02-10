//
// Copyright 2005, 2006, 2010, 2020 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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

    public sealed partial class TypeReference : IEquatable<TypeReference>, IFormattable {

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

        static Exception _TryParse(string text,
                                   IServiceProvider serviceProvider,
                                   out TypeReference result) {
            result = null;
            if (text == null)
                return new ArgumentNullException("type");
            text = text.Trim();

            if (text.Length == 0)
                return Failure.AllWhitespace("text");

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

                var resolveThunk = new DefaultResolver(assemblyName, typeName);
                result = new TypeReference(text, resolveThunk);
                return null;

            } else {
                var resolveThunk = new SlowResolver(typeName);
                result = new TypeReference(text, resolveThunk);
                return null;
            }
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
            if (type == null) {
                throw new ArgumentNullException("type");
            }

            return new TypeReference(null, new TrivialResolver(type));
        }

        public static TypeReference FromQualifiedName(QualifiedName qualifiedName) {
            if (qualifiedName == null) {
                throw new ArgumentNullException(nameof(qualifiedName));
            }

            return new TypeReference(null, new QualifiedNameResolver(qualifiedName));
        }

    }

}
