//
// Copyright 2013, 2019 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using Carbonfrost.Commons.Core.Runtime;
using Carbonfrost.Commons.Spec;

namespace Carbonfrost.UnitTests.Core.Runtime {

    public class NamedObjectCollectionTests {

        class ImplicityNamed {
            public string Name { get; set; }
        }

        [Fact]
        public void Add_should_add_by_implicit_name_nominal() {
            NamedObjectCollection<ImplicityNamed> t = new NamedObjectCollection<ImplicityNamed>();
            var item = new ImplicityNamed { Name = "a" };
            t.Add(item);

            Assert.Same(item, t["a"]);
            Assert.True(t.Contains("a"));
            Assert.True(t.Contains(item));
        }

        [Fact]
        public void Clear_should_remove_names() {
            NamedObjectCollection<ImplicityNamed> t = new NamedObjectCollection<ImplicityNamed>();
            var item = new ImplicityNamed { Name = "a" };
            t.Add(item);
            t.Clear();

            Assert.Equal(0, t.Count);
            Assert.Null(t["a"]);
        }

        [Fact]
        public void Add_should_add_using_full_type_name() {
            var t = new NamedObjectCollection<UriParser>();
            var item = new HttpStyleUriParser();
            t.Add(item);

            Assert.Same(item, t["System.HttpStyleUriParser"]);
        }

        [Fact]
        public void Add_should_add_using_implicit_and_explicit_names() {
            var t = new NamedObjectCollection<UriParser>();
            var item = new HttpStyleUriParser();
            t.Add("http", item);

            Assert.Same(item, t["System.HttpStyleUriParser"]);
            Assert.Same(item, t["http"]);
        }

        [Fact]
        public void Add_should_allow_multiple_with_same_implicit_name() {
            var t = new NamedObjectCollection<UriParser>();
            var item1 = new HttpStyleUriParser();
            var item2 = new HttpStyleUriParser();

            t.Add(item1);
            t.Add(item2);

            Assert.HasCount(2, t);
        }

        [Fact]
        public void Add_should_not_use_implicit_name_on_second_instance() {
            var t = new NamedObjectCollection<UriParser>();
            var item1 = new HttpStyleUriParser();
            var item2 = new HttpStyleUriParser();

            t.Add(item1);
            t.Add(item2);

            Assert.Equal(new [] {"System.HttpStyleUriParser"}, t.GetNames(item1));
            Assert.Empty(t.GetNames(item2));
        }
    }
}
