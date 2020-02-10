//
// Copyright 2013, 2016, 2020 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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

using Carbonfrost.Commons.Core;
using Carbonfrost.Commons.Spec;

namespace Carbonfrost.UnitTests.Core {

    public class GlobTests {

        static Comparison<string> FileNameComparison = (x, y) =>
            string.Compare(x.Replace('\\', '/'), y.Replace('\\', '/'), StringComparison.Ordinal);

        [Fact]
        public void Glob_assumptions_for_fileobject() {
            // Sanity checks on the implementation of FileObject

            var e = Fixture1Enumerator();
            var f = Fixture1();
            string[] results = Glob.FilterDirectory(Glob.Anything,
                                                    "\\",
                                                    e.FileExists,
                                                    e).ToArray();
            Assume.Equal(new [] {
                            "/",
                            "/a",
                            "/a/b.txt",
                            "/a/c.csv",
                            "/a/d.txt",
                            "/a/e",
                            "/a/e/f",
                            "/a/e/f/g",
                            "/a/e/f/g/h.txt",
                            "/a/e/f/g/i.txt",
                            "/a/e/f/j",
                            "/a/e/.k",
                            "/a/e/l.csv",
                        }.Sorted(), f.Visit().Keys.Sorted().ToArray(), FileNameComparison);

            Assume.Equal(new [] {
                            "/a"
                        },
                        e.EnumerateDirectories("/").ToArray(), FileNameComparison);

            Assume.Equal(new [] {
                            "/a/e"
                        },
                        e.EnumerateDirectories("/a").ToArray(),
                        FileNameComparison);
            Assume.Equal(new [] {
                            "/a/b.txt",
                            "/a/c.csv",
                            "/a/d.txt"
                        }, e.EnumerateFiles("/a").ToArray(), FileNameComparison);
        }

        [Fact]
        public void FilterDirectory_enumerate_rooted_directory() {
            // When a root directory is used, it is checked directly
            var e = Fixture1Enumerator();

            string[] results = Glob.FilterDirectory(Glob.Parse("/a/e/.k"),
                                                    "/anywhere/s",
                                                    e.FileExists,
                                                    e).ToArray();

            Assert.Equal(new [] { "/a/e/.k", }, results, FileNameComparison);
        }

        [Fact]
        public void FilterDirectory_enumerate_literal_directory() {
            var e = Fixture1Enumerator();
            var glob = Glob.Parse("a/e/**/*.txt");

            string[] results = Glob.FilterDirectory(Glob.Parse("a/e/**/*.txt"),
                                                    "/",
                                                    e.FileExists,
                                                    e).ToArray();

            Assert.Equal(new [] {
                            "/a/e/f/g/h.txt",
                            "/a/e/f/g/i.txt",
                        }, results, FileNameComparison);
        }

        [Fact]
        public void FilterDirectory_enumerate_files_in_multiglob() {
            var e = Fixture1Enumerator();

            string[] results = Glob.FilterDirectory(Glob.Parse("**/*.csv;**/*.txt"),
                                                    "/",
                                                    e.FileExists,
                                                    e).ToArray();

            Assert.SetEqual(new [] {
                                "/a/b.txt",
                                "/a/c.csv",
                                "/a/d.txt",
                                "/a/e/f/g/h.txt",
                                "/a/e/f/g/i.txt",
                                "/a/e/l.csv",
                            }, results, FileNameComparison);
        }

        [Fact]
        public void FilterDirectory_enumerate_files_by_extension() {
            var e = Fixture1Enumerator();

            string[] results = Glob.FilterDirectory(Glob.Parse("**/*.csv"),
                                                    "/",
                                                    e.FileExists,
                                                    e).ToArray();

            Assert.Equal(new [] {
                            "/a/c.csv",
                            "/a/e/l.csv",
                        }, results, FileNameComparison);
        }

        [Fact]
        public void FilterDirectory_enumerate_every_file() {
            var e = Fixture1Enumerator();

            string[] results = Glob.FilterDirectory(Glob.Anything,
                                                    "/",
                                                    e.FileExists,
                                                    e).ToArray();

            Assert.SetEqual(new [] {
                                "/a/b.txt",
                                "/a/c.csv",
                                "/a/d.txt",
                                "/a/e/f/g/h.txt",
                                "/a/e/f/g/i.txt",
                                "/a/e/.k",
                                "/a/e/l.csv",
                            }, results, FileNameComparison);
        }

        [Fact]
        public void FilterDirectory_Regex_enumerate_top_level_files() {
            var e = Fixture1Enumerator();
            string[] results = Glob.FilterDirectory(Glob.Parse("*.*"),
                                                    "/a",
                                                    e.FileExists,
                                                    e).ToArray();

            Assert.True(Glob.Parse("*.*").ToRegex().IsMatch("/a/b.txt"));
            Assert.Equal(new [] {
                            "/a/b.txt",
                            "/a/c.csv",
                            "/a/d.txt",
                        }, results, FileNameComparison);
        }

