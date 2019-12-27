//
// Copyright 2005, 2006, 2010, 2019 Carbonfrost Systems, Inc.
// (http://carbonfrost.com)
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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using Carbonfrost.Commons.Core.Runtime;

namespace Carbonfrost.Commons.Core {

    static class Utility {

        const string BASE_64_PREFIX = "base64:";
        static readonly Assembly THIS_ASSEMBLY = typeof(Utility).GetTypeInfo().Assembly;
        static readonly byte[][] SYSTEM_PKT = new [] {
            typeof(object).GetTypeInfo().Assembly.GetName().GetPublicKeyToken(),
            typeof(Uri).GetTypeInfo().Assembly.GetName().GetPublicKeyToken(),
        };
        static readonly Dictionary<Assembly, bool> SCANNABLE = new Dictionary<Assembly, bool>();

        public static readonly IXmlNamespaceResolver NullNamespaceResolver = new NullNamespaceResolverImpl();
        public static readonly IEqualityComparer<Type> EquivalentComparer = new ExtendedTypeComparer();

        private class ExtendedTypeComparer : IEqualityComparer<Type> {

            // If a forwarded type is used, we still want it to be treated
            // the same in the dictionary in .Net 4

            public bool Equals(Type x, Type y) {
                #if NET_4_0
                return x.IsEquivalentTo(y);
                #else
                return x.Equals(y);
                #endif
            }

            public int GetHashCode(Type obj) {
                return obj.FullName.GetHashCode();
            }

        }

        class NullNamespaceResolverImpl : IXmlNamespaceResolver {

            public IDictionary<string, string> GetNamespacesInScope(XmlNamespaceScope scope) {
                return new Dictionary<string, string>();
            }

            public string LookupNamespace(string prefix) {
                return null;
            }

            public string LookupPrefix(string namespaceName) {
                return null;
            }
        }

        public static string Camel(string name) {
            return char.ToLowerInvariant(name[0]) + name.Substring(1);
        }

        public static IEnumerable<string> ReadAllLines(TextReader r) {
            string line;
            while ((line = r.ReadLine()) != null) {
                yield return line;
            }
        }

        public static byte[] ConvertHexToBytes(string hex, bool allowBase64 = true) {
            if (allowBase64 && hex.StartsWith(BASE_64_PREFIX, StringComparison.OrdinalIgnoreCase))
                return Convert.FromBase64String(hex.Substring(BASE_64_PREFIX.Length));

            if ((hex.Length % 2) == 1)
                throw RuntimeFailure.NotValidHexString();

            byte[] result = new byte[hex.Length / 2];

            for (int i = 0; i < result.Length; i++) {
                result[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return result;
        }

        public static string Display(object value) {
            if (object.ReferenceEquals(value, null))
                return "<null>";
            else
                return string.Concat("`", value, "'");
        }

        public static StreamReader MakeStreamReader(Stream stream, Encoding encoding) {
            if (encoding == null)
                return new StreamReader(stream);
            else
                return new StreamReader(stream, encoding);
        }

        public static string Unescape(string text) {
            StringBuilder result = new StringBuilder();

            using (StringReader reader = new StringReader(text)) {
                int i;

                while ((i = reader.Read()) != -1) {
                    char c = (char) i;

                    if (c == '\\') {
                        i = reader.Read();
                        if (i == -1) {
                            result.Append('\\');
                            break;
                        }

                        char ch = (char) i;
                        switch (ch) {
                            case 'b':
                                ch = '\b';
                                break;

                            case 't':
                                ch = '\t';
                                break;

                            case 'n':
                                ch = '\n';
                                break;

                            case 'f':
                                ch = '\f';
                                break;

                            case 'r':
                                ch = '\r';
                                break;

                            case 'u':
                                string unicodeValue = new String(_ReadOrThrow(reader, 4));
                                result.Append(UnescapeUnicode(unicodeValue));
                                break;

                            case 'x':
                                string hexValue = new String(_ReadOrThrow(reader, 2));
                                result.Append(UnescapeHex(hexValue));
                                break;

                            default:
                                break;
                        }

                        result.Append(ch);
                    } else {
                        result.Append(c);
                    }

                } // end while

            } // end using

            return result.ToString();
        }

        public static char UnescapeUnicode(string chars) {
            if (chars == null)
                throw RuntimeFailure.IncompleteEscapeSequence();
            int unicodeValue = Convert.ToInt32(chars, 16);
            return Convert.ToChar(unicodeValue);
        }

        public static char UnescapeHex(string chars) {
            if (chars == null)
                throw RuntimeFailure.IncompleteEscapeSequence();

            int hexValue = Convert.ToInt32(chars, 16);
            return Convert.ToChar(hexValue);
        }

        private static char[] _ReadOrThrow(TextReader reader, int count) {
            char[] result = new char[count];
            for (int i = 0; i < count; i++) {
                int character = reader.Read();
                if (character == -1)
                    throw RuntimeFailure.IncompleteEscapeSequence();

                result[i] = (char) character;
            }

            return result;
        }

        public static bool IsScannableAssembly(Assembly a) {
            return SCANNABLE.GetValueOrCache(
                a,
                () => {
#if NET
                    if (a.ReflectionOnly) {
                        return false;
                    }
#endif
                    if (a == THIS_ASSEMBLY) {
                        return true;
                    }

                    var pkt = a.GetName().GetPublicKeyToken();

                    foreach (var other in SYSTEM_PKT) {
                        if (pkt.SequenceEqual(other)) {
                            return false;
                        }
                    }

                    return true;
                });
        }

        public static T LateBoundProperty<T>(object source,
                                             string property) {

            PropertyInfo pi = source.GetType().GetProperty(property);
            if (pi == null)
                return default(T);
            try {
                object value = pi.GetValue(source, null);
                if (pi.PropertyType == typeof(T))
                    return (T) value;
                else
                    return default(T);

            } catch (Exception ex) {
                if (Failure.IsCriticalException(ex))
                    throw;

                return default(T);
            }
        }

        public static string GetImpliedName(Type type, string name) {
            string tname = type.Name;
            if (tname.EndsWith(name, StringComparison.Ordinal))
                return tname.Substring(0, tname.Length - name.Length);
            else
                return tname;
        }

        public static T OptimalComposite<T>(IEnumerable<T> items, Func<IReadOnlyCollection<T>, T> compositeFactory, T nullInstance)
            where T : class
        {
            if (items == null) {
                return nullInstance;
            }

            items = items.Where(t => t != null && !object.ReferenceEquals(t, nullInstance));
            if (!items.Any()) {
                return nullInstance;
            }
            if (items.Skip(1).Any()) { // 2 or more
                return compositeFactory(new Buffer<T>(items));
            }

            return items.First();
        }
    }

}
