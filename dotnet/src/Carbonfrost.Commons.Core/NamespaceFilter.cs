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
using System.Text;
using System.Text.RegularExpressions;

namespace Carbonfrost.Commons.Core {

    class NamespaceFilter {

        private readonly Regex _filter;

        public NamespaceFilter(string pattern) {
            if (string.IsNullOrWhiteSpace(pattern) || pattern == "*") {
                _filter = null;
            } else {
                _filter = GetNamespaceFilterRegex(pattern);
            }
        }

        public IEnumerable<string> Filter(IEnumerable<string> all) {
            if (_filter == null) {
                return all;
            }

            return all.Where(t => IsMatch(t ?? string.Empty));
        }

        public bool IsMatch(string t) {
            return _filter.IsMatch(t ?? string.Empty);
        }

        private static Regex GetNamespaceFilterRegex(string pattern) {
            StringBuilder sb = new StringBuilder();

            foreach (string p in pattern.Split(',')) {
                if (sb.Length > 0) {
                    sb.Append("|");
                }

                sb.Append(GetNamespaceFilterRegexInternal(p));
            }

            Regex r = new Regex(sb.ToString());
            return r;
        }

        private static string GetNamespaceFilterRegexInternal(string pattern) {
            // Last one is special (allow .* to be used at end)
            pattern = pattern.Trim();
            if (pattern.EndsWith(".*", StringComparison.Ordinal)) {
                pattern = pattern.Substring(0, pattern.Length - 2) + (@"(\..+)?");
            }

            return string.Concat("(^", pattern.Replace("*", ".+?"), "$)");
        }

    }
}


