//
// Copyright 2005, 2006, 2010, 2012, 2017, 2019-2020 Carbonfrost Systems, Inc.
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
using System.Globalization;
using System.Xml;

using Carbonfrost.Commons.Core.Runtime;

namespace Carbonfrost.Commons.Core {

    public sealed partial class QualifiedName : IEquatable<QualifiedName>, IComparable<QualifiedName>, IFormattable {

        private readonly string _localName;
        private readonly NamespaceUri _ns;

        public string LocalName {
            get {
                return _localName;
            }
        }

        public NamespaceUri Namespace {
            get {
                return _ns;
            }
        }

        public string NamespaceName {
            get {
                return _ns.NamespaceName;
            }
        }

        // Constructors
        internal QualifiedName(NamespaceUri ns, string localName) {
            // N.B. Argument checking is done upstream
            _ns = ns;
            _localName = localName;
        }

        public QualifiedName ChangeLocalName(string value) {
            VerifyLocalName("value", value);
            return Namespace + value;
        }

        public QualifiedName ChangeNamespace(NamespaceUri value) {
            if (value == null) {
                throw new ArgumentNullException("value");
            }
            return value + LocalName;
        }

        public static QualifiedName Create(NamespaceUri namespaceUri, string name) {
            return (namespaceUri ?? NamespaceUri.Default) + name;
        }

        public static QualifiedName Create(Uri namespaceUri, string name) {
            NamespaceUri nu = NamespaceUri.Default;
            if (namespaceUri != null) {
                nu = NamespaceUri.Create(namespaceUri);
            }

            return nu + name;
        }

        public static bool TryParse(string text, IServiceProvider serviceProvider, out QualifiedName result) {
            return _TryParse(text, serviceProvider, out result) == null;
        }

        public static bool TryParse(string text, out QualifiedName result) {
            return _TryParse(text, null, out result) == null;
        }

        public static QualifiedName Parse(string text) {
            QualifiedName result;
            var ex = _TryParse(text, ServiceProvider.Null, out result);
            if (ex != null) {
                throw ex;
            }
            return result;
        }

        public static QualifiedName Parse(string text, IServiceProvider serviceProvider) {
            QualifiedName result;
            var ex = _TryParse(text, serviceProvider, out result);
            if (ex != null) {
                throw ex;
            }
            return result;
        }

        static Exception _TryParse(string text, IServiceProvider serviceProvider, out QualifiedName result) {
            serviceProvider = serviceProvider ?? ServiceProvider.Null;
            result = null;

            if (string.IsNullOrEmpty(text)) {
                throw Failure.NullOrEmptyString(nameof(text));
            }

            // Remove decorations:  [prefix:b] ==> prefix:b
            if (text[0] == '[' && text[text.Length - 1] == ']') {
                text = text.Substring(1, text.Length - 2);

            } else if (text[0] == '{') {
                int num = text.LastIndexOf('}');

                if ((num <= 1) || (num == (text.Length - 1))) {
                    return Failure.NotParsable("text", typeof(QualifiedName));
                }

                if (num - 1 == 0) {
                    // The default namespace is used (as in '{} expandedName')
                    result = NamespaceUri.Default.GetName(text.Trim());
                    return null;

                } else {
                    // Some other namespace is used
                    string ns = text.Substring(1, num - 1);
                    string localName = text.Substring(num + 1).Trim();

                    NamespaceUri nu = NamespaceUri._TryParse(ns, false);
                    if (nu == null) {
                        return Failure.NotParsable("text", typeof(QualifiedName));
                    }
                    result = nu.GetName(localName);
                    return null;
                }

            }

            if (!text.Contains(":")) {
                result = NamespaceUri.Default.GetName(text.Trim());
                return null;
            }

            var resolver = (IXmlNamespaceResolver) serviceProvider.GetService(typeof(IXmlNamespaceResolver))
                ?? XmlNamespaceResolver.Global;

            int index = text.IndexOf(':');

            string prefix = text.Substring(0, index);
            string name = text.Substring(index + 1);
            string fullNs = resolver.LookupNamespace(prefix);
            if (fullNs != null) {
                result = QualifiedName.Create(fullNs, name);
                return null;
            }
            return Failure.NotParsable("text", typeof(QualifiedName), RuntimeFailure.CannotExpandPrefixNotFound(prefix));
        }

