//
// Copyright 2005, 2006, 2010, 2019 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using System.Globalization;
using System.IO;
using System.Linq.Expressions;
using System.Text;

namespace Carbonfrost.Commons.Core.Runtime {

    [Providers]
    public static partial class Activation {

        static readonly Expression<Func<string, object>> Parse
            = (a) => (default(object));

        static readonly Expression<Func<string, CultureInfo, object>> ParseWithCulture
            = (a, b) => (default(object));

        public static T CreateInstance<T>(QualifiedName typeName,
                                          IEnumerable<KeyValuePair<string, object>> values = null,
                                          IServiceProvider serviceProvider = null) {
            if (typeName == null) {
                throw new ArgumentNullException("typeName");
            }

            Type type = App.GetTypeByQualifiedName(typeName);
            return CreateInstance<T>(type,
                                     values,
                                     serviceProvider);
        }

        public static object CreateInstance(QualifiedName typeName,
                                            IEnumerable<KeyValuePair<string, object>> values = null,
                                            IServiceProvider serviceProvider = null) {
            if (typeName == null) {
                throw new ArgumentNullException("typeName");
            }

            return CreateInstance(App.GetTypeByQualifiedName(typeName),
                                  values,
                                  serviceProvider);
        }

        public static T CreateInstance<T>() {
            return (T) CreateInstance(typeof(T), null, null);
        }

        public static T CreateInstance<T>(IServiceProvider serviceProvider) {
            return (T) CreateInstance(typeof(T), null, serviceProvider);
        }

        public static T CreateInstance<T>(Type type,
                                          IEnumerable<KeyValuePair<string, object>> values,
                                          IServiceProvider serviceProvider,
                                          params Attribute[] attributes) {
            return (T) CreateInstance(type, values, serviceProvider, attributes);
        }

        public static T CreateInstance<T>(Type type,
                                          IServiceProvider serviceProvider) {
            return (T) CreateInstance(type, null, serviceProvider);
        }

        public static T CreateInstance<T>(Type type) {
            return (T) CreateInstance(type, null, null, null);
        }

        public static T CreateInstance<T>(IEnumerable<KeyValuePair<string, object>> values,
                                          IServiceProvider serviceProvider,
                                          params Attribute[] attributes) {
            return (T) CreateInstance(typeof(T), values, serviceProvider, attributes);
        }

        public static T CreateInstance<T>(IEnumerable<KeyValuePair<string, object>> values) {
            return (T) CreateInstance(typeof(T), values, null, null);
        }

        public static T CreateInstance<T>(this IActivationFactory factory) {
            return (T) CreateInstance(factory: factory, type: typeof(T));
        }

        public static T CreateInstance<T>(this IActivationFactory factory, IServiceProvider serviceProvider) {
            return (T) CreateInstance(factory: factory, type: typeof(T), serviceProvider: serviceProvider);
        }

        static IActivationFactory GetDefaultFactory(IActivationFactory factory, IServiceProvider serviceProvider) {
            serviceProvider = (serviceProvider ?? ServiceProvider.Null);
            return serviceProvider.GetServiceOrDefault(factory ?? ActivationFactory.Default);
        }

        public static T CreateInstance<T>(this IActivationFactory factory,
                                          Type type,
                                          IEnumerable<KeyValuePair<string, object>> values = null,
                                          IServiceProvider serviceProvider = null,
                                          params Attribute[] attributes) {

            factory = GetDefaultFactory(factory, serviceProvider);
            return (T) factory.CreateInstance(type, values, serviceProvider, attributes);
        }

        public static object CreateInstance(this IActivationFactory factory,
                                            Type type,
                                            IEnumerable<KeyValuePair<string, object>> values = null,
                                            IServiceProvider serviceProvider = null,
                                            params Attribute[] attributes) {
            factory = GetDefaultFactory(factory, serviceProvider);
            return factory.CreateInstance(type, values, serviceProvider, attributes);
        }

        public static object Build(Type type,
                                   IEnumerable<KeyValuePair<string, object>> values = null,
                                   IServiceProvider serviceProvider = null,
                                   params Attribute[] attributes) {

            return ActivationFactory.Build.CreateInstance(type, values, serviceProvider, attributes);
        }

        public static T Build<T>(IEnumerable<KeyValuePair<string, object>> values = null,
                                 IServiceProvider serviceProvider = null,
                                 params Attribute[] attributes) {

            return (T) ActivationFactory.Build.CreateInstance(typeof(T), values, serviceProvider, attributes);
        }

        public static object CreateInstance(Type type,
                                            IEnumerable<KeyValuePair<string, object>> values = null,
                                            IServiceProvider serviceProvider = null,
                                            params Attribute[] attributes) {

            var factory = GetDefaultFactory(null, serviceProvider);
            return factory.CreateInstance(type, values, serviceProvider, attributes);
        }

        public static void Initialize(object component,
                                      IEnumerable<KeyValuePair<string, object>> values,
                                      IServiceProvider serviceProvider = null) {

            ActivationFactory af = (ActivationFactory) ActivationFactory.Default;
            af.InitializeInternal(component, values, serviceProvider);
        }

