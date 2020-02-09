//
// Copyright 2005, 2006, 2010, 2016, 2019 Carbonfrost Systems, Inc.
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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Carbonfrost.Commons.Core.Runtime {

    partial class PropertyProvider {

        public static readonly IPropertyProvider Null = Properties.Null;

        public static IPropertyProvider LateBound(TypeReference type) {
            if (type == null) {
                throw new ArgumentNullException("type");
            }

            return new LateBoundPropertyProvider(type);
        }

        public static IPropertyProvider Compose(IEnumerable<IPropertyProvider> providers) {
            return ComposeCore(providers);
        }

        public static IPropertyProvider Compose(IEnumerable<KeyValuePair<string, IPropertyProvider>> providers) {
            if (providers == null) {
                throw new ArgumentNullException("providers"); // $NON-NLS-1
            }

            return new NamespacePropertyProvider(providers);
        }

        public static IPropertyProvider Compose(
            IEnumerable<KeyValuePair<string, object>> propertyProviders) {
            if (propertyProviders == null) {
                throw new ArgumentNullException("propertyProviders");
            }

            return Compose(propertyProviders.Select(
                s => new KeyValuePair<string, IPropertyProvider>(s.Key, PropertyProvider.FromValue(s.Value))));
        }

        public static IPropertyProvider FromFactory(Func<IPropertyProvider> factory) {
            if (factory == null) {
                throw new ArgumentNullException("factory");
            }
            return new ThunkPropertyProvider(factory);
        }

        public static IPropertyProvider FromFactory(Func<IPropertiesContainer> factory) {
            if (factory == null) {
                throw new ArgumentNullException("factory");
            }
            Func<IPropertyProvider> _valueFactory = () => {
                var result = factory();
                if (result == null) {
                    return PropertyProvider.Null;
                }
                return result.Properties;
            };
            return FromFactory(_valueFactory);
        }

        public static IPropertyProvider Compose(params IPropertyProvider[] providers) {
            return ComposeCore(providers);
        }

        public static IPropertyProvider FromValue(object context) {
            if (context == null) {
                return PropertyProvider.Null;
            }

            IPropertyProvider pp = context as IPropertyProvider;
            if (pp != null) {
                return pp;
            }

            return Compose(
                ReflectionPropertyProviderUsingIndexer.TryCreate(context),
                new ReflectionPropertyProvider(context)
            );
        }

        public static IPropertyProvider FromArray(params object[] values) {
            if (values == null || values.Length == 0) {
                return PropertyProvider.Null;
            }

            return new ArrayPropertyProvider(values);
        }

        public static string Format(string format, object args) {
            if (string.IsNullOrEmpty(format)) {
                return string.Empty;
            }

            if (args == null) {
                return format;
            }

            return Format(format, (IPropertyProvider) Properties.FromValue(args));
        }

        public static string Format(string format,
                                    IPropertyProvider propertyProvider) {

            if (string.IsNullOrEmpty(format)) {
                return string.Empty;
            }
            var ppf = PropertyProviderFormat.Parse(format);
            return ppf.Format(propertyProvider);
        }

        public static string Format(string format,
                                    IEnumerable<KeyValuePair<string, object>> propertyProviders) {
            if (string.IsNullOrEmpty(format)) {
                return string.Empty;
            }

            return Format(format, PropertyProvider.Compose(propertyProviders));
        }

        static IPropertyProvider ComposeCore(IEnumerable<IPropertyProvider> providers) {
            return Utility.OptimalComposite(providers,
                                            t => new CompositePropertyProvider(t),
                                            Null);
        }
    }
}
