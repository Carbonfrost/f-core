//
// Copyright 2016 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Carbonfrost.Commons.Core.Runtime {

    public class PropertyProviderFormat {

        private static readonly Regex EXPR_FORMAT = new Regex(
@"
(
    (?<DD> \$\$|\$$|\$(?=\s))
    | \$ \{ (?<Exp>  [^\}]*        )  (?<ExpEnd> \} | $ )
    | \$    (?<Exp>  [:a-z0-9_\.]+ )
)", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

        private readonly IReadOnlyList<Expr> _expressions;

        private PropertyProviderFormat(Expr[] exp) {
            _expressions = exp;
        }

        protected PropertyProviderFormat(string text) {
            _expressions = Parse(text)._expressions;
        }

        public string Format(object args) {
            return Format((IPropertyProvider) Properties.FromValue(args));
        }

        public string Format(IPropertyProvider propertyProvider) {
            if (propertyProvider == null) {
                return ToString();
            }
            var sb = new StringBuilder();
            foreach (var e in _expressions) {
                sb.Append(e.Eval(propertyProvider));
            }
            return sb.ToString();
        }

        public string Format(IEnumerable<KeyValuePair<string, object>> propertyProvider) {
            return Format(PropertyProvider.Compose(propertyProvider));
        }

        public static PropertyProviderFormat Parse(string text) {
            PropertyProviderFormat result;
            Exception ex = _TryParse(text, out result);
            if (ex == null)
                return result;
            else
                throw ex;
        }

        public static bool TryParse(string text, out PropertyProviderFormat result) {
            return _TryParse(text, out result) == null;
        }

        static Exception _TryParse(string text, out PropertyProviderFormat result) {
            var items = ParseCore(text).ToArray();

            if (items.OfType<Error>().Any()) {
                result = null;
                return Failure.NotParsable("text", typeof(PropertyProviderFormat));
            }

            result = new PropertyProviderFormat(items);
            return null;
        }

        static object Eval(IPropertyProvider pp, string tokenName) {
            object result = pp.GetProperty(tokenName);

            Delegate d = result as Delegate;
            if (d == null) {
                return result;
            }
            var type = d.GetType().GetTypeInfo();

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Func<,>)) {
                return d.DynamicInvoke(tokenName);
            }
            return d.DynamicInvoke(null);
        }

        abstract class Expr {
            public abstract string Eval(IPropertyProvider pp);
        }

        sealed class Literal : Expr {
            private readonly string _text;

            public Literal(string text) {
                _text = text;
            }

            public override string Eval(IPropertyProvider pp) {
                return _text;
            }

            public override string ToString() {
                return _text;
            }
        }

        sealed class Expansion : Expr {
            private readonly string _text;

            public Expansion(string text) {
                _text = text;
            }

            public override string Eval(IPropertyProvider pp) {
                return Convert.ToString(PropertyProviderFormat.Eval(pp, _text));
            }

            public override string ToString() {
                return "${" + _text + "}";
            }
        }

        sealed class Error : Expr {
            public override string Eval(IPropertyProvider pp) {
                return null;
            }
        }

        sealed class DD : Expr {
            public override string Eval(IPropertyProvider pp) {
                return "$";
            }
            public override string ToString() {
                return "$$";
            }
        }

        public override string ToString() {
            return string.Concat(_expressions);
        }

        static IEnumerable<Expr> ParseCore(string text) {
            MatchCollection matches = EXPR_FORMAT.Matches(text);
            int previousIndex = 0;
            foreach (Match match in matches) {
                yield return new Literal(text.Substring(previousIndex, match.Index - previousIndex));

                if (match.Groups["DD"].Success) {
                    yield return new DD();

                } else {
                    string expText = match.Groups["Exp"].Value;
                    if (expText.Length == 0 || Regex.IsMatch(expText, @"\s")) {
                        yield return new Error();
                    }
                    else if (match.Groups["ExpEnd"].Success && match.Groups["ExpEnd"].Value != "}") {
                        yield return new Error();
                    }
                    else {
                        yield return new Expansion(expText);
                    }
                }

                previousIndex = match.Index + match.Length;
            }
            yield return new Literal(text.Substring(previousIndex, text.Length - previousIndex));
        }
    }

}
