//
// Copyright 2013, 2019 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using System.IO;
using System.Text;

namespace Carbonfrost.Commons.Core.Runtime {

    public abstract class TextSource : StreamingSource {

        public Encoding Encoding { get; set; }

        public abstract void Save(TextWriter writer, object value);
        public abstract object Load(TextReader reader, Type instanceType);

        public virtual void Load(TextReader reader, object instance) {
            if (reader == null) {
                throw new ArgumentNullException("reader");
            }

            var copyFrom = Load(reader, instance.GetType());
            Template.Copy(copyFrom, instance);
        }

        public override void Save(StreamContext outputTarget,
                                  object value) {

            if (outputTarget == null) {
                throw new ArgumentNullException("outputTarget");
            }
            if (value == null) {
                throw new ArgumentNullException("value");
            }

            using (StreamWriter sw = outputTarget.AppendText(Encoding)) {
                Save(sw, value);
            }
        }

        public override object Load(StreamContext inputSource,
                                    Type instanceType) {
            if (inputSource == null) {
                throw new ArgumentNullException("inputSource");
            }

            using (StreamReader sr = inputSource.OpenText()) {
                return Load(sr, instanceType);
            }
        }

        internal override object LoadFromText(string text, Type componentType) {
            return Load(new StringReader(text), componentType);
        }
    }
}

