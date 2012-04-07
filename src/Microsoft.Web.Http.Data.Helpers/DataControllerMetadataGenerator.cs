// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Microsoft.Web.Http.Data.Helpers
{
    internal static class DataControllerMetadataGenerator
    {
        private static readonly ConcurrentDictionary<DataControllerDescription, IEnumerable<TypeMetadata>> _metadataMap =
            new ConcurrentDictionary<DataControllerDescription, IEnumerable<TypeMetadata>>();

        private static readonly IEnumerable<JProperty> _emptyJsonPropertyEnumerable = Enumerable.Empty<JProperty>();

        public static IEnumerable<TypeMetadata> GetMetadata(DataControllerDescription description)
        {
            return _metadataMap.GetOrAdd(description, desc =>
            {
                return GenerateMetadata(desc);
            });
        }

        private static IEnumerable<TypeMetadata> GenerateMetadata(DataControllerDescription description)
        {
            List<TypeMetadata> metadata = new List<TypeMetadata>();
            foreach (Type entityType in description.EntityTypes)
            {
                metadata.Add(new TypeMetadata(entityType));
            }
            // TODO: Complex types are NYI in DataControllerDescription
            // foreach (Type complexType in description.ComplexTypes)
            // {
            //     metadata.Add(new TypeMetadata(complexType));
            // }
            return metadata;
        }

        private static string EncodeTypeName(string typeName, string typeNamespace)
        {
            return String.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", typeName, MetadataStrings.NamespaceMarker, typeNamespace);
        }

        private static class MetadataStrings
        {
            public const string NamespaceMarker = ":#";
            public const string TypeString = "type";
            public const string ArrayString = "array";
            public const string AssociationString = "association";
            public const string FieldsString = "fields";
            public const string ThisKeyString = "thisKey";
            public const string IsForeignKey = "isForeignKey";
            public const string OtherKeyString = "otherKey";
            public const string NameString = "name";
            public const string ReadOnlyString = "readonly";
            public const string KeyString = "key";
            public const string RulesString = "rules";
            public const string MessagesString = "messages";
        }

        public class TypeMetadata
        {
            private List<string> _key = new List<string>();
            private List<TypePropertyMetadata> _properties = new List<TypePropertyMetadata>();

            public TypeMetadata(Type entityType)
            {
                Type type = TypeUtility.GetElementType(entityType);
                TypeName = type.Name;
                TypeNamespace = type.Namespace;

                IEnumerable<PropertyDescriptor> properties =
                    TypeDescriptor.GetProperties(entityType).Cast<PropertyDescriptor>().OrderBy(p => p.Name)
                        .Where(p => TypeUtility.IsDataMember(p));

                foreach (PropertyDescriptor pd in properties)
                {
                    _properties.Add(new TypePropertyMetadata(pd));
                    if (TypeDescriptorExtensions.ExplicitAttributes(pd)[typeof(KeyAttribute)] != null)
                    {
                        _key.Add(pd.Name);
                    }
                }
            }

            public string TypeName { get; private set; }
            public string TypeNamespace { get; private set; }

            public string EncodedTypeName
            {
                get { return EncodeTypeName(TypeName, TypeNamespace); }
            }

            public IEnumerable<string> Key
            {
                get { return _key; }
            }

            public IEnumerable<TypePropertyMetadata> Properties
            {
                get { return _properties; }
            }

            public JToken ToJToken()
            {
                JObject value = new JObject();

                value[MetadataStrings.KeyString] = new JArray(Key.Select(k => (JToken)k));
                value[MetadataStrings.FieldsString] = new JObject(Properties.Select(p => new JProperty(p.Name, p.ToJToken())));

                // TODO: Only include these properties when they'll have non-empty values.  Need to update SPA T4 templates to tolerate null in scaffolded SPA JavaScript.
                //if (Properties.Any(p => p.ValidationRules.Count > 0))
                //{
                value[MetadataStrings.RulesString] = new JObject(
                    Properties.SelectMany(
                        p => p.ValidationRules.Count == 0
                                 ? _emptyJsonPropertyEnumerable
                                 : new JProperty[]
                                 {
                                     new JProperty(
                                       p.Name,
                                       new JObject(p.ValidationRules.Select(
                                           r => new JProperty(r.Name, r.ToJToken()))))
                                 }));
                //}
                //if (Properties.Any(p => p.ValidationRules.Any(r => r.ErrorMessageString != null))) 
                //{
                value[MetadataStrings.MessagesString] = new JObject(
                    Properties.SelectMany(
                        p => !p.ValidationRules.Any(r => r.ErrorMessageString != null)
                                 ? _emptyJsonPropertyEnumerable
                                 : new JProperty[]
                                 {
                                     new JProperty(
                                       p.Name,
                                       new JObject(p.ValidationRules.SelectMany(r =>
                                                                                   r.ErrorMessageString == null
                                                                                       ? _emptyJsonPropertyEnumerable
                                                                                       : new JProperty[]
                                                                                       {
                                                                                           new JProperty(r.Name, r.ErrorMessageString)
                                                                                       })))
                                 }));
                //}

                return value;
            }
        }

        public class TypePropertyAssociationMetadata
        {
            private List<string> _thisKeyMembers = new List<string>();
            private List<string> _otherKeyMembers = new List<string>();

            public TypePropertyAssociationMetadata(AssociationAttribute associationAttr)
            {
                Name = associationAttr.Name;
                IsForeignKey = associationAttr.IsForeignKey;
                _otherKeyMembers = associationAttr.OtherKeyMembers.ToList<string>();
                _thisKeyMembers = associationAttr.ThisKeyMembers.ToList<string>();
            }

            public string Name { get; private set; }
            public bool IsForeignKey { get; private set; }

            public IEnumerable<string> ThisKeyMembers
            {
                get { return _thisKeyMembers; }
            }

            public IEnumerable<string> OtherKeyMembers
            {
                get { return _otherKeyMembers; }
            }

            public JToken ToJToken()
            {
                JObject value = new JObject();
                value[MetadataStrings.NameString] = Name;
                value[MetadataStrings.ThisKeyString] = new JArray(ThisKeyMembers.Select(k => (JToken)k));
                value[MetadataStrings.OtherKeyString] = new JArray(OtherKeyMembers.Select(k => (JToken)k));
                value[MetadataStrings.IsForeignKey] = IsForeignKey;
                return value;
            }
        }

        public class TypePropertyMetadata
        {
            private List<TypePropertyValidationRuleMetadata> _validationRules = new List<TypePropertyValidationRuleMetadata>();

            public TypePropertyMetadata(PropertyDescriptor descriptor)
            {
                Name = descriptor.Name;

                Type elementType = TypeUtility.GetElementType(descriptor.PropertyType);
                IsArray = !elementType.Equals(descriptor.PropertyType);
                // TODO: What should we do with nullable types here?
                TypeName = elementType.Name;
                TypeNamespace = elementType.Namespace;

                AttributeCollection propertyAttributes = TypeDescriptorExtensions.ExplicitAttributes(descriptor);

                // TODO, 336102, ReadOnlyAttribute for editability?  RIA used EditableAttribute?
                ReadOnlyAttribute readonlyAttr = (ReadOnlyAttribute)propertyAttributes[typeof(ReadOnlyAttribute)];
                IsReadOnly = (readonlyAttr != null) ? readonlyAttr.IsReadOnly : false;

                AssociationAttribute associationAttr = (AssociationAttribute)propertyAttributes[typeof(AssociationAttribute)];
                if (associationAttr != null)
                {
                    Association = new TypePropertyAssociationMetadata(associationAttr);
                }

                RequiredAttribute requiredAttribute = (RequiredAttribute)propertyAttributes[typeof(RequiredAttribute)];
                if (requiredAttribute != null)
                {
                    _validationRules.Add(new TypePropertyValidationRuleMetadata(requiredAttribute));
                }

                RangeAttribute rangeAttribute = (RangeAttribute)propertyAttributes[typeof(RangeAttribute)];
                if (rangeAttribute != null)
                {
                    Type operandType = rangeAttribute.OperandType;
                    operandType = Nullable.GetUnderlyingType(operandType) ?? operandType;
                    if (operandType.Equals(typeof(Double))
                        || operandType.Equals(typeof(Int16))
                        || operandType.Equals(typeof(Int32))
                        || operandType.Equals(typeof(Int64))
                        || operandType.Equals(typeof(Single)))
                    {
                        _validationRules.Add(new TypePropertyValidationRuleMetadata(rangeAttribute));
                    }
                }

                StringLengthAttribute stringLengthAttribute = (StringLengthAttribute)propertyAttributes[typeof(StringLengthAttribute)];
                if (stringLengthAttribute != null)
                {
                    _validationRules.Add(new TypePropertyValidationRuleMetadata(stringLengthAttribute));
                }

                DataTypeAttribute dataTypeAttribute = (DataTypeAttribute)propertyAttributes[typeof(DataTypeAttribute)];
                if (dataTypeAttribute != null)
                {
                    if (dataTypeAttribute.DataType.Equals(DataType.EmailAddress)
                        || dataTypeAttribute.DataType.Equals(DataType.Url))
                    {
                        _validationRules.Add(new TypePropertyValidationRuleMetadata(dataTypeAttribute));
                    }
                }
            }

            public string Name { get; private set; }
            public string TypeName { get; private set; }
            public string TypeNamespace { get; private set; }
            public bool IsReadOnly { get; private set; }
            public bool IsArray { get; private set; }
            public TypePropertyAssociationMetadata Association { get; private set; }

            public IList<TypePropertyValidationRuleMetadata> ValidationRules
            {
                get { return _validationRules; }
            }

            public JToken ToJToken()
            {
                JObject value = new JObject();

                value[MetadataStrings.TypeString] = EncodeTypeName(TypeName, TypeNamespace);

                if (IsReadOnly)
                {
                    value[MetadataStrings.ReadOnlyString] = true;
                }

                if (IsArray)
                {
                    value[MetadataStrings.ArrayString] = true;
                }

                if (Association != null)
                {
                    value[MetadataStrings.AssociationString] = Association.ToJToken();
                }

                return value;
            }
        }

        public class TypePropertyValidationRuleMetadata
        {
            private string _type;

            public TypePropertyValidationRuleMetadata(RequiredAttribute attribute)
                : this((ValidationAttribute)attribute)
            {
                Name = "required";
                Value1 = true;
                _type = "boolean";
            }

            public TypePropertyValidationRuleMetadata(RangeAttribute attribute)
                : this((ValidationAttribute)attribute)
            {
                Name = "range";
                Value1 = attribute.Minimum;
                Value2 = attribute.Maximum;
                _type = "array";
            }

            public TypePropertyValidationRuleMetadata(StringLengthAttribute attribute)
                : this((ValidationAttribute)attribute)
            {
                if (attribute.MinimumLength != 0)
                {
                    Name = "rangelength";
                    Value1 = attribute.MinimumLength;
                    Value2 = attribute.MaximumLength;
                    _type = "array";
                }
                else
                {
                    Name = "maxlength";
                    Value1 = attribute.MaximumLength;
                    _type = "number";
                }
            }

            public TypePropertyValidationRuleMetadata(DataTypeAttribute attribute)
                : this((ValidationAttribute)attribute)
            {
                switch (attribute.DataType)
                {
                    case DataType.EmailAddress:
                        Name = "email";
                        break;
                    case DataType.Url:
                        Name = "url";
                        break;
                    default:
                        break;
                }
                Value1 = true;
                _type = "boolean";
            }

            public TypePropertyValidationRuleMetadata(ValidationAttribute attribute)
            {
                if (attribute.ErrorMessage != null)
                {
                    ErrorMessageString = attribute.ErrorMessage;
                }
            }

            public string Name { get; private set; }
            public object Value1 { get; private set; }
            public object Value2 { get; private set; }
            public string ErrorMessageString { get; private set; }

            public JToken ToJToken()
            {
                // The output json is determined by the number of values. The object constructor takes care the value assignment.
                // When we have two values, we have two numbers that are written as an array.
                // When we have only one value, it is written as it's type only.
                if (_type == "array")
                {
                    return new JArray() { new JValue(Value1), new JValue(Value2) };
                }
                else if (_type == "boolean")
                {
                    return (bool)Value1;
                }
                else if (_type == "number")
                {
                    return (int)Value1;
                }
                else
                {
                    throw new InvalidOperationException("Unexpected validation rule type.");
                }
            }
        }
    }
}