        [Fact]
        public void FilterDirectory_enumerate_top_level_directories() {
            var e = Fixture1Enumerator();
            string[] results = Glob.FilterDirectory(Glob.Parse("*"),
                                                    "/",
                                                    e.DirectoryExists,
                                                    e).ToArray();

            Assert.Equal(new [] {
                            "/a",
                        }, results, FileNameComparison);
        }


        [Fact]
        public void FilterDirectory_enumerate_root_directory() {
            var e = Fixture1Enumerator();
            string[] results = Glob.FilterDirectory(Glob.Parse("/*"),
                                                    "/",
                                                    e.DirectoryExists,
                                                    e).ToArray();

            Assert.Equal(new [] {
                            "/a",
                        }, results, FileNameComparison);
        }

        [Fact]
        public void FilterDirectory_enumerate_root_from_arbitrary_other_directory() {
            var e = Fixture1Enumerator();
            string[] results = Glob.FilterDirectory(Glob.Parse("/*"),
                                                    "/a", // some directory
                                                    e.DirectoryExists,
                                                    e).ToArray();

            Assert.Equal(new [] {
                            "/a",
                        }, results, FileNameComparison);
        }

        [Fact]
        public void FilterDirectory_enumerate_root_directory_child_by_name() {
            var e = Fixture1Enumerator();
            string[] results = Glob.FilterDirectory(Glob.Parse("a/*.txt"),
                                                    "/",
                                                    e.FileExists,
                                                    e).ToArray();

            Assert.Equal(new [] {
                            "/a/b.txt",
                            "/a/d.txt",
                        }, results, FileNameComparison);
        }

        [Fact]
        public void FilterDirectory_enumerate_by_bracket_choice_recursive() {
            // TODO [a-z].txt could be used
            var e = Fixture1Enumerator();

            string[] results = Glob.FilterDirectory(Glob.Parse("**/[bhi].txt"),
                                                    "/",
                                                    e.FileExists,
                                                    e).ToArray();
            Assert.Equal("([^/\\:]+/)*/(b|h|i)\\.txt$", Glob.Parse("**/[bhi].txt").ToRegex().ToString());
            Assert.True(Glob.Parse("**/[bhi].txt").IsMatch("/a/e/f/g/h.txt"));
            Assert.Equal(new [] {
                            "/a/b.txt",
                            "/a/e/f/g/h.txt",
                            "/a/e/f/g/i.txt",
                        }, results, FileNameComparison);
        }

        [Fact]
        public void FilterDirectory_enumerate_by_wildcard_recursive_similar_names() {
            var e = Fixture2Enumerator();

            // Should exclude the ~a dire
            string[] results = Glob.FilterDirectory(Glob.Parse("src/**/a/*.txt"),
                                                    "/",
                                                    e.FileExists,
                                                    e).ToArray();
            Assert.Equal(new [] {
                            "/src/a/bon.txt",
                            "/src/a/bot.txt",
                        }, results, FileNameComparison);
        }

        [Fact]
        public void Parse_provides_glob_anything_equivalence() {
            Assert.Equal(Glob.Parse("**/*.*"), Glob.Anything);
        }

        [Fact]
        public void Constructor_produces_custom_Glob_implementation() {
            Assert.Contains("/base/how", new MyGlob("how").EnumerateFiles(), FileNameComparison);
        }

        [Theory]
        [InlineData("s", "s/*.cs")]
        [InlineData("/", "/*.cs")]
        [InlineData("", "*.cs")]
        public void Concat_produces_Glob_by_nominal(string path, string expected) {
            Assert.Equal(expected, Glob.Concat(path, Glob.Parse("*.cs")).ToString(), FileNameComparison);
        }

        [Theory]
        [InlineData("s")]
        [InlineData("/")]
        [InlineData("")]
        public void Concat_produces_input_Glob_when_it_is_rooted(string path) {
            string expected = "/var/*.cs";
            Assert.Equal(expected, Glob.Concat(path, Glob.Parse(expected)).ToString(), FileNameComparison);
        }

        [Fact]
        public void Concat_produces_Glob_from_composed_Glob() {
            Assert.Equal("a/*.cs;a/*.vb", Glob.Concat("a", Glob.Parse("*.cs;*.vb")).ToString(), FileNameComparison);
        }

        [Theory]
        [InlineData("./Suite", "/Suite$")]
        [InlineData("./././Suite", "/Suite$")]
        public void ToRegex_should_implement_special_directory_name(string text, string expected) {
            Assert.Equal(expected, Glob.Parse(text).ToRegex().ToString());
        }

