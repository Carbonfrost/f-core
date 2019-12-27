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
using System.Linq;
using System.Reflection;

namespace Carbonfrost.Commons.Core.Runtime {

    partial class Platform {

        internal class NetFrameworkRuntimeVersionFinder : IPlatformVersionFinder {

            public IEnumerable<PlatformVersion> FindPlatformVersions() {
                string version = GetType().GetTypeInfo().Assembly.ImageRuntimeVersion;
                version = version.TrimStart('v');
                string innerVersion = FindFileVersion();
                if (innerVersion == null) {
                    return new [] { new PlatformVersion(".NET", version, null) };
                }

                var result = new PlatformVersion(".NET", innerVersion, null);
                result.Attributes.Add("like .NET/" + version);
                return new [] { result };
            }

            static string FindFileVersion() {
                var assembly = typeof(object).GetTypeInfo().Assembly;
                string path = SafelyFindFileVersion(assembly.CodeBase);
                if (path != null) {
                    return path;
                }

                foreach (AssemblyFileVersionAttribute attr in assembly.GetCustomAttributes(typeof(AssemblyFileVersionAttribute))) {
                    return attr.Version;
                }

                return null;
            }
        }
    }
}
