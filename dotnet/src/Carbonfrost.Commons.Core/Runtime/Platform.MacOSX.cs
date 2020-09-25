//
// Copyright 2016, 2019 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using System.Text.RegularExpressions;

namespace Carbonfrost.Commons.Core.Runtime {

    partial class Platform {

        internal class MacOSXSwversVersionFinder : IPlatformVersionFinder {

            const string SWVERS_LOCATION = "/usr/bin/sw_vers";

            IEnumerable<PlatformVersion> IPlatformVersionFinder.FindPlatformVersions() {
                if (File.Exists(SWVERS_LOCATION)) {
                    var lines = Platform.RunCommand(SWVERS_LOCATION, "", true);
                    return ParseResult(lines);
                }
                return Array.Empty<PlatformVersion>();
            }

            internal IEnumerable<PlatformVersion> ParseResult(IEnumerable<string> lines) {
                if (lines == null) {
                    return Array.Empty<PlatformVersion>();
                }
                var result = SwVersResult.Parse(lines);
                if (result == null) {
                    return Array.Empty<PlatformVersion>();
                }
                if (result.ProductName == "macOS") {
                    return ParseResultmacOS(result);
                }
                return ParseResultMacOSX(result);
            }

            private IEnumerable<PlatformVersion> ParseResultMacOSX(SwVersResult result) {
                yield return new PlatformVersion(result.ProductName.Replace(" ", ""), result.ProductVersion, "MacOSX") {
                    Attributes = {
                        BuildVersion(result),
                        FriendlyName(result),
                    }
                };

                // Easing branding into macOS
                if (string.Compare(result.ProductVersion, "10.12") > 0) {
                    yield return new PlatformVersion("macOS", result.ProductVersion, "MacOSX");
                }
            }

            private IEnumerable<PlatformVersion> ParseResultmacOS(SwVersResult result) {
                yield return new PlatformVersion(result.ProductName.Replace(" ", ""), result.ProductVersion, "MacOSX") {
                    Attributes = {
                        BuildVersion(result),
                        FriendlyName(result),
                        "like MacOSX"
                    }
                };
            }

            static string BuildVersion(SwVersResult result) {
                return string.Format("Build {0}", result.BuildVersion);
            }

            static string FriendlyName(SwVersResult result) {
                return "\"" + _FriendlyName(result.ProductVersion) + "\"";
            }

            static string _FriendlyName(string productVersion) {
                var match = Regex.Match(productVersion, @"^\d+\.\d+");
                if (!match.Success) {
                    return "Mac OS X " + productVersion;
                }
                switch (match.Value) {
                    case "11.0":
                        return "macOS Big Sur";
                    case "10.15":
                        return "macOS Catalina";
                    case "10.14":
                        return "macOS Mojave";
                    case "10.13":
                        return "macOS High Sierra";
                    case "10.12":
                        return "macOS Sierra";
                    case "10.11":
                        return "Mac OS X El Capitan";
                    case "10.10":
                        return "Mac OS X Yosemite";
                    case "10.9":
                        return "Mac OS X Mavericks";
                    default:
                        return "Mac OS X " + productVersion;
                }
            }

            class SwVersResult {

                public string ProductName, ProductVersion, BuildVersion;

                public static SwVersResult Parse(IEnumerable<string> lines) {
                    var result = new SwVersResult();
                    foreach (var line in lines) {
                        string[] parts = (line ?? string.Empty).Split(new [] { ':' }, 2);
                        switch (parts[0]) {
                            case "ProductName":
                                result.ProductName = parts[1].Trim();
                                break;
                            case "ProductVersion":
                                result.ProductVersion = parts[1].Trim();
                                break;
                            case "BuildVersion":
                                result.BuildVersion = parts[1].Trim();
                                break;
                        }
                    }
                    if (string.IsNullOrEmpty(result.ProductName)
                        || string.IsNullOrEmpty(result.ProductVersion)
                        || string.IsNullOrEmpty(result.BuildVersion)) {
                        return null;
                    }
                    return result;
                }
            }
        }
    }
}
