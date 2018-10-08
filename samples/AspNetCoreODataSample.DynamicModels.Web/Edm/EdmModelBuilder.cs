// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using AspNetCoreODataSample.DynamicModels.Web.Models;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Validation;

namespace AspNetCoreODataSample.DynamicModels.Web.Edm
{
    public class EdmModelBuilder
    {
        private readonly HouseContext _houseContext;
        private readonly IPluralizer _pluralizer;
        private IEdmModel _edmModel;

        public EdmModelBuilder(HouseContext houseContext, IPluralizer pluralizer)
        {
            _houseContext = houseContext;
            _pluralizer = pluralizer;
        }

        public IEdmModel GetEdmModel()
        {
            // here you would use some caching system which ensures
            // the EDM model is only rebuilt when it was updated in the DB
            if (_edmModel == null)
            {
                _edmModel = BuildEdmModel();
            }

            return _edmModel;
        }

        private IEdmModel BuildEdmModel()
        {
            var builder = new ODataConventionModelBuilder();

            // General Entities
            builder.EntityType<House>();
            builder.EntitySet<House>("Houses");
            builder.EntityType<Room>();
            builder.EntitySet<House>("Rooms");

            // Interior definition (user-defined properties)
            builder.EntityType<InteriorDefinition>()
                .HasMany(d => d.Properties).AutomaticallyExpand(true);
            builder.EntitySet<House>("InteriorDefinitions");

            builder.EntityType<InteriorPropertyDefinition>();
            builder.EnumType<InteriorPropertyType>();

            // Register base type of interiors and ignore user-defined properties 
            // they are defined below according to the current DB confiuration
            var interiorTypeConfiguration = builder.EntityType<Interior>();
            interiorTypeConfiguration.Ignore(i => i.DefinitionID);
            interiorTypeConfiguration.Ignore(i => i.Definition);
            interiorTypeConfiguration.Ignore(i => i.StringProperty1);
            interiorTypeConfiguration.Ignore(i => i.StringProperty2);
            interiorTypeConfiguration.Ignore(i => i.StringProperty3);
            interiorTypeConfiguration.Ignore(i => i.IntProperty1);
            interiorTypeConfiguration.Ignore(i => i.IntProperty2);
            interiorTypeConfiguration.Ignore(i => i.IntProperty3);
            interiorTypeConfiguration.Ignore(i => i.DoubleProperty1);
            interiorTypeConfiguration.Ignore(i => i.DoubleProperty2);
            interiorTypeConfiguration.Ignore(i => i.DoubleProperty3);
            builder.EntitySet<Interior>("Interior");

            // Build the intermediate model and register all dynamic interior definitions
            var model = (EdmModel)builder.GetEdmModel();
            var entityType = (IEdmEntityType)model.FindDeclaredType(interiorTypeConfiguration.FullName);
            var container = (EdmEntityContainer)model.EntityContainer;
            var ns = model.SchemaElements.First().Namespace;
            var propertyLookup = BuildPropertyLookup(typeof(IUserDefinedPropertyBag));

            var definitions = _houseContext.InteriorDefinitions.Include(d => d.Properties).ToArray();
            foreach (var definition in definitions)
            {
                // register type by name using the Interior as base type. 
                var type = new EdmEntityType(ns, definition.Name, entityType, false, false);
                // define the correct CLR type as backstore
                model.SetAnnotationValue(type, new ClrTypeAnnotation(typeof(Interior)));
                // remember which interior definition represents this EDM type. 
                model.SetAnnotationValue(type, new InteriorDefinitionAnnotation(definition.ID));
                // register user defined properties
                foreach (var property in definition.Properties)
                {
                    var edmProperty = type.AddStructuralProperty(property.Name, MapToEdmType(property.PropertyType));
                    model.SetAnnotationValue(edmProperty, new ClrPropertyInfoAnnotation(propertyLookup[property.PropertyName].Single()));
                }

                // add to model and register entity set
                model.AddElement(type);
                container.AddEntitySet(_pluralizer.Pluralize(definition.Name), type);
            }

            // Register type mapping handler which translates Interiors correctly.
            model.SetAnnotationValue<IEdmModelClrTypeMappingHandler>(model, new InteriorDefinitionAwareClrTypeMappingHandler());

            // validate the model again newly added entities
            if (!model.Validate(out var errors))
            {
                Debug.Fail("EDM is not valid", string.Join(Environment.NewLine, errors.Select(e => e.ToString())));
            }

            return model;
        }

        private EdmPrimitiveTypeKind MapToEdmType(InteriorPropertyType propertyType)
        {
            switch (propertyType)
            {
                case InteriorPropertyType.String:
                    return EdmPrimitiveTypeKind.String;
                case InteriorPropertyType.Int:
                    return EdmPrimitiveTypeKind.Int32;
                case InteriorPropertyType.Double:
                    return EdmPrimitiveTypeKind.Double;
                default:
                    throw new ArgumentOutOfRangeException(nameof(propertyType), propertyType, null);
            }
        }

        private ILookup<string, PropertyInfo> BuildPropertyLookup(Type type)
            => type.GetProperties(BindingFlags.Public | BindingFlags.Instance).ToLookup(prop => prop.Name);

        public bool IsInterior(IEdmModel model, IEdmType type)
        {
            return model.GetAnnotationValue<InteriorDefinitionAnnotation>(type) != null;
        }
    }
}
