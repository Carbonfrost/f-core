//
// Copyright 2016, 2020 Carbonfrost Systems, Inc. (https://carbonfrost.com)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     https://www.apache.org/licenses/LICENSE-2.0
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
using System.Runtime.Serialization;

namespace Carbonfrost.Commons.Core.Runtime {

    public partial class Template {

        protected class TemplateBuilderContext {

            private readonly object _object;

            public PropertyInfo Property { get; private set; }

            public object Object {
                get {
                    return _object ?? Parent.Object;
                }
            }

            public bool IsPropertyContext {
                get {
                    return Property != null;
                }
            }

            public object PropertyValue {
                get {
                    return Property.GetValue(Object);
                }
            }

            public TemplateBuilderContext Parent { get; private set; }

            internal TemplateBuilderContext(TemplateBuilderContext parent,
                object obj,
                PropertyInfo prop) {
                Parent = parent;
                _object = obj;
                Property = prop;
            }
        }

        protected class TemplateBuilder {

            private readonly List<ITemplateCommand> _commands = new List<ITemplateCommand>();
            private readonly ObjectIDGenerator _items = new ObjectIDGenerator();
            private TemplateBuilderContext _current;

            internal List<ITemplateCommand> Commands {
                get {
                    return _commands;
                }
            }

            public TemplateBuilderContext CurrentContext {
                get {
                    return _current;
                }
            }

            public void CreateChild(Type objectType) {
                var child = Activation.CreateInstance(objectType);
                PushCurrent(child, null);
                AddCommand(new PushObjectCommand(child));
            }

            public void CopyContent(object value) {
                foreach (var obj in (IEnumerable) value) {
                    if (!TryAddToGraph(obj)) {
                        continue;
                    }
                    if (obj == null || Template.IsImmutable(obj.GetType()) || obj.GetType().GetTemplatingMode() == TemplatingMode.Copy) {
                        AddChild(obj);
                    } else {
                        CreateChild(obj.GetType());
                        CopyObjectOverride();
                        AddChild();
                    }
                }
            }

            public virtual void ApplyTemplate(ITemplate template) {
                if (template == null) {
                    throw new ArgumentNullException("template");
                }
                AddCommand(new ApplyTemplateCommand(template));
            }

            public void AddChild(object value) {
                AddCommand(new PushObjectCommand(value));
                AddCommand(new AddChildCommand());
            }

            public void AddChild() {
                PopCurrent();
                AddCommand(new AddChildCommand());
            }

            protected bool TryAddToGraph(object obj) {
                if (obj == null || obj.GetType().GetTypeInfo().IsValueType) {
                    return false;
                }
                bool first;
                _items.GetId(obj, out first);
                return first;
            }

            public void CopyObject(object obj) {
                if (!TryAddToGraph(obj)) {
                    return;
                }
                StartObject(obj);
                CopyObjectOverride();
                EndObject();
            }

            internal void CopyObject(object obj, IEnumerable<PropertyInfo> props) {
                if (!TryAddToGraph(obj)) {
                    return;
                }
                StartObject(obj);
                CopyFromContentOrProperties(props);
                EndObject();
            }

            protected virtual void CopyObjectOverride() {
                CopyFromContentOrProperties(null);
            }

            public void StartObject(object obj) {
                if (obj == null) {
                    throw new ArgumentNullException("obj");
                }
                PushCurrent(obj, null);
            }

            public void EndObject() {
                PopCurrent();
            }

            protected virtual TemplatingMode GetPropertyTemplatingMode(PropertyInfo property) {
                return property.GetTemplatingMode();
            }

            protected virtual IEnumerable<PropertyInfo> GetTemplateProperties(object obj) {
                return Template.GetProperties(obj);
            }

            protected virtual void CopyPropertyOverride() {
                var currentObject = CurrentContext.Object;
                var p = CurrentContext.Property;
                if (p.GetMethod == null || !p.GetMethod.IsPublic) {
                    return;
                }
                var mode = GetPropertyTemplatingMode(p);
                if (mode == TemplatingMode.Hidden) {
                    return;
                }
                bool writable = p.SetMethod != null && p.SetMethod.IsPublic;
                if (!writable && mode == TemplatingMode.Copy) {
                    // Can't write to the property and it's a direct set operation
                    // hence just skip
                    return;
                }

                var propValue = p.GetValue(currentObject);
                if (propValue == null || Equals(propValue, p.PropertyType.GetDefaultValue())) {
                    // Nothing to do if null or zero
                    return;
                }
                if (mode == TemplatingMode.Content) {
                    CopyContent(propValue);
                    return;
                }

                if (writable) {
                    SetValue(propValue);
                } else {
                    CopyObject(propValue);
                }
            }

            public void CopyProperty(PropertyInfo property) {
                StartProperty(property);
                CopyPropertyOverride();
                EndProperty();
            }

            public void StartProperty(PropertyInfo property) {
                PushCurrent(null, property);
                AddCommand(new PushCommand(property));
            }

            public void EndProperty() {
                PopCurrent();
                AddCommand(new PopCommand());
            }

            public virtual void SetValue(object value) {
                RequireProperty();
                AddCommand(new SetPropertyInfoCommand(CurrentContext.Property, value));
            }

            private void RequireProperty() {
                if (!CurrentContext.IsPropertyContext) {
                    throw new NotImplementedException();
                }
            }

            protected bool TryCopyFromMethod() {
                object initializer = CurrentContext.Object;
                var copyFromMethod = Template.FindCopyFromMethod(initializer.GetType());
                if (copyFromMethod != null) {
                    AddCommand(new CopyFromCommand(copyFromMethod, initializer));
                    return true;
                }
                return false;
            }

            protected bool TryCopyContent() {
                object initializer = CurrentContext.Object;
                if (initializer is IEnumerable) {
                    CopyContent(initializer);
                    return true;
                }
                return false;
            }

            protected void CopyProperties() {
                CopyPropertiesCore(null);
            }

            private void CopyPropertiesCore(IEnumerable<PropertyInfo> properties) {
                foreach (var p in properties) {
                    CopyProperty(p);
                }
            }

            internal void CopyFromContentOrProperties(IEnumerable<PropertyInfo> properties) {
                object initializer = CurrentContext.Object;
                if (properties == null && TryCopyFromMethod()) {
                    return;
                }
                if (properties == null) {
                    properties = GetTemplateProperties(initializer);
                }
                TryCopyContent();
                CopyPropertiesCore(properties);
            }

            private void AddCommand(ITemplateCommand cmd) {
                _commands.Add(cmd);
            }

            private void PopCurrent() {
                _current = _current.Parent;
            }

            private void PushCurrent(object obj, PropertyInfo pi) {
                _current = new TemplateBuilderContext(CurrentContext, obj, pi);
            }
        }
    }

}
