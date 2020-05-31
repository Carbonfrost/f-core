//
// Copyright 2005, 2006, 2010, 2016, 2019-2020 Carbonfrost Systems, Inc.
// (https://carbonfrost.com)
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
using System.Collections.Generic;
using System.Linq;

namespace Carbonfrost.Commons.Core.Runtime {

    partial class PropertyProvider {

        public static readonly IPropertyProvider Null = Properties.Null;

        public static IPropertyProvider LateBound(TypeReference type) {
            if (type == null) {
                throw new ArgumentNullException(nameof(type));
            }

            return new LateBoundPropertyProvider(type);
        }

        public static IPropertyProvider Dynamic(dynamic obj) {
            if (obj == null) {
                return Null;
            }
            return new DynamicPropertyProvider(obj);
        }

        public static IPropertyProvider Compose(IEnumerable<IPropertyProvider> providers) {
            return ComposeCore(providers);
        }

        public static IPropertyProvider Compose(IEnumerable<KeyValuePair<string, IPropertyProvider>> providers) {
            if (providers == null) {
                throw new ArgumentNullException(nameof(providers));
            }

            return new NamespacePropertyProvider(providers);
        }

        public static IPropertyProvider Compose(
            IEnumerable<KeyValuePair<string, object>> propertyProviders) {
            if (propertyProviders == null) {
                throw new ArgumentNullException(nameof(propertyProviders));
            }

            return Compose(propertyProviders.Select(
                s => new KeyValuePair<string, IPropertyProvider>(s.Key, PropertyProvider.FromValue(s.Value))));
        }

        public static IPropertyProvider Compose(params IPropertyProvider[] providers) {
            return ComposeCore(providers);
        }

        public static IPropertyProvider Except(IPropertyProvider propertyProvider, params string[] properties) {
            return Except(propertyProvider, (IEnumerable<string>) properties);
        }

        public static IPropertyProvider Except(IPropertyProvider propertyProvider, IEnumerable<string> properties) {
            var props = new HashSet<string>(properties, StringComparer.OrdinalIgnoreCase);
            return Filter(propertyProvider, p => !props.Contains(p));
        }

        public static IPropertyProvider Filter(IPropertyProvider propertyProvider, Func<string, bool> propertyFilter) {
            if (propertyProvider == null) {
                throw new ArgumentNullException(nameof(propertyProvider));
            }
            if (propertyFilter == null) {
                throw new ArgumentNullException(nameof(propertyFilter));
            }
            return new FilterPropertyProvider(propertyProvider, propertyFilter);
        }

        public static IPropertyProvider FromFactory(Func<IPropertyProvider> factory) {
            if (factory == null) {
                throw new ArgumentNullException(nameof(factory));
            }
            return new ThunkPropertyProvider(factory);
        }

        public static IPropertyProvider FromFactory(Func<IPropertiesContainer> factory) {
            if (factory == null) {
                throw new ArgumentNullException(nameof(factory));
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
