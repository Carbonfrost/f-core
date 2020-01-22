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
using System.Text;
using System.Text.RegularExpressions;

namespace Carbonfrost.Commons.Core {

    public partial class Glob {

        private readonly string _text;
        private readonly SegmentSequence[] _segments;
        private readonly Lazy<Regex> _regexCache;
        private readonly GlobController _controller;

        public bool IsAnything {
            get { return this.Equals(Glob.Anything); }
        }

        internal IEnumerable<SegmentSequence> Segments {
            get { return _segments; }
        }

        private Glob(string text, SegmentSequence[] segments) {
            _text = text;
            _segments = segments;
            _regexCache = new Lazy<Regex>(MakeRegex);
            _controller = new GlobController();
        }

        protected Glob(string text, Glob.GlobController controller) {
            if (controller == null) {
                controller = new GlobController();
            }
            text = controller.ExpandEnvironmentVariables(text);
            Exception ex = _TryParse(text, out _segments);
            if (ex != null) {
                throw ex;
            }
            _text = text;
            _regexCache = new Lazy<Regex>(MakeRegex);
            _controller = controller;
        }

        public IEnumerable<string> EnumerateDirectories() {
            return EnumerateDirectories(null);
        }

        public IEnumerable<string> EnumerateDirectories(string workingDirectory) {
            return Glob.FilterDirectory(this, workingDirectory ?? _controller.WorkingDirectory, _controller.DirectoryExists, this._controller);
        }

        public IEnumerable<string> EnumerateFiles() {
            return EnumerateFiles(null);
        }

        public IEnumerable<string> EnumerateFiles(string workingDirectory) {
            return Glob.FilterDirectory(this, workingDirectory ?? _controller.WorkingDirectory, _controller.FileExists, this._controller);
        }

        public IEnumerable<string> EnumerateFileSystemEntries() {
            return EnumerateFileSystemEntries(null);
        }

        public IEnumerable<string> EnumerateFileSystemEntries(string workingDirectory) {
            throw new NotImplementedException();
        }

        public bool IsMatch(string input) {
            return ToRegex().IsMatch(input);
        }

        public Regex ToRegex() {
            return this._regexCache.Value;
        }

        public override string ToString() {
            return this._text;
        }

        // `object' overrides
        public override bool Equals(object obj) {
            return StaticEquals(this, obj as Glob);
        }

        public override int GetHashCode() {
            int hashCode = 0;
            unchecked {
                if (_text != null)
                    hashCode += 7 * _text.GetHashCode();
            }
            return hashCode;
        }

        public static bool operator ==(Glob lhs, Glob rhs) {
            return StaticEquals(lhs, rhs);
        }

        public static bool operator !=(Glob lhs, Glob rhs) {
            return !StaticEquals(lhs, rhs);
        }

        static bool StaticEquals(Glob lhs, Glob rhs) {
            if (ReferenceEquals(lhs, rhs))
                return true;
            if (ReferenceEquals(lhs, null) || ReferenceEquals(rhs, null))
                return false;

            return lhs._text == rhs._text;
        }

        private Regex MakeRegex() {
            StringBuilder text = new StringBuilder();
            bool needPipe = false;

            foreach (SegmentSequence segments in this._segments) {
                if (needPipe) {
                    text.Append("|");
                }

                segments.AppendRegex(text);

                needPipe = true;
            }

            text.Append("$");
            return new Regex(text.ToString());
        }
    }
}
