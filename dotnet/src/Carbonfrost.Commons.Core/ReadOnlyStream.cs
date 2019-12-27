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
using System.IO;
using Carbonfrost.Commons.Core.Runtime;

namespace Carbonfrost.Commons.Core {

    internal sealed class ReadOnlyStream : Stream {

        private bool _isDisposed;
        private readonly Stream _baseStream;

        private Stream BaseStream {
            get {
                ThrowIfDisposed();
                return _baseStream;
            }
        }

        public ReadOnlyStream(Stream baseStream) {
            _baseStream = baseStream;
        }

        private void ThrowIfDisposed() {
            if (_isDisposed) {
                throw Failure.Disposed(ToString());
            }
        }

        public override bool CanRead {
            get {
                return BaseStream.CanRead;
            }
        }

        public override bool CanWrite {
            get {
                ThrowIfDisposed();
                return false;
            }
        }

        public override bool CanSeek {
            get {
                return BaseStream.CanSeek;
            }
        }

        public override long Length {
            get {
                return BaseStream.Length;
            }
        }

        public override long Position {
            get {
                return BaseStream.Position;
            }
            set {
                BaseStream.Position = value;
            }
        }

        public override void SetLength(long value) {
            throw RuntimeFailure.CannotWriteToStream();
        }

        public override long Seek(long offset, SeekOrigin origin) {
            if (!BaseStream.CanSeek) {
                throw RuntimeFailure.SeekNotSupportedByBase();
            }
            if (!(origin == SeekOrigin.Begin || origin == SeekOrigin.End || origin == SeekOrigin.Current)) {
                throw Failure.NotDefinedEnum("origin", origin);
            }
            if (offset < 0 && origin == SeekOrigin.Begin) {
                throw RuntimeFailure.SeekNegativeBegin("offset", offset);
            }

            // Do the seek on the base
            return BaseStream.Seek(offset, origin);
        }

        public override void Flush() {
            BaseStream.Flush();
        }

        protected override void Dispose(bool disposing) {
            _isDisposed = true;
            if (disposing) {
                _baseStream.Flush();
                // Don't close base stream
            }
            base.Dispose(disposing);
        }

        public override int Read(byte[] buffer, int offset, int count) {
            return BaseStream.Read(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count) {
            if (CanWrite) {
                BaseStream.Write(buffer, offset, count);
            } else {
                throw RuntimeFailure.CannotWriteToStream();
            }
        }
    }
}
