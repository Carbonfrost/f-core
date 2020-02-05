//
// Copyright 2005, 2006, 2010, 2016, 2019-2020 Carbonfrost Systems, Inc.
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

    partial class StreamContext {

        public static readonly StreamContext Null = new NullStreamContext();
        public static readonly StreamContext Invalid = new InvalidStreamContext();

        public static StreamContext FromByteArray(byte[] data) {
            if (data == null)
                throw new ArgumentNullException("data");

            var ms = new MemoryStream(data, false);
            return new DataStreamContext(ms, ContentType.Parse(ContentType.Binary));
        }

        public static StreamContext FromText(string text, Encoding encoding) {
            if (string.IsNullOrEmpty(text))
                return StreamContext.Null;

            encoding = (encoding ?? Encoding.UTF8);
            if (encoding == Encoding.UTF8) {
                return new DataStreamContext(text);
            }
            MemoryStream ms = new MemoryStream(encoding.GetBytes(text));

            var param = new Dictionary<string, string> {
                { "charset", encoding.WebName }
            };

            return new DataStreamContext(ms, new ContentType("text", "plain", param));
        }

        public static StreamContext FromText(string text) {
            return FromText(text, Encoding.UTF8);
        }

        public static StreamContext FromFile(string fileName) {
            if (string.IsNullOrEmpty(fileName)) {
                throw Failure.NullOrEmptyString(nameof(fileName));
            }
            var uri = new Uri(fileName, UriKind.RelativeOrAbsolute);
            if (Path.IsPathRooted(fileName)) {
                uri = new Uri("file://" + fileName);
            }
            return new FileSystemStreamContext(uri, null);
        }

        public static StreamContext FromStream(Stream stream) {
            if (stream == null) { throw new ArgumentNullException("stream"); }
            return new StreamStreamContext(stream, null);
        }

        public static StreamContext FromStream(Stream stream, Encoding encoding) {
            if (stream == null)
                throw new ArgumentNullException("stream");
            return new StreamStreamContext(stream, encoding);
        }

        public static StreamContext FromSource(Uri source) {
            if (source == null) {
                throw new ArgumentNullException("source");
            }

            // Look for native providers (file, res, iso, mem, stream)
            if (source.IsAbsoluteUri) {
                switch (source.Scheme) {
                    case "file":
                        return new FileSystemStreamContext(source, null);

                    case "data":
                        return new DataStreamContext(source);

                    case "invalid":
                        return StreamContext.Invalid;

                    case "null":
                        return StreamContext.Null;

                    case "stdout":
                        // TODO Consider implementing an alternative using Console stream
                        return new StreamStreamContext(new Uri("stdout://"),
                                                       Console.OpenStandardOutput(),
                                                       Console.OutputEncoding);

                    case "stderr":
                        return new StreamStreamContext(new Uri("stderr://"),
                                                       Console.OpenStandardError(),
                                                       Console.OutputEncoding);

                    case "stdin":
                        return new StreamStreamContext(new Uri("stdin://"),
                                                       Console.OpenStandardInput(),
                                                       Console.InputEncoding);

                    case "stream":
                        throw RuntimeFailure.ForbiddenStreamStreamContext();

                    default:
                        // Fall back to the URI
                        return new UriStreamContext(source);
                }
            } else {
                // Relative URIs must be handled in this way
                return FromFile(source.ToString());
            }
        }
    }

}
