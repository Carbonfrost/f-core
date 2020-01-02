//
// Copyright 2019 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using System.Reflection;
using System.Security;

using Carbonfrost.Commons.Core.Resources;

namespace Carbonfrost.Commons.Core.Runtime {

    static class RuntimeFailure {

        public static ArgumentException TemplateDoesNotSupportOperand(string argumentName) {
            return Failure.Prepare(new ArgumentException(SR.TemplateDoesNotSupportOperand(), argumentName));
        }

        public static ArgumentException PropertyNotFound(string propertyNameArgument, string propertyName) {
            return Failure.Prepare(new ArgumentException(SR.PropertyNotFound(propertyName), propertyNameArgument));
        }

        public static FormatException NotValidHexString() {
            return Failure.Prepare(new FormatException(SR.NotValidHexString()));
        }

        public static NotSupportedException SeekNotSupportedByBase() {
            return Failure.Prepare(new NotSupportedException(SR.SeekNotSupportedByBase()));
        }

        public static ArgumentOutOfRangeException SeekNegativeBegin(string argName, long offset) {
            return Failure.Prepare(new ArgumentOutOfRangeException(argName, offset, SR.CannotBeNegative())); // $NON-NLS-1
         }

        public static ArgumentException CannotExpandPrefixNotFound(string prefix) {
            prefix = string.IsNullOrEmpty(prefix) ? SR.Empty() : prefix;
            return Failure.Prepare(new ArgumentException(SR.CannotExpandPrefixNotFound(prefix)));
        }

        public static ArgumentException CannotActivateNoConstructor(string argName, Type type) {
            return Failure.Prepare(new ArgumentException(SR.CannotActivateNoConstructor(type), argName));
        }

        public static ArgumentException CannotActivateNoConstructorOrBuilder(string argName, Type type) {
            return Failure.Prepare(new ArgumentException(SR.CannotActivateNoConstructorOrBuilder(type), argName));
        }

        public static UriFormatException NotValidDataUri() {
            return Failure.Prepare(new UriFormatException(SR.NotValidDataUri()));
        }

        public static FormatException ExpectedIdentifier() {
            return Failure.Prepare(new FormatException(SR.ExpectedIdentifier()));
        }

        public static FormatException ExpectedRightBrace() {
            return Failure.Prepare(new FormatException(SR.ExpectedRightBrace()));
        }

        public static FormatException IncompleteEscapeSequence() {
            return Failure.Prepare(new FormatException(SR.IncompleteEscapeSequence()));
        }

        public static FormatException PropertiesParseKeyNameExpected() {
            return Failure.Prepare(new FormatException(SR.PropertiesParseKeyNameExpected()));
        }

        public static InvalidOperationException PropertyMissing(string key) {
            return Failure.Prepare(new InvalidOperationException(SR.PropertyMissing(key)));
        }

        public static InvalidOperationException PropertiesCategoryMissingBrackets() {
            return Failure.Prepare(new InvalidOperationException(SR.PropertiesCategoryMissingBrackets()));
        }

        public static ArgumentException NotValidLocalName(string argName) {
            return Failure.Prepare(new ArgumentException(SR.NotValidLocalName(), argName));
        }

        public static ArgumentException ForbiddenStreamStreamContext() {
            return Failure.Prepare(new ArgumentException(SR.ForbiddenStreamStreamContext()));
        }

        public static InvalidOperationException TypeMissing(TypeReference tr) {
            return Failure.Prepare(new InvalidOperationException(SR.TypeMissing(tr)));
        }

        public static TypeLoadException TypeMissingFromQualifiedName(QualifiedName name) {
            return Failure.Prepare(new TypeLoadException(SR.TypeMissingFromQualifiedName(name)));
        }

        public static ArgumentException NoAcceptableStreamingSource(Type instanceType) {
            return Failure.Prepare(new ArgumentException(SR.NoAcceptableStreamingSource(instanceType)));
        }

