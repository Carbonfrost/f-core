//
// Copyright 2019 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using System.Text.RegularExpressions;

namespace Carbonfrost.Commons.Core.Runtime {

    public abstract partial class StreamContext : IUriContext {

        public virtual ContentType ContentType {
            get {
                return ContentType.Parse(ContentType.Binary);
            }
        }

        protected Uri BaseUri { get; set; }

        public abstract Uri Uri { get; }

        public string Extension {
            get {
                string file;
                if (Uri.IsAbsoluteUri)
                    file = Uri.AbsolutePath;
                else
                    file = Uri.ToString();

                Match m = Regex.Match(file, @"\.\w+$");
                return m.Success ? m.Groups[0].Value : string.Empty;
            }
        }

        protected StreamContext() {}

        public StreamWriter AppendText() {
            return AppendText(null);
        }

        public virtual StreamWriter AppendText(Encoding encoding) {
            if (encoding == null) {
                return new StreamWriter(Open());
            }
            return new StreamWriter(Open(), encoding);
        }

        public abstract StreamContext ChangePath(string relativePath);

        public virtual StreamContext ChangeContentType(ContentType contentType) {
            throw new NotSupportedException();
        }

        public StreamContext ChangeExtension(string extension) {
            string localName = Path.GetFileName(Uri.LocalPath);
            int index = localName.LastIndexOf('.'); // $NON-NLS-1

            // Replace the local name with the extension
            index = (index < 0) ? localName.Length - 1 : index;
            string targetFile
                = localName.Substring(0, index) + extension; // $NON-NLS-1

            return ChangePath("./" + targetFile); // $NON-NLS-1
        }

        public StreamWriter CreateText() {
            return CreateText(null);
        }

        public virtual StreamWriter CreateText(Encoding encoding) {
            if (encoding == null) {
                return new StreamWriter(Open());
            }
            return new StreamWriter(Open(), encoding);
        }

        public StreamReader OpenText() {
            return OpenText(null);
        }

        public StreamReader OpenText(Encoding encoding) {
            if (encoding == null) {
                return new StreamReader(OpenRead());
            }
            return new StreamReader(OpenRead(), encoding);
        }

        public abstract Stream Open();

        public virtual Stream OpenRead() {
            return new ReadOnlyStream(Open());
        }

        public virtual Stream OpenWrite() {
            return Open();
        }

        public object ReadValue(Type componentType) {
            return Activation.FromStreamContext(componentType, this);
        }

        public T ReadValue<T>() {
            return (T) ReadValue(typeof(T));
        }

        public byte[] ReadAllBytes() {
            using (Stream s = OpenRead()) {
                return BufferedCopyToBytes(s);
            }
        }

        public string[] ReadAllLines() {
            return ReadAllLines(null);
        }

        public IEnumerable<string> ReadLines() {
            return ReadLines(null);
        }

        public IEnumerable<string> ReadLines(Encoding encoding) {
            using (StreamReader reader = OpenText(encoding)) {
                string s;
                while ((s = reader.ReadLine()) != null) {
                    yield return s;
                }
            }
        }

        public string[] ReadAllLines(Encoding encoding) {
            List<string> result = new List<string>();
            result.AddRange(ReadLines(encoding));
            return result.ToArray();
        }

        public string ReadAllText(Encoding encoding) {
            using (StreamReader sr = OpenText(encoding)) {
                return sr.ReadToEnd();
            }
        }

        public string ReadAllText() {
            return ReadAllText(null);
        }

        public void WriteValue(Type componentType, object value) {
            if (componentType == null) {
                throw new ArgumentNullException("componentType");
            }
            if (value == null) {
                throw new ArgumentNullException("value");
            }

            StreamingSource ss = StreamingSource.Create(
                componentType, ContentType, null, ServiceProvider.Null
            );
            ss.Save(this, value);
        }

        public void WriteValue<T>(T value) {
            WriteValue(typeof(T), value);
        }

        public void WriteAllBytes(byte[] value) {
            if (value == null) {
                throw new ArgumentNullException("value");
            }

            using (Stream s = OpenWrite()) {
                new MemoryStream(value).CopyTo(s);
            }
        }

        public void WriteAllLines(IEnumerable<string> lines) {
            if (lines == null) {
                throw new ArgumentNullException("lines");
            }

            WriteAllLines(lines, null);
        }

        public void WriteAllLines(IEnumerable<string> lines, Encoding encoding) {
            if (lines == null) {
                throw new ArgumentNullException("lines");
            }

            using (TextWriter w = AppendText(encoding)) {
                foreach (string line in lines) {
                    w.WriteLine(line);
                }
            }
        }

        public void WriteAllText(string text, Encoding encoding) {
            using (StreamWriter sr = AppendText(encoding)) {
                sr.Write(text);
            }
        }

        public void WriteAllText(string text) {
            WriteAllText(text, null);
        }

        public override string ToString() {
            return this.Uri.ToString();
        }

        Uri IUriContext.BaseUri {
            get {
                return BaseUri;
            }
            set {
                BaseUri = value;
            }
        }

        private static byte[] BufferedCopyToBytes(Stream source) {
            const int BUFFER_CAPACITY = 2048;
            int capacity = BUFFER_CAPACITY;
            try {
                capacity = (int) source.Length;
            } catch (NotSupportedException) {
            }

            MemoryStream memory = new MemoryStream(capacity);
            source.CopyTo(memory, BUFFER_CAPACITY);
            return memory.ToArray();
        }
    }
}
