//
// Copyright 2005, 2006, 2010 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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

namespace Carbonfrost.Commons.Core.Runtime {

    sealed class TextStreamingSource : StreamingSource {

        public override void Save(StreamContext outputTarget,
                                  object value)
        {
            if (value == null) {
                throw new ArgumentNullException("value");
            }

            string text = value.ToString();
            TextWriter writer = outputTarget.AppendText();
            writer.Write(text);
        }

        public override object Load(StreamContext inputSource,
                                    Type instanceType)
        {
            if (instanceType == null) {
                throw new ArgumentNullException("instanceType");
            }

            return Activation.FromStreamContext(instanceType, inputSource);
        }

    }
}

