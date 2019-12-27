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

    public class GlobTemplate {

        private readonly List<string> _variables;
        private static readonly Regex EXPR_FORMAT = new Regex(@"(\{ (?<Expression> [^\}]+) \})", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
        private readonly Regex _regex;
        private readonly Glob _glob;

        public IReadOnlyList<string> Variables {
            get {
                return _variables;
            }
        }

        public Glob Glob {
            get {
                return _glob;
            }
        }

        internal Regex Pattern {
            get {
                return _regex;
            }
        }

        private GlobTemplate(IEnumerable<string> variables, string glob, string regex) {
            _variables = new List<string>(variables);
            _regex = new Regex(regex);
            _glob = Glob.Parse(glob);
        }

        public static GlobTemplate Parse(string text) {
            GlobTemplate result;
            Exception ex = _TryParse(text, out result);
            if (ex != null) {
                throw ex;
            }
            return result;
        }

        public static bool TryParse(string text, out GlobTemplate result) {
            return _TryParse(text, out result) == null;
        }

        private static Exception _TryParse(string text, out GlobTemplate result) {
            MatchCollection matches = EXPR_FORMAT.Matches(text);
            int previousIndex = 0;
            StringBuilder regexBuilder = new StringBuilder();
            StringBuilder globBuilder = new StringBuilder();
            List<string> variables = new List<string>();

            foreach (Match match in matches) {
                var literal = text.Substring(previousIndex, match.Index - previousIndex);

                regexBuilder.Append(Regex.Escape(literal));
                globBuilder.Append(literal);
                string expText = match.Groups["Expression"].Value;
                regexBuilder.AppendFormat("(?<{0}>.*?)", expText);
                globBuilder.Append("*");
                previousIndex = match.Index + match.Length;
                variables.Add(expText);
            }

            var literal2 = text.Substring(previousIndex, text.Length - previousIndex);
            regexBuilder.Append(Regex.Escape(literal2));
            globBuilder.Append(literal2);
            regexBuilder.Append("$");

            string regex = regexBuilder.ToString();
            // remove ./ except if the string starts with it
            regex = Regex.Replace(regex, @"(?<!^)\\\./", "");
            regex = "^.*" + regex.Replace(@"\./", "/");

            result = new GlobTemplate(variables, globBuilder.ToString(), regex);
            return null;
        }

        public bool IsMatch(string input) {
            return Glob.IsMatch(input);
        }

        public IEnumerable<GlobTemplateMatch> EnumerateDirectories() {
            return NewMatches(WindowsPathSeparators(Glob.EnumerateDirectories()));
        }

        public IEnumerable<GlobTemplateMatch> EnumerateDirectories(string workingDirectory) {
            return NewMatches(WindowsPathSeparators(Glob.EnumerateDirectories(workingDirectory)));
        }

        public IEnumerable<GlobTemplateMatch> EnumerateFiles() {
            return NewMatches(WindowsPathSeparators(Glob.EnumerateFiles()));
        }

        public IEnumerable<GlobTemplateMatch> EnumerateFiles(string workingDirectory) {
            return NewMatches(WindowsPathSeparators(Glob.EnumerateFiles(workingDirectory)));
        }

        public IEnumerable<GlobTemplateMatch> EnumerateFiles(IEnumerable<string> paths) {
            foreach (var p in paths) {
                foreach (var match in WindowsPathSeparators(Glob.EnumerateFiles(p))) {
                    yield return NewMatch(match);
                }
            }
        }

        public IEnumerable<GlobTemplateMatch> EnumerateFileSystemEntries() {
            return NewMatches(WindowsPathSeparators(Glob.EnumerateFileSystemEntries()));
        }

        public IEnumerable<GlobTemplateMatch> EnumerateFileSystemEntries(string workingDirectory) {
            return NewMatches(WindowsPathSeparators(Glob.EnumerateFileSystemEntries(workingDirectory)));
        }

        public override string ToString() {
            return Glob.ToString();
        }

        static IEnumerable<string> WindowsPathSeparators(IEnumerable<string> paths) {
            // TODO Only applies to Windows
            return paths.Select(t => t.Replace("\\", "/"));
        }

        private GlobTemplateMatch NewMatch(string match) {
            var m = _regex.Match(match);
            var vars = _regex.GetGroupNames().ToDictionary(t => t, t => m.Groups[t].Value);
            return new GlobTemplateMatch(match, vars, true);
        }

        private IEnumerable<GlobTemplateMatch> NewMatches(IEnumerable<string> matches) {
            return matches.Select(m => NewMatch(m));
        }
    }

}
