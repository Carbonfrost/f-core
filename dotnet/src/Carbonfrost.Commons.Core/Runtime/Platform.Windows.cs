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

namespace Carbonfrost.Commons.Core.Runtime {

    partial class Platform {

        internal class WindowsVersionFinder : IPlatformVersionFinder {

            IEnumerable<PlatformVersion> IPlatformVersionFinder.FindPlatformVersions() {
                string windowsVersion = SafelyFindFileVersion(@"%SystemRoot%\system32\winver.exe");
                if (windowsVersion != null) {
                    var result = new PlatformVersion("Windows", windowsVersion, "Windows");

                    ApplyWindowsAttributes(result);
                    yield return result;
                }
            }

            void ApplyWindowsAttributes(PlatformVersion result) {
                result.Attributes.Add("\"" + PlainWindowsSku + "\"");

                if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ProgramW6432"))) {
                    result.Attributes.Add("WOW64");
                }
            }

            static string PlainWindowsSku {
                get {
                    var osVer = Environment.OSVersion.Version;

                    int type = 0;
                    if (NativeMethods.GetProductInfo(osVer.Major, osVer.Minor, osVer.MajorRevision, osVer.MinorRevision, ref type)) {

                    }

                    // https://msdn.microsoft.com/en-us/library/windows/desktop/ms724358(v=vs.85).aspx
                    var ver = osVer.Major + "." + osVer.Minor;
                    switch (ver) {
                            // TODO These could be server editions
                        case "6.0":
                            return "Windows Vista";
                            // return "Windows Server 2008";
                        case "6.1":
                            return "Windows 7";
                            // return "Windows Server 2008 R2";
                        case "6.2":
                            if (type >= 0x62) {
                                return "Windows 10";
                                // return "Windows Server 2016";
                            }
                            return "Windows 8";
                    }

                    return "Windows";
                }
            }

        }
    }
}
