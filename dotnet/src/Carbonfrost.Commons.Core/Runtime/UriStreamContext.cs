//
// Copyright 2005, 2006, 2010, 2020 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using System.Net.Http;

namespace Carbonfrost.Commons.Core.Runtime {

    internal sealed class UriStreamContext : StreamContext {

        private readonly Uri _uri;

        public UriStreamContext(Uri uri) {
            if (uri == null) {
                throw new ArgumentNullException(nameof(uri));
            }
            _uri = uri;
        }

        public override Uri Uri {
            get {
                return _uri;
            }
        }

        public override StreamContext ChangePath(string relativePath) {
            if (relativePath == null) {
                throw new ArgumentNullException(nameof(relativePath));
            }

            UriBuilder baseUri = new UriBuilder(new Uri(_uri, relativePath));
            return StreamContext.FromSource(baseUri.Uri);
        }

        public override Stream Open() {
            using (var client = new HttpClient()) {
                return client.GetStreamAsync(_uri).Result;
            }
        }
    }
}
