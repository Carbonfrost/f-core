//
// Copyright 2005, 2006, 2010, 2016, 2019 Carbonfrost Systems, Inc.
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
using System.IO;
using System.Text;

namespace Carbonfrost.Commons.Core.Runtime {

    class PropertiesReader : DisposableObject {

        public const string AnnotationKey = "#annotation";
        public const string CategoryKey = "#category";

        private string _key;
        private string _value;
        private string _category = string.Empty;
        private PropertyNodeKind _nodeKind;

        private TextReader BaseReader { get; set; }

        public PropertyNodeKind NodeKind { get { return _nodeKind; } }

        public string QualifiedKey {
            get {
                if (_category.Length == 0) {
                    return _key;
                } else {
                    return _category + "." + _key;
                }
            }
        }

        public string Value { get { return _value; } }

        public PropertiesReader(TextReader source) {
            BaseReader = source;
        }

        public PropertiesReader(StreamContext source, Encoding encoding = null) {
            BaseReader = source.OpenText(encoding);
        }

        public bool Read() {
            string line = BaseReader.ReadLine();
            if (line == null) { return false; }

            // This removes preceeding whitespace on continued lines
            line = line.Trim();

            // Skip blank lines
            while (line.Length == 0) {
                line = BaseReader.ReadLine();
                if (line == null) { return false; }
                line = line.Trim();
            }

            // This is a category line
            if (line.StartsWith("[", StringComparison.Ordinal)) {
                if (line.EndsWith("]", StringComparison.Ordinal)) {
                    EnterCategory(line);
                } else
                    throw RuntimeFailure.PropertiesCategoryMissingBrackets();


            } else {
                // Either pick this as a comment or a property
                if (line[0] == ';' || line[0] == '!' || line[0] == '#') {
                    EnterComment(line);

                } else {
                    StringBuilder buffer = new StringBuilder();
                    // Deal with line continuations \
                    // New to 1.3: assume that if a equals sign is missing, it is part
                    // of the previous line

                    while (line != null && line.EndsWith("\\", StringComparison.Ordinal)) {
                        buffer.Append(line, 0, line.Length - 1);
                        line = this.BaseReader.ReadLine();

                        if (line != null) {
                            line = line.Trim();
                        }
                    }
                    if (line != null) {
                        buffer.AppendLine(line);
                    }
                    EnterText(buffer.ToString());
                }
            }

            return true;
        }

        public bool MoveToProperty() {
            bool readResult = false;
            // Skip past comments and categories
            do {
                readResult = Read();
            } while (NodeKind != PropertyNodeKind.Property && readResult == true);

            return readResult;
        }

        public IEnumerable<KeyValuePair<string, string>> ReadToEnd() {
            while (MoveToProperty()) {
                yield return new KeyValuePair<string, string>(QualifiedKey, Value);
            }
        }

        private void EnterCategory(string categoryDeclaration) {
            _key = PropertiesReader.CategoryKey;
            _value = _category =
                categoryDeclaration.Substring(1, categoryDeclaration.Length - 2);
            _nodeKind = PropertyNodeKind.Category;
        }

        private void EnterComment(string commentLine) {
            _nodeKind = PropertyNodeKind.Annotation;
            _key = PropertiesReader.AnnotationKey;
            if (commentLine.Length == 1) {
                _value = string.Empty;

            } else {
                _value = commentLine.Substring(1);
            }
        }

        private void EnterText(string line) {
            int equalsIndex = line.IndexOf('=');
            if (equalsIndex < 0) {
                throw RuntimeFailure.PropertyDeclarationMissingKey();
            }

            string newKey = null;
            string newValue = null;
            if (equalsIndex == line.Length - 1) {
                newKey = line.Substring(0, line.Length - 1);
                newValue = string.Empty;

            } else {
                newKey = line.Substring(0, equalsIndex);
                newValue = line.Substring(equalsIndex + 1).Trim();
            }

            _key = newKey;
            _value = StringUnescaper.Unescape(newValue);
            _nodeKind = PropertyNodeKind.Property;
        }
    }

}
