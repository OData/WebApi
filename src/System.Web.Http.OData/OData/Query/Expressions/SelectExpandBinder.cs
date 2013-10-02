// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.Http.OData.Formatter;
using System.Web.Http.OData.Formatter.Serialization;
using System.Web.Http.OData.Properties;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;
using Microsoft.Data.OData.Query.SemanticAst;

namespace System.Web.Http.OData.Query.Expressions
{
    /// <summary>
    /// Applies the given <see cref="SelectExpandQueryOption"/> to the given <see cref="IQueryable"/>.
    /// </summary>
    internal class SelectExpandBinder
    {
        private SelectExpandQueryOption _selectExpandQuery;
        private ODataQueryContext _context;
        private IEdmModel _model;
        private ODataQuerySettings _settings;
        private string _modelID;

        public SelectExpandBinder(ODataQuerySettings settings, SelectExpandQueryOption selectExpandQuery)
        {
            Contract.Assert(settings != null);
            Contract.Assert(selectExpandQuery != null);
            Contract.Assert(selectExpandQuery.Context != null);
            Contract.Assert(selectExpandQuery.Context.Model != null);
            Contract.Assert(settings.HandleNullPropagation != HandleNullPropagationOption.Default);

            _selectExpandQuery = selectExpandQuery;
            _context = selectExpandQuery.Context;
            _model = _context.Model;
            _modelID = ModelContainer.GetModelID(_model);
            _settings = settings;
        }

        public static IQueryable Bind(IQueryable queryable, ODataQuerySettings settings, SelectExpandQueryOption selectExpandQuery)
        {
            Contract.Assert(queryable != null);

            SelectExpandBinder binder = new SelectExpandBinder(settings, selectExpandQuery);
            return binder.Bind(queryable);
        }

        public static object Bind(object entity, ODataQuerySettings settings, SelectExpandQueryOption selectExpandQuery)
        {
            Contract.Assert(entity != null);

            SelectExpandBinder binder = new SelectExpandBinder(settings, selectExpandQuery);
            return binder.Bind(entity);
        }

        private object Bind(object entity)
        {
            Contract.Assert(entity != null);

            LambdaExpression projectionLambda = GetProjectionLambda();

            // TODO: cache this ?
            return projectionLambda.Compile().DynamicInvoke(entity);
        }

        private IQueryable Bind(IQueryable queryable)
        {
            Type elementType = _selectExpandQuery.Context.ElementClrType;

            LambdaExpression projectionLambda = GetProjectionLambda();

            MethodInfo selectMethod = ExpressionHelperMethods.QueryableSelectGeneric.MakeGenericMethod(elementType, projectionLambda.Body.Type);
            return selectMethod.Invoke(null, new object[] { queryable, projectionLambda }) as IQueryable;
        }

        private LambdaExpression GetProjectionLambda()
        {
            Type elementType = _selectExpandQuery.Context.ElementClrType;
            ParameterExpression source = Expression.Parameter(elementType);

            // expression looks like -> new Wrapper { Instance = source , Properties = "...", Container = new PropertyContainer { ... } }
            Expression projectionExpression = ProjectElement(source, _selectExpandQuery.SelectExpandClause, _context.ElementType as IEdmEntityType);

            // expression looks like -> source => new Wrapper { Instance = source .... }
            LambdaExpression projectionLambdaExpression = Expression.Lambda(projectionExpression, source);

            return projectionLambdaExpression;
        }

        internal Expression ProjectAsWrapper(Expression source, SelectExpandClause selectExpandClause, IEdmEntityType entityType)
        {
            Type elementType;
            if (source.Type.IsCollection(out elementType))
            {
                // new CollectionWrapper<ElementType> { Instance = source.Select(s => new Wrapper { ... }) };
                return ProjectCollection(source, elementType, selectExpandClause, entityType);
            }
            else
            {
                // new Wrapper { v1 = source.property ... }
                return ProjectElement(source, selectExpandClause, entityType);
            }
        }

