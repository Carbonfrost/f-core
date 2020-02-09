//
// Copyright 2005, 2006, 2010, 2016, 2020 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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

        public static readonly new IProperties Null = new NullProperties();
        public static readonly IProperties Empty = ReadOnly(Null);

        public static Properties FromValues<TValue>(IEnumerable<KeyValuePair<string, TValue>> keyValuePairs) {
            var result = new Properties();
            if (keyValuePairs != null) {
                foreach (var kvp in keyValuePairs) {
                    result.SetProperty(kvp.Key, kvp.Value);
                }
            }
            return result;
        }

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
                throw new ArgumentNullException("fileName");
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

            return MakeDictionaryProperties(context)
                ?? ReflectionPropertiesUsingIndexer.TryCreate(context)
                ?? new ReflectionProperties(context);
        }

        public static new IProperties FromArray(params object[] values) {
            if (values == null || values.Length == 0) {
                return Properties.Null;
            }

            return new Properties(values.Select((t, i) => new KeyValuePair<string, object>(i.ToString(), t)));
        }

        public static Properties Parse(string text) {
            if (string.IsNullOrWhiteSpace(text)) {
                return new Properties();
            }

            Properties result = new Properties();
            foreach (var kvp in ParseKeyValuePairs(text))
                result.InnerMap.Add(kvp.Key, kvp.Value);

            return result;
        }

        public static IProperties ReadOnly<TValue>(IReadOnlyDictionary<string, TValue> value) {
            if (value == null || ReferenceEquals(value, Properties.Null)) {
                return Properties.Null;
            }

            return new PropertiesDictionaryAdapter<TValue>(value);
        }

        public static IProperties ReadOnly(IPropertyStore value) {
            if (value == null || ReferenceEquals(value, Properties.Null))
                return Properties.Null;

            return new ReadOnlyProperties(value);
        }

        internal static string ToKeyValuePairs(IEnumerable<KeyValuePair<string, object>> properties) {
            if (properties == null)
                throw new ArgumentNullException("properties");

            StringBuilder sb = new StringBuilder();
            bool needComma = false;

            foreach (KeyValuePair<string, object> kvp in properties) {
                if (needComma) {
                    sb.Append(";");
                }

                sb.Append(kvp.Key);
                sb.Append("=");
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
            text = text.Replace("\\", "\\\\");
            if (quotes && apos || hasWhitespace) {
                // Escape apostrophies and use it as the string literal notation
                return "'" + text.Replace("'", @"\'") + "'";
            } else if (quotes) {
                return "'" + text + "'";
            } else if (apos) {
                return "\"" + text + "\"";
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
                    case "=":
                        atKey = false;
                        if (key == null)
                            throw RuntimeFailure.PropertiesParseKeyNameExpected();
                        break;

                    case ";":
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

            if (key != null) {
                yield return new KeyValuePair<string, string>(key, value ?? string.Empty);
            }
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

                    case '=':
                    case ';':
                        if (quoteChar != '\0') goto default;

                        if (sb.Length > 0) {
                            yield return sb.ToString();
                            sb.Length = 0;
                        }
                        yield return (c.Current == ';') ? ";" : "=";
                        break;

                    default:
                        sb.Append(c.Current);
                        break;
                }
            }
            if (sb.Length > 0) {
                yield return sb.ToString();
            }
        }
    }
}
