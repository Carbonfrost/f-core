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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Carbonfrost.Commons.Core.Runtime {

    class DefaultAssemblyProbe : AssemblyProbe, IEnumerable<Assembly> {

        private readonly AssemblyProbe[] _probes = {
            new ProbeAppDomain(),
            new ProbeForAssembliesInAppDirectory(),
            new ProbeForReferences(),
            new ProbeForRelatedAssemblies(),
        };

        public override IEnumerable<Assembly> EnumerateAssemblies() {
            return this;
        }

        public IEnumerator<Assembly> GetEnumerator() {
            return new Enumerator(_probes);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        class Enumerator : IEnumerator<Assembly> {

            private readonly List<Assembly> _buffer = new List<Assembly>();
            private readonly HashSet<Assembly> _bufferUnique = new HashSet<Assembly>(
                new AssemblyNameComparer()
            );
            private readonly Queue<AssemblyReference> _pending = new Queue<AssemblyReference>();
            private readonly AssemblyProbe[] _probes;
            private readonly Queue<Action> _actions = new Queue<Action>();
            private int _index;

            public Assembly Current {
                get {
                    if (_index < 0) {
                        throw Failure.OutsideEnumeration();
                    }
                    if (_index < _buffer.Count) {
                        return _buffer[_index];
                    }

                    throw Failure.OutsideEnumeration();
                }
            }

            object IEnumerator.Current {
                get {
                    return Current;
                }
            }

            public Enumerator(AssemblyProbe[] probes) {
                Reset();

                // Allow probes to enumerate assemblies that are already loaded,
                // then allow them to try assemblies that will need loading.
                _probes = probes;
                EnqueueActions(_probes.Select(ExecuteProbe));
                EnqueueActions(_probes.Select(ExecuteProbeDeferred));

                // Handle assemblies loaded elsewhere
                AppDomain.CurrentDomain.AssemblyLoad += (_, args) => {
                    if (_bufferUnique.Contains(args.LoadedAssembly)) {
                        return;
                    }
                    _pending.Enqueue(args.LoadedAssembly);
                };
            }

            private Action ExecuteProbe(AssemblyProbe probe) {
                return () => AddRange(probe.EnumerateAssemblies().Select(AssemblyReference.CreateFromAssembly));
            }

            private Action ExecuteProbeDeferred(AssemblyProbe probe) {
                return () => AddRange(probe.EnumerateDeferredAssemblies());
            }

            private Action ExecuteProbeAgain(AssemblyProbe probe, IEnumerable<Assembly> asms) {
                return () => AddRange(probe.EnumerateAgain(asms));
            }

            private Action Breakpoint() {
                int startIndex  = _buffer.Count;
                return () => {
                    Assembly[] assembliesLoadedSince = _buffer.Skip(startIndex).ToArray();
                    if (assembliesLoadedSince.Any()) {
                        EnqueueActions(
                            _probes.Select(p => ExecuteProbeAgain(p, assembliesLoadedSince))
                        );
                    }
                };
            }

            private void AddRange(IEnumerable<AssemblyReference> enumerable) {
                // When something is added, we need to be able to retry all probes
                // to see if there is something more.
                if (enumerable.Any()) {
                    EnqueueAction(Breakpoint());
                }

                foreach (var o in enumerable) {
                    _pending.Enqueue(o);
                }
            }

            public bool MoveNext() {
                _index++;
                if (_index < _buffer.Count) {
                    return true;
                }
                while (_pending.Count > 0 || TryActions()) {
                    if (ProcessPending()) {
                        return true;
                    }
                }

                return false;
            }

            public void Reset() {
                _index = -1;
            }

            public void Dispose() {}

            private bool ProcessPending() {
                if (_pending.Count == 0) {
                    return false;
                }

                var addOne = _pending.Dequeue().TryLoad();
                if (ReferenceEquals(addOne, null)) {
                    return false;
                }
                if (!_bufferUnique.Add(addOne)) {
                    return false;
                }

                _buffer.Add(addOne);
                return true;
            }

            private bool TryActions() {
                while (_actions.Count > 0 && _pending.Count == 0) {
                    var act = _actions.Dequeue();
                    act();
                }

                return _pending.Count > 0;
            }

            private void EnqueueActions(IEnumerable<Action> results) {
                foreach (var a in results) {
                    EnqueueAction(a);
                }
            }

            private void EnqueueAction(Action a) {
                _actions.Enqueue(a);
            }
        }
    }
}
