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
using System.Linq;
using System.Reflection;

namespace Carbonfrost.Commons.Core.Runtime {

    partial class Template {

        internal interface ITemplateCommand {
            void Apply(Template impl, Stack<object> values);
        }

        sealed class AddChildCommand : ITemplateCommand {

            void ITemplateCommand.Apply(Template impl, Stack<object> values) {
                var child = values.Pop();
                object destination = values.Peek();
                var addon = Template.FindAddonMethod(destination.GetType());
                if (addon == null) {
                    throw new NotImplementedException();
                }
                addon.Invoke(destination, new[] { child });
            }

            public override string ToString() {
                return "addchild";
            }
        }

        sealed class SetPropertyInfoCommand : ITemplateCommand {

            private readonly object _value;
            private readonly PropertyInfo _prop;

            public SetPropertyInfoCommand(PropertyInfo prop, object value) {
                _prop = prop;
                _value = value;
            }

            void ITemplateCommand.Apply(Template impl, Stack<object> values) {
                // HACK We need to remove and putback the property value, which is on the stack
                values.Pop();
                object component = values.Peek();
                values.Push(_value);
                var pi = _prop;
                if (pi.SetMethod == null && pi.GetMethod != null && pi.GetMethod.IsPublic) {
                    TryAggregation(component, _value);
                    return;
                }
                pi.SetValue(component, _value);
            }

            public override string ToString() {
                return string.Format("set {0}({1})", _prop, _value);
            }
        }

        sealed class ApplyTemplateCommand : ITemplateCommand {

            private readonly ITemplate _template;

            public ApplyTemplateCommand(ITemplate template) {
                _template = template;
            }

            void ITemplateCommand.Apply(Template impl, Stack<object> values) {
                var comp = values.Peek();
                _template.Apply(comp);
            }

            public override string ToString() {
                return string.Format("apply {0}", _template);
            }
        }

        sealed class PushCommand : ITemplateCommand {

            private readonly PropertyInfo _property;

            public PushCommand(PropertyInfo property) {
                _property = property;
            }

            void ITemplateCommand.Apply(Template impl, Stack<object> values) {
                var comp = values.Peek();
                var next = _property.GetValue(comp);
                values.Push(next);
            }

            public override string ToString() {
                return string.Format("pushproperty {0}", _property);
            }

        }

        sealed class PushObjectCommand : ITemplateCommand {

            private readonly object _value;

            public PushObjectCommand(object value) {
                _value = value;
            }

            void ITemplateCommand.Apply(Template impl, Stack<object> values) {
                values.Push(_value);
            }

            public override string ToString() {
                return string.Format("push {0}", _value);
            }

        }

        sealed class PopCommand : ITemplateCommand {

            void ITemplateCommand.Apply(Template impl, Stack<object> values) {
                values.Pop();
            }

            public override string ToString() {
                return "pop";
            }

        }

        sealed class CopyFromCommand : ITemplateCommand {

            private readonly MethodInfo _copyFromMethod;
            private readonly object _source;

            public CopyFromCommand(MethodInfo copyFromMethod, object source) {
                _copyFromMethod = copyFromMethod;
                _source = source;
            }

            void ITemplateCommand.Apply(Template impl, Stack<object> values) {
                object destination = values.Peek();
                _copyFromMethod.Invoke(destination, new object[] {
                                           _source
                                       });
            }

            public override string ToString() {
                return string.Format("copy {0}", _copyFromMethod);
            }

        }
    }
}
