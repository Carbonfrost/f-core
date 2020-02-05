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
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Carbonfrost.Commons.Core {

    partial class Glob {

        static readonly string NON_PATH_SEP = @"[^/\:]";
        static readonly char[] SPECIAL_CHARS = { '*', '?', '[', ']' };
        static readonly Regex DEVICE = new Regex(@"(?<Name> [a-z])\:", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

        static readonly string PlatformMatchDirectorySeparator = GetDirectorySeparator();

        public static readonly Glob Anything = Glob.Parse("**/*.*");

        public static Glob Combine(IEnumerable<Glob> items) {
            if (items == null)
                throw new ArgumentNullException("items");

            return Combine(items.ToArray<Glob>());
        }

        public static Glob Combine(params Glob[] items) {
            if (items == null) {
                throw new ArgumentNullException("items");
            }
            switch (items.Length) {
                case 0:
                    return Anything;

                case 1:
                    return items[0];

                case 2:
                    return Combine(items[0], items[1]);

                case 3:
                    return Combine(items[0], items[1], items[2]);
            }

            if (items.Any(t => t.IsAnything)) {
                return Anything;
            }

            return new Glob(
                string.Join(";", items.Select(t => t._text)),
                items.SelectMany(t => t._segments).ToArray());
        }

        public static Glob Combine(Glob arg1, Glob arg2) {
            if (arg1 == null) {
                throw new ArgumentNullException("arg1");
            }

            if (arg2 == null) {
                throw new ArgumentNullException("arg2");
            }

            if (object.ReferenceEquals(arg1, arg2))
                return arg1;

            if (arg1.IsAnything || arg2.IsAnything)
                return Glob.Anything;

            return new Glob(arg1._text + ";" + arg2._text,
                            arg1._segments.Concat(arg2._segments).ToArray<SegmentSequence>());
        }

        public static Glob Combine(Glob arg1, Glob arg2, Glob arg3) {
            if (arg1 == null) {
                throw new ArgumentNullException("arg1");
            }

            if (arg2 == null) {
                throw new ArgumentNullException("arg2");
            }

            if (arg3 == null) {
                throw new ArgumentNullException("arg3");
            }

            return Combine(arg1, Combine(arg2, arg3));
        }

        public static Glob Concat(string path, Glob glob) {
            if (glob == null) {
                throw new ArgumentNullException("glob");
            }
            var globParts = glob.ToString().Split(';');
            var result = string.Join(";", globParts.Select(p => Path.Combine(path, p)));
            return Glob.Parse(result);
        }

        public static Glob Parse(string text) {
            Glob result;
            Exception ex = _TryParse(text, out result);
            if (ex == null)
                return result;
            else
                throw ex;
        }

        public static bool TryParse(string text, out Glob value) {
            return _TryParse(text, out value) == null;
        }

        static Exception _TryParseList(string text, out IteratedSegmentSequence segments) {
            segments = null;
            string[] items = text.Split('/', '\\');
            List<Segment> results = new List<Segment>(items.Length);

            foreach (string s in items) {
                Segment sgt;
                if (s.Length == 0) {
                    // TODO Only apply at real root; enforce match segment as file or directory
                    sgt = new RootSegment();

                } else if (DEVICE.IsMatch(s)) {
                    sgt = new DeviceSegment(DEVICE.Match(s).Groups["Name"].Value);

                } else if (s == "*") {
                    sgt = new AnyDirectorySegment();

                } else if (s == "**") {
                    sgt = new RecursiveSegment();

                    // TODO Support directory navigation
                } else if (s == "..") {
                    throw new NotImplementedException();

                } else if (s == ".") {
                    sgt = new CwdSegment();

                } else {
                    string segment = ExpandSegment(s);
                    if (segment == null)
                        return Failure.NotParsable("text", typeof(Glob));

                    sgt = new MatchSegment(segment);
                }

                results.Add(sgt);
            }

            segments = new IteratedSegmentSequence(results.ToArray());
            return null;
        }

        static Exception _TryParse(string text, out Glob value) {
            return _TryParse(text, new GlobController(), out value);
        }

        static Exception _TryParse(string text, GlobController controller, out Glob value) {
            text = controller.ExpandEnvironmentVariables(text);

            SegmentSequence[] segments;
            Exception ex = _TryParse(text, out segments);
            value = null;

            if (ex == null) {
                value = new Glob(text, segments);
                return null;
            } else
                return ex;
        }

        static Exception _TryParse(string text, out SegmentSequence[] results2) {
            results2 = null;
            if (string.IsNullOrEmpty(text)) {
                return Failure.NullOrEmptyString(nameof(text));
            }

            List<SegmentSequence> results = new List<SegmentSequence>();

            foreach (string sub in text.Split(';')) {

                if (Path.IsPathRooted(sub) && NoSpecialChars(sub)) {
                    RootedSegmentSequence sequence = new RootedSegmentSequence(sub);
                    results.Add(sequence);

                } else {
                    IteratedSegmentSequence segments;
                    _TryParseList(sub, out segments);
                    results.Add(segments);
                }
            }

            results2 = results.ToArray();
            return null;
        }

        static bool NoSpecialChars(string s) {
            return s.IndexOfAny(SPECIAL_CHARS) < 0;
        }

        static string ExpandSegment(string s) {
            StringBuilder sb = new StringBuilder(s.Length + 3);
            sb.Append(PlatformMatchDirectorySeparator);

            StringBuilder b = null;
            var e = ((IEnumerable<char>) s).GetEnumerator();

            while (e.MoveNext()) {
                char c = e.Current;
                switch (c) {
                    case '[':
                        if (b != null)
                            return null;
                        b = new StringBuilder("(");
                        break;

                    case ']':
                        if (b == null)
                            return null;
                        b.Append(")");
                        sb.Append(b.ToString());
                        b = null;
                        break;

                    case '.':
                    case '(':
                    case ')':
                        sb.Append('\\');
                        sb.Append(c);
                        break;

                    case '?':
                        sb.Append(NON_PATH_SEP);
                        break;
                    case '*':
                        sb.Append(NON_PATH_SEP + "*");
                        break;

                    default:
                        if (b != null)
                            b.Append(b.Length > 1 ? "|" : null).Append(c);
                        else
                            sb.Append(c);
                        break;
                }
            }

            sb.Append("$");
            return sb.ToString();
        }

        protected internal abstract class Segment {
            internal abstract IEnumerable<string> Enumerate(IEnumerable<string> items,
                                                            GlobController enumerator);

            public abstract string ToRegexString();
        }

        internal static IEnumerable<string> FilterDirectory(Glob glob,
                                                            string directory,
                                                            Predicate<string> match,
                                                            GlobController enumerator) {

            var items = Enumerable.Empty<string>();
            IEnumerable<string> subItemsStart = new[] { enumerator.ExpandEnvironmentVariables(directory) };
            foreach (SegmentSequence sub in glob._segments) {
                var subItems = sub.Enumerate(subItemsStart, enumerator);
                items = items.Concat(subItems);
            }

            return items
                .Where(t => match(t))
                .Distinct();
        }

        static string GetDirectorySeparator() {
            return "/";
        }

        internal abstract class SegmentSequence {

            internal abstract IEnumerable<string> Enumerate(IEnumerable<string> items,
                                                            GlobController enumerator);

            public abstract void AppendRegex(StringBuilder text);
        }

        // Corresponds to a sequence that is rooted and contains
        // no patterns
        class RootedSegmentSequence : SegmentSequence {

            private readonly string text;

            public RootedSegmentSequence(string text) {
                this.text = text;
            }

            internal override IEnumerable<string> Enumerate(IEnumerable<string> items,
                                                            GlobController enumerator) {
                return new [] { text };
            }

            public override void AppendRegex(StringBuilder text)
            {
                text.Append(this.text);
            }
        }

        class IteratedSegmentSequence : SegmentSequence {

            private readonly Segment[] segments;

            public IteratedSegmentSequence(Glob.Segment[] segments) {
                this.segments = segments;
            }

            internal override IEnumerable<string> Enumerate(IEnumerable<string> items,
                                                            GlobController enumerator) {
                foreach (Segment s in this.segments) {
                    items = s.Enumerate(items, enumerator);
                }

                return items;
            }

            public override void AppendRegex(StringBuilder text)
            {
                foreach (Segment s in segments) {
                    text.Append(s.ToRegexString());
                }
            }
        }

        class MatchSegment : Segment {

            private readonly Regex regex;

            public MatchSegment(string pattern) {
                // TODO We may want to re-allow case sensitivity
                RegexOptions options = RegexOptions.IgnoreCase;
                this.regex = new Regex(pattern, options);
            }

            internal override IEnumerable<string> Enumerate(IEnumerable<string> items,
                                                            GlobController enumerator) {
                var result = items.SelectMany(t => enumerator.EnumerateFileSystemEntries(t)).Where(t => regex.IsMatch(t));
                return result;
            }

            public override string ToRegexString() {
                // Remove trailing $
                string s = regex.ToString();
                return s.Substring(0, s.Length - 1);
            }
        }

        class RecursiveSegment : Segment {

            internal override IEnumerable<string> Enumerate(IEnumerable<string> items,
                                                            GlobController enumerator) {

                Queue<string> results = new Queue<string>(enumerator.OnlyDirectories(items));
                while (results.Count > 0) {
                    var item = results.Dequeue();
                    yield return item;

                    var descendents = enumerator.EnumerateDirectories(item);
                    foreach (var d in descendents)
                        results.Enqueue(d);
                }
            }

            public override string ToRegexString() {
                return "(" + NON_PATH_SEP + "+/)*";
            }
        }

        class DeviceSegment : Segment {

            private readonly string device;

            public DeviceSegment(string device) {
                this.device = device;
            }

            internal override IEnumerable<string> Enumerate(IEnumerable<string> items,
                                                            GlobController enumerator) {

                string deviceName = (device + ":" + "/");
                return new [] { deviceName };
            }

            public override string ToRegexString() {
                return "^" + device + ":" + "/";
            }
        }

        class CwdSegment : Segment {

            public CwdSegment() {
            }

            internal override IEnumerable<string> Enumerate(IEnumerable<string> items,
                                                            GlobController enumerator) {
                return items.Where(t => enumerator.DirectoryExists(t));
            }

            public override string ToRegexString() {
                return "";
            }

        }

        class RootSegment : Segment {

            internal override IEnumerable<string> Enumerate(IEnumerable<string> items,
                                                            GlobController enumerator) {

                return new [] { "/" };
            }

            // TODO On Windows, should match / and C:/

            public override string ToRegexString() {
                return "^/";
            }
        }

        class AnyDirectorySegment : Segment {

            internal override IEnumerable<string> Enumerate(IEnumerable<string> items,
                                                            GlobController enumerator) {

                return items.SelectMany(t => enumerator.EnumerateDirectories(t));
            }

            public override string ToRegexString() {
                return NON_PATH_SEP + "+" + "/";
            }
        }

    }


}
