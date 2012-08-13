// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web.Http.OData.Properties;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;

namespace System.Web.Http.OData.Builder
{
    public static class EdmModelHelperMethods
    {
        internal static IEntitySetLinkBuilder GetEntitySetLinkBuilder(this IEdmModel model, IEdmEntitySet entitySet)
        {
            IEntitySetLinkBuilder annotation = model.GetAnnotationValue<IEntitySetLinkBuilder>(entitySet);
            if (annotation == null)
            {
                throw Error.NotSupported(SRResources.EntitySetHasNoBuildLinkAnnotation, entitySet.Name);
            }

            return annotation;
        }

        internal static void SetEntitySetLinkBuilderAnnotation(this IEdmModel model, IEdmEntitySet entitySet, IEntitySetLinkBuilder entitySetLinkBuilder)
        {
            model.SetAnnotationValue(entitySet, entitySetLinkBuilder);
        }

        internal static string GetEntitySetUrl(this IEdmModel model, IEdmEntitySet entitySet)
        {
            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }

            if (entitySet == null)
            {
                throw Error.ArgumentNull("entitySet");
            }

            EntitySetUrlAnnotation annotation = model.GetAnnotationValue<EntitySetUrlAnnotation>(entitySet);
            if (annotation == null)
            {
                return entitySet.Name;
            }
            else
            {
                return annotation.Url;
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Class coupling warranted")]
        public static IEdmModel BuildEdmModel(string containerNamespace, string containerName, IEnumerable<IStructuralTypeConfiguration> entityTypeConfigurations, IEnumerable<IEntitySetConfiguration> entitySetConfigurations)
        {
            if (entityTypeConfigurations == null)
            {
                throw Error.ArgumentNull("entityTypeConfigurations");
            }

            if (entitySetConfigurations == null)
            {
                throw Error.ArgumentNull("entitySetConfigurations");
            }

            EdmModel model = new EdmModel();

            Dictionary<string, IEdmStructuredType> types = EdmTypeBuilder.GetTypes(entityTypeConfigurations)
                .OfType<IEdmStructuredType>()
                .ToDictionary(t => t.ToString());

            foreach (IEdmStructuredType type in types.Values)
            {
                if (type.TypeKind == EdmTypeKind.Complex)
                {
                    model.AddElement(type as IEdmComplexType);
                }
                else if (type.TypeKind == EdmTypeKind.Entity)
                {
                    model.AddElement(type as IEdmEntityType);
                }
                else
                {
                    throw Error.InvalidOperation(SRResources.UnsupportedEntityTypeInModel);
                }
            }

            if (entitySetConfigurations.Any())
            {
                EdmEntityContainer container = new EdmEntityContainer(containerNamespace, containerName);
                Dictionary<string, EdmEntitySet> edmEntitySetMap = new Dictionary<string, EdmEntitySet>();
                foreach (IEntitySetConfiguration entitySet in entitySetConfigurations)
                {
                    EdmEntitySet edmEntitySet = container.AddEntitySet(entitySet.Name, (IEdmEntityType)types[entitySet.EntityType.FullName]);
                    EntitySetLinkBuilderAnnotation entitySetLinkBuilderAnnotation = new EntitySetLinkBuilderAnnotation(entitySet);

                    model.SetEntitySetLinkBuilderAnnotation(edmEntitySet, entitySetLinkBuilderAnnotation);
                    model.SetAnnotationValue<EntitySetUrlAnnotation>(edmEntitySet, new EntitySetUrlAnnotation { Url = entitySet.GetUrl() });
                    edmEntitySetMap.Add(edmEntitySet.Name, edmEntitySet);
                }

                foreach (IEntitySetConfiguration entitySet in entitySetConfigurations)
                {
                    EdmEntitySet edmEntitySet = edmEntitySetMap[entitySet.Name];
                    EntitySetLinkBuilderAnnotation entitySetLinkBuilderAnnotation = model.GetEntitySetLinkBuilder(edmEntitySet) as EntitySetLinkBuilderAnnotation;
                    foreach (NavigationPropertyConfiguration navigation in entitySet.EntityType.NavigationProperties)
                    {
                        NavigationPropertyBinding binding = entitySet.FindBinding(navigation);
                        if (binding != null)
                        {
                            EdmEntityType edmEntityType = types[entitySet.EntityType.FullName] as EdmEntityType;
                            IEdmNavigationProperty edmNavigationProperty = edmEntityType.NavigationProperties().Single(np => np.Name == navigation.Name);

                            edmEntitySet.AddNavigationTarget(edmNavigationProperty, edmEntitySetMap[binding.EntitySet.Name]);
                            entitySetLinkBuilderAnnotation.AddNavigationPropertyLinkBuilder(edmNavigationProperty, entitySet.GetNavigationPropertyLink(edmNavigationProperty.Name));
                        }
                    }
                }

                model.AddElement(container);
            }

            return model;
        }
    }
}
