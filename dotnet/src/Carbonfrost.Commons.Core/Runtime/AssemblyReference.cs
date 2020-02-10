//
// Copyright 2020 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using System.Reflection;
using System.Security;

namespace Carbonfrost.Commons.Core.Runtime {

    public abstract class AssemblyReference {

        public static AssemblyReference CreateFromName(AssemblyName name) {
            if (name == null) {
                throw new ArgumentNullException(nameof(name));
            }
            return new NameAssemblyReference(name);
        }

        public static AssemblyReference CreateFromFile(string fileName) {
            if (string.IsNullOrEmpty(fileName)) {
                throw Failure.NullOrEmptyString(nameof(fileName));
            }
            return new FileAssemblyReference(fileName);
        }

        public static AssemblyReference CreateFromAssembly(Assembly assembly) {
            if (assembly == null) {
                throw new ArgumentNullException(nameof(assembly));
            }
            return new ValueAssemblyReference(assembly);
        }

        public static implicit operator AssemblyReference(Assembly assembly) {
            return CreateFromAssembly(assembly);
        }

        public abstract Assembly Load();

        public Assembly TryLoad() {
            Exception error = null;

            try {
                return Load();

            } catch (FileNotFoundException ex) {
                error = ex;
            } catch (FileLoadException ex) {
                error = ex;
            } catch (SecurityException ex) {
                error = ex;
            } catch (BadImageFormatException ex) {
                error = ex;
            }

            // TODO Trace errors
            return null;
        }

        private class FileAssemblyReference : AssemblyReference {
            private readonly string _file;

            public FileAssemblyReference(string file) {
                _file = file;
            }

            public override Assembly Load() {
                return Assembly.LoadFile(_file);
            }

            // TODO Consider error handling and tracing assembly names
        }

        private class NameAssemblyReference : AssemblyReference {
            private readonly AssemblyName _name;

            public NameAssemblyReference(AssemblyName name) {
                _name = name;
            }

            public override Assembly Load() {
                return Assembly.Load(NoCodeBase(_name));
            }

            private static AssemblyName NoCodeBase(AssemblyName name) {
                name.CodeBase = null;
                return name;
            }
        }

        private class ValueAssemblyReference : AssemblyReference {
            private readonly Assembly _value;

            public ValueAssemblyReference(Assembly value) {
                _value = value;
            }

            public override Assembly Load() {
                return _value;
            }
        }
    }
}
