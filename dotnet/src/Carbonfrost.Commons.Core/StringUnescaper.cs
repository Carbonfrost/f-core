//
// Copyright 2020 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using System.Text;
using Carbonfrost.Commons.Core.Runtime;

namespace Carbonfrost.Commons.Core {

    static class StringUnescaper {

        public static string Unescape(string text) {
            var c = ((IEnumerable<char>) text).GetEnumerator();
            return Unescape(c);
        }

        public static string Unescape(IEnumerator<char> text) {
            StringBuilder result = new StringBuilder();

            while (text.MoveNext()) {
                var c = text.Current;
                if (c == '\\') {
                    result.Append(UnescapeChar(text));
                } else {
                    result.Append(c);
                }
            }
            return result.ToString();
        }

        internal static char UnescapeChar(IEnumerator<char> e) {
            char? c0 = e.RequireNext();
            if (!c0.HasValue) {
                throw RuntimeFailure.IncompleteEscapeSequence();
            }

            char c = c0.Value;

            switch (c) {
                case 'b':
                    return '\b';

                case 't':
                    return '\t';

                case 'n':
                    return '\n';

                case 'f':
                    return '\f';

                case 'r':
                    return '\r';

                case 'u':
                    return UnescapeUnicode(e.RequireNext(4));

                case 'x':
                    return UnescapeHex(e.RequireNext(2));

                case '0':
                    return '\0';

                default:
                    return c;
            }
        }


        private static char UnescapeUnicode(string chars) {
            if (chars == null) {
                throw RuntimeFailure.IncompleteEscapeSequence();
            }
            int unicodeValue = Convert.ToInt32(chars, 16);
            return Convert.ToChar(unicodeValue);
        }

        public static char UnescapeHex(string chars) {
            if (chars == null) {
                throw RuntimeFailure.IncompleteEscapeSequence();
            }

            int hexValue = Convert.ToInt32(chars, 16);
            return Convert.ToChar(hexValue);
        }
    }

}
