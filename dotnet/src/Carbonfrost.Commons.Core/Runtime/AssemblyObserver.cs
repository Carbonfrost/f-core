//
// Copyright 2015, 2019 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using System.Reflection;
using System.Security;

namespace Carbonfrost.Commons.Core.Runtime {

    class AssemblyObserver : IEnumerable<Assembly> {

        private readonly BufferEnumerator<Assembly> _enumerator;
        private bool _probed;
        private readonly HashSet<AssemblyName> _previousNames
            = new HashSet<AssemblyName>(new AssemblyNameComparer());

        public static readonly AssemblyObserver Instance
            = new AssemblyObserver();

        public AssemblyObserver() {
            // If we don't have or if we miss assembly load events, then our last chance
            // is GetAssemblies() and probes
            _enumerator = new BufferEnumerator<Assembly>(
                new AssemblyNameComparer(),
                ProbeForAssemblies
            );
            EnumerateAppDomain();
        }

        public IEnumerator<Assembly> GetEnumerator() {
            return _enumerator.Clone();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        private void DeferAssemblyReferences(Assembly assembly) {
            foreach (AssemblyName m in assembly.GetReferencedAssemblies()) {
                DeferAssembly(m);
            }
        }

        private Assembly[] ProbeForAssemblies() {
            var list = new List<Assembly>();
            list.AddRange(App.LoadedAssemblies);

            if (!_probed) {
                _probed = true;
                var probe = AssemblyProbe.CreateProbe();

                Traceables.ProbingForAssemblies(probe.GetType());
                foreach (var t in probe.EnumerateAssemblyFiles()) {

                    // TODO Consider error handling and tracing assembly names
                    try {
                        var asm = RuntimeUtility.GetAssemblyName(t);
                        DeferAssembly(asm);

                    } catch (FileNotFoundException) {
                    } catch (FileLoadException) {
                    } catch (SecurityException) {
                    } catch (BadImageFormatException) {
                    }
                }
            }
            return list.ToArray();
        }

        private void EnumerateAppDomain() {
            var domain = AppDomain.CurrentDomain;
            domain.AssemblyLoad += (_, args) => {
                DeferAssembly(args.LoadedAssembly.GetName());
                DeferAssemblyReferences(args.LoadedAssembly);
            };

            foreach (var assembly in App.LoadedAssemblies) {
                _enumerator.Add(assembly);
                DeferAssemblyReferences(assembly);
            }
        }

        private void DeferAssembly(AssemblyName name) {
            if (!_previousNames.Add(name)) {
                return;
            }

            Func<Assembly> load = () => {
                AssemblyName any = name;
                Exception error = null;

                Assembly assembly = null;
                try {
                    assembly = Assembly.Load(NoCodeBase(any));
                    DeferAssemblyReferences(assembly);

                } catch (FileNotFoundException ex) {
                    error = ex;
                } catch (FileLoadException ex) {
                    error = ex;
                } catch (BadImageFormatException ex) {
                    error = ex;
                }

                if (error != null) {
                    return null;

                }
                return assembly;
            };

            _enumerator.Add(load);
        }

        private static AssemblyName NoCodeBase(AssemblyName name) {
            name.CodeBase = null;
            return name;
        }
    }
}
