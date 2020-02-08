//
// Copyright 2020 Carbonfrost Systems, Inc. (https://carbonfrost.com)
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Carbonfrost.Commons.Core.Runtime {

    partial class Adaptable {

        public static IReadOnlyList<Assembly> GetRelatedAssemblies(this Assembly assembly) {
            return GetRelatedAssemblies(assembly, true);
        }

        public static IReadOnlyList<Assembly> GetRelatedAssemblies(this Assembly assembly, bool throwOnError = false) {
            var result = GetRelatedAssemblyReferences(assembly);
            if (throwOnError) {
                return result.Select(a => a.Load()).ToList();
            }

            return result.Select(a => a.TryLoad()).WhereNotNull().ToList();
        }

        public static IReadOnlyList<AssemblyReference> GetRelatedAssemblyReferences(this Assembly assembly) {
            return GetRelatedAssemblyReferences(assembly, File.Exists, Assembly.LoadFile);
        }

        internal static IReadOnlyList<AssemblyReference> GetRelatedAssemblyReferences(
            Assembly assembly,
            Func<string, bool> fileExists,
            Func<string, Assembly> loadFile
        ) {
            if (assembly == null) {
                throw new ArgumentNullException(nameof(assembly));
            }

            if (assembly.IsDynamic || assembly.ReflectionOnly || string.IsNullOrEmpty(assembly.CodeBase)) {
                return Array.Empty<AssemblyReference>();
            }

            var attributes = assembly.GetCustomAttributes<RelatedAssemblyAttribute>().ToArray();
            if (attributes.Length == 0) {
                return Array.Empty<AssemblyReference>();
            }

            var assemblyName = assembly.GetName().Name;
            var assemblyLocation = GetAssemblyLocation(assembly);
            var assemblyDirectory = Path.GetDirectoryName(assemblyLocation);

            var relatedAssemblies = new List<AssemblyReference>(attributes.Length);
            foreach (var attribute in attributes) {
                if (string.Equals(assemblyName, attribute.AssemblyFileName, StringComparison.OrdinalIgnoreCase)) {
                    throw RuntimeFailure.InvalidSelfRelatedAssembly(assemblyName);
                }

                relatedAssemblies.Add(new RelatedAssemblyReference(
                    assembly.GetName(),
                    assemblyDirectory,
                    attribute.AssemblyFileName,
                    fileExists,
                    loadFile
                ));
            }

            return relatedAssemblies;
        }

        internal class RelatedAssemblyReference : AssemblyReference {
            private readonly Func<string, bool> _fileExists;
            private readonly Func<string, Assembly> _loadFile;
            private readonly string _name;
            private readonly string _assemblyDirectory;
            private readonly AssemblyName _requestor;

            private string RelatedAssemblyLocation {
                get {
                    return Path.Combine(_assemblyDirectory, _name + ".dll");
                }
            }

            public RelatedAssemblyReference(
                AssemblyName requestor,
                string assemblyDirectory,
                string name,
                Func<string, bool> fileExists,
                Func<string, Assembly> loadFile
            ) {
                _assemblyDirectory = assemblyDirectory;
                _name = name;
                _fileExists = fileExists;
                _loadFile = loadFile;
                _requestor = requestor;
            }

            public override Assembly Load() {
                if (!_fileExists(RelatedAssemblyLocation)) {
                    throw RuntimeFailure.UnableToLoadRelatedAssembly(_name, _requestor, new [] { _assemblyDirectory }, RelatedAssemblyLocation);
                }

                return _loadFile(RelatedAssemblyLocation);
            }
        }

        internal static string GetAssemblyLocation(Assembly assembly) {
            Uri result;
            if (Uri.TryCreate(assembly.CodeBase, UriKind.Absolute, out result) &&
                result.IsFile && string.IsNullOrWhiteSpace(result.Fragment)) {

                return result.LocalPath;
            }
            return assembly.Location;
        }
    }
}
