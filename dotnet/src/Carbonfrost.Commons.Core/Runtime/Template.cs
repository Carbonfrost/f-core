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
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Carbonfrost.Commons.Core.Runtime {

    public partial class Template : ITemplate {

        private readonly IList<ITemplateCommand> _commands;

        public Template(object initializer) : this(initializer, new TemplateBuilder()) {
        }

        protected Template(object initializer, TemplateBuilder templateBuilder) {
            if (initializer == null) {
                throw new ArgumentNullException("initializer");
            }
            if (templateBuilder == null) {
                throw new ArgumentNullException("templateBuilder");
            }
            templateBuilder.CopyObject(initializer);
            _commands = templateBuilder.Commands;
        }

        internal Template(object initializer, IEnumerable<PropertyInfo> props) {
            var templateBuilder = new TemplateBuilder();
            templateBuilder.CopyObject(initializer, props);
            _commands = templateBuilder.Commands;
        }

        public void Apply(object value) {
            if (value == null) {
                throw new ArgumentNullException("value");
            }
            var stack = new Stack<object>();
            stack.Push(value);

            foreach (var e in _commands) {
                e.Apply(this, stack);
            }
        }

        internal string PrintProgram() {
            var sb = new StringBuilder();
            foreach (var cmd in _commands) {
                sb.AppendLine(cmd.ToString());
            }
            return sb.ToString();
        }

        // TODO Unlike property trees, we don't find the _best_ aggregation

        static void TryAggregation(object component, object value) {
            var enumerable = value as IEnumerable;
            if (enumerable == null) {
                return;
            }
            var items = enumerable;
            if (!ReferenceEquals(component, items) && enumerable.GetEnumerator().MoveNext()) {
                MethodInfo mi = Template.FindAddonMethod(component.GetType());
                if (mi == null) {
                    return;
                }
                foreach (var item in items) {
                    mi.Invoke(component, new object[] {
                                  item
                              });
                }
            }
        }
    }
}
