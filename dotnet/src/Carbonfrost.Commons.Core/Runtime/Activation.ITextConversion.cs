//
// Copyright 2015, 2019 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using System.Linq;
using System.Reflection;

namespace Carbonfrost.Commons.Core.Runtime {

    partial class Activation {

        private static readonly Dictionary<Type, ITextConversion> _textConversionCache
            = new Dictionary<Type, ITextConversion>() {
            { typeof(string), new StringNopConversion() },
            { typeof(Uri), new UriConversion() },
        };

        private static readonly ITextConversion _nullTextConversion = new NullTextConversion();

        internal static ITextConversion GetTextConversion(Type componentType) {
            return _textConversionCache.GetValueOrCache(componentType,
                                                        () => GetTryParseMethod(componentType)
                                                        ?? GetParseMethod(componentType)
                                                        ?? GetStreamingSource(componentType)
                                                        ?? _nullTextConversion);
            // TODO Probe for IValueSerializer
        }

        static ITextConversion GetTryParseMethod(Type type) {
            MethodInfo parse = type.GetTryParseMethod();

            if (parse != null) {
                return new TryParseMethodConversion(parse);
            }
            return null;

        }

        static ITextConversion GetParseMethod(Type type) {
            MethodInfo parse = type.GetParseMethod();

            if (parse != null) {
                return new ParseMethodConversion(parse);
            }
            return null;
        }

        static ITextConversion GetStreamingSource(Type type) {
            if (StreamingSource.Create(type) == null) {
                return null;
            }

            return StreamingSourceConversion.Instance;
        }

        internal interface ITextConversion {
            bool TryConvertFromText(string text, Type componentType, IServiceProvider sp, CultureInfo culture, out object result);
        }

        sealed class StreamingSourceConversion : ITextConversion {

            public static readonly StreamingSourceConversion Instance = new StreamingSourceConversion();

            public bool TryConvertFromText(string text, Type componentType, IServiceProvider sp, CultureInfo culture, out object result) {
                StreamingSource ss = StreamingSource.Create(componentType);
                result = null;
                if (ss == null)
                    return false;

                TextSource ts = ss as TextSource;
                if (ts == null)
                    result = ss.Load(StreamContext.FromText(text), componentType);
                else
                    result = ts.Load(new StringReader(text), componentType);
                return true;
            }
        }

        sealed class ParseMethodConversion : ITextConversion {

            private readonly MethodInfo _parse;

            public ParseMethodConversion(MethodInfo parse) {
                _parse = parse;
            }

            public bool TryConvertFromText(string text, Type componentType, IServiceProvider sp, CultureInfo culture, out object result) {
                object[] parms = _parse.GetParameters().Select<ParameterInfo, object>(
                    p => {
                        Type pt = p.ParameterType;
                        if (typeof(IServiceProvider).Equals(pt))
                            return sp;

                        if (typeof(CultureInfo).Equals(pt))
                            return culture;

                        if (typeof(string).Equals(pt))
                            return text;
                        else
                            return null;
                    }).ToArray();

                result = _parse.Invoke(null, parms);
                return true;
            }
        }

        sealed class TryParseMethodConversion : ITextConversion {

            private readonly MethodInfo _parse;

            public TryParseMethodConversion(MethodInfo parse) {
                _parse = parse;
            }

            public bool TryConvertFromText(string text, Type componentType, IServiceProvider sp, CultureInfo culture, out object result) {
                int outParameter = -1;
                object[] parms = _parse.GetParameters().Select<ParameterInfo, object>(
                    p => {
                        if (p.IsOut) {
                            outParameter = p.Position;
                        }

                        Type pt = p.ParameterType;
                        if (typeof(IServiceProvider).Equals(pt))
                            return sp;

                        if (typeof(CultureInfo).Equals(pt))
                            return culture;

                        if (typeof(string).Equals(pt))
                            return text;
                        else
                            return null;
                    }).ToArray();

                bool answer = (bool) _parse.Invoke(null, parms);
                result = parms[outParameter];
                return answer;
            }
        }

        sealed class NullTextConversion : ITextConversion {

            public bool TryConvertFromText(string text, Type componentType, IServiceProvider sp, CultureInfo culture, out object result) {
                result = null;
                throw RuntimeFailure.NoAvailableTextConversion(componentType);
            }
        }

        sealed class StringNopConversion : ITextConversion {

            public bool TryConvertFromText(string text, Type componentType, IServiceProvider sp, CultureInfo culture, out object result) {
                result = text;
                return true;
            }
        }

        sealed class UriConversion : ITextConversion {

            public bool TryConvertFromText(string text, Type componentType, IServiceProvider sp, CultureInfo culture, out object result) {
                Uri uri;
                if (Uri.TryCreate(text, UriKind.RelativeOrAbsolute, out uri)) {
                    result = uri;
                    return true;
                }
                result = null;
                return false;
            }
        }
    }
}
