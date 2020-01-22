//
// Copyright 2005, 2006, 2010, 2016 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Carbonfrost.Commons.Core.Runtime {

    partial class Properties {

        const string KVP_DELIM = "=";
        const string ITEM_DELIM = ";";
        const char CHAR_KVP_DELIM = '=';
        const char CHAR_ITEM_DELIM = ';';

        public static readonly new IProperties Null = new NullProperties();
        public static readonly IProperties Empty = ReadOnly(Null);

        // Implicit construction.
        // N.B. Conforms with Streaming source patterns
        public static Properties FromStream(Stream stream, Encoding encoding = null) {
            if (stream == null) {
                throw new ArgumentNullException("stream");
            }
            Properties p = new Properties();
            p.Load(stream, encoding);
            return p;
        }

        public static Properties FromStreamContext(StreamContext source, Encoding encoding = null) {
            if (source == null) {
                throw new ArgumentNullException("source");
            }

            Properties p = new Properties();
            p.Load(source.OpenRead(), encoding);
            return p;
        }

        public static Properties FromFile(string fileName) {
            if (fileName == null) {
                throw new ArgumentNullException("fileName"); // $NON-NLS-1
            }
            if (string.IsNullOrWhiteSpace(fileName)) {
                throw Failure.AllWhitespace("fileName");
            }

            Properties p = new Properties();
            p.Load(fileName);
            return p;
        }

        public static new IProperties FromValue(object context) {
            if (context == null) {
                return Properties.Null;
            }

            IProperties pp = context as IProperties;
            if (pp != null) {
                return pp;
            }

            IPropertiesContainer container = context as IPropertiesContainer;
            if (container != null) {
                return container.Properties;
            }

            NameValueCollection nvc = context as NameValueCollection;
            if (nvc != null) {
                return new NameValueCollectionAdapter(nvc);
            }
            pp = MakeDictionaryProperties(context);
            if (pp != null) {
                return pp;
            }

            var indexer = FindIndexerProperty(context.GetType());
            if (indexer != null) {
                return new ReflectionPropertiesUsingIndexer(context, indexer);
            }
            return new ReflectionProperties(context);
        }

        public static new IProperties FromArray(params object[] values) {
            if (values == null || values.Length == 0) {
                return Properties.Null;
            }

            return new Properties(values.Select((t, i) => new KeyValuePair<string, object>(i.ToString(), t)));
        }

        public static Properties Parse(string text) {
            if (string.IsNullOrEmpty(text))
                return new Properties();

            Properties result = new Properties();
            foreach (var kvp in ParseKeyValuePairs(text))
                result.InnerMap.Add(kvp.Key, kvp.Value);

            return result;
        }

        public static IProperties ReadOnly(IReadOnlyDictionary<string, object> value) {
            if (value == null || ReferenceEquals(value, Properties.Null)) {
                return Properties.Null;
            }

            return new PropertiesDictionaryAdapter(value);
        }

        public static IProperties ReadOnly(IPropertyStore value) {
            if (value == null || ReferenceEquals(value, Properties.Null))
                return Properties.Null;

            return new ReadOnlyProperties(value);
        }

        internal static string ToKeyValuePairs(IEnumerable<KeyValuePair<string, object>> properties) {
            if (properties == null)
                throw new ArgumentNullException("properties"); // $NON-NLS-1

            StringBuilder sb = new StringBuilder();
            bool needComma = false;

            foreach (KeyValuePair<string, object> kvp in properties) {
                if (needComma)
                    sb.Append(ITEM_DELIM);

                sb.Append(kvp.Key);
                sb.Append(KVP_DELIM);
                sb.Append(_Escape(kvp.Value));
                needComma = true;
            }

            return sb.ToString();
        }

        private static IProperties MakeDictionaryProperties(object items) {
            Type dictionaryType = null;
            foreach (var tz in items.GetType().GetTypeInfo().GetInterfaces()) {
                var t = tz.GetTypeInfo();
                if (t.IsGenericType
                    && !t.IsGenericTypeDefinition) {
                    var def = t.GetGenericTypeDefinition();
                    var args = t.GetGenericArguments();
                    if (def.Equals(typeof(IDictionary<,>)) && args[0] == typeof(string)) {
                        dictionaryType = tz;
                        break;
                    }
                }
            }

            if (dictionaryType == null)
                return null;

            Type outputType = typeof(DictionaryProperties<>).MakeGenericType(dictionaryType.GetGenericArguments()[1]);
            return (IProperties) Activator.CreateInstance(outputType, items);
        }

        private static string _Escape(object value) {
            if (value == null)
                return string.Empty;
            string text = value.ToString();
            if (string.IsNullOrEmpty(text))
                return string.Empty;
            // Use either quotes or apostrophies or neither
            bool apos = (text.IndexOf('\'') > 0);
            bool quotes = (text.IndexOf('"') > 0);
            bool hasWhitespace = false;
            foreach (char c in text) {
                if (Char.IsWhiteSpace(c)) {
                    hasWhitespace = true;
                    break;
                }
            }
            text = text.Replace("\\", "\\\\"); // $NON-NLS-1, $NON-NLS-2
            if (quotes && apos || hasWhitespace) {
                // Escape apostrophies and use it as the string literal notation
                return "'" + text.Replace("'", @"\'") + "'"; // $NON-NLS-1, $NON-NLS-2, $NON-NLS-3, $NON-NLS-4
            } else if (quotes) {
                return "'" + text + "'"; // $NON-NLS-1, $NON-NLS-2
            } else if (apos) {
                return "\"" + text + "\""; // $NON-NLS-1, $NON-NLS-2
            } else {
                return text;
            }
        }

        internal static IEnumerable<KeyValuePair<string, string>> ParseKeyValuePairs(string text) {
            var tokens = ParseTokenizer(text);
            bool atKey = true;
            string key = null;
            string value = null;

            foreach (var s in tokens) {
                switch (s) {
                    case KVP_DELIM:
                        atKey = false;
                        if (key == null)
                            throw RuntimeFailure.PropertiesParseKeyNameExpected();
                        break;

                    case ITEM_DELIM:
                        atKey = true;
                        if (key != null) {
                            yield return new KeyValuePair<string, string>(key, value ?? string.Empty);
                            key = value = null;
                        }
                        break;
                    default:
                        if (atKey)
                            key = s;
                        else
                            value = s;
                        break;
                }
            }

            if (key != null)
                yield return new KeyValuePair<string, string>(key, value ?? string.Empty);
        }

        static IEnumerable<string> ParseTokenizer(string text) {
            char quoteChar = '\0';
            StringBuilder sb = new StringBuilder();
            var c = ((IEnumerable<char>) text).GetEnumerator();

            while (c.MoveNext()) {
                switch (c.Current) {
                    case '\\':
                        sb.Append(StringUnescaper.UnescapeChar(c));
                        break;

                    case '"':
                    case '\'':
                        if (quoteChar == '\0') {
                            quoteChar = c.Current;

                        } else if (quoteChar == c.Current) {
                            quoteChar = '\0';

                        } else {
                            goto default;
                        }
                        break;

                    case CHAR_KVP_DELIM:
                    case CHAR_ITEM_DELIM:
                        if (quoteChar != '\0') goto default;

                        if (sb.Length > 0) {
                            yield return sb.ToString();
                            sb.Length = 0;
                        }
                        if (c.Current == CHAR_ITEM_DELIM)
                            yield return ITEM_DELIM;
                        else
                            yield return KVP_DELIM;
                        break;

                    default:
                        sb.Append(c.Current);
                        break;
                }
            }
            if (sb.Length > 0)
                yield return sb.ToString();
        }

        static PropertyInfo FindIndexerProperty(Type type) {
            foreach (PropertyInfo pi in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)) {
                var parameters = pi.GetIndexParameters();
                if (parameters.Length == 1 && parameters[0].ParameterType == typeof(string)) {
                    return pi;
                }
            }
            return null;
        }
    }
}
