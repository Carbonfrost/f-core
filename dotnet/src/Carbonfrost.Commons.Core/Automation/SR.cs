
// This file was automatically generated.  DO NOT EDIT or else
// your changes could be lost!

#pragma warning disable 1570

using System;
using System.Globalization;
using System.Resources;
using System.Reflection;

namespace Carbonfrost.Commons.Core.Resources {

    /// <summary>
    /// Contains strongly-typed string resources.
    /// </summary>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("srgen", "1.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute]
    internal static partial class SR {

        private static global::System.Resources.ResourceManager _resources;
        private static global::System.Globalization.CultureInfo _currentCulture;
        private static global::System.Func<string, string> _resourceFinder;

        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(_resources, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Carbonfrost.Commons.Core.Automation.SR", typeof(SR).GetTypeInfo().Assembly);
                    _resources = temp;
                }
                return _resources;
            }
        }

        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return _currentCulture;
            }
            set {
                _currentCulture = value;
            }
        }

        private static global::System.Func<string, string> ResourceFinder {
            get {
                if (object.ReferenceEquals(_resourceFinder, null)) {
                    try {
                        global::System.Resources.ResourceManager rm = ResourceManager;
                        _resourceFinder = delegate (string s) {
                            return rm.GetString(s);
                        };
                    } catch (global::System.Exception ex) {
                        _resourceFinder = delegate (string s) {
                            return string.Format("localization error! {0}: {1} ({2})", s, ex.GetType(), ex.Message);
                        };
                    }
                }
                return _resourceFinder;
            }
        }


  /// <summary>Specified adapter role name is not defined: ${role}</summary>
    internal static string AdapterRoleNotDefined(
    object @role
    ) {
        return string.Format(Culture, ResourceFinder("AdapterRoleNotDefined") , @role);
    }

  /// <summary>The argument cannot be the empty string or all whitespace.</summary>
    internal static string AllWhitespace(
    
    ) {
        return string.Format(Culture, ResourceFinder("AllWhitespace") );
    }

  /// <summary>The instance is already initialized.</summary>
    internal static string AlreadyInitialized(
    
    ) {
        return string.Format(Culture, ResourceFinder("AlreadyInitialized") );
    }

  /// <summary>Can't use the specified static method because it must have at least one parameter for use as the 'this' argument.</summary>
    internal static string ApplyPropertiesStaticMethodRequiresArg(
    
    ) {
        return string.Format(Culture, ResourceFinder("ApplyPropertiesStaticMethodRequiresArg") );
    }

  /// <summary>Cannot activate the type `${type}' because it has no available activation constructor.</summary>
    internal static string CannotActivateNoConstructor(
    object @type
    ) {
        return string.Format(Culture, ResourceFinder("CannotActivateNoConstructor") , @type);
    }

  /// <summary>Cannot activate the type `${type}' because it has no available activation constructor and no builder defined.</summary>
    internal static string CannotActivateNoConstructorOrBuilder(
    object @type
    ) {
        return string.Format(Culture, ResourceFinder("CannotActivateNoConstructorOrBuilder") , @type);
    }

  /// <summary>The specified argument cannot be negative.</summary>
    internal static string CannotBeNegative(
    
    ) {
        return string.Format(Culture, ResourceFinder("CannotBeNegative") );
    }

  /// <summary>The value for the argument cannot be positive or zero.</summary>
    internal static string CannotBeNonNegative(
    
    ) {
        return string.Format(Culture, ResourceFinder("CannotBeNonNegative") );
    }

  /// <summary>The specified argument cannot be negative or zero.</summary>
    internal static string CannotBeNonPositive(
    
    ) {
        return string.Format(Culture, ResourceFinder("CannotBeNonPositive") );
    }

  /// <summary>The value for the argument cannot be positive.</summary>
    internal static string CannotBePositive(
    
    ) {
        return string.Format(Culture, ResourceFinder("CannotBePositive") );
    }

  /// <summary>The specified argument cannot be equal to zero.</summary>
    internal static string CannotBeZero(
    
    ) {
        return string.Format(Culture, ResourceFinder("CannotBeZero") );
    }

  /// <summary>Cannot build a tag URI from this instance.  The instance isn't a URL with a valid path and query.</summary>
    internal static string CannotBuildTagUri(
    
    ) {
        return string.Format(Culture, ResourceFinder("CannotBuildTagUri") );
    }

  /// <summary>Cannot expand qualified name.  The prefix `${prefix}' was not found.</summary>
    internal static string CannotExpandPrefixNotFound(
    object @prefix
    ) {
        return string.Format(Culture, ResourceFinder("CannotExpandPrefixNotFound") , @prefix);
    }

  /// <summary>The position of the stream cannot be seeked or set because the stream does not support the operation.</summary>
    internal static string CannotSeekOrSetPosition(
    
    ) {
        return string.Format(Culture, ResourceFinder("CannotSeekOrSetPosition") );
    }

  /// <summary>The key in the key-value pair must not have a null key.</summary>
    internal static string CannotSpecifyNullKey(
    
    ) {
        return string.Format(Culture, ResourceFinder("CannotSpecifyNullKey") );
    }

  /// <summary>Stream is read-only; cannot write to stream.</summary>
    internal static string CannotWriteToStream(
    
    ) {
        return string.Format(Culture, ResourceFinder("CannotWriteToStream") );
    }

  /// <summary>The instance has been closed.</summary>
    internal static string Closed(
    
    ) {
        return string.Format(Culture, ResourceFinder("Closed") );
    }

  /// <summary>An empty collection is not valid for this argument.</summary>
    internal static string CollectionCannotBeEmpty(
    
    ) {
        return string.Format(Culture, ResourceFinder("CollectionCannotBeEmpty") );
    }

  /// <summary>Cannot copy the collection contents to a multidimensional array.</summary>
    internal static string CollectionCannotCopyToMultidimensionalArray(
    
    ) {
        return string.Format(Culture, ResourceFinder("CollectionCannotCopyToMultidimensionalArray") );
    }

  /// <summary>An element in the collection is null.</summary>
    internal static string CollectionContainsNullElement(
    
    ) {
        return string.Format(Culture, ResourceFinder("CollectionContainsNullElement") );
    }

  /// <summary>The specified count must be greater than or equal to 0 and less than the total number of items in the collection less the index offset (${lowerBound} to ${upperBound}, inclusive).</summary>
    internal static string CollectionCountOutOfRange(
    object @lowerBound, object @upperBound
    ) {
        return string.Format(Culture, ResourceFinder("CollectionCountOutOfRange") , @lowerBound, @upperBound);
    }

  /// <summary>The operation is not supported for collections that are fixed size.</summary>
    internal static string CollectionFixedSize(
    
    ) {
        return string.Format(Culture, ResourceFinder("CollectionFixedSize") );
    }

  /// <summary>The specified index must be between ${lowerBound} and ${upperBound}, inclusive.</summary>
    internal static string CollectionIndexOutOfRange(
    object @lowerBound, object @upperBound
    ) {
        return string.Format(Culture, ResourceFinder("CollectionIndexOutOfRange") , @lowerBound, @upperBound);
    }

  /// <summary>The specified index is out of range for the collection.</summary>
    internal static string CollectionIndexOutOfRangeNoElements(
    
    ) {
        return string.Format(Culture, ResourceFinder("CollectionIndexOutOfRangeNoElements") );
    }

  /// <summary>The types of the comparison operands do not match.</summary>
    internal static string ComparisonOperandsMustMatch(
    
    ) {
        return string.Format(Culture, ResourceFinder("ComparisonOperandsMustMatch") );
    }

  /// <summary>Unexpected result from the concrete class provider for `${type}`.  Concrete class result must extend the base type and may not be abstract or an interface.</summary>
    internal static string ConcreteClassError(
    object @type
    ) {
        return string.Format(Culture, ResourceFinder("ConcreteClassError") , @type);
    }

  /// <summary>Specified type is not one of the supported standard types for an Internet media type.</summary>
    internal static string ContentTypeNotStandard(
    
    ) {
        return string.Format(Culture, ResourceFinder("ContentTypeNotStandard") );
    }

  /// <summary>Either the type or value of the data provider must be specified, but not both.</summary>
    internal static string DataProviderTypeOrValueNotBoth(
    
    ) {
        return string.Format(Culture, ResourceFinder("DataProviderTypeOrValueNotBoth") );
    }

  /// <summary>Define provider ${name} ${providerType}</summary>
    internal static string DefineProvider(
    object @name, object @providerType
    ) {
        return string.Format(Culture, ResourceFinder("DefineProvider") , @name, @providerType);
    }

  /// <summary>Define root provider ${providerType}</summary>
    internal static string DefineRootProvider(
    object @providerType
    ) {
        return string.Format(Culture, ResourceFinder("DefineRootProvider") , @providerType);
    }

  /// <summary>Cannot utilize `${instanceName}' after the instance has been disposed.</summary>
    internal static string Disposed(
    object @instanceName
    ) {
        return string.Format(Culture, ResourceFinder("Disposed") , @instanceName);
    }

  /// <summary><empty></summary>
    internal static string Empty(
    
    ) {
        return string.Format(Culture, ResourceFinder("Empty") );
    }

  /// <summary>An empty collection is not valid for this argument.</summary>
    internal static string EmptyCollectionNotValid(
    
    ) {
        return string.Format(Culture, ResourceFinder("EmptyCollectionNotValid") );
    }

  /// <summary>The empty string is not valid for the argument.</summary>
    internal static string EmptyStringNotValid(
    
    ) {
        return string.Format(Culture, ResourceFinder("EmptyStringNotValid") );
    }

  /// <summary>Expected: 'identifier'.</summary>
    internal static string ExpectedIdentifier(
    
    ) {
        return string.Format(Culture, ResourceFinder("ExpectedIdentifier") );
    }

  /// <summary>Expected: '}'</summary>
    internal static string ExpectedRightBrace(
    
    ) {
        return string.Format(Culture, ResourceFinder("ExpectedRightBrace") );
    }

  /// <summary>The `stream:' protocol cannot be used as a stream context type.</summary>
    internal static string ForbiddenStreamStreamContext(
    
    ) {
        return string.Format(Culture, ResourceFinder("ForbiddenStreamStreamContext") );
    }

  /// <summary>The specified type cannot be used for the argument: ${argumentType}.</summary>
    internal static string ForbiddenType(
    object @argumentType
    ) {
        return string.Format(Culture, ResourceFinder("ForbiddenType") , @argumentType);
    }

  /// <summary>Unrecognized or incomplete escape sequence.</summary>
    internal static string IncompleteEscapeSequence(
    
    ) {
        return string.Format(Culture, ResourceFinder("IncompleteEscapeSequence") );
    }

  /// <summary>Invalid provider `${type}': ${ex}</summary>
    internal static string InvalidProviderDeclared(
    object @type, object @ex
    ) {
        return string.Format(Culture, ResourceFinder("InvalidProviderDeclared") , @type, @ex);
    }

  /// <summary>Given provider field or method is not valid because it is not static or has a return type that doesn't extend the provider base type.</summary>
    internal static string InvalidProviderFieldOrMethod(
    
    ) {
        return string.Format(Culture, ResourceFinder("InvalidProviderFieldOrMethod") );
    }

  /// <summary>Given provider instance type is not valid because it is abstract or doesn't extend the provider base type.</summary>
    internal static string InvalidProviderInstanceType(
    
    ) {
        return string.Format(Culture, ResourceFinder("InvalidProviderInstanceType") );
    }

  /// <summary>Assembly `${assembly}' specifies related assembly with the same name, which is not allowed.</summary>
    internal static string InvalidSelfRelatedAssembly(
    object @assembly
    ) {
        return string.Format(Culture, ResourceFinder("InvalidSelfRelatedAssembly") , @assembly);
    }

  /// <summary>The specified item already exists within the collection: ${itemExists}.</summary>
    internal static string ItemAlreadyExists(
    object @itemExists
    ) {
        return string.Format(Culture, ResourceFinder("ItemAlreadyExists") , @itemExists);
    }

  /// <summary>The item is required to exist in the collection.</summary>
    internal static string ItemRequiredToExistInCollection(
    
    ) {
        return string.Format(Culture, ResourceFinder("ItemRequiredToExistInCollection") );
    }

  /// <summary>The specified key or identifier, ${key}, cannot be added to the collection because it already exists.</summary>
    internal static string KeyAlreadyExists(
    object @key
    ) {
        return string.Format(Culture, ResourceFinder("KeyAlreadyExists") , @key);
    }

  /// <summary>Late-bound loading of a type `${typeName}' that was referenced by a component has failed.</summary>
    internal static string LateBoundTypeFailure(
    object @typeName
    ) {
        return string.Format(Culture, ResourceFinder("LateBoundTypeFailure") , @typeName);
    }

  /// <summary>The minimum value must be less than or equal to the maximum value.</summary>
    internal static string MinMustBeLessThanMax(
    
    ) {
        return string.Format(Culture, ResourceFinder("MinMustBeLessThanMax") );
    }

  /// <summary>The enumeration may not complete because the collection has been modified.</summary>
    internal static string Modified(
    
    ) {
        return string.Format(Culture, ResourceFinder("Modified") );
    }

  /// <summary>Multiple namespaces matched the given selection.</summary>
    internal static string MultipleNamespaces(
    
    ) {
        return string.Format(Culture, ResourceFinder("MultipleNamespaces") );
    }

  /// <summary>Multiple providers match the given criteria.</summary>
    internal static string MultipleProviders(
    
    ) {
        return string.Format(Culture, ResourceFinder("MultipleProviders") );
    }

  /// <summary>Provider implements more than one provider type.</summary>
    internal static string MultipleProviderTypes(
    
    ) {
        return string.Format(Culture, ResourceFinder("MultipleProviderTypes") );
    }

  /// <summary>The value is not-a-number, which is not valid for this argument.</summary>
    internal static string NaN(
    
    ) {
        return string.Format(Culture, ResourceFinder("NaN") );
    }

  /// <summary>No acceptable streaming source could be created for the required output type, ${type}.</summary>
    internal static string NoAcceptableStreamingSource(
    object @type
    ) {
        return string.Format(Culture, ResourceFinder("NoAcceptableStreamingSource") , @type);
    }

  /// <summary>No adapter can supply the conversion from the source type `${sourceType}' to the needed adapter role, `${adapterRole}'.</summary>
    internal static string NoAdapterForRole(
    object @sourceType, object @adapterRole
    ) {
        return string.Format(Culture, ResourceFinder("NoAdapterForRole") , @sourceType, @adapterRole);
    }

  /// <summary>No technique is available to convert type `${type}' from text.</summary>
    internal static string NoAvailableTextConversion(
    object @type
    ) {
        return string.Format(Culture, ResourceFinder("NoAvailableTextConversion") , @type);
    }

  /// <summary>The type `${argumentType}' cannot be assigned to the required type, `${requiredType}' because it is not a subclass or implementer of the required type.</summary>
    internal static string NotAssignableFrom(
    object @argumentType, object @requiredType
    ) {
        return string.Format(Culture, ResourceFinder("NotAssignableFrom") , @argumentType, @requiredType);
    }

  /// <summary>The specified identifier is not compliant.</summary>
    internal static string NotCompliantIdentifier2(
    
    ) {
        return string.Format(Culture, ResourceFinder("NotCompliantIdentifier2") );
    }

  /// <summary>Given the index, there is not enough space in the array.</summary>
    internal static string NotEnoughSpaceInArray(
    
    ) {
        return string.Format(Culture, ResourceFinder("NotEnoughSpaceInArray") );
    }

  /// <summary>The operation cannot continue in the current state because the instance has not been initialized.</summary>
    internal static string NotInitialized(
    
    ) {
        return string.Format(Culture, ResourceFinder("NotInitialized") );
    }

  /// <summary>The value is not an instance of the required type, `${requiredType}'.</summary>
    internal static string NotInstanceOf(
    object @requiredType
    ) {
        return string.Format(Culture, ResourceFinder("NotInstanceOf") , @requiredType);
    }

  /// <summary>The type `${argumentType}' is not a subclass of the required type, `${requiredType}'.</summary>
    internal static string NotSubclassOf(
    object @argumentType, object @requiredType
    ) {
        return string.Format(Culture, ResourceFinder("NotSubclassOf") , @argumentType, @requiredType);
    }

  /// <summary>The URI does not represent a valid data: URI.</summary>
    internal static string NotValidDataUri(
    
    ) {
        return string.Format(Culture, ResourceFinder("NotValidDataUri") );
    }

  /// <summary>Local name contains an illegal character.</summary>
    internal static string NotValidLocalName(
    
    ) {
        return string.Format(Culture, ResourceFinder("NotValidLocalName") );
    }

  /// <summary>The path `${pathText}' contains an invalid character or does not identify a supported path name.</summary>
    internal static string NotValidPathCharacter(
    object @pathText
    ) {
        return string.Format(Culture, ResourceFinder("NotValidPathCharacter") , @pathText);
    }

  /// <summary>The specified value is not within the defined values for the enumeration type, ${enumType}.</summary>
    internal static string NotWithinEnum(
    object @enumType
    ) {
        return string.Format(Culture, ResourceFinder("NotWithinEnum") , @enumType);
    }

  /// <summary>The given nullable argument must have a value.</summary>
    internal static string NullableMustHaveValue(
    
    ) {
        return string.Format(Culture, ResourceFinder("NullableMustHaveValue") );
    }

  /// <summary>Argument cannot be null or the empty string.</summary>
    internal static string NullOrEmptyStringNotValid(
    
    ) {
        return string.Format(Culture, ResourceFinder("NullOrEmptyStringNotValid") );
    }

  /// <summary>The value must be in the range ${min} to ${max}, inclusive.</summary>
    internal static string OutOfRangeInclusive(
    object @min, object @max
    ) {
        return string.Format(Culture, ResourceFinder("OutOfRangeInclusive") , @min, @max);
    }

  /// <summary>The operation is not valid because the enumerator is positioned before or after the enumeration body.</summary>
    internal static string OutsideEnumeration(
    
    ) {
        return string.Format(Culture, ResourceFinder("OutsideEnumeration") );
    }

  /// <summary>The text cannot be parsed into a valid instance of type `${typeName}'.</summary>
    internal static string ParseFailure(
    object @typeName
    ) {
        return string.Format(Culture, ResourceFinder("ParseFailure") , @typeName);
    }

  /// <summary>Probing for assemblies (${type})</summary>
    internal static string ProbingForAssemblies(
    object @type
    ) {
        return string.Format(Culture, ResourceFinder("ProbingForAssemblies") , @type);
    }

  /// <summary>Problem executing provider registration method `${type}.${method}'</summary>
    internal static string ProblemExecutingProviderRegistrationMethod(
    object @type, object @method
    ) {
        return string.Format(Culture, ResourceFinder("ProblemExecutingProviderRegistrationMethod") , @type, @method);
    }

  /// <summary>Problem executing service registration method `${startClass}.${method}'</summary>
    internal static string ProblemExecutingServiceRegistration(
    object @startClass, object @method
    ) {
        return string.Format(Culture, ResourceFinder("ProblemExecutingServiceRegistration") , @startClass, @method);
    }

  /// <summary>A category name must be specified in brackets.</summary>
    internal static string PropertiesCategoryMissingBrackets(
    
    ) {
        return string.Format(Culture, ResourceFinder("PropertiesCategoryMissingBrackets") );
    }

  /// <summary>Parser error: Expected key.</summary>
    internal static string PropertiesParseKeyNameExpected(
    
    ) {
        return string.Format(Culture, ResourceFinder("PropertiesParseKeyNameExpected") );
    }

  /// <summary>The property declaration does not specify a key.</summary>
    internal static string PropertyDeclarationMissingKey(
    
    ) {
        return string.Format(Culture, ResourceFinder("PropertyDeclarationMissingKey") );
    }

  /// <summary>The specified property, ${propertyName}, does not exist or cannot be converted to the required type.</summary>
    internal static string PropertyMissing(
    object @propertyName
    ) {
        return string.Format(Culture, ResourceFinder("PropertyMissing") , @propertyName);
    }

  /// <summary>The property, ${propertyName}, is expected to be set.</summary>
    internal static string PropertyMustBeSet(
    object @propertyName
    ) {
        return string.Format(Culture, ResourceFinder("PropertyMustBeSet") , @propertyName);
    }

  /// <summary>Property with the given name was not found, ${propertyName}.</summary>
    internal static string PropertyNotFound(
    object @propertyName
    ) {
        return string.Format(Culture, ResourceFinder("PropertyNotFound") , @propertyName);
    }

  /// <summary>No provider of type `${type}' was not found matching the criteria.</summary>
    internal static string ProviderNotFound(
    object @type
    ) {
        return string.Format(Culture, ResourceFinder("ProviderNotFound") , @type);
    }

  /// <summary>Qualified names cannot be created for array, byref, or pointer types, or for constructed generic types or generic parameters.</summary>
    internal static string QualifiedNameCannotBeGeneratedFromConstructed(
    
    ) {
        return string.Format(Culture, ResourceFinder("QualifiedNameCannotBeGeneratedFromConstructed") );
    }

  /// <summary>The operation cannot proceed because the collection is read-only.</summary>
    internal static string ReadOnlyCollection(
    
    ) {
        return string.Format(Culture, ResourceFinder("ReadOnlyCollection") );
    }

  /// <summary>The property, `${propertyName}' cannot be modified because it is read-only.</summary>
    internal static string ReadOnlyProperty(
    object @propertyName
    ) {
        return string.Format(Culture, ResourceFinder("ReadOnlyProperty") , @propertyName);
    }

  /// <summary>The property cannot be modified because it is read-only.</summary>
    internal static string ReadOnlyPropertyNoName(
    
    ) {
        return string.Format(Culture, ResourceFinder("ReadOnlyPropertyNoName") );
    }

  /// <summary>Failed to initialize custom root service provider type `${type}': ${ex}</summary>
    internal static string RootServiceProviderInitFailure(
    object @type, object @ex
    ) {
        return string.Format(Culture, ResourceFinder("RootServiceProviderInitFailure") , @type, @ex);
    }

  /// <summary>The operation cannot continue because the instance is not sealed.</summary>
    internal static string SealableNotSealed(
    
    ) {
        return string.Format(Culture, ResourceFinder("SealableNotSealed") );
    }

  /// <summary>Once sealed, the object cannot be modified.</summary>
    internal static string SealableReadOnly(
    
    ) {
        return string.Format(Culture, ResourceFinder("SealableReadOnly") );
    }

  /// <summary>The stream cannot be seeked because the base stream does not support seeking.</summary>
    internal static string SeekNotSupportedByBase(
    
    ) {
        return string.Format(Culture, ResourceFinder("SeekNotSupportedByBase") );
    }

  /// <summary>The service instance must be the same as the service type `${type}', derive from it, or implement a supported factory or metadata class.</summary>
    internal static string ServiceContainerAddInvalidServiceDescriptor(
    object @type
    ) {
        return string.Format(Culture, ResourceFinder("ServiceContainerAddInvalidServiceDescriptor") , @type);
    }

  /// <summary>Failed to start service: `${serviceType}'</summary>
    internal static string ServiceFailedToStart(
    object @serviceType
    ) {
        return string.Format(Culture, ResourceFinder("ServiceFailedToStart") , @serviceType);
    }

  /// <summary>The specified service `${type}' was not found.</summary>
    internal static string ServiceNotFound(
    object @type
    ) {
        return string.Format(Culture, ResourceFinder("ServiceNotFound") , @type);
    }

  /// <summary>The stream cannot be read, but it is expected to be readable.</summary>
    internal static string StreamCannotRead(
    
    ) {
        return string.Format(Culture, ResourceFinder("StreamCannotRead") );
    }

  /// <summary>The stream cannot be written to, but it is expected to be writable.</summary>
    internal static string StreamCannotWrite(
    
    ) {
        return string.Format(Culture, ResourceFinder("StreamCannotWrite") );
    }

  /// <summary>Specified URI scheme is not supported</summary>
    internal static string StreamContextUriSchemeNotSupported(
    
    ) {
        return string.Format(Culture, ResourceFinder("StreamContextUriSchemeNotSupported") );
    }

  /// <summary>Specified argument is not supported by the template.</summary>
    internal static string TemplateDoesNotSupportOperand(
    
    ) {
        return string.Format(Culture, ResourceFinder("TemplateDoesNotSupportOperand") );
    }

  /// <summary>Template with the given name not found: ${template}</summary>
    internal static string TemplateNotFound(
    object @template
    ) {
        return string.Format(Culture, ResourceFinder("TemplateNotFound") , @template);
    }

  /// <summary>Can't apply method to an instance of ${type}.</summary>
    internal static string ThisArgumentIncorrectType(
    object @type
    ) {
        return string.Format(Culture, ResourceFinder("ThisArgumentIncorrectType") , @type);
    }

  /// <summary>The type was not found: ${typeReference}.</summary>
    internal static string TypeMissing(
    object @typeReference
    ) {
        return string.Format(Culture, ResourceFinder("TypeMissing") , @typeReference);
    }

  /// <summary>Could not load type because no type matches the qualified name: ${name}</summary>
    internal static string TypeMissingFromQualifiedName(
    object @name
    ) {
        return string.Format(Culture, ResourceFinder("TypeMissingFromQualifiedName") , @name);
    }

  /// <summary>The type, `${typeName}', is not of the required type(s), `${requiredType}', and there is no suitable adapter factory in the service context to supply the conversion.</summary>
    internal static string TypeNotCastableAdaptable(
    object @typeName, object @requiredType
    ) {
        return string.Format(Culture, ResourceFinder("TypeNotCastableAdaptable") , @typeName, @requiredType);
    }

  /// <summary>An exception occurred while resolving the type: ${exception}</summary>
    internal static string TypeReferenceResolveError(
    object @exception
    ) {
        return string.Format(Culture, ResourceFinder("TypeReferenceResolveError") , @exception);
    }

  /// <summary>"Type reference resolving type `${fullName}'</summary>
    internal static string TypeReferenceResolvingType(
    object @fullName
    ) {
        return string.Format(Culture, ResourceFinder("TypeReferenceResolvingType") , @fullName);
    }

  /// <summary>Could not find related assembly `${relatedAssembly}' defined in `${definingAssembly}' (search locations: ${searchLocations})</summary>
    internal static string UnableToLoadRelatedAssembly(
    object @relatedAssembly, object @definingAssembly, object @searchLocations
    ) {
        return string.Format(Culture, ResourceFinder("UnableToLoadRelatedAssembly") , @relatedAssembly, @definingAssembly, @searchLocations);
    }

  /// <summary>The file ended unexpectedly when the reader expected more data.</summary>
    internal static string UnexpectedEof(
    
    ) {
        return string.Format(Culture, ResourceFinder("UnexpectedEof") );
    }

  /// <summary>A required value was unexpectedly null.</summary>
    internal static string UnexpectedlyNull(
    
    ) {
        return string.Format(Culture, ResourceFinder("UnexpectedlyNull") );
    }

  /// <summary>Don't use this attribute constructor to specify custom provider registration.  Use the overload that accepts the type.</summary>
    internal static string UseProviderRegistrationAttributeOverload(
    
    ) {
        return string.Format(Culture, ResourceFinder("UseProviderRegistrationAttributeOverload") );
    }

    }
}