        public static QualifiedName Create(string namespaceName, string localName) {
            return NamespaceUri.Parse(namespaceName).GetName(localName);
        }

        // Operators.
        public static bool operator ==(QualifiedName left, QualifiedName right) {
            return StaticEquals(left, right);
        }

        public static bool operator !=(QualifiedName left, QualifiedName right) {
            return !StaticEquals(left, right);
        }

        // 'Object' overrides.
        public override bool Equals(object obj) {
            return Equals(obj as QualifiedName);
        }

        public override int GetHashCode() {
            unchecked {
                var result = 1000000009 * _localName.GetHashCode();
                result += 1000000021 * _ns.GetHashCode();
                return result;
            }
        }

        public override string ToString() {
            return FullName();
        }

        string FullName() {
            if (_ns.IsDefault) {
                return _localName;
            } else {
                return string.Concat("{", _ns.NamespaceName, "} ", _localName);
            }
        }

        public string ToString(string format) {
            return ToString(format, CultureInfo.InvariantCulture);
        }

        // IFormattable
        public string ToString(string format, IFormatProvider formatProvider) {
            if (string.IsNullOrEmpty(format))
                return FullName();

            if (format.Length > 1)
                throw new FormatException();

            switch (char.ToUpperInvariant(format[0])) {
                case 'G':
                case 'F':
                    return FullName();
                case 'S':
                    return _localName;
                case 'N':
                    return _ns.NamespaceName;
                case 'M':
                    return string.Concat("{", _ns.NamespaceName, "}");
                case 'C':
                    var resolver = (IXmlNamespaceResolver) formatProvider.GetFormat(typeof(IXmlNamespaceResolver))
                        ?? XmlNamespaceResolver.Global;
                    string prefix = resolver.LookupPrefix(NamespaceName);
                    return string.Format("[{0}:{1}]", prefix, LocalName);
                default:
                    throw new FormatException();
            }
        }

        // 'IEquatable' implementation.
        internal static bool StaticEquals(QualifiedName a, QualifiedName b) {
            if (object.ReferenceEquals(a, b)) {
                return true;
            } else if (object.ReferenceEquals(a, null)
                       || object.ReferenceEquals(b, null)) {
                return false;
            } else {
                return a._localName == b._localName
                    && object.Equals(a._ns, b._ns);
            }
        }

        public bool Equals(QualifiedName other) {
            return StaticEquals(this, other);
        }

        internal static void VerifyLocalName(string argName, string value) {
            if (value == null) {
                throw new ArgumentNullException(argName);
            }
            if (value.Length == 0) {
                throw Failure.EmptyString(argName);
            }

            foreach (char c in value) {
                if (!IsValidChar(c))
                    throw RuntimeFailure.NotValidLocalName(argName);
            }
        }

        static bool IsValidChar(char c) {
            return ('A' <= c && c <= 'Z')
                || ('a' <= c && c <= 'z')
                || ('0' <= c && c <= '9')
                || (c == '-' || c == '_' || c == '.');
        }

        // `IComparable' implementation
        public int CompareTo(QualifiedName other) {
            if (other == null)
                return 1;
            else
                return string.Compare(FullName(), other.FullName(), StringComparison.Ordinal);
        }

        internal bool EqualsIgnoreCase(QualifiedName name) {
            return this == name
                || (Namespace == name.Namespace && string.Equals(LocalName, name.LocalName, StringComparison.OrdinalIgnoreCase));
        }
    }
}
