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


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Carbonfrost.Commons.Core.Runtime {

    public sealed partial class Platform {

        private static readonly Platform _current = new Platform();
        private static readonly bool _isMono = Type.GetType("Mono.Runtime") != null;
        private IList<PlatformVersion> _versions;

        private static readonly IPlatformVersionFinder[] _finders = typeof(Platform)
            .GetTypeInfo()
            .GetNestedTypes(BindingFlags.NonPublic)
            .Where(t => t != typeof(IPlatformVersionFinder) && typeof(IPlatformVersionFinder).GetTypeInfo().IsAssignableFrom(t))
            .Select(t => (IPlatformVersionFinder) Activator.CreateInstance(t))
            .ToArray();

        public static Platform Current {
            get {
                return _current;
            }
        }

        public bool IsMono {
            get {
                return _isMono;
            }
        }

        private IList<PlatformVersion> Versions {
            get {
                return _versions ?? (_versions = _finders.SelectMany(SafelyFind).ToList());
            }
        }

        public string PlatformFamily {
            get {
                return Versions.Select(t => t.PlatformFamily).First(t => t != null);
            }
        }

        public string UserAgent {
            get {
                return string.Join(" ", Versions);
            }
        }

        public override string ToString() {
            return UserAgent;
        }

        static IEnumerable<PlatformVersion> SafelyFind(IPlatformVersionFinder finder) {
            try {
                return finder.FindPlatformVersions().ToList();
            } catch {
                return Enumerable.Empty<PlatformVersion>();
            }
        }

        static string SafelyFindFileVersion(string filePath) {
            try {
                string path = Environment.ExpandEnvironmentVariables(Regex.Replace(filePath, "^file:///", ""));

                var fvi = FileVersionInfo.GetVersionInfo(path);
                if (fvi.FileVersion != null) {
                    return fvi.FileVersion.Split(' ').FirstOrDefault();
                }
            } catch {
            }
            return null;
        }

        static List<string> RunCommand(string cmd, string arguments, bool ignoreExit = false) {
            Process process = new Process();
            ProcessStartInfo info = new ProcessStartInfo(cmd, arguments);
            info.CreateNoWindow = true;
            info.RedirectStandardError = true;
            info.RedirectStandardInput = true;
            info.RedirectStandardOutput = true;
            info.UseShellExecute = false;

            process.StartInfo = info;
            process.EnableRaisingEvents = true;

            process.Start();

            List<string> lines = new List<string>();
            process.OutputDataReceived += (o, e) => {
                lines.Add(e.Data);
            };

            process.BeginOutputReadLine();
            process.WaitForExit();

            if (!ignoreExit && process.ExitCode != 0) {
                return null;
            }

            process.Dispose();
            return lines;
        }

        internal class PlatformVersion {
            public readonly string Name;
            public readonly string Version;
            public readonly IList<string> Attributes = new List<string>();
            public readonly string PlatformFamily;

            public PlatformVersion(string name, string version, string platformFamily) {
                Name = name;
                Version = version;
                PlatformFamily = platformFamily;
            }

            public override string ToString() {
                string result = string.Format("{0}/{1}", Name, Version);
                if (Attributes.Count > 0) {
                    result += string.Format(" ({0})", string.Join("; ", Attributes));
                }
                return result;
            }
        }

        internal interface IPlatformVersionFinder {
            IEnumerable<PlatformVersion> FindPlatformVersions();
        }
    }

}