        internal Expression CreatePropertyNameExpression(IEdmEntityType elementType, IEdmProperty property, Expression source)
        {
            Contract.Assert(elementType != null);
            Contract.Assert(property != null);
            Contract.Assert(source != null);

            IEdmEntityType declaringType = property.DeclaringType as IEdmEntityType;
            Contract.Assert(declaringType != null, "only entity types are projected.");

            Expression propertyName;

            // derived navigation property using cast
            if (elementType != declaringType)
            {
                Type castType = EdmLibHelpers.GetClrType(declaringType, _model);
                if (castType == null)
                {
                    throw new ODataException(Error.Format(SRResources.MappingDoesNotContainEntityType, declaringType.FullName()));
                }

                // Expression
                //          source is navigationPropertyDeclaringType ? propertyName : null
                propertyName = Expression.Condition(
                    test: Expression.TypeIs(source, castType),
                    ifTrue: Expression.Constant(property.Name),
                    ifFalse: Expression.Constant(null, typeof(string)));
            }
            else
            {
                // Expression
                //          "propertyName"
                propertyName = Expression.Constant(property.Name);
            }

            return propertyName;
        }

        internal Expression CreatePropertyValueExpression(IEdmEntityType elementType, IEdmProperty property, Expression source)
        {
            Contract.Assert(elementType != null);
            Contract.Assert(property != null);
            Contract.Assert(source != null);

            IEdmEntityType declaringType = property.DeclaringType as IEdmEntityType;
            Contract.Assert(declaringType != null, "only entity types are projected.");

            // derived property using cast
            if (elementType != declaringType)
            {
                Type castType = EdmLibHelpers.GetClrType(declaringType, _model);
                if (castType == null)
                {
                    throw new ODataException(Error.Format(SRResources.MappingDoesNotContainEntityType, declaringType.FullName()));
                }

                source = Expression.TypeAs(source, castType);
            }

            Expression propertyValue = Expression.Property(source, property.Name);

            if (_settings.HandleNullPropagation == HandleNullPropagationOption.True)
            {
                // source == null ? null : propertyValue
                propertyValue = Expression.Condition(
                        test: Expression.Equal(source, Expression.Constant(null)),
                        ifTrue: Expression.Constant(null, propertyValue.Type.ToNullable()),
                        ifFalse: ExpressionHelpers.ToNullable(propertyValue));
            }
            else
            {
                // need to cast this to nullable as EF would fail while materializing if the property is not nullable and source is null.
                propertyValue = ExpressionHelpers.ToNullable(propertyValue);
            }

            return propertyValue;
        }

        // Generates the expression
        //      source => new Wrapper { Instance = source, Container = new PropertyContainer { ..expanded properties.. } }
        private Expression ProjectElement(Expression source, SelectExpandClause selectExpandClause, IEdmEntityType entityType)
        {
            Contract.Assert(source != null);

            Type elementType = source.Type;
            Type wrapperType = typeof(SelectExpandWrapper<>).MakeGenericType(elementType);
            List<MemberAssignment> wrapperTypeMemberAssignments = new List<MemberAssignment>();

            PropertyInfo wrapperProperty;
            bool isInstancePropertySet = false;
            bool isTypeNamePropertySet = false;
            bool isContainerPropertySet = false;

            // Initialize property 'ModelID' on the wrapper class.
            // source = new Wrapper { ModelID = 'some-guid-id' }
            wrapperProperty = wrapperType.GetProperty("ModelID");
            wrapperTypeMemberAssignments.Add(Expression.Bind(wrapperProperty, Expression.Constant(_modelID)));

            if (IsSelectAll(selectExpandClause))
            {
                // Initialize property 'Instance' on the wrapper class
                // source => new Wrapper { Instance = element }
                wrapperProperty = wrapperType.GetProperty("Instance");
                Contract.Assert(wrapperProperty != null);
                wrapperTypeMemberAssignments.Add(Expression.Bind(wrapperProperty, source));
                isInstancePropertySet = true;
            }
            else
            {
                // Initialize property 'TypeName' on the wrapper class as we don't have the instance.
                Expression typeName = CreateTypeNameExpression(source, entityType, _model);
                if (typeName != null)
                {
                    wrapperProperty = wrapperType.GetProperty("TypeName");
                    Contract.Assert(wrapperProperty != null);
                    wrapperTypeMemberAssignments.Add(Expression.Bind(wrapperProperty, typeName));
                    isTypeNamePropertySet = true;
                }
            }

            // Initialize the property 'Container' on the wrapper class
            // source => new Wrapper { Container =  new PropertyContainer { .... } }
            if (selectExpandClause != null)
            {
                Dictionary<IEdmNavigationProperty, SelectExpandClause> propertiesToExpand = GetPropertiesToExpandInQuery(selectExpandClause);
                ISet<IEdmStructuralProperty> autoSelectedProperties;
                ISet<IEdmStructuralProperty> propertiesToInclude = GetPropertiesToIncludeInQuery(selectExpandClause, entityType, out autoSelectedProperties);

                if (propertiesToExpand.Count > 0 || propertiesToInclude.Count > 0 || autoSelectedProperties.Count > 0)
                {
                    wrapperProperty = wrapperType.GetProperty("Container");
                    Contract.Assert(wrapperProperty != null);

                    Expression propertyContainerCreation =
                        BuildPropertyContainer(entityType, source, propertiesToExpand, propertiesToInclude, autoSelectedProperties);

                    wrapperTypeMemberAssignments.Add(Expression.Bind(wrapperProperty, propertyContainerCreation));
                    isContainerPropertySet = true;
                }
            }

            Type wrapperGenericType = GetWrapperGenericType(isInstancePropertySet, isTypeNamePropertySet, isContainerPropertySet);
            wrapperType = wrapperGenericType.MakeGenericType(elementType);
            return Expression.MemberInit(Expression.New(wrapperType), wrapperTypeMemberAssignments);
        }

