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
using System.IO;
using System.Linq;

namespace Carbonfrost.Commons.Core {

    public partial class Glob {

        protected internal class GlobController {

            public virtual string WorkingDirectory {
                get {
                    return Directory.GetCurrentDirectory();
                }
            }

            public virtual string RootDirectory {
                get {
                    return "/";
                }
            }

            public virtual bool DirectoryExists(string path) {
                return Directory.Exists(path);
            }

            public virtual bool FileExists(string path) {
                return File.Exists(path);
            }

            public virtual IEnumerable<string> EnumerateFiles(string path) {
                return Directory.EnumerateFiles(path);
            }

            public virtual IEnumerable<string> EnumerateDirectories(string path) {
                return Directory.EnumerateDirectories(path);
            }

            public virtual IEnumerable<string> EnumerateFileSystemEntries(string path) {
                return Directory.EnumerateFileSystemEntries(path);
            }

            public virtual string ExpandEnvironmentVariables(string path) {
                if (!string.IsNullOrEmpty(path)) {
                    path = Environment.ExpandEnvironmentVariables(path);
                }
                // Prefer the platform-agnostic form
                return (path ?? string.Empty)
                    .Replace(Path.AltDirectorySeparatorChar, '/')
                    .Replace(Path.DirectorySeparatorChar, '/');
            }

            internal IEnumerable<string> OnlyDirectories(IEnumerable<string> items) {
                return items.Where(DirectoryExists);
            }

        }
    }
}
