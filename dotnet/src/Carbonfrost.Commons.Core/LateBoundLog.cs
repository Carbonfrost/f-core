//
// Copyright 2013, 2020 Carbonfrost Systems, Inc. (https://carbonfrost.com)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     https://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using System;

namespace Carbonfrost.Commons.Core {

    static class LateBoundLog {

        private static Action<string, Exception> _callback;
        private static Action<string> _traceCallback;

        [FriendAccessAllowed]
        public static void SetFailCallback(Action<string, Exception> callback) {
            LateBoundLog._callback = callback;
        }

        [FriendAccessAllowed]
        public static void SetTraceCallback(Action<string> callback) {
            LateBoundLog._traceCallback = callback;
        }

        public static void Fail(string text, Exception ex = null) {
            if (_callback != null) {
                _callback(text, ex);
            }
        }

        public static void Try(string message, Action action) {
            try {
                action();
            } catch (Exception ex) {
                Fail(message, ex);
            }
        }

        public static void Trace(string text) {
            if (_traceCallback != null) {
                _traceCallback(text);
            }
        }
    }
}
