//
// Copyright 2012, 2016 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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

    public partial class Template {

        private static readonly IDictionary<Type, ReflectedData> _dataCache = new Dictionary<Type, ReflectedData>();

        public static readonly ITemplate Null = new NullTemplate();

        public static ITemplate Compose(params ITemplate[] items) {
            if (items == null || items.Length == 0) {
                return Null;
            }
            if (items.Length == 1) {
                return items[0];
            }

            return new CompositeTemplate(items.ToArray());
        }

        public static ITemplate Compose(IEnumerable<ITemplate> items) {
            if (items == null) {
                return Null;
            }

            return Compose(items.ToArray());
        }

        public static Template<T> Create<T>(Action<T> initializer) {
            if (initializer == null) {
                return Typed<T>(Null);
            }

            return Typed<T>(new ThunkTemplate<T>(initializer));
        }

        public static Template<T> Typed<T>(ITemplate source) {
            return new Template<T>(source ?? Null);
        }

        public static Type GetTemplateType(Type componentType) {
            if (componentType == null) {
                throw new ArgumentNullException("componentType");
            }
            return TemplateFactory.Default.GetTemplateType(componentType)
                ?? typeof(Template);
        }

        public static Func<T> ToFactory<T>(ITemplate template) {
            if (template == null)
                throw new ArgumentNullException("template");

            return () => {
                var result = Activation.CreateInstance<T>();
                template.Apply(result);
                return result;
            };
        }

        public static Template<T> Create<T>(T initializer,
                                            Func<T, PropertyInfo, bool> propertyFilter,
                                            IComparer<PropertyInfo> propertyComparer) {
            if (object.Equals(initializer, null)) {
                return Typed<T>(Null);
            }

            var props = GetProperties(initializer);
            if (propertyFilter != null) {
                props = props.Where(t => propertyFilter(initializer, t));
            }
            if (propertyComparer != null) {
                props = props.OrderBy(p => p, propertyComparer);
            }

            return Typed<T>(new Template(initializer, props));
        }

        public static Template<T> Create<T>(T initializer,
                                                    Func<T, PropertyInfo, bool> propertyFilter) {
            return Create<T>(initializer, propertyFilter, null);
        }

        public static Template<T> Create<T>(T initializer,
                                                    Func<PropertyInfo, bool> propertyFilter) {
            if (propertyFilter == null) {
                return Create<T>(initializer);
            }
            Func<T, PropertyInfo, bool> closed = (a, o) => propertyFilter(o);
            return Create<T>(initializer, closed, null);
        }

        public static Template<T> Create<T>(T initializer) {
            if (object.Equals(initializer, null)) {
                return Typed<T>(Null);
            }
            return Typed<T>(new Template(initializer));
        }

        public static ITemplate Create(IEnumerable<KeyValuePair<string, object>> initializer) {
            if (initializer == null) {
                return Null;
            }

            return new PropertyBagTemplate(initializer);
        }

        public static ITemplate FromName(Type templateType, string name) {
            if (templateType == null) {
                throw new ArgumentNullException("templateType");
            }
            if (name == null) {
                throw new ArgumentNullException("name");
            }
            if (name.Length == 0) {
                throw Failure.EmptyString("name");
            }
            return TemplateData.GetTemplatesByLocalName(templateType, name).SingleOrDefault();
        }

        public static ITemplate FromName(Type templateType, QualifiedName name) {
            if (templateType == null) {
                throw new ArgumentNullException("templateType");
            }
            if (name == null) {
                throw new ArgumentNullException("name");
            }
            return TemplateData.GetTemplate(templateType, name);
        }

        public static Template<T> FromName<T>(string name) {
            return Typed<T>(FromName(typeof(T), name));
        }

        public static Template<T> FromName<T>(QualifiedName name) {
            return Typed<T>(FromName(typeof(T), name));
        }

        public static object Copy(object source, object destination) {
            if (ReferenceEquals(source, null)) {
                throw new ArgumentNullException("source");
            }
            if (ReferenceEquals(destination, null)) {
                throw new ArgumentNullException("destination");
            }
            new Template(source).Apply(destination);
            return destination;
        }

        public static object Copy(object source) {
            if (source == null) {
                throw new ArgumentNullException("source");
            }

            var destination = Activation.CreateInstance(source.GetType());
            return Copy(source, destination);
        }

        public static QualifiedName GetTemplateName(ITemplate source) {
            if (source == null) {
                throw new ArgumentNullException("source");
            }
            var wrapper = source as ITemplateWrapper;
            if (wrapper != null) {
                source = wrapper.InnerTemplate;
            }
            // Expected via ReflectedTemplate, Template<>
            return Utility.LateBoundProperty<QualifiedName>(source, "QualifiedName");
        }

        internal static MethodInfo FindAddonMethod(Type type) {
            var data = RequiredReflected(type);
            if (data.AddOn == null) {
                data.AddOn = type.GetTypeInfo().GetMethods(BindingFlags.Instance | BindingFlags.Public)
                    .FirstOrDefault(IsValidAddon);
            }

            return data.AddOn;
        }

        internal static void DefaultCopyContent(object source, object destination) {
            if (destination is System.Collections.IEnumerable && source is System.Collections.IEnumerable) {
                var addon = Template.FindAddonMethod(destination.GetType());
                if (addon != null) {
                    foreach (var o in (System.Collections.IEnumerable)source) {
                        addon.Invoke(destination, new[] { o });
                    }
                }
            }
        }

        internal static bool IsImmutable(Type t) {
            var type = t.GetTypeInfo();
            if (type.IsPointer || type.IsPrimitive || type.IsEnum) {
                return true;
            }
            if (Nullable.GetUnderlyingType(t) != null) {
                return true;
            }
            if (t.GetTemplatingMode() == TemplatingMode.Copy) {
                return true;
            }
            if (type.IsArray) {
                return true;
            }
            if (typeof(MemberInfo).GetTypeInfo().IsAssignableFrom(type)) {
                return true;
            }
            if (FindAddonMethod(t) != null) {
                return false;
            }
            var data = RequiredReflected(t);
            if (!data.Immutable.HasValue) {
                // In order to prevent reentrancy on this, we preemptively set ourselves as mutable.
                data.Immutable = false;
                data.Immutable = !type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Any(IsMutableProperty);
            }

            return data.Immutable.Value;
        }

        internal static PropertyInfo FirstMutableProperty(Type type) {
            return type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .FirstOrDefault(IsMutableProperty);
        }

        static bool IsMutableProperty(PropertyInfo p) {
            if (p.GetTemplatingMode() == TemplatingMode.Hidden) {
                return false;
            }
            return (p.SetMethod != null && p.SetMethod.IsPublic)
                || !IsImmutable(p.PropertyType);
        }

        internal static bool IsValidAddon(MethodInfo mi) {
            ParameterInfo[] parameters = mi.GetParameters();
            if (parameters.Length == 1) {
                var param = parameters[0];

                return mi.ReturnType == typeof(void)
                    && mi.Name == "Add"
                    && !param.IsOut
                    && !param.ParameterType.IsByRef;
            }

            return false;
        }

        internal static MethodInfo FindCopyFromMethod(Type type) {
            var data = RequiredReflected(type);
            if (data.CopyFrom == null) {
                try {
                    // Look for CopyFrom method
                    data.CopyFrom = type.GetMethod("CopyFrom", new[] { type });
                } catch (AmbiguousMatchException) {
                }
            }
            return data.CopyFrom;
        }

        internal static IEnumerable<PropertyInfo> GetProperties(object obj) {
            if (obj == null)
                return Array.Empty<PropertyInfo>();

            return obj.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(t => t.CanRead && t.GetIndexParameters().Length == 0);
        }

        internal static IDictionary<string, PropertyInfo> GetPropertyCache(object obj) {
            if (obj == null)
                return Empty<string, PropertyInfo>.Dictionary;

            var data = RequiredReflected(obj.GetType());
            if (data.Properties == null) {
                var props = Template.GetProperties(obj);
                IDictionary<string, PropertyInfo> properties = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);

                foreach (var p in props) {
                    if (!properties.ContainsKey(p.Name)) {
                        properties[p.Name] = p;
                    }
                }
                data.Properties = properties;
            }
            return data.Properties;
        }

        internal static Type[] GetActivationProviderTypes(Type type) {
            var data = RequiredReflected(type);
            if (data.ActivationProviders == null) {
                // Each interface is considered to see if it has an activation provider; the type's
                // attributes are also considered to see if they are IActivationProvider or define
                // an activation provider

                IEnumerable<Type> interfaces = type.GetTypeInfo().GetInterfaces();
                if (type.GetTypeInfo().IsInterface) {
                    interfaces = interfaces.Concat(new [] { type });
                }
                HashSet<Type> e = new HashSet<Type>(
                    interfaces
                    .Select(i => i.GetAdapterType(AdapterRole.ActivationProvider))
                    .WhereNotNull());
                data.ActivationProviders = e.ToArray();
            }
            return data.ActivationProviders;
        }

        private static ReflectedData RequiredReflected(Type type) {
            return _dataCache.GetValueOrCache(type, () => new ReflectedData());
        }

        sealed class NullTemplate : ITemplate {
            void ITemplate.Apply(object value) {}
        }

        sealed class CompositeTemplate : ITemplate {

            readonly ITemplate[] _items;

            public CompositeTemplate(ITemplate[] items) {
                _items = items;
            }

            public void Apply(object value) {
                foreach (var t in _items) {
                    t.Apply(value);
                }
            }
        }

        sealed class PropertyBagTemplate : ITemplate {

            private readonly List<KeyValuePair<string, object>> _properties;

            public PropertyBagTemplate(IEnumerable<KeyValuePair<string, object>> properties) {
                _properties = properties.ToList();
            }

            public void Apply(object value) {
                Activation.Initialize(value, _properties);
            }
        }

        sealed class ThunkTemplate<T> : ITemplate {

            private readonly Action<T> _initializer;

            public ThunkTemplate(Action<T> initializer) {
                _initializer = initializer;
            }

            public void Apply(object value) {
                _initializer((T) value);
            }
        }

        private class ReflectedData {
            public bool? Immutable;
            public MethodInfo AddOn;
            public MethodInfo CopyFrom;
            public IDictionary<string, PropertyInfo> Properties;
            public Type[] ActivationProviders;
        }

    }
}