        private Expression BuildPropertyContainer(IEdmEntityType elementType, Expression source,
            Dictionary<IEdmNavigationProperty, SelectExpandClause> propertiesToExpand,
            ISet<IEdmStructuralProperty> propertiesToInclude, ISet<IEdmStructuralProperty> autoSelectedProperties)
        {
            IList<NamedPropertyExpression> includedProperties = new List<NamedPropertyExpression>();

            foreach (KeyValuePair<IEdmNavigationProperty, SelectExpandClause> kvp in propertiesToExpand)
            {
                IEdmNavigationProperty propertyToExpand = kvp.Key;
                SelectExpandClause projection = kvp.Value;

                Expression propertyName = CreatePropertyNameExpression(elementType, propertyToExpand, source);
                Expression propertyValue = CreatePropertyValueExpression(elementType, propertyToExpand, source);
                Expression nullCheck = Expression.Equal(propertyValue, Expression.Constant(null));

                // projection can be null if the expanded navigation property is not further projected or expanded.
                if (projection != null)
                {
                    propertyValue = ProjectAsWrapper(propertyValue, projection, propertyToExpand.ToEntityType());
                }

                NamedPropertyExpression propertyExpression = new NamedPropertyExpression(propertyName, propertyValue);
                if (projection != null)
                {
                    if (!propertyToExpand.Type.IsCollection())
                    {
                        propertyExpression.NullCheck = nullCheck;
                    }
                    else if (_settings.PageSize != null)
                    {
                        propertyExpression.PageSize = _settings.PageSize.Value;
                    }
                }

                includedProperties.Add(propertyExpression);
            }

            foreach (IEdmStructuralProperty propertyToInclude in propertiesToInclude)
            {
                Expression propertyName = CreatePropertyNameExpression(elementType, propertyToInclude, source);
                Expression propertyValue = CreatePropertyValueExpression(elementType, propertyToInclude, source);
                includedProperties.Add(new NamedPropertyExpression(propertyName, propertyValue));
            }

            foreach (IEdmStructuralProperty propertyToInclude in autoSelectedProperties)
            {
                Expression propertyName = CreatePropertyNameExpression(elementType, propertyToInclude, source);
                Expression propertyValue = CreatePropertyValueExpression(elementType, propertyToInclude, source);
                includedProperties.Add(new NamedPropertyExpression(propertyName, propertyValue) { AutoSelected = true });
            }

            // create a property container that holds all these property names and values.
            return PropertyContainer.CreatePropertyContainer(includedProperties);
        }