        [Fact]
        public void FilterDirectory_should_enumerate_with_current_working_directory() {
            var e = Fixture2Enumerator();

            string[] results = Glob.FilterDirectory(Glob.Parse("./././ca/./*.txt"),
                                                    "/src",
                                                    e.FileExists,
                                                    e).ToArray();
            Assert.Equal(new [] {
                            "/src/ca/bon.txt",
                            "/src/ca/bot.txt",
                        }, results, FileNameComparison);
        }

        public class MyGlob : Glob {

            public MyGlob(string text)
                : base(text, new MyGlobController()) {}

            private class MyGlobController : GlobController {

                public override bool FileExists(string path) {
                    return true;
                }

                public override bool DirectoryExists(string path) {
                    return true;
                }

                public override IEnumerable<string> EnumerateFileSystemEntries(string path) {
                    if (path == @"\base") {
                        return new [] { @"\base\how" };
                    }
                    if (path == "/base") {
                        return new [] { "/base/how" };
                    }
                    return base.EnumerateFileSystemEntries(path);
                }

                public override string RootDirectory {
                    get {
                        return "/";
                    }
                }
                public override string WorkingDirectory {
                    get {
                        return "/base";
                    }
                }
            }
        }

        private static Glob.GlobController CreateEnumerator(FileObject file) {
            return new TestController(file);
        }

        private static Glob.GlobController Fixture1Enumerator() {
            return CreateEnumerator(Fixture1());
        }

        private static FileObject Fixture1() {
            return Root(Dir("a",
                            File("b.txt"),
                            File("c.csv"),
                            File("d.txt"),
                            Dir("e",
                                Dir("f",
                                    Dir("g",
                                        File("h.txt"),
                                        File("i.txt")),
                                    File("j")),
                                File(".k"),
                                File("l.csv"))));
        }

        private static Glob.GlobController Fixture2Enumerator() {
            return CreateEnumerator(Fixture2());
        }

        private static FileObject Fixture2() {
            return Root(Dir("src",
                            Dir("a",
                                File("bon.txt"),
                                File("boy.csv"),
                                File("bot.txt"),
                                File("l.csv")),
                            Dir("ca",
                                File("bon.txt"),
                                File("boy.csv"),
                                File("bot.txt"),
                                File("l.csv")),
                            Dir("~a",
                                File("bon.txt"),
                                File("boy.csv"),
                                File("bot.txt"),
                                File("l.csv"))));
        }

        sealed class TestController : Glob.GlobController {

            private IDictionary<string, FileObject> _mapCache;
            private FileObject _file;

            public TestController(FileObject file) {
                _file = file;
            }

            private IDictionary<string, FileObject> MapCache() {
                if (_mapCache == null) {
                    _mapCache = _file.Visit();
                }
                return _mapCache;
            }

            public override bool DirectoryExists(string path) {
                return true;
            }

            public override bool FileExists(string path) {
                return true;
            }

            public override IEnumerable<string> EnumerateFiles(string path) {
                return Invoke(t => !t.IsDirectory, path);
            }

            public override IEnumerable<string> EnumerateDirectories(string path) {
                return Invoke(t => t.IsDirectory, path);
            }

            public override IEnumerable<string> EnumerateFileSystemEntries(string path) {
                return Invoke(_ => true, path);
            }

            IEnumerable<string> Invoke(Predicate<FileObject> entityType, string path) {
                FileObject result;
                if (!MapCache().TryGetValue(path, out result)) {
                    result = FileObject.Empty;
                }

                return result.Files.Where(t => entityType(t)).Select(t => t.Path);
            }
        }

        class FileObject {

            public readonly bool IsDirectory;
            public readonly string Name;
            public readonly List<FileObject> Files = new List<FileObject>();
            public string Path;

            public static readonly FileObject Empty = new FileObject(true, null, Enumerable.Empty<FileObject>());

            public FileObject(bool directory, string name, IEnumerable<FileObject> files) {
                this.Name = name;
                IsDirectory = directory;

                if (files != null) {
                    foreach (var file in files)
                        this.Files.Add(file);
                }
            }

            public IDictionary<string, FileObject> Visit() {
                var result = new Dictionary<string, FileObject>();
                Visit(string.Empty, result);
                return result;
            }

            public void Visit(string path, IDictionary<string, FileObject> map) {
                this.Path = string.Concat(path, this.Name);
                path = this.Path + "/";

                if (this.Path == string.Empty) this.Path = "/";
                foreach (var f in this.Files) {
                    f.Visit(path, map);
                }

                map.Add(this.Path, this);
            }
        }


        private static FileObject Root(params FileObject[] files) {
            return new FileObject(true, "", files);
        }

        private static FileObject Dir(string name, params FileObject[] files) {
            return new FileObject(true, name, files);
        }

        private static FileObject File(string name) {
            return new FileObject(false, name, null);
        }

    }

}
