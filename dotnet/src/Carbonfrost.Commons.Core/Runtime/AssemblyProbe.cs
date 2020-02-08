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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Carbonfrost.Commons.Core.Runtime {

    abstract class AssemblyProbe {

        public static AssemblyProbe Create() {
            return new DefaultAssemblyProbe();
        }

        public virtual IEnumerable<Assembly> EnumerateAssemblies() {
            return Enumerable.Empty<Assembly>();
        }

        public virtual IEnumerable<AssemblyReference> EnumerateDeferredAssemblies() {
            return Enumerable.Empty<AssemblyReference>();
        }

        public virtual IEnumerable<AssemblyReference> EnumerateAgain(IEnumerable<Assembly> asm) {
            return Enumerable.Empty<AssemblyReference>();
        }

        internal class ProbeAppDomain : AssemblyProbe {

            public override IEnumerable<Assembly> EnumerateAssemblies() {
                return App.LoadedAssemblies;
            }

        }

        internal class ProbeForReferences : AssemblyProbe {

            private readonly HashSet<AssemblyName> _previousNames
                = new HashSet<AssemblyName>(new AssemblyNameComparer());

            public override IEnumerable<AssemblyReference> EnumerateAgain(IEnumerable<Assembly> asm) {
                foreach (AssemblyName name in asm.SelectMany(a => a.GetReferencedAssemblies())) {
                    if (!_previousNames.Add(name)) {
                        continue;
                    }

                    yield return AssemblyReference.CreateFromName(name);
                }
            }
        }

        internal class ProbeForAssembliesInAppDirectory : AssemblyProbe {

            // TODO Improve by looking at binding manifest

            public override IEnumerable<AssemblyReference> EnumerateDeferredAssemblies() {
                var assemblies = Directory.EnumerateFiles(App.BasePath, "*.dll");
                return assemblies.Select(AssemblyReference.CreateFromFile);
            }
        }

        internal class ProbeForRelatedAssemblies : AssemblyProbe {

            public override IEnumerable<AssemblyReference> EnumerateAgain(IEnumerable<Assembly> asm) {
                return asm.SelectMany(a => a.GetRelatedAssemblyReferences());
            }
        }
    }
}
