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
using System.Linq;
using System.Text;

namespace Carbonfrost.Commons.Core.Runtime {

    public sealed class ContentType : IEquatable<ContentType> {

        internal const string Binary = "application/octet-stream";

        private readonly Dictionary<string, string> parameters;

        readonly static HashSet<string> VALID_TYPES = new HashSet<string> {
            "application", "message", "text", "audio", "video", "image"
        };

        public string Type { get; private set; }
        public string Subtype { get; private set; }

        public string MediaType {
            get { return string.Concat(Type, '/', Subtype); }
        }

        public ContentType Parent {
            get {
                int index = Subtype.LastIndexOf('+');
                if (index < 0)
                    return null;
                else
                    return new ContentType(Type, Subtype.Substring(1 + index));
            }
        }

        public IReadOnlyDictionary<string, string> Parameters {
            get { return parameters; }
        }

        public ContentType(string type, string subtype)
            : this(type, subtype, null) {
        }

        public ContentType(string type,
                           string subtype,
                           IEnumerable<KeyValuePair<string, string>> parameters) {

            Exception ex = CheckArguments(type, subtype, parameters);
            if (ex != null)
                throw ex;

            this.Subtype = subtype;
            this.Type = type;

            IDictionary<string, string> dict;

            if (parameters == null)
                dict = new Dictionary<string, string>();
            else {
                dict = new Dictionary<string, string>();
                foreach (var kvp in parameters)
                    dict.Add(kvp);
            }
            this.parameters = new Dictionary<string, string>(dict);
        }

        static Exception CheckArguments(string type, string subtype,
                                        IEnumerable<KeyValuePair<string, string>> parameters) {
            if (string.IsNullOrEmpty(type)) {
                throw Failure.NullOrEmptyString(nameof(type));
            }
            if (!VALID_TYPES.Contains(type))
                return RuntimeFailure.ContentTypeNotStandard("type", type);

            if (string.IsNullOrEmpty(subtype)) {
                throw Failure.NullOrEmptyString(nameof(subtype));
            }

            return null;
        }

        public static ContentType Parse(string text) {
            ContentType result;
            Exception ex = _TryParse(text, out result);
            if (ex == null)
                return result;
            else
                throw ex;
        }

        public static bool TryParse(string text, out ContentType result) {
            return _TryParse(text, out result) == null;
        }

        static Exception _TryParse(string text, out ContentType result) {
            result = null;
            if (text == null)
                return new ArgumentNullException("text");

            text = text.Trim();
            if (string.IsNullOrEmpty(text)) {
                return Failure.AllWhitespace("text");
            }

            string[] split = text.Split(';');
            string[] mediaType = split[0].Split('/');
            result = new ContentType(mediaType[0],
                                     mediaType[1],
                                     ParseParameters(split.Skip(1)));

            return null;
        }

        static IEnumerable<KeyValuePair<string, string>> ParseParameters(IEnumerable<string> param) {
            foreach (var s in param) {
                if (string.IsNullOrEmpty(s)) {
                    continue;
                }

                string[] split = s.Split(new [] { '=' }, 2);
                string key = split[0].Trim();
                string value = (split.Length > 1) ? split[1].Trim() : "";

                // Trim away quotes
                int valLength = value.Length;
                if (valLength >= 2 && value[0] == '"' && value[valLength - 1] == '"') {
                    value = value.Substring(1, valLength - 2);
                }
                yield return new KeyValuePair<string, string>(key, value);
            }
        }

        public bool Equals(ContentType other) {
            return StaticEquals(this, other);
        }

        public override bool Equals(object obj) {
            return StaticEquals(this, obj as ContentType);
        }

        public override int GetHashCode() {
            return this.ToString().GetHashCode();
        }

        public static bool operator ==(ContentType lhs, ContentType rhs) {
            return StaticEquals(lhs, rhs);
        }

        public static bool operator !=(ContentType lhs, ContentType rhs) {
            return !StaticEquals(lhs, rhs);
        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            sb.Append(this.Type);
            sb.Append('/');
            sb.Append(this.Subtype);

            if (parameters.Count > 0) {
                foreach (var kvp in parameters) {
                    sb.Append("; ");
                    sb.Append(kvp.Key);
                    sb.Append('=');
                    sb.Append(EscapeValue(kvp.Value));
                }
            }

            return sb.ToString();
        }

        static string EscapeValue(string s) {
            // TODO Proper escaping
            return s;
        }

        static bool StaticEquals(ContentType a, ContentType b) {
            if (ReferenceEquals(a, b))
                return true;

            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                return false;

            return a.Type == b.Type && a.Subtype == b.Subtype
                && EqualDictionary(a.parameters, b.parameters);
        }

        static bool EqualDictionary(IDictionary<string, string> a, IDictionary<string, string> b) {
            if (a.Count != b.Count)
                return false;

            foreach (var kvp in a) {
                if (!b.Contains(kvp))
                    return false;
            }

            return true;
        }
    }
}
