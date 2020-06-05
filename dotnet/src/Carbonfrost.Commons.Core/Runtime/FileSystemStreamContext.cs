//
// Copyright 2005, 2015, 2016 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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

    class FileSystemStreamContext : StreamContext {

        private readonly Uri _uri;
        private readonly ContentType _contentType;

        public FileSystemStreamContext(Uri uri, ContentType contentType) {
            _uri = uri;
            _contentType = contentType;
        }

        public override ContentType ContentType {
            get {
                return _contentType ?? base.ContentType;
            }
        }
        public override Uri Uri {
            get {
                return _uri;
            }
        }

        public override StreamWriter AppendText(Encoding encoding) {
            Stream stream = Open(FileMode.Append, FileAccess.Write);
            if (encoding == null) {
                return new StreamWriter(stream);
            }
            return new StreamWriter(stream, encoding);
        }

        public override StreamContext ChangeContentType(ContentType contentType) {
            if (contentType == _contentType) {
                return this;
            }
            return new FileSystemStreamContext(Uri, contentType);
        }

        public override StreamContext ChangePath(string relativePath) {
            var newUri = new Uri(Uri, relativePath);
            return new FileSystemStreamContext(newUri, ContentType);
        }

        public override Stream OpenRead() {
            return Open(FileMode.Open, FileAccess.Read);
        }

        public override Stream Open() {
            return Open(FileMode.OpenOrCreate, FileAccess.ReadWrite);
        }

        public Stream Open(FileMode mode, FileAccess access) {
            string realPath = Uri.IsAbsoluteUri ? Uri.LocalPath : Uri.ToString();
            return new FileStream(realPath, mode, access);
        }
    }
}
