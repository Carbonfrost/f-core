//
// Copyright 2005, 2006, 2010, 2014, 2016, 2019 Carbonfrost Systems, Inc.
// (http://carbonfrost.com)
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
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Carbonfrost.Commons.Core.Runtime {

    public static partial class Adaptable {

        static readonly MethodInfo GetDefaultValueMethod = typeof(Adaptable).GetMethod("GetDefaultValue_", BindingFlags.Static | BindingFlags.NonPublic);

        public static object ApplyProperties(this MethodInfo method, object thisArg, IProperties properties) {
            if (method == null) {
                throw new ArgumentNullException("method");
            }

            properties = properties ?? Properties.Empty;
            var methodParameters = method.GetParameters();
            var parms = new object[methodParameters.Length];
            int paramIndex = 0;
            if (method.IsStatic) {
                if (methodParameters.Length == 0) {
                    throw RuntimeFailure.ApplyPropertiesStaticMethodRequiresArg("method");
                }
                if (thisArg == null) {
                    throw new ArgumentNullException("thisArg");
                }
                parms[0] = thisArg;
                paramIndex = 1;
                if (!methodParameters[0].ParameterType.IsInstanceOfType(thisArg)) {
                    throw RuntimeFailure.ThisArgumentIncorrectType(thisArg.GetType());
                }
            }

            for (; paramIndex < methodParameters.Length; paramIndex++) {
                var pi = methodParameters[paramIndex];
                parms[paramIndex] = properties.GetProperty(pi.Name);
            }

            return method.Invoke(thisArg, parms);
        }

        public static object GetDefaultValue(this PropertyInfo property) {
            if (property == null) {
                throw new ArgumentNullException("property");
            }
            var data = property.CustomAttributes.FirstOrDefault(t => t.AttributeType == typeof(DefaultValueAttribute));
            if (data != null && data.ConstructorArguments.Count == 2) {
                // Do a parse to extract the value
                return Activation.FromText((Type) data.ConstructorArguments[0].Value,
                    (string) data.ConstructorArguments[1].Value);
            }

            var attr = property.GetCustomAttribute<DefaultValueAttribute>();
            if (attr == null) {
                return property.PropertyType.GetDefaultValue();
            }
            return attr.Value;
        }

        public static object GetDefaultValue(this Type type) {
            if (type.GetTypeInfo().IsValueType)
                return Adaptable.GetDefaultValueMethod.MakeGenericMethod(type).Invoke(null, null);

            return null;
        }

        private static T GetDefaultValue_<T>() {
            return default(T);
        }

        public static IEnumerable<string> GetAdapterRoleNames(this Assembly assembly) {
            return assembly.GetCustomAttributes(typeof(DefinesAttribute))
                .Select(t => ((DefinesAttribute) t).AdapterName)
                .Distinct();
        }

        public static MethodInfo GetTemplateMethod(this Type type) {
            if (type == null)
                throw new ArgumentNullException("type");

            return Template.FindCopyFromMethod(type);
        }

        public static MethodInfo GetParseMethod(this Type type) {
            if (type == null) {
                throw new ArgumentNullException("type");
            }
            return type.GetTypeInfo().GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Static)
                .FirstOrDefault(m => m.Name == "Parse" && !m.IsGenericMethod);
        }

        public static MethodInfo GetTryParseMethod(this Type type) {
            if (type == null) {
                throw new ArgumentNullException("type");
            }
            return type.GetTypeInfo().GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Static)
                .FirstOrDefault(m => m.Name == "TryParse" && !m.IsGenericMethod && m.ReturnType == typeof(bool));
        }

        public static ConstructorInfo GetActivationConstructor(this Type type) {
            if (type == null) {
                throw new ArgumentNullException("type");
            }

            ConstructorInfo[] ci = type.GetTypeInfo().GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (ci.Length == 0) {
                return null;
            }
            return ci.FirstOrDefault(IsActivationConstructor) ?? ci[0];
        }

        public static IEnumerable<IActivationProvider> GetActivationProviders(this Type type) {
            if (type == null) {
                throw new ArgumentNullException("type");
            }

            var types = Template.GetActivationProviderTypes(type);
            List<IActivationProvider> results = types.Select(t => Activator.CreateInstance(t))
                .Cast<IActivationProvider>()
                .ToList();
            var providers = App.GetProviders<IActivationProvider>();

            results.AddMany(type.GetTypeInfo().GetCustomAttributes(true).OfType<IActivationProvider>());
            results.AddMany(providers);
            return results;
        }

        public static IEnumerable<Type> GetStartClasses(this Assembly assembly, string className) {
            if (assembly == null) {
                throw new ArgumentNullException("assembly");
            }

            return StartClassInfo.Get(assembly).GetByName(className);
        }

        public static IEnumerable<TValue> GetStartFields<TValue>(this Assembly assembly, string className) {
            if (assembly == null)
                throw new ArgumentNullException("assembly");

            return StartClassInfo.FindStartFields<TValue>(StartClassInfo.Get(assembly).GetByName(className));
        }

        public static IEnumerable<Type> GetTypesByNamespaceUri(
            this Assembly assembly, NamespaceUri namespaceUri) {

            if (assembly == null)
                throw new ArgumentNullException("assembly");
            if (namespaceUri == null)
                throw new ArgumentNullException("namespaceUri");

            return assembly.GetTypesHelper().Select(t => t.AsType()).Where(t => t.GetQualifiedName().Namespace == namespaceUri);
        }

        public static IEnumerable<string> FilterNamespaces(
            this Assembly assembly, string namespacePattern) {
            if (assembly == null)
                throw new ArgumentNullException("assembly");

            return AssemblyInfo.GetAssemblyInfo(assembly).GetNamespaces(namespacePattern);
        }

        public static IEnumerable<MethodInfo> GetImplicitFilterMethods(this PropertyInfo property,
                                                                       Type attributeType) {
            if (property == null)
                throw new ArgumentNullException("property"); // $NON-NLS-1

            if (attributeType == null)
                throw new ArgumentNullException("attributeType"); // $NON-NLS-1

            // For instance, LocalizableAtrribute/Title ==>  GetLocalizableTitle
            string nakedName = Utility.GetImpliedName(attributeType, "Attribute");
            string methodName = string.Concat("Get", nakedName, property.Name);

            return property.DeclaringType.GetMethods().Where(mi => mi.Name == methodName);
        }

        public static object Adapt(this object source, string adapterRoleName, IServiceProvider serviceProvider = null) {
            object result = TryAdapt(source, adapterRoleName, serviceProvider);
            if (result == null) {
                throw Failure.NotAdaptableTo("source", source, adapterRoleName);
            }
            return result;
        }

        public static object Adapt(this object source, Type adapterType, IServiceProvider serviceProvider = null) {
            object result = TryAdapt(source, adapterType, serviceProvider);
            if (result == null) {
                throw Failure.NotAdaptableTo("source", source, adapterType);
            }
            return result;
        }

        public static T Adapt<T>(this object source, IServiceProvider serviceProvider = null)
            where T: class
        {
            return (T) Adapt(source, typeof(T), serviceProvider);
        }

        public static object TryAdapt(this object source, string adapterRoleName, IServiceProvider serviceProvider = null) {
            if (source == null) {
                throw new ArgumentNullException("source"); // $NON-NLS-1
            }
            if (adapterRoleName == null) {
                throw new ArgumentNullException("adapterRoleName");
            }
            if (string.IsNullOrEmpty(adapterRoleName)) {
                throw Failure.EmptyString("adapterRoleName");
            }
            var af = (serviceProvider ?? ServiceProvider.Null)
                .GetServiceOrDefault<IAdapterFactory>(AdapterFactory.Default);

            return af.GetAdapter(source, adapterRoleName);
        }

        public static object TryAdapt(this object source, Type adapterType, IServiceProvider serviceProvider = null) {
            if (source == null)
                throw new ArgumentNullException("source"); // $NON-NLS-1
            if (adapterType == null)
                throw new ArgumentNullException("adapterType"); // $NON-NLS-1

            if (adapterType.IsInstanceOfType(source)) {
                return source;
            }

            return null;
        }

        public static T TryAdapt<T>(this object source, IServiceProvider serviceProvider = null)
            where T: class
        {
            return (T) TryAdapt(source, typeof(T), serviceProvider);
        }

        public static IEnumerable<string> GetAdapterRoleNames(this Type adapteeType) {
            if (adapteeType == null) {
                throw new ArgumentNullException("adapteeType"); // $NON-NLS-1
            }

            return GetAdapterRoleNames(adapteeType, true);
        }

        public static IEnumerable<string> GetAdapterRoleNames(this Type adapteeType, bool inherit) {
            if (adapteeType == null) {
                throw new ArgumentNullException("adapteeType");
            }

            AdapterAttribute[] items = (AdapterAttribute[]) adapteeType.GetTypeInfo().GetCustomAttributes(typeof(AdapterAttribute), inherit);
            return items.Select(t => t.Role).Distinct(StringComparer.OrdinalIgnoreCase);
        }

        public static Type GetAdapterType(this Type adapteeType, string adapterRoleName) {
            return AdapterFactory.Default.GetAdapterType(adapteeType, adapterRoleName);
        }

        public static MethodInfo GetMethodBySignature<TDelegate>(this Type instanceType, string name, Expression<TDelegate> signature)
            where TDelegate : class
        {
            return GetMethodBySignatureCore<TDelegate>(
                instanceType,
                name,
                signature);
        }

        public static MethodInfo GetStaticMethodBySignature<TDelegate>(this Type instanceType, string name, Expression<TDelegate> signature)
            where TDelegate : class
        {
            return GetMethodBySignatureCore<TDelegate>(
                instanceType,
                name,
                signature);
        }

        static MethodInfo GetMethodBySignatureCore<TDelegate>(Type instanceType,
                                                              string name,
                                                              Expression<TDelegate> signature)
            where TDelegate : class
        {
            if (instanceType == null)
                throw new ArgumentNullException("instanceType"); // $NON-NLS-1
            if (name == null)
                throw new ArgumentNullException("name"); // $NON-NLS-1

            if (name.Length == 0)
                throw Failure.EmptyString("name");

            Type[] argTypes = signature.Parameters.Select(p => p.Type).ToArray();
            MethodInfo mi = instanceType.GetTypeInfo().GetMethod(name, argTypes, null);
            if (mi == null)
                return null;
            if (signature.ReturnType == null)
                return mi.ReturnType == null ? mi : null;
            if (signature.ReturnType.IsAssignableFrom(mi.ReturnType))
                return mi;
            else
                return null;
        }

        public static Type GetBuilderType(this Type adapteeType) {
            if (adapteeType == null) {
                throw new ArgumentNullException("adapteeType"); // $NON-NLS-1
            }

            var result = Adaptable.GetAdapterType(adapteeType, AdapterRole.Builder);
            if (result == null || AdapterRole.IsBuilderType(result, adapteeType)) {
                return result;
            }
            return null;
        }

        public static bool IsProviderType(this Type providerType) {
            if (providerType == null) {
                throw new ArgumentNullException("providerType");
            }

            return App.GetProviderTypes().Contains(providerType);
        }

        public static bool IsServiceType(this Type type) {
            if (type == null)
                throw new ArgumentNullException("type");

            var tt = type.GetTypeInfo();
            bool isStatic = tt.IsSealed && tt.IsAbstract;
            return !(tt.IsPrimitive || tt.IsEnum || isStatic);
        }

        public static bool IsComposable(this Type type) {
            if (type == null) {
                throw new ArgumentNullException("type");
            }

            return type.GetTypeInfo().IsDefined(typeof(ComposableAttribute), false);
        }

        internal static object InvokeBuilder(
            object instance, out MethodInfo buildMethod, IServiceProvider serviceProvider) {

            if (instance == null)
                throw new ArgumentNullException("instance");

            Type componentType = instance.GetType();
            // Invoke the builder
            Type[] argtypes = { typeof(IServiceProvider) };

            buildMethod = componentType.GetTypeInfo().GetMethod("Build", argtypes);
            if (buildMethod != null) {
                object[] arguments = { serviceProvider };
                return buildMethod.Invoke(instance, arguments);
            }

            // Check for the parameterless implementation
            buildMethod = componentType.GetMethod("Build", Type.EmptyTypes);

            if (buildMethod != null)
                return buildMethod.Invoke(instance, null);

            return null;
        }

        public static object InvokeBuilder(
            this object instance, IServiceProvider serviceProvider) {

            MethodInfo info;
            return InvokeBuilder(instance, out info, serviceProvider);
        }

        public static QualifiedName GetQualifiedName(this Type type) {
            if (type == null) {
                throw new ArgumentNullException("type");
            }
            var tt = type.GetTypeInfo();
            if (tt.IsGenericParameter || (tt.IsGenericType && !tt.IsGenericTypeDefinition)) {
                throw RuntimeFailure.QualifiedNameCannotBeGeneratedFromConstructed("type");
            }

            AssemblyInfo ai = AssemblyInfo.GetAssemblyInfo(tt.Assembly);
            NamespaceUri xmlns = ai.GetXmlNamespace(tt.Namespace);
            return xmlns + QualName(type);
        }

        static string QualName(Type type) {
            string name = type.Name.Replace("`", "-");
            if (type.IsNested)
                return string.Concat(QualName(type.DeclaringType), '.', name);
            else
                return name;
        }

        static IActivationProvider MakeActivationProvider(Type type) {
            return (IActivationProvider) Activator.CreateInstance(type);
        }

        static bool IsActivationConstructor(ConstructorInfo t) {
            return t.IsDefined(typeof(ActivationConstructorAttribute), false);
        }
    }
}
