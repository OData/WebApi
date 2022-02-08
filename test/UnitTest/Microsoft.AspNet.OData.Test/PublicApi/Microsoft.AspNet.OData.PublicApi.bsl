[
FlagsAttribute(),
]
public enum Microsoft.AspNet.OData.CompatibilityOptions : int {
	AllowNextLinkWithNonPositiveTopValue = 1
	DisableCaseInsensitiveRequestPropertyBinding = 2
	None = 0
	ThrowExceptionAfterLoggingModelStateError = 4
}

public enum Microsoft.AspNet.OData.EdmDeltaEntityKind : int {
	DeletedEntry = 1
	DeletedLinkEntry = 2
	Entry = 0
	LinkEntry = 3
	Unknown = 4
}

public interface Microsoft.AspNet.OData.IDelta {
	void Clear ()
	System.Collections.Generic.IEnumerable`1[[System.String]] GetChangedPropertyNames ()
	System.Collections.Generic.IEnumerable`1[[System.String]] GetUnchangedPropertyNames ()
	bool TryGetPropertyType (string name, out System.Type& type)
	bool TryGetPropertyValue (string name, out System.Object& value)
	bool TrySetPropertyValue (string name, object value)
}

public interface Microsoft.AspNet.OData.IDeltaDeletedEntityObject {
	System.Uri Id  { public abstract get; public abstract set; }
	Microsoft.OData.Edm.IEdmNavigationSource NavigationSource  { public abstract get; public abstract set; }
	System.Nullable`1[[Microsoft.OData.DeltaDeletedEntryReason]] Reason  { public abstract get; public abstract set; }
}

public interface Microsoft.AspNet.OData.IDeltaSet {
}

public interface Microsoft.AspNet.OData.IDeltaSetItem {
	EdmDeltaEntityKind DeltaKind  { public abstract get; }
	IODataIdContainer ODataIdContainer  { public abstract get; public abstract set; }
	Microsoft.OData.UriParser.ODataPath ODataPath  { public abstract get; public abstract set; }
	IODataInstanceAnnotationContainer TransientInstanceAnnotationContainer  { public abstract get; public abstract set; }
}

public interface Microsoft.AspNet.OData.IEdmChangedObject : IEdmObject, IEdmStructuredObject {
	EdmDeltaEntityKind DeltaKind  { public abstract get; }
}

public interface Microsoft.AspNet.OData.IEdmComplexObject : IEdmObject, IEdmStructuredObject {
}

public interface Microsoft.AspNet.OData.IEdmDeltaDeletedEntityObject : IEdmChangedObject, IEdmObject, IEdmStructuredObject {
	string Id  { public abstract get; public abstract set; }
	Microsoft.OData.DeltaDeletedEntryReason Reason  { public abstract get; public abstract set; }
}

public interface Microsoft.AspNet.OData.IEdmDeltaDeletedLink : IEdmChangedObject, IEdmDeltaLinkBase, IEdmObject, IEdmStructuredObject {
}

public interface Microsoft.AspNet.OData.IEdmDeltaLink : IEdmChangedObject, IEdmDeltaLinkBase, IEdmObject, IEdmStructuredObject {
}

public interface Microsoft.AspNet.OData.IEdmDeltaLinkBase : IEdmChangedObject, IEdmObject, IEdmStructuredObject {
	string Relationship  { public abstract get; public abstract set; }
	System.Uri Source  { public abstract get; public abstract set; }
	System.Uri Target  { public abstract get; public abstract set; }
}

public interface Microsoft.AspNet.OData.IEdmEntityObject : IEdmObject, IEdmStructuredObject {
}

public interface Microsoft.AspNet.OData.IEdmEnumObject : IEdmObject {
}

public interface Microsoft.AspNet.OData.IEdmObject {
	Microsoft.OData.Edm.IEdmTypeReference GetEdmType ()
}

public interface Microsoft.AspNet.OData.IEdmStructuredObject : IEdmObject {
	bool TryGetPropertyValue (string propertyName, out System.Object& value)
}

public interface Microsoft.AspNet.OData.IODataIdContainer {
	string ODataId  { public abstract get; public abstract set; }
}

public interface Microsoft.AspNet.OData.IPerRouteContainer {
	System.Func`1[[Microsoft.OData.IContainerBuilder]] BuilderFactory  { public abstract get; public abstract set; }

	void AddRoute (string routeName, string routePrefix)
	System.IServiceProvider CreateODataRootContainer (string routeName, System.Action`1[[Microsoft.OData.IContainerBuilder]] configureAction)
	System.IServiceProvider GetODataRootContainer (string routeName)
	string GetRoutePrefix (string routeName)
	bool HasODataRootContainer (string routeName)
}

[
NonValidatingParameterBindingAttribute(),
]
public abstract class Microsoft.AspNet.OData.Delta : System.Dynamic.DynamicObject, IDynamicMetaObjectProvider, IDelta {
	protected Delta ()

	public abstract void Clear ()
	public abstract System.Collections.Generic.IEnumerable`1[[System.String]] GetChangedPropertyNames ()
	public abstract System.Collections.Generic.IEnumerable`1[[System.String]] GetUnchangedPropertyNames ()
	public virtual bool TryGetMember (System.Dynamic.GetMemberBinder binder, out System.Object& result)
	public abstract bool TryGetPropertyType (string name, out System.Type& type)
	public abstract bool TryGetPropertyValue (string name, out System.Object& value)
	public virtual bool TrySetMember (System.Dynamic.SetMemberBinder binder, object value)
	public abstract bool TrySetPropertyValue (string name, object value)
}

[
NonValidatingParameterBindingAttribute(),
]
public abstract class Microsoft.AspNet.OData.EdmStructuredObject : Delta, IDynamicMetaObjectProvider, IDelta, IEdmObject, IEdmStructuredObject {
	protected EdmStructuredObject (Microsoft.OData.Edm.IEdmStructuredType edmType)
	protected EdmStructuredObject (Microsoft.OData.Edm.IEdmStructuredTypeReference edmType)
	protected EdmStructuredObject (Microsoft.OData.Edm.IEdmStructuredType edmType, bool isNullable)

	Microsoft.OData.Edm.IEdmStructuredType ActualEdmType  { public get; public set; }
	Microsoft.OData.Edm.IEdmStructuredType ExpectedEdmType  { public get; public set; }
	bool IsNullable  { public get; public set; }

	public virtual void Clear ()
	public virtual System.Collections.Generic.IEnumerable`1[[System.String]] GetChangedPropertyNames ()
	public virtual Microsoft.OData.Edm.IEdmTypeReference GetEdmType ()
	public virtual System.Collections.Generic.IEnumerable`1[[System.String]] GetUnchangedPropertyNames ()
	public System.Collections.Generic.Dictionary`2[[System.String],[System.Object]] TryGetDynamicProperties ()
	public virtual bool TryGetPropertyType (string name, out System.Type& type)
	public virtual bool TryGetPropertyValue (string name, out System.Object& value)
	public virtual bool TrySetPropertyValue (string name, object value)
}

[
ODataFormattingAttribute(),
ODataRoutingAttribute(),
ApiExplorerSettingsAttribute(),
]
public abstract class Microsoft.AspNet.OData.ODataController : System.Web.Http.ApiController, IDisposable, IHttpController {
	protected ODataController ()

	protected virtual CreatedODataResult`1 Created (TEntity entity)
	protected virtual void Dispose (bool disposing)
	protected virtual UpdatedODataResult`1 Updated (TEntity entity)
}

[
DataContractAttribute(),
]
public abstract class Microsoft.AspNet.OData.PageResult {
	protected PageResult (System.Uri nextPageLink, System.Nullable`1[[System.Int64]] count)

	[
	DataMemberAttribute(),
	]
	System.Nullable`1[[System.Int64]] Count  { public get; }

	[
	DataMemberAttribute(),
	]
	System.Uri NextPageLink  { public get; }
}

public abstract class Microsoft.AspNet.OData.PerRouteContainerBase : IPerRouteContainer {
	protected PerRouteContainerBase ()

	System.Func`1[[Microsoft.OData.IContainerBuilder]] BuilderFactory  { public virtual get; public virtual set; }

	public virtual void AddRoute (string routeName, string routePrefix)
	protected Microsoft.OData.IContainerBuilder CreateContainerBuilderWithCoreServices ()
	public System.IServiceProvider CreateODataRootContainer (System.Action`1[[Microsoft.OData.IContainerBuilder]] configureAction)
	public virtual System.IServiceProvider CreateODataRootContainer (string routeName, System.Action`1[[Microsoft.OData.IContainerBuilder]] configureAction)
	protected abstract System.IServiceProvider GetContainer (string routeName)
	public virtual System.IServiceProvider GetODataRootContainer (string routeName)
	public virtual string GetRoutePrefix (string routeName)
	public virtual bool HasODataRootContainer (string routeName)
	protected abstract void SetContainer (string routeName, System.IServiceProvider rootContainer)
}

public abstract class Microsoft.AspNet.OData.TypedDelta : Delta, IDynamicMetaObjectProvider, IDelta {
	protected TypedDelta ()

	System.Type ExpectedClrType  { public abstract get; }
	System.Type StructuredType  { public abstract get; }
}

[
EditorBrowsableAttribute(),
ExtensionAttribute(),
]
public sealed class Microsoft.AspNet.OData.EdmModelExtensions {
	[
	ExtensionAttribute(),
	]
	public static NavigationSourceLinkBuilderAnnotation GetNavigationSourceLinkBuilder (Microsoft.OData.Edm.IEdmModel model, Microsoft.OData.Edm.IEdmNavigationSource navigationSource)

	[
	ExtensionAttribute(),
	]
	public static OperationLinkBuilder GetOperationLinkBuilder (Microsoft.OData.Edm.IEdmModel model, Microsoft.OData.Edm.IEdmOperation operation)

	[
	ExtensionAttribute(),
	]
	public static void SetNavigationSourceLinkBuilder (Microsoft.OData.Edm.IEdmModel model, Microsoft.OData.Edm.IEdmNavigationSource navigationSource, NavigationSourceLinkBuilderAnnotation navigationSourceLinkBuilder)

	[
	ExtensionAttribute(),
	]
	public static void SetOperationLinkBuilder (Microsoft.OData.Edm.IEdmModel model, Microsoft.OData.Edm.IEdmOperation operation, OperationLinkBuilder operationLinkBuilder)
}

[
ExtensionAttribute(),
]
public sealed class Microsoft.AspNet.OData.EdmTypeExtensions {
	[
	ExtensionAttribute(),
	]
	public static bool IsDeltaFeed (Microsoft.OData.Edm.IEdmType type)

	[
	ExtensionAttribute(),
	]
	public static bool IsDeltaResource (IEdmObject resource)
}

public sealed class Microsoft.AspNet.OData.ODataUriFunctions {
	public static void AddCustomUriFunction (string functionName, Microsoft.OData.UriParser.FunctionSignatureWithReturnType functionSignature, System.Reflection.MethodInfo methodInfo)
	public static bool RemoveCustomUriFunction (string functionName, Microsoft.OData.UriParser.FunctionSignatureWithReturnType functionSignature, System.Reflection.MethodInfo methodInfo)
}

public class Microsoft.AspNet.OData.ClrEnumMemberAnnotation {
	public ClrEnumMemberAnnotation (System.Collections.Generic.IDictionary`2[[System.Enum],[Microsoft.OData.Edm.IEdmEnumMember]] map)

	public System.Enum GetClrEnumMember (Microsoft.OData.Edm.IEdmEnumMember edmEnumMember)
	public Microsoft.OData.Edm.IEdmEnumMember GetEdmEnumMember (System.Enum clrEnumMemberInfo)
}

public class Microsoft.AspNet.OData.ClrPropertyInfoAnnotation {
	public ClrPropertyInfoAnnotation (System.Reflection.PropertyInfo clrPropertyInfo)

	System.Reflection.PropertyInfo ClrPropertyInfo  { public get; }
}

public class Microsoft.AspNet.OData.ClrTypeAnnotation {
	public ClrTypeAnnotation (System.Type clrType)

	System.Type ClrType  { public get; }
}

public class Microsoft.AspNet.OData.ConcurrencyPropertiesAnnotation : System.Collections.Concurrent.ConcurrentDictionary`2[[Microsoft.OData.Edm.IEdmNavigationSource],[System.Collections.Generic.IEnumerable`1[[Microsoft.OData.Edm.IEdmStructuralProperty]]]], ICollection, IDictionary, IEnumerable, IDictionary`2, IReadOnlyDictionary`2, ICollection`1, IEnumerable`1, IReadOnlyCollection`1 {
	public ConcurrencyPropertiesAnnotation ()
}

public class Microsoft.AspNet.OData.CustomAggregateMethodAnnotation {
	public CustomAggregateMethodAnnotation ()

	public CustomAggregateMethodAnnotation AddMethod (string methodToken, System.Collections.Generic.IDictionary`2[[System.Type],[System.Reflection.MethodInfo]] methods)
	public bool GetMethodInfo (string methodToken, System.Type returnType, out System.Reflection.MethodInfo& methodInfo)
}

public class Microsoft.AspNet.OData.DefaultContainerBuilder : IContainerBuilder {
	public DefaultContainerBuilder ()