        public static ArgumentOutOfRangeException ContentTypeNotStandard(string argumentName, string argumentType) {
            return Failure.Prepare(new ArgumentOutOfRangeException(argumentName, argumentType, SR.ContentTypeNotStandard()));
        }

        public static InvalidOperationException PropertyDeclarationMissingKey() {
            return Failure.Prepare(new InvalidOperationException(SR.PropertyDeclarationMissingKey()));
        }

        public static AmbiguousMatchException MultipleNamespaces() {
            return Failure.Prepare(new AmbiguousMatchException(SR.MultipleNamespaces()));
        }

        public static AmbiguousMatchException MultipleProviders() {
            return Failure.Prepare(new AmbiguousMatchException(SR.MultipleProviders()));
        }

        public static AmbiguousMatchException MultipleProviderTypes() {
            return Failure.Prepare(new AmbiguousMatchException(SR.MultipleProviderTypes()));
        }

        public static InvalidOperationException ServiceNotFound(Type type, string name) {
            string text = string.Join("/", type, name);
            return Failure.Prepare(new InvalidOperationException(SR.ServiceNotFound(text)));
        }

        public static InvalidOperationException CannotBuildTagUri() {
            return Failure.Prepare(new InvalidOperationException(SR.CannotBuildTagUri()));
        }

        public static InvalidOperationException TemplateNotFound(string templateName) {
            return Failure.Prepare(new InvalidOperationException(SR.TemplateNotFound(templateName)));
        }

        public static FormatException ConcreteClassError(Type baseType) {
            return Failure.Prepare(new FormatException(SR.ConcreteClassError(baseType)));
        }

        public static ArgumentOutOfRangeException UseProviderRegistrationAttributeOverload(
            string argumentName, ProviderRegistrationType type) {

            return Failure.Prepare(new ArgumentOutOfRangeException(argumentName, type, SR.UseProviderRegistrationAttributeOverload()));
        }

        public static ArgumentException QualifiedNameCannotBeGeneratedFromConstructed(string argumentName) {
            return Failure.Prepare(new ArgumentException(SR.QualifiedNameCannotBeGeneratedFromConstructed(), argumentName));
        }

        public static ArgumentException InvalidProviderInstanceType(string argumentName) {
            return Failure.Prepare(new ArgumentException(SR.InvalidProviderInstanceType(), argumentName));
        }

        public static ArgumentException InvalidProviderFieldOrMethod(string argumentName) {
            return Failure.Prepare(new ArgumentException(SR.InvalidProviderFieldOrMethod(), argumentName));
        }

        public static NotSupportedException NoAvailableTextConversion(Type componentType) {
            return Failure.Prepare(new NotSupportedException(SR.NoAvailableTextConversion(componentType)));
        }

        public static ArgumentException CannotSpecifyNullKey(string argumentName) {
            return Failure.Prepare(new ArgumentException(SR.CannotSpecifyNullKey(), argumentName));
        }

        public static InvalidOperationException CannotWriteToStream() {
            return Failure.Prepare(new InvalidOperationException(SR.CannotWriteToStream()));
        }

        public static ArgumentException ThisArgumentIncorrectType(object type) {
            return Failure.Prepare(new ArgumentException(SR.ThisArgumentIncorrectType(type)));
        }

        public static ArgumentException ApplyPropertiesStaticMethodRequiresArg(string argumentName) {
            return Failure.Prepare(new ArgumentException(SR.ApplyPropertiesStaticMethodRequiresArg(), argumentName));
        }

        public static InvalidOperationException ProviderNotFound(Type type) {
            return Failure.Prepare(new InvalidOperationException(SR.ProviderNotFound(type)));
        }

        public static ArgumentException AdapterRoleNotDefined(string argumentName, string argumentValue) {
            return Failure.Prepare(new ArgumentException(SR.AdapterRoleNotDefined(argumentValue), argumentName));
        }

        public static ArgumentException DataProviderTypeOrValueNotBoth() {
            return Failure.Prepare(new ArgumentException(SR.DataProviderTypeOrValueNotBoth()));
        }
    }
}