        // TODO Needs work -- we want there to be as much fallback as
        // possible with parsing and stream loading.  Also probably
        // get encoding from content type

        public static object FromFile(Type componentType, string fileName) {
            if (componentType == null) {
                throw new ArgumentNullException("componentType");
            }
            if (fileName == null) {
                throw new ArgumentNullException("fileName");
            }
            if (fileName.Length == 0) {
                throw Failure.EmptyString("fileName");
            }

            return FromStreamContext(componentType, StreamContext.FromFile(fileName));
        }

        public static T FromFile<T>(string fileName) {
            return (T) FromFile(typeof(T), fileName);
        }

        public static T FromStream<T>(Stream stream, Encoding encoding = null) {
            if (stream == null)
                throw new ArgumentNullException("stream");

            return (T) FromStream(typeof(T), stream, encoding);
        }

        public static object FromStream(Type instanceType, Stream stream, Encoding encoding = null) {
            if (instanceType == null) {
                throw new ArgumentNullException("instanceType");
            }
            if (stream == null) {
                throw new ArgumentNullException("stream");
            }

            return FromStreamContext(instanceType, StreamContext.FromStream(stream), encoding);
        }

        public static T FromSource<T>(Uri uri) {
            return (T) FromSource(typeof(T), uri);
        }

        public static object FromSource(Type componentType, Uri uri) {
            if (componentType == null) {
                throw new ArgumentNullException("componentType");
            }
            if (uri == null) {
                throw new ArgumentNullException("uri");
            }

            return FromStreamContext(componentType, StreamContext.FromSource(uri));
        }

        public static T FromStreamContext<T>(StreamContext streamContext) {
            return (T) FromStreamContext(typeof(T), streamContext);
        }

        public static object FromStreamContext(Type componentType,
                                               StreamContext streamContext,
                                               Encoding encoding = null) {
            if (componentType == null) {
                throw new ArgumentNullException("componentType");
            }
            if (streamContext == null) {
                throw new ArgumentNullException("streamContext");
            }

            StreamingSource ss = StreamingSource.Create(
                componentType,
                streamContext.ContentType,
                streamContext.Extension
            );
            if (ss == null) {
                throw RuntimeFailure.NoAcceptableStreamingSource(componentType);
            }
            if (ss is TextSource text) {
                text.Encoding = encoding;
            }

            return ss.Load(streamContext, componentType);
        }

        public static T FromText<T>(string text, CultureInfo culture = null, IServiceProvider serviceProvider = null) {
            return (T) FromText(typeof(T), text, culture, serviceProvider);
        }

        public static object FromText(Type componentType, string text, CultureInfo culture = null, IServiceProvider serviceProvider = null) {
            if (componentType == null) {
                throw new ArgumentNullException("componentType");
            }

            object result;
            if (Activation.GetTextConversion(componentType)
                .TryConvertFromText(text, componentType, serviceProvider ?? ServiceProvider.Current, culture, out result)) {
                return result;

            } else {
                throw Failure.NotParsable("text", componentType);
            }
        }

        static T CreateInstanceSafe<T>(ITemplate temp, string name) {
            if (temp == null) {
                throw RuntimeFailure.TemplateNotFound(name);
            }
            var result = Activation.CreateInstance<T>();
            temp.Apply(result);
            return result;
        }

        public static T FromProvider<T>(string name,
                                        IEnumerable<KeyValuePair<string, object>> values = null,
                                        IServiceProvider serviceProvider = null) {
            if (name == null) {
                throw new ArgumentNullException("name");
            }
            if (name.Length == 0) {
                throw Failure.EmptyString("name");
            }

            return (T) ProviderData.GetProvidersByLocalName(typeof(T), name,
                                                            t => t.Activate(values, serviceProvider))
                .SingleOrThrow(RuntimeFailure.MultipleProviders);
        }

        public static T FromProvider<T>(QualifiedName name,
                                        IEnumerable<KeyValuePair<string, object>> values = null,
                                        IServiceProvider serviceProvider = null) {
            if (name == null) {
                throw new ArgumentNullException("name");
            }

            return (T) App.DescribeProviders().GetProviderInfo(typeof(T), name).Activate(values, serviceProvider);
        }

        public static T FromTemplate<T>(string name) {
            ITemplate temp = Template.FromName(typeof(T), name);
            return CreateInstanceSafe<T>(temp, name);
        }

        public static T FromTemplate<T>(QualifiedName name) {
            ITemplate temp = Template.FromName(typeof(T), name);
            return CreateInstanceSafe<T>(temp, name.ToString());
        }

        public static object FromTemplate(Type componentType, string name) {
            ITemplate temp = Template.FromName(componentType, name);
            return CreateAndApply(componentType, temp);
        }

        public static object FromTemplate(Type componentType, QualifiedName name) {
            ITemplate temp = Template.FromName(componentType, name);
            return CreateAndApply(componentType, temp);
        }

        static object CreateAndApply(Type componentType, ITemplate temp) {
            var result = ActivationFactory.Default.CreateInstance(componentType);
            temp.Apply(result);
            return result;
        }
    }
}
