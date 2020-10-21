// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using Microsoft.AspNet.OData.Common;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNet.OData.Routing
{
    /// <summary>
    /// A handler used to calculate some values based on the odata path.
    /// </summary>
    public class ODataPathSegmentHandler : PathSegmentHandler
    {
        private readonly IList<string> _pathTemplate;
        private readonly IList<string> _pathUriLiteral;
        private IEdmNavigationSource _navigationSource; // used to record the navigation source in the last segment.

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataPathSegmentHandler"/> class.
        /// </summary>
        public ODataPathSegmentHandler()
        {
            _navigationSource = null;
            _pathTemplate = new List<string> { ODataSegmentKinds.ServiceBase }; // ~
            _pathUriLiteral = new List<string>();
        }

        /// <summary>
        /// Gets the path navigation source.
        /// </summary>
        public IEdmNavigationSource NavigationSource
        {
            get { return _navigationSource; }
        }

        /// <summary>
        /// Gets the path template.
        /// </summary>
        public string PathTemplate
        {
            get { return String.Join("/", _pathTemplate); }
        }

        /// <summary>
        /// Gets the path literal.
        /// </summary>
        public string PathLiteral
        {
            get { return String.Join("/", _pathUriLiteral); }
        }

        /// <summary>
        /// Handle a EntitySetSegment
        /// </summary>
        /// <param name="segment">the segment to handle</param>
        public override void Handle(EntitySetSegment segment)
        {
            _navigationSource = segment.EntitySet;

            _pathTemplate.Add(ODataSegmentKinds.EntitySet); // entityset

            _pathUriLiteral.Add(segment.EntitySet.Name);
        }

        /// <summary>
        /// Handle a KeySegment
        /// </summary>
        /// <param name="segment">the segment to handle</param>
        public override void Handle(KeySegment segment)
        {
            _navigationSource = segment.NavigationSource;

            if (_pathTemplate.Last() == ODataSegmentKinds.Ref)
            {
                _pathTemplate.Insert(_pathTemplate.Count - 1, ODataSegmentKinds.Key);
            }
            else
            {
                _pathTemplate.Add(ODataSegmentKinds.Key);
            }

            string value = ConvertKeysToString(segment.Keys, segment.EdmType);

            // update the previous segment Uri literal
            if (!_pathUriLiteral.Any())
            {
                _pathUriLiteral.Add("(" + value + ")");
                return;
            }

            if (_pathUriLiteral.Last() == ODataSegmentKinds.Ref)
            {
                _pathUriLiteral[_pathUriLiteral.Count - 2] =
                    _pathUriLiteral[_pathUriLiteral.Count - 2] + "(" + value + ")";
            }
            else
            {
                _pathUriLiteral[_pathUriLiteral.Count - 1] =
                    _pathUriLiteral[_pathUriLiteral.Count - 1] + "(" + value + ")";
            }
        }

        /// <summary>
        /// Handle a NavigationPropertyLinkSegment
        /// </summary>
        /// <param name="segment">the segment to Handle</param>
        public override void Handle(NavigationPropertyLinkSegment segment)
        {
            _navigationSource = segment.NavigationSource;

            // TODO: do we really need to add $ref?
            _pathTemplate.Add(ODataSegmentKinds.Navigation); // navigation
            _pathTemplate.Add(ODataSegmentKinds.Ref); // $ref

            _pathUriLiteral.Add(segment.NavigationProperty.Name);
            _pathUriLiteral.Add(ODataSegmentKinds.Ref);
        }

        /// <summary>
        /// Handle a NavigationPropertySegment
        /// </summary>
        /// <param name="segment">the segment to Handle</param>
        public override void Handle(NavigationPropertySegment segment)
        {
            _navigationSource = segment.NavigationSource;

            _pathTemplate.Add(ODataSegmentKinds.Navigation); // navigation

            _pathUriLiteral.Add(segment.NavigationProperty.Name);
        }

        /// <summary>
        /// Handle a OpenPropertySegment
        /// </summary>
        /// <param name="segment">the segment to Handle</param>
        public override void Handle(DynamicPathSegment segment)
        {
            _navigationSource = null;

            _pathTemplate.Add(ODataSegmentKinds.DynamicProperty); // dynamic property

            _pathUriLiteral.Add(segment.Identifier);
        }

        /// <summary>
        /// Handle a OperationImportSegment
        /// </summary>
        /// <param name="segment">the segment to Handle</param>
        public override void Handle(OperationImportSegment segment)
        {
            _navigationSource = segment.EntitySet;

            IEdmOperationImport operationImport = segment.OperationImports.Single();
            IEdmActionImport actionImport = operationImport as IEdmActionImport;

            if (actionImport != null)
            {
                _pathTemplate.Add(ODataSegmentKinds.UnboundAction); // unbound action
                _pathUriLiteral.Add(actionImport.Name);
            }
            else
            {
                IEdmFunctionImport function = (IEdmFunctionImport)operationImport;

                _pathTemplate.Add(ODataSegmentKinds.UnboundFunction); // unbound function

                // Translate the nodes in ODL path to string literals as parameter of UnboundFunctionPathSegment.
                Dictionary<string, string> parameterValues = segment.Parameters.ToDictionary(
                    parameterValue => parameterValue.Name,
                    parameterValue => TranslateNode(parameterValue.Value, function.Name, parameterValue.Name));

                 IEnumerable<string> parameters = parameterValues.Select(v => String.Format(CultureInfo.InvariantCulture, "{0}={1}", v.Key, v.Value));
                 string literal = String.Format(CultureInfo.InvariantCulture, "{0}({1})", function.Name, String.Join(",", parameters));

                 _pathUriLiteral.Add(literal);
            }
        }

        /// <summary>
        /// Handle an OperationSegment
        /// </summary>
        /// <param name="segment">the segment to handle</param>
        public override void Handle(OperationSegment segment)
        {
            _navigationSource = segment.EntitySet;

            IEdmOperation edmOperation = segment.Operations.Single();
            IEdmAction action = edmOperation as IEdmAction;

            if (action != null)
            {
                _pathTemplate.Add(ODataSegmentKinds.Action); // action
                _pathUriLiteral.Add(action.FullName());
            }
            else
            {
                IEdmFunction function = (IEdmFunction)edmOperation;
                _pathTemplate.Add(ODataSegmentKinds.Function); // function

                // Translate the nodes in ODL path to string literals as parameter of BoundFunctionPathSegment.
                Dictionary<string, string> parameterValues = segment.Parameters.ToDictionary(
                    parameterValue => parameterValue.Name,
                    parameterValue => TranslateNode(parameterValue.Value, function.Name, parameterValue.Name));

                // TODO: refactor the function literal for parameter alias
                IEnumerable<string> parameters = parameterValues.Select(v => String.Format(CultureInfo.InvariantCulture, "{0}={1}", v.Key, v.Value));
                string literal = String.Format(CultureInfo.InvariantCulture, "{0}({1})", function.FullName(), String.Join(",", parameters));

                _pathUriLiteral.Add(literal);
            }
        }

        /// <summary>
        /// Handle a PropertySegment
        /// </summary>
        /// <param name="segment">the segment to handle</param>
        public override void Handle(PathTemplateSegment segment)
        {
            _navigationSource = null;

            _pathTemplate.Add(ODataSegmentKinds.Property); // path template

            _pathUriLiteral.Add(segment.LiteralText);
        }

        /// <summary>
        /// Handle a PropertySegment
        /// </summary>
        /// <param name="segment">the segment to handle</param>
        public override void Handle(PropertySegment segment)
        {
            // Not setting navigation source to null as the relevant navigation source for the path will be the previous navigation source.

            _pathTemplate.Add(ODataSegmentKinds.Property); // property

            _pathUriLiteral.Add(segment.Property.Name);
        }

        /// <summary>
        /// Handle a SingletonSegment
        /// </summary>
        /// <param name="segment">the segment to handle</param>
        public override void Handle(SingletonSegment segment)
        {
            _navigationSource = segment.Singleton;

            _pathTemplate.Add(ODataSegmentKinds.Singleton); // singleton

            _pathUriLiteral.Add(segment.Singleton.Name);
        }

        /// <summary>
        /// Handle a TypeSegment, we use "cast" for type segment.
        /// </summary>
        /// <param name="segment">the segment to handle</param>
        public override void Handle(TypeSegment segment)
        {
            _navigationSource = segment.NavigationSource;

            _pathTemplate.Add(ODataSegmentKinds.Cast); // cast

            // Uri literal does not use the collection type.
            IEdmType elementType = segment.EdmType;
            if (segment.EdmType.TypeKind == EdmTypeKind.Collection)
            {
                elementType = ((IEdmCollectionType)segment.EdmType).ElementType.Definition;
            }

            _pathUriLiteral.Add(elementType.FullTypeName());
        }

        /// <summary>
        /// Handle a ValueSegment
        /// </summary>
        /// <param name="segment">the segment to handle</param>
        public override void Handle(ValueSegment segment)
        {
            // do nothing for the navigation source for $value.
            // It means to use the previous the navigation source

            _pathTemplate.Add(ODataSegmentKinds.Value); // $value
            _pathUriLiteral.Add(ODataSegmentKinds.Value);
        }

        /// <summary>
        /// Handle a CountSegment
        /// </summary>
        /// <param name="segment">the segment to handle</param>
        public override void Handle(CountSegment segment)
        {
            _navigationSource = null;

            _pathTemplate.Add(ODataSegmentKinds.Count); // $count
            _pathUriLiteral.Add(ODataSegmentKinds.Count);
        }

        /// <summary>
        /// Handle a BatchSegment
        /// </summary>
        /// <param name="segment">the segment to handle</param>
        public override void Handle(BatchSegment segment)
        {
            _navigationSource = null;

            _pathTemplate.Add(ODataSegmentKinds.Batch);
            _pathUriLiteral.Add(ODataSegmentKinds.Batch);
        }

        /// <summary>
        /// Handle a MetadataSegment
        /// </summary>
        /// <param name="segment">the segment to handle</param>
        public override void Handle(MetadataSegment segment)
        {
            _navigationSource = null;

            _pathTemplate.Add(ODataSegmentKinds.Metadata); // $metadata
            _pathUriLiteral.Add(ODataSegmentKinds.Metadata);
        }

        /// <summary>
        /// Handle a general path segment
        /// </summary>
        /// <param name="segment">the segment to handle</param>
        public override void Handle(ODataPathSegment segment)
        {
            // ODL doesn't provide the handle function for general path segment
            _navigationSource = null;

            _pathTemplate.Add(segment.ToString());
            _pathUriLiteral.Add(segment.ToString());
        }

        /// <summary>
        /// Handle a UnresolvedPathSegment
        /// </summary>
        /// <param name="segment">the segment to handle</param>
        public virtual void Handle(UnresolvedPathSegment segment)
        {
            // ODL doesn't provide the handle function for unresolved path segment
            _navigationSource = null;

            _pathTemplate.Add(ODataSegmentKinds.Unresolved); // unresolved
            _pathUriLiteral.Add(segment.SegmentValue);
        }

        // Convert the objects of keys in ODL path to string literals.
        private static string ConvertKeysToString(IEnumerable<KeyValuePair<string, object>> keys, IEdmType edmType)
        {
            Contract.Assert(keys != null);

            IEdmEntityType entityType = edmType as IEdmEntityType;
            Contract.Assert(entityType != null);

            IList<KeyValuePair<string, object>> keyValuePairs = keys as IList<KeyValuePair<string, object>> ?? keys.ToList();
            if (keyValuePairs.Count < 1)
            {
                return String.Empty;
            }

            if (keyValuePairs.Count < 2)
            {
                var keyValue = keyValuePairs.First();
                bool isDeclaredKey = entityType.Key().Any(k => k.Name == keyValue.Key);

                // To support the alternate key
                if (isDeclaredKey)
                {
                    return String.Join(
                        ",",
                        keyValuePairs.Select(keyValuePair =>
                            TranslateKeySegmentValue(keyValuePair.Value)).ToArray());
                }
            }

            return String.Join(
                ",",
                keyValuePairs.Select(keyValuePair =>
                    (keyValuePair.Key +
                     "=" +
                     TranslateKeySegmentValue(keyValuePair.Value))).ToArray());
        }

        // Translate the object of key in ODL path to string literal.
        private static string TranslateKeySegmentValue(object value)
        {
            if (value == null)
            {
                throw Error.ArgumentNull("value");
            }

            UriTemplateExpression uriTemplateExpression = value as UriTemplateExpression;
            if (uriTemplateExpression != null)
            {
                return uriTemplateExpression.LiteralText;
            }

            ConstantNode constantNode = value as ConstantNode;
            if (constantNode != null)
            {
                ODataEnumValue enumValue = constantNode.Value as ODataEnumValue;
                if (enumValue != null)
                {
                    return ODataUriUtils.ConvertToUriLiteral(enumValue, ODataVersion.V4);
                }
            }

            return ODataUriUtils.ConvertToUriLiteral(value, ODataVersion.V4);
        }

        private static string TranslateNode(object node, string functionName, string parameterName)
        {
            // If the function parameter is null, for example myFunction(param=null),
            // the input node here is not null, it is a contant node with a value as "null".
            // However, if a function call (or key) using parameter alias but without providing the parameter alias value,
            // the input node here is a null.
            if (node == null)
            {
                // We can't throw ODataException here because ODataException will be caught and return 404 response with empty message.
                throw new InvalidOperationException(Error.Format(SRResources.MissingConvertNode, parameterName, functionName));
            }

            ConstantNode constantNode = node as ConstantNode;
            if (constantNode != null)
            {
                UriTemplateExpression uriTemplateExpression = constantNode.Value as UriTemplateExpression;
                if (uriTemplateExpression != null)
                {
                    return uriTemplateExpression.LiteralText;
                }

                // Make the enum prefix free to work.
                ODataEnumValue enumValue = constantNode.Value as ODataEnumValue;
                if (enumValue != null)
                {
                    return ODataUriUtils.ConvertToUriLiteral(enumValue, ODataVersion.V4);
                }

                return constantNode.LiteralText;
            }

            ConvertNode convertNode = node as ConvertNode;
            if (convertNode != null)
            {
                return TranslateNode(convertNode.Source, functionName, parameterName);
            }

            ParameterAliasNode parameterAliasNode = node as ParameterAliasNode;
            if (parameterAliasNode != null)
            {
                return parameterAliasNode.Alias;
            }

            //return node.ToString();
            throw Error.NotSupported(SRResources.CannotRecognizeNodeType, typeof(ODataPathSegmentHandler),
                node.GetType().FullName);
        }
    }
}
