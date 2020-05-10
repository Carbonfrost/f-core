//
// Copyright 2005, 2006, 2010, 2016, 2020 Carbonfrost Systems, Inc.
// (https://carbonfrost.com)
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
using System.IO;
using System.Text;

namespace Carbonfrost.Commons.Core.Runtime {

    internal sealed class StreamStreamContext : StreamContext {

        private static readonly Uri STREAM_URI = new Uri("stream://");
        private readonly Stream _stream;
        private readonly Encoding _encoding;
        private readonly Uri _uri;

        public StreamStreamContext(Stream stream, Encoding encoding)
            : this(UriFromStream(stream), stream, encoding) {
        }

        public StreamStreamContext(Uri uri, Stream stream, Encoding encoding) {
            if (stream == null) {
                throw new ArgumentNullException(nameof(stream));
            }

            _uri = uri;
            _stream = stream;
            _encoding = encoding;
        }

        public override Uri Uri {
            get {
                return _uri;
            }
        }

        public override StreamContext ChangePath(string relativePath) {
            if (Uri == STREAM_URI) {
                throw new NotSupportedException();
            }
            return base.ChangePath(relativePath);
        }

        public override Stream Open() {
            return new ReadOnlyStream(_stream);
        }

        private static Uri UriFromStream(Stream s) {
            if (s == null) {
                return null;
            }
            if (s is FileStream fileStream) {
                return new Uri(fileStream.Name);
            }
            if (s == Stream.Null) {
                return new Uri("null://");
            }
            return STREAM_URI;
        }
    }
}
