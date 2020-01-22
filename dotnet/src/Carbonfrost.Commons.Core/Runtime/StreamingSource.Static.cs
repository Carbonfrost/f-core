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

namespace Carbonfrost.Commons.Core.Runtime {

    [Providers]
    public abstract partial class StreamingSource {

        public static readonly StreamingSource XmlFormatter;
        public static readonly StreamingSource Text;

        [StreamingSourceUsage(ContentTypes = "text/x-properties|text/x-ini", Extensions = ".ini; .conf; .cfg; .properties")]
        public static readonly StreamingSource Properties;

        static StreamingSource() {
            Text = new TextStreamingSource();
            XmlFormatter = new XmlFormatterStreamingSource();
            Properties = new PropertiesStreamingSource();
        }

        public static StreamingSource FromName(string name) {
            return App.GetProvider<StreamingSource>(name);
        }

        public static StreamingSource FromName(QualifiedName name) {
            return App.GetProvider<StreamingSource>(name);
        }

        public static StreamingSource FromCriteria(object criteria) {
            return App.GetProvider<StreamingSource>(criteria);
        }

        public static StreamingSource Create(KnownStreamingSource knownSource) {
            return (StreamingSource) Activator.CreateInstance(GetStreamingSourceType(knownSource));
        }

        public static StreamingSource Create(Type componentType,
                                             ContentType contentType = null,
                                             string extension = null,
                                             IServiceProvider serviceProvider = null) {

            componentType = componentType ?? typeof(object);

            Type result = componentType.GetAdapterType(AdapterRole.StreamingSource);
            if (result != null) {
                return Activation.CreateInstance<StreamingSource>(result, serviceProvider);
            }

            return App.GetProvider<StreamingSource>(new { ContentType = contentType, OutputType = componentType, Extension = extension });
        }

        public static Type GetStreamingSourceType(KnownStreamingSource knownStreamingSource) {
            switch (knownStreamingSource) {
                case KnownStreamingSource.XmlFormatter:
                    return typeof(XmlFormatterStreamingSource);

                case KnownStreamingSource.Properties:
                    return typeof(PropertiesStreamingSource);

                case KnownStreamingSource.Text:
                    return typeof(TextStreamingSource);
                default:
                    return null;
            }
        }

    }
}
