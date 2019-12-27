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

namespace Carbonfrost.Commons.Core.Runtime {

    public struct Template<T> : ITemplate, ITemplateWrapper {

        private readonly ITemplate _source;

        public Template(T initializer)
            : this(Template.Create(initializer)) {
        }

        public Template(Action<T> initializer)
            : this(Template.Create(initializer)) {
        }

        public Template(ITemplate source) {
            if (source == null) {
                throw new ArgumentNullException("source");
            }
            _source = source;
        }

        public void Apply(object value) {
            if (value is T) {
                _source.Apply(value);
            } else {
                throw RuntimeFailure.TemplateDoesNotSupportOperand("value");
            }
        }

        public T CreateInstance() {
            return CreateInstance(null);
        }

        public T CreateInstance(IActivationFactory factory) {
            var result = (factory ?? ActivationFactory.Default).CreateInstance<T>();
            _source.Apply(result);
            return result;
        }

        ITemplate ITemplateWrapper.InnerTemplate {
            get {
                return _source;
            }
        }
    }
}
