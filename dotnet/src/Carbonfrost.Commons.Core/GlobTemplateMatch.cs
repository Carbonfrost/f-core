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
using System.Collections.ObjectModel;

namespace Carbonfrost.Commons.Core {

    public class GlobTemplateMatch {

        private readonly IReadOnlyDictionary<string, string> _data;
        private readonly bool _success;
        private readonly string _value;

        public static readonly GlobTemplateMatch Empty
            = new GlobTemplateMatch(null, new Dictionary<string, string>(), false);

        public IReadOnlyDictionary<string, string> Data {
            get {
                return _data;
            }
        }

        public string this[string name] {
            get {
                string result;
                if (_data.TryGetValue(name, out result)) {
                    return result;
                }
                return null;
            }
        }

        public bool Success {
            get {
                return _success;
            }
        }

        public string FileName {
            get {
                return _value;
            }
        }

        internal GlobTemplateMatch(string value, IDictionary<string, string> dict, bool success) {
            _data = new ReadOnlyDictionary<string, string>(dict);
            _success = success;
            _value = value;
        }

        public override string ToString() {
            return Convert.ToString(FileName);
        }
    }
}