	public virtual Microsoft.OData.IContainerBuilder AddService (Microsoft.OData.ServiceLifetime lifetime, System.Type serviceType, System.Func`2[[System.IServiceProvider],[System.Object]] implementationFactory)
	public virtual Microsoft.OData.IContainerBuilder AddService (Microsoft.OData.ServiceLifetime lifetime, System.Type serviceType, System.Type implementationType)
	public virtual System.IServiceProvider BuildContainer ()
}

[
NonValidatingParameterBindingAttribute(),
]
public class Microsoft.AspNet.OData.Delta`1 : TypedDelta, IDynamicMetaObjectProvider, IDelta, IDeltaSetItem {
	public Delta`1 ()
	public Delta`1 (System.Type structuralType)
	public Delta`1 (System.Type structuralType, System.Collections.Generic.IEnumerable`1[[System.String]] updatableProperties)
	public Delta`1 (System.Type structuralType, System.Collections.Generic.IEnumerable`1[[System.String]] updatableProperties, System.Reflection.PropertyInfo dynamicDictionaryPropertyInfo)
	public Delta`1 (System.Type structuralType, System.Collections.Generic.IEnumerable`1[[System.String]] updatableProperties, System.Reflection.PropertyInfo dynamicDictionaryPropertyInfo, bool isComplexType)
	public Delta`1 (System.Type structuralType, System.Collections.Generic.IEnumerable`1[[System.String]] updatableProperties, System.Reflection.PropertyInfo dynamicDictionaryPropertyInfo, bool isComplexType, System.Reflection.PropertyInfo instanceAnnotationsPropertyInfo)

	EdmDeltaEntityKind DeltaKind  { public virtual get; protected set; }
	System.Type ExpectedClrType  { public virtual get; }
	bool IsComplexType  { public get; }
	IODataIdContainer ODataIdContainer  { public virtual get; public virtual set; }
	Microsoft.OData.UriParser.ODataPath ODataPath  { public virtual get; public virtual set; }
	System.Type StructuredType  { public virtual get; }
	IODataInstanceAnnotationContainer TransientInstanceAnnotationContainer  { public virtual get; public virtual set; }
	System.Collections.Generic.IList`1[[System.String]] UpdatableProperties  { public get; }

	public virtual void Clear ()
	public TStructuralType CopyChangedValues (TStructuralType original)
	public void CopyUnchangedValues (TStructuralType original)
	public virtual System.Collections.Generic.IEnumerable`1[[System.String]] GetChangedPropertyNames ()
	public TStructuralType GetInstance ()
	public virtual System.Collections.Generic.IEnumerable`1[[System.String]] GetUnchangedPropertyNames ()
	public void Patch (TStructuralType original)
	public void Put (TStructuralType original)
	public bool TryGetNestedPropertyValue (string name, out System.Object& value)
	public virtual bool TryGetPropertyType (string name, out System.Type& type)
	public virtual bool TryGetPropertyValue (string name, out System.Object& value)
	public virtual bool TrySetPropertyValue (string name, object value)
}

[
NonValidatingParameterBindingAttribute(),
]
public class Microsoft.AspNet.OData.DeltaDeletedEntityObject`1 : Delta`1, IDynamicMetaObjectProvider, IDelta, IDeltaDeletedEntityObject, IDeltaSetItem {
	public DeltaDeletedEntityObject`1 ()
	public DeltaDeletedEntityObject`1 (System.Type structuralType)
	public DeltaDeletedEntityObject`1 (System.Type structuralType, System.Collections.Generic.IEnumerable`1[[System.String]] updatableProperties)
	public DeltaDeletedEntityObject`1 (System.Type structuralType, System.Reflection.PropertyInfo instanceAnnotationsPropertyInfo)
	public DeltaDeletedEntityObject`1 (System.Type structuralType, System.Reflection.PropertyInfo dynamicDictionaryPropertyInfo, System.Reflection.PropertyInfo instanceAnnotationsPropertyInfo)
	public DeltaDeletedEntityObject`1 (System.Type structuralType, System.Collections.Generic.IEnumerable`1[[System.String]] updatableProperties, System.Reflection.PropertyInfo dynamicDictionaryPropertyInfo, System.Reflection.PropertyInfo instanceAnnotationsPropertyInfo)
	public DeltaDeletedEntityObject`1 (System.Type structuralType, System.Collections.Generic.IEnumerable`1[[System.String]] updatableProperties, System.Reflection.PropertyInfo dynamicDictionaryPropertyInfo, bool isComplexType, System.Reflection.PropertyInfo instanceAnnotationsPropertyInfo)

	System.Uri Id  { public virtual get; public virtual set; }
	Microsoft.OData.Edm.IEdmNavigationSource NavigationSource  { public virtual get; public virtual set; }
	System.Nullable`1[[Microsoft.OData.DeltaDeletedEntryReason]] Reason  { public virtual get; public virtual set; }
}

[
NonValidatingParameterBindingAttribute(),
]
public class Microsoft.AspNet.OData.DeltaSet`1 : System.Collections.ObjectModel.Collection`1[[Microsoft.AspNet.OData.IDeltaSetItem]], ICollection, IEnumerable, IList, IDeltaSet, ICollection`1, IEnumerable`1, IList`1, IReadOnlyCollection`1, IReadOnlyList`1 {
	public DeltaSet`1 (System.Collections.Generic.IList`1[[System.String]] keys)

	protected virtual void InsertItem (int index, IDeltaSetItem item)
}

[
NonValidatingParameterBindingAttribute(),
]
public class Microsoft.AspNet.OData.EdmChangedObjectCollection : System.Collections.ObjectModel.Collection`1[[Microsoft.AspNet.OData.IEdmChangedObject]], ICollection, IEnumerable, IList, IEdmObject, ICollection`1, IEnumerable`1, IList`1, IReadOnlyCollection`1, IReadOnlyList`1 {
	public EdmChangedObjectCollection (Microsoft.OData.Edm.IEdmEntityType entityType)
	public EdmChangedObjectCollection (Microsoft.OData.Edm.IEdmEntityType entityType, System.Collections.Generic.IList`1[[Microsoft.AspNet.OData.IEdmChangedObject]] changedObjectList)

	Microsoft.OData.Edm.IEdmEntityType EntityType  { public get; }

	public virtual Microsoft.OData.Edm.IEdmTypeReference GetEdmType ()
}

[
NonValidatingParameterBindingAttribute(),
]
public class Microsoft.AspNet.OData.EdmComplexObject : EdmStructuredObject, IDynamicMetaObjectProvider, IDelta, IEdmComplexObject, IEdmObject, IEdmStructuredObject {
	public EdmComplexObject (Microsoft.OData.Edm.IEdmComplexType edmType)
	public EdmComplexObject (Microsoft.OData.Edm.IEdmComplexTypeReference edmType)
	public EdmComplexObject (Microsoft.OData.Edm.IEdmComplexType edmType, bool isNullable)
}

[
NonValidatingParameterBindingAttribute(),
]
public class Microsoft.AspNet.OData.EdmComplexObjectCollection : System.Collections.ObjectModel.Collection`1[[Microsoft.AspNet.OData.IEdmComplexObject]], ICollection, IEnumerable, IList, IEdmObject, ICollection`1, IEnumerable`1, IList`1, IReadOnlyCollection`1, IReadOnlyList`1 {
	public EdmComplexObjectCollection (Microsoft.OData.Edm.IEdmCollectionTypeReference edmType)
	public EdmComplexObjectCollection (Microsoft.OData.Edm.IEdmCollectionTypeReference edmType, System.Collections.Generic.IList`1[[Microsoft.AspNet.OData.IEdmComplexObject]] list)

	public virtual Microsoft.OData.Edm.IEdmTypeReference GetEdmType ()
}

[
NonValidatingParameterBindingAttribute(),
]
public class Microsoft.AspNet.OData.EdmDeltaComplexObject : EdmComplexObject, IDynamicMetaObjectProvider, IDelta, IEdmComplexObject, IEdmObject, IEdmStructuredObject {
	public EdmDeltaComplexObject (Microsoft.OData.Edm.IEdmComplexType edmType)
	public EdmDeltaComplexObject (Microsoft.OData.Edm.IEdmComplexTypeReference edmType)
	public EdmDeltaComplexObject (Microsoft.OData.Edm.IEdmComplexType edmType, bool isNullable)
}

[
NonValidatingParameterBindingAttribute(),
]
public class Microsoft.AspNet.OData.EdmDeltaDeletedEntityObject : EdmEntityObject, IDynamicMetaObjectProvider, IDelta, IEdmChangedObject, IEdmDeltaDeletedEntityObject, IEdmEntityObject, IEdmObject, IEdmStructuredObject {
	public EdmDeltaDeletedEntityObject (Microsoft.OData.Edm.IEdmEntityType entityType)
	public EdmDeltaDeletedEntityObject (Microsoft.OData.Edm.IEdmEntityTypeReference entityTypeReference)
	public EdmDeltaDeletedEntityObject (Microsoft.OData.Edm.IEdmEntityType entityType, bool isNullable)

	EdmDeltaEntityKind DeltaKind  { public virtual get; }
	string Id  { public virtual get; public virtual set; }
	Microsoft.OData.Edm.IEdmNavigationSource NavigationSource  { public get; public set; }
	Microsoft.OData.DeltaDeletedEntryReason Reason  { public virtual get; public virtual set; }
}

[
NonValidatingParameterBindingAttribute(),
]
public class Microsoft.AspNet.OData.EdmDeltaDeletedLink : EdmEntityObject, IDynamicMetaObjectProvider, IDelta, IEdmChangedObject, IEdmDeltaDeletedLink, IEdmDeltaLinkBase, IEdmEntityObject, IEdmObject, IEdmStructuredObject {
	public EdmDeltaDeletedLink (Microsoft.OData.Edm.IEdmEntityType entityType)
	public EdmDeltaDeletedLink (Microsoft.OData.Edm.IEdmEntityTypeReference entityTypeReference)
	public EdmDeltaDeletedLink (Microsoft.OData.Edm.IEdmEntityType entityType, bool isNullable)

	EdmDeltaEntityKind DeltaKind  { public virtual get; }
	string Relationship  { public virtual get; public virtual set; }
	System.Uri Source  { public virtual get; public virtual set; }
	System.Uri Target  { public virtual get; public virtual set; }
}

[
NonValidatingParameterBindingAttribute(),
]
public class Microsoft.AspNet.OData.EdmDeltaEntityObject : EdmEntityObject, IDynamicMetaObjectProvider, IDelta, IEdmChangedObject, IEdmEntityObject, IEdmObject, IEdmStructuredObject {
	public EdmDeltaEntityObject (Microsoft.OData.Edm.IEdmEntityType entityType)
	public EdmDeltaEntityObject (Microsoft.OData.Edm.IEdmEntityTypeReference entityTypeReference)
	public EdmDeltaEntityObject (Microsoft.OData.Edm.IEdmEntityType entityType, bool isNullable)

	EdmDeltaEntityKind DeltaKind  { public virtual get; }
	Microsoft.OData.Edm.IEdmNavigationSource NavigationSource  { public get; public set; }
}

[
NonValidatingParameterBindingAttribute(),
]
public class Microsoft.AspNet.OData.EdmDeltaLink : EdmEntityObject, IDynamicMetaObjectProvider, IDelta, IEdmChangedObject, IEdmDeltaLink, IEdmDeltaLinkBase, IEdmEntityObject, IEdmObject, IEdmStructuredObject {
	public EdmDeltaLink (Microsoft.OData.Edm.IEdmEntityType entityType)
	public EdmDeltaLink (Microsoft.OData.Edm.IEdmEntityTypeReference entityTypeReference)
	public EdmDeltaLink (Microsoft.OData.Edm.IEdmEntityType entityType, bool isNullable)

	EdmDeltaEntityKind DeltaKind  { public virtual get; }
	string Relationship  { public virtual get; public virtual set; }
	System.Uri Source  { public virtual get; public virtual set; }
	System.Uri Target  { public virtual get; public virtual set; }
}

[
NonValidatingParameterBindingAttribute(),
]
public class Microsoft.AspNet.OData.EdmEntityObject : EdmStructuredObject, IDynamicMetaObjectProvider, IDelta, IEdmChangedObject, IEdmEntityObject, IEdmObject, IEdmStructuredObject {
	public EdmEntityObject (Microsoft.OData.Edm.IEdmEntityType edmType)
	public EdmEntityObject (Microsoft.OData.Edm.IEdmEntityTypeReference edmType)
	public EdmEntityObject (Microsoft.OData.Edm.IEdmEntityType edmType, bool isNullable)

	EdmDeltaEntityKind DeltaKind  { public virtual get; }
	IODataIdContainer ODataIdContainer  { public get; public set; }
	IODataInstanceAnnotationContainer PersistentInstanceAnnotationsContainer  { public get; public set; }

	public void AddDataException (Org.OData.Core.V1.DataModificationExceptionType dataModificationException)
	public Org.OData.Core.V1.DataModificationExceptionType GetDataException ()
}

[
NonValidatingParameterBindingAttribute(),
]
public class Microsoft.AspNet.OData.EdmEntityObjectCollection : System.Collections.ObjectModel.Collection`1[[Microsoft.AspNet.OData.IEdmEntityObject]], ICollection, IEnumerable, IList, IEdmObject, ICollection`1, IEnumerable`1, IList`1, IReadOnlyCollection`1, IReadOnlyList`1 {
	public EdmEntityObjectCollection (Microsoft.OData.Edm.IEdmCollectionTypeReference edmType)
	public EdmEntityObjectCollection (Microsoft.OData.Edm.IEdmCollectionTypeReference edmType, System.Collections.Generic.IList`1[[Microsoft.AspNet.OData.IEdmEntityObject]] list)

	public virtual Microsoft.OData.Edm.IEdmTypeReference GetEdmType ()
}

[
NonValidatingParameterBindingAttribute(),
]
public class Microsoft.AspNet.OData.EdmEnumObject : IEdmEnumObject, IEdmObject {
	public EdmEnumObject (Microsoft.OData.Edm.IEdmEnumType edmType, string value)
	public EdmEnumObject (Microsoft.OData.Edm.IEdmEnumTypeReference edmType, string value)
	public EdmEnumObject (Microsoft.OData.Edm.IEdmEnumType edmType, string value, bool isNullable)

	bool IsNullable  { public get; public set; }
	string Value  { public get; public set; }

	public virtual Microsoft.OData.Edm.IEdmTypeReference GetEdmType ()
}

[
NonValidatingParameterBindingAttribute(),
]
public class Microsoft.AspNet.OData.EdmEnumObjectCollection : System.Collections.ObjectModel.Collection`1[[Microsoft.AspNet.OData.IEdmEnumObject]], ICollection, IEnumerable, IList, IEdmObject, ICollection`1, IEnumerable`1, IList`1, IReadOnlyCollection`1, IReadOnlyList`1 {
	public EdmEnumObjectCollection (Microsoft.OData.Edm.IEdmCollectionTypeReference edmType)
	public EdmEnumObjectCollection (Microsoft.OData.Edm.IEdmCollectionTypeReference edmType, System.Collections.Generic.IList`1[[Microsoft.AspNet.OData.IEdmEnumObject]] list)

	public virtual Microsoft.OData.Edm.IEdmTypeReference GetEdmType ()
}

[
AttributeUsageAttribute(),
]
public class Microsoft.AspNet.OData.EnableQueryAttribute : System.Web.Http.Filters.ActionFilterAttribute, _Attribute, IActionFilter, IFilter {
	public EnableQueryAttribute ()

	AllowedArithmeticOperators AllowedArithmeticOperators  { public get; public set; }
	AllowedFunctions AllowedFunctions  { public get; public set; }
	AllowedLogicalOperators AllowedLogicalOperators  { public get; public set; }
	string AllowedOrderByProperties  { public get; public set; }
	AllowedQueryOptions AllowedQueryOptions  { public get; public set; }
	bool EnableConstantParameterization  { public get; public set; }
	bool EnableCorrelatedSubqueryBuffering  { public get; public set; }
	bool EnsureStableOrdering  { public get; public set; }
	HandleNullPropagationOption HandleNullPropagation  { public get; public set; }
	bool HandleReferenceNavigationPropertyExpandFilter  { public get; public set; }
	int MaxAnyAllExpressionDepth  { public get; public set; }
	int MaxExpansionDepth  { public get; public set; }
	int MaxNodeCount  { public get; public set; }
	int MaxOrderByNodeCount  { public get; public set; }
	int MaxSkip  { public get; public set; }
	int MaxTop  { public get; public set; }
	int PageSize  { public get; public set; }

	public virtual System.Linq.IQueryable ApplyQuery (System.Linq.IQueryable queryable, ODataQueryOptions queryOptions)
	public virtual object ApplyQuery (object entity, ODataQueryOptions queryOptions)
	public virtual Microsoft.OData.Edm.IEdmModel GetModel (System.Type elementClrType, System.Net.Http.HttpRequestMessage request, System.Web.Http.Controllers.HttpActionDescriptor actionDescriptor)
	public virtual void OnActionExecuted (System.Web.Http.Filters.HttpActionExecutedContext actionExecutedContext)
	public virtual void ValidateQuery (System.Net.Http.HttpRequestMessage request, ODataQueryOptions queryOptions)
}

public class Microsoft.AspNet.OData.ETagMessageHandler : System.Net.Http.DelegatingHandler, IDisposable {
	public ETagMessageHandler ()

	[
	AsyncStateMachineAttribute(),
	]
	protected virtual System.Threading.Tasks.Task`1[[System.Net.Http.HttpResponseMessage]] SendAsync (System.Net.Http.HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
}

public class Microsoft.AspNet.OData.HttpRequestScope {
	public HttpRequestScope ()

	System.Net.Http.HttpRequestMessage HttpRequest  { public get; public set; }
}

public class Microsoft.AspNet.OData.MetadataController : ODataController, IDisposable, IHttpController {
	public MetadataController ()

	public Microsoft.OData.Edm.IEdmModel GetMetadata ()
	public Microsoft.OData.ODataServiceDocument GetServiceDocument ()
}

public class Microsoft.AspNet.OData.NavigationPath : System.Collections.Generic.List`1[[Microsoft.AspNet.OData.PathItem]], ICollection, IEnumerable, IList, ICollection`1, IEnumerable`1, IList`1, IReadOnlyCollection`1, IReadOnlyList`1 {
	public NavigationPath (Microsoft.OData.UriParser.ODataPath path)
	public NavigationPath (string path, Microsoft.OData.Edm.IEdmModel model)
}

public class Microsoft.AspNet.OData.NullEdmComplexObject : IEdmComplexObject, IEdmObject, IEdmStructuredObject {
	public NullEdmComplexObject (Microsoft.OData.Edm.IEdmComplexTypeReference edmType)

	public virtual Microsoft.OData.Edm.IEdmTypeReference GetEdmType ()
	public virtual bool TryGetPropertyValue (string propertyName, out System.Object& value)
}

[
NonValidatingParameterBindingAttribute(),
]
public class Microsoft.AspNet.OData.ODataActionParameters : System.Collections.Generic.Dictionary`2[[System.String],[System.Object]], ICollection, IDictionary, IEnumerable, IDeserializationCallback, ISerializable, IDictionary`2, IReadOnlyDictionary`2, ICollection`1, IEnumerable`1, IReadOnlyCollection`1 {
	public ODataActionParameters ()
}

[
AttributeUsageAttribute(),
]
public class Microsoft.AspNet.OData.ODataFormattingAttribute : System.Attribute, _Attribute, IControllerConfiguration {
	public ODataFormattingAttribute ()

	public virtual System.Collections.Generic.IList`1[[Microsoft.AspNet.OData.Formatter.ODataMediaTypeFormatter]] CreateODataFormatters ()
	public virtual void Initialize (System.Web.Http.Controllers.HttpControllerSettings controllerSettings, System.Web.Http.Controllers.HttpControllerDescriptor controllerDescriptor)
}

public class Microsoft.AspNet.OData.ODataIdContainer : IODataIdContainer {
	public ODataIdContainer ()

	string ODataId  { public virtual get; public virtual set; }
}

public class Microsoft.AspNet.OData.ODataNullValueMessageHandler : System.Net.Http.DelegatingHandler, IDisposable {
	public ODataNullValueMessageHandler ()

	[
	AsyncStateMachineAttribute(),
	]
	protected virtual System.Threading.Tasks.Task`1[[System.Net.Http.HttpResponseMessage]] SendAsync (System.Net.Http.HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
}

public class Microsoft.AspNet.OData.ODataQueryContext {
	public ODataQueryContext (Microsoft.OData.Edm.IEdmModel model, Microsoft.OData.Edm.IEdmType elementType, ODataPath path)
	public ODataQueryContext (Microsoft.OData.Edm.IEdmModel model, System.Type elementClrType, ODataPath path)

	DefaultQuerySettings DefaultQuerySettings  { public get; }
	System.Type ElementClrType  { public get; }
	Microsoft.OData.Edm.IEdmType ElementType  { public get; }
	Microsoft.OData.Edm.IEdmModel Model  { public get; }
	Microsoft.OData.Edm.IEdmNavigationSource NavigationSource  { public get; }
	ODataPath Path  { public get; }
	System.IServiceProvider RequestContainer  { public get; }
}

public class Microsoft.AspNet.OData.ODataSwaggerConverter {
	public ODataSwaggerConverter (Microsoft.OData.Edm.IEdmModel model)

	string BasePath  { public get; public set; }
	Microsoft.OData.Edm.IEdmModel EdmModel  { public get; }
	string Host  { public get; public set; }
	System.Uri MetadataUri  { public get; public set; }
	Newtonsoft.Json.Linq.JObject SwaggerDocument  { protected virtual get; protected virtual set; }
	Newtonsoft.Json.Linq.JObject SwaggerPaths  { protected virtual get; protected virtual set; }
	Newtonsoft.Json.Linq.JObject SwaggerTypeDefinitions  { protected virtual get; protected virtual set; }
	System.Version SwaggerVersion  { public virtual get; }

	public virtual Newtonsoft.Json.Linq.JObject GetSwaggerModel ()
	protected virtual void InitializeContainer ()
	protected virtual void InitializeDocument ()
	protected virtual void InitializeEnd ()
	protected virtual void InitializeOperations ()
	protected virtual void InitializeStart ()
	protected virtual void InitializeTypeDefinitions ()
}

[
NonValidatingParameterBindingAttribute(),
]
public class Microsoft.AspNet.OData.ODataUntypedActionParameters : System.Collections.Generic.Dictionary`2[[System.String],[System.Object]], ICollection, IDictionary, IEnumerable, IDeserializationCallback, ISerializable, IDictionary`2, IReadOnlyDictionary`2, ICollection`1, IEnumerable`1, IReadOnlyCollection`1 {
	public ODataUntypedActionParameters (Microsoft.OData.Edm.IEdmAction action)

	Microsoft.OData.Edm.IEdmAction Action  { public get; }
}

[
JsonObjectAttribute(),
DataContractAttribute(),
]
public class Microsoft.AspNet.OData.PageResult`1 : PageResult, IEnumerable`1, IEnumerable {
	public PageResult`1 (IEnumerable`1 items, System.Uri nextPageLink, System.Nullable`1[[System.Int64]] count)

	[
	DataMemberAttribute(),
	]
	IEnumerable`1 Items  { public get; }

	public virtual IEnumerator`1 GetEnumerator ()
	System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
}

public class Microsoft.AspNet.OData.PathItem {
	public PathItem ()

	string CastTypeName  { public get; }
	bool IsCastType  { public get; }
	System.Collections.Generic.Dictionary`2[[System.String],[System.Object]] KeyProperties  { public get; }
	string Name  { public get; }
}

public class Microsoft.AspNet.OData.PerRouteContainer : PerRouteContainerBase, IPerRouteContainer {
	public PerRouteContainer (System.Web.Http.HttpConfiguration configuration)

	protected virtual System.IServiceProvider GetContainer (string routeName)
	protected virtual void SetContainer (string routeName, System.IServiceProvider rootContainer)
}

public class Microsoft.AspNet.OData.QueryableRestrictions {
	public QueryableRestrictions ()
	public QueryableRestrictions (PropertyConfiguration propertyConfiguration)

	bool AutoExpand  { public get; public set; }
	bool DisableAutoExpandWhenSelectIsPresent  { public get; public set; }
	bool NonFilterable  { public get; public set; }
	bool NotCountable  { public get; public set; }
	bool NotExpandable  { public get; public set; }
	bool NotFilterable  { public get; public set; }
	bool NotNavigable  { public get; public set; }
	bool NotSortable  { public get; public set; }
	bool Unsortable  { public get; public set; }
}

public class Microsoft.AspNet.OData.QueryableRestrictionsAnnotation {
	public QueryableRestrictionsAnnotation (QueryableRestrictions restrictions)

	QueryableRestrictions Restrictions  { public get; }
}

public class Microsoft.AspNet.OData.ResourceContext {
	public ResourceContext ()
	public ResourceContext (ODataSerializerContext serializerContext, Microsoft.OData.Edm.IEdmStructuredTypeReference structuredType, object resourceInstance)

	System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] DynamicComplexProperties  { public get; public set; }
	Microsoft.OData.Edm.IEdmModel EdmModel  { public get; public set; }
	IEdmStructuredObject EdmObject  { public get; public set; }
	Microsoft.OData.Edm.IEdmNavigationSource NavigationSource  { public get; public set; }
	System.Net.Http.HttpRequestMessage Request  { public get; public set; }
	object ResourceInstance  { public get; public set; }
	ODataSerializerContext SerializerContext  { public get; public set; }
	bool SkipExpensiveAvailabilityChecks  { public get; public set; }
	Microsoft.OData.Edm.IEdmStructuredType StructuredType  { public get; public set; }
	System.Web.Http.Routing.UrlHelper Url  { public get; public set; }

	public object GetPropertyValue (string propertyName)
}

public class Microsoft.AspNet.OData.ResourceContext`1 : ResourceContext {
	public ResourceContext`1 ()

	[
	ObsoleteAttribute(),
	]
	TStructuredType ResourceInstance  { public get; public set; }
}

public class Microsoft.AspNet.OData.ResourceSetContext {
	public ResourceSetContext ()

	Microsoft.OData.Edm.IEdmModel EdmModel  { public get; }
	Microsoft.OData.Edm.IEdmEntitySetBase EntitySetBase  { public get; public set; }
	System.Net.Http.HttpRequestMessage Request  { public get; public set; }
	System.Web.Http.Controllers.HttpRequestContext RequestContext  { public get; public set; }
	object ResourceSetInstance  { public get; public set; }
	System.Web.Http.Routing.UrlHelper Url  { public get; public set; }
}

public class Microsoft.AspNet.OData.UnqualifiedCallAndEnumPrefixFreeResolver : Microsoft.OData.UriParser.ODataUriResolver {
	public UnqualifiedCallAndEnumPrefixFreeResolver ()

	bool EnableCaseInsensitive  { public virtual get; public virtual set; }

	public virtual void PromoteBinaryOperandTypes (Microsoft.OData.UriParser.BinaryOperatorKind binaryOperatorKind, Microsoft.OData.UriParser.SingleValueNode& leftNode, Microsoft.OData.UriParser.SingleValueNode& rightNode, out Microsoft.OData.Edm.IEdmTypeReference& typeReference)
	public virtual System.Collections.Generic.IEnumerable`1[[Microsoft.OData.Edm.IEdmOperation]] ResolveBoundOperations (Microsoft.OData.Edm.IEdmModel model, string identifier, Microsoft.OData.Edm.IEdmType bindingType)
	public virtual System.Collections.Generic.IEnumerable`1[[System.Collections.Generic.KeyValuePair`2[[System.String],[System.Object]]]] ResolveKeys (Microsoft.OData.Edm.IEdmEntityType type, System.Collections.Generic.IDictionary`2[[System.String],[System.String]] namedValues, System.Func`3[[Microsoft.OData.Edm.IEdmTypeReference],[System.String],[System.Object]] convertFunc)
	public virtual System.Collections.Generic.IEnumerable`1[[System.Collections.Generic.KeyValuePair`2[[System.String],[System.Object]]]] ResolveKeys (Microsoft.OData.Edm.IEdmEntityType type, System.Collections.Generic.IList`1[[System.String]] positionalValues, System.Func`3[[Microsoft.OData.Edm.IEdmTypeReference],[System.String],[System.Object]] convertFunc)
	public virtual System.Collections.Generic.IDictionary`2[[Microsoft.OData.Edm.IEdmOperationParameter],[Microsoft.OData.UriParser.SingleValueNode]] ResolveOperationParameters (Microsoft.OData.Edm.IEdmOperation operation, System.Collections.Generic.IDictionary`2[[System.String],[Microsoft.OData.UriParser.SingleValueNode]] input)
	public virtual System.Collections.Generic.IEnumerable`1[[Microsoft.OData.Edm.IEdmOperation]] ResolveUnboundOperations (Microsoft.OData.Edm.IEdmModel model, string identifier)
}

[
AttributeUsageAttribute(),
]
public sealed class Microsoft.AspNet.OData.FromODataUriAttribute : System.Web.Http.ModelBinding.ModelBinderAttribute, _Attribute {
	public FromODataUriAttribute ()

	public virtual System.Web.Http.Controllers.HttpParameterBinding GetBinding (System.Web.Http.Controllers.HttpParameterDescriptor parameter)
}

[
AttributeUsageAttribute(),
]
public sealed class Microsoft.AspNet.OData.ODataQueryParameterBindingAttribute : System.Web.Http.ParameterBindingAttribute, _Attribute {
	public ODataQueryParameterBindingAttribute ()

	public virtual System.Web.Http.Controllers.HttpParameterBinding GetBinding (System.Web.Http.Controllers.HttpParameterDescriptor parameter)
}

[
AttributeUsageAttribute(),
]
public sealed class Microsoft.AspNet.OData.ODataRoutingAttribute : System.Attribute, _Attribute, IControllerConfiguration {
	public ODataRoutingAttribute ()

	public virtual void Initialize (System.Web.Http.Controllers.HttpControllerSettings controllerSettings, System.Web.Http.Controllers.HttpControllerDescriptor controllerDescriptor)
}

public abstract class Microsoft.AspNet.OData.Batch.ODataBatchHandler : System.Web.Http.Batch.HttpBatchHandler, IDisposable {
	protected ODataBatchHandler (System.Web.Http.HttpServer httpServer)

	Microsoft.OData.ODataMessageQuotas MessageQuotas  { public get; }
	string ODataRouteName  { public get; public set; }

	public virtual System.Threading.Tasks.Task`1[[System.Net.Http.HttpResponseMessage]] CreateResponseMessageAsync (System.Collections.Generic.IEnumerable`1[[Microsoft.AspNet.OData.Batch.ODataBatchResponseItem]] responses, System.Net.Http.HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
	public virtual System.Uri GetBaseUri (System.Net.Http.HttpRequestMessage request)
	public virtual void ValidateRequest (System.Net.Http.HttpRequestMessage request)
}

public abstract class Microsoft.AspNet.OData.Batch.ODataBatchRequestItem : IDisposable {
	protected ODataBatchRequestItem ()

	System.Collections.Generic.IDictionary`2[[System.String],[System.String]] ContentIdToLocationMapping  { public get; public set; }

	public virtual void Dispose ()
	protected abstract void Dispose (bool disposing)
	public abstract System.Collections.Generic.IEnumerable`1[[System.IDisposable]] GetResourcesForDisposal ()
	[
	AsyncStateMachineAttribute(),
	]
	public static System.Threading.Tasks.Task`1[[System.Net.Http.HttpResponseMessage]] SendMessageAsync (System.Net.Http.HttpMessageInvoker invoker, System.Net.Http.HttpRequestMessage request, System.Threading.CancellationToken cancellationToken, System.Collections.Generic.IDictionary`2[[System.String],[System.String]] contentIdToLocationMapping)

	public abstract System.Threading.Tasks.Task`1[[Microsoft.AspNet.OData.Batch.ODataBatchResponseItem]] SendRequestAsync (System.Net.Http.HttpMessageInvoker invoker, System.Threading.CancellationToken cancellationToken)
}

public abstract class Microsoft.AspNet.OData.Batch.ODataBatchResponseItem : IDisposable {
	protected ODataBatchResponseItem ()

	public virtual void Dispose ()
	protected abstract void Dispose (bool disposing)
	internal virtual bool IsResponseSuccessful ()
	public static System.Threading.Tasks.Task WriteMessageAsync (Microsoft.OData.ODataBatchWriter writer, System.Net.Http.HttpResponseMessage response)
	[
	AsyncStateMachineAttribute(),
	]
	public static System.Threading.Tasks.Task WriteMessageAsync (Microsoft.OData.ODataBatchWriter writer, System.Net.Http.HttpResponseMessage response, System.Threading.CancellationToken cancellationToken)

	[
	AsyncStateMachineAttribute(),
	]
	public static System.Threading.Tasks.Task WriteMessageAsync (Microsoft.OData.ODataBatchWriter writer, System.Net.Http.HttpResponseMessage response, System.Threading.CancellationToken cancellationToken, bool asyncWriter)

	public System.Threading.Tasks.Task WriteResponseAsync (Microsoft.OData.ODataBatchWriter writer, System.Threading.CancellationToken cancellationToken)
	public abstract System.Threading.Tasks.Task WriteResponseAsync (Microsoft.OData.ODataBatchWriter writer, System.Threading.CancellationToken cancellationToken, bool asyncWriter)
}

[
EditorBrowsableAttribute(),
ExtensionAttribute(),
]
public sealed class Microsoft.AspNet.OData.Batch.ODataBatchHttpRequestMessageExtensions {
	[
	ExtensionAttribute(),
	]
	public static System.Nullable`1[[System.Guid]] GetODataBatchId (System.Net.Http.HttpRequestMessage request)

	[
	ExtensionAttribute(),
	]
	public static System.Nullable`1[[System.Guid]] GetODataChangeSetId (System.Net.Http.HttpRequestMessage request)

	[
	ExtensionAttribute(),
	]
	public static string GetODataContentId (System.Net.Http.HttpRequestMessage request)

	[
	ExtensionAttribute(),
	]
	public static System.Collections.Generic.IDictionary`2[[System.String],[System.String]] GetODataContentIdMapping (System.Net.Http.HttpRequestMessage request)

	[
	ExtensionAttribute(),
	]
	public static void SetODataBatchId (System.Net.Http.HttpRequestMessage request, System.Guid batchId)

	[
	ExtensionAttribute(),
	]
	public static void SetODataChangeSetId (System.Net.Http.HttpRequestMessage request, System.Guid changeSetId)

	[
	ExtensionAttribute(),
	]
	public static void SetODataContentId (System.Net.Http.HttpRequestMessage request, string contentId)

	[
	ExtensionAttribute(),
	]
	public static void SetODataContentIdMapping (System.Net.Http.HttpRequestMessage request, System.Collections.Generic.IDictionary`2[[System.String],[System.String]] contentIdMapping)
}

[
EditorBrowsableAttribute(),
ExtensionAttribute(),
]
public sealed class Microsoft.AspNet.OData.Batch.ODataBatchReaderExtensions {
	[
	ExtensionAttribute(),
	]
	public static System.Threading.Tasks.Task`1[[System.Net.Http.HttpRequestMessage]] ReadChangeSetOperationRequestAsync (Microsoft.OData.ODataBatchReader reader, System.Guid batchId, System.Guid changeSetId, bool bufferContentStream)

	[
	ExtensionAttribute(),
	]
	public static System.Threading.Tasks.Task`1[[System.Net.Http.HttpRequestMessage]] ReadChangeSetOperationRequestAsync (Microsoft.OData.ODataBatchReader reader, System.Guid batchId, System.Guid changeSetId, bool bufferContentStream, System.Threading.CancellationToken cancellationToken)

	[
	ExtensionAttribute(),
	]
	public static System.Threading.Tasks.Task`1[[System.Collections.Generic.IList`1[[System.Net.Http.HttpRequestMessage]]]] ReadChangeSetRequestAsync (Microsoft.OData.ODataBatchReader reader, System.Guid batchId)

	[
	AsyncStateMachineAttribute(),
	ExtensionAttribute(),
	]
	public static System.Threading.Tasks.Task`1[[System.Collections.Generic.IList`1[[System.Net.Http.HttpRequestMessage]]]] ReadChangeSetRequestAsync (Microsoft.OData.ODataBatchReader reader, System.Guid batchId, System.Threading.CancellationToken cancellationToken)

	[
	ExtensionAttribute(),
	]
	public static System.Threading.Tasks.Task`1[[System.Net.Http.HttpRequestMessage]] ReadOperationRequestAsync (Microsoft.OData.ODataBatchReader reader, System.Guid batchId, bool bufferContentStream)

	[
	ExtensionAttribute(),
	]
	public static System.Threading.Tasks.Task`1[[System.Net.Http.HttpRequestMessage]] ReadOperationRequestAsync (Microsoft.OData.ODataBatchReader reader, System.Guid batchId, bool bufferContentStream, System.Threading.CancellationToken cancellationToken)
}

[
EditorBrowsableAttribute(),
ExtensionAttribute(),
]
public sealed class Microsoft.AspNet.OData.Batch.ODataHttpContentExtensions {
	[
	ExtensionAttribute(),
	]
	public static System.Threading.Tasks.Task`1[[Microsoft.OData.ODataMessageReader]] GetODataMessageReaderAsync (System.Net.Http.HttpContent content, System.IServiceProvider requestContainer)

	[
	AsyncStateMachineAttribute(),
	ExtensionAttribute(),
	]
	public static System.Threading.Tasks.Task`1[[Microsoft.OData.ODataMessageReader]] GetODataMessageReaderAsync (System.Net.Http.HttpContent content, System.IServiceProvider requestContainer, System.Threading.CancellationToken cancellationToken)
}

public class Microsoft.AspNet.OData.Batch.ChangeSetRequestItem : ODataBatchRequestItem, IDisposable {
	public ChangeSetRequestItem (System.Collections.Generic.IEnumerable`1[[System.Net.Http.HttpRequestMessage]] requests)

	System.Collections.Generic.IEnumerable`1[[System.Net.Http.HttpRequestMessage]] Requests  { public get; }

	protected virtual void Dispose (bool disposing)
	public virtual System.Collections.Generic.IEnumerable`1[[System.IDisposable]] GetResourcesForDisposal ()
	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task`1[[Microsoft.AspNet.OData.Batch.ODataBatchResponseItem]] SendRequestAsync (System.Net.Http.HttpMessageInvoker invoker, System.Threading.CancellationToken cancellationToken)
}

public class Microsoft.AspNet.OData.Batch.ChangeSetResponseItem : ODataBatchResponseItem, IDisposable {
	public ChangeSetResponseItem (System.Collections.Generic.IEnumerable`1[[System.Net.Http.HttpResponseMessage]] responses)

	System.Collections.Generic.IEnumerable`1[[System.Net.Http.HttpResponseMessage]] Responses  { public get; }

	protected virtual void Dispose (bool disposing)
	internal virtual bool IsResponseSuccessful ()
	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task WriteResponseAsync (Microsoft.OData.ODataBatchWriter writer, System.Threading.CancellationToken cancellationToken, bool asyncWriter)
}

public class Microsoft.AspNet.OData.Batch.DefaultODataBatchHandler : ODataBatchHandler, IDisposable {
	public DefaultODataBatchHandler (System.Web.Http.HttpServer httpServer)

	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task`1[[System.Collections.Generic.IList`1[[Microsoft.AspNet.OData.Batch.ODataBatchResponseItem]]]] ExecuteRequestMessagesAsync (System.Collections.Generic.IEnumerable`1[[Microsoft.AspNet.OData.Batch.ODataBatchRequestItem]] requests, System.Threading.CancellationToken cancellationToken)

	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task`1[[System.Collections.Generic.IList`1[[Microsoft.AspNet.OData.Batch.ODataBatchRequestItem]]]] ParseBatchRequestsAsync (System.Net.Http.HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)

	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task`1[[System.Net.Http.HttpResponseMessage]] ProcessBatchAsync (System.Net.Http.HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
}

public class Microsoft.AspNet.OData.Batch.ODataBatchContent : System.Net.Http.HttpContent, IDisposable {
	public ODataBatchContent (System.Collections.Generic.IEnumerable`1[[Microsoft.AspNet.OData.Batch.ODataBatchResponseItem]] responses, System.IServiceProvider requestContainer)
	public ODataBatchContent (System.Collections.Generic.IEnumerable`1[[Microsoft.AspNet.OData.Batch.ODataBatchResponseItem]] responses, System.IServiceProvider requestContainer, System.Net.Http.Headers.MediaTypeHeaderValue contentType)

	System.Collections.Generic.IEnumerable`1[[Microsoft.AspNet.OData.Batch.ODataBatchResponseItem]] Responses  { public get; }

	protected virtual void Dispose (bool disposing)
	protected virtual System.Threading.Tasks.Task SerializeToStreamAsync (System.IO.Stream stream, System.Net.TransportContext context)
	protected virtual bool TryComputeLength (out System.Int64& length)
}

public class Microsoft.AspNet.OData.Batch.OperationRequestItem : ODataBatchRequestItem, IDisposable {
	public OperationRequestItem (System.Net.Http.HttpRequestMessage request)

	System.Net.Http.HttpRequestMessage Request  { public get; }

	protected virtual void Dispose (bool disposing)
	public virtual System.Collections.Generic.IEnumerable`1[[System.IDisposable]] GetResourcesForDisposal ()
	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task`1[[Microsoft.AspNet.OData.Batch.ODataBatchResponseItem]] SendRequestAsync (System.Net.Http.HttpMessageInvoker invoker, System.Threading.CancellationToken cancellationToken)
}

public class Microsoft.AspNet.OData.Batch.OperationResponseItem : ODataBatchResponseItem, IDisposable {
	public OperationResponseItem (System.Net.Http.HttpResponseMessage response)

	System.Net.Http.HttpResponseMessage Response  { public get; }

	protected virtual void Dispose (bool disposing)
	internal virtual bool IsResponseSuccessful ()
	public virtual System.Threading.Tasks.Task WriteResponseAsync (Microsoft.OData.ODataBatchWriter writer, System.Threading.CancellationToken cancellationToken, bool asyncWriter)
}

public class Microsoft.AspNet.OData.Batch.UnbufferedODataBatchHandler : ODataBatchHandler, IDisposable {
	public UnbufferedODataBatchHandler (System.Web.Http.HttpServer httpServer)

	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task`1[[Microsoft.AspNet.OData.Batch.ODataBatchResponseItem]] ExecuteChangeSetAsync (Microsoft.OData.ODataBatchReader batchReader, System.Guid batchId, System.Net.Http.HttpRequestMessage originalRequest, System.Threading.CancellationToken cancellationToken)

	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task`1[[Microsoft.AspNet.OData.Batch.ODataBatchResponseItem]] ExecuteOperationAsync (Microsoft.OData.ODataBatchReader batchReader, System.Guid batchId, System.Net.Http.HttpRequestMessage originalRequest, System.Threading.CancellationToken cancellationToken)

	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task`1[[System.Net.Http.HttpResponseMessage]] ProcessBatchAsync (System.Net.Http.HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
}

[
FlagsAttribute(),
]
public enum Microsoft.AspNet.OData.Builder.NameResolverOptions : int {
	ProcessDataMemberAttributePropertyNames = 2
	ProcessExplicitPropertyNames = 4
	ProcessReflectedPropertyNames = 1
}

public enum Microsoft.AspNet.OData.Builder.NavigationPropertyBindingOption : int {
	Auto = 1
	None = 0
}

public enum Microsoft.AspNet.OData.Builder.OperationKind : int {
	Action = 0
	Function = 1
	ServiceOperation = 2
}

public enum Microsoft.AspNet.OData.Builder.PropertyKind : int {
	Collection = 2
	Complex = 1
	Dynamic = 5
	Enum = 4
	InstanceAnnotations = 6
	Navigation = 3
	Primitive = 0
}

public interface Microsoft.AspNet.OData.Builder.IEdmTypeConfiguration {
	System.Type ClrType  { public abstract get; }
	string FullName  { public abstract get; }
	Microsoft.OData.Edm.EdmTypeKind Kind  { public abstract get; }
	ODataModelBuilder ModelBuilder  { public abstract get; }
	string Name  { public abstract get; }
	string Namespace  { public abstract get; }
}

public interface Microsoft.AspNet.OData.Builder.IODataInstanceAnnotationContainer {
	void AddPropertyAnnotation (string propertyName, string annotationName, object value)
	void AddResourceAnnotation (string annotationName, object value)
	object GetPropertyAnnotation (string propertyName, string annotationName)
	System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] GetPropertyAnnotations (string propertyName)
	object GetResourceAnnotation (string annotationName)
	System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] GetResourceAnnotations ()
}

public abstract class Microsoft.AspNet.OData.Builder.NavigationSourceConfiguration {
	protected NavigationSourceConfiguration ()
	protected NavigationSourceConfiguration (ODataModelBuilder modelBuilder, EntityTypeConfiguration entityType, string name)
	protected NavigationSourceConfiguration (ODataModelBuilder modelBuilder, System.Type entityClrType, string name)

	System.Collections.Generic.IEnumerable`1[[Microsoft.AspNet.OData.Builder.NavigationPropertyBindingConfiguration]] Bindings  { public get; }
	System.Type ClrType  { public get; }
	DerivedTypeConstraintConfiguration DerivedTypeConstraints  { public get; }
	EntityTypeConfiguration EntityType  { public virtual get; }
	string Name  { public get; }

	public virtual NavigationPropertyBindingConfiguration AddBinding (NavigationPropertyConfiguration navigationConfiguration, NavigationSourceConfiguration targetNavigationSource)
	public virtual NavigationPropertyBindingConfiguration AddBinding (NavigationPropertyConfiguration navigationConfiguration, NavigationSourceConfiguration targetNavigationSource, System.Collections.Generic.IList`1[[System.Reflection.MemberInfo]] bindingPath)
	public virtual System.Collections.Generic.IEnumerable`1[[Microsoft.AspNet.OData.Builder.NavigationPropertyBindingConfiguration]] FindBinding (NavigationPropertyConfiguration navigationConfiguration)
	public virtual NavigationPropertyBindingConfiguration FindBinding (NavigationPropertyConfiguration navigationConfiguration, System.Collections.Generic.IList`1[[System.Reflection.MemberInfo]] bindingPath)
	public virtual System.Collections.Generic.IEnumerable`1[[Microsoft.AspNet.OData.Builder.NavigationPropertyBindingConfiguration]] FindBindings (string propertyName)
	public virtual Microsoft.AspNet.OData.Builder.SelfLinkBuilder`1[[System.Uri]] GetEditLink ()
	public virtual Microsoft.AspNet.OData.Builder.SelfLinkBuilder`1[[System.Uri]] GetIdLink ()
	public virtual NavigationLinkBuilder GetNavigationPropertyLink (NavigationPropertyConfiguration navigationProperty)
	public virtual Microsoft.AspNet.OData.Builder.SelfLinkBuilder`1[[System.Uri]] GetReadLink ()
	public virtual string GetUrl ()
	public virtual NavigationSourceConfiguration HasEditLink (Microsoft.AspNet.OData.Builder.SelfLinkBuilder`1[[System.Uri]] editLinkBuilder)
	public virtual NavigationSourceConfiguration HasIdLink (Microsoft.AspNet.OData.Builder.SelfLinkBuilder`1[[System.Uri]] idLinkBuilder)
	public virtual NavigationSourceConfiguration HasNavigationPropertiesLink (System.Collections.Generic.IEnumerable`1[[Microsoft.AspNet.OData.Builder.NavigationPropertyConfiguration]] navigationProperties, NavigationLinkBuilder navigationLinkBuilder)
	public virtual NavigationSourceConfiguration HasNavigationPropertyLink (NavigationPropertyConfiguration navigationProperty, NavigationLinkBuilder navigationLinkBuilder)
	public virtual NavigationSourceConfiguration HasReadLink (Microsoft.AspNet.OData.Builder.SelfLinkBuilder`1[[System.Uri]] readLinkBuilder)
	public virtual NavigationSourceConfiguration HasUrl (string url)
	public virtual void RemoveBinding (NavigationPropertyConfiguration navigationConfiguration)
	public virtual void RemoveBinding (NavigationPropertyConfiguration navigationConfiguration, string bindingPath)
}

public abstract class Microsoft.AspNet.OData.Builder.NavigationSourceConfiguration`1 {
	BindingPathConfiguration`1 Binding  { public get; }
	EntityTypeConfiguration`1 EntityType  { public get; }

	public System.Collections.Generic.IEnumerable`1[[Microsoft.AspNet.OData.Builder.NavigationPropertyBindingConfiguration]] FindBinding (NavigationPropertyConfiguration navigationConfiguration)
	public NavigationPropertyBindingConfiguration FindBinding (NavigationPropertyConfiguration navigationConfiguration, System.Collections.Generic.IList`1[[System.Reflection.MemberInfo]] bindingPath)
	public System.Collections.Generic.IEnumerable`1[[Microsoft.AspNet.OData.Builder.NavigationPropertyBindingConfiguration]] FindBindings (string propertyName)
	public void HasEditLink (Func`2 editLinkFactory, bool followsConventions)
	public void HasIdLink (Func`2 idLinkFactory, bool followsConventions)
	public NavigationPropertyBindingConfiguration HasManyBinding (Expression`1 navigationExpression, NavigationSourceConfiguration`1 targetEntitySet)
	public NavigationPropertyBindingConfiguration HasManyBinding (Expression`1 navigationExpression, NavigationSourceConfiguration`1 targetEntitySet)
	public NavigationPropertyBindingConfiguration HasManyBinding (Expression`1 navigationExpression, string entitySetName)
	public NavigationPropertyBindingConfiguration HasManyBinding (Expression`1 navigationExpression, string entitySetName)
	public void HasNavigationPropertiesLink (System.Collections.Generic.IEnumerable`1[[Microsoft.AspNet.OData.Builder.NavigationPropertyConfiguration]] navigationProperties, Func`3 navigationLinkFactory, bool followsConventions)
	public void HasNavigationPropertyLink (NavigationPropertyConfiguration navigationProperty, Func`3 navigationLinkFactory, bool followsConventions)
	public NavigationPropertyBindingConfiguration HasOptionalBinding (Expression`1 navigationExpression, NavigationSourceConfiguration`1 targetEntitySet)
	public NavigationPropertyBindingConfiguration HasOptionalBinding (Expression`1 navigationExpression, NavigationSourceConfiguration`1 targetEntitySet)
	public NavigationPropertyBindingConfiguration HasOptionalBinding (Expression`1 navigationExpression, string entitySetName)
	public NavigationPropertyBindingConfiguration HasOptionalBinding (Expression`1 navigationExpression, string entitySetName)
	public void HasReadLink (Func`2 readLinkFactory, bool followsConventions)
	public NavigationPropertyBindingConfiguration HasRequiredBinding (Expression`1 navigationExpression, NavigationSourceConfiguration`1 targetEntitySet)
	public NavigationPropertyBindingConfiguration HasRequiredBinding (Expression`1 navigationExpression, NavigationSourceConfiguration`1 targetEntitySet)
	public NavigationPropertyBindingConfiguration HasRequiredBinding (Expression`1 navigationExpression, string entitySetName)
	public NavigationPropertyBindingConfiguration HasRequiredBinding (Expression`1 navigationExpression, string entitySetName)
	public NavigationPropertyBindingConfiguration HasSingletonBinding (Expression`1 navigationExpression, NavigationSourceConfiguration`1 targetSingleton)
	public NavigationPropertyBindingConfiguration HasSingletonBinding (Expression`1 navigationExpression, NavigationSourceConfiguration`1 targetSingleton)
	public NavigationPropertyBindingConfiguration HasSingletonBinding (Expression`1 navigationExpression, string singletonName)
	public NavigationPropertyBindingConfiguration HasSingletonBinding (Expression`1 navigationExpression, string singletonName)
}

public abstract class Microsoft.AspNet.OData.Builder.OperationConfiguration {
	BindingParameterConfiguration BindingParameter  { public virtual get; }
	System.Collections.Generic.IEnumerable`1[[System.String]] EntitySetPath  { public get; }
	bool FollowsConventions  { public get; protected set; }
	string FullyQualifiedName  { public get; }
	bool IsBindable  { public virtual get; }
	bool IsComposable  { public virtual get; }
	bool IsSideEffecting  { public abstract get; }
	OperationKind Kind  { public abstract get; }
	ODataModelBuilder ModelBuilder  { protected get; protected set; }
	string Name  { public get; protected set; }
	string Namespace  { public get; public set; }
	NavigationSourceConfiguration NavigationSource  { public get; public set; }
	OperationLinkBuilder OperationLinkBuilder  { protected get; protected set; }
	System.Collections.Generic.IEnumerable`1[[Microsoft.AspNet.OData.Builder.ParameterConfiguration]] Parameters  { public virtual get; }
	bool ReturnNullable  { public get; public set; }
	IEdmTypeConfiguration ReturnType  { public get; public set; }
	DerivedTypeConstraintConfiguration ReturnTypeConstraints  { public get; protected set; }
	string Title  { public get; public set; }

	public ParameterConfiguration AddParameter (string name, IEdmTypeConfiguration parameterType)
	public ParameterConfiguration CollectionEntityParameter (string name)
	public ParameterConfiguration CollectionEntityParameter (System.Type clrElementEntityType, string name)
	public ParameterConfiguration CollectionParameter (string name)
	public ParameterConfiguration CollectionParameter (System.Type clrElementType, string name)
	public ParameterConfiguration EntityParameter (string name)
	public ParameterConfiguration EntityParameter (System.Type clrEntityType, string name)
	public ParameterConfiguration Parameter (string name)
	public ParameterConfiguration Parameter (System.Type clrParameterType, string name)
}

public abstract class Microsoft.AspNet.OData.Builder.ParameterConfiguration {
	protected ParameterConfiguration (string name, IEdmTypeConfiguration parameterType)

	string DefaultValue  { public get; protected set; }
	DerivedTypeConstraintConfiguration DerivedTypeConstraints  { public get; }
	bool IsOptional  { public get; protected set; }
	string Name  { public get; protected set; }
	bool Nullable  { public get; public set; }
	IEdmTypeConfiguration TypeConfiguration  { public get; protected set; }

	public ParameterConfiguration HasDefaultValue (string defaultValue)
	public ParameterConfiguration HasDerivedTypeConstraint ()
	public ParameterConfiguration HasDerivedTypeConstraints (System.Type[] subtypes)
	public ParameterConfiguration Optional ()
	public ParameterConfiguration Required ()
}

public abstract class Microsoft.AspNet.OData.Builder.PropertyConfiguration {
	protected PropertyConfiguration (System.Reflection.PropertyInfo property, StructuralTypeConfiguration declaringType)

	bool AddedExplicitly  { public get; public set; }
	bool AutoExpand  { public get; public set; }
	StructuralTypeConfiguration DeclaringType  { public get; }
	DerivedTypeConstraintConfiguration DerivedTypeConstraints  { public get; }
	bool DisableAutoExpandWhenSelectIsPresent  { public get; public set; }
	bool IsRestricted  { public get; }
	PropertyKind Kind  { public abstract get; }
	string Name  { public get; public set; }
	bool NonFilterable  { public get; public set; }
	bool NotCountable  { public get; public set; }
	bool NotExpandable  { public get; public set; }
	bool NotFilterable  { public get; public set; }
	bool NotNavigable  { public get; public set; }
	bool NotSortable  { public get; public set; }
	int Order  { public get; public set; }
	System.Reflection.PropertyInfo PropertyInfo  { public get; }
	QueryConfiguration QueryConfiguration  { public get; public set; }
	System.Type RelatedClrType  { public abstract get; }
	bool Unsortable  { public get; public set; }

	public PropertyConfiguration Count ()
	public PropertyConfiguration Count (QueryOptionSetting queryOptionSetting)
	public PropertyConfiguration Expand ()
	public PropertyConfiguration Expand (SelectExpandType expandType)
	public PropertyConfiguration Expand (int maxDepth)
	public PropertyConfiguration Expand (string[] properties)
	public PropertyConfiguration Expand (SelectExpandType expandType, int maxDepth)
	public PropertyConfiguration Expand (SelectExpandType expandType, string[] properties)
	public PropertyConfiguration Expand (int maxDepth, string[] properties)
	public PropertyConfiguration Expand (int maxDepth, SelectExpandType expandType, string[] properties)
	public PropertyConfiguration Filter ()
	public PropertyConfiguration Filter (QueryOptionSetting setting)
	public PropertyConfiguration Filter (string[] properties)
	public PropertyConfiguration Filter (QueryOptionSetting setting, string[] properties)
	public PropertyConfiguration IsCountable ()
	public PropertyConfiguration IsExpandable ()
	public PropertyConfiguration IsFilterable ()
	public PropertyConfiguration IsNavigable ()
	public PropertyConfiguration IsNonFilterable ()
	public PropertyConfiguration IsNotCountable ()
	public PropertyConfiguration IsNotExpandable ()
	public PropertyConfiguration IsNotFilterable ()
	public PropertyConfiguration IsNotNavigable ()
	public PropertyConfiguration IsNotSortable ()
	public PropertyConfiguration IsSortable ()
	public PropertyConfiguration IsUnsortable ()
	public PropertyConfiguration OrderBy ()
	public PropertyConfiguration OrderBy (QueryOptionSetting setting)
	public PropertyConfiguration OrderBy (string[] properties)
	public PropertyConfiguration OrderBy (QueryOptionSetting setting, string[] properties)
	public PropertyConfiguration Page ()
	public PropertyConfiguration Page (System.Nullable`1[[System.Int32]] maxTopValue, System.Nullable`1[[System.Int32]] pageSizeValue)
	public PropertyConfiguration Select ()
	public PropertyConfiguration Select (SelectExpandType selectType)
	public PropertyConfiguration Select (string[] properties)
	public PropertyConfiguration Select (SelectExpandType selectType, string[] properties)
}

public abstract class Microsoft.AspNet.OData.Builder.StructuralPropertyConfiguration : PropertyConfiguration {
	protected StructuralPropertyConfiguration (System.Reflection.PropertyInfo property, StructuralTypeConfiguration declaringType)

	bool ConcurrencyToken  { public get; public set; }
	bool OptionalProperty  { public get; public set; }
}

public abstract class Microsoft.AspNet.OData.Builder.StructuralTypeConfiguration : IEdmTypeConfiguration {
	protected StructuralTypeConfiguration ()
	protected StructuralTypeConfiguration (ODataModelBuilder modelBuilder, System.Type clrType)

	bool AddedExplicitly  { public get; public set; }
	bool BaseTypeConfigured  { public virtual get; }
	StructuralTypeConfiguration BaseTypeInternal  { protected virtual get; }
	System.Type ClrType  { public virtual get; }
	System.Reflection.PropertyInfo DynamicPropertyDictionary  { public get; }
	System.Collections.Generic.IDictionary`2[[System.Reflection.PropertyInfo],[Microsoft.AspNet.OData.Builder.PropertyConfiguration]] ExplicitProperties  { protected get; }
	string FullName  { public virtual get; }
	System.Collections.ObjectModel.ReadOnlyCollection`1[[System.Reflection.PropertyInfo]] IgnoredProperties  { public get; }
	System.Reflection.PropertyInfo InstanceAnnotationsContainer  { public get; }
	System.Nullable`1[[System.Boolean]] IsAbstract  { public virtual get; public virtual set; }
	bool IsOpen  { public get; }
	Microsoft.OData.Edm.EdmTypeKind Kind  { public abstract get; }
	ODataModelBuilder ModelBuilder  { public virtual get; }
	string Name  { public virtual get; public virtual set; }
	string Namespace  { public virtual get; public virtual set; }
	System.Collections.Generic.IEnumerable`1[[Microsoft.AspNet.OData.Builder.NavigationPropertyConfiguration]] NavigationProperties  { public virtual get; }
	System.Collections.Generic.IEnumerable`1[[Microsoft.AspNet.OData.Builder.PropertyConfiguration]] Properties  { public get; }
	QueryConfiguration QueryConfiguration  { public get; public set; }
	System.Collections.Generic.IList`1[[System.Reflection.PropertyInfo]] RemovedProperties  { protected get; }
	bool SupportsInstanceAnnotations  { public get; }

	internal virtual void AbstractImpl ()
	public virtual CollectionPropertyConfiguration AddCollectionProperty (System.Reflection.PropertyInfo propertyInfo)
	public virtual ComplexPropertyConfiguration AddComplexProperty (System.Reflection.PropertyInfo propertyInfo)
	public virtual NavigationPropertyConfiguration AddContainedNavigationProperty (System.Reflection.PropertyInfo navigationProperty, Microsoft.OData.Edm.EdmMultiplicity multiplicity)
	public virtual void AddDynamicPropertyDictionary (System.Reflection.PropertyInfo propertyInfo)
	public virtual EnumPropertyConfiguration AddEnumProperty (System.Reflection.PropertyInfo propertyInfo)
	public virtual void AddInstanceAnnotationContainer (System.Reflection.PropertyInfo propertyInfo)
	public virtual NavigationPropertyConfiguration AddNavigationProperty (System.Reflection.PropertyInfo navigationProperty, Microsoft.OData.Edm.EdmMultiplicity multiplicity)
	public virtual PrimitivePropertyConfiguration AddProperty (System.Reflection.PropertyInfo propertyInfo)
	internal virtual void DerivesFromImpl (StructuralTypeConfiguration baseType)
	internal virtual void DerivesFromNothingImpl ()
	public virtual void RemoveProperty (System.Reflection.PropertyInfo propertyInfo)
}

public abstract class Microsoft.AspNet.OData.Builder.StructuralTypeConfiguration`1 {
	protected StructuralTypeConfiguration`1 (StructuralTypeConfiguration configuration)

	string FullName  { public get; }
	bool IsOpen  { public get; }
	string Name  { public get; public set; }
	string Namespace  { public get; public set; }
	System.Collections.Generic.IEnumerable`1[[Microsoft.AspNet.OData.Builder.PropertyConfiguration]] Properties  { public get; }

	public CollectionPropertyConfiguration CollectionProperty (Expression`1 propertyExpression)
	public ComplexPropertyConfiguration ComplexProperty (Expression`1 propertyExpression)
	public NavigationPropertyConfiguration ContainsMany (Expression`1 navigationPropertyExpression)
	public NavigationPropertyConfiguration ContainsOptional (Expression`1 navigationPropertyExpression)
	public NavigationPropertyConfiguration ContainsRequired (Expression`1 navigationPropertyExpression)
	public StructuralTypeConfiguration`1 Count ()
	public StructuralTypeConfiguration`1 Count (QueryOptionSetting setting)
	public EnumPropertyConfiguration EnumProperty (Expression`1 propertyExpression)
	public EnumPropertyConfiguration EnumProperty (Expression`1 propertyExpression)
	public StructuralTypeConfiguration`1 Expand ()
	public StructuralTypeConfiguration`1 Expand (SelectExpandType expandType)
	public StructuralTypeConfiguration`1 Expand (int maxDepth)
	public StructuralTypeConfiguration`1 Expand (string[] properties)
	public StructuralTypeConfiguration`1 Expand (SelectExpandType expandType, int maxDepth)
	public StructuralTypeConfiguration`1 Expand (SelectExpandType expandType, string[] properties)
	public StructuralTypeConfiguration`1 Expand (int maxDepth, string[] properties)
	public StructuralTypeConfiguration`1 Expand (int maxDepth, SelectExpandType expandType, string[] properties)
	public StructuralTypeConfiguration`1 Filter ()
	public StructuralTypeConfiguration`1 Filter (QueryOptionSetting setting)
	public StructuralTypeConfiguration`1 Filter (string[] properties)
	public StructuralTypeConfiguration`1 Filter (QueryOptionSetting setting, string[] properties)
	public void HasDynamicProperties (Expression`1 propertyExpression)
	public void HasInstanceAnnotations (Expression`1 propertyExpression)
	public NavigationPropertyConfiguration HasMany (Expression`1 navigationPropertyExpression)
	public NavigationPropertyConfiguration HasOptional (Expression`1 navigationPropertyExpression)
	public NavigationPropertyConfiguration HasOptional (Expression`1 navigationPropertyExpression, Expression`1 referentialConstraintExpression)
	public NavigationPropertyConfiguration HasOptional (Expression`1 navigationPropertyExpression, Expression`1 referentialConstraintExpression, Expression`1 partnerExpression)
	public NavigationPropertyConfiguration HasOptional (Expression`1 navigationPropertyExpression, Expression`1 referentialConstraintExpression, Expression`1 partnerExpression)
	public NavigationPropertyConfiguration HasRequired (Expression`1 navigationPropertyExpression)
	public NavigationPropertyConfiguration HasRequired (Expression`1 navigationPropertyExpression, Expression`1 referentialConstraintExpression)
	public NavigationPropertyConfiguration HasRequired (Expression`1 navigationPropertyExpression, Expression`1 referentialConstraintExpression, Expression`1 partnerExpression)
	public NavigationPropertyConfiguration HasRequired (Expression`1 navigationPropertyExpression, Expression`1 referentialConstraintExpression, Expression`1 partnerExpression)
	public virtual void Ignore (Expression`1 propertyExpression)
	public StructuralTypeConfiguration`1 OrderBy ()
	public StructuralTypeConfiguration`1 OrderBy (QueryOptionSetting setting)
	public StructuralTypeConfiguration`1 OrderBy (string[] properties)
	public StructuralTypeConfiguration`1 OrderBy (QueryOptionSetting setting, string[] properties)
	public StructuralTypeConfiguration`1 Page ()
	public StructuralTypeConfiguration`1 Page (System.Nullable`1[[System.Int32]] maxTopValue, System.Nullable`1[[System.Int32]] pageSizeValue)
	public LengthPropertyConfiguration Property (Expression`1 propertyExpression)
	public DecimalPropertyConfiguration Property (Expression`1 propertyExpression)
	public PrecisionPropertyConfiguration Property (Expression`1 propertyExpression)
	public PrecisionPropertyConfiguration Property (Expression`1 propertyExpression)
	public PrecisionPropertyConfiguration Property (Expression`1 propertyExpression)
	public PrecisionPropertyConfiguration Property (Expression`1 propertyExpression)
	public PrecisionPropertyConfiguration Property (Expression`1 propertyExpression)
	public DecimalPropertyConfiguration Property (Expression`1 propertyExpression)
	public PrimitivePropertyConfiguration Property (Expression`1 propertyExpression)
	public PrecisionPropertyConfiguration Property (Expression`1 propertyExpression)
	public LengthPropertyConfiguration Property (Expression`1 propertyExpression)
	public PrimitivePropertyConfiguration Property (Expression`1 propertyExpression)
	public PrimitivePropertyConfiguration Property (Expression`1 propertyExpression)
	public StructuralTypeConfiguration`1 Select ()
	public StructuralTypeConfiguration`1 Select (SelectExpandType selectType)
	public StructuralTypeConfiguration`1 Select (string[] properties)
	public StructuralTypeConfiguration`1 Select (SelectExpandType selectType, string[] properties)
}

[
EditorBrowsableAttribute(),
ExtensionAttribute(),
]
public sealed class Microsoft.AspNet.OData.Builder.LinkGenerationHelpers {
	[
	ExtensionAttribute(),
	]
	public static System.Uri GenerateActionLink (ResourceContext resourceContext, Microsoft.OData.Edm.IEdmOperation action)

	[
	ExtensionAttribute(),
	]
	public static System.Uri GenerateActionLink (ResourceSetContext resourceSetContext, Microsoft.OData.Edm.IEdmOperation action)

	[
	ExtensionAttribute(),
	]
	public static System.Uri GenerateFunctionLink (ResourceContext resourceContext, Microsoft.OData.Edm.IEdmOperation function)

	[
	ExtensionAttribute(),
	]
	public static System.Uri GenerateFunctionLink (ResourceSetContext resourceSetContext, Microsoft.OData.Edm.IEdmOperation function)

	[
	ExtensionAttribute(),
	]
	public static System.Uri GenerateNavigationPropertyLink (ResourceContext resourceContext, Microsoft.OData.Edm.IEdmNavigationProperty navigationProperty, bool includeCast)

	[
	ExtensionAttribute(),
	]
	public static System.Uri GenerateSelfLink (ResourceContext resourceContext, bool includeCast)
}

[
EditorBrowsableAttribute(),
ExtensionAttribute(),
]
public sealed class Microsoft.AspNet.OData.Builder.ODataConventionModelBuilderExtensions {
	[
	ExtensionAttribute(),
	]
	public static ODataConventionModelBuilder EnableLowerCamelCase (ODataConventionModelBuilder builder)

	[
	ExtensionAttribute(),
	]
	public static ODataConventionModelBuilder EnableLowerCamelCase (ODataConventionModelBuilder builder, NameResolverOptions options)
}

[
ExtensionAttribute(),
]
public sealed class Microsoft.AspNet.OData.Builder.PrimitivePropertyConfigurationExtensions {
	[
	ExtensionAttribute(),
	]
	public static PrimitivePropertyConfiguration AsDate (PrimitivePropertyConfiguration property)

	[
	ExtensionAttribute(),
	]
	public static PrimitivePropertyConfiguration AsTimeOfDay (PrimitivePropertyConfiguration property)
}

public class Microsoft.AspNet.OData.Builder.ActionConfiguration : OperationConfiguration {
	bool IsSideEffecting  { public virtual get; }
	OperationKind Kind  { public virtual get; }

	public System.Func`2[[Microsoft.AspNet.OData.ResourceContext],[System.Uri]] GetActionLink ()
	public System.Func`2[[Microsoft.AspNet.OData.ResourceSetContext],[System.Uri]] GetFeedActionLink ()
	public ActionConfiguration HasActionLink (System.Func`2[[Microsoft.AspNet.OData.ResourceContext],[System.Uri]] actionLinkFactory, bool followsConventions)
	public ActionConfiguration HasDerivedTypeConstraintForReturnType ()
	public ActionConfiguration HasDerivedTypeConstraintsForReturnType (System.Type[] subtypes)
	public ActionConfiguration HasFeedActionLink (System.Func`2[[Microsoft.AspNet.OData.ResourceSetContext],[System.Uri]] actionLinkFactory, bool followsConventions)
	public ActionConfiguration Returns ()
	public ActionConfiguration Returns (System.Type clrReturnType)
	public ActionConfiguration ReturnsCollection ()
	public ActionConfiguration ReturnsCollectionFromEntitySet (EntitySetConfiguration`1 entitySetConfiguration)
	public ActionConfiguration ReturnsCollectionFromEntitySet (string entitySetName)
	public ActionConfiguration ReturnsCollectionViaEntitySetPath (string entitySetPath)
	public ActionConfiguration ReturnsCollectionViaEntitySetPath (string[] entitySetPath)
	public ActionConfiguration ReturnsEntityViaEntitySetPath (string entitySetPath)
	public ActionConfiguration ReturnsEntityViaEntitySetPath (string[] entitySetPath)
	public ActionConfiguration ReturnsFromEntitySet (EntitySetConfiguration`1 entitySetConfiguration)
	public ActionConfiguration ReturnsFromEntitySet (string entitySetName)
	public ActionConfiguration SetBindingParameter (string name, IEdmTypeConfiguration bindingParameterType)
}

public class Microsoft.AspNet.OData.Builder.BindingParameterConfiguration : ParameterConfiguration {
	public static string DefaultBindingParameterName = "bindingParameter"

	public BindingParameterConfiguration (string name, IEdmTypeConfiguration parameterType)
}

public class Microsoft.AspNet.OData.Builder.BindingPathConfiguration`1 {
	public BindingPathConfiguration`1 (ODataModelBuilder modelBuilder, StructuralTypeConfiguration`1 structuralType, NavigationSourceConfiguration navigationSource)
	public BindingPathConfiguration`1 (ODataModelBuilder modelBuilder, StructuralTypeConfiguration`1 structuralType, NavigationSourceConfiguration navigationSource, System.Collections.Generic.IList`1[[System.Reflection.MemberInfo]] bindingPath)

	string BindingPath  { public get; }
	System.Collections.Generic.IList`1[[System.Reflection.MemberInfo]] Path  { public get; }

	public NavigationPropertyBindingConfiguration HasManyBinding (Expression`1 navigationExpression, string targetEntitySet)
	public NavigationPropertyBindingConfiguration HasManyBinding (Expression`1 navigationExpression, string targetEntitySet)
	public BindingPathConfiguration`1 HasManyPath (Expression`1 pathExpression)
	public BindingPathConfiguration`1 HasManyPath (Expression`1 pathExpression)
	public BindingPathConfiguration`1 HasManyPath (Expression`1 pathExpression, bool contained)
	public BindingPathConfiguration`1 HasManyPath (Expression`1 pathExpression, bool contained)
	public NavigationPropertyBindingConfiguration HasOptionalBinding (Expression`1 navigationExpression, string targetEntitySet)
	public NavigationPropertyBindingConfiguration HasOptionalBinding (Expression`1 navigationExpression, string targetEntitySet)
	public NavigationPropertyBindingConfiguration HasRequiredBinding (Expression`1 navigationExpression, string targetEntitySet)
	public NavigationPropertyBindingConfiguration HasRequiredBinding (Expression`1 navigationExpression, string targetEntitySet)
	public BindingPathConfiguration`1 HasSinglePath (Expression`1 pathExpression)
	public BindingPathConfiguration`1 HasSinglePath (Expression`1 pathExpression)
	public BindingPathConfiguration`1 HasSinglePath (Expression`1 pathExpression, bool required, bool contained)
	public BindingPathConfiguration`1 HasSinglePath (Expression`1 pathExpression, bool required, bool contained)
}

public class Microsoft.AspNet.OData.Builder.CollectionPropertyConfiguration : StructuralPropertyConfiguration {
	public CollectionPropertyConfiguration (System.Reflection.PropertyInfo property, StructuralTypeConfiguration declaringType)

	System.Type ElementType  { public get; }
	PropertyKind Kind  { public virtual get; }
	System.Type RelatedClrType  { public virtual get; }

	public CollectionPropertyConfiguration HasDerivedTypeConstraint ()
	public CollectionPropertyConfiguration HasDerivedTypeConstraints (System.Type[] subtypes)
	public CollectionPropertyConfiguration IsOptional ()
	public CollectionPropertyConfiguration IsRequired ()
}

public class Microsoft.AspNet.OData.Builder.CollectionTypeConfiguration : IEdmTypeConfiguration {
	public CollectionTypeConfiguration (IEdmTypeConfiguration elementType, System.Type clrType)

	System.Type ClrType  { public virtual get; }
	IEdmTypeConfiguration ElementType  { public get; }
	string FullName  { public virtual get; }
	Microsoft.OData.Edm.EdmTypeKind Kind  { public virtual get; }
	ODataModelBuilder ModelBuilder  { public virtual get; }
	string Name  { public virtual get; }
	string Namespace  { public virtual get; }
}

public class Microsoft.AspNet.OData.Builder.ComplexPropertyConfiguration : StructuralPropertyConfiguration {
	public ComplexPropertyConfiguration (System.Reflection.PropertyInfo property, StructuralTypeConfiguration declaringType)

	PropertyKind Kind  { public virtual get; }
	System.Type RelatedClrType  { public virtual get; }

	public ComplexPropertyConfiguration HasDerivedTypeConstraint ()
	public ComplexPropertyConfiguration HasDerivedTypeConstraints (System.Type[] subtypes)
	public ComplexPropertyConfiguration IsOptional ()
	public ComplexPropertyConfiguration IsRequired ()
}

public class Microsoft.AspNet.OData.Builder.ComplexTypeConfiguration : StructuralTypeConfiguration, IEdmTypeConfiguration {
	public ComplexTypeConfiguration ()
	public ComplexTypeConfiguration (ODataModelBuilder modelBuilder, System.Type clrType)

	ComplexTypeConfiguration BaseType  { public virtual get; public virtual set; }
	Microsoft.OData.Edm.EdmTypeKind Kind  { public virtual get; }

	public virtual ComplexTypeConfiguration Abstract ()
	public virtual ComplexTypeConfiguration DerivesFrom (ComplexTypeConfiguration baseType)
	public virtual ComplexTypeConfiguration DerivesFromNothing ()
}

public class Microsoft.AspNet.OData.Builder.ComplexTypeConfiguration`1 : StructuralTypeConfiguration`1 {
	ComplexTypeConfiguration BaseType  { public get; }

	public ComplexTypeConfiguration`1 Abstract ()
	public ComplexTypeConfiguration`1 DerivesFrom ()
	public ComplexTypeConfiguration`1 DerivesFromNothing ()
}

public class Microsoft.AspNet.OData.Builder.DecimalPropertyConfiguration : PrecisionPropertyConfiguration {
	public DecimalPropertyConfiguration (System.Reflection.PropertyInfo property, StructuralTypeConfiguration declaringType)

	System.Nullable`1[[System.Int32]] Scale  { public get; public set; }
}

public class Microsoft.AspNet.OData.Builder.DerivedTypeConstraintConfiguration {
	public DerivedTypeConstraintConfiguration ()
	public DerivedTypeConstraintConfiguration (Microsoft.OData.Edm.Csdl.EdmVocabularyAnnotationSerializationLocation location)

	Microsoft.OData.Edm.Csdl.EdmVocabularyAnnotationSerializationLocation Location  { public get; public set; }

	public DerivedTypeConstraintConfiguration AddConstraint ()
	public void AddConstraints (System.Collections.Generic.IEnumerable`1[[System.Type]] derivedTypes)
}

public class Microsoft.AspNet.OData.Builder.DynamicPropertyDictionaryAnnotation {
	public DynamicPropertyDictionaryAnnotation (System.Reflection.PropertyInfo propertyInfo)

	System.Reflection.PropertyInfo PropertyInfo  { public get; }
}

public class Microsoft.AspNet.OData.Builder.EntityCollectionConfiguration`1 : CollectionTypeConfiguration, IEdmTypeConfiguration {
	public ActionConfiguration Action (string name)
	public FunctionConfiguration Function (string name)
}

public class Microsoft.AspNet.OData.Builder.EntitySetConfiguration : NavigationSourceConfiguration {
	public EntitySetConfiguration ()
	public EntitySetConfiguration (ODataModelBuilder modelBuilder, EntityTypeConfiguration entityType, string name)
	public EntitySetConfiguration (ODataModelBuilder modelBuilder, System.Type entityClrType, string name)

	public virtual System.Func`2[[Microsoft.AspNet.OData.ResourceSetContext],[System.Uri]] GetFeedSelfLink ()
	public EntitySetConfiguration HasDerivedTypeConstraint ()
	public EntitySetConfiguration HasDerivedTypeConstraints (System.Type[] subtypes)
	public virtual NavigationSourceConfiguration HasFeedSelfLink (System.Func`2[[Microsoft.AspNet.OData.ResourceSetContext],[System.Uri]] feedSelfLinkFactory)
}

public class Microsoft.AspNet.OData.Builder.EntitySetConfiguration`1 : NavigationSourceConfiguration`1 {
	public EntitySetConfiguration`1 HasDerivedTypeConstraint ()
	public EntitySetConfiguration`1 HasDerivedTypeConstraints (System.Type[] subtypes)
	public virtual void HasFeedSelfLink (System.Func`2[[Microsoft.AspNet.OData.ResourceSetContext],[System.String]] feedSelfLinkFactory)
	public virtual void HasFeedSelfLink (System.Func`2[[Microsoft.AspNet.OData.ResourceSetContext],[System.Uri]] feedSelfLinkFactory)
}

public class Microsoft.AspNet.OData.Builder.EntityTypeConfiguration : StructuralTypeConfiguration, IEdmTypeConfiguration {
	public EntityTypeConfiguration ()
	public EntityTypeConfiguration (ODataModelBuilder modelBuilder, System.Type clrType)

	EntityTypeConfiguration BaseType  { public virtual get; public virtual set; }
	System.Collections.Generic.IEnumerable`1[[Microsoft.AspNet.OData.Builder.EnumPropertyConfiguration]] EnumKeys  { public virtual get; }
	bool HasStream  { public virtual get; public virtual set; }
	System.Collections.Generic.IEnumerable`1[[Microsoft.AspNet.OData.Builder.PrimitivePropertyConfiguration]] Keys  { public virtual get; }
	Microsoft.OData.Edm.EdmTypeKind Kind  { public virtual get; }

	public virtual EntityTypeConfiguration Abstract ()
	public virtual EntityTypeConfiguration DerivesFrom (EntityTypeConfiguration baseType)
	public virtual EntityTypeConfiguration DerivesFromNothing ()
	public virtual EntityTypeConfiguration HasKey (System.Reflection.PropertyInfo keyProperty)
	public virtual EntityTypeConfiguration MediaType ()
	public virtual void RemoveKey (EnumPropertyConfiguration enumKeyProperty)
	public virtual void RemoveKey (PrimitivePropertyConfiguration keyProperty)
	public virtual void RemoveProperty (System.Reflection.PropertyInfo propertyInfo)
}

public class Microsoft.AspNet.OData.Builder.EntityTypeConfiguration`1 : StructuralTypeConfiguration`1 {
	EntityTypeConfiguration BaseType  { public get; }
	EntityCollectionConfiguration`1 Collection  { public get; }
	System.Collections.Generic.IEnumerable`1[[Microsoft.AspNet.OData.Builder.NavigationPropertyConfiguration]] NavigationProperties  { public get; }

	public EntityTypeConfiguration`1 Abstract ()
	public ActionConfiguration Action (string name)
	public EntityTypeConfiguration`1 DerivesFrom ()
	public EntityTypeConfiguration`1 DerivesFromNothing ()
	public FunctionConfiguration Function (string name)
	public EntityTypeConfiguration`1 HasKey (Expression`1 keyDefinitionExpression)
	public EntityTypeConfiguration`1 MediaType ()
}

public class Microsoft.AspNet.OData.Builder.EnumMemberConfiguration {
	public EnumMemberConfiguration (System.Enum member, EnumTypeConfiguration declaringType)

	bool AddedExplicitly  { public get; public set; }
	EnumTypeConfiguration DeclaringType  { public get; }
	System.Enum MemberInfo  { public get; }
	string Name  { public get; public set; }
}

public class Microsoft.AspNet.OData.Builder.EnumPropertyConfiguration : StructuralPropertyConfiguration {
	public EnumPropertyConfiguration (System.Reflection.PropertyInfo property, StructuralTypeConfiguration declaringType)

	string DefaultValueString  { public get; public set; }
	PropertyKind Kind  { public virtual get; }
	System.Type RelatedClrType  { public virtual get; }

	public EnumPropertyConfiguration IsConcurrencyToken ()
	public EnumPropertyConfiguration IsOptional ()
	public EnumPropertyConfiguration IsRequired ()
}

public class Microsoft.AspNet.OData.Builder.EnumTypeConfiguration : IEdmTypeConfiguration {
	public EnumTypeConfiguration (ODataModelBuilder builder, System.Type clrType)

	bool AddedExplicitly  { public get; public set; }
	System.Type ClrType  { public virtual get; }
	System.Collections.Generic.IDictionary`2[[System.Enum],[Microsoft.AspNet.OData.Builder.EnumMemberConfiguration]] ExplicitMembers  { protected get; }
	string FullName  { public virtual get; }
	System.Collections.ObjectModel.ReadOnlyCollection`1[[System.Enum]] IgnoredMembers  { public get; }
	bool IsFlags  { public get; }
	Microsoft.OData.Edm.EdmTypeKind Kind  { public virtual get; }
	System.Collections.Generic.IEnumerable`1[[Microsoft.AspNet.OData.Builder.EnumMemberConfiguration]] Members  { public get; }
	ODataModelBuilder ModelBuilder  { public virtual get; }
	string Name  { public virtual get; public set; }
	string Namespace  { public virtual get; public set; }
	System.Collections.Generic.IList`1[[System.Enum]] RemovedMembers  { protected get; }
	System.Type UnderlyingType  { public get; }

	public EnumMemberConfiguration AddMember (System.Enum member)
	public void RemoveMember (System.Enum member)
}

public class Microsoft.AspNet.OData.Builder.EnumTypeConfiguration`1 {
	string FullName  { public get; }
	System.Collections.Generic.IEnumerable`1[[Microsoft.AspNet.OData.Builder.EnumMemberConfiguration]] Members  { public get; }
	string Name  { public get; public set; }
	string Namespace  { public get; public set; }

	public EnumMemberConfiguration Member (TEnumType enumMember)
	public virtual void RemoveMember (TEnumType member)
}

public class Microsoft.AspNet.OData.Builder.FunctionConfiguration : OperationConfiguration {
	bool IncludeInServiceDocument  { public get; public set; }
	bool IsComposable  { public get; public set; }
	bool IsSideEffecting  { public virtual get; }
	OperationKind Kind  { public virtual get; }
	bool SupportedInFilter  { public get; public set; }
	bool SupportedInOrderBy  { public get; public set; }

	public System.Func`2[[Microsoft.AspNet.OData.ResourceSetContext],[System.Uri]] GetFeedFunctionLink ()
	public System.Func`2[[Microsoft.AspNet.OData.ResourceContext],[System.Uri]] GetFunctionLink ()
	public FunctionConfiguration HasDerivedTypeConstraintForReturnType ()
	public FunctionConfiguration HasDerivedTypeConstraintsForReturnType (System.Type[] subtypes)
	public FunctionConfiguration HasFeedFunctionLink (System.Func`2[[Microsoft.AspNet.OData.ResourceSetContext],[System.Uri]] functionLinkFactory, bool followsConventions)
	public FunctionConfiguration HasFunctionLink (System.Func`2[[Microsoft.AspNet.OData.ResourceContext],[System.Uri]] functionLinkFactory, bool followsConventions)
	public FunctionConfiguration Returns ()
	public FunctionConfiguration Returns (System.Type clrReturnType)
	public FunctionConfiguration ReturnsCollection ()
	public FunctionConfiguration ReturnsCollectionFromEntitySet (string entitySetName)
	public FunctionConfiguration ReturnsCollectionViaEntitySetPath (string entitySetPath)
	public FunctionConfiguration ReturnsCollectionViaEntitySetPath (string[] entitySetPath)
	public FunctionConfiguration ReturnsEntityViaEntitySetPath (string entitySetPath)
	public FunctionConfiguration ReturnsEntityViaEntitySetPath (string[] entitySetPath)
	public FunctionConfiguration ReturnsFromEntitySet (string entitySetName)
	public FunctionConfiguration SetBindingParameter (string name, IEdmTypeConfiguration bindingParameterType)
}

public class Microsoft.AspNet.OData.Builder.LengthPropertyConfiguration : PrimitivePropertyConfiguration {
	public LengthPropertyConfiguration (System.Reflection.PropertyInfo property, StructuralTypeConfiguration declaringType)

	System.Nullable`1[[System.Int32]] MaxLength  { public get; public set; }
}

public class Microsoft.AspNet.OData.Builder.LowerCamelCaser {
	public LowerCamelCaser ()
	public LowerCamelCaser (NameResolverOptions options)

	public void ApplyLowerCamelCase (ODataConventionModelBuilder builder)
	public virtual string ToLowerCamelCase (string name)
}

public class Microsoft.AspNet.OData.Builder.NavigationLinkBuilder {
	public NavigationLinkBuilder (System.Func`3[[Microsoft.AspNet.OData.ResourceContext],[Microsoft.OData.Edm.IEdmNavigationProperty],[System.Uri]] navigationLinkFactory, bool followsConventions)

	System.Func`3[[Microsoft.AspNet.OData.ResourceContext],[Microsoft.OData.Edm.IEdmNavigationProperty],[System.Uri]] Factory  { public get; }
	bool FollowsConventions  { public get; }
}

public class Microsoft.AspNet.OData.Builder.NavigationPropertyBindingConfiguration {
	public NavigationPropertyBindingConfiguration (NavigationPropertyConfiguration navigationProperty, NavigationSourceConfiguration navigationSource)
	public NavigationPropertyBindingConfiguration (NavigationPropertyConfiguration navigationProperty, NavigationSourceConfiguration navigationSource, System.Collections.Generic.IList`1[[System.Reflection.MemberInfo]] path)

	string BindingPath  { public get; }
	NavigationPropertyConfiguration NavigationProperty  { public get; }
	System.Collections.Generic.IList`1[[System.Reflection.MemberInfo]] Path  { public get; }
	NavigationSourceConfiguration TargetNavigationSource  { public get; }
}

public class Microsoft.AspNet.OData.Builder.NavigationPropertyConfiguration : PropertyConfiguration {
	public NavigationPropertyConfiguration (System.Reflection.PropertyInfo property, Microsoft.OData.Edm.EdmMultiplicity multiplicity, StructuralTypeConfiguration declaringType)

	bool ContainsTarget  { public get; }
	System.Collections.Generic.IEnumerable`1[[System.Reflection.PropertyInfo]] DependentProperties  { public get; }
	PropertyKind Kind  { public virtual get; }
	Microsoft.OData.Edm.EdmMultiplicity Multiplicity  { public get; }
	Microsoft.OData.Edm.EdmOnDeleteAction OnDeleteAction  { public get; public set; }
	NavigationPropertyConfiguration Partner  { public get; }
	System.Collections.Generic.IEnumerable`1[[System.Reflection.PropertyInfo]] PrincipalProperties  { public get; }
	System.Type RelatedClrType  { public virtual get; }

	public NavigationPropertyConfiguration AutomaticallyExpand (bool disableWhenSelectIsPresent)
	public NavigationPropertyConfiguration CascadeOnDelete ()
	public NavigationPropertyConfiguration CascadeOnDelete (bool cascade)
	public NavigationPropertyConfiguration Contained ()
	public NavigationPropertyConfiguration HasConstraint (System.Collections.Generic.KeyValuePair`2[[System.Reflection.PropertyInfo],[System.Reflection.PropertyInfo]] constraint)
	public NavigationPropertyConfiguration HasConstraint (System.Reflection.PropertyInfo dependentPropertyInfo, System.Reflection.PropertyInfo principalPropertyInfo)
	public NavigationPropertyConfiguration HasDerivedTypeConstraint ()
	public NavigationPropertyConfiguration HasDerivedTypeConstraints (System.Type[] subtypes)
	public NavigationPropertyConfiguration NonContained ()
	public NavigationPropertyConfiguration Optional ()
	public NavigationPropertyConfiguration Required ()
}

public class Microsoft.AspNet.OData.Builder.NavigationSourceLinkBuilderAnnotation {
	public NavigationSourceLinkBuilderAnnotation ()
	public NavigationSourceLinkBuilderAnnotation (NavigationSourceConfiguration navigationSource)
	public NavigationSourceLinkBuilderAnnotation (Microsoft.OData.Edm.IEdmNavigationSource navigationSource, Microsoft.OData.Edm.IEdmModel model)
	public NavigationSourceLinkBuilderAnnotation (Microsoft.OData.Edm.IEdmNavigationSource navigationSource, Microsoft.AspNet.OData.Builder.SelfLinkBuilder`1[[System.Uri]] idLinkBuilder, Microsoft.AspNet.OData.Builder.SelfLinkBuilder`1[[System.Uri]] editLinkBuilder, Microsoft.AspNet.OData.Builder.SelfLinkBuilder`1[[System.Uri]] readLinkBuilder)

	public void AddNavigationPropertyLinkBuilder (Microsoft.OData.Edm.IEdmNavigationProperty navigationProperty, NavigationLinkBuilder linkBuilder)
	public virtual System.Uri BuildEditLink (ResourceContext instanceContext, ODataMetadataLevel metadataLevel, System.Uri idLink)
	public virtual EntitySelfLinks BuildEntitySelfLinks (ResourceContext instanceContext, ODataMetadataLevel metadataLevel)
	public virtual System.Uri BuildIdLink (ResourceContext instanceContext, ODataMetadataLevel metadataLevel)
	public virtual System.Uri BuildNavigationLink (ResourceContext instanceContext, Microsoft.OData.Edm.IEdmNavigationProperty navigationProperty, ODataMetadataLevel metadataLevel)
	public virtual System.Uri BuildReadLink (ResourceContext instanceContext, ODataMetadataLevel metadataLevel, System.Uri editLink)
}

public class Microsoft.AspNet.OData.Builder.NonbindingParameterConfiguration : ParameterConfiguration {
	public NonbindingParameterConfiguration (string name, IEdmTypeConfiguration parameterType)
}

public class Microsoft.AspNet.OData.Builder.ODataConventionModelBuilder : ODataModelBuilder {
	public ODataConventionModelBuilder ()
	public ODataConventionModelBuilder (System.Web.Http.HttpConfiguration configuration)
	public ODataConventionModelBuilder (System.Web.Http.HttpConfiguration configuration, bool isQueryCompositionMode)

	bool ModelAliasingEnabled  { public get; public set; }
	System.Action`1[[Microsoft.AspNet.OData.Builder.ODataConventionModelBuilder]] OnModelCreating  { public get; public set; }

	public virtual ComplexTypeConfiguration AddComplexType (System.Type type)
	public virtual EntitySetConfiguration AddEntitySet (string name, EntityTypeConfiguration entityType)
	public virtual EntityTypeConfiguration AddEntityType (System.Type type)
	public virtual EnumTypeConfiguration AddEnumType (System.Type type)
	public virtual SingletonConfiguration AddSingleton (string name, EntityTypeConfiguration entityType)
	public virtual Microsoft.OData.Edm.IEdmModel GetEdmModel ()
	public ODataConventionModelBuilder Ignore ()
	public ODataConventionModelBuilder Ignore (System.Type[] types)
	public virtual void ValidateModel (Microsoft.OData.Edm.IEdmModel model)
}

public class Microsoft.AspNet.OData.Builder.ODataModelBuilder {
	public ODataModelBuilder ()

	NavigationPropertyBindingOption BindingOptions  { public get; public set; }
	string ContainerName  { public get; public set; }
	System.Version DataServiceVersion  { public get; public set; }
	System.Collections.Generic.IEnumerable`1[[Microsoft.AspNet.OData.Builder.EntitySetConfiguration]] EntitySets  { public virtual get; }
	System.Collections.Generic.IEnumerable`1[[Microsoft.AspNet.OData.Builder.EnumTypeConfiguration]] EnumTypes  { public virtual get; }
	System.Version MaxDataServiceVersion  { public get; public set; }
	string Namespace  { public get; public set; }
	System.Collections.Generic.IEnumerable`1[[Microsoft.AspNet.OData.Builder.NavigationSourceConfiguration]] NavigationSources  { public virtual get; }
	System.Collections.Generic.IEnumerable`1[[Microsoft.AspNet.OData.Builder.OperationConfiguration]] Operations  { public virtual get; }
	System.Collections.Generic.IEnumerable`1[[Microsoft.AspNet.OData.Builder.SingletonConfiguration]] Singletons  { public virtual get; }
	System.Collections.Generic.IEnumerable`1[[Microsoft.AspNet.OData.Builder.StructuralTypeConfiguration]] StructuralTypes  { public virtual get; }

	public virtual ActionConfiguration Action (string name)
	public virtual ComplexTypeConfiguration AddComplexType (System.Type type)
	public virtual EntitySetConfiguration AddEntitySet (string name, EntityTypeConfiguration entityType)
	public virtual EntityTypeConfiguration AddEntityType (System.Type type)
	public virtual EnumTypeConfiguration AddEnumType (System.Type type)
	public virtual void AddOperation (OperationConfiguration operation)
	public virtual SingletonConfiguration AddSingleton (string name, EntityTypeConfiguration entityType)
	public ComplexTypeConfiguration`1 ComplexType ()
	public EntitySetConfiguration`1 EntitySet (string name)
	public EntityTypeConfiguration`1 EntityType ()
	public EnumTypeConfiguration`1 EnumType ()
	public virtual FunctionConfiguration Function (string name)
	public virtual Microsoft.OData.Edm.IEdmModel GetEdmModel ()
	public IEdmTypeConfiguration GetTypeConfigurationOrNull (System.Type type)
	public virtual bool RemoveEntitySet (string name)
	public virtual bool RemoveEnumType (System.Type type)
	public virtual bool RemoveOperation (OperationConfiguration operation)
	public virtual bool RemoveOperation (string name)
	public virtual bool RemoveSingleton (string name)
	public virtual bool RemoveStructuralType (System.Type type)
	public SingletonConfiguration`1 Singleton (string name)
	public virtual void ValidateModel (Microsoft.OData.Edm.IEdmModel model)
}

public class Microsoft.AspNet.OData.Builder.OperationLinkBuilder {
	public OperationLinkBuilder (System.Func`2[[Microsoft.AspNet.OData.ResourceContext],[System.Uri]] linkFactory, bool followsConventions)
	public OperationLinkBuilder (System.Func`2[[Microsoft.AspNet.OData.ResourceSetContext],[System.Uri]] linkFactory, bool followsConventions)

	bool FollowsConventions  { public get; }

	public virtual System.Uri BuildLink (ResourceContext context)
	public virtual System.Uri BuildLink (ResourceSetContext context)
}

public class Microsoft.AspNet.OData.Builder.PrecisionPropertyConfiguration : PrimitivePropertyConfiguration {
	public PrecisionPropertyConfiguration (System.Reflection.PropertyInfo property, StructuralTypeConfiguration declaringType)

	System.Nullable`1[[System.Int32]] Precision  { public get; public set; }
}

public class Microsoft.AspNet.OData.Builder.PrimitivePropertyConfiguration : StructuralPropertyConfiguration {
	public PrimitivePropertyConfiguration (System.Reflection.PropertyInfo property, StructuralTypeConfiguration declaringType)

	string DefaultValueString  { public get; public set; }
	PropertyKind Kind  { public virtual get; }
	System.Type RelatedClrType  { public virtual get; }
	System.Nullable`1[[Microsoft.OData.Edm.EdmPrimitiveTypeKind]] TargetEdmTypeKind  { public get; }

	public PrimitivePropertyConfiguration IsConcurrencyToken ()
	public PrimitivePropertyConfiguration IsOptional ()
	public PrimitivePropertyConfiguration IsRequired ()
}

public class Microsoft.AspNet.OData.Builder.PrimitiveTypeConfiguration : IEdmTypeConfiguration {
	public PrimitiveTypeConfiguration (ODataModelBuilder builder, Microsoft.OData.Edm.IEdmPrimitiveType edmType, System.Type clrType)

	System.Type ClrType  { public virtual get; }
	Microsoft.OData.Edm.IEdmPrimitiveType EdmPrimitiveType  { public get; }
	string FullName  { public virtual get; }
	Microsoft.OData.Edm.EdmTypeKind Kind  { public virtual get; }
	ODataModelBuilder ModelBuilder  { public virtual get; }
	string Name  { public virtual get; }
	string Namespace  { public virtual get; }
}

public class Microsoft.AspNet.OData.Builder.QueryConfiguration {
	public QueryConfiguration ()

	ModelBoundQuerySettings ModelBoundQuerySettings  { public get; public set; }

	public virtual void SetCount (bool enableCount)
	public virtual void SetExpand (System.Collections.Generic.IEnumerable`1[[System.String]] properties, System.Nullable`1[[System.Int32]] maxDepth, SelectExpandType expandType)
	public virtual void SetFilter (System.Collections.Generic.IEnumerable`1[[System.String]] properties, bool enableFilter)
	public virtual void SetMaxTop (System.Nullable`1[[System.Int32]] maxTop)
	public virtual void SetOrderBy (System.Collections.Generic.IEnumerable`1[[System.String]] properties, bool enableOrderBy)
	public virtual void SetPageSize (System.Nullable`1[[System.Int32]] pageSize)
	public virtual void SetSelect (System.Collections.Generic.IEnumerable`1[[System.String]] properties, SelectExpandType selectType)
}

public class Microsoft.AspNet.OData.Builder.SelfLinkBuilder`1 {
	public SelfLinkBuilder`1 (Func`2 linkFactory, bool followsConventions)

	Func`2 Factory  { public get; }
	bool FollowsConventions  { public get; }
}

public class Microsoft.AspNet.OData.Builder.SingletonConfiguration : NavigationSourceConfiguration {
	public SingletonConfiguration ()
	public SingletonConfiguration (ODataModelBuilder modelBuilder, EntityTypeConfiguration entityType, string name)
	public SingletonConfiguration (ODataModelBuilder modelBuilder, System.Type entityClrType, string name)

	public SingletonConfiguration HasDerivedTypeConstraint ()
	public SingletonConfiguration HasDerivedTypeConstraints (System.Type[] subtypes)
}

public class Microsoft.AspNet.OData.Builder.SingletonConfiguration`1 : NavigationSourceConfiguration`1 {
	public SingletonConfiguration`1 HasDerivedTypeConstraints ()
	public SingletonConfiguration`1 HasDerivedTypeConstraints (System.Type[] subtypes)
}

[
AttributeUsageAttribute(),
]
public sealed class Microsoft.AspNet.OData.Builder.ActionOnDeleteAttribute : System.Attribute, _Attribute {
	public ActionOnDeleteAttribute (Microsoft.OData.Edm.EdmOnDeleteAction onDeleteAction)

	Microsoft.OData.Edm.EdmOnDeleteAction OnDeleteAction  { public get; }
}

[
AttributeUsageAttribute(),
]
public sealed class Microsoft.AspNet.OData.Builder.AutoExpandAttribute : System.Attribute, _Attribute {
	public AutoExpandAttribute ()

	bool DisableWhenSelectPresent  { public get; public set; }
}

[
AttributeUsageAttribute(),
]
public sealed class Microsoft.AspNet.OData.Builder.ContainedAttribute : System.Attribute, _Attribute {
	public ContainedAttribute ()
}

[
AttributeUsageAttribute(),
]
public sealed class Microsoft.AspNet.OData.Builder.DerivedTypeConstraintAttribute : System.Attribute, _Attribute {
	public DerivedTypeConstraintAttribute ()
	public DerivedTypeConstraintAttribute (System.Type[] types)

	System.Collections.Generic.ISet`1[[System.Type]] DerivedTypeConstraints  { public get; }
}

[
AttributeUsageAttribute(),
]
public sealed class Microsoft.AspNet.OData.Builder.MediaTypeAttribute : System.Attribute, _Attribute {
	public MediaTypeAttribute ()
}

public sealed class Microsoft.AspNet.OData.Builder.ODataInstanceAnnotationContainer : IODataInstanceAnnotationContainer {
	public ODataInstanceAnnotationContainer ()

	public virtual void AddPropertyAnnotation (string propertyName, string annotationName, object value)
	public virtual void AddResourceAnnotation (string annotationName, object value)
	public virtual object GetPropertyAnnotation (string propertyName, string annotationName)
	public virtual System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] GetPropertyAnnotations (string propertyName)
	public virtual object GetResourceAnnotation (string annotationName)
	public virtual System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] GetResourceAnnotations ()
}

[
AttributeUsageAttribute(),
]
public sealed class Microsoft.AspNet.OData.Builder.SingletonAttribute : System.Attribute, _Attribute {
	public SingletonAttribute ()
}

[
EditorBrowsableAttribute(),
ExtensionAttribute(),
]
public sealed class Microsoft.AspNet.OData.Extensions.HttpConfigurationExtensions {
	[
	ExtensionAttribute(),
	]
	public static void AddODataQueryFilter (System.Web.Http.HttpConfiguration configuration)

	[
	ExtensionAttribute(),
	]
	public static void AddODataQueryFilter (System.Web.Http.HttpConfiguration configuration, System.Web.Http.Filters.IActionFilter queryFilter)

	[
	ExtensionAttribute(),
	]
	public static System.Web.Http.HttpConfiguration Count (System.Web.Http.HttpConfiguration configuration)

	[
	ExtensionAttribute(),
	]
	public static System.Web.Http.HttpConfiguration Count (System.Web.Http.HttpConfiguration configuration, QueryOptionSetting setting)

	[
	ExtensionAttribute(),
	]
	public static void EnableContinueOnErrorHeader (System.Web.Http.HttpConfiguration configuration)

	[
	ExtensionAttribute(),
	]
	public static void EnableDependencyInjection (System.Web.Http.HttpConfiguration configuration)

	[
	ExtensionAttribute(),
	]
	public static void EnableDependencyInjection (System.Web.Http.HttpConfiguration configuration, System.Action`1[[Microsoft.OData.IContainerBuilder]] configureAction)

	[
	ExtensionAttribute(),
	]
	public static System.Web.Http.HttpConfiguration Expand (System.Web.Http.HttpConfiguration configuration)

	[
	ExtensionAttribute(),
	]
	public static System.Web.Http.HttpConfiguration Expand (System.Web.Http.HttpConfiguration configuration, QueryOptionSetting setting)

	[
	ExtensionAttribute(),
	]
	public static System.Web.Http.HttpConfiguration Filter (System.Web.Http.HttpConfiguration configuration)

	[
	ExtensionAttribute(),
	]
	public static System.Web.Http.HttpConfiguration Filter (System.Web.Http.HttpConfiguration configuration, QueryOptionSetting setting)

	[
	ExtensionAttribute(),
	]
	public static DefaultQuerySettings GetDefaultQuerySettings (System.Web.Http.HttpConfiguration configuration)

	[
	ExtensionAttribute(),
	]
	public static IETagHandler GetETagHandler (System.Web.Http.HttpConfiguration configuration)

	[
	ExtensionAttribute(),
	]
	public static System.TimeZoneInfo GetTimeZoneInfo (System.Web.Http.HttpConfiguration configuration)

	[
	ExtensionAttribute(),
	]
	public static ODataRoute MapODataServiceRoute (System.Web.Http.HttpConfiguration configuration, string routeName, string routePrefix, Microsoft.OData.Edm.IEdmModel model)

	[
	ExtensionAttribute(),
	]
	public static ODataRoute MapODataServiceRoute (System.Web.Http.HttpConfiguration configuration, string routeName, string routePrefix, System.Action`1[[Microsoft.OData.IContainerBuilder]] configureAction)

	[
	ExtensionAttribute(),
	]
	public static ODataRoute MapODataServiceRoute (System.Web.Http.HttpConfiguration configuration, string routeName, string routePrefix, Microsoft.OData.Edm.IEdmModel model, ODataBatchHandler batchHandler)

	[
	ExtensionAttribute(),
	]
	public static ODataRoute MapODataServiceRoute (System.Web.Http.HttpConfiguration configuration, string routeName, string routePrefix, Microsoft.OData.Edm.IEdmModel model, System.Net.Http.HttpMessageHandler defaultHandler)

	[
	ExtensionAttribute(),
	]
	public static ODataRoute MapODataServiceRoute (System.Web.Http.HttpConfiguration configuration, string routeName, string routePrefix, Microsoft.OData.Edm.IEdmModel model, IODataPathHandler pathHandler, System.Collections.Generic.IEnumerable`1[[Microsoft.AspNet.OData.Routing.Conventions.IODataRoutingConvention]] routingConventions)

	[
	ExtensionAttribute(),
	]
	public static ODataRoute MapODataServiceRoute (System.Web.Http.HttpConfiguration configuration, string routeName, string routePrefix, Microsoft.OData.Edm.IEdmModel model, IODataPathHandler pathHandler, System.Collections.Generic.IEnumerable`1[[Microsoft.AspNet.OData.Routing.Conventions.IODataRoutingConvention]] routingConventions, ODataBatchHandler batchHandler)

	[
	ExtensionAttribute(),
	]
	public static ODataRoute MapODataServiceRoute (System.Web.Http.HttpConfiguration configuration, string routeName, string routePrefix, Microsoft.OData.Edm.IEdmModel model, IODataPathHandler pathHandler, System.Collections.Generic.IEnumerable`1[[Microsoft.AspNet.OData.Routing.Conventions.IODataRoutingConvention]] routingConventions, System.Net.Http.HttpMessageHandler defaultHandler)

	[
	ExtensionAttribute(),
	]
	public static System.Web.Http.HttpConfiguration MaxTop (System.Web.Http.HttpConfiguration configuration, System.Nullable`1[[System.Int32]] maxTopValue)

	[
	ExtensionAttribute(),
	]
	public static System.Web.Http.HttpConfiguration OrderBy (System.Web.Http.HttpConfiguration configuration)

	[
	ExtensionAttribute(),
	]
	public static System.Web.Http.HttpConfiguration OrderBy (System.Web.Http.HttpConfiguration configuration, QueryOptionSetting setting)

	[
	ExtensionAttribute(),
	]
	public static System.Web.Http.HttpConfiguration Select (System.Web.Http.HttpConfiguration configuration)

	[
	ExtensionAttribute(),
	]
	public static System.Web.Http.HttpConfiguration Select (System.Web.Http.HttpConfiguration configuration, QueryOptionSetting setting)

	[
	ExtensionAttribute(),
	]
	public static void SetCompatibilityOptions (System.Web.Http.HttpConfiguration configuration, CompatibilityOptions options)

	[
	ExtensionAttribute(),
	]
	public static void SetDefaultQuerySettings (System.Web.Http.HttpConfiguration configuration, DefaultQuerySettings defaultQuerySettings)

	[
	ExtensionAttribute(),
	]
	public static void SetETagHandler (System.Web.Http.HttpConfiguration configuration, IETagHandler handler)

	[
	ExtensionAttribute(),
	]
	public static void SetSerializeNullDynamicProperty (System.Web.Http.HttpConfiguration configuration, bool serialize)

	[
	ExtensionAttribute(),
	]
	public static void SetTimeZoneInfo (System.Web.Http.HttpConfiguration configuration, System.TimeZoneInfo timeZoneInfo)

	[
	ExtensionAttribute(),
	]
	public static void SetUrlKeyDelimiter (System.Web.Http.HttpConfiguration configuration, Microsoft.OData.ODataUrlKeyDelimiter urlKeyDelimiter)

	[
	ExtensionAttribute(),
	]
	public static System.Web.Http.HttpConfiguration SkipToken (System.Web.Http.HttpConfiguration configuration)

	[
	ExtensionAttribute(),
	]
	public static System.Web.Http.HttpConfiguration SkipToken (System.Web.Http.HttpConfiguration configuration, QueryOptionSetting setting)

	[
	ExtensionAttribute(),
	]
	public static System.Web.Http.HttpConfiguration UseCustomContainerBuilder (System.Web.Http.HttpConfiguration configuration, System.Func`1[[Microsoft.OData.IContainerBuilder]] builderFactory)
}

[
EditorBrowsableAttribute(),
ExtensionAttribute(),
]
public sealed class Microsoft.AspNet.OData.Extensions.HttpErrorExtensions {
	[
	ExtensionAttribute(),
	]
	public static Microsoft.OData.ODataError CreateODataError (System.Web.Http.HttpError httpError)
}

[
EditorBrowsableAttribute(),
ExtensionAttribute(),
]
public sealed class Microsoft.AspNet.OData.Extensions.HttpRequestMessageExtensions {
	[
	ExtensionAttribute(),
	]
	public static System.Net.Http.HttpResponseMessage CreateErrorResponse (System.Net.Http.HttpRequestMessage request, System.Net.HttpStatusCode statusCode, Microsoft.OData.ODataError oDataError)

	[
	ExtensionAttribute(),
	]
	public static System.IServiceProvider CreateRequestContainer (System.Net.Http.HttpRequestMessage request, string routeName)

	[
	ExtensionAttribute(),
	]
	public static void DeleteRequestContainer (System.Net.Http.HttpRequestMessage request, bool dispose)

	[
	ExtensionAttribute(),
	]
	public static ODataDeserializerProvider GetDeserializerProvider (System.Net.Http.HttpRequestMessage request)

	[
	ExtensionAttribute(),
	]
	public static ETag GetETag (System.Net.Http.HttpRequestMessage request, System.Net.Http.Headers.EntityTagHeaderValue entityTagHeaderValue)

	[
	ExtensionAttribute(),
	]
	public static ETag`1 GetETag (System.Net.Http.HttpRequestMessage request, System.Net.Http.Headers.EntityTagHeaderValue entityTagHeaderValue)

	[
	ExtensionAttribute(),
	]
	public static Microsoft.OData.Edm.IEdmModel GetModel (System.Net.Http.HttpRequestMessage request)

	[
	ExtensionAttribute(),
	]
	public static System.Uri GetNextPageLink (System.Net.Http.HttpRequestMessage request, int pageSize)

	[
	ExtensionAttribute(),
	]
	public static System.Uri GetNextPageLink (System.Net.Http.HttpRequestMessage request, int pageSize, object instance, System.Func`2[[System.Object],[System.String]] objToSkipTokenValue)

	[
	ExtensionAttribute(),
	]
	public static IODataPathHandler GetPathHandler (System.Net.Http.HttpRequestMessage request)

	[
	ExtensionAttribute(),
	]
	public static Microsoft.OData.ODataMessageReaderSettings GetReaderSettings (System.Net.Http.HttpRequestMessage request)

	[
	ExtensionAttribute(),
	]
	public static System.IServiceProvider GetRequestContainer (System.Net.Http.HttpRequestMessage request)

	[
	ExtensionAttribute(),
	]
	public static System.Collections.Generic.IEnumerable`1[[Microsoft.AspNet.OData.Routing.Conventions.IODataRoutingConvention]] GetRoutingConventions (System.Net.Http.HttpRequestMessage request)

	[
	ExtensionAttribute(),
	]
	public static ODataSerializerProvider GetSerializerProvider (System.Net.Http.HttpRequestMessage request)

	[
	ExtensionAttribute(),
	]
	public static Microsoft.OData.ODataMessageWriterSettings GetWriterSettings (System.Net.Http.HttpRequestMessage request)

	[
	ExtensionAttribute(),
	]
	public static HttpRequestMessageProperties ODataProperties (System.Net.Http.HttpRequestMessage request)
}

[
EditorBrowsableAttribute(),
ExtensionAttribute(),
]
public sealed class Microsoft.AspNet.OData.Extensions.UrlHelperExtensions {
	[
	ExtensionAttribute(),
	]
	public static string CreateODataLink (System.Web.Http.Routing.UrlHelper urlHelper, Microsoft.OData.UriParser.ODataPathSegment[] segments)

	[
	ExtensionAttribute(),
	]
	public static string CreateODataLink (System.Web.Http.Routing.UrlHelper urlHelper, System.Collections.Generic.IList`1[[Microsoft.OData.UriParser.ODataPathSegment]] segments)

	[
	ExtensionAttribute(),
	]
	public static string CreateODataLink (System.Web.Http.Routing.UrlHelper urlHelper, string routeName, IODataPathHandler pathHandler, System.Collections.Generic.IList`1[[Microsoft.OData.UriParser.ODataPathSegment]] segments)
}

public class Microsoft.AspNet.OData.Extensions.HttpRequestMessageProperties {
	Microsoft.OData.UriParser.Aggregation.ApplyClause ApplyClause  { public get; public set; }
	System.Uri DeltaLink  { public get; public set; }
	System.Uri NextLink  { public get; public set; }
	ODataPath Path  { public get; public set; }
	string RouteName  { public get; public set; }
	System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] RoutingConventionsStore  { public get; }
	Microsoft.OData.UriParser.SelectExpandClause SelectExpandClause  { public get; public set; }
	System.Nullable`1[[System.Int64]] TotalCount  { public get; public set; }
}

public enum Microsoft.AspNet.OData.Formatter.ODataMetadataLevel : int {
	FullMetadata = 1
	MinimalMetadata = 0
	NoMetadata = 2
}

public interface Microsoft.AspNet.OData.Formatter.IETagHandler {
	System.Net.Http.Headers.EntityTagHeaderValue CreateETag (System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] properties)
	System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] ParseETag (System.Net.Http.Headers.EntityTagHeaderValue etagHeaderValue)
}

public abstract class Microsoft.AspNet.OData.Formatter.ODataRawValueMediaTypeMapping : System.Net.Http.Formatting.MediaTypeMapping {
	protected ODataRawValueMediaTypeMapping (string mediaType)

	protected abstract bool IsMatch (Microsoft.OData.UriParser.PropertySegment propertySegment)
	public virtual double TryMatchMediaType (System.Net.Http.HttpRequestMessage request)
}

[
ExtensionAttribute(),
]
public sealed class Microsoft.AspNet.OData.Formatter.ODataMediaTypeFormatters {
	public static System.Collections.Generic.IList`1[[Microsoft.AspNet.OData.Formatter.ODataMediaTypeFormatter]] Create ()
}

public sealed class Microsoft.AspNet.OData.Formatter.ODataModelBinderConverter {
	public static object Convert (object graph, Microsoft.OData.Edm.IEdmTypeReference edmTypeReference, System.Type clrType, string parameterName, ODataDeserializerContext readContext, System.IServiceProvider requestContainer)
}

[
DefaultMemberAttribute(),
]
public class Microsoft.AspNet.OData.Formatter.ETag : System.Dynamic.DynamicObject, IDynamicMetaObjectProvider {
	public ETag ()

	System.Type EntityType  { public get; public set; }
	bool IsAny  { public get; public set; }
	bool IsIfNoneMatch  { public get; public set; }
	bool IsWellFormed  { public get; public set; }
	object Item [string key] { public get; public set; }

	public virtual System.Linq.IQueryable ApplyTo (System.Linq.IQueryable query)
	public virtual bool TryGetMember (System.Dynamic.GetMemberBinder binder, out System.Object& result)
	public virtual bool TrySetMember (System.Dynamic.SetMemberBinder binder, object value)
}

public class Microsoft.AspNet.OData.Formatter.ETag`1 : ETag, IDynamicMetaObjectProvider {
	public ETag`1 ()

	public IQueryable`1 ApplyTo (IQueryable`1 query)
	public virtual System.Linq.IQueryable ApplyTo (System.Linq.IQueryable query)
}

public class Microsoft.AspNet.OData.Formatter.ODataBinaryValueMediaTypeMapping : ODataRawValueMediaTypeMapping {
	public ODataBinaryValueMediaTypeMapping ()

	protected virtual bool IsMatch (Microsoft.OData.UriParser.PropertySegment propertySegment)
}

public class Microsoft.AspNet.OData.Formatter.ODataCountMediaTypeMapping : System.Net.Http.Formatting.MediaTypeMapping {
	public ODataCountMediaTypeMapping ()

	public virtual double TryMatchMediaType (System.Net.Http.HttpRequestMessage request)
}

public class Microsoft.AspNet.OData.Formatter.ODataEnumValueMediaTypeMapping : ODataRawValueMediaTypeMapping {
	public ODataEnumValueMediaTypeMapping ()

	protected virtual bool IsMatch (Microsoft.OData.UriParser.PropertySegment propertySegment)
}

public class Microsoft.AspNet.OData.Formatter.ODataMediaTypeFormatter : System.Net.Http.Formatting.MediaTypeFormatter {
	public ODataMediaTypeFormatter (System.Collections.Generic.IEnumerable`1[[Microsoft.OData.ODataPayloadKind]] payloadKinds)

	System.Func`2[[System.Net.Http.HttpRequestMessage],[System.Uri]] BaseAddressFactory  { public get; public set; }
	System.Net.Http.HttpRequestMessage Request  { public get; public set; }

	public virtual bool CanReadType (System.Type type)
	public virtual bool CanWriteType (System.Type type)
	public static System.Uri GetDefaultBaseAddress (System.Net.Http.HttpRequestMessage request)
	public virtual System.Net.Http.Formatting.MediaTypeFormatter GetPerRequestFormatterInstance (System.Type type, System.Net.Http.HttpRequestMessage request, System.Net.Http.Headers.MediaTypeHeaderValue mediaType)
	public virtual System.Threading.Tasks.Task`1[[System.Object]] ReadFromStreamAsync (System.Type type, System.IO.Stream readStream, System.Net.Http.HttpContent content, System.Net.Http.Formatting.IFormatterLogger formatterLogger)
	public virtual void SetDefaultContentHeaders (System.Type type, System.Net.Http.Headers.HttpContentHeaders headers, System.Net.Http.Headers.MediaTypeHeaderValue mediaType)
	public virtual System.Threading.Tasks.Task WriteToStreamAsync (System.Type type, object value, System.IO.Stream writeStream, System.Net.Http.HttpContent content, System.Net.TransportContext transportContext, System.Threading.CancellationToken cancellationToken)
}

public class Microsoft.AspNet.OData.Formatter.ODataModelBinderProvider : System.Web.Http.ModelBinding.ModelBinderProvider {
	public ODataModelBinderProvider ()

	public virtual System.Web.Http.ModelBinding.IModelBinder GetBinder (System.Web.Http.HttpConfiguration configuration, System.Type modelType)
}

public class Microsoft.AspNet.OData.Formatter.ODataPrimitiveValueMediaTypeMapping : ODataRawValueMediaTypeMapping {
	public ODataPrimitiveValueMediaTypeMapping ()

	protected virtual bool IsMatch (Microsoft.OData.UriParser.PropertySegment propertySegment)
}

public class Microsoft.AspNet.OData.Formatter.ODataStreamMediaTypeMapping : System.Net.Http.Formatting.MediaTypeMapping {
	public ODataStreamMediaTypeMapping ()

	public virtual double TryMatchMediaType (System.Net.Http.HttpRequestMessage request)
}

public class Microsoft.AspNet.OData.Formatter.QueryStringMediaTypeMapping : System.Net.Http.Formatting.MediaTypeMapping {
	public QueryStringMediaTypeMapping (string queryStringParameterName, System.Net.Http.Headers.MediaTypeHeaderValue mediaType)
	public QueryStringMediaTypeMapping (string queryStringParameterName, string mediaType)

	string QueryStringParameterName  { public get; }

	public virtual double TryMatchMediaType (System.Net.Http.HttpRequestMessage request)
}

[
FlagsAttribute(),
]
public enum Microsoft.AspNet.OData.Query.AllowedArithmeticOperators : int {
	Add = 1
	All = 31
	Divide = 8
	Modulo = 16
	Multiply = 4
	None = 0
	Subtract = 2
}

[
FlagsAttribute(),
]
public enum Microsoft.AspNet.OData.Query.AllowedFunctions : int {
	All = 268435456
	AllDateTimeFunctions = 7010304
	AllFunctions = 535494655
	AllMathFunctions = 58720256
	AllStringFunctions = 1023
	Any = 134217728
	Cast = 1024
	Ceiling = 33554432
	Concat = 32
	Contains = 4
	Date = 4096
	Day = 32768
	EndsWith = 2
	Floor = 16777216
	FractionalSeconds = 4194304
	Hour = 131072
	IndexOf = 16
	IsOf = 67108864
	Length = 8
	Minute = 524288
	Month = 8192
	None = 0
	Round = 8388608
	Second = 2097152
	StartsWith = 1
	Substring = 64
	Time = 16384
	ToLower = 128
	ToUpper = 256
	Trim = 512
	Year = 2048
}

[
FlagsAttribute(),
]
public enum Microsoft.AspNet.OData.Query.AllowedLogicalOperators : int {
	All = 1023
	And = 2
	Equal = 4
	GreaterThan = 16
	GreaterThanOrEqual = 32
	Has = 512
	LessThan = 64
	LessThanOrEqual = 128
	None = 0
	Not = 256
	NotEqual = 8
	Or = 1
}

[
FlagsAttribute(),
]
public enum Microsoft.AspNet.OData.Query.AllowedQueryOptions : int {
	All = 2047
	Apply = 1024
	Count = 64
	DeltaToken = 512
	Expand = 2
	Filter = 1
	Format = 128
	None = 0
	OrderBy = 8
	Select = 4
	Skip = 32
	SkipToken = 256
	Supported = 1535
	Top = 16
}

public enum Microsoft.AspNet.OData.Query.HandleNullPropagationOption : int {
	Default = 0
	False = 2
	True = 1
}

public enum Microsoft.AspNet.OData.Query.QueryOptionSetting : int {
	Allowed = 0
	Disabled = 1
}

public enum Microsoft.AspNet.OData.Query.SelectExpandType : int {
	Allowed = 0
	Automatic = 1
	Disabled = 2
}

public interface Microsoft.AspNet.OData.Query.IODataQueryOptionsParser {
	bool CanParse (System.Net.Http.HttpRequestMessage request)
	System.Threading.Tasks.Task`1[[System.String]] ParseAsync (System.IO.Stream requestStream)
}

public interface Microsoft.AspNet.OData.Query.IPropertyMapper {
	string MapProperty (string propertyName)
}

public interface Microsoft.AspNet.OData.Query.ISelectExpandWrapper {
	System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] ToDictionary ()
	System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] ToDictionary (System.Func`3[[Microsoft.OData.Edm.IEdmModel],[Microsoft.OData.Edm.IEdmStructuredType],[Microsoft.AspNet.OData.Query.IPropertyMapper]] propertyMapperProvider)
}

public interface Microsoft.AspNet.OData.Query.ITruncatedCollection : IEnumerable {
	bool IsTruncated  { public abstract get; }
	int PageSize  { public abstract get; }
}

public abstract class Microsoft.AspNet.OData.Query.OrderByNode {
	protected OrderByNode (Microsoft.OData.UriParser.OrderByClause orderByClause)
	protected OrderByNode (Microsoft.OData.UriParser.OrderByDirection direction)

	Microsoft.OData.UriParser.OrderByDirection Direction  { public get; }

	public static System.Collections.Generic.IList`1[[Microsoft.AspNet.OData.Query.OrderByNode]] CreateCollection (Microsoft.OData.UriParser.OrderByClause orderByClause)
}

public abstract class Microsoft.AspNet.OData.Query.SkipTokenHandler {
	protected SkipTokenHandler ()

	public abstract IQueryable`1 ApplyTo (IQueryable`1 query, SkipTokenQueryOption skipTokenQueryOption)
	public abstract System.Linq.IQueryable ApplyTo (System.Linq.IQueryable query, SkipTokenQueryOption skipTokenQueryOption)
	public abstract System.Uri GenerateNextPageLink (System.Uri baseUri, int pageSize, object instance, ODataSerializerContext context)
}

public sealed class Microsoft.AspNet.OData.Query.ODataQueryOptionsParserFactory {
	public static System.Collections.Generic.IList`1[[Microsoft.AspNet.OData.Query.IODataQueryOptionsParser]] Create ()
	public static IODataQueryOptionsParser GetQueryOptionsParser (System.Net.Http.HttpRequestMessage request)
}

public class Microsoft.AspNet.OData.Query.ApplyQueryOption {
	public ApplyQueryOption (string rawValue, ODataQueryContext context, Microsoft.OData.UriParser.ODataQueryOptionParser queryOptionParser)

	Microsoft.OData.UriParser.Aggregation.ApplyClause ApplyClause  { public get; }
	ODataQueryContext Context  { public get; }
	string RawValue  { public get; }
	System.Type ResultClrType  { public get; }

	public System.Linq.IQueryable ApplyTo (System.Linq.IQueryable query, ODataQuerySettings querySettings)
}

public class Microsoft.AspNet.OData.Query.CountQueryOption {
	public CountQueryOption (string rawValue, ODataQueryContext context, Microsoft.OData.UriParser.ODataQueryOptionParser queryOptionParser)

	ODataQueryContext Context  { public get; }
	string RawValue  { public get; }
	CountQueryValidator Validator  { public get; public set; }
	bool Value  { public get; }

	public System.Nullable`1[[System.Int64]] GetEntityCount (System.Linq.IQueryable query)
	public void Validate (ODataValidationSettings validationSettings)
}

public class Microsoft.AspNet.OData.Query.DefaultQuerySettings {
	public DefaultQuerySettings ()

	bool EnableCount  { public get; public set; }
	bool EnableExpand  { public get; public set; }
	bool EnableFilter  { public get; public set; }
	bool EnableOrderBy  { public get; public set; }
	bool EnableSelect  { public get; public set; }
	bool EnableSkipToken  { public get; public set; }
	System.Nullable`1[[System.Int32]] MaxTop  { public get; public set; }
}

public class Microsoft.AspNet.OData.Query.DefaultSkipTokenHandler : SkipTokenHandler {
	public DefaultSkipTokenHandler ()

	public virtual IQueryable`1 ApplyTo (IQueryable`1 query, SkipTokenQueryOption skipTokenQueryOption)
	public virtual System.Linq.IQueryable ApplyTo (System.Linq.IQueryable query, SkipTokenQueryOption skipTokenQueryOption)
	public virtual System.Uri GenerateNextPageLink (System.Uri baseUri, int pageSize, object instance, ODataSerializerContext context)
}

public class Microsoft.AspNet.OData.Query.ExpandConfiguration {
	public ExpandConfiguration ()

	SelectExpandType ExpandType  { public get; public set; }
	int MaxDepth  { public get; public set; }
}

public class Microsoft.AspNet.OData.Query.FilterQueryOption {
	public FilterQueryOption (string rawValue, ODataQueryContext context, Microsoft.OData.UriParser.ODataQueryOptionParser queryOptionParser)

	ODataQueryContext Context  { public get; }
	Microsoft.OData.UriParser.FilterClause FilterClause  { public get; }
	string RawValue  { public get; }
	FilterQueryValidator Validator  { public get; public set; }

	public System.Linq.IQueryable ApplyTo (System.Linq.IQueryable query, ODataQuerySettings querySettings)
	public void Validate (ODataValidationSettings validationSettings)
}

public class Microsoft.AspNet.OData.Query.ModelBoundQuerySettings {
	public ModelBoundQuerySettings ()
	public ModelBoundQuerySettings (ModelBoundQuerySettings querySettings)

	System.Nullable`1[[System.Boolean]] Countable  { public get; public set; }
	System.Nullable`1[[System.Boolean]] DefaultEnableFilter  { public get; public set; }
	System.Nullable`1[[System.Boolean]] DefaultEnableOrderBy  { public get; public set; }
	System.Nullable`1[[Microsoft.AspNet.OData.Query.SelectExpandType]] DefaultExpandType  { public get; public set; }
	int DefaultMaxDepth  { public get; public set; }
	System.Nullable`1[[Microsoft.AspNet.OData.Query.SelectExpandType]] DefaultSelectType  { public get; public set; }
	System.Collections.Generic.Dictionary`2[[System.String],[Microsoft.AspNet.OData.Query.ExpandConfiguration]] ExpandConfigurations  { public get; }
	System.Collections.Generic.Dictionary`2[[System.String],[System.Boolean]] FilterConfigurations  { public get; }
	System.Nullable`1[[System.Int32]] MaxTop  { public get; public set; }
	System.Collections.Generic.Dictionary`2[[System.String],[System.Boolean]] OrderByConfigurations  { public get; }
	System.Nullable`1[[System.Int32]] PageSize  { public get; public set; }
	System.Collections.Generic.Dictionary`2[[System.String],[Microsoft.AspNet.OData.Query.SelectExpandType]] SelectConfigurations  { public get; }
}

[
ODataQueryParameterBindingAttribute(),
]
public class Microsoft.AspNet.OData.Query.ODataQueryOptions {
	public ODataQueryOptions (ODataQueryContext context, System.Net.Http.HttpRequestMessage request)

	ApplyQueryOption Apply  { public get; }
	ODataQueryContext Context  { public get; }
	CountQueryOption Count  { public get; }
	FilterQueryOption Filter  { public get; }
	ETag IfMatch  { public virtual get; }
	ETag IfNoneMatch  { public virtual get; }
	OrderByQueryOption OrderBy  { public get; }
	ODataRawQueryOptions RawValues  { public get; }
	System.Net.Http.HttpRequestMessage Request  { public get; }
	SelectExpandQueryOption SelectExpand  { public get; }
	SkipQueryOption Skip  { public get; }
	SkipTokenQueryOption SkipToken  { public get; }
	TopQueryOption Top  { public get; }
	ODataQueryValidator Validator  { public get; public set; }

	public virtual System.Linq.IQueryable ApplyTo (System.Linq.IQueryable query)
	public virtual System.Linq.IQueryable ApplyTo (System.Linq.IQueryable query, AllowedQueryOptions ignoreQueryOptions)
	public virtual System.Linq.IQueryable ApplyTo (System.Linq.IQueryable query, ODataQuerySettings querySettings)
	public virtual object ApplyTo (object entity, ODataQuerySettings querySettings)
	public virtual System.Linq.IQueryable ApplyTo (System.Linq.IQueryable query, ODataQuerySettings querySettings, AllowedQueryOptions ignoreQueryOptions)
	public virtual object ApplyTo (object entity, ODataQuerySettings querySettings, AllowedQueryOptions ignoreQueryOptions)
	public virtual OrderByQueryOption GenerateStableOrder ()
	internal virtual ETag GetETag (System.Net.Http.Headers.EntityTagHeaderValue etagHeaderValue)
	public bool IsSupportedQueryOption (string queryOptionName)
	public static bool IsSystemQueryOption (string queryOptionName)
	public static bool IsSystemQueryOption (string queryOptionName, bool isDollarSignOptional)
	public static IQueryable`1 LimitResults (IQueryable`1 queryable, int limit, out System.Boolean& resultsLimited)
	public static IQueryable`1 LimitResults (IQueryable`1 queryable, int limit, bool parameterize, out System.Boolean& resultsLimited)
	public virtual void Validate (ODataValidationSettings validationSettings)
}

[
ODataQueryParameterBindingAttribute(),
]
public class Microsoft.AspNet.OData.Query.ODataQueryOptions`1 : ODataQueryOptions {
	public ODataQueryOptions`1 (ODataQueryContext context, System.Net.Http.HttpRequestMessage request)

	ETag`1 IfMatch  { public get; }
	ETag`1 IfNoneMatch  { public get; }

	public virtual System.Linq.IQueryable ApplyTo (System.Linq.IQueryable query)
	public virtual System.Linq.IQueryable ApplyTo (System.Linq.IQueryable query, ODataQuerySettings querySettings)
	internal virtual ETag GetETag (System.Net.Http.Headers.EntityTagHeaderValue etagHeaderValue)
}

public class Microsoft.AspNet.OData.Query.ODataQuerySettings {
	public ODataQuerySettings ()

	bool EnableConstantParameterization  { public get; public set; }
	bool EnableCorrelatedSubqueryBuffering  { public get; public set; }
	bool EnsureStableOrdering  { public get; public set; }
	HandleNullPropagationOption HandleNullPropagation  { public get; public set; }
	bool HandleReferenceNavigationPropertyExpandFilter  { public get; public set; }
	System.Nullable`1[[System.Int32]] PageSize  { public get; public set; }
}

public class Microsoft.AspNet.OData.Query.ODataRawQueryOptions {
	public ODataRawQueryOptions ()

	string Apply  { public get; }
	string Count  { public get; }
	string DeltaToken  { public get; }
	string Expand  { public get; }
	string Filter  { public get; }
	string Format  { public get; }
	string OrderBy  { public get; }
	string Select  { public get; }
	string Skip  { public get; }
	string SkipToken  { public get; }
	string Top  { public get; }
}

public class Microsoft.AspNet.OData.Query.ODataValidationSettings {
	public ODataValidationSettings ()

	AllowedArithmeticOperators AllowedArithmeticOperators  { public get; public set; }
	AllowedFunctions AllowedFunctions  { public get; public set; }
	AllowedLogicalOperators AllowedLogicalOperators  { public get; public set; }
	System.Collections.ObjectModel.Collection`1[[System.String]] AllowedOrderByProperties  { public get; }
	AllowedQueryOptions AllowedQueryOptions  { public get; public set; }
	int MaxAnyAllExpressionDepth  { public get; public set; }
	int MaxExpansionDepth  { public get; public set; }
	int MaxNodeCount  { public get; public set; }
	int MaxOrderByNodeCount  { public get; public set; }
	System.Nullable`1[[System.Int32]] MaxSkip  { public get; public set; }
	System.Nullable`1[[System.Int32]] MaxTop  { public get; public set; }
}

public class Microsoft.AspNet.OData.Query.OrderByCountNode : OrderByNode {
	public OrderByCountNode (Microsoft.OData.UriParser.OrderByClause orderByClause)

	Microsoft.OData.UriParser.OrderByClause OrderByClause  { public get; }
}

public class Microsoft.AspNet.OData.Query.OrderByItNode : OrderByNode {
	public OrderByItNode (Microsoft.OData.UriParser.OrderByDirection direction)
}

public class Microsoft.AspNet.OData.Query.OrderByOpenPropertyNode : OrderByNode {
	public OrderByOpenPropertyNode (Microsoft.OData.UriParser.OrderByClause orderByClause)

	Microsoft.OData.UriParser.OrderByClause OrderByClause  { public get; }
	string PropertyName  { public get; }
}

public class Microsoft.AspNet.OData.Query.OrderByPropertyNode : OrderByNode {
	public OrderByPropertyNode (Microsoft.OData.UriParser.OrderByClause orderByClause)
	public OrderByPropertyNode (Microsoft.OData.Edm.IEdmProperty property, Microsoft.OData.UriParser.OrderByDirection direction)

	Microsoft.OData.UriParser.OrderByClause OrderByClause  { public get; }
	Microsoft.OData.Edm.IEdmProperty Property  { public get; }
}

public class Microsoft.AspNet.OData.Query.OrderByQueryOption {
	public OrderByQueryOption (string rawValue, ODataQueryContext context, Microsoft.OData.UriParser.ODataQueryOptionParser queryOptionParser)

	ODataQueryContext Context  { public get; }
	Microsoft.OData.UriParser.OrderByClause OrderByClause  { public get; }
	System.Collections.Generic.IList`1[[Microsoft.AspNet.OData.Query.OrderByNode]] OrderByNodes  { public get; }
	string RawValue  { public get; }
	OrderByQueryValidator Validator  { public get; public set; }

	public IOrderedQueryable`1 ApplyTo (IQueryable`1 query)
	public System.Linq.IOrderedQueryable ApplyTo (System.Linq.IQueryable query)
	public IOrderedQueryable`1 ApplyTo (IQueryable`1 query, ODataQuerySettings querySettings)
	public System.Linq.IOrderedQueryable ApplyTo (System.Linq.IQueryable query, ODataQuerySettings querySettings)
	public void Validate (ODataValidationSettings validationSettings)
}

public class Microsoft.AspNet.OData.Query.ParameterAliasNodeTranslator : Microsoft.OData.UriParser.QueryNodeVisitor`1[[Microsoft.OData.UriParser.QueryNode]] {
	public ParameterAliasNodeTranslator (System.Collections.Generic.IDictionary`2[[System.String],[Microsoft.OData.UriParser.SingleValueNode]] parameterAliasNodes)

	public virtual Microsoft.OData.UriParser.QueryNode Visit (Microsoft.OData.UriParser.AllNode nodeIn)
	public virtual Microsoft.OData.UriParser.QueryNode Visit (Microsoft.OData.UriParser.AnyNode nodeIn)
	public virtual Microsoft.OData.UriParser.QueryNode Visit (Microsoft.OData.UriParser.BinaryOperatorNode nodeIn)
	public virtual Microsoft.OData.UriParser.QueryNode Visit (Microsoft.OData.UriParser.CollectionComplexNode nodeIn)
	public virtual Microsoft.OData.UriParser.QueryNode Visit (Microsoft.OData.UriParser.CollectionConstantNode nodeIn)
	public virtual Microsoft.OData.UriParser.QueryNode Visit (Microsoft.OData.UriParser.CollectionFunctionCallNode nodeIn)
	public virtual Microsoft.OData.UriParser.QueryNode Visit (Microsoft.OData.UriParser.CollectionNavigationNode nodeIn)
	public virtual Microsoft.OData.UriParser.QueryNode Visit (Microsoft.OData.UriParser.CollectionOpenPropertyAccessNode nodeIn)
	public virtual Microsoft.OData.UriParser.QueryNode Visit (Microsoft.OData.UriParser.CollectionPropertyAccessNode nodeIn)
	public virtual Microsoft.OData.UriParser.QueryNode Visit (Microsoft.OData.UriParser.CollectionResourceCastNode nodeIn)
	public virtual Microsoft.OData.UriParser.QueryNode Visit (Microsoft.OData.UriParser.CollectionResourceFunctionCallNode nodeIn)
	public virtual Microsoft.OData.UriParser.QueryNode Visit (Microsoft.OData.UriParser.ConstantNode nodeIn)
	public virtual Microsoft.OData.UriParser.QueryNode Visit (Microsoft.OData.UriParser.ConvertNode nodeIn)
	public virtual Microsoft.OData.UriParser.QueryNode Visit (Microsoft.OData.UriParser.CountNode nodeIn)
	public virtual Microsoft.OData.UriParser.QueryNode Visit (Microsoft.OData.UriParser.InNode nodeIn)
	public virtual Microsoft.OData.UriParser.QueryNode Visit (Microsoft.OData.UriParser.NamedFunctionParameterNode nodeIn)
	public virtual Microsoft.OData.UriParser.QueryNode Visit (Microsoft.OData.UriParser.NonResourceRangeVariableReferenceNode nodeIn)
	public virtual Microsoft.OData.UriParser.QueryNode Visit (Microsoft.OData.UriParser.ParameterAliasNode nodeIn)
	public virtual Microsoft.OData.UriParser.QueryNode Visit (Microsoft.OData.UriParser.ResourceRangeVariableReferenceNode nodeIn)
	public virtual Microsoft.OData.UriParser.QueryNode Visit (Microsoft.OData.UriParser.SearchTermNode nodeIn)
	public virtual Microsoft.OData.UriParser.QueryNode Visit (Microsoft.OData.UriParser.SingleComplexNode nodeIn)
	public virtual Microsoft.OData.UriParser.QueryNode Visit (Microsoft.OData.UriParser.SingleNavigationNode nodeIn)
	public virtual Microsoft.OData.UriParser.QueryNode Visit (Microsoft.OData.UriParser.SingleResourceCastNode nodeIn)
	public virtual Microsoft.OData.UriParser.QueryNode Visit (Microsoft.OData.UriParser.SingleResourceFunctionCallNode nodeIn)
	public virtual Microsoft.OData.UriParser.QueryNode Visit (Microsoft.OData.UriParser.SingleValueFunctionCallNode nodeIn)
	public virtual Microsoft.OData.UriParser.QueryNode Visit (Microsoft.OData.UriParser.SingleValueOpenPropertyAccessNode nodeIn)
	public virtual Microsoft.OData.UriParser.QueryNode Visit (Microsoft.OData.UriParser.SingleValuePropertyAccessNode nodeIn)
	public virtual Microsoft.OData.UriParser.QueryNode Visit (Microsoft.OData.UriParser.UnaryOperatorNode nodeIn)
}

public class Microsoft.AspNet.OData.Query.PlainTextODataQueryOptionsParser : IODataQueryOptionsParser {
	public PlainTextODataQueryOptionsParser ()

	public virtual bool CanParse (System.Net.Http.HttpRequestMessage request)
	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task`1[[System.String]] ParseAsync (System.IO.Stream requestStream)
}

public class Microsoft.AspNet.OData.Query.QueryFilterProvider : IFilterProvider {
	public QueryFilterProvider (System.Web.Http.Filters.IActionFilter queryFilter)

	System.Web.Http.Filters.IActionFilter QueryFilter  { public get; }

	public virtual System.Collections.Generic.IEnumerable`1[[System.Web.Http.Filters.FilterInfo]] GetFilters (System.Web.Http.HttpConfiguration configuration, System.Web.Http.Controllers.HttpActionDescriptor actionDescriptor)
}

public class Microsoft.AspNet.OData.Query.SelectExpandQueryOption {
	public SelectExpandQueryOption (string select, string expand, ODataQueryContext context, Microsoft.OData.UriParser.ODataQueryOptionParser queryOptionParser)

	ODataQueryContext Context  { public get; }
	int LevelsMaxLiteralExpansionDepth  { public get; public set; }
	string RawExpand  { public get; }
	string RawSelect  { public get; }
	Microsoft.OData.UriParser.SelectExpandClause SelectExpandClause  { public get; }
	SelectExpandQueryValidator Validator  { public get; public set; }

	public System.Linq.IQueryable ApplyTo (System.Linq.IQueryable queryable, ODataQuerySettings settings)
	public object ApplyTo (object entity, ODataQuerySettings settings)
	public void Validate (ODataValidationSettings validationSettings)
}

public class Microsoft.AspNet.OData.Query.SkipQueryOption {
	public SkipQueryOption (string rawValue, ODataQueryContext context, Microsoft.OData.UriParser.ODataQueryOptionParser queryOptionParser)

	ODataQueryContext Context  { public get; }
	string RawValue  { public get; }
	SkipQueryValidator Validator  { public get; public set; }
	int Value  { public get; }

	public IQueryable`1 ApplyTo (IQueryable`1 query, ODataQuerySettings querySettings)
	public System.Linq.IQueryable ApplyTo (System.Linq.IQueryable query, ODataQuerySettings querySettings)
	public void Validate (ODataValidationSettings validationSettings)
}

public class Microsoft.AspNet.OData.Query.SkipTokenQueryOption {
	public SkipTokenQueryOption (string rawValue, ODataQueryContext context, Microsoft.OData.UriParser.ODataQueryOptionParser queryOptionParser)

	ODataQueryContext Context  { public get; }
	ODataQueryOptions QueryOptions  { public get; }
	ODataQuerySettings QuerySettings  { public get; }
	string RawValue  { public get; }
	SkipTokenQueryValidator Validator  { public get; }

	public virtual IQueryable`1 ApplyTo (IQueryable`1 query, ODataQuerySettings querySettings, ODataQueryOptions queryOptions)
	public virtual System.Linq.IQueryable ApplyTo (System.Linq.IQueryable query, ODataQuerySettings querySettings, ODataQueryOptions queryOptions)
	public void Validate (ODataValidationSettings validationSettings)
}

public class Microsoft.AspNet.OData.Query.TopQueryOption {
	public TopQueryOption (string rawValue, ODataQueryContext context, Microsoft.OData.UriParser.ODataQueryOptionParser queryOptionParser)

	ODataQueryContext Context  { public get; }
	string RawValue  { public get; }
	TopQueryValidator Validator  { public get; public set; }
	int Value  { public get; }

	public IOrderedQueryable`1 ApplyTo (IQueryable`1 query, ODataQuerySettings querySettings)
	public System.Linq.IQueryable ApplyTo (System.Linq.IQueryable query, ODataQuerySettings querySettings)
	public void Validate (ODataValidationSettings validationSettings)
}

public class Microsoft.AspNet.OData.Query.TruncatedCollection`1 : List`1, ICollection`1, IEnumerable`1, IList`1, IReadOnlyCollection`1, IReadOnlyList`1, ICollection, IEnumerable, IList, ICountOptionCollection, ITruncatedCollection {
	public TruncatedCollection`1 (IEnumerable`1 source, int pageSize)
	public TruncatedCollection`1 (IQueryable`1 source, int pageSize)
	public TruncatedCollection`1 (IEnumerable`1 source, int pageSize, System.Nullable`1[[System.Int64]] totalCount)
	public TruncatedCollection`1 (IQueryable`1 source, int pageSize, bool parameterize)
	public TruncatedCollection`1 (IQueryable`1 source, int pageSize, System.Nullable`1[[System.Int64]] totalCount)
	public TruncatedCollection`1 (IQueryable`1 source, int pageSize, System.Nullable`1[[System.Int64]] totalCount, bool parameterize)

	bool IsTruncated  { public virtual get; }
	int PageSize  { public virtual get; }
	System.Nullable`1[[System.Int64]] TotalCount  { public virtual get; }
}

[
AttributeUsageAttribute(),
]
public sealed class Microsoft.AspNet.OData.Query.CountAttribute : System.Attribute, _Attribute {
	public CountAttribute ()

	bool Disabled  { public get; public set; }
}

[
AttributeUsageAttribute(),
]
public sealed class Microsoft.AspNet.OData.Query.ExpandAttribute : System.Attribute, _Attribute {
	public ExpandAttribute ()
	public ExpandAttribute (string[] properties)

	System.Collections.Generic.Dictionary`2[[System.String],[Microsoft.AspNet.OData.Query.ExpandConfiguration]] ExpandConfigurations  { public get; }
	SelectExpandType ExpandType  { public get; public set; }
	int MaxDepth  { public get; public set; }
}

[
AttributeUsageAttribute(),
]
public sealed class Microsoft.AspNet.OData.Query.FilterAttribute : System.Attribute, _Attribute {
	public FilterAttribute ()
	public FilterAttribute (string[] properties)

	bool Disabled  { public get; public set; }
	System.Collections.Generic.Dictionary`2[[System.String],[System.Boolean]] FilterConfigurations  { public get; }
}

[
AttributeUsageAttribute(),
]
public sealed class Microsoft.AspNet.OData.Query.NonFilterableAttribute : System.Attribute, _Attribute {
	public NonFilterableAttribute ()
}

[
AttributeUsageAttribute(),
]
public sealed class Microsoft.AspNet.OData.Query.NotCountableAttribute : System.Attribute, _Attribute {
	public NotCountableAttribute ()
}

[
AttributeUsageAttribute(),
]
public sealed class Microsoft.AspNet.OData.Query.NotExpandableAttribute : System.Attribute, _Attribute {
	public NotExpandableAttribute ()
}

[
AttributeUsageAttribute(),
]
public sealed class Microsoft.AspNet.OData.Query.NotFilterableAttribute : System.Attribute, _Attribute {
	public NotFilterableAttribute ()
}

[
AttributeUsageAttribute(),
]
public sealed class Microsoft.AspNet.OData.Query.NotNavigableAttribute : System.Attribute, _Attribute {
	public NotNavigableAttribute ()
}

[
AttributeUsageAttribute(),
]
public sealed class Microsoft.AspNet.OData.Query.NotSortableAttribute : System.Attribute, _Attribute {
	public NotSortableAttribute ()
}

[
AttributeUsageAttribute(),
]
public sealed class Microsoft.AspNet.OData.Query.OrderByAttribute : System.Attribute, _Attribute {
	public OrderByAttribute ()
	public OrderByAttribute (string[] properties)

	bool Disabled  { public get; public set; }
	System.Collections.Generic.Dictionary`2[[System.String],[System.Boolean]] OrderByConfigurations  { public get; }
}

[
AttributeUsageAttribute(),
]
public sealed class Microsoft.AspNet.OData.Query.PageAttribute : System.Attribute, _Attribute {
	public PageAttribute ()

	int MaxTop  { public get; public set; }
	int PageSize  { public get; public set; }
}

[
AttributeUsageAttribute(),
]
public sealed class Microsoft.AspNet.OData.Query.SelectAttribute : System.Attribute, _Attribute {
	public SelectAttribute ()
	public SelectAttribute (string[] properties)

	System.Collections.Generic.Dictionary`2[[System.String],[Microsoft.AspNet.OData.Query.SelectExpandType]] SelectConfigurations  { public get; }
	SelectExpandType SelectType  { public get; public set; }
}

[
AttributeUsageAttribute(),
]
public sealed class Microsoft.AspNet.OData.Query.UnsortableAttribute : System.Attribute, _Attribute {
	public UnsortableAttribute ()
}

public class Microsoft.AspNet.OData.Results.CreatedODataResult`1 : IHttpActionResult {
	public CreatedODataResult`1 (T entity, System.Web.Http.ApiController controller)
	public CreatedODataResult`1 (T entity, System.Net.Http.Formatting.IContentNegotiator contentNegotiator, System.Net.Http.HttpRequestMessage request, System.Collections.Generic.IEnumerable`1[[System.Net.Http.Formatting.MediaTypeFormatter]] formatters, System.Uri locationHeader)

	System.Net.Http.Formatting.IContentNegotiator ContentNegotiator  { public get; }
	T Entity  { public get; }
	System.Collections.Generic.IEnumerable`1[[System.Net.Http.Formatting.MediaTypeFormatter]] Formatters  { public get; }
	System.Uri LocationHeader  { public get; }
	System.Net.Http.HttpRequestMessage Request  { public get; }

	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task`1[[System.Net.Http.HttpResponseMessage]] ExecuteAsync (System.Threading.CancellationToken cancellationToken)
}

public class Microsoft.AspNet.OData.Results.UpdatedODataResult`1 : IHttpActionResult {
	public UpdatedODataResult`1 (T entity, System.Web.Http.ApiController controller)
	public UpdatedODataResult`1 (T entity, System.Net.Http.Formatting.IContentNegotiator contentNegotiator, System.Net.Http.HttpRequestMessage request, System.Collections.Generic.IEnumerable`1[[System.Net.Http.Formatting.MediaTypeFormatter]] formatters)

	System.Net.Http.Formatting.IContentNegotiator ContentNegotiator  { public get; }
	T Entity  { public get; }
	System.Collections.Generic.IEnumerable`1[[System.Net.Http.Formatting.MediaTypeFormatter]] Formatters  { public get; }
	System.Net.Http.HttpRequestMessage Request  { public get; }

	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task`1[[System.Net.Http.HttpResponseMessage]] ExecuteAsync (System.Threading.CancellationToken cancellationToken)
}

public interface Microsoft.AspNet.OData.Routing.IODataPathHandler {
	Microsoft.OData.ODataUrlKeyDelimiter UrlKeyDelimiter  { public abstract get; public abstract set; }

	string Link (ODataPath path)
	ODataPath Parse (string serviceRoot, string odataPath, System.IServiceProvider requestContainer)
}

public interface Microsoft.AspNet.OData.Routing.IODataPathTemplateHandler {
	ODataPathTemplate ParseTemplate (string odataPathTemplate, System.IServiceProvider requestContainer)
}

[
ExtensionAttribute(),
]
public sealed class Microsoft.AspNet.OData.Routing.ODataParameterHelper {
	[
	ExtensionAttribute(),
	]
	public static object GetParameterValue (Microsoft.OData.UriParser.OperationImportSegment segment, string parameterName)

	[
	ExtensionAttribute(),
	]
	public static object GetParameterValue (Microsoft.OData.UriParser.OperationSegment segment, string parameterName)

	[
	ExtensionAttribute(),
	]
	public static bool TryGetParameterValue (Microsoft.OData.UriParser.OperationImportSegment segment, string parameterName, out System.Object& parameterValue)

	[
	ExtensionAttribute(),
	]
	public static bool TryGetParameterValue (Microsoft.OData.UriParser.OperationSegment segment, string parameterName, out System.Object& parameterValue)
}

public sealed class Microsoft.AspNet.OData.Routing.ODataRouteConstants {
	public static readonly string Action = "action"
	public static readonly string Batch = "$batch"
	public static readonly string ConstraintName = "ODataConstraint"
	public static readonly string Controller = "controller"
	public static readonly string DynamicProperty = "dynamicProperty"
	public static readonly string Key = "key"
	public static readonly string KeyCount = "ODataRouteKeyCount"
	public static readonly string MethodInfo = "methodInfo"
	public static readonly string NavigationProperty = "navigationProperty"
	public static readonly string ODataPath = "odataPath"
	public static readonly string ODataPathTemplate = "{*odataPath}"
	public static readonly string OptionalParameters = "Microsoft.AspNet.OData.Routing.ODataOptionalParameter"
	public static readonly string QuerySegment = "$query"
	public static readonly string RelatedKey = "relatedKey"
	public static readonly string VersionConstraintName = "ODataVersionConstraint"
}

public sealed class Microsoft.AspNet.OData.Routing.ODataSegmentKinds {
	public static string Action = "action"
	public static string Batch = "$batch"
	public static string Cast = "cast"
	public static string Count = "$count"
	public static string DynamicProperty = "dynamicproperty"
	public static string EntitySet = "entityset"
	public static string Function = "function"
	public static string Key = "key"
	public static string Metadata = "$metadata"
	public static string Navigation = "navigation"
	public static string PathTemplate = "template"
	public static string Property = "property"
	public static string Ref = "$ref"
	public static string ServiceBase = "~"
	public static string Singleton = "singleton"
	public static string UnboundAction = "unboundaction"
	public static string UnboundFunction = "unboundfunction"
	public static string Unresolved = "unresolved"
	public static string Value = "$value"
}

public class Microsoft.AspNet.OData.Routing.DefaultODataPathHandler : IODataPathHandler, IODataPathTemplateHandler {
	public DefaultODataPathHandler ()

	Microsoft.OData.ODataUrlKeyDelimiter UrlKeyDelimiter  { public virtual get; public virtual set; }

	public virtual string Link (ODataPath path)
	public virtual ODataPath Parse (string serviceRoot, string odataPath, System.IServiceProvider requestContainer)
	public virtual ODataPathTemplate ParseTemplate (string odataPathTemplate, System.IServiceProvider requestContainer)
}

public class Microsoft.AspNet.OData.Routing.DefaultODataPathValidator : Microsoft.OData.UriParser.PathSegmentHandler {
	public DefaultODataPathValidator (Microsoft.OData.Edm.IEdmModel model)

	public virtual void Handle (UnresolvedPathSegment segment)
	public virtual void Handle (Microsoft.OData.UriParser.BatchSegment segment)
	public virtual void Handle (Microsoft.OData.UriParser.CountSegment segment)
	public virtual void Handle (Microsoft.OData.UriParser.DynamicPathSegment segment)
	public virtual void Handle (Microsoft.OData.UriParser.EntitySetSegment segment)
	public virtual void Handle (Microsoft.OData.UriParser.KeySegment segment)
	public virtual void Handle (Microsoft.OData.UriParser.MetadataSegment segment)
	public virtual void Handle (Microsoft.OData.UriParser.NavigationPropertyLinkSegment segment)
	public virtual void Handle (Microsoft.OData.UriParser.NavigationPropertySegment segment)
	public virtual void Handle (Microsoft.OData.UriParser.OperationImportSegment segment)
	public virtual void Handle (Microsoft.OData.UriParser.OperationSegment segment)
	public virtual void Handle (Microsoft.OData.UriParser.PathTemplateSegment segment)
	public virtual void Handle (Microsoft.OData.UriParser.PropertySegment segment)
	public virtual void Handle (Microsoft.OData.UriParser.SingletonSegment segment)
	public virtual void Handle (Microsoft.OData.UriParser.TypeSegment segment)
	public virtual void Handle (Microsoft.OData.UriParser.ValueSegment segment)
}

public class Microsoft.AspNet.OData.Routing.ODataActionSelector : IHttpActionSelector {
	public ODataActionSelector (System.Web.Http.Controllers.IHttpActionSelector innerSelector)

	public virtual System.Linq.ILookup`2[[System.String],[System.Web.Http.Controllers.HttpActionDescriptor]] GetActionMapping (System.Web.Http.Controllers.HttpControllerDescriptor controllerDescriptor)
	public virtual System.Web.Http.Controllers.HttpActionDescriptor SelectAction (System.Web.Http.Controllers.HttpControllerContext controllerContext)
}

[
ODataPathParameterBindingAttribute(),
]
public class Microsoft.AspNet.OData.Routing.ODataPath {
	public ODataPath (Microsoft.OData.UriParser.ODataPathSegment[] segments)
	public ODataPath (System.Collections.Generic.IEnumerable`1[[Microsoft.OData.UriParser.ODataPathSegment]] segments)

	Microsoft.OData.Edm.IEdmType EdmType  { public get; }
	Microsoft.OData.Edm.IEdmNavigationSource NavigationSource  { public get; }
	Microsoft.OData.UriParser.ODataPath Path  { public get; }
	string PathTemplate  { public virtual get; }
	System.Collections.ObjectModel.ReadOnlyCollection`1[[Microsoft.OData.UriParser.ODataPathSegment]] Segments  { public get; }

	public virtual string ToString ()
}

public class Microsoft.AspNet.OData.Routing.ODataPathRouteConstraint : IHttpRouteConstraint {
	public ODataPathRouteConstraint (string routeName)

	string RouteName  { public get; }

	public virtual bool Match (System.Net.Http.HttpRequestMessage request, System.Web.Http.Routing.IHttpRoute route, string parameterName, System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] values, System.Web.Http.Routing.HttpRouteDirection routeDirection)
	protected virtual string SelectControllerName (ODataPath path, System.Net.Http.HttpRequestMessage request)
}

public class Microsoft.AspNet.OData.Routing.ODataPathSegmentHandler : Microsoft.OData.UriParser.PathSegmentHandler {
	public ODataPathSegmentHandler ()

	Microsoft.OData.Edm.IEdmNavigationSource NavigationSource  { public get; }
	string PathLiteral  { public get; }
	string PathTemplate  { public get; }

	public virtual void Handle (UnresolvedPathSegment segment)
	public virtual void Handle (Microsoft.OData.UriParser.BatchSegment segment)
	public virtual void Handle (Microsoft.OData.UriParser.CountSegment segment)
	public virtual void Handle (Microsoft.OData.UriParser.DynamicPathSegment segment)
	public virtual void Handle (Microsoft.OData.UriParser.EntitySetSegment segment)
	public virtual void Handle (Microsoft.OData.UriParser.KeySegment segment)
	public virtual void Handle (Microsoft.OData.UriParser.MetadataSegment segment)
	public virtual void Handle (Microsoft.OData.UriParser.NavigationPropertyLinkSegment segment)
	public virtual void Handle (Microsoft.OData.UriParser.NavigationPropertySegment segment)
	public virtual void Handle (Microsoft.OData.UriParser.ODataPathSegment segment)
	public virtual void Handle (Microsoft.OData.UriParser.OperationImportSegment segment)
	public virtual void Handle (Microsoft.OData.UriParser.OperationSegment segment)
	public virtual void Handle (Microsoft.OData.UriParser.PathTemplateSegment segment)
	public virtual void Handle (Microsoft.OData.UriParser.PropertySegment segment)
	public virtual void Handle (Microsoft.OData.UriParser.SingletonSegment segment)
	public virtual void Handle (Microsoft.OData.UriParser.TypeSegment segment)
	public virtual void Handle (Microsoft.OData.UriParser.ValueSegment segment)
}

public class Microsoft.AspNet.OData.Routing.ODataPathSegmentTranslator : Microsoft.OData.UriParser.PathSegmentTranslator`1[[Microsoft.OData.UriParser.ODataPathSegment]] {
	public ODataPathSegmentTranslator (Microsoft.OData.Edm.IEdmModel model, System.Collections.Generic.IDictionary`2[[System.String],[Microsoft.OData.UriParser.SingleValueNode]] parameterAliasNodes)

	public virtual Microsoft.OData.UriParser.ODataPathSegment Translate (Microsoft.OData.UriParser.BatchSegment segment)
	public virtual Microsoft.OData.UriParser.ODataPathSegment Translate (Microsoft.OData.UriParser.CountSegment segment)
	public virtual Microsoft.OData.UriParser.ODataPathSegment Translate (Microsoft.OData.UriParser.DynamicPathSegment segment)
	public virtual Microsoft.OData.UriParser.ODataPathSegment Translate (Microsoft.OData.UriParser.EntitySetSegment segment)
	public virtual Microsoft.OData.UriParser.ODataPathSegment Translate (Microsoft.OData.UriParser.KeySegment segment)
	public virtual Microsoft.OData.UriParser.ODataPathSegment Translate (Microsoft.OData.UriParser.MetadataSegment segment)
	public virtual Microsoft.OData.UriParser.ODataPathSegment Translate (Microsoft.OData.UriParser.NavigationPropertyLinkSegment segment)
	public virtual Microsoft.OData.UriParser.ODataPathSegment Translate (Microsoft.OData.UriParser.NavigationPropertySegment segment)
	public virtual Microsoft.OData.UriParser.ODataPathSegment Translate (Microsoft.OData.UriParser.OperationImportSegment segment)
	public virtual Microsoft.OData.UriParser.ODataPathSegment Translate (Microsoft.OData.UriParser.OperationSegment segment)
	public virtual Microsoft.OData.UriParser.ODataPathSegment Translate (Microsoft.OData.UriParser.PathTemplateSegment segment)
	public virtual Microsoft.OData.UriParser.ODataPathSegment Translate (Microsoft.OData.UriParser.PropertySegment segment)
	public virtual Microsoft.OData.UriParser.ODataPathSegment Translate (Microsoft.OData.UriParser.SingletonSegment segment)
	public virtual Microsoft.OData.UriParser.ODataPathSegment Translate (Microsoft.OData.UriParser.TypeSegment segment)
	public virtual Microsoft.OData.UriParser.ODataPathSegment Translate (Microsoft.OData.UriParser.ValueSegment segment)
	public static System.Collections.Generic.IEnumerable`1[[Microsoft.OData.UriParser.ODataPathSegment]] Translate (Microsoft.OData.Edm.IEdmModel model, Microsoft.OData.UriParser.ODataPath path, System.Collections.Generic.IDictionary`2[[System.String],[Microsoft.OData.UriParser.SingleValueNode]] parameterAliasNodes)
}

public class Microsoft.AspNet.OData.Routing.ODataRoute : System.Web.Http.Routing.HttpRoute, IHttpRoute {
	public ODataRoute (string routePrefix, ODataPathRouteConstraint pathConstraint)
	public ODataRoute (string routePrefix, System.Web.Http.Routing.IHttpRouteConstraint routeConstraint)
	public ODataRoute (string routePrefix, ODataPathRouteConstraint pathConstraint, System.Web.Http.Routing.HttpRouteValueDictionary defaults, System.Web.Http.Routing.HttpRouteValueDictionary constraints, System.Web.Http.Routing.HttpRouteValueDictionary dataTokens, System.Net.Http.HttpMessageHandler handler)
	public ODataRoute (string routePrefix, System.Web.Http.Routing.IHttpRouteConstraint routeConstraint, System.Web.Http.Routing.HttpRouteValueDictionary defaults, System.Web.Http.Routing.HttpRouteValueDictionary constraints, System.Web.Http.Routing.HttpRouteValueDictionary dataTokens, System.Net.Http.HttpMessageHandler handler)

	ODataPathRouteConstraint PathRouteConstraint  { public get; }
	System.Web.Http.Routing.IHttpRouteConstraint RouteConstraint  { public get; }
	string RoutePrefix  { public get; }

	public virtual System.Web.Http.Routing.IHttpVirtualPathData GetVirtualPath (System.Net.Http.HttpRequestMessage request, System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] values)
	[
	ObsoleteAttribute(),
	]
	public ODataRoute HasRelaxedODataVersionConstraint ()
}

public class Microsoft.AspNet.OData.Routing.ODataVersionConstraint : IHttpRouteConstraint {
	public ODataVersionConstraint ()

	bool IsRelaxedMatch  { public get; public set; }
	Microsoft.OData.ODataVersion Version  { public get; }

	public virtual bool Match (System.Net.Http.HttpRequestMessage request, System.Web.Http.Routing.IHttpRoute route, string parameterName, System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] values, System.Web.Http.Routing.HttpRouteDirection routeDirection)
}

public class Microsoft.AspNet.OData.Routing.UnresolvedPathSegment : Microsoft.OData.UriParser.ODataPathSegment {
	public UnresolvedPathSegment (string segmentValue)

	Microsoft.OData.Edm.IEdmType EdmType  { public virtual get; }
	string SegmentKind  { public virtual get; }
	string SegmentValue  { public get; }

	public virtual void HandleWith (Microsoft.OData.UriParser.PathSegmentHandler handler)
	public virtual string ToString ()
	public virtual T TranslateWith (PathSegmentTranslator`1 translator)
}

[
AttributeUsageAttribute(),
]
public sealed class Microsoft.AspNet.OData.Routing.ODataPathParameterBindingAttribute : System.Web.Http.ParameterBindingAttribute, _Attribute {
	public ODataPathParameterBindingAttribute ()

	public virtual System.Web.Http.Controllers.HttpParameterBinding GetBinding (System.Web.Http.Controllers.HttpParameterDescriptor parameter)
}

[
AttributeUsageAttribute(),
]
public sealed class Microsoft.AspNet.OData.Routing.ODataRouteAttribute : System.Attribute, _Attribute {
	public ODataRouteAttribute ()
	public ODataRouteAttribute (string pathTemplate)

	string PathTemplate  { public get; }
	string RouteName  { public get; public set; }
}

[
AttributeUsageAttribute(),
]
public sealed class Microsoft.AspNet.OData.Routing.ODataRoutePrefixAttribute : System.Attribute, _Attribute {
	public ODataRoutePrefixAttribute (string prefix)

	string Prefix  { public get; }
}

public enum Org.OData.Core.V1.DataModificationOperationKind : int {
	Delete = 3
	Insert = 0
	Invoke = 4
	Link = 5
	Unlink = 6
	Update = 1
	Upsert = 2
}

public abstract class Org.OData.Core.V1.ExceptionType {
	protected ExceptionType ()

	Org.OData.Core.V1.MessageType MessageType  { public get; public set; }
}

public class Org.OData.Core.V1.DataModificationExceptionType : Org.OData.Core.V1.ExceptionType {
	public DataModificationExceptionType (Org.OData.Core.V1.DataModificationOperationKind failedOperation)

	Org.OData.Core.V1.DataModificationOperationKind FailedOperation  { public get; }
	short ResponseCode  { public get; public set; }
}

public class Org.OData.Core.V1.MessageType {
	public MessageType ()

	string Code  { public get; public set; }
	string Details  { public get; public set; }
	string Message  { public get; public set; }
	string Severity  { public get; public set; }
	string Target  { public get; public set; }
}

public abstract class Microsoft.AspNet.OData.Formatter.Deserialization.ODataDeserializer {
	protected ODataDeserializer (Microsoft.OData.ODataPayloadKind payloadKind)

	Microsoft.OData.ODataPayloadKind ODataPayloadKind  { public get; }

	public virtual object Read (Microsoft.OData.ODataMessageReader messageReader, System.Type type, ODataDeserializerContext readContext)
	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task`1[[System.Object]] ReadAsync (Microsoft.OData.ODataMessageReader messageReader, System.Type type, ODataDeserializerContext readContext)
}

public abstract class Microsoft.AspNet.OData.Formatter.Deserialization.ODataDeserializerProvider {
	protected ODataDeserializerProvider ()

	public abstract ODataEdmTypeDeserializer GetEdmTypeDeserializer (Microsoft.OData.Edm.IEdmTypeReference edmType)
	public abstract ODataDeserializer GetODataDeserializer (System.Type type, System.Net.Http.HttpRequestMessage request)
}

public abstract class Microsoft.AspNet.OData.Formatter.Deserialization.ODataEdmTypeDeserializer : ODataDeserializer {
	protected ODataEdmTypeDeserializer (Microsoft.OData.ODataPayloadKind payloadKind)
	protected ODataEdmTypeDeserializer (Microsoft.OData.ODataPayloadKind payloadKind, ODataDeserializerProvider deserializerProvider)

	ODataDeserializerProvider DeserializerProvider  { public get; }

	public virtual object ReadInline (object item, Microsoft.OData.Edm.IEdmTypeReference edmType, ODataDeserializerContext readContext)
}

public abstract class Microsoft.AspNet.OData.Formatter.Deserialization.ODataItemBase {
	protected ODataItemBase (Microsoft.OData.ODataItem item)

	Microsoft.OData.ODataItem Item  { public get; }
}

public abstract class Microsoft.AspNet.OData.Formatter.Deserialization.ODataResourceSetWrapperBase : ODataItemBase {
	public ODataResourceSetWrapperBase (Microsoft.OData.ODataResourceSetBase item)

	System.Collections.Generic.IList`1[[Microsoft.AspNet.OData.Formatter.Deserialization.ODataResourceWrapper]] Resources  { public get; }
	Microsoft.OData.ODataResourceSetBase ResourceSetBase  { public get; }
}

[
ExtensionAttribute(),
]
public sealed class Microsoft.AspNet.OData.Formatter.Deserialization.ODataReaderExtensions {
	[
	ExtensionAttribute(),
	]
	public static ODataItemBase ReadResourceOrResourceSet (Microsoft.OData.ODataReader reader)

	[
	AsyncStateMachineAttribute(),
	ExtensionAttribute(),
	]
	public static System.Threading.Tasks.Task`1[[Microsoft.AspNet.OData.Formatter.Deserialization.ODataItemBase]] ReadResourceOrResourceSetAsync (Microsoft.OData.ODataReader reader)
}

public class Microsoft.AspNet.OData.Formatter.Deserialization.DefaultODataDeserializerProvider : ODataDeserializerProvider {
	public DefaultODataDeserializerProvider (System.IServiceProvider rootContainer)

	public virtual ODataEdmTypeDeserializer GetEdmTypeDeserializer (Microsoft.OData.Edm.IEdmTypeReference edmType)
	public virtual ODataDeserializer GetODataDeserializer (System.Type type, System.Net.Http.HttpRequestMessage request)
}

public class Microsoft.AspNet.OData.Formatter.Deserialization.ODataActionPayloadDeserializer : ODataDeserializer {
	public ODataActionPayloadDeserializer (ODataDeserializerProvider deserializerProvider)

	ODataDeserializerProvider DeserializerProvider  { public get; }

	public virtual object Read (Microsoft.OData.ODataMessageReader messageReader, System.Type type, ODataDeserializerContext readContext)
	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task`1[[System.Object]] ReadAsync (Microsoft.OData.ODataMessageReader messageReader, System.Type type, ODataDeserializerContext readContext)
}

public class Microsoft.AspNet.OData.Formatter.Deserialization.ODataCollectionDeserializer : ODataEdmTypeDeserializer {
	public ODataCollectionDeserializer (ODataDeserializerProvider deserializerProvider)

	public virtual object Read (Microsoft.OData.ODataMessageReader messageReader, System.Type type, ODataDeserializerContext readContext)
	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task`1[[System.Object]] ReadAsync (Microsoft.OData.ODataMessageReader messageReader, System.Type type, ODataDeserializerContext readContext)

	public virtual System.Collections.IEnumerable ReadCollectionValue (Microsoft.OData.ODataCollectionValue collectionValue, Microsoft.OData.Edm.IEdmTypeReference elementType, ODataDeserializerContext readContext)
	public virtual object ReadInline (object item, Microsoft.OData.Edm.IEdmTypeReference edmType, ODataDeserializerContext readContext)
}

public class Microsoft.AspNet.OData.Formatter.Deserialization.ODataDeserializerContext {
	public ODataDeserializerContext ()

	Microsoft.OData.Edm.IEdmModel Model  { public get; public set; }
	ODataPath Path  { public get; public set; }
	System.Net.Http.HttpRequestMessage Request  { public get; public set; }
	System.Web.Http.Controllers.HttpRequestContext RequestContext  { public get; public set; }
	Microsoft.OData.Edm.IEdmTypeReference ResourceEdmType  { public get; public set; }
	System.Type ResourceType  { public get; public set; }
}

public class Microsoft.AspNet.OData.Formatter.Deserialization.ODataEntityReferenceLinkBase : ODataItemBase {
	public ODataEntityReferenceLinkBase (Microsoft.OData.ODataEntityReferenceLink item)

	Microsoft.OData.ODataEntityReferenceLink EntityReferenceLink  { public get; }
}

public class Microsoft.AspNet.OData.Formatter.Deserialization.ODataEntityReferenceLinkDeserializer : ODataDeserializer {
	public ODataEntityReferenceLinkDeserializer ()

	public virtual object Read (Microsoft.OData.ODataMessageReader messageReader, System.Type type, ODataDeserializerContext readContext)
	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task`1[[System.Object]] ReadAsync (Microsoft.OData.ODataMessageReader messageReader, System.Type type, ODataDeserializerContext readContext)
}

public class Microsoft.AspNet.OData.Formatter.Deserialization.ODataEnumDeserializer : ODataEdmTypeDeserializer {
	public ODataEnumDeserializer ()

	public virtual object Read (Microsoft.OData.ODataMessageReader messageReader, System.Type type, ODataDeserializerContext readContext)
	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task`1[[System.Object]] ReadAsync (Microsoft.OData.ODataMessageReader messageReader, System.Type type, ODataDeserializerContext readContext)

	public virtual object ReadInline (object item, Microsoft.OData.Edm.IEdmTypeReference edmType, ODataDeserializerContext readContext)
}

public class Microsoft.AspNet.OData.Formatter.Deserialization.ODataPrimitiveDeserializer : ODataEdmTypeDeserializer {
	public ODataPrimitiveDeserializer ()

	public virtual object Read (Microsoft.OData.ODataMessageReader messageReader, System.Type type, ODataDeserializerContext readContext)
	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task`1[[System.Object]] ReadAsync (Microsoft.OData.ODataMessageReader messageReader, System.Type type, ODataDeserializerContext readContext)

	public virtual object ReadInline (object item, Microsoft.OData.Edm.IEdmTypeReference edmType, ODataDeserializerContext readContext)
	public virtual object ReadPrimitive (Microsoft.OData.ODataProperty primitiveProperty, ODataDeserializerContext readContext)
}

public class Microsoft.AspNet.OData.Formatter.Deserialization.ODataResourceDeserializer : ODataEdmTypeDeserializer {
	public ODataResourceDeserializer (ODataDeserializerProvider deserializerProvider)

	public virtual void ApplyInstanceAnnotations (object resource, ODataResourceWrapper resourceWrapper, Microsoft.OData.Edm.IEdmStructuredTypeReference structuredType, ODataDeserializerContext readContext)
	public virtual void ApplyNestedProperties (object resource, ODataResourceWrapper resourceWrapper, Microsoft.OData.Edm.IEdmStructuredTypeReference structuredType, ODataDeserializerContext readContext)
	public virtual void ApplyNestedProperty (object resource, ODataNestedResourceInfoWrapper resourceInfoWrapper, Microsoft.OData.Edm.IEdmStructuredTypeReference structuredType, ODataDeserializerContext readContext)
	public virtual void ApplyStructuralProperties (object resource, ODataResourceWrapper resourceWrapper, Microsoft.OData.Edm.IEdmStructuredTypeReference structuredType, ODataDeserializerContext readContext)
	public virtual void ApplyStructuralProperty (object resource, Microsoft.OData.ODataProperty structuralProperty, Microsoft.OData.Edm.IEdmStructuredTypeReference structuredType, ODataDeserializerContext readContext)
	public virtual object CreateResourceInstance (Microsoft.OData.Edm.IEdmStructuredTypeReference structuredType, ODataDeserializerContext readContext)
	public virtual object Read (Microsoft.OData.ODataMessageReader messageReader, System.Type type, ODataDeserializerContext readContext)
	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task`1[[System.Object]] ReadAsync (Microsoft.OData.ODataMessageReader messageReader, System.Type type, ODataDeserializerContext readContext)

	public virtual object ReadInline (object item, Microsoft.OData.Edm.IEdmTypeReference edmType, ODataDeserializerContext readContext)
	public virtual object ReadResource (ODataResourceWrapper resourceWrapper, Microsoft.OData.Edm.IEdmStructuredTypeReference structuredType, ODataDeserializerContext readContext)
}

public class Microsoft.AspNet.OData.Formatter.Deserialization.ODataResourceSetDeserializer : ODataEdmTypeDeserializer {
	public ODataResourceSetDeserializer (ODataDeserializerProvider deserializerProvider)

	public virtual object Read (Microsoft.OData.ODataMessageReader messageReader, System.Type type, ODataDeserializerContext readContext)
	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task`1[[System.Object]] ReadAsync (Microsoft.OData.ODataMessageReader messageReader, System.Type type, ODataDeserializerContext readContext)

	public virtual object ReadInline (object item, Microsoft.OData.Edm.IEdmTypeReference edmType, ODataDeserializerContext readContext)
	public virtual System.Collections.IEnumerable ReadResourceSet (ODataResourceSetWrapperBase resourceSet, Microsoft.OData.Edm.IEdmStructuredTypeReference elementType, ODataDeserializerContext readContext)
}

public sealed class Microsoft.AspNet.OData.Formatter.Deserialization.ODataDeltaResourceSetWrapper : ODataResourceSetWrapperBase {
	public ODataDeltaResourceSetWrapper (Microsoft.OData.ODataDeltaResourceSet item)
}

public sealed class Microsoft.AspNet.OData.Formatter.Deserialization.ODataNestedResourceInfoWrapper : ODataItemBase {
	public ODataNestedResourceInfoWrapper (Microsoft.OData.ODataNestedResourceInfo item)

	System.Collections.Generic.IList`1[[Microsoft.AspNet.OData.Formatter.Deserialization.ODataItemBase]] NestedItems  { public get; }
	Microsoft.OData.ODataNestedResourceInfo NestedResourceInfo  { public get; }
}

public sealed class Microsoft.AspNet.OData.Formatter.Deserialization.ODataResourceSetWrapper : ODataResourceSetWrapperBase {
	public ODataResourceSetWrapper (Microsoft.OData.ODataResourceSet item)

	Microsoft.OData.ODataResourceSet ResourceSet  { public get; }
}

public sealed class Microsoft.AspNet.OData.Formatter.Deserialization.ODataResourceWrapper : ODataItemBase {
	public ODataResourceWrapper (Microsoft.OData.ODataResourceBase item)

	System.Collections.Generic.IList`1[[Microsoft.AspNet.OData.Formatter.Deserialization.ODataNestedResourceInfoWrapper]] NestedResourceInfos  { public get; }
	[
	ObsoleteAttribute(),
	]
	Microsoft.OData.ODataResource Resource  { public get; }

	Microsoft.OData.ODataResourceBase ResourceBase  { public get; }
}

public abstract class Microsoft.AspNet.OData.Formatter.Serialization.ODataEdmTypeSerializer : ODataSerializer {
	protected ODataEdmTypeSerializer (Microsoft.OData.ODataPayloadKind payloadKind)
	protected ODataEdmTypeSerializer (Microsoft.OData.ODataPayloadKind payloadKind, ODataSerializerProvider serializerProvider)

	ODataSerializerProvider SerializerProvider  { public get; }

	public virtual Microsoft.OData.ODataValue CreateODataValue (object graph, Microsoft.OData.Edm.IEdmTypeReference expectedType, ODataSerializerContext writeContext)
	internal virtual Microsoft.OData.ODataProperty CreateProperty (object graph, Microsoft.OData.Edm.IEdmTypeReference expectedType, string elementName, ODataSerializerContext writeContext)
	public virtual void WriteObjectInline (object graph, Microsoft.OData.Edm.IEdmTypeReference expectedType, Microsoft.OData.ODataWriter writer, ODataSerializerContext writeContext)
	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task WriteObjectInlineAsync (object graph, Microsoft.OData.Edm.IEdmTypeReference expectedType, Microsoft.OData.ODataWriter writer, ODataSerializerContext writeContext)
}

public abstract class Microsoft.AspNet.OData.Formatter.Serialization.ODataSerializer {
	protected ODataSerializer (Microsoft.OData.ODataPayloadKind payloadKind)

	Microsoft.OData.ODataPayloadKind ODataPayloadKind  { public get; }

	public virtual void WriteObject (object graph, System.Type type, Microsoft.OData.ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
	public virtual System.Threading.Tasks.Task WriteObjectAsync (object graph, System.Type type, Microsoft.OData.ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
}

public abstract class Microsoft.AspNet.OData.Formatter.Serialization.ODataSerializerProvider {
	protected ODataSerializerProvider ()

	public abstract ODataEdmTypeSerializer GetEdmTypeSerializer (Microsoft.OData.Edm.IEdmTypeReference edmType)
	public abstract ODataSerializer GetODataPayloadSerializer (System.Type type, System.Net.Http.HttpRequestMessage request)
}

public class Microsoft.AspNet.OData.Formatter.Serialization.DefaultODataSerializerProvider : ODataSerializerProvider {
	public DefaultODataSerializerProvider (System.IServiceProvider rootContainer)

	public virtual ODataEdmTypeSerializer GetEdmTypeSerializer (Microsoft.OData.Edm.IEdmTypeReference edmType)
	public virtual ODataSerializer GetODataPayloadSerializer (System.Type type, System.Net.Http.HttpRequestMessage request)
}

public class Microsoft.AspNet.OData.Formatter.Serialization.EntitySelfLinks {
	public EntitySelfLinks ()

	System.Uri EditLink  { public get; public set; }
	System.Uri IdLink  { public get; public set; }
	System.Uri ReadLink  { public get; public set; }
}

public class Microsoft.AspNet.OData.Formatter.Serialization.ODataCollectionSerializer : ODataEdmTypeSerializer {
	public ODataCollectionSerializer (ODataSerializerProvider serializerProvider)
	public ODataCollectionSerializer (ODataSerializerProvider serializerProvider, bool isForAnnotations)

	protected static void AddTypeNameAnnotationAsNeeded (Microsoft.OData.ODataCollectionValue value, ODataMetadataLevel metadataLevel)
	public virtual Microsoft.OData.ODataCollectionValue CreateODataCollectionValue (System.Collections.IEnumerable enumerable, Microsoft.OData.Edm.IEdmTypeReference elementType, ODataSerializerContext writeContext)
	public virtual Microsoft.OData.ODataValue CreateODataValue (object graph, Microsoft.OData.Edm.IEdmTypeReference expectedType, ODataSerializerContext writeContext)
	internal virtual Microsoft.OData.ODataProperty CreateProperty (object graph, Microsoft.OData.Edm.IEdmTypeReference expectedType, string elementName, ODataSerializerContext writeContext)
	public void WriteCollection (Microsoft.OData.ODataCollectionWriter writer, object graph, Microsoft.OData.Edm.IEdmTypeReference collectionType, ODataSerializerContext writeContext)
	[
	AsyncStateMachineAttribute(),
	]
	public System.Threading.Tasks.Task WriteCollectionAsync (Microsoft.OData.ODataCollectionWriter writer, object graph, Microsoft.OData.Edm.IEdmTypeReference collectionType, ODataSerializerContext writeContext)

	public virtual void WriteObject (object graph, System.Type type, Microsoft.OData.ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task WriteObjectAsync (object graph, System.Type type, Microsoft.OData.ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
}

public class Microsoft.AspNet.OData.Formatter.Serialization.ODataDeltaFeedSerializer : ODataEdmTypeSerializer {
	public ODataDeltaFeedSerializer (ODataSerializerProvider serializerProvider)

	public virtual Microsoft.OData.ODataDeltaResourceSet CreateODataDeltaFeed (System.Collections.IEnumerable feedInstance, Microsoft.OData.Edm.IEdmCollectionTypeReference feedType, ODataSerializerContext writeContext)
	public virtual void WriteDeltaDeletedEntry (object graph, Microsoft.OData.ODataWriter writer, ODataSerializerContext writeContext)
	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task WriteDeltaDeletedEntryAsync (object graph, Microsoft.OData.ODataWriter writer, ODataSerializerContext writeContext)

	public virtual void WriteDeltaDeletedLink (object graph, Microsoft.OData.ODataWriter writer, ODataSerializerContext writeContext)
	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task WriteDeltaDeletedLinkAsync (object graph, Microsoft.OData.ODataWriter writer, ODataSerializerContext writeContext)

	public virtual void WriteDeltaFeedInline (object graph, Microsoft.OData.Edm.IEdmTypeReference expectedType, Microsoft.OData.ODataWriter writer, ODataSerializerContext writeContext)
	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task WriteDeltaFeedInlineAsync (object graph, Microsoft.OData.Edm.IEdmTypeReference expectedType, Microsoft.OData.ODataWriter writer, ODataSerializerContext writeContext)

	public virtual void WriteDeltaLink (object graph, Microsoft.OData.ODataWriter writer, ODataSerializerContext writeContext)
	[
	AsyncStateMachineAttribute(),
	]
	public System.Threading.Tasks.Task WriteDeltaLinkAsync (object graph, Microsoft.OData.ODataWriter writer, ODataSerializerContext writeContext)

	public virtual void WriteObject (object graph, System.Type type, Microsoft.OData.ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task WriteObjectAsync (object graph, System.Type type, Microsoft.OData.ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
}

public class Microsoft.AspNet.OData.Formatter.Serialization.ODataEntityReferenceLinkSerializer : ODataSerializer {
	public ODataEntityReferenceLinkSerializer ()

	public virtual void WriteObject (object graph, System.Type type, Microsoft.OData.ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
	public virtual System.Threading.Tasks.Task WriteObjectAsync (object graph, System.Type type, Microsoft.OData.ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
}

public class Microsoft.AspNet.OData.Formatter.Serialization.ODataEntityReferenceLinksSerializer : ODataSerializer {
	public ODataEntityReferenceLinksSerializer ()

	public virtual void WriteObject (object graph, System.Type type, Microsoft.OData.ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
	public virtual System.Threading.Tasks.Task WriteObjectAsync (object graph, System.Type type, Microsoft.OData.ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
}

public class Microsoft.AspNet.OData.Formatter.Serialization.ODataEnumSerializer : ODataEdmTypeSerializer {
	public ODataEnumSerializer (ODataSerializerProvider serializerProvider)

	public virtual Microsoft.OData.ODataEnumValue CreateODataEnumValue (object graph, Microsoft.OData.Edm.IEdmEnumTypeReference enumType, ODataSerializerContext writeContext)
	public virtual Microsoft.OData.ODataValue CreateODataValue (object graph, Microsoft.OData.Edm.IEdmTypeReference expectedType, ODataSerializerContext writeContext)
	public virtual void WriteObject (object graph, System.Type type, Microsoft.OData.ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
	public virtual System.Threading.Tasks.Task WriteObjectAsync (object graph, System.Type type, Microsoft.OData.ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
}

public class Microsoft.AspNet.OData.Formatter.Serialization.ODataErrorSerializer : ODataSerializer {
	public ODataErrorSerializer ()

	public virtual void WriteObject (object graph, System.Type type, Microsoft.OData.ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
	public virtual System.Threading.Tasks.Task WriteObjectAsync (object graph, System.Type type, Microsoft.OData.ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
}

public class Microsoft.AspNet.OData.Formatter.Serialization.ODataMetadataSerializer : ODataSerializer {
	public ODataMetadataSerializer ()

	public virtual void WriteObject (object graph, System.Type type, Microsoft.OData.ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
	public virtual System.Threading.Tasks.Task WriteObjectAsync (object graph, System.Type type, Microsoft.OData.ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
}

public class Microsoft.AspNet.OData.Formatter.Serialization.ODataPrimitiveSerializer : ODataEdmTypeSerializer {
	public ODataPrimitiveSerializer ()

	public virtual Microsoft.OData.ODataPrimitiveValue CreateODataPrimitiveValue (object graph, Microsoft.OData.Edm.IEdmPrimitiveTypeReference primitiveType, ODataSerializerContext writeContext)
	public virtual Microsoft.OData.ODataValue CreateODataValue (object graph, Microsoft.OData.Edm.IEdmTypeReference expectedType, ODataSerializerContext writeContext)
	public virtual void WriteObject (object graph, System.Type type, Microsoft.OData.ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
	public virtual System.Threading.Tasks.Task WriteObjectAsync (object graph, System.Type type, Microsoft.OData.ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
}

public class Microsoft.AspNet.OData.Formatter.Serialization.ODataRawValueSerializer : ODataSerializer {
	public ODataRawValueSerializer ()

	public virtual void WriteObject (object graph, System.Type type, Microsoft.OData.ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
	public virtual System.Threading.Tasks.Task WriteObjectAsync (object graph, System.Type type, Microsoft.OData.ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
}

public class Microsoft.AspNet.OData.Formatter.Serialization.ODataResourceSerializer : ODataEdmTypeSerializer {
	public ODataResourceSerializer (ODataSerializerProvider serializerProvider)

	public virtual void AppendDynamicProperties (Microsoft.OData.ODataResourceBase resource, SelectExpandNode selectExpandNode, ResourceContext resourceContext)
	public virtual void AppendInstanceAnnotations (Microsoft.OData.ODataResourceBase resource, ResourceContext resourceContext)
	public virtual Microsoft.OData.ODataDeletedResource CreateDeletedResource (SelectExpandNode selectExpandNode, ResourceContext resourceContext)
	public virtual string CreateETag (ResourceContext resourceContext)
	public virtual Microsoft.OData.ODataNestedResourceInfo CreateNavigationLink (Microsoft.OData.Edm.IEdmNavigationProperty navigationProperty, ResourceContext resourceContext)
	public virtual Microsoft.OData.ODataAction CreateODataAction (Microsoft.OData.Edm.IEdmAction action, ResourceContext resourceContext)
	public virtual Microsoft.OData.ODataFunction CreateODataFunction (Microsoft.OData.Edm.IEdmFunction function, ResourceContext resourceContext)
	public virtual Microsoft.OData.ODataResource CreateResource (SelectExpandNode selectExpandNode, ResourceContext resourceContext)
	public virtual SelectExpandNode CreateSelectExpandNode (ResourceContext resourceContext)
	internal virtual Microsoft.OData.ODataStreamPropertyInfo CreateStreamProperty (Microsoft.OData.Edm.IEdmStructuralProperty structuralProperty, ResourceContext resourceContext)
	public virtual Microsoft.OData.ODataProperty CreateStructuralProperty (Microsoft.OData.Edm.IEdmStructuralProperty structuralProperty, ResourceContext resourceContext)
	public virtual void WriteDeltaObjectInline (object graph, Microsoft.OData.Edm.IEdmTypeReference expectedType, Microsoft.OData.ODataWriter writer, ODataSerializerContext writeContext)
	public virtual System.Threading.Tasks.Task WriteDeltaObjectInlineAsync (object graph, Microsoft.OData.Edm.IEdmTypeReference expectedType, Microsoft.OData.ODataWriter writer, ODataSerializerContext writeContext)
	public virtual void WriteObject (object graph, System.Type type, Microsoft.OData.ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task WriteObjectAsync (object graph, System.Type type, Microsoft.OData.ODataMessageWriter messageWriter, ODataSerializerContext writeContext)

	public virtual void WriteObjectInline (object graph, Microsoft.OData.Edm.IEdmTypeReference expectedType, Microsoft.OData.ODataWriter writer, ODataSerializerContext writeContext)
	public virtual System.Threading.Tasks.Task WriteObjectInlineAsync (object graph, Microsoft.OData.Edm.IEdmTypeReference expectedType, Microsoft.OData.ODataWriter writer, ODataSerializerContext writeContext)
}

public class Microsoft.AspNet.OData.Formatter.Serialization.ODataResourceSetSerializer : ODataEdmTypeSerializer {
	public ODataResourceSetSerializer (ODataSerializerProvider serializerProvider)

	public virtual Microsoft.OData.ODataOperation CreateODataOperation (Microsoft.OData.Edm.IEdmOperation operation, ResourceSetContext resourceSetContext, ODataSerializerContext writeContext)
	public virtual Microsoft.OData.ODataResourceSet CreateResourceSet (System.Collections.IEnumerable resourceSetInstance, Microsoft.OData.Edm.IEdmCollectionTypeReference resourceSetType, ODataSerializerContext writeContext)
	public virtual void WriteObject (object graph, System.Type type, Microsoft.OData.ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task WriteObjectAsync (object graph, System.Type type, Microsoft.OData.ODataMessageWriter messageWriter, ODataSerializerContext writeContext)

	public virtual void WriteObjectInline (object graph, Microsoft.OData.Edm.IEdmTypeReference expectedType, Microsoft.OData.ODataWriter writer, ODataSerializerContext writeContext)
	public virtual System.Threading.Tasks.Task WriteObjectInlineAsync (object graph, Microsoft.OData.Edm.IEdmTypeReference expectedType, Microsoft.OData.ODataWriter writer, ODataSerializerContext writeContext)
}

public class Microsoft.AspNet.OData.Formatter.Serialization.ODataResourceValueSerializer : ODataEdmTypeSerializer {
	public ODataResourceValueSerializer (ODataSerializerProvider serializerProvider)
	protected ODataResourceValueSerializer (Microsoft.OData.ODataPayloadKind payloadKind, ODataSerializerProvider serializerProvider)

	public virtual Microsoft.OData.ODataValue CreateODataValue (object graph, Microsoft.OData.Edm.IEdmTypeReference expectedType, ODataSerializerContext writeContext)
	public virtual void WriteObject (object graph, System.Type type, Microsoft.OData.ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
	public virtual System.Threading.Tasks.Task WriteObjectAsync (object graph, System.Type type, Microsoft.OData.ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
}

public class Microsoft.AspNet.OData.Formatter.Serialization.ODataSerializerContext {
	public ODataSerializerContext ()
	public ODataSerializerContext (ResourceContext resource, Microsoft.OData.UriParser.SelectExpandClause selectExpandClause, Microsoft.OData.Edm.IEdmProperty edmProperty)

	Microsoft.OData.Edm.IEdmProperty EdmProperty  { public get; public set; }
	ResourceContext ExpandedResource  { public get; public set; }
	bool ExpandReference  { public get; public set; }
	System.Collections.Generic.IDictionary`2[[System.Object],[System.Object]] Items  { public get; }
	ODataMetadataLevel MetadataLevel  { public get; public set; }
	Microsoft.OData.Edm.IEdmModel Model  { public get; public set; }
	Microsoft.OData.Edm.IEdmNavigationProperty NavigationProperty  { public get; }
	Microsoft.OData.Edm.IEdmNavigationSource NavigationSource  { public get; public set; }
	ODataPath Path  { public get; public set; }
	ODataQueryOptions QueryOptions  { public get; }
	System.Net.Http.HttpRequestMessage Request  { public get; public set; }
	System.Web.Http.Controllers.HttpRequestContext RequestContext  { public get; public set; }
	string RootElementName  { public get; public set; }
	Microsoft.OData.UriParser.SelectExpandClause SelectExpandClause  { public get; public set; }
	bool SkipExpensiveAvailabilityChecks  { public get; public set; }
	System.Web.Http.Routing.UrlHelper Url  { public get; public set; }
}

public class Microsoft.AspNet.OData.Formatter.Serialization.ODataServiceDocumentSerializer : ODataSerializer {
	public ODataServiceDocumentSerializer ()

	public virtual void WriteObject (object graph, System.Type type, Microsoft.OData.ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
	public virtual System.Threading.Tasks.Task WriteObjectAsync (object graph, System.Type type, Microsoft.OData.ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
}

public class Microsoft.AspNet.OData.Formatter.Serialization.SelectExpandNode {
	public SelectExpandNode ()
	public SelectExpandNode (SelectExpandNode selectExpandNodeToCopy)
	public SelectExpandNode (Microsoft.OData.Edm.IEdmStructuredType structuredType, ODataSerializerContext writeContext)
	public SelectExpandNode (Microsoft.OData.UriParser.SelectExpandClause selectExpandClause, Microsoft.OData.Edm.IEdmStructuredType structuredType, Microsoft.OData.Edm.IEdmModel model)

	System.Collections.Generic.IDictionary`2[[Microsoft.OData.Edm.IEdmNavigationProperty],[Microsoft.OData.UriParser.ExpandedNavigationSelectItem]] ExpandedProperties  { public get; }
	[
	ObsoleteAttribute(),
	]
	System.Collections.Generic.ISet`1[[Microsoft.OData.Edm.IEdmNavigationProperty]] ReferencedNavigationProperties  { public get; }

	System.Collections.Generic.IDictionary`2[[Microsoft.OData.Edm.IEdmNavigationProperty],[Microsoft.OData.UriParser.ExpandedReferenceSelectItem]] ReferencedProperties  { public get; }
	bool SelectAllDynamicProperties  { public get; }
	System.Collections.Generic.ISet`1[[Microsoft.OData.Edm.IEdmAction]] SelectedActions  { public get; }
	[
	ObsoleteAttribute(),
	]
	System.Collections.Generic.ISet`1[[Microsoft.OData.Edm.IEdmStructuralProperty]] SelectedComplexProperties  { public get; }

	System.Collections.Generic.IDictionary`2[[Microsoft.OData.Edm.IEdmStructuralProperty],[Microsoft.OData.UriParser.PathSelectItem]] SelectedComplexTypeProperties  { public get; }
	System.Collections.Generic.ISet`1[[System.String]] SelectedDynamicProperties  { public get; }
	System.Collections.Generic.ISet`1[[Microsoft.OData.Edm.IEdmFunction]] SelectedFunctions  { public get; }
	System.Collections.Generic.ISet`1[[Microsoft.OData.Edm.IEdmNavigationProperty]] SelectedNavigationProperties  { public get; }
	System.Collections.Generic.ISet`1[[Microsoft.OData.Edm.IEdmStructuralProperty]] SelectedStructuralProperties  { public get; }

	[
	ObsoleteAttribute(),
	]
	public static void GetStructuralProperties (Microsoft.OData.Edm.IEdmStructuredType structuredType, System.Collections.Generic.HashSet`1[[Microsoft.OData.Edm.IEdmStructuralProperty]] structuralProperties, System.Collections.Generic.HashSet`1[[Microsoft.OData.Edm.IEdmStructuralProperty]] nestedStructuralProperties)
}

public abstract class Microsoft.AspNet.OData.Query.Expressions.DynamicTypeWrapper {
	protected DynamicTypeWrapper ()

	System.Collections.Generic.Dictionary`2[[System.String],[System.Object]] Values  { public abstract get; }

	public virtual bool TryGetPropertyValue (string propertyName, out System.Object& value)
}

public abstract class Microsoft.AspNet.OData.Query.Expressions.ExpressionBinderBase {
	protected ExpressionBinderBase (System.IServiceProvider requestContainer)

	System.Linq.Expressions.ParameterExpression Parameter  { protected abstract get; }

	public abstract System.Linq.Expressions.Expression Bind (Microsoft.OData.UriParser.QueryNode node)
	protected System.Linq.Expressions.Expression[] BindArguments (System.Collections.Generic.IEnumerable`1[[Microsoft.OData.UriParser.QueryNode]] nodes)
	public virtual System.Linq.Expressions.Expression BindCollectionConstantNode (Microsoft.OData.UriParser.CollectionConstantNode node)
	public virtual System.Linq.Expressions.Expression BindConstantNode (Microsoft.OData.UriParser.ConstantNode constantNode)
	public virtual System.Linq.Expressions.Expression BindSingleValueFunctionCallNode (Microsoft.OData.UriParser.SingleValueFunctionCallNode node)
	protected void EnsureFlattenedPropertyContainer (System.Linq.Expressions.ParameterExpression source)
	protected System.Reflection.PropertyInfo GetDynamicPropertyContainer (Microsoft.OData.UriParser.SingleValueOpenPropertyAccessNode openNode)
	protected System.Linq.Expressions.Expression GetFlattenedPropertyExpression (string propertyPath)
}

public class Microsoft.AspNet.OData.Query.Expressions.FilterBinder : ExpressionBinderBase {
	public FilterBinder (System.IServiceProvider requestContainer)

	System.Linq.Expressions.ParameterExpression Parameter  { protected virtual get; }

	public virtual System.Linq.Expressions.Expression Bind (Microsoft.OData.UriParser.QueryNode node)
	public virtual System.Linq.Expressions.Expression BindAllNode (Microsoft.OData.UriParser.AllNode allNode)
	public virtual System.Linq.Expressions.Expression BindAnyNode (Microsoft.OData.UriParser.AnyNode anyNode)
	public virtual System.Linq.Expressions.Expression BindBinaryOperatorNode (Microsoft.OData.UriParser.BinaryOperatorNode binaryOperatorNode)
	public virtual System.Linq.Expressions.Expression BindCollectionComplexNode (Microsoft.OData.UriParser.CollectionComplexNode collectionComplexNode)
	public virtual System.Linq.Expressions.Expression BindCollectionPropertyAccessNode (Microsoft.OData.UriParser.CollectionPropertyAccessNode propertyAccessNode)
	public virtual System.Linq.Expressions.Expression BindCollectionResourceCastNode (Microsoft.OData.UriParser.CollectionResourceCastNode node)
	public virtual System.Linq.Expressions.Expression BindConvertNode (Microsoft.OData.UriParser.ConvertNode convertNode)
	public virtual System.Linq.Expressions.Expression BindDynamicPropertyAccessQueryNode (Microsoft.OData.UriParser.SingleValueOpenPropertyAccessNode openNode)
	public virtual System.Linq.Expressions.Expression BindInNode (Microsoft.OData.UriParser.InNode inNode)
	public virtual System.Linq.Expressions.Expression BindNavigationPropertyNode (Microsoft.OData.UriParser.QueryNode sourceNode, Microsoft.OData.Edm.IEdmNavigationProperty navigationProperty)
	public virtual System.Linq.Expressions.Expression BindNavigationPropertyNode (Microsoft.OData.UriParser.QueryNode sourceNode, Microsoft.OData.Edm.IEdmNavigationProperty navigationProperty, string propertyPath)
	public virtual System.Linq.Expressions.Expression BindPropertyAccessQueryNode (Microsoft.OData.UriParser.SingleValuePropertyAccessNode propertyAccessNode)
	public virtual System.Linq.Expressions.Expression BindRangeVariable (Microsoft.OData.UriParser.RangeVariable rangeVariable)
	public virtual System.Linq.Expressions.Expression BindSingleComplexNode (Microsoft.OData.UriParser.SingleComplexNode singleComplexNode)
	public virtual System.Linq.Expressions.Expression BindSingleResourceCastNode (Microsoft.OData.UriParser.SingleResourceCastNode node)
	public virtual System.Linq.Expressions.Expression BindSingleResourceFunctionCallNode (Microsoft.OData.UriParser.SingleResourceFunctionCallNode node)
	public virtual System.Linq.Expressions.Expression BindUnaryOperatorNode (Microsoft.OData.UriParser.UnaryOperatorNode unaryOperatorNode)
}

public class Microsoft.AspNet.OData.Query.Validators.CountQueryValidator {
	public CountQueryValidator (DefaultQuerySettings defaultQuerySettings)

	public virtual void Validate (CountQueryOption countQueryOption, ODataValidationSettings validationSettings)
}

public class Microsoft.AspNet.OData.Query.Validators.FilterQueryValidator {
	public FilterQueryValidator (DefaultQuerySettings defaultQuerySettings)

	public virtual void Validate (FilterQueryOption filterQueryOption, ODataValidationSettings settings)
	public virtual void Validate (Microsoft.OData.UriParser.FilterClause filterClause, ODataValidationSettings settings, Microsoft.OData.Edm.IEdmModel model)
	internal virtual void Validate (Microsoft.OData.Edm.IEdmProperty property, Microsoft.OData.Edm.IEdmStructuredType structuredType, Microsoft.OData.UriParser.FilterClause filterClause, ODataValidationSettings settings, Microsoft.OData.Edm.IEdmModel model)
	public virtual void ValidateAllNode (Microsoft.OData.UriParser.AllNode allNode, ODataValidationSettings settings)
	public virtual void ValidateAnyNode (Microsoft.OData.UriParser.AnyNode anyNode, ODataValidationSettings settings)
	public virtual void ValidateArithmeticOperator (Microsoft.OData.UriParser.BinaryOperatorNode binaryNode, ODataValidationSettings settings)
	public virtual void ValidateBinaryOperatorNode (Microsoft.OData.UriParser.BinaryOperatorNode binaryOperatorNode, ODataValidationSettings settings)
	public virtual void ValidateCollectionComplexNode (Microsoft.OData.UriParser.CollectionComplexNode collectionComplexNode, ODataValidationSettings settings)
	public virtual void ValidateCollectionPropertyAccessNode (Microsoft.OData.UriParser.CollectionPropertyAccessNode propertyAccessNode, ODataValidationSettings settings)
	public virtual void ValidateCollectionResourceCastNode (Microsoft.OData.UriParser.CollectionResourceCastNode collectionResourceCastNode, ODataValidationSettings settings)
	public virtual void ValidateConstantNode (Microsoft.OData.UriParser.ConstantNode constantNode, ODataValidationSettings settings)
	public virtual void ValidateConvertNode (Microsoft.OData.UriParser.ConvertNode convertNode, ODataValidationSettings settings)
	public virtual void ValidateCountNode (Microsoft.OData.UriParser.CountNode countNode, ODataValidationSettings settings)
	public virtual void ValidateLogicalOperator (Microsoft.OData.UriParser.BinaryOperatorNode binaryNode, ODataValidationSettings settings)
	public virtual void ValidateNavigationPropertyNode (Microsoft.OData.UriParser.QueryNode sourceNode, Microsoft.OData.Edm.IEdmNavigationProperty navigationProperty, ODataValidationSettings settings)
	public virtual void ValidateQueryNode (Microsoft.OData.UriParser.QueryNode node, ODataValidationSettings settings)
	public virtual void ValidateRangeVariable (Microsoft.OData.UriParser.RangeVariable rangeVariable, ODataValidationSettings settings)
	public virtual void ValidateSingleComplexNode (Microsoft.OData.UriParser.SingleComplexNode singleComplexNode, ODataValidationSettings settings)
	public virtual void ValidateSingleResourceCastNode (Microsoft.OData.UriParser.SingleResourceCastNode singleResourceCastNode, ODataValidationSettings settings)
	public virtual void ValidateSingleResourceFunctionCallNode (Microsoft.OData.UriParser.SingleResourceFunctionCallNode node, ODataValidationSettings settings)
	public virtual void ValidateSingleValueFunctionCallNode (Microsoft.OData.UriParser.SingleValueFunctionCallNode node, ODataValidationSettings settings)
	public virtual void ValidateSingleValuePropertyAccessNode (Microsoft.OData.UriParser.SingleValuePropertyAccessNode propertyAccessNode, ODataValidationSettings settings)
	public virtual void ValidateUnaryOperatorNode (Microsoft.OData.UriParser.UnaryOperatorNode unaryOperatorNode, ODataValidationSettings settings)
}

public class Microsoft.AspNet.OData.Query.Validators.ODataQueryValidator {
	public ODataQueryValidator ()

	public virtual void Validate (ODataQueryOptions options, ODataValidationSettings validationSettings)
}

public class Microsoft.AspNet.OData.Query.Validators.OrderByQueryValidator {
	public OrderByQueryValidator (DefaultQuerySettings defaultQuerySettings)

	public virtual void Validate (OrderByQueryOption orderByOption, ODataValidationSettings validationSettings)
}

public class Microsoft.AspNet.OData.Query.Validators.SelectExpandQueryValidator {
	public SelectExpandQueryValidator (DefaultQuerySettings defaultQuerySettings)

	public virtual void Validate (SelectExpandQueryOption selectExpandQueryOption, ODataValidationSettings validationSettings)
}

public class Microsoft.AspNet.OData.Query.Validators.SkipQueryValidator {
	public SkipQueryValidator ()

	public virtual void Validate (SkipQueryOption skipQueryOption, ODataValidationSettings validationSettings)
}

public class Microsoft.AspNet.OData.Query.Validators.SkipTokenQueryValidator {
	public SkipTokenQueryValidator ()

	public virtual void Validate (SkipTokenQueryOption skipToken, ODataValidationSettings validationSettings)
}

public class Microsoft.AspNet.OData.Query.Validators.TopQueryValidator {
	public TopQueryValidator ()

	public virtual void Validate (TopQueryOption topQueryOption, ODataValidationSettings validationSettings)
}

public interface Microsoft.AspNet.OData.Routing.Conventions.IODataRoutingConvention {
	string SelectAction (ODataPath odataPath, System.Web.Http.Controllers.HttpControllerContext controllerContext, System.Linq.ILookup`2[[System.String],[System.Web.Http.Controllers.HttpActionDescriptor]] actionMap)
	string SelectController (ODataPath odataPath, System.Net.Http.HttpRequestMessage request)
}

public abstract class Microsoft.AspNet.OData.Routing.Conventions.NavigationSourceRoutingConvention : IODataRoutingConvention {
	protected NavigationSourceRoutingConvention ()

	public abstract string SelectAction (ODataPath odataPath, System.Web.Http.Controllers.HttpControllerContext controllerContext, System.Linq.ILookup`2[[System.String],[System.Web.Http.Controllers.HttpActionDescriptor]] actionMap)
	public virtual string SelectController (ODataPath odataPath, System.Net.Http.HttpRequestMessage request)
}

public sealed class Microsoft.AspNet.OData.Routing.Conventions.ODataRoutingConventions {
	public static System.Collections.Generic.IList`1[[Microsoft.AspNet.OData.Routing.Conventions.IODataRoutingConvention]] CreateDefault ()
	public static System.Collections.Generic.IList`1[[Microsoft.AspNet.OData.Routing.Conventions.IODataRoutingConvention]] CreateDefaultWithAttributeRouting (string routeName, System.Web.Http.HttpConfiguration configuration)
}

public class Microsoft.AspNet.OData.Routing.Conventions.ActionRoutingConvention : NavigationSourceRoutingConvention, IODataRoutingConvention {
	public ActionRoutingConvention ()

	public virtual string SelectAction (ODataPath odataPath, System.Web.Http.Controllers.HttpControllerContext controllerContext, System.Linq.ILookup`2[[System.String],[System.Web.Http.Controllers.HttpActionDescriptor]] actionMap)
}

public class Microsoft.AspNet.OData.Routing.Conventions.AttributeRoutingConvention : IODataRoutingConvention {
	public AttributeRoutingConvention (string routeName, System.Collections.Generic.IEnumerable`1[[System.Web.Http.Controllers.HttpControllerDescriptor]] controllers)
	public AttributeRoutingConvention (string routeName, System.Web.Http.HttpConfiguration configuration)
	public AttributeRoutingConvention (string routeName, System.Collections.Generic.IEnumerable`1[[System.Web.Http.Controllers.HttpControllerDescriptor]] controllers, IODataPathTemplateHandler pathTemplateHandler)
	public AttributeRoutingConvention (string routeName, System.Web.Http.HttpConfiguration configuration, IODataPathTemplateHandler pathTemplateHandler)

	IODataPathTemplateHandler ODataPathTemplateHandler  { public get; }

	public virtual string SelectAction (ODataPath odataPath, System.Web.Http.Controllers.HttpControllerContext controllerContext, System.Linq.ILookup`2[[System.String],[System.Web.Http.Controllers.HttpActionDescriptor]] actionMap)
	public virtual string SelectController (ODataPath odataPath, System.Net.Http.HttpRequestMessage request)
	public virtual bool ShouldMapController (System.Web.Http.Controllers.HttpControllerDescriptor controller)
}

public class Microsoft.AspNet.OData.Routing.Conventions.DynamicPropertyRoutingConvention : NavigationSourceRoutingConvention, IODataRoutingConvention {
	public DynamicPropertyRoutingConvention ()

	public virtual string SelectAction (ODataPath odataPath, System.Web.Http.Controllers.HttpControllerContext controllerContext, System.Linq.ILookup`2[[System.String],[System.Web.Http.Controllers.HttpActionDescriptor]] actionMap)
}

public class Microsoft.AspNet.OData.Routing.Conventions.EntityRoutingConvention : NavigationSourceRoutingConvention, IODataRoutingConvention {
	public EntityRoutingConvention ()

	public virtual string SelectAction (ODataPath odataPath, System.Web.Http.Controllers.HttpControllerContext controllerContext, System.Linq.ILookup`2[[System.String],[System.Web.Http.Controllers.HttpActionDescriptor]] actionMap)
}

public class Microsoft.AspNet.OData.Routing.Conventions.EntitySetRoutingConvention : NavigationSourceRoutingConvention, IODataRoutingConvention {
	public EntitySetRoutingConvention ()

	public virtual string SelectAction (ODataPath odataPath, System.Web.Http.Controllers.HttpControllerContext controllerContext, System.Linq.ILookup`2[[System.String],[System.Web.Http.Controllers.HttpActionDescriptor]] actionMap)
}

public class Microsoft.AspNet.OData.Routing.Conventions.FunctionRoutingConvention : NavigationSourceRoutingConvention, IODataRoutingConvention {
	public FunctionRoutingConvention ()

	public virtual string SelectAction (ODataPath odataPath, System.Web.Http.Controllers.HttpControllerContext controllerContext, System.Linq.ILookup`2[[System.String],[System.Web.Http.Controllers.HttpActionDescriptor]] actionMap)
}

public class Microsoft.AspNet.OData.Routing.Conventions.MetadataRoutingConvention : IODataRoutingConvention {
	public MetadataRoutingConvention ()

	public virtual string SelectAction (ODataPath odataPath, System.Web.Http.Controllers.HttpControllerContext controllerContext, System.Linq.ILookup`2[[System.String],[System.Web.Http.Controllers.HttpActionDescriptor]] actionMap)
	public virtual string SelectController (ODataPath odataPath, System.Net.Http.HttpRequestMessage request)
}

public class Microsoft.AspNet.OData.Routing.Conventions.NavigationRoutingConvention : NavigationSourceRoutingConvention, IODataRoutingConvention {
	public NavigationRoutingConvention ()

	public virtual string SelectAction (ODataPath odataPath, System.Web.Http.Controllers.HttpControllerContext controllerContext, System.Linq.ILookup`2[[System.String],[System.Web.Http.Controllers.HttpActionDescriptor]] actionMap)
}

public class Microsoft.AspNet.OData.Routing.Conventions.OperationImportRoutingConvention : NavigationSourceRoutingConvention, IODataRoutingConvention {
	public OperationImportRoutingConvention ()

	public virtual string SelectAction (ODataPath odataPath, System.Web.Http.Controllers.HttpControllerContext controllerContext, System.Linq.ILookup`2[[System.String],[System.Web.Http.Controllers.HttpActionDescriptor]] actionMap)
}

public class Microsoft.AspNet.OData.Routing.Conventions.PropertyRoutingConvention : NavigationSourceRoutingConvention, IODataRoutingConvention {
	public PropertyRoutingConvention ()

	public virtual string SelectAction (ODataPath odataPath, System.Web.Http.Controllers.HttpControllerContext controllerContext, System.Linq.ILookup`2[[System.String],[System.Web.Http.Controllers.HttpActionDescriptor]] actionMap)
}

public class Microsoft.AspNet.OData.Routing.Conventions.RefRoutingConvention : NavigationSourceRoutingConvention, IODataRoutingConvention {
	public RefRoutingConvention ()

	public virtual string SelectAction (ODataPath odataPath, System.Web.Http.Controllers.HttpControllerContext controllerContext, System.Linq.ILookup`2[[System.String],[System.Web.Http.Controllers.HttpActionDescriptor]] actionMap)
}

public class Microsoft.AspNet.OData.Routing.Conventions.SelectControllerResult {
	public SelectControllerResult (string controllerName, System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] values)

	string ControllerName  { public get; }
	System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] Values  { public get; }
}

public class Microsoft.AspNet.OData.Routing.Conventions.SingletonRoutingConvention : NavigationSourceRoutingConvention, IODataRoutingConvention {
	public SingletonRoutingConvention ()

	public virtual string SelectAction (ODataPath odataPath, System.Web.Http.Controllers.HttpControllerContext controllerContext, System.Linq.ILookup`2[[System.String],[System.Web.Http.Controllers.HttpActionDescriptor]] actionMap)
}

public class Microsoft.AspNet.OData.Routing.Conventions.UnmappedRequestRoutingConvention : NavigationSourceRoutingConvention, IODataRoutingConvention {
	public UnmappedRequestRoutingConvention ()

	public virtual string SelectAction (ODataPath odataPath, System.Web.Http.Controllers.HttpControllerContext controllerContext, System.Linq.ILookup`2[[System.String],[System.Web.Http.Controllers.HttpActionDescriptor]] actionMap)
}

public abstract class Microsoft.AspNet.OData.Routing.Template.ODataPathSegmentTemplate {
	protected ODataPathSegmentTemplate ()

	public virtual bool TryMatch (Microsoft.OData.UriParser.ODataPathSegment pathSegment, System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] values)
}

public class Microsoft.AspNet.OData.Routing.Template.DynamicSegmentTemplate : ODataPathSegmentTemplate {
	public DynamicSegmentTemplate (Microsoft.OData.UriParser.DynamicPathSegment segment)

	Microsoft.OData.UriParser.DynamicPathSegment Segment  { public get; }

	public virtual bool TryMatch (Microsoft.OData.UriParser.ODataPathSegment pathSegment, System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] values)
}

public class Microsoft.AspNet.OData.Routing.Template.EntitySetSegmentTemplate : ODataPathSegmentTemplate {
	public EntitySetSegmentTemplate (Microsoft.OData.UriParser.EntitySetSegment segment)

	Microsoft.OData.UriParser.EntitySetSegment Segment  { public get; }

	public virtual bool TryMatch (Microsoft.OData.UriParser.ODataPathSegment pathSegment, System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] values)
}

public class Microsoft.AspNet.OData.Routing.Template.KeySegmentTemplate : ODataPathSegmentTemplate {
	public KeySegmentTemplate (Microsoft.OData.UriParser.KeySegment segment)

	System.Collections.Generic.IDictionary`2[[System.String],[System.String]] ParameterMappings  { public get; }
	Microsoft.OData.UriParser.KeySegment Segment  { public get; public set; }

	public virtual bool TryMatch (Microsoft.OData.UriParser.ODataPathSegment pathSegment, System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] values)
}

public class Microsoft.AspNet.OData.Routing.Template.NavigationPropertyLinkSegmentTemplate : ODataPathSegmentTemplate {
	public NavigationPropertyLinkSegmentTemplate (Microsoft.OData.UriParser.NavigationPropertyLinkSegment segment)

	Microsoft.OData.UriParser.NavigationPropertyLinkSegment Segment  { public get; }

	public virtual bool TryMatch (Microsoft.OData.UriParser.ODataPathSegment pathSegment, System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] values)
}

public class Microsoft.AspNet.OData.Routing.Template.NavigationPropertySegmentTemplate : ODataPathSegmentTemplate {
	public NavigationPropertySegmentTemplate (Microsoft.OData.UriParser.NavigationPropertySegment segment)

	Microsoft.OData.UriParser.NavigationPropertySegment Segment  { public get; }

	public virtual bool TryMatch (Microsoft.OData.UriParser.ODataPathSegment pathSegment, System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] values)
}

public class Microsoft.AspNet.OData.Routing.Template.ODataPathSegmentTemplate`1 : ODataPathSegmentTemplate {
	public ODataPathSegmentTemplate`1 ()

	public virtual bool TryMatch (Microsoft.OData.UriParser.ODataPathSegment pathSegment, System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] values)
}

public class Microsoft.AspNet.OData.Routing.Template.ODataPathSegmentTemplateTranslator : Microsoft.OData.UriParser.PathSegmentTranslator`1[[Microsoft.AspNet.OData.Routing.Template.ODataPathSegmentTemplate]] {
	public ODataPathSegmentTemplateTranslator ()

	public virtual ODataPathSegmentTemplate Translate (Microsoft.OData.UriParser.BatchReferenceSegment segment)
	public virtual ODataPathSegmentTemplate Translate (Microsoft.OData.UriParser.BatchSegment segment)
	public virtual ODataPathSegmentTemplate Translate (Microsoft.OData.UriParser.CountSegment segment)
	public virtual ODataPathSegmentTemplate Translate (Microsoft.OData.UriParser.DynamicPathSegment segment)
	public virtual ODataPathSegmentTemplate Translate (Microsoft.OData.UriParser.EntitySetSegment segment)
	public virtual ODataPathSegmentTemplate Translate (Microsoft.OData.UriParser.KeySegment segment)
	public virtual ODataPathSegmentTemplate Translate (Microsoft.OData.UriParser.MetadataSegment segment)
	public virtual ODataPathSegmentTemplate Translate (Microsoft.OData.UriParser.NavigationPropertyLinkSegment segment)
	public virtual ODataPathSegmentTemplate Translate (Microsoft.OData.UriParser.NavigationPropertySegment segment)
	public virtual ODataPathSegmentTemplate Translate (Microsoft.OData.UriParser.OperationImportSegment segment)
	public virtual ODataPathSegmentTemplate Translate (Microsoft.OData.UriParser.OperationSegment segment)
	public virtual ODataPathSegmentTemplate Translate (Microsoft.OData.UriParser.PathTemplateSegment segment)
	public virtual ODataPathSegmentTemplate Translate (Microsoft.OData.UriParser.PropertySegment segment)
	public virtual ODataPathSegmentTemplate Translate (Microsoft.OData.UriParser.SingletonSegment segment)
	public virtual ODataPathSegmentTemplate Translate (Microsoft.OData.UriParser.TypeSegment segment)
	public virtual ODataPathSegmentTemplate Translate (Microsoft.OData.UriParser.ValueSegment segment)
}

public class Microsoft.AspNet.OData.Routing.Template.ODataPathTemplate {
	public ODataPathTemplate (ODataPathSegmentTemplate[] segments)
	public ODataPathTemplate (System.Collections.Generic.IEnumerable`1[[Microsoft.AspNet.OData.Routing.Template.ODataPathSegmentTemplate]] segments)
	public ODataPathTemplate (System.Collections.Generic.IList`1[[Microsoft.AspNet.OData.Routing.Template.ODataPathSegmentTemplate]] segments)

	System.Collections.ObjectModel.ReadOnlyCollection`1[[Microsoft.AspNet.OData.Routing.Template.ODataPathSegmentTemplate]] Segments  { public get; }

	public bool TryMatch (ODataPath path, System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] values)
}

public class Microsoft.AspNet.OData.Routing.Template.OperationImportSegmentTemplate : ODataPathSegmentTemplate {
	public OperationImportSegmentTemplate (Microsoft.OData.UriParser.OperationImportSegment segment)

	System.Collections.Generic.IDictionary`2[[System.String],[System.String]] ParameterMappings  { public get; }
	Microsoft.OData.UriParser.OperationImportSegment Segment  { public get; }

	public virtual bool TryMatch (Microsoft.OData.UriParser.ODataPathSegment pathSegment, System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] values)
}

public class Microsoft.AspNet.OData.Routing.Template.OperationSegmentTemplate : ODataPathSegmentTemplate {
	public OperationSegmentTemplate (Microsoft.OData.UriParser.OperationSegment segment)

	System.Collections.Generic.IDictionary`2[[System.String],[System.String]] ParameterMappings  { public get; }
	Microsoft.OData.UriParser.OperationSegment Segment  { public get; }

	public virtual bool TryMatch (Microsoft.OData.UriParser.ODataPathSegment pathSegment, System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] values)
}

public class Microsoft.AspNet.OData.Routing.Template.PathTemplateSegmentTemplate : ODataPathSegmentTemplate {
	public PathTemplateSegmentTemplate (Microsoft.OData.UriParser.PathTemplateSegment segment)

	string PropertyName  { public get; }
	string SegmentName  { public get; }
	Microsoft.OData.UriParser.PathTemplateSegment TemplateSegment  { public get; }

	public virtual bool TryMatch (Microsoft.OData.UriParser.ODataPathSegment pathSegment, System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] values)
}

public class Microsoft.AspNet.OData.Routing.Template.PropertySegmentTemplate : ODataPathSegmentTemplate {
	public PropertySegmentTemplate (Microsoft.OData.UriParser.PropertySegment segment)

	Microsoft.OData.UriParser.PropertySegment Segment  { public get; }

	public virtual bool TryMatch (Microsoft.OData.UriParser.ODataPathSegment pathSegment, System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] values)
}

public class Microsoft.AspNet.OData.Routing.Template.SingletonSegmentTemplate : ODataPathSegmentTemplate {
	public SingletonSegmentTemplate (Microsoft.OData.UriParser.SingletonSegment segment)

	Microsoft.OData.UriParser.SingletonSegment Segment  { public get; }

	public virtual bool TryMatch (Microsoft.OData.UriParser.ODataPathSegment pathSegment, System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] values)
}

public class Microsoft.AspNet.OData.Routing.Template.TypeSegmentTemplate : ODataPathSegmentTemplate {
	public TypeSegmentTemplate (Microsoft.OData.UriParser.TypeSegment segment)

	Microsoft.OData.UriParser.TypeSegment Segment  { public get; }

	public virtual bool TryMatch (Microsoft.OData.UriParser.ODataPathSegment pathSegment, System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] values)
}