        // new CollectionWrapper<ElementType> { Instance = source.Select((ElementType element) => new Wrapper { }) }
        private Expression ProjectCollection(Expression source, Type elementType, SelectExpandClause selectExpandClause, IEdmEntityType entityType)
        {
            ParameterExpression element = Expression.Parameter(elementType);

            // expression
            //      new Wrapper { }
            Expression projection = ProjectElement(element, selectExpandClause, entityType);

            // expression
            //      (ElementType element) => new Wrapper { }
            LambdaExpression selector = Expression.Lambda(projection, element);

            if (_settings.PageSize != null && _settings.PageSize.HasValue)
            {
                // nested paging. Take one more than page size as we need to know whether the collection
                // was truncated or not while generating next page links.
                source = ExpressionHelpers.Take(source, _settings.PageSize.Value + 1, elementType, _settings.EnableConstantParameterization);
            }

            // expression
            //      source.Select((ElementType element) => new Wrapper { })
            Expression selectedExpresion = Expression.Call(GetSelectMethod(elementType, projection.Type), source, selector);

            if (_settings.HandleNullPropagation == HandleNullPropagationOption.True)
            {
                // source == null ? null : projectedCollection
                return Expression.Condition(
                        test: Expression.Equal(source, Expression.Constant(null)),
                        ifTrue: Expression.Constant(null, selectedExpresion.Type),
                        ifFalse: selectedExpresion);
            }
            else
            {
                return selectedExpresion;
            }
        }

        // OData formatter requires the type name of the entity that is being written if the type has derived types.
        // Expression
        //      source is GrandChild ? "GrandChild" : ( source is Child ? "Child" : "Root" )
        // Notice that the order is important here. The most derived type must be the first to check.
        // If entity framework had a way to figure out the type name without selecting the whole object, we don't have to do this magic.
        internal static Expression CreateTypeNameExpression(Expression source, IEdmEntityType elementType, IEdmModel model)
        {
            IReadOnlyList<IEdmEntityType> derivedTypes = GetAllDerivedTypes(elementType, model);

            if (derivedTypes.Count == 0)
            {
                // no inheritance.
                return null;
            }
            else
            {
                Expression expression = Expression.Constant(elementType.FullName());
                for (int i = 0; i < derivedTypes.Count; i++)
                {
                    Type clrType = EdmLibHelpers.GetClrType(derivedTypes[i], model);
                    if (clrType == null)
                    {
                        throw new ODataException(Error.Format(SRResources.MappingDoesNotContainEntityType, derivedTypes[0].FullName()));
                    }

                    expression = Expression.Condition(
                                    test: Expression.TypeIs(source, clrType),
                                    ifTrue: Expression.Constant(derivedTypes[i].FullName()),
                                    ifFalse: expression);
                }

                return expression;
            }
        }

        // returns all the derived types (direct and indirect) of baseType ordered according to their depth. The direct children
        // are the first in the list.
        private static IReadOnlyList<IEdmEntityType> GetAllDerivedTypes(IEdmEntityType baseType, IEdmModel model)
        {
            IEnumerable<IEdmEntityType> allEntityTypes = model.SchemaElements.OfType<IEdmEntityType>();

            List<Tuple<int, IEdmEntityType>> derivedTypes = new List<Tuple<int, IEdmEntityType>>();
            foreach (IEdmEntityType entityType in allEntityTypes)
            {
                int distance = IsDerivedTypeOf(entityType, baseType);
                if (distance > 0)
                {
                    derivedTypes.Add(Tuple.Create(distance, entityType));
                }
            }

            return derivedTypes.OrderBy(tuple => tuple.Item1).Select(tuple => tuple.Item2).ToList();
        }

        // returns -1 if type does not derive from baseType and a positive number representing the distance
        // between them if it does.
        private static int IsDerivedTypeOf(IEdmEntityType type, IEdmEntityType baseType)
        {
            int distance = 0;
            while (type != null)
            {
                if (baseType == type)
                {
                    return distance;
                }

                type = type.BaseEntityType();
                distance++;
            }

            return -1;
        }

        private static MethodInfo GetSelectMethod(Type elementType, Type resultType)
        {
            return ExpressionHelperMethods.EnumerableSelectGeneric.MakeGenericMethod(elementType, resultType);
        }

