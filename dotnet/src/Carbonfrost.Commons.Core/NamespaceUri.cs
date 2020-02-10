//
// Copyright 2005, 2006, 2010, 2012, 2019-2020 Carbonfrost Systems, Inc.
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
using System.Text.RegularExpressions;

namespace Carbonfrost.Commons.Core {

    public sealed partial class NamespaceUri : IEquatable<NamespaceUri>, IFormattable {

        private static readonly Dictionary<string, NamespaceUri> namespaces
            = new Dictionary<string, NamespaceUri>();

        private readonly Dictionary<string, QualifiedName> _names
            = new Dictionary<string, QualifiedName>();

        private readonly string _namespaceUri;

        private static readonly NamespaceUri xml = new NamespaceUri("http://www.w3.org/XML/1998/namespace");
        private static readonly NamespaceUri xmlns = new NamespaceUri("http://www.w3.org/2000/xmlns/");
        private static readonly NamespaceUri defaultNamespace = new NamespaceUri("");

        public static NamespaceUri Default { get { return defaultNamespace; } }
        public bool IsDefault { get { return object.ReferenceEquals(this, NamespaceUri.Default); } }
        public string NamespaceName { get { return _namespaceUri; } }

        public static NamespaceUri Xml { get { return xml; } }
        public static NamespaceUri Xmlns { get { return xmlns; } }

        internal NamespaceUri(string namespaceName) {
            // N.B. Argument checking done upstream
            _namespaceUri = namespaceName;
        }

        public static NamespaceUri Create(string namespaceName) {
            if (string.IsNullOrEmpty(namespaceName)) {
                throw Failure.NullOrEmptyString(nameof(namespaceName));
            }

            return Parse(namespaceName);
        }

        public static NamespaceUri Create(Uri uri) {
            if (uri == null) {
                throw new ArgumentNullException("uri");
            }

            return Parse(uri.ToString());
        }

        public static bool TryParse(string text, out NamespaceUri value) {
            value = _TryParse(text, true);
            return value != null;
        }

        public static NamespaceUri Parse(string text) {
            return _TryParse(text, true);
        }

        internal static NamespaceUri _TryParse(string text, bool throwOnError) {
            if (text == null) {
                if (throwOnError) {
                    throw new ArgumentNullException("text");
                }
                return null;
            }
            if (text.Length == 0) {
                return NamespaceUri.Default;
            }

            NamespaceUri result = null;

            if (namespaces.TryGetValue(text, out result)) {
                return result;
            }

            result = new NamespaceUri(text);
            namespaces.Add(text, result);
            return result;
        }

        public QualifiedName GetName(string localName) {
            QualifiedName.VerifyLocalName("localName", localName);

            QualifiedName result = null;

            if (_names.TryGetValue(localName, out result)) {
                return result;
            }

            result = new QualifiedName(this, localName);;
            _names.Add(localName, result);
            return result;
        }

        public static QualifiedName operator +(NamespaceUri ns, string localName) {
            return (ns ?? NamespaceUri.Default).GetName(localName);
        }

        public static bool operator ==(NamespaceUri left, NamespaceUri right) {
            return object.ReferenceEquals(left, right);
        }

        [CLSCompliant(false)]
        public static implicit operator NamespaceUri(string namespaceName) {
            if (namespaceName == null) {
                return null;
            }
            return Parse(namespaceName);
        }

        public static bool operator !=(NamespaceUri left, NamespaceUri right) {
            return !object.ReferenceEquals(left, right);
        }

        public override int GetHashCode() {
            unchecked {
                return 9 * NamespaceName.GetHashCode();
            }
        }

        public override bool Equals(object obj) {
            return Equals(obj as NamespaceUri);
        }

        public static bool Equals(NamespaceUri x, NamespaceUri y) {
            return Equals(x, y, NamespaceUriComparison.Default);
        }

        public static bool Equals(NamespaceUri x, NamespaceUri y, NamespaceUriComparison comparison) {
            if (object.ReferenceEquals(x, y)) {
                return true;
            }

            if (comparison == NamespaceUriComparison.Default) {
                return string.Compare(
                    NormalizeUri(x._namespaceUri), NormalizeUri(y._namespaceUri), StringComparison.OrdinalIgnoreCase
                ) == 0;
            }
            return string.Compare(x._namespaceUri, y._namespaceUri, StringComparison.Ordinal) == 0;
        }

        private static string NormalizeUri(string s) {
            s = Regex.Replace(s, "^(http://)", @"https://");
            s = Regex.Replace(s, "/$", "");

            if (!s.StartsWith("https://")) {
                return "https://" + s;
            }
            return s;
        }

        public override string ToString() {
            return _namespaceUri;
        }

        public string ToString(string format, IFormatProvider formatProvider = null) {
            if (string.IsNullOrEmpty(format)) {
                format = "G";
            }

            if (format.Length > 1) {
                throw new FormatException();
            }

            switch (char.ToLowerInvariant(format[0])) {
                case 'g':
                case 'f':
                    return NamespaceName;
                case 'b':
                    return string.Concat("{", NamespaceName, "}");;

                default:
                    throw new FormatException();
            }
        }

        public bool Equals(NamespaceUri other) {
            return Equals(this, other, NamespaceUriComparison.Default);
        }
    }
}
