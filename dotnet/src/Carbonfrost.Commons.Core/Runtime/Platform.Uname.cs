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
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Carbonfrost.Commons.Core.Runtime {

    partial class Platform {

        internal class UNameVersionFinder : IPlatformVersionFinder {

            IEnumerable<PlatformVersion> IPlatformVersionFinder.FindPlatformVersions() {
                string version;
                string fullVersion;
                string name = GetPosixSystemName(out version, out fullVersion);
                if (name != null) {
                    yield return new PlatformVersion(name, version, name) {
                        Attributes = {
                            fullVersion,
                            "like POSIX",
                            "like Unix",
                        }
                    };
                }
            }

            static string GetPosixSystemName(out string version, out string fullVersion) {
                version = string.Empty;
                fullVersion = string.Empty;
                var utsname = IntPtr.Zero;
                try {
                    utsname = Marshal.AllocHGlobal(8192);
                    if (NativeMethods.uname(utsname) == 0) {
                        // TODO May not be fully xplat
                        fullVersion = Marshal.PtrToStringAnsi(utsname + 256 * 3); // "utsname.version"

                        // Delete portion before ';' because it is verbose
                        fullVersion = Regex.Replace(fullVersion ?? string.Empty, @"^.+;\s+", "");
                        version = Marshal.PtrToStringAnsi(utsname + 256 * 2); // "utsname.release"
                        return Marshal.PtrToStringAnsi(utsname);
                    }
                } catch (DllNotFoundException) {
                } finally {
                    if (utsname != IntPtr.Zero) {
                        Marshal.FreeHGlobal(utsname);
                    }
                }
                return null;
            }
        }
    }
}