        private static Dictionary<IEdmNavigationProperty, SelectExpandClause> GetPropertiesToExpandInQuery(SelectExpandClause selectExpandClause)
        {
            Dictionary<IEdmNavigationProperty, SelectExpandClause> properties = new Dictionary<IEdmNavigationProperty, SelectExpandClause>();

            foreach (SelectItem selectItem in selectExpandClause.SelectedItems)
            {
                ExpandedNavigationSelectItem expandItem = selectItem as ExpandedNavigationSelectItem;
                if (expandItem != null)
                {
                    SelectExpandNode.ValidatePathIsSupported(expandItem.PathToNavigationProperty);
                    NavigationPropertySegment navigationSegment = expandItem.PathToNavigationProperty.LastSegment as NavigationPropertySegment;
                    if (navigationSegment == null)
                    {
                        throw new ODataException(SRResources.UnsupportedSelectExpandPath);
                    }

                    properties[navigationSegment.NavigationProperty] = expandItem.SelectAndExpand;
                }
            }

            return properties;
        }

        private static ISet<IEdmStructuralProperty> GetPropertiesToIncludeInQuery(
            SelectExpandClause selectExpandClause, IEdmEntityType entityType, out ISet<IEdmStructuralProperty> autoSelectedProperties)
        {
            autoSelectedProperties = new HashSet<IEdmStructuralProperty>();
            HashSet<IEdmStructuralProperty> propertiesToInclude = new HashSet<IEdmStructuralProperty>();

            IEnumerable<SelectItem> selectedItems = selectExpandClause.SelectedItems;
            if (!IsSelectAll(selectExpandClause))
            {
                // only select requested properties and keys.
                foreach (PathSelectItem pathSelectItem in selectedItems.OfType<PathSelectItem>())
                {
                    SelectExpandNode.ValidatePathIsSupported(pathSelectItem.SelectedPath);
                    PropertySegment structuralPropertySegment = pathSelectItem.SelectedPath.LastSegment as PropertySegment;
                    if (structuralPropertySegment != null)
                    {
                        propertiesToInclude.Add(structuralPropertySegment.Property);
                    }
                }

                // add keys
                foreach (IEdmStructuralProperty keyProperty in entityType.Key())
                {
                    if (!propertiesToInclude.Contains(keyProperty))
                    {
                        autoSelectedProperties.Add(keyProperty);
                    }
                }
            }

            return propertiesToInclude;
        }

        private static bool IsSelectAll(SelectExpandClause selectExpandClause)
        {
            if (selectExpandClause == null)
            {
                return true;
            }

            if (selectExpandClause.AllSelected || selectExpandClause.SelectedItems.OfType<WildcardSelectItem>().Any())
            {
                return true;
            }

            return false;
        }

        private static Type GetWrapperGenericType(bool isInstancePropertySet, bool isTypeNamePropertySet, bool isContainerPropertySet)
        {
            if (isInstancePropertySet)
            {
                // select all
                Contract.Assert(!isTypeNamePropertySet, "we don't set type name if we set instance as it can be figured from instance");

                return isContainerPropertySet ? typeof(SelectAllAndExpand<>) : typeof(SelectAll<>);
            }
            else
            {
                Contract.Assert(isContainerPropertySet, "if it is not select all, container should hold something");

                return isTypeNamePropertySet ? typeof(SelectSomeAndInheritance<>) : typeof(SelectSome<>);
            }
        }

        /* Entityframework requires that the two different type initializers for a given type in the same query have the 
        same set of properties in the same order.
        
        A ~/People?$select=Name&$expand=Friend results in a select expression that has two SelectExpandWrapper<Person>
        expressions, one for the root level person and the second for the expanded Friend person.
        The first wrapper has the Container property set (contains Name and Friend values) where as the second wrapper
        has the Instance property set as it contains all the properties of the expanded person.
        
        The below four classes workaround that entity framework limitation by defining a seperate type for each
        property selection combination possible. */

        private class SelectAllAndExpand<TEntity> : SelectExpandWrapper<TEntity>
        {
        }

        private class SelectAll<TEntity> : SelectExpandWrapper<TEntity>
        {
        }

        private class SelectSomeAndInheritance<TEntity> : SelectExpandWrapper<TEntity>
        {
        }

        private class SelectSome<TEntity> : SelectAllAndExpand<TEntity>
        {
        }
    }
}
