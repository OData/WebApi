public enum System.Web.OData.EdmDeltaEntityKind : int {
	DeletedEntry = 1
	DeletedLinkEntry = 2
	Entry = 0
	LinkEntry = 3
	Unknown = 4
}

public interface System.Web.OData.IDelta {
	void Clear ()
	System.Collections.Generic.IEnumerable`1[[System.String]] GetChangedPropertyNames ()
	System.Collections.Generic.IEnumerable`1[[System.String]] GetUnchangedPropertyNames ()
	bool TryGetPropertyType (string name, out System.Type& type)
	bool TryGetPropertyValue (string name, out System.Object& value)
	bool TrySetPropertyValue (string name, object value)
}

public interface System.Web.OData.IEdmChangedObject : IEdmObject, IEdmStructuredObject {
	EdmDeltaEntityKind DeltaKind  { public abstract get; }
}

public interface System.Web.OData.IEdmComplexObject : IEdmObject, IEdmStructuredObject {
}

public interface System.Web.OData.IEdmDeltaDeletedEntityObject : IEdmChangedObject, IEdmObject, IEdmStructuredObject {
	string Id  { public abstract get; public abstract set; }
	Microsoft.OData.Core.DeltaDeletedEntryReason Reason  { public abstract get; public abstract set; }
}

public interface System.Web.OData.IEdmDeltaDeletedLink : IEdmChangedObject, IEdmDeltaLinkBase, IEdmObject, IEdmStructuredObject {
}

public interface System.Web.OData.IEdmDeltaLink : IEdmChangedObject, IEdmDeltaLinkBase, IEdmObject, IEdmStructuredObject {
}

public interface System.Web.OData.IEdmDeltaLinkBase : IEdmChangedObject, IEdmObject, IEdmStructuredObject {
	string Relationship  { public abstract get; public abstract set; }
	System.Uri Source  { public abstract get; public abstract set; }
	System.Uri Target  { public abstract get; public abstract set; }
}

public interface System.Web.OData.IEdmEntityObject : IEdmObject, IEdmStructuredObject {
}

public interface System.Web.OData.IEdmEnumObject : IEdmObject {
}

public interface System.Web.OData.IEdmObject {
	Microsoft.OData.Edm.IEdmTypeReference GetEdmType ()
}

public interface System.Web.OData.IEdmStructuredObject : IEdmObject {
	bool TryGetPropertyValue (string propertyName, out System.Object& value)
}

[
NonValidatingParameterBindingAttribute(),
]
public abstract class System.Web.OData.Delta : System.Dynamic.DynamicObject, IDynamicMetaObjectProvider, IDelta {
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
public abstract class System.Web.OData.EdmStructuredObject : Delta, IDynamicMetaObjectProvider, IDelta, IEdmObject, IEdmStructuredObject {
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
public abstract class System.Web.OData.ODataController : System.Web.Http.ApiController, IDisposable, IHttpController {
	protected ODataController ()

	protected virtual CreatedODataResult`1 Created (TEntity entity)
	protected virtual UpdatedODataResult`1 Updated (TEntity entity)
}

[
DataContractAttribute(),
]
public abstract class System.Web.OData.PageResult {
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

public abstract class System.Web.OData.TypedDelta : Delta, IDynamicMetaObjectProvider, IDelta {
	protected TypedDelta ()

	System.Type EntityType  { public abstract get; }
	System.Type ExpectedClrType  { public abstract get; }
}

[
EditorBrowsableAttribute(),
ExtensionAttribute(),
]
public sealed class System.Web.OData.EdmModelExtensions {
	[
	ExtensionAttribute(),
	]
	public static ActionLinkBuilder GetActionLinkBuilder (Microsoft.OData.Edm.IEdmModel model, Microsoft.OData.Edm.IEdmAction action)

	[
	ExtensionAttribute(),
	]
	public static FunctionLinkBuilder GetFunctionLinkBuilder (Microsoft.OData.Edm.IEdmModel model, Microsoft.OData.Edm.IEdmFunction function)

	[
	ExtensionAttribute(),
	]
	public static NavigationSourceLinkBuilderAnnotation GetNavigationSourceLinkBuilder (Microsoft.OData.Edm.IEdmModel model, Microsoft.OData.Edm.IEdmNavigationSource navigationSource)

	[
	ExtensionAttribute(),
	]
	public static void SetActionLinkBuilder (Microsoft.OData.Edm.IEdmModel model, Microsoft.OData.Edm.IEdmAction action, ActionLinkBuilder actionLinkBuilder)

	[
	ExtensionAttribute(),
	]
	public static void SetFunctionLinkBuilder (Microsoft.OData.Edm.IEdmModel model, Microsoft.OData.Edm.IEdmFunction function, FunctionLinkBuilder functionLinkBuilder)

	[
	ExtensionAttribute(),
	]
	public static void SetNavigationSourceLinkBuilder (Microsoft.OData.Edm.IEdmModel model, Microsoft.OData.Edm.IEdmNavigationSource navigationSource, NavigationSourceLinkBuilderAnnotation navigationSourceLinkBuilder)
}

[
ExtensionAttribute(),
]
public sealed class System.Web.OData.EdmTypeExtensions {
	[
	ExtensionAttribute(),
	]
	public static bool IsDeltaFeed (Microsoft.OData.Edm.IEdmType type)
}

public sealed class System.Web.OData.ODataUriFunctions {
	public static void AddUriCustomFunction (string functionName, Microsoft.OData.Core.UriParser.FunctionSignatureWithReturnType functionSignature, System.Reflection.MethodInfo methodInfo)
	public static bool RemoveCustomUriFunction (string functionName, Microsoft.OData.Core.UriParser.FunctionSignatureWithReturnType functionSignature, System.Reflection.MethodInfo methodInfo)
}

public class System.Web.OData.ClrPropertyInfoAnnotation {
	public ClrPropertyInfoAnnotation (System.Reflection.PropertyInfo clrPropertyInfo)

	System.Reflection.PropertyInfo ClrPropertyInfo  { public get; }
}

public class System.Web.OData.ClrTypeAnnotation {
	public ClrTypeAnnotation (System.Type clrType)

	System.Type ClrType  { public get; }
}

[
NonValidatingParameterBindingAttribute(),
]
public class System.Web.OData.Delta`1 : TypedDelta, IDynamicMetaObjectProvider, IDelta {
	public Delta`1 ()
	public Delta`1 (System.Type entityType)
	public Delta`1 (System.Type entityType, System.Collections.Generic.IEnumerable`1[[System.String]] updatableProperties)
	public Delta`1 (System.Type entityType, System.Collections.Generic.IEnumerable`1[[System.String]] updatableProperties, System.Reflection.PropertyInfo dynamicDictionaryPropertyInfo)

	System.Type EntityType  { public virtual get; }
	System.Type ExpectedClrType  { public virtual get; }

	public virtual void Clear ()
	public void CopyChangedValues (TEntityType original)
	public void CopyUnchangedValues (TEntityType original)
	public virtual System.Collections.Generic.IEnumerable`1[[System.String]] GetChangedPropertyNames ()
	public TEntityType GetEntity ()
	public virtual System.Collections.Generic.IEnumerable`1[[System.String]] GetUnchangedPropertyNames ()
	public void Patch (TEntityType original)
	public void Put (TEntityType original)
	public virtual bool TryGetPropertyType (string name, out System.Type& type)
	public virtual bool TryGetPropertyValue (string name, out System.Object& value)
	public virtual bool TrySetPropertyValue (string name, object value)
}

[
NonValidatingParameterBindingAttribute(),
]
public class System.Web.OData.EdmChangedObjectCollection : System.Collections.ObjectModel.Collection`1[[System.Web.OData.IEdmChangedObject]], ICollection, IEnumerable, IList, IEdmObject, ICollection`1, IEnumerable`1, IList`1, IReadOnlyCollection`1, IReadOnlyList`1 {
	public EdmChangedObjectCollection (Microsoft.OData.Edm.IEdmEntityType entityType)
	public EdmChangedObjectCollection (Microsoft.OData.Edm.IEdmEntityType entityType, System.Collections.Generic.IList`1[[System.Web.OData.IEdmChangedObject]] changedObjectList)

	public virtual Microsoft.OData.Edm.IEdmTypeReference GetEdmType ()
}

[
NonValidatingParameterBindingAttribute(),
]
public class System.Web.OData.EdmComplexObject : EdmStructuredObject, IDynamicMetaObjectProvider, IDelta, IEdmComplexObject, IEdmObject, IEdmStructuredObject {
	public EdmComplexObject (Microsoft.OData.Edm.IEdmComplexType edmType)
	public EdmComplexObject (Microsoft.OData.Edm.IEdmComplexTypeReference edmType)
	public EdmComplexObject (Microsoft.OData.Edm.IEdmComplexType edmType, bool isNullable)
}

[
NonValidatingParameterBindingAttribute(),
]
public class System.Web.OData.EdmComplexObjectCollection : System.Collections.ObjectModel.Collection`1[[System.Web.OData.IEdmComplexObject]], ICollection, IEnumerable, IList, IEdmObject, ICollection`1, IEnumerable`1, IList`1, IReadOnlyCollection`1, IReadOnlyList`1 {
	public EdmComplexObjectCollection (Microsoft.OData.Edm.IEdmCollectionTypeReference edmType)
	public EdmComplexObjectCollection (Microsoft.OData.Edm.IEdmCollectionTypeReference edmType, System.Collections.Generic.IList`1[[System.Web.OData.IEdmComplexObject]] list)

	public virtual Microsoft.OData.Edm.IEdmTypeReference GetEdmType ()
}

[
NonValidatingParameterBindingAttribute(),
]
public class System.Web.OData.EdmDeltaDeletedEntityObject : EdmEntityObject, IDynamicMetaObjectProvider, IDelta, IEdmChangedObject, IEdmDeltaDeletedEntityObject, IEdmEntityObject, IEdmObject, IEdmStructuredObject {
	public EdmDeltaDeletedEntityObject (Microsoft.OData.Edm.IEdmEntityType entityType)
	public EdmDeltaDeletedEntityObject (Microsoft.OData.Edm.IEdmEntityTypeReference entityTypeReference)
	public EdmDeltaDeletedEntityObject (Microsoft.OData.Edm.IEdmEntityType entityType, bool isNullable)

	EdmDeltaEntityKind DeltaKind  { public virtual get; }
	string Id  { public virtual get; public virtual set; }
	Microsoft.OData.Core.DeltaDeletedEntryReason Reason  { public virtual get; public virtual set; }
}

[
NonValidatingParameterBindingAttribute(),
]
public class System.Web.OData.EdmDeltaDeletedLink : EdmEntityObject, IDynamicMetaObjectProvider, IDelta, IEdmChangedObject, IEdmDeltaDeletedLink, IEdmDeltaLinkBase, IEdmEntityObject, IEdmObject, IEdmStructuredObject {
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
public class System.Web.OData.EdmDeltaEntityObject : EdmEntityObject, IDynamicMetaObjectProvider, IDelta, IEdmChangedObject, IEdmEntityObject, IEdmObject, IEdmStructuredObject {
	public EdmDeltaEntityObject (Microsoft.OData.Edm.IEdmEntityType entityType)
	public EdmDeltaEntityObject (Microsoft.OData.Edm.IEdmEntityTypeReference entityTypeReference)
	public EdmDeltaEntityObject (Microsoft.OData.Edm.IEdmEntityType entityType, bool isNullable)

	EdmDeltaEntityKind DeltaKind  { public virtual get; }
}

[
NonValidatingParameterBindingAttribute(),
]
public class System.Web.OData.EdmDeltaLink : EdmEntityObject, IDynamicMetaObjectProvider, IDelta, IEdmChangedObject, IEdmDeltaLink, IEdmDeltaLinkBase, IEdmEntityObject, IEdmObject, IEdmStructuredObject {
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
public class System.Web.OData.EdmEntityObject : EdmStructuredObject, IDynamicMetaObjectProvider, IDelta, IEdmEntityObject, IEdmObject, IEdmStructuredObject {
	public EdmEntityObject (Microsoft.OData.Edm.IEdmEntityType edmType)
	public EdmEntityObject (Microsoft.OData.Edm.IEdmEntityTypeReference edmType)
	public EdmEntityObject (Microsoft.OData.Edm.IEdmEntityType edmType, bool isNullable)
}

[
NonValidatingParameterBindingAttribute(),
]
public class System.Web.OData.EdmEntityObjectCollection : System.Collections.ObjectModel.Collection`1[[System.Web.OData.IEdmEntityObject]], ICollection, IEnumerable, IList, IEdmObject, ICollection`1, IEnumerable`1, IList`1, IReadOnlyCollection`1, IReadOnlyList`1 {
	public EdmEntityObjectCollection (Microsoft.OData.Edm.IEdmCollectionTypeReference edmType)
	public EdmEntityObjectCollection (Microsoft.OData.Edm.IEdmCollectionTypeReference edmType, System.Collections.Generic.IList`1[[System.Web.OData.IEdmEntityObject]] list)

	public virtual Microsoft.OData.Edm.IEdmTypeReference GetEdmType ()
}

[
NonValidatingParameterBindingAttribute(),
]
public class System.Web.OData.EdmEnumObject : IEdmEnumObject, IEdmObject {
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
public class System.Web.OData.EdmEnumObjectCollection : System.Collections.ObjectModel.Collection`1[[System.Web.OData.IEdmEnumObject]], ICollection, IEnumerable, IList, IEdmObject, ICollection`1, IEnumerable`1, IList`1, IReadOnlyCollection`1, IReadOnlyList`1 {
	public EdmEnumObjectCollection (Microsoft.OData.Edm.IEdmCollectionTypeReference edmType)
	public EdmEnumObjectCollection (Microsoft.OData.Edm.IEdmCollectionTypeReference edmType, System.Collections.Generic.IList`1[[System.Web.OData.IEdmEnumObject]] list)

	public virtual Microsoft.OData.Edm.IEdmTypeReference GetEdmType ()
}

[
AttributeUsageAttribute(),
]
public class System.Web.OData.EnableQueryAttribute : System.Web.Http.Filters.ActionFilterAttribute, _Attribute, IActionFilter, IFilter {
	public EnableQueryAttribute ()

	AllowedArithmeticOperators AllowedArithmeticOperators  { public get; public set; }
	AllowedFunctions AllowedFunctions  { public get; public set; }
	AllowedLogicalOperators AllowedLogicalOperators  { public get; public set; }
	string AllowedOrderByProperties  { public get; public set; }
	AllowedQueryOptions AllowedQueryOptions  { public get; public set; }
	bool EnableConstantParameterization  { public get; public set; }
	bool EnsureStableOrdering  { public get; public set; }
	HandleNullPropagationOption HandleNullPropagation  { public get; public set; }
	int MaxAnyAllExpressionDepth  { public get; public set; }
	int MaxExpansionDepth  { public get; public set; }
	int MaxNodeCount  { public get; public set; }
	int MaxOrderByNodeCount  { public get; public set; }
	int MaxSkip  { public get; public set; }
	int MaxTop  { public get; public set; }
	int PageSize  { public get; public set; }
	bool SearchDerivedTypeWhenAutoExpand  { public get; public set; }

	public virtual System.Linq.IQueryable ApplyQuery (System.Linq.IQueryable queryable, ODataQueryOptions queryOptions)
	public virtual object ApplyQuery (object entity, ODataQueryOptions queryOptions)
	public virtual Microsoft.OData.Edm.IEdmModel GetModel (System.Type elementClrType, System.Net.Http.HttpRequestMessage request, System.Web.Http.Controllers.HttpActionDescriptor actionDescriptor)
	public virtual void OnActionExecuted (System.Web.Http.Filters.HttpActionExecutedContext actionExecutedContext)
	public virtual void ValidateQuery (System.Net.Http.HttpRequestMessage request, ODataQueryOptions queryOptions)
}

public class System.Web.OData.EntityInstanceContext {
	public EntityInstanceContext ()
	public EntityInstanceContext (ODataSerializerContext serializerContext, Microsoft.OData.Edm.IEdmEntityTypeReference entityType, object entityInstance)

	Microsoft.OData.Edm.IEdmModel EdmModel  { public get; public set; }
	IEdmEntityObject EdmObject  { public get; public set; }
	object EntityInstance  { public get; public set; }
	Microsoft.OData.Edm.IEdmEntityType EntityType  { public get; public set; }
	Microsoft.OData.Edm.IEdmNavigationSource NavigationSource  { public get; public set; }
	System.Net.Http.HttpRequestMessage Request  { public get; public set; }
	ODataSerializerContext SerializerContext  { public get; public set; }
	bool SkipExpensiveAvailabilityChecks  { public get; public set; }
	System.Web.Http.Routing.UrlHelper Url  { public get; public set; }
}

public class System.Web.OData.EntityInstanceContext`1 : EntityInstanceContext {
	public EntityInstanceContext`1 ()

	[
	ObsoleteAttribute(),
	]
	TEntityType EntityInstance  { public get; public set; }
}

public class System.Web.OData.ETagMessageHandler : System.Net.Http.DelegatingHandler, IDisposable {
	public ETagMessageHandler ()

	[
	DebuggerStepThroughAttribute(),
	AsyncStateMachineAttribute(),
	]
	protected virtual System.Threading.Tasks.Task`1[[System.Net.Http.HttpResponseMessage]] SendAsync (System.Net.Http.HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
}

public class System.Web.OData.FeedContext {
	public FeedContext ()

	Microsoft.OData.Edm.IEdmEntitySetBase EntitySetBase  { public get; public set; }
	object FeedInstance  { public get; public set; }
	System.Net.Http.HttpRequestMessage Request  { public get; public set; }
	System.Web.Http.Controllers.HttpRequestContext RequestContext  { public get; public set; }
	System.Web.Http.Routing.UrlHelper Url  { public get; public set; }
}

public class System.Web.OData.MetadataController : ODataController, IDisposable, IHttpController {
	public MetadataController ()

	public Microsoft.OData.Edm.IEdmModel GetMetadata ()
	public Microsoft.OData.Core.ODataServiceDocument GetServiceDocument ()
}

public class System.Web.OData.NullEdmComplexObject : IEdmComplexObject, IEdmObject, IEdmStructuredObject {
	public NullEdmComplexObject (Microsoft.OData.Edm.IEdmComplexTypeReference edmType)

	public virtual Microsoft.OData.Edm.IEdmTypeReference GetEdmType ()
	public virtual bool TryGetPropertyValue (string propertyName, out System.Object& value)
}

[
NonValidatingParameterBindingAttribute(),
]
public class System.Web.OData.ODataActionParameters : System.Collections.Generic.Dictionary`2[[System.String],[System.Object]], ICollection, IDictionary, IEnumerable, IDeserializationCallback, ISerializable, IDictionary`2, IReadOnlyDictionary`2, ICollection`1, IEnumerable`1, IReadOnlyCollection`1 {
	public ODataActionParameters ()
}

[
AttributeUsageAttribute(),
]
public class System.Web.OData.ODataFormattingAttribute : System.Attribute, _Attribute, IControllerConfiguration {
	public ODataFormattingAttribute ()

	public virtual System.Collections.Generic.IList`1[[System.Web.OData.Formatter.ODataMediaTypeFormatter]] CreateODataFormatters ()
	public virtual void Initialize (System.Web.Http.Controllers.HttpControllerSettings controllerSettings, System.Web.Http.Controllers.HttpControllerDescriptor controllerDescriptor)
}

public class System.Web.OData.ODataNullValueMessageHandler : System.Net.Http.DelegatingHandler, IDisposable {
	public ODataNullValueMessageHandler ()

	[
	DebuggerStepThroughAttribute(),
	AsyncStateMachineAttribute(),
	]
	protected virtual System.Threading.Tasks.Task`1[[System.Net.Http.HttpResponseMessage]] SendAsync (System.Net.Http.HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
}

public class System.Web.OData.ODataQueryContext {
	public ODataQueryContext (Microsoft.OData.Edm.IEdmModel model, Microsoft.OData.Edm.IEdmType elementType, ODataPath path)
	public ODataQueryContext (Microsoft.OData.Edm.IEdmModel model, System.Type elementClrType, ODataPath path)

	System.Type ElementClrType  { public get; }
	Microsoft.OData.Edm.IEdmType ElementType  { public get; }
	Microsoft.OData.Edm.IEdmModel Model  { public get; }
	Microsoft.OData.Edm.IEdmNavigationSource NavigationSource  { public get; }
	ODataPath Path  { public get; }
}

public class System.Web.OData.ODataSwaggerConverter {
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
public class System.Web.OData.ODataUntypedActionParameters : System.Collections.Generic.Dictionary`2[[System.String],[System.Object]], ICollection, IDictionary, IEnumerable, IDeserializationCallback, ISerializable, IDictionary`2, IReadOnlyDictionary`2, ICollection`1, IEnumerable`1, IReadOnlyCollection`1 {
	public ODataUntypedActionParameters (Microsoft.OData.Edm.IEdmAction action)

	Microsoft.OData.Edm.IEdmAction Action  { public get; }
}

[
JsonObjectAttribute(),
DataContractAttribute(),
]
public class System.Web.OData.PageResult`1 : PageResult, IEnumerable`1, IEnumerable {
	public PageResult`1 (IEnumerable`1 items, System.Uri nextPageLink, System.Nullable`1[[System.Int64]] count)

	[
	DataMemberAttribute(),
	]
	IEnumerable`1 Items  { public get; }

	public virtual IEnumerator`1 GetEnumerator ()
	System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
}

public class System.Web.OData.QueryableRestrictions {
	public QueryableRestrictions ()
	public QueryableRestrictions (PropertyConfiguration propertyConfiguration)

	bool AutoExpand  { public get; public set; }
	bool NonFilterable  { public get; public set; }
	bool NotCountable  { public get; public set; }
	bool NotExpandable  { public get; public set; }
	bool NotFilterable  { public get; public set; }
	bool NotNavigable  { public get; public set; }
	bool NotSortable  { public get; public set; }
	bool Unsortable  { public get; public set; }
}

public class System.Web.OData.QueryableRestrictionsAnnotation {
	public QueryableRestrictionsAnnotation (QueryableRestrictions restrictions)

	QueryableRestrictions Restrictions  { public get; }
}

[
AttributeUsageAttribute(),
]
public sealed class System.Web.OData.FromODataUriAttribute : System.Web.Http.ModelBinding.ModelBinderAttribute, _Attribute {
	public FromODataUriAttribute ()

	public virtual System.Web.Http.Controllers.HttpParameterBinding GetBinding (System.Web.Http.Controllers.HttpParameterDescriptor parameter)
}

[
AttributeUsageAttribute(),
]
public sealed class System.Web.OData.ODataQueryParameterBindingAttribute : System.Web.Http.ParameterBindingAttribute, _Attribute {
	public ODataQueryParameterBindingAttribute ()

	public virtual System.Web.Http.Controllers.HttpParameterBinding GetBinding (System.Web.Http.Controllers.HttpParameterDescriptor parameter)
}

[
AttributeUsageAttribute(),
]
public sealed class System.Web.OData.ODataRoutingAttribute : System.Attribute, _Attribute, IControllerConfiguration {
	public ODataRoutingAttribute ()

	public virtual void Initialize (System.Web.Http.Controllers.HttpControllerSettings controllerSettings, System.Web.Http.Controllers.HttpControllerDescriptor controllerDescriptor)
}

public abstract class System.Web.OData.Batch.ODataBatchHandler : System.Web.Http.Batch.HttpBatchHandler, IDisposable {
	protected ODataBatchHandler (System.Web.Http.HttpServer httpServer)

	Microsoft.OData.Core.ODataMessageQuotas MessageQuotas  { public get; }
	string ODataRouteName  { public get; public set; }

	public virtual System.Threading.Tasks.Task`1[[System.Net.Http.HttpResponseMessage]] CreateResponseMessageAsync (System.Collections.Generic.IEnumerable`1[[System.Web.OData.Batch.ODataBatchResponseItem]] responses, System.Net.Http.HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
	public virtual System.Uri GetBaseUri (System.Net.Http.HttpRequestMessage request)
	public virtual void ValidateRequest (System.Net.Http.HttpRequestMessage request)
}

public abstract class System.Web.OData.Batch.ODataBatchRequestItem : IDisposable {
	protected ODataBatchRequestItem ()

	public virtual void Dispose ()
	protected abstract void Dispose (bool disposing)
	public abstract System.Collections.Generic.IEnumerable`1[[System.IDisposable]] GetResourcesForDisposal ()
	[
	DebuggerStepThroughAttribute(),
	AsyncStateMachineAttribute(),
	]
	public static System.Threading.Tasks.Task`1[[System.Net.Http.HttpResponseMessage]] SendMessageAsync (System.Net.Http.HttpMessageInvoker invoker, System.Net.Http.HttpRequestMessage request, System.Threading.CancellationToken cancellationToken, System.Collections.Generic.Dictionary`2[[System.String],[System.String]] contentIdToLocationMapping)

	public abstract System.Threading.Tasks.Task`1[[System.Web.OData.Batch.ODataBatchResponseItem]] SendRequestAsync (System.Net.Http.HttpMessageInvoker invoker, System.Threading.CancellationToken cancellationToken)
}

public abstract class System.Web.OData.Batch.ODataBatchResponseItem : IDisposable {
	protected ODataBatchResponseItem ()

	public virtual void Dispose ()
	protected abstract void Dispose (bool disposing)
	internal virtual bool IsResponseSuccessful ()
	public static System.Threading.Tasks.Task WriteMessageAsync (Microsoft.OData.Core.ODataBatchWriter writer, System.Net.Http.HttpResponseMessage response)
	[
	DebuggerStepThroughAttribute(),
	AsyncStateMachineAttribute(),
	]
	public static System.Threading.Tasks.Task WriteMessageAsync (Microsoft.OData.Core.ODataBatchWriter writer, System.Net.Http.HttpResponseMessage response, System.Threading.CancellationToken cancellationToken)

	public abstract System.Threading.Tasks.Task WriteResponseAsync (Microsoft.OData.Core.ODataBatchWriter writer, System.Threading.CancellationToken cancellationToken)
}

[
EditorBrowsableAttribute(),
ExtensionAttribute(),
]
public sealed class System.Web.OData.Batch.ODataBatchHttpRequestMessageExtensions {
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
public sealed class System.Web.OData.Batch.ODataBatchReaderExtensions {
	[
	ExtensionAttribute(),
	]
	public static System.Threading.Tasks.Task`1[[System.Net.Http.HttpRequestMessage]] ReadChangeSetOperationRequestAsync (Microsoft.OData.Core.ODataBatchReader reader, System.Guid batchId, System.Guid changeSetId, bool bufferContentStream)

	[
	ExtensionAttribute(),
	]
	public static System.Threading.Tasks.Task`1[[System.Net.Http.HttpRequestMessage]] ReadChangeSetOperationRequestAsync (Microsoft.OData.Core.ODataBatchReader reader, System.Guid batchId, System.Guid changeSetId, bool bufferContentStream, System.Threading.CancellationToken cancellationToken)

	[
	ExtensionAttribute(),
	]
	public static System.Threading.Tasks.Task`1[[System.Collections.Generic.IList`1[[System.Net.Http.HttpRequestMessage]]]] ReadChangeSetRequestAsync (Microsoft.OData.Core.ODataBatchReader reader, System.Guid batchId)

	[
	DebuggerStepThroughAttribute(),
	AsyncStateMachineAttribute(),
	ExtensionAttribute(),
	]
	public static System.Threading.Tasks.Task`1[[System.Collections.Generic.IList`1[[System.Net.Http.HttpRequestMessage]]]] ReadChangeSetRequestAsync (Microsoft.OData.Core.ODataBatchReader reader, System.Guid batchId, System.Threading.CancellationToken cancellationToken)

	[
	ExtensionAttribute(),
	]
	public static System.Threading.Tasks.Task`1[[System.Net.Http.HttpRequestMessage]] ReadOperationRequestAsync (Microsoft.OData.Core.ODataBatchReader reader, System.Guid batchId, bool bufferContentStream)

	[
	ExtensionAttribute(),
	]
	public static System.Threading.Tasks.Task`1[[System.Net.Http.HttpRequestMessage]] ReadOperationRequestAsync (Microsoft.OData.Core.ODataBatchReader reader, System.Guid batchId, bool bufferContentStream, System.Threading.CancellationToken cancellationToken)
}

[
EditorBrowsableAttribute(),
ExtensionAttribute(),
]
public sealed class System.Web.OData.Batch.ODataHttpContentExtensions {
	[
	ExtensionAttribute(),
	]
	public static System.Threading.Tasks.Task`1[[Microsoft.OData.Core.ODataMessageReader]] GetODataMessageReaderAsync (System.Net.Http.HttpContent content, Microsoft.OData.Core.ODataMessageReaderSettings settings)

	[
	DebuggerStepThroughAttribute(),
	AsyncStateMachineAttribute(),
	ExtensionAttribute(),
	]
	public static System.Threading.Tasks.Task`1[[Microsoft.OData.Core.ODataMessageReader]] GetODataMessageReaderAsync (System.Net.Http.HttpContent content, Microsoft.OData.Core.ODataMessageReaderSettings settings, System.Threading.CancellationToken cancellationToken)
}

public class System.Web.OData.Batch.ChangeSetRequestItem : ODataBatchRequestItem, IDisposable {
	public ChangeSetRequestItem (System.Collections.Generic.IEnumerable`1[[System.Net.Http.HttpRequestMessage]] requests)

	System.Collections.Generic.IEnumerable`1[[System.Net.Http.HttpRequestMessage]] Requests  { public get; }

	protected virtual void Dispose (bool disposing)
	public virtual System.Collections.Generic.IEnumerable`1[[System.IDisposable]] GetResourcesForDisposal ()
	[
	DebuggerStepThroughAttribute(),
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task`1[[System.Web.OData.Batch.ODataBatchResponseItem]] SendRequestAsync (System.Net.Http.HttpMessageInvoker invoker, System.Threading.CancellationToken cancellationToken)
}

public class System.Web.OData.Batch.ChangeSetResponseItem : ODataBatchResponseItem, IDisposable {
	public ChangeSetResponseItem (System.Collections.Generic.IEnumerable`1[[System.Net.Http.HttpResponseMessage]] responses)

	System.Collections.Generic.IEnumerable`1[[System.Net.Http.HttpResponseMessage]] Responses  { public get; }

	protected virtual void Dispose (bool disposing)
	internal virtual bool IsResponseSuccessful ()
	[
	DebuggerStepThroughAttribute(),
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task WriteResponseAsync (Microsoft.OData.Core.ODataBatchWriter writer, System.Threading.CancellationToken cancellationToken)
}

public class System.Web.OData.Batch.DefaultODataBatchHandler : ODataBatchHandler, IDisposable {
	public DefaultODataBatchHandler (System.Web.Http.HttpServer httpServer)

	[
	DebuggerStepThroughAttribute(),
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task`1[[System.Collections.Generic.IList`1[[System.Web.OData.Batch.ODataBatchResponseItem]]]] ExecuteRequestMessagesAsync (System.Collections.Generic.IEnumerable`1[[System.Web.OData.Batch.ODataBatchRequestItem]] requests, System.Threading.CancellationToken cancellationToken)

	[
	DebuggerStepThroughAttribute(),
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task`1[[System.Collections.Generic.IList`1[[System.Web.OData.Batch.ODataBatchRequestItem]]]] ParseBatchRequestsAsync (System.Net.Http.HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)

	[
	DebuggerStepThroughAttribute(),
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task`1[[System.Net.Http.HttpResponseMessage]] ProcessBatchAsync (System.Net.Http.HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
}

public class System.Web.OData.Batch.ODataBatchContent : System.Net.Http.HttpContent, IDisposable {
	public ODataBatchContent (System.Collections.Generic.IEnumerable`1[[System.Web.OData.Batch.ODataBatchResponseItem]] responses)
	public ODataBatchContent (System.Collections.Generic.IEnumerable`1[[System.Web.OData.Batch.ODataBatchResponseItem]] responses, Microsoft.OData.Core.ODataMessageWriterSettings writerSettings)

	System.Collections.Generic.IEnumerable`1[[System.Web.OData.Batch.ODataBatchResponseItem]] Responses  { public get; }

	protected virtual void Dispose (bool disposing)
	[
	DebuggerStepThroughAttribute(),
	AsyncStateMachineAttribute(),
	]
	protected virtual System.Threading.Tasks.Task SerializeToStreamAsync (System.IO.Stream stream, System.Net.TransportContext context)

	protected virtual bool TryComputeLength (out System.Int64& length)
}

public class System.Web.OData.Batch.OperationRequestItem : ODataBatchRequestItem, IDisposable {
	public OperationRequestItem (System.Net.Http.HttpRequestMessage request)

	System.Net.Http.HttpRequestMessage Request  { public get; }

	protected virtual void Dispose (bool disposing)
	public virtual System.Collections.Generic.IEnumerable`1[[System.IDisposable]] GetResourcesForDisposal ()
	[
	DebuggerStepThroughAttribute(),
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task`1[[System.Web.OData.Batch.ODataBatchResponseItem]] SendRequestAsync (System.Net.Http.HttpMessageInvoker invoker, System.Threading.CancellationToken cancellationToken)
}

public class System.Web.OData.Batch.OperationResponseItem : ODataBatchResponseItem, IDisposable {
	public OperationResponseItem (System.Net.Http.HttpResponseMessage response)

	System.Net.Http.HttpResponseMessage Response  { public get; }

	protected virtual void Dispose (bool disposing)
	internal virtual bool IsResponseSuccessful ()
	public virtual System.Threading.Tasks.Task WriteResponseAsync (Microsoft.OData.Core.ODataBatchWriter writer, System.Threading.CancellationToken cancellationToken)
}

public class System.Web.OData.Batch.UnbufferedODataBatchHandler : ODataBatchHandler, IDisposable {
	public UnbufferedODataBatchHandler (System.Web.Http.HttpServer httpServer)

	[
	DebuggerStepThroughAttribute(),
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task`1[[System.Web.OData.Batch.ODataBatchResponseItem]] ExecuteChangeSetAsync (Microsoft.OData.Core.ODataBatchReader batchReader, System.Guid batchId, System.Net.Http.HttpRequestMessage originalRequest, System.Threading.CancellationToken cancellationToken)

	[
	DebuggerStepThroughAttribute(),
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task`1[[System.Web.OData.Batch.ODataBatchResponseItem]] ExecuteOperationAsync (Microsoft.OData.Core.ODataBatchReader batchReader, System.Guid batchId, System.Net.Http.HttpRequestMessage originalRequest, System.Threading.CancellationToken cancellationToken)

	[
	DebuggerStepThroughAttribute(),
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task`1[[System.Net.Http.HttpResponseMessage]] ProcessBatchAsync (System.Net.Http.HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
}

[
FlagsAttribute(),
]
public enum System.Web.OData.Builder.NameResolverOptions : int {
	ProcessDataMemberAttributePropertyNames = 2
	ProcessExplicitPropertyNames = 4
	ProcessReflectedPropertyNames = 1
}

public enum System.Web.OData.Builder.ProcedureKind : int {
	Action = 0
	Function = 1
	ServiceOperation = 2
}

public enum System.Web.OData.Builder.PropertyKind : int {
	Collection = 2
	Complex = 1
	Dynamic = 5
	Enum = 4
	Navigation = 3
	Primitive = 0
}

public interface System.Web.OData.Builder.IEdmTypeConfiguration {
	System.Type ClrType  { public abstract get; }
	string FullName  { public abstract get; }
	Microsoft.OData.Edm.EdmTypeKind Kind  { public abstract get; }
	ODataModelBuilder ModelBuilder  { public abstract get; }
	string Name  { public abstract get; }
	string Namespace  { public abstract get; }
}

public interface System.Web.OData.Builder.INavigationSourceConfiguration {
	System.Collections.Generic.IEnumerable`1[[System.Web.OData.Builder.NavigationPropertyBindingConfiguration]] Bindings  { public abstract get; }
	System.Type ClrType  { public abstract get; }
	EntityTypeConfiguration EntityType  { public abstract get; }
	string Name  { public abstract get; }

	NavigationPropertyBindingConfiguration AddBinding (NavigationPropertyConfiguration navigationConfiguration, INavigationSourceConfiguration targetNavigationSource)
	NavigationPropertyBindingConfiguration FindBinding (string propertyName)
	NavigationPropertyBindingConfiguration FindBinding (NavigationPropertyConfiguration navigationConfiguration)
	NavigationPropertyBindingConfiguration FindBinding (NavigationPropertyConfiguration navigationConfiguration, bool autoCreate)
	System.Web.OData.Builder.SelfLinkBuilder`1[[System.Uri]] GetEditLink ()
	System.Web.OData.Builder.SelfLinkBuilder`1[[System.Uri]] GetIdLink ()
	NavigationLinkBuilder GetNavigationPropertyLink (NavigationPropertyConfiguration navigationProperty)
	System.Web.OData.Builder.SelfLinkBuilder`1[[System.Uri]] GetReadLink ()
	string GetUrl ()
	INavigationSourceConfiguration HasEditLink (System.Web.OData.Builder.SelfLinkBuilder`1[[System.Uri]] editLinkBuilder)
	INavigationSourceConfiguration HasIdLink (System.Web.OData.Builder.SelfLinkBuilder`1[[System.Uri]] idLinkBuilder)
	INavigationSourceConfiguration HasNavigationPropertiesLink (System.Collections.Generic.IEnumerable`1[[System.Web.OData.Builder.NavigationPropertyConfiguration]] navigationProperties, NavigationLinkBuilder navigationLinkBuilder)
	INavigationSourceConfiguration HasNavigationPropertyLink (NavigationPropertyConfiguration navigationProperty, NavigationLinkBuilder navigationLinkBuilder)
	INavigationSourceConfiguration HasReadLink (System.Web.OData.Builder.SelfLinkBuilder`1[[System.Uri]] readLinkBuilder)
	INavigationSourceConfiguration HasUrl (string url)
	void RemoveBinding (NavigationPropertyConfiguration navigationConfiguration)
}

public abstract class System.Web.OData.Builder.NavigationSourceConfiguration : INavigationSourceConfiguration {
	protected NavigationSourceConfiguration ()
	protected NavigationSourceConfiguration (ODataModelBuilder modelBuilder, System.Type entityClrType, string name)
	protected NavigationSourceConfiguration (ODataModelBuilder modelBuilder, EntityTypeConfiguration entityType, string name)

	System.Collections.Generic.IEnumerable`1[[System.Web.OData.Builder.NavigationPropertyBindingConfiguration]] Bindings  { public virtual get; }
	System.Type ClrType  { public virtual get; }
	EntityTypeConfiguration EntityType  { public virtual get; }
	string Name  { public virtual get; }

	public virtual NavigationPropertyBindingConfiguration AddBinding (NavigationPropertyConfiguration navigationConfiguration, INavigationSourceConfiguration targetNavigationSource)
	public virtual NavigationPropertyBindingConfiguration FindBinding (string propertyName)
	public virtual NavigationPropertyBindingConfiguration FindBinding (NavigationPropertyConfiguration navigationConfiguration)
	public virtual NavigationPropertyBindingConfiguration FindBinding (NavigationPropertyConfiguration navigationConfiguration, bool autoCreate)
	public virtual System.Web.OData.Builder.SelfLinkBuilder`1[[System.Uri]] GetEditLink ()
	public virtual System.Web.OData.Builder.SelfLinkBuilder`1[[System.Uri]] GetIdLink ()
	public virtual NavigationLinkBuilder GetNavigationPropertyLink (NavigationPropertyConfiguration navigationProperty)
	public virtual System.Web.OData.Builder.SelfLinkBuilder`1[[System.Uri]] GetReadLink ()
	public virtual string GetUrl ()
	public virtual INavigationSourceConfiguration HasEditLink (System.Web.OData.Builder.SelfLinkBuilder`1[[System.Uri]] editLinkBuilder)
	public virtual INavigationSourceConfiguration HasIdLink (System.Web.OData.Builder.SelfLinkBuilder`1[[System.Uri]] idLinkBuilder)
	public virtual INavigationSourceConfiguration HasNavigationPropertiesLink (System.Collections.Generic.IEnumerable`1[[System.Web.OData.Builder.NavigationPropertyConfiguration]] navigationProperties, NavigationLinkBuilder navigationLinkBuilder)
	public virtual INavigationSourceConfiguration HasNavigationPropertyLink (NavigationPropertyConfiguration navigationProperty, NavigationLinkBuilder navigationLinkBuilder)
	public virtual INavigationSourceConfiguration HasReadLink (System.Web.OData.Builder.SelfLinkBuilder`1[[System.Uri]] readLinkBuilder)
	public virtual INavigationSourceConfiguration HasUrl (string url)
	public virtual void RemoveBinding (NavigationPropertyConfiguration navigationConfiguration)
}

public abstract class System.Web.OData.Builder.NavigationSourceConfiguration`1 {
	EntityTypeConfiguration`1 EntityType  { public get; }

	public NavigationPropertyBindingConfiguration FindBinding (string propertyName)
	public NavigationPropertyBindingConfiguration FindBinding (NavigationPropertyConfiguration navigationConfiguration)
	public NavigationPropertyBindingConfiguration FindBinding (NavigationPropertyConfiguration navigationConfiguration, bool autoCreate)
	public void HasEditLink (Func`2 editLinkFactory, bool followsConventions)
	public void HasIdLink (Func`2 idLinkFactory, bool followsConventions)
	public NavigationPropertyBindingConfiguration HasManyBinding (Expression`1 navigationExpression, NavigationSourceConfiguration`1 targetEntitySet)
	public NavigationPropertyBindingConfiguration HasManyBinding (Expression`1 navigationExpression, NavigationSourceConfiguration`1 targetEntitySet)
	public NavigationPropertyBindingConfiguration HasManyBinding (Expression`1 navigationExpression, string entitySetName)
	public NavigationPropertyBindingConfiguration HasManyBinding (Expression`1 navigationExpression, string entitySetName)
	public void HasNavigationPropertiesLink (System.Collections.Generic.IEnumerable`1[[System.Web.OData.Builder.NavigationPropertyConfiguration]] navigationProperties, Func`3 navigationLinkFactory, bool followsConventions)
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

public abstract class System.Web.OData.Builder.ParameterConfiguration {
	protected ParameterConfiguration (string name, IEdmTypeConfiguration parameterType)

	string Name  { public get; protected set; }
	bool OptionalParameter  { public get; public set; }
	IEdmTypeConfiguration TypeConfiguration  { public get; protected set; }
}

public abstract class System.Web.OData.Builder.ProcedureConfiguration {
	BindingParameterConfiguration BindingParameter  { public virtual get; }
	System.Collections.Generic.IEnumerable`1[[System.String]] EntitySetPath  { public get; }
	bool FollowsConventions  { public get; protected set; }
	string FullyQualifiedName  { public get; }
	bool IsBindable  { public virtual get; }
	bool IsComposable  { public virtual get; }
	bool IsSideEffecting  { public abstract get; }
	ProcedureKind Kind  { public abstract get; }
	System.Func`2[[System.Web.OData.EntityInstanceContext],[System.Uri]] LinkFactory  { protected get; protected set; }
	ODataModelBuilder ModelBuilder  { protected get; protected set; }
	string Name  { public get; protected set; }
	string Namespace  { public get; public set; }
	NavigationSourceConfiguration NavigationSource  { public get; public set; }
	bool OptionalReturn  { public get; public set; }
	System.Collections.Generic.IEnumerable`1[[System.Web.OData.Builder.ParameterConfiguration]] Parameters  { public virtual get; }
	ProcedureLinkBuilder ProcedureLinkBuilder  { protected get; protected set; }
	IEdmTypeConfiguration ReturnType  { public get; public set; }
	string Title  { public get; public set; }

	public ParameterConfiguration AddParameter (string name, IEdmTypeConfiguration parameterType)
	public ParameterConfiguration CollectionEntityParameter (string name)
	public ParameterConfiguration CollectionParameter (string name)
	public ParameterConfiguration EntityParameter (string name)
	public ParameterConfiguration Parameter (string name)
	public ParameterConfiguration Parameter (System.Type clrParameterType, string name)
}

public abstract class System.Web.OData.Builder.PropertyConfiguration {
	protected PropertyConfiguration (System.Reflection.PropertyInfo property, StructuralTypeConfiguration declaringType)

	bool AddedExplicitly  { public get; public set; }
	bool AutoExpand  { public get; public set; }
	StructuralTypeConfiguration DeclaringType  { public get; }
	bool IsRestricted  { public get; }
	PropertyKind Kind  { public abstract get; }
	string Name  { public get; public set; }
	bool NonFilterable  { public get; public set; }
	bool NotCountable  { public get; public set; }
	bool NotExpandable  { public get; public set; }
	bool NotFilterable  { public get; public set; }
	bool NotNavigable  { public get; public set; }
	bool NotSortable  { public get; public set; }
	System.Reflection.PropertyInfo PropertyInfo  { public get; }
	System.Type RelatedClrType  { public abstract get; }
	bool Unsortable  { public get; public set; }

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
}

public abstract class System.Web.OData.Builder.StructuralPropertyConfiguration : PropertyConfiguration {
	protected StructuralPropertyConfiguration (System.Reflection.PropertyInfo property, StructuralTypeConfiguration declaringType)

	bool ConcurrencyToken  { public get; public set; }
	bool OptionalProperty  { public get; public set; }
}

public abstract class System.Web.OData.Builder.StructuralTypeConfiguration : IEdmTypeConfiguration {
	protected StructuralTypeConfiguration ()
	protected StructuralTypeConfiguration (ODataModelBuilder modelBuilder, System.Type clrType)

	bool AddedExplicitly  { public get; public set; }
	bool BaseTypeConfigured  { public virtual get; }
	StructuralTypeConfiguration BaseTypeInternal  { protected virtual get; }
	System.Type ClrType  { public virtual get; }
	System.Reflection.PropertyInfo DynamicPropertyDictionary  { public get; }
	System.Collections.Generic.IDictionary`2[[System.Reflection.PropertyInfo],[System.Web.OData.Builder.PropertyConfiguration]] ExplicitProperties  { protected get; }
	string FullName  { public virtual get; }
	System.Collections.ObjectModel.ReadOnlyCollection`1[[System.Reflection.PropertyInfo]] IgnoredProperties  { public get; }
	System.Nullable`1[[System.Boolean]] IsAbstract  { public virtual get; public virtual set; }
	bool IsOpen  { public get; }
	Microsoft.OData.Edm.EdmTypeKind Kind  { public abstract get; }
	ODataModelBuilder ModelBuilder  { public virtual get; }
	string Name  { public virtual get; public virtual set; }
	string Namespace  { public virtual get; public virtual set; }
	System.Collections.Generic.IEnumerable`1[[System.Web.OData.Builder.PropertyConfiguration]] Properties  { public get; }
	System.Collections.Generic.IList`1[[System.Reflection.PropertyInfo]] RemovedProperties  { protected get; }

	internal virtual void AbstractImpl ()
	public virtual CollectionPropertyConfiguration AddCollectionProperty (System.Reflection.PropertyInfo propertyInfo)
	public virtual ComplexPropertyConfiguration AddComplexProperty (System.Reflection.PropertyInfo propertyInfo)
	public virtual void AddDynamicPropertyDictionary (System.Reflection.PropertyInfo propertyInfo)
	public virtual EnumPropertyConfiguration AddEnumProperty (System.Reflection.PropertyInfo propertyInfo)
	public virtual PrimitivePropertyConfiguration AddProperty (System.Reflection.PropertyInfo propertyInfo)
	internal virtual void DerivesFromImpl (StructuralTypeConfiguration baseType)
	internal virtual void DerivesFromNothingImpl ()
	public virtual void RemoveProperty (System.Reflection.PropertyInfo propertyInfo)
}

public abstract class System.Web.OData.Builder.StructuralTypeConfiguration`1 {
	protected StructuralTypeConfiguration`1 (StructuralTypeConfiguration configuration)

	string FullName  { public get; }
	bool IsOpen  { public get; }
	string Name  { public get; public set; }
	string Namespace  { public get; public set; }
	System.Collections.Generic.IEnumerable`1[[System.Web.OData.Builder.PropertyConfiguration]] Properties  { public get; }

	public CollectionPropertyConfiguration CollectionProperty (Expression`1 propertyExpression)
	public ComplexPropertyConfiguration ComplexProperty (Expression`1 propertyExpression)
	public EnumPropertyConfiguration EnumProperty (Expression`1 propertyExpression)
	public EnumPropertyConfiguration EnumProperty (Expression`1 propertyExpression)
	public void HasDynamicProperties (Expression`1 propertyExpression)
	public virtual void Ignore (Expression`1 propertyExpression)
	public PrimitivePropertyConfiguration Property (Expression`1 propertyExpression)
	public PrimitivePropertyConfiguration Property (Expression`1 propertyExpression)
	public PrimitivePropertyConfiguration Property (Expression`1 propertyExpression)
	public PrimitivePropertyConfiguration Property (Expression`1 propertyExpression)
	public PrimitivePropertyConfiguration Property (Expression`1 propertyExpression)
}

[
EditorBrowsableAttribute(),
ExtensionAttribute(),
]
public sealed class System.Web.OData.Builder.LinkGenerationHelpers {
	[
	ExtensionAttribute(),
	]
	public static System.Uri GenerateActionLink (EntityInstanceContext entityContext, Microsoft.OData.Edm.IEdmOperation action)

	[
	ExtensionAttribute(),
	]
	public static System.Uri GenerateActionLink (FeedContext feedContext, Microsoft.OData.Edm.IEdmOperation action)

	[
	ExtensionAttribute(),
	]
	public static System.Uri GenerateFunctionLink (EntityInstanceContext entityContext, Microsoft.OData.Edm.IEdmOperation function)

	[
	ExtensionAttribute(),
	]
	public static System.Uri GenerateFunctionLink (FeedContext feedContext, Microsoft.OData.Edm.IEdmOperation function)

	[
	ExtensionAttribute(),
	]
	public static System.Uri GenerateNavigationPropertyLink (EntityInstanceContext entityContext, Microsoft.OData.Edm.IEdmNavigationProperty navigationProperty, bool includeCast)

	[
	ExtensionAttribute(),
	]
	public static System.Uri GenerateSelfLink (EntityInstanceContext entityContext, bool includeCast)
}

[
EditorBrowsableAttribute(),
ExtensionAttribute(),
]
public sealed class System.Web.OData.Builder.ODataConventionModelBuilderExtensions {
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
public sealed class System.Web.OData.Builder.PrimitivePropertyConfigurationExtensions {
	[
	ExtensionAttribute(),
	]
	public static PrimitivePropertyConfiguration AsDate (PrimitivePropertyConfiguration property)

	[
	ExtensionAttribute(),
	]
	public static PrimitivePropertyConfiguration AsTimeOfDay (PrimitivePropertyConfiguration property)
}

public class System.Web.OData.Builder.ActionConfiguration : ProcedureConfiguration {
	bool IsSideEffecting  { public virtual get; }
	ProcedureKind Kind  { public virtual get; }

	public System.Func`2[[System.Web.OData.EntityInstanceContext],[System.Uri]] GetActionLink ()
	public System.Func`2[[System.Web.OData.FeedContext],[System.Uri]] GetFeedActionLink ()
	public ActionConfiguration HasActionLink (System.Func`2[[System.Web.OData.EntityInstanceContext],[System.Uri]] actionLinkFactory, bool followsConventions)
	public ActionConfiguration HasFeedActionLink (System.Func`2[[System.Web.OData.FeedContext],[System.Uri]] actionLinkFactory, bool followsConventions)
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

public class System.Web.OData.Builder.ActionLinkBuilder : ProcedureLinkBuilder {
	public ActionLinkBuilder (System.Func`2[[System.Web.OData.EntityInstanceContext],[System.Uri]] actionLinkFactory, bool followsConventions)
	public ActionLinkBuilder (System.Func`2[[System.Web.OData.FeedContext],[System.Uri]] actionLinkFactory, bool followsConventions)

	public virtual System.Uri BuildActionLink (EntityInstanceContext context)
	public virtual System.Uri BuildActionLink (FeedContext context)
	public static System.Func`2[[System.Web.OData.EntityInstanceContext],[System.Uri]] CreateActionLinkFactory (System.Func`2[[System.Web.OData.EntityInstanceContext],[System.Uri]] baseFactory, System.Func`2[[System.Web.OData.EntityInstanceContext],[System.Boolean]] expensiveAvailabilityCheck)
}

public class System.Web.OData.Builder.BindingParameterConfiguration : ParameterConfiguration {
	public static string DefaultBindingParameterName = "bindingParameter"

	public BindingParameterConfiguration (string name, IEdmTypeConfiguration parameterType)
}

public class System.Web.OData.Builder.CollectionPropertyConfiguration : StructuralPropertyConfiguration {
	public CollectionPropertyConfiguration (System.Reflection.PropertyInfo property, StructuralTypeConfiguration declaringType)

	System.Type ElementType  { public get; }
	PropertyKind Kind  { public virtual get; }
	System.Type RelatedClrType  { public virtual get; }

	public CollectionPropertyConfiguration IsOptional ()
	public CollectionPropertyConfiguration IsRequired ()
}

public class System.Web.OData.Builder.CollectionTypeConfiguration : IEdmTypeConfiguration {
	public CollectionTypeConfiguration (IEdmTypeConfiguration elementType, System.Type clrType)

	System.Type ClrType  { public virtual get; }
	IEdmTypeConfiguration ElementType  { public get; }
	string FullName  { public virtual get; }
	Microsoft.OData.Edm.EdmTypeKind Kind  { public virtual get; }
	ODataModelBuilder ModelBuilder  { public virtual get; }
	string Name  { public virtual get; }
	string Namespace  { public virtual get; }
}

public class System.Web.OData.Builder.ComplexPropertyConfiguration : StructuralPropertyConfiguration {
	public ComplexPropertyConfiguration (System.Reflection.PropertyInfo property, StructuralTypeConfiguration declaringType)

	PropertyKind Kind  { public virtual get; }
	System.Type RelatedClrType  { public virtual get; }

	public ComplexPropertyConfiguration IsOptional ()
	public ComplexPropertyConfiguration IsRequired ()
}

public class System.Web.OData.Builder.ComplexTypeConfiguration : StructuralTypeConfiguration, IEdmTypeConfiguration {
	public ComplexTypeConfiguration ()
	public ComplexTypeConfiguration (ODataModelBuilder modelBuilder, System.Type clrType)

	ComplexTypeConfiguration BaseType  { public virtual get; public virtual set; }
	Microsoft.OData.Edm.EdmTypeKind Kind  { public virtual get; }

	public virtual ComplexTypeConfiguration Abstract ()
	public virtual ComplexTypeConfiguration DerivesFrom (ComplexTypeConfiguration baseType)
	public virtual ComplexTypeConfiguration DerivesFromNothing ()
}

public class System.Web.OData.Builder.ComplexTypeConfiguration`1 : StructuralTypeConfiguration`1 {
	ComplexTypeConfiguration BaseType  { public get; }

	public ComplexTypeConfiguration`1 Abstract ()
	public ComplexTypeConfiguration`1 DerivesFrom ()
	public ComplexTypeConfiguration`1 DerivesFromNothing ()
}

public class System.Web.OData.Builder.DynamicPropertyDictionaryAnnotation {
	public DynamicPropertyDictionaryAnnotation (System.Reflection.PropertyInfo propertyInfo)

	System.Reflection.PropertyInfo PropertyInfo  { public get; }
}

public class System.Web.OData.Builder.EntityCollectionConfiguration`1 : CollectionTypeConfiguration, IEdmTypeConfiguration {
	public ActionConfiguration Action (string name)
	public FunctionConfiguration Function (string name)
}

public class System.Web.OData.Builder.EntitySetConfiguration : NavigationSourceConfiguration, INavigationSourceConfiguration {
	public EntitySetConfiguration ()
	public EntitySetConfiguration (ODataModelBuilder modelBuilder, System.Type entityClrType, string name)
	public EntitySetConfiguration (ODataModelBuilder modelBuilder, EntityTypeConfiguration entityType, string name)

	public virtual System.Func`2[[System.Web.OData.FeedContext],[System.Uri]] GetFeedSelfLink ()
	public virtual INavigationSourceConfiguration HasFeedSelfLink (System.Func`2[[System.Web.OData.FeedContext],[System.Uri]] feedSelfLinkFactory)
}

public class System.Web.OData.Builder.EntitySetConfiguration`1 : NavigationSourceConfiguration`1 {
	public virtual void HasFeedSelfLink (System.Func`2[[System.Web.OData.FeedContext],[System.String]] feedSelfLinkFactory)
	public virtual void HasFeedSelfLink (System.Func`2[[System.Web.OData.FeedContext],[System.Uri]] feedSelfLinkFactory)
}

public class System.Web.OData.Builder.EntityTypeConfiguration : StructuralTypeConfiguration, IEdmTypeConfiguration {
	public EntityTypeConfiguration ()
	public EntityTypeConfiguration (ODataModelBuilder modelBuilder, System.Type clrType)

	EntityTypeConfiguration BaseType  { public virtual get; public virtual set; }
	System.Collections.Generic.IEnumerable`1[[System.Web.OData.Builder.EnumPropertyConfiguration]] EnumKeys  { public virtual get; }
	bool HasStream  { public virtual get; public virtual set; }
	System.Collections.Generic.IEnumerable`1[[System.Web.OData.Builder.PrimitivePropertyConfiguration]] Keys  { public virtual get; }
	Microsoft.OData.Edm.EdmTypeKind Kind  { public virtual get; }
	System.Collections.Generic.IEnumerable`1[[System.Web.OData.Builder.NavigationPropertyConfiguration]] NavigationProperties  { public virtual get; }

	public virtual EntityTypeConfiguration Abstract ()
	public virtual NavigationPropertyConfiguration AddContainedNavigationProperty (System.Reflection.PropertyInfo navigationProperty, Microsoft.OData.Edm.EdmMultiplicity multiplicity)
	public virtual NavigationPropertyConfiguration AddNavigationProperty (System.Reflection.PropertyInfo navigationProperty, Microsoft.OData.Edm.EdmMultiplicity multiplicity)
	public virtual EntityTypeConfiguration DerivesFrom (EntityTypeConfiguration baseType)
	public virtual EntityTypeConfiguration DerivesFromNothing ()
	public virtual EntityTypeConfiguration HasKey (System.Reflection.PropertyInfo keyProperty)
	public virtual EntityTypeConfiguration MediaType ()
	public virtual void RemoveKey (EnumPropertyConfiguration enumKeyProperty)
	public virtual void RemoveKey (PrimitivePropertyConfiguration keyProperty)
	public virtual void RemoveProperty (System.Reflection.PropertyInfo propertyInfo)
}

public class System.Web.OData.Builder.EntityTypeConfiguration`1 : StructuralTypeConfiguration`1 {
	EntityTypeConfiguration BaseType  { public get; }
	EntityCollectionConfiguration`1 Collection  { public get; }
	System.Collections.Generic.IEnumerable`1[[System.Web.OData.Builder.NavigationPropertyConfiguration]] NavigationProperties  { public get; }

	public EntityTypeConfiguration`1 Abstract ()
	public ActionConfiguration Action (string name)
	public NavigationPropertyConfiguration ContainsMany (Expression`1 navigationPropertyExpression)
	public NavigationPropertyConfiguration ContainsOptional (Expression`1 navigationPropertyExpression)
	public NavigationPropertyConfiguration ContainsRequired (Expression`1 navigationPropertyExpression)
	public EntityTypeConfiguration`1 DerivesFrom ()
	public EntityTypeConfiguration`1 DerivesFromNothing ()
	public FunctionConfiguration Function (string name)
	public EntityTypeConfiguration`1 HasKey (Expression`1 keyDefinitionExpression)
	public NavigationPropertyConfiguration HasMany (Expression`1 navigationPropertyExpression)
	public NavigationPropertyConfiguration HasOptional (Expression`1 navigationPropertyExpression)
	public NavigationPropertyConfiguration HasOptional (Expression`1 navigationPropertyExpression, Expression`1 referentialConstraintExpression)
	public NavigationPropertyConfiguration HasRequired (Expression`1 navigationPropertyExpression)
	public NavigationPropertyConfiguration HasRequired (Expression`1 navigationPropertyExpression, Expression`1 referentialConstraintExpression)
	public EntityTypeConfiguration`1 MediaType ()
}

public class System.Web.OData.Builder.EnumMemberConfiguration {
	public EnumMemberConfiguration (System.Enum member, EnumTypeConfiguration declaringType)

	bool AddedExplicitly  { public get; public set; }
	EnumTypeConfiguration DeclaringType  { public get; }
	System.Enum MemberInfo  { public get; }
	string Name  { public get; public set; }
}

public class System.Web.OData.Builder.EnumPropertyConfiguration : StructuralPropertyConfiguration {
	public EnumPropertyConfiguration (System.Reflection.PropertyInfo property, StructuralTypeConfiguration declaringType)

	PropertyKind Kind  { public virtual get; }
	System.Type RelatedClrType  { public virtual get; }

	public EnumPropertyConfiguration IsConcurrencyToken ()
	public EnumPropertyConfiguration IsOptional ()
	public EnumPropertyConfiguration IsRequired ()
}

public class System.Web.OData.Builder.EnumTypeConfiguration : IEdmTypeConfiguration {
	public EnumTypeConfiguration (ODataModelBuilder builder, System.Type clrType)

	bool AddedExplicitly  { public get; public set; }
	System.Type ClrType  { public virtual get; }
	System.Collections.Generic.IDictionary`2[[System.Enum],[System.Web.OData.Builder.EnumMemberConfiguration]] ExplicitMembers  { protected get; }
	string FullName  { public virtual get; }
	System.Collections.ObjectModel.ReadOnlyCollection`1[[System.Enum]] IgnoredMembers  { public get; }
	bool IsFlags  { public get; }
	Microsoft.OData.Edm.EdmTypeKind Kind  { public virtual get; }
	System.Collections.Generic.IEnumerable`1[[System.Web.OData.Builder.EnumMemberConfiguration]] Members  { public get; }
	ODataModelBuilder ModelBuilder  { public virtual get; }
	string Name  { public virtual get; public set; }
	string Namespace  { public virtual get; public set; }
	System.Collections.Generic.IList`1[[System.Enum]] RemovedMembers  { protected get; }
	System.Type UnderlyingType  { public get; }

	public EnumMemberConfiguration AddMember (System.Enum member)
	public void RemoveMember (System.Enum member)
}

public class System.Web.OData.Builder.EnumTypeConfiguration`1 {
	string FullName  { public get; }
	System.Collections.Generic.IEnumerable`1[[System.Web.OData.Builder.EnumMemberConfiguration]] Members  { public get; }
	string Name  { public get; public set; }
	string Namespace  { public get; public set; }

	public EnumMemberConfiguration Member (TEnumType enumMember)
	public virtual void RemoveMember (TEnumType member)
}

public class System.Web.OData.Builder.FunctionConfiguration : ProcedureConfiguration {
	bool IncludeInServiceDocument  { public get; public set; }
	bool IsComposable  { public get; public set; }
	bool IsSideEffecting  { public virtual get; }
	ProcedureKind Kind  { public virtual get; }
	bool SupportedInFilter  { public get; public set; }
	bool SupportedInOrderBy  { public get; public set; }

	public System.Func`2[[System.Web.OData.FeedContext],[System.Uri]] GetFeedFunctionLink ()
	public System.Func`2[[System.Web.OData.EntityInstanceContext],[System.Uri]] GetFunctionLink ()
	public FunctionConfiguration HasFeedFunctionLink (System.Func`2[[System.Web.OData.FeedContext],[System.Uri]] functionLinkFactory, bool followsConventions)
	public FunctionConfiguration HasFunctionLink (System.Func`2[[System.Web.OData.EntityInstanceContext],[System.Uri]] functionLinkFactory, bool followsConventions)
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

public class System.Web.OData.Builder.FunctionLinkBuilder : ProcedureLinkBuilder {
	public FunctionLinkBuilder (System.Func`2[[System.Web.OData.EntityInstanceContext],[System.Uri]] functionLinkFactory, bool followsConventions)
	public FunctionLinkBuilder (System.Func`2[[System.Web.OData.FeedContext],[System.Uri]] functionLinkFactory, bool followsConventions)

	public virtual System.Uri BuildFunctionLink (EntityInstanceContext context)
	public virtual System.Uri BuildFunctionLink (FeedContext context)
	public static System.Func`2[[System.Web.OData.EntityInstanceContext],[System.Uri]] CreateFunctionLinkFactory (System.Func`2[[System.Web.OData.EntityInstanceContext],[System.Uri]] baseFactory, System.Func`2[[System.Web.OData.EntityInstanceContext],[System.Boolean]] expensiveAvailabilityCheck)
}

public class System.Web.OData.Builder.LowerCamelCaser {
	public LowerCamelCaser ()
	public LowerCamelCaser (NameResolverOptions options)

	public void ApplyLowerCamelCase (ODataConventionModelBuilder builder)
	public virtual string ToLowerCamelCase (string name)
}

public class System.Web.OData.Builder.NavigationLinkBuilder {
	public NavigationLinkBuilder (System.Func`3[[System.Web.OData.EntityInstanceContext],[Microsoft.OData.Edm.IEdmNavigationProperty],[System.Uri]] navigationLinkFactory, bool followsConventions)

	System.Func`3[[System.Web.OData.EntityInstanceContext],[Microsoft.OData.Edm.IEdmNavigationProperty],[System.Uri]] Factory  { public get; }
	bool FollowsConventions  { public get; }
}

public class System.Web.OData.Builder.NavigationPropertyBindingConfiguration {
	public NavigationPropertyBindingConfiguration (NavigationPropertyConfiguration navigationProperty, INavigationSourceConfiguration navigationSource)

	NavigationPropertyConfiguration NavigationProperty  { public get; }
	INavigationSourceConfiguration TargetNavigationSource  { public get; }
}

public class System.Web.OData.Builder.NavigationPropertyConfiguration : PropertyConfiguration {
	public NavigationPropertyConfiguration (System.Reflection.PropertyInfo property, Microsoft.OData.Edm.EdmMultiplicity multiplicity, EntityTypeConfiguration declaringType)

	bool ContainsTarget  { public get; }
	EntityTypeConfiguration DeclaringEntityType  { public get; }
	System.Collections.Generic.IEnumerable`1[[System.Reflection.PropertyInfo]] DependentProperties  { public get; }
	PropertyKind Kind  { public virtual get; }
	Microsoft.OData.Edm.EdmMultiplicity Multiplicity  { public get; }
	Microsoft.OData.Edm.EdmOnDeleteAction OnDeleteAction  { public get; public set; }
	System.Collections.Generic.IEnumerable`1[[System.Reflection.PropertyInfo]] PrincipalProperties  { public get; }
	System.Type RelatedClrType  { public virtual get; }

	public NavigationPropertyConfiguration AutomaticallyExpand ()
	public NavigationPropertyConfiguration CascadeOnDelete ()
	public NavigationPropertyConfiguration CascadeOnDelete (bool cascade)
	public NavigationPropertyConfiguration Contained ()
	public NavigationPropertyConfiguration HasConstraint (System.Collections.Generic.KeyValuePair`2[[System.Reflection.PropertyInfo],[System.Reflection.PropertyInfo]] constraint)
	public NavigationPropertyConfiguration HasConstraint (System.Reflection.PropertyInfo dependentPropertyInfo, System.Reflection.PropertyInfo principalPropertyInfo)
	public NavigationPropertyConfiguration NonContained ()
	public NavigationPropertyConfiguration Optional ()
	public NavigationPropertyConfiguration Required ()
}

public class System.Web.OData.Builder.NavigationSourceLinkBuilderAnnotation {
	public NavigationSourceLinkBuilderAnnotation ()
	public NavigationSourceLinkBuilderAnnotation (NavigationSourceConfiguration navigationSource)
	public NavigationSourceLinkBuilderAnnotation (Microsoft.OData.Edm.IEdmNavigationSource navigationSource, Microsoft.OData.Edm.IEdmModel model)
	public NavigationSourceLinkBuilderAnnotation (Microsoft.OData.Edm.IEdmNavigationSource navigationSource, System.Web.OData.Builder.SelfLinkBuilder`1[[System.Uri]] idLinkBuilder, System.Web.OData.Builder.SelfLinkBuilder`1[[System.Uri]] editLinkBuilder, System.Web.OData.Builder.SelfLinkBuilder`1[[System.Uri]] readLinkBuilder)

	public void AddNavigationPropertyLinkBuilder (Microsoft.OData.Edm.IEdmNavigationProperty navigationProperty, NavigationLinkBuilder linkBuilder)
	public virtual System.Uri BuildEditLink (EntityInstanceContext instanceContext, ODataMetadataLevel metadataLevel, System.Uri idLink)
	public virtual EntitySelfLinks BuildEntitySelfLinks (EntityInstanceContext instanceContext, ODataMetadataLevel metadataLevel)
	public virtual System.Uri BuildIdLink (EntityInstanceContext instanceContext, ODataMetadataLevel metadataLevel)
	public virtual System.Uri BuildNavigationLink (EntityInstanceContext instanceContext, Microsoft.OData.Edm.IEdmNavigationProperty navigationProperty, ODataMetadataLevel metadataLevel)
	public virtual System.Uri BuildReadLink (EntityInstanceContext instanceContext, ODataMetadataLevel metadataLevel, System.Uri editLink)
}

public class System.Web.OData.Builder.NonbindingParameterConfiguration : ParameterConfiguration {
	public NonbindingParameterConfiguration (string name, IEdmTypeConfiguration parameterType)
}

public class System.Web.OData.Builder.ODataConventionModelBuilder : ODataModelBuilder {
	public ODataConventionModelBuilder ()
	public ODataConventionModelBuilder (System.Web.Http.HttpConfiguration configuration)
	public ODataConventionModelBuilder (System.Web.Http.HttpConfiguration configuration, bool isQueryCompositionMode)

	bool ModelAliasingEnabled  { public get; public set; }
	System.Action`1[[System.Web.OData.Builder.ODataConventionModelBuilder]] OnModelCreating  { public get; public set; }

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

public class System.Web.OData.Builder.ODataModelBuilder {
	public ODataModelBuilder ()

	string ContainerName  { public get; public set; }
	System.Version DataServiceVersion  { public get; public set; }
	System.Collections.Generic.IEnumerable`1[[System.Web.OData.Builder.EntitySetConfiguration]] EntitySets  { public virtual get; }
	System.Collections.Generic.IEnumerable`1[[System.Web.OData.Builder.EnumTypeConfiguration]] EnumTypes  { public virtual get; }
	System.Version MaxDataServiceVersion  { public get; public set; }
	string Namespace  { public get; public set; }
	System.Collections.Generic.IEnumerable`1[[System.Web.OData.Builder.INavigationSourceConfiguration]] NavigationSources  { public virtual get; }
	System.Collections.Generic.IEnumerable`1[[System.Web.OData.Builder.ProcedureConfiguration]] Procedures  { public virtual get; }
	System.Collections.Generic.IEnumerable`1[[System.Web.OData.Builder.SingletonConfiguration]] Singletons  { public virtual get; }
	System.Collections.Generic.IEnumerable`1[[System.Web.OData.Builder.StructuralTypeConfiguration]] StructuralTypes  { public virtual get; }

	public virtual ActionConfiguration Action (string name)
	public virtual ComplexTypeConfiguration AddComplexType (System.Type type)
	public virtual EntitySetConfiguration AddEntitySet (string name, EntityTypeConfiguration entityType)
	public virtual EntityTypeConfiguration AddEntityType (System.Type type)
	public virtual EnumTypeConfiguration AddEnumType (System.Type type)
	public virtual void AddProcedure (ProcedureConfiguration procedure)
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
	public virtual bool RemoveProcedure (string name)
	public virtual bool RemoveProcedure (ProcedureConfiguration procedure)
	public virtual bool RemoveSingleton (string name)
	public virtual bool RemoveStructuralType (System.Type type)
	public SingletonConfiguration`1 Singleton (string name)
	public virtual void ValidateModel (Microsoft.OData.Edm.IEdmModel model)
}

public class System.Web.OData.Builder.PrimitivePropertyConfiguration : StructuralPropertyConfiguration {
	public PrimitivePropertyConfiguration (System.Reflection.PropertyInfo property, StructuralTypeConfiguration declaringType)

	PropertyKind Kind  { public virtual get; }
	System.Type RelatedClrType  { public virtual get; }
	System.Nullable`1[[Microsoft.OData.Edm.EdmPrimitiveTypeKind]] TargetEdmTypeKind  { public get; }

	public PrimitivePropertyConfiguration IsConcurrencyToken ()
	public PrimitivePropertyConfiguration IsOptional ()
	public PrimitivePropertyConfiguration IsRequired ()
}

public class System.Web.OData.Builder.PrimitiveTypeConfiguration : IEdmTypeConfiguration {
	public PrimitiveTypeConfiguration (ODataModelBuilder builder, Microsoft.OData.Edm.IEdmPrimitiveType edmType, System.Type clrType)

	System.Type ClrType  { public virtual get; }
	Microsoft.OData.Edm.IEdmPrimitiveType EdmPrimitiveType  { public get; }
	string FullName  { public virtual get; }
	Microsoft.OData.Edm.EdmTypeKind Kind  { public virtual get; }
	ODataModelBuilder ModelBuilder  { public virtual get; }
	string Name  { public virtual get; }
	string Namespace  { public virtual get; }
}

public class System.Web.OData.Builder.ProcedureLinkBuilder {
	public ProcedureLinkBuilder (System.Func`2[[System.Web.OData.EntityInstanceContext],[System.Uri]] linkFactory, bool followsConventions)
	public ProcedureLinkBuilder (System.Func`2[[System.Web.OData.FeedContext],[System.Uri]] linkFactory, bool followsConventions)

	System.Func`2[[System.Web.OData.FeedContext],[System.Uri]] FeedLinkFactory  { public get; }
	bool FollowsConventions  { public get; }
	System.Func`2[[System.Web.OData.EntityInstanceContext],[System.Uri]] LinkFactory  { public get; }

	public virtual System.Uri BuildLink (EntityInstanceContext context)
	public virtual System.Uri BuildLink (FeedContext context)
}

public class System.Web.OData.Builder.SelfLinkBuilder`1 {
	public SelfLinkBuilder`1 (Func`2 linkFactory, bool followsConventions)

	Func`2 Factory  { public get; }
	bool FollowsConventions  { public get; }
}

public class System.Web.OData.Builder.SingletonConfiguration : NavigationSourceConfiguration, INavigationSourceConfiguration {
	public SingletonConfiguration ()
	public SingletonConfiguration (ODataModelBuilder modelBuilder, System.Type entityClrType, string name)
	public SingletonConfiguration (ODataModelBuilder modelBuilder, EntityTypeConfiguration entityType, string name)
}

public class System.Web.OData.Builder.SingletonConfiguration`1 : NavigationSourceConfiguration`1 {
}

[
AttributeUsageAttribute(),
]
public sealed class System.Web.OData.Builder.ActionOnDeleteAttribute : System.Attribute, _Attribute {
	public ActionOnDeleteAttribute (Microsoft.OData.Edm.EdmOnDeleteAction onDeleteAction)

	Microsoft.OData.Edm.EdmOnDeleteAction OnDeleteAction  { public get; }
}

[
AttributeUsageAttribute(),
]
public sealed class System.Web.OData.Builder.AutoExpandAttribute : System.Attribute, _Attribute {
	public AutoExpandAttribute ()
}

[
AttributeUsageAttribute(),
]
public sealed class System.Web.OData.Builder.ContainedAttribute : System.Attribute, _Attribute {
	public ContainedAttribute ()
}

[
AttributeUsageAttribute(),
]
public sealed class System.Web.OData.Builder.MediaTypeAttribute : System.Attribute, _Attribute {
	public MediaTypeAttribute ()
}

[
AttributeUsageAttribute(),
]
public sealed class System.Web.OData.Builder.SingletonAttribute : System.Attribute, _Attribute {
	public SingletonAttribute ()
}

[
EditorBrowsableAttribute(),
ExtensionAttribute(),
]
public sealed class System.Web.OData.Extensions.HttpConfigurationExtensions {
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
	public static void EnableAlternateKeys (System.Web.Http.HttpConfiguration configuration, bool alternateKeys)

	[
	ExtensionAttribute(),
	]
	public static void EnableCaseInsensitive (System.Web.Http.HttpConfiguration configuration, bool caseInsensitive)

	[
	ExtensionAttribute(),
	]
	public static void EnableContinueOnErrorHeader (System.Web.Http.HttpConfiguration configuration)

	[
	ExtensionAttribute(),
	]
	public static void EnableEnumPrefixFree (System.Web.Http.HttpConfiguration configuration, bool enumPrefixFree)

	[
	ExtensionAttribute(),
	]
	public static void EnableUnqualifiedNameCall (System.Web.Http.HttpConfiguration configuration, bool unqualifiedNameCall)

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
	public static ODataRoute MapODataServiceRoute (System.Web.Http.HttpConfiguration configuration, string routeName, string routePrefix, Microsoft.OData.Edm.IEdmModel model, System.Net.Http.HttpMessageHandler defaultHandler)

	[
	ExtensionAttribute(),
	]
	public static ODataRoute MapODataServiceRoute (System.Web.Http.HttpConfiguration configuration, string routeName, string routePrefix, Microsoft.OData.Edm.IEdmModel model, ODataBatchHandler batchHandler)

	[
	ExtensionAttribute(),
	]
	public static ODataRoute MapODataServiceRoute (System.Web.Http.HttpConfiguration configuration, string routeName, string routePrefix, Microsoft.OData.Edm.IEdmModel model, IODataPathHandler pathHandler, System.Collections.Generic.IEnumerable`1[[System.Web.OData.Routing.Conventions.IODataRoutingConvention]] routingConventions)

	[
	ExtensionAttribute(),
	]
	public static ODataRoute MapODataServiceRoute (System.Web.Http.HttpConfiguration configuration, string routeName, string routePrefix, Microsoft.OData.Edm.IEdmModel model, IODataPathHandler pathHandler, System.Collections.Generic.IEnumerable`1[[System.Web.OData.Routing.Conventions.IODataRoutingConvention]] routingConventions, System.Net.Http.HttpMessageHandler defaultHandler)

	[
	ExtensionAttribute(),
	]
	public static ODataRoute MapODataServiceRoute (System.Web.Http.HttpConfiguration configuration, string routeName, string routePrefix, Microsoft.OData.Edm.IEdmModel model, IODataPathHandler pathHandler, System.Collections.Generic.IEnumerable`1[[System.Web.OData.Routing.Conventions.IODataRoutingConvention]] routingConventions, ODataBatchHandler batchHandler)

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
	public static void SetUrlConventions (System.Web.Http.HttpConfiguration configuration, Microsoft.OData.Core.UriParser.ODataUrlConventions conventions)
}

[
EditorBrowsableAttribute(),
ExtensionAttribute(),
]
public sealed class System.Web.OData.Extensions.HttpErrorExtensions {
	[
	ExtensionAttribute(),
	]
	public static Microsoft.OData.Core.ODataError CreateODataError (System.Web.Http.HttpError httpError)
}

[
EditorBrowsableAttribute(),
ExtensionAttribute(),
]
public sealed class System.Web.OData.Extensions.HttpRequestMessageExtensions {
	[
	ExtensionAttribute(),
	]
	public static System.Net.Http.HttpResponseMessage CreateErrorResponse (System.Net.Http.HttpRequestMessage request, System.Net.HttpStatusCode statusCode, Microsoft.OData.Core.ODataError oDataError)

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
	public static System.Uri GetNextPageLink (System.Net.Http.HttpRequestMessage request, int pageSize)

	[
	ExtensionAttribute(),
	]
	public static HttpRequestMessageProperties ODataProperties (System.Net.Http.HttpRequestMessage request)
}

[
EditorBrowsableAttribute(),
ExtensionAttribute(),
]
public sealed class System.Web.OData.Extensions.UrlHelperExtensions {
	[
	ExtensionAttribute(),
	]
	public static string CreateODataLink (System.Web.Http.Routing.UrlHelper urlHelper, System.Collections.Generic.IList`1[[System.Web.OData.Routing.ODataPathSegment]] segments)

	[
	ExtensionAttribute(),
	]
	public static string CreateODataLink (System.Web.Http.Routing.UrlHelper urlHelper, ODataPathSegment[] segments)

	[
	ExtensionAttribute(),
	]
	public static string CreateODataLink (System.Web.Http.Routing.UrlHelper urlHelper, string routeName, IODataPathHandler pathHandler, System.Collections.Generic.IList`1[[System.Web.OData.Routing.ODataPathSegment]] segments)
}

public class System.Web.OData.Extensions.HttpRequestMessageProperties {
	Microsoft.OData.Core.UriParser.Aggregation.ApplyClause ApplyClause  { public get; public set; }
	Microsoft.OData.Edm.IEdmModel Model  { public get; public set; }
	System.Uri NextLink  { public get; public set; }
	ODataPath Path  { public get; public set; }
	IODataPathHandler PathHandler  { public get; public set; }
	string RouteName  { public get; public set; }
	System.Collections.Generic.IEnumerable`1[[System.Web.OData.Routing.Conventions.IODataRoutingConvention]] RoutingConventions  { public get; public set; }
	System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] RoutingConventionsStore  { public get; }
	Microsoft.OData.Core.UriParser.Semantic.SelectExpandClause SelectExpandClause  { public get; public set; }
	System.Nullable`1[[System.Int64]] TotalCount  { public get; public set; }
}

public enum System.Web.OData.Formatter.ODataMetadataLevel : int {
	FullMetadata = 1
	MinimalMetadata = 0
	NoMetadata = 2
}

public interface System.Web.OData.Formatter.IETagHandler {
	System.Net.Http.Headers.EntityTagHeaderValue CreateETag (System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] properties)
	System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] ParseETag (System.Net.Http.Headers.EntityTagHeaderValue etagHeaderValue)
}

public abstract class System.Web.OData.Formatter.ODataRawValueMediaTypeMapping : System.Net.Http.Formatting.MediaTypeMapping {
	protected ODataRawValueMediaTypeMapping (string mediaType)

	protected abstract bool IsMatch (PropertyAccessPathSegment propertySegment)
	public virtual double TryMatchMediaType (System.Net.Http.HttpRequestMessage request)
}

[
ExtensionAttribute(),
]
public sealed class System.Web.OData.Formatter.ODataMediaTypeFormatters {
	public static System.Collections.Generic.IList`1[[System.Web.OData.Formatter.ODataMediaTypeFormatter]] Create ()
	public static System.Collections.Generic.IList`1[[System.Web.OData.Formatter.ODataMediaTypeFormatter]] Create (ODataSerializerProvider serializerProvider, ODataDeserializerProvider deserializerProvider)
}

[
DefaultMemberAttribute(),
]
public class System.Web.OData.Formatter.ETag : System.Dynamic.DynamicObject, IDynamicMetaObjectProvider {
	public ETag ()

	System.Type EntityType  { public get; public set; }
	bool IsAny  { public get; public set; }
	bool IsWellFormed  { public get; public set; }
	object Item [string key] { public get; public set; }

	public virtual System.Linq.IQueryable ApplyTo (System.Linq.IQueryable query)
	public virtual bool TryGetMember (System.Dynamic.GetMemberBinder binder, out System.Object& result)
	public virtual bool TrySetMember (System.Dynamic.SetMemberBinder binder, object value)
}

public class System.Web.OData.Formatter.ETag`1 : ETag, IDynamicMetaObjectProvider {
	public ETag`1 ()

	public IQueryable`1 ApplyTo (IQueryable`1 query)
	public virtual System.Linq.IQueryable ApplyTo (System.Linq.IQueryable query)
}

public class System.Web.OData.Formatter.ODataBinaryValueMediaTypeMapping : ODataRawValueMediaTypeMapping {
	public ODataBinaryValueMediaTypeMapping ()

	protected virtual bool IsMatch (PropertyAccessPathSegment propertySegment)
}

public class System.Web.OData.Formatter.ODataCountMediaTypeMapping : System.Net.Http.Formatting.MediaTypeMapping {
	public ODataCountMediaTypeMapping ()

	public virtual double TryMatchMediaType (System.Net.Http.HttpRequestMessage request)
}

public class System.Web.OData.Formatter.ODataEnumValueMediaTypeMapping : ODataRawValueMediaTypeMapping {
	public ODataEnumValueMediaTypeMapping ()

	protected virtual bool IsMatch (PropertyAccessPathSegment propertySegment)
}

public class System.Web.OData.Formatter.ODataMediaTypeFormatter : System.Net.Http.Formatting.MediaTypeFormatter {
	public ODataMediaTypeFormatter (System.Collections.Generic.IEnumerable`1[[Microsoft.OData.Core.ODataPayloadKind]] payloadKinds)
	public ODataMediaTypeFormatter (ODataDeserializerProvider deserializerProvider, ODataSerializerProvider serializerProvider, System.Collections.Generic.IEnumerable`1[[Microsoft.OData.Core.ODataPayloadKind]] payloadKinds)

	System.Func`2[[System.Net.Http.HttpRequestMessage],[System.Uri]] BaseAddressFactory  { public get; public set; }
	ODataDeserializerProvider DeserializerProvider  { public get; }
	Microsoft.OData.Core.ODataMessageQuotas MessageReaderQuotas  { public get; }
	Microsoft.OData.Core.ODataMessageReaderSettings MessageReaderSettings  { public get; }
	Microsoft.OData.Core.ODataMessageQuotas MessageWriterQuotas  { public get; }
	Microsoft.OData.Core.ODataMessageWriterSettings MessageWriterSettings  { public get; }
	ODataSerializerProvider SerializerProvider  { public get; }

	public virtual bool CanReadType (System.Type type)
	public virtual bool CanWriteType (System.Type type)
	public static System.Uri GetDefaultBaseAddress (System.Net.Http.HttpRequestMessage request)
	public virtual System.Net.Http.Formatting.MediaTypeFormatter GetPerRequestFormatterInstance (System.Type type, System.Net.Http.HttpRequestMessage request, System.Net.Http.Headers.MediaTypeHeaderValue mediaType)
	public virtual System.Threading.Tasks.Task`1[[System.Object]] ReadFromStreamAsync (System.Type type, System.IO.Stream readStream, System.Net.Http.HttpContent content, System.Net.Http.Formatting.IFormatterLogger formatterLogger)
	public virtual void SetDefaultContentHeaders (System.Type type, System.Net.Http.Headers.HttpContentHeaders headers, System.Net.Http.Headers.MediaTypeHeaderValue mediaType)
	public virtual System.Threading.Tasks.Task WriteToStreamAsync (System.Type type, object value, System.IO.Stream writeStream, System.Net.Http.HttpContent content, System.Net.TransportContext transportContext, System.Threading.CancellationToken cancellationToken)
}

public class System.Web.OData.Formatter.ODataModelBinderProvider : System.Web.Http.ModelBinding.ModelBinderProvider {
	public ODataModelBinderProvider ()

	public virtual System.Web.Http.ModelBinding.IModelBinder GetBinder (System.Web.Http.HttpConfiguration configuration, System.Type modelType)
}

public class System.Web.OData.Formatter.ODataPrimitiveValueMediaTypeMapping : ODataRawValueMediaTypeMapping {
	public ODataPrimitiveValueMediaTypeMapping ()

	protected virtual bool IsMatch (PropertyAccessPathSegment propertySegment)
}

public class System.Web.OData.Formatter.QueryStringMediaTypeMapping : System.Net.Http.Formatting.MediaTypeMapping {
	public QueryStringMediaTypeMapping (string queryStringParameterName, System.Net.Http.Headers.MediaTypeHeaderValue mediaType)
	public QueryStringMediaTypeMapping (string queryStringParameterName, string mediaType)

	string QueryStringParameterName  { public get; }

	public virtual double TryMatchMediaType (System.Net.Http.HttpRequestMessage request)
}

[
FlagsAttribute(),
]
public enum System.Web.OData.Query.AllowedArithmeticOperators : int {
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
public enum System.Web.OData.Query.AllowedFunctions : int {
	All = 268435456
	AllDateTimeFunctions = 7010304
	AllFunctions = 535494655
	AllMathFunctions = 58720256
	AllStringFunctions = 1023
	Any = 134217728
	Cast = 1024
	Ceiling = 33554432
	Concat = 32
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
	SubstringOf = 4
	Time = 16384
	ToLower = 128
	ToUpper = 256
	Trim = 512
	Year = 2048
}

[
FlagsAttribute(),
]
public enum System.Web.OData.Query.AllowedLogicalOperators : int {
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
public enum System.Web.OData.Query.AllowedQueryOptions : int {
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
	Supported = 1279
	Top = 16
}

public enum System.Web.OData.Query.HandleNullPropagationOption : int {
	Default = 0
	False = 2
	True = 1
}

public interface System.Web.OData.Query.IPropertyMapper {
	string MapProperty (string propertyName)
}

public interface System.Web.OData.Query.ISelectExpandWrapper {
	System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] ToDictionary ()
	System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] ToDictionary (System.Func`3[[Microsoft.OData.Edm.IEdmModel],[Microsoft.OData.Edm.IEdmStructuredType],[System.Web.OData.Query.IPropertyMapper]] propertyMapperProvider)
}

public interface System.Web.OData.Query.ITruncatedCollection : IEnumerable {
	bool IsTruncated  { public abstract get; }
	int PageSize  { public abstract get; }
}

public abstract class System.Web.OData.Query.OrderByNode {
	protected OrderByNode (Microsoft.OData.Core.UriParser.OrderByDirection direction)

	Microsoft.OData.Core.UriParser.OrderByDirection Direction  { public get; }

	public static System.Collections.Generic.IList`1[[System.Web.OData.Query.OrderByNode]] CreateCollection (Microsoft.OData.Core.UriParser.Semantic.OrderByClause orderByClause)
}

public class System.Web.OData.Query.ApplyQueryOption {
	public ApplyQueryOption (string rawValue, ODataQueryContext context, Microsoft.OData.Core.UriParser.ODataQueryOptionParser queryOptionParser)

	Microsoft.OData.Core.UriParser.Aggregation.ApplyClause ApplyClause  { public get; }
	ODataQueryContext Context  { public get; }
	string RawValue  { public get; }
	System.Type ResultClrType  { public get; }

	public System.Linq.IQueryable ApplyTo (System.Linq.IQueryable query, ODataQuerySettings querySettings)
	public System.Linq.IQueryable ApplyTo (System.Linq.IQueryable query, ODataQuerySettings querySettings, System.Web.Http.Dispatcher.IAssembliesResolver assembliesResolver)
}

public class System.Web.OData.Query.CountQueryOption {
	public CountQueryOption (string rawValue, ODataQueryContext context, Microsoft.OData.Core.UriParser.ODataQueryOptionParser queryOptionParser)

	ODataQueryContext Context  { public get; }
	string RawValue  { public get; }
	CountQueryValidator Validator  { public get; public set; }
	bool Value  { public get; }

	public System.Nullable`1[[System.Int64]] GetEntityCount (System.Linq.IQueryable query)
	public void Validate (ODataValidationSettings validationSettings)
}

public class System.Web.OData.Query.FilterQueryOption {
	public FilterQueryOption (string rawValue, ODataQueryContext context, Microsoft.OData.Core.UriParser.ODataQueryOptionParser queryOptionParser)

	ODataQueryContext Context  { public get; }
	Microsoft.OData.Core.UriParser.Semantic.FilterClause FilterClause  { public get; }
	string RawValue  { public get; }
	FilterQueryValidator Validator  { public get; public set; }

	public System.Linq.IQueryable ApplyTo (System.Linq.IQueryable query, ODataQuerySettings querySettings)
	public System.Linq.IQueryable ApplyTo (System.Linq.IQueryable query, ODataQuerySettings querySettings, System.Web.Http.Dispatcher.IAssembliesResolver assembliesResolver)
	public void Validate (ODataValidationSettings validationSettings)
}

[
ODataQueryParameterBindingAttribute(),
]
public class System.Web.OData.Query.ODataQueryOptions {
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
	TopQueryOption Top  { public get; }
	ODataQueryValidator Validator  { public get; public set; }

	public virtual System.Linq.IQueryable ApplyTo (System.Linq.IQueryable query)
	public virtual System.Linq.IQueryable ApplyTo (System.Linq.IQueryable query, AllowedQueryOptions ignoreQueryOptions)
	public virtual System.Linq.IQueryable ApplyTo (System.Linq.IQueryable query, ODataQuerySettings querySettings)
	public virtual object ApplyTo (object entity, ODataQuerySettings querySettings)
	public virtual System.Linq.IQueryable ApplyTo (System.Linq.IQueryable query, ODataQuerySettings querySettings, AllowedQueryOptions ignoreQueryOptions)
	public virtual object ApplyTo (object entity, ODataQuerySettings querySettings, AllowedQueryOptions ignoreQueryOptions)
	internal virtual ETag GetETag (System.Net.Http.Headers.EntityTagHeaderValue etagHeaderValue)
	public bool IsSupportedQueryOption (string queryOptionName)
	public static bool IsSystemQueryOption (string queryOptionName)
	public static IQueryable`1 LimitResults (IQueryable`1 queryable, int limit, out System.Boolean& resultsLimited)
	public virtual void Validate (ODataValidationSettings validationSettings)
}

[
ODataQueryParameterBindingAttribute(),
]
public class System.Web.OData.Query.ODataQueryOptions`1 : ODataQueryOptions {
	public ODataQueryOptions`1 (ODataQueryContext context, System.Net.Http.HttpRequestMessage request)

	ETag`1 IfMatch  { public get; }
	ETag`1 IfNoneMatch  { public get; }

	public virtual System.Linq.IQueryable ApplyTo (System.Linq.IQueryable query)
	public virtual System.Linq.IQueryable ApplyTo (System.Linq.IQueryable query, ODataQuerySettings querySettings)
	internal virtual ETag GetETag (System.Net.Http.Headers.EntityTagHeaderValue etagHeaderValue)
}

public class System.Web.OData.Query.ODataQuerySettings {
	public ODataQuerySettings ()
	public ODataQuerySettings (ODataQuerySettings settings)

	bool EnableConstantParameterization  { public get; public set; }
	bool EnsureStableOrdering  { public get; public set; }
	HandleNullPropagationOption HandleNullPropagation  { public get; public set; }
	System.Nullable`1[[System.Int32]] PageSize  { public get; public set; }
	bool SearchDerivedTypeWhenAutoExpand  { public get; public set; }
}

public class System.Web.OData.Query.ODataRawQueryOptions {
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

public class System.Web.OData.Query.ODataValidationSettings {
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

public class System.Web.OData.Query.OrderByItNode : OrderByNode {
	public OrderByItNode (Microsoft.OData.Core.UriParser.OrderByDirection direction)
}

public class System.Web.OData.Query.OrderByOpenPropertyNode : OrderByNode {
	public OrderByOpenPropertyNode (Microsoft.OData.Core.UriParser.Semantic.OrderByClause orderByClause)

	Microsoft.OData.Core.UriParser.Semantic.OrderByClause OrderByClause  { public get; }
	string PropertyName  { public get; }
}

public class System.Web.OData.Query.OrderByPropertyNode : OrderByNode {
	public OrderByPropertyNode (Microsoft.OData.Core.UriParser.Semantic.OrderByClause orderByClause)
	public OrderByPropertyNode (Microsoft.OData.Edm.IEdmProperty property, Microsoft.OData.Core.UriParser.OrderByDirection direction)

	Microsoft.OData.Core.UriParser.Semantic.OrderByClause OrderByClause  { public get; }
	Microsoft.OData.Edm.IEdmProperty Property  { public get; }
}

public class System.Web.OData.Query.OrderByQueryOption {
	public OrderByQueryOption (string rawValue, ODataQueryContext context, Microsoft.OData.Core.UriParser.ODataQueryOptionParser queryOptionParser)

	ODataQueryContext Context  { public get; }
	Microsoft.OData.Core.UriParser.Semantic.OrderByClause OrderByClause  { public get; }
	System.Collections.Generic.IList`1[[System.Web.OData.Query.OrderByNode]] OrderByNodes  { public get; }
	string RawValue  { public get; }
	OrderByQueryValidator Validator  { public get; public set; }

	public IOrderedQueryable`1 ApplyTo (IQueryable`1 query)
	public System.Linq.IOrderedQueryable ApplyTo (System.Linq.IQueryable query)
	public IOrderedQueryable`1 ApplyTo (IQueryable`1 query, ODataQuerySettings querySettings)
	public System.Linq.IOrderedQueryable ApplyTo (System.Linq.IQueryable query, ODataQuerySettings querySettings)
	public void Validate (ODataValidationSettings validationSettings)
}

public class System.Web.OData.Query.ParameterAliasNodeTranslator : Microsoft.OData.Core.UriParser.Visitors.QueryNodeVisitor`1[[Microsoft.OData.Core.UriParser.Semantic.QueryNode]] {
	public ParameterAliasNodeTranslator (System.Collections.Generic.IDictionary`2[[System.String],[Microsoft.OData.Core.UriParser.Semantic.SingleValueNode]] parameterAliasNodes)

	public virtual Microsoft.OData.Core.UriParser.Semantic.QueryNode Visit (Microsoft.OData.Core.UriParser.Semantic.AllNode nodeIn)
	public virtual Microsoft.OData.Core.UriParser.Semantic.QueryNode Visit (Microsoft.OData.Core.UriParser.Semantic.AnyNode nodeIn)
	public virtual Microsoft.OData.Core.UriParser.Semantic.QueryNode Visit (Microsoft.OData.Core.UriParser.Semantic.BinaryOperatorNode nodeIn)
	public virtual Microsoft.OData.Core.UriParser.Semantic.QueryNode Visit (Microsoft.OData.Core.UriParser.Semantic.CollectionFunctionCallNode nodeIn)
	public virtual Microsoft.OData.Core.UriParser.Semantic.QueryNode Visit (Microsoft.OData.Core.UriParser.Semantic.CollectionNavigationNode nodeIn)
	public virtual Microsoft.OData.Core.UriParser.Semantic.QueryNode Visit (Microsoft.OData.Core.UriParser.Semantic.CollectionOpenPropertyAccessNode nodeIn)
	public virtual Microsoft.OData.Core.UriParser.Semantic.QueryNode Visit (Microsoft.OData.Core.UriParser.Semantic.CollectionPropertyAccessNode nodeIn)
	public virtual Microsoft.OData.Core.UriParser.Semantic.QueryNode Visit (Microsoft.OData.Core.UriParser.Semantic.CollectionPropertyCastNode nodeIn)
	public virtual Microsoft.OData.Core.UriParser.Semantic.QueryNode Visit (Microsoft.OData.Core.UriParser.Semantic.ConstantNode nodeIn)
	public virtual Microsoft.OData.Core.UriParser.Semantic.QueryNode Visit (Microsoft.OData.Core.UriParser.Semantic.ConvertNode nodeIn)
	public virtual Microsoft.OData.Core.UriParser.Semantic.QueryNode Visit (Microsoft.OData.Core.UriParser.Semantic.EntityCollectionCastNode nodeIn)
	public virtual Microsoft.OData.Core.UriParser.Semantic.QueryNode Visit (Microsoft.OData.Core.UriParser.Semantic.EntityCollectionFunctionCallNode nodeIn)
	public virtual Microsoft.OData.Core.UriParser.Semantic.QueryNode Visit (Microsoft.OData.Core.UriParser.Semantic.EntityRangeVariableReferenceNode nodeIn)
	public virtual Microsoft.OData.Core.UriParser.Semantic.QueryNode Visit (Microsoft.OData.Core.UriParser.Semantic.NamedFunctionParameterNode nodeIn)
	public virtual Microsoft.OData.Core.UriParser.Semantic.QueryNode Visit (Microsoft.OData.Core.UriParser.Semantic.NonentityRangeVariableReferenceNode nodeIn)
	public virtual Microsoft.OData.Core.UriParser.Semantic.QueryNode Visit (Microsoft.OData.Core.UriParser.Semantic.ParameterAliasNode nodeIn)
	public virtual Microsoft.OData.Core.UriParser.Semantic.QueryNode Visit (Microsoft.OData.Core.UriParser.Semantic.SearchTermNode nodeIn)
	public virtual Microsoft.OData.Core.UriParser.Semantic.QueryNode Visit (Microsoft.OData.Core.UriParser.Semantic.SingleEntityCastNode nodeIn)
	public virtual Microsoft.OData.Core.UriParser.Semantic.QueryNode Visit (Microsoft.OData.Core.UriParser.Semantic.SingleEntityFunctionCallNode nodeIn)
	public virtual Microsoft.OData.Core.UriParser.Semantic.QueryNode Visit (Microsoft.OData.Core.UriParser.Semantic.SingleNavigationNode nodeIn)
	public virtual Microsoft.OData.Core.UriParser.Semantic.QueryNode Visit (Microsoft.OData.Core.UriParser.Semantic.SingleValueCastNode nodeIn)
	public virtual Microsoft.OData.Core.UriParser.Semantic.QueryNode Visit (Microsoft.OData.Core.UriParser.Semantic.SingleValueFunctionCallNode nodeIn)
	public virtual Microsoft.OData.Core.UriParser.Semantic.QueryNode Visit (Microsoft.OData.Core.UriParser.Semantic.SingleValueOpenPropertyAccessNode nodeIn)
	public virtual Microsoft.OData.Core.UriParser.Semantic.QueryNode Visit (Microsoft.OData.Core.UriParser.Semantic.SingleValuePropertyAccessNode nodeIn)
	public virtual Microsoft.OData.Core.UriParser.Semantic.QueryNode Visit (Microsoft.OData.Core.UriParser.Semantic.UnaryOperatorNode nodeIn)
}

public class System.Web.OData.Query.QueryFilterProvider : IFilterProvider {
	public QueryFilterProvider (System.Web.Http.Filters.IActionFilter queryFilter)

	System.Web.Http.Filters.IActionFilter QueryFilter  { public get; }

	public virtual System.Collections.Generic.IEnumerable`1[[System.Web.Http.Filters.FilterInfo]] GetFilters (System.Web.Http.HttpConfiguration configuration, System.Web.Http.Controllers.HttpActionDescriptor actionDescriptor)
}

public class System.Web.OData.Query.SelectExpandQueryOption {
	public SelectExpandQueryOption (string select, string expand, ODataQueryContext context, Microsoft.OData.Core.UriParser.ODataQueryOptionParser queryOptionParser)

	ODataQueryContext Context  { public get; }
	int LevelsMaxLiteralExpansionDepth  { public get; public set; }
	string RawExpand  { public get; }
	string RawSelect  { public get; }
	bool SearchDerivedTypeWhenAutoExpand  { public get; public set; }
	Microsoft.OData.Core.UriParser.Semantic.SelectExpandClause SelectExpandClause  { public get; }
	SelectExpandQueryValidator Validator  { public get; public set; }

	public System.Linq.IQueryable ApplyTo (System.Linq.IQueryable queryable, ODataQuerySettings settings)
	public object ApplyTo (object entity, ODataQuerySettings settings)
	public System.Linq.IQueryable ApplyTo (System.Linq.IQueryable queryable, ODataQuerySettings settings, System.Web.Http.Dispatcher.IAssembliesResolver assembliesResolver)
	public object ApplyTo (object entity, ODataQuerySettings settings, System.Web.Http.Dispatcher.IAssembliesResolver assembliesResolver)
	public void Validate (ODataValidationSettings validationSettings)
}

public class System.Web.OData.Query.SkipQueryOption {
	public SkipQueryOption (string rawValue, ODataQueryContext context, Microsoft.OData.Core.UriParser.ODataQueryOptionParser queryOptionParser)

	ODataQueryContext Context  { public get; }
	string RawValue  { public get; }
	SkipQueryValidator Validator  { public get; public set; }
	int Value  { public get; }

	public IQueryable`1 ApplyTo (IQueryable`1 query, ODataQuerySettings querySettings)
	public System.Linq.IQueryable ApplyTo (System.Linq.IQueryable query, ODataQuerySettings querySettings)
	public void Validate (ODataValidationSettings validationSettings)
}

public class System.Web.OData.Query.TopQueryOption {
	public TopQueryOption (string rawValue, ODataQueryContext context, Microsoft.OData.Core.UriParser.ODataQueryOptionParser queryOptionParser)

	ODataQueryContext Context  { public get; }
	string RawValue  { public get; }
	TopQueryValidator Validator  { public get; public set; }
	int Value  { public get; }

	public IOrderedQueryable`1 ApplyTo (IQueryable`1 query, ODataQuerySettings querySettings)
	public System.Linq.IQueryable ApplyTo (System.Linq.IQueryable query, ODataQuerySettings querySettings)
	public void Validate (ODataValidationSettings validationSettings)
}

public class System.Web.OData.Query.TruncatedCollection`1 : List`1, ICollection`1, IEnumerable`1, IList`1, IReadOnlyCollection`1, IReadOnlyList`1, ICollection, IEnumerable, IList, ICountOptionCollection, ITruncatedCollection {
	public TruncatedCollection`1 (IEnumerable`1 source, int pageSize)
	public TruncatedCollection`1 (IQueryable`1 source, int pageSize)
	public TruncatedCollection`1 (IEnumerable`1 source, int pageSize, System.Nullable`1[[System.Int64]] totalCount)
	public TruncatedCollection`1 (IQueryable`1 source, int pageSize, System.Nullable`1[[System.Int64]] totalCount)

	bool IsTruncated  { public virtual get; }
	int PageSize  { public virtual get; }
	System.Nullable`1[[System.Int64]] TotalCount  { public virtual get; }
}

[
AttributeUsageAttribute(),
]
public sealed class System.Web.OData.Query.NonFilterableAttribute : System.Attribute, _Attribute {
	public NonFilterableAttribute ()
}

[
AttributeUsageAttribute(),
]
public sealed class System.Web.OData.Query.NotCountableAttribute : System.Attribute, _Attribute {
	public NotCountableAttribute ()
}

[
AttributeUsageAttribute(),
]
public sealed class System.Web.OData.Query.NotExpandableAttribute : System.Attribute, _Attribute {
	public NotExpandableAttribute ()
}

[
AttributeUsageAttribute(),
]
public sealed class System.Web.OData.Query.NotFilterableAttribute : System.Attribute, _Attribute {
	public NotFilterableAttribute ()
}

[
AttributeUsageAttribute(),
]
public sealed class System.Web.OData.Query.NotNavigableAttribute : System.Attribute, _Attribute {
	public NotNavigableAttribute ()
}

[
AttributeUsageAttribute(),
]
public sealed class System.Web.OData.Query.NotSortableAttribute : System.Attribute, _Attribute {
	public NotSortableAttribute ()
}

[
AttributeUsageAttribute(),
]
public sealed class System.Web.OData.Query.UnsortableAttribute : System.Attribute, _Attribute {
	public UnsortableAttribute ()
}

public class System.Web.OData.Results.CreatedODataResult`1 : IHttpActionResult {
	public CreatedODataResult`1 (T entity, System.Web.Http.ApiController controller)
	public CreatedODataResult`1 (T entity, System.Net.Http.Formatting.IContentNegotiator contentNegotiator, System.Net.Http.HttpRequestMessage request, System.Collections.Generic.IEnumerable`1[[System.Net.Http.Formatting.MediaTypeFormatter]] formatters, System.Uri locationHeader)

	System.Net.Http.Formatting.IContentNegotiator ContentNegotiator  { public get; }
	T Entity  { public get; }
	System.Collections.Generic.IEnumerable`1[[System.Net.Http.Formatting.MediaTypeFormatter]] Formatters  { public get; }
	System.Uri LocationHeader  { public get; }
	System.Net.Http.HttpRequestMessage Request  { public get; }

	[
	DebuggerStepThroughAttribute(),
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task`1[[System.Net.Http.HttpResponseMessage]] ExecuteAsync (System.Threading.CancellationToken cancellationToken)
}

public class System.Web.OData.Results.UpdatedODataResult`1 : IHttpActionResult {
	public UpdatedODataResult`1 (T entity, System.Web.Http.ApiController controller)
	public UpdatedODataResult`1 (T entity, System.Net.Http.Formatting.IContentNegotiator contentNegotiator, System.Net.Http.HttpRequestMessage request, System.Collections.Generic.IEnumerable`1[[System.Net.Http.Formatting.MediaTypeFormatter]] formatters)

	System.Net.Http.Formatting.IContentNegotiator ContentNegotiator  { public get; }
	T Entity  { public get; }
	System.Collections.Generic.IEnumerable`1[[System.Net.Http.Formatting.MediaTypeFormatter]] Formatters  { public get; }
	System.Net.Http.HttpRequestMessage Request  { public get; }

	[
	DebuggerStepThroughAttribute(),
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task`1[[System.Net.Http.HttpResponseMessage]] ExecuteAsync (System.Threading.CancellationToken cancellationToken)
}

public interface System.Web.OData.Routing.IODataPathHandler {
	string Link (ODataPath path)
	ODataPath Parse (Microsoft.OData.Edm.IEdmModel model, string serviceRoot, string odataPath)
}

public interface System.Web.OData.Routing.IODataPathTemplateHandler {
	ODataPathTemplate ParseTemplate (Microsoft.OData.Edm.IEdmModel model, string odataPathTemplate)
}

public abstract class System.Web.OData.Routing.ODataPathSegment : ODataPathSegmentTemplate {
	protected ODataPathSegment ()

	string SegmentKind  { public abstract get; }

	public abstract Microsoft.OData.Edm.IEdmType GetEdmType (Microsoft.OData.Edm.IEdmType previousEdmType)
	public abstract Microsoft.OData.Edm.IEdmNavigationSource GetNavigationSource (Microsoft.OData.Edm.IEdmNavigationSource previousNavigationSource)
}

public abstract class System.Web.OData.Routing.ODataPathSegmentTemplate {
	protected ODataPathSegmentTemplate ()

	public virtual bool TryMatch (ODataPathSegment pathSegment, System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] values)
}

public sealed class System.Web.OData.Routing.ODataRouteConstants {
	public static readonly string Action = "action"
	public static readonly string Batch = "$batch"
	public static readonly string ConstraintName = "ODataConstraint"
	public static readonly string Controller = "controller"
	public static readonly string DynamicProperty = "dynamicProperty"
	public static readonly string Key = "key"
	public static readonly string NavigationProperty = "navigationProperty"
	public static readonly string ODataPath = "odataPath"
	public static readonly string ODataPathTemplate = "{*odataPath}"
	public static readonly string RelatedKey = "relatedKey"
	public static readonly string VersionConstraintName = "ODataVersionConstraint"
}

public sealed class System.Web.OData.Routing.ODataSegmentKinds {
	public static readonly string Action = "action"
	public static readonly string Batch = "$batch"
	public static readonly string Cast = "cast"
	public static readonly string ComplexCast = "complexcast"
	public static readonly string Count = "$count"
	public static readonly string DynamicProperty = "dynamicproperty"
	public static readonly string EntitySet = "entityset"
	public static readonly string Function = "function"
	public static readonly string Key = "key"
	public static readonly string Metadata = "$metadata"
	public static readonly string Navigation = "navigation"
	public static readonly string Property = "property"
	public static readonly string Ref = "$ref"
	public static readonly string ServiceBase = "~"
	public static readonly string Singleton = "singleton"
	public static readonly string UnboundAction = "unboundaction"
	public static readonly string UnboundFunction = "unboundfunction"
	public static readonly string Unresolved = "unresolved"
	public static readonly string Value = "$value"
}

public class System.Web.OData.Routing.BatchPathSegment : ODataPathSegment {
	public BatchPathSegment ()

	string SegmentKind  { public virtual get; }

	public virtual Microsoft.OData.Edm.IEdmType GetEdmType (Microsoft.OData.Edm.IEdmType previousEdmType)
	public virtual Microsoft.OData.Edm.IEdmNavigationSource GetNavigationSource (Microsoft.OData.Edm.IEdmNavigationSource previousNavigationSource)
	public virtual string ToString ()
	public virtual bool TryMatch (ODataPathSegment pathSegment, System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] values)
}

public class System.Web.OData.Routing.BoundActionPathSegment : ODataPathSegment {
	public BoundActionPathSegment (Microsoft.OData.Edm.IEdmAction action, Microsoft.OData.Edm.IEdmModel model)

	Microsoft.OData.Edm.IEdmAction Action  { public get; }
	string ActionName  { public get; }
	string SegmentKind  { public virtual get; }

	public virtual Microsoft.OData.Edm.IEdmType GetEdmType (Microsoft.OData.Edm.IEdmType previousEdmType)
	public virtual Microsoft.OData.Edm.IEdmNavigationSource GetNavigationSource (Microsoft.OData.Edm.IEdmNavigationSource previousNavigationSource)
	public virtual string ToString ()
	public virtual bool TryMatch (ODataPathSegment pathSegment, System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] values)
}

public class System.Web.OData.Routing.BoundFunctionPathSegment : ODataPathSegment {
	public BoundFunctionPathSegment (Microsoft.OData.Edm.IEdmFunction function, Microsoft.OData.Edm.IEdmModel model, System.Collections.Generic.IDictionary`2[[System.String],[System.String]] parameterValues)

	Microsoft.OData.Edm.IEdmFunction Function  { public get; }
	string FunctionName  { public get; }
	string SegmentKind  { public virtual get; }

	public virtual Microsoft.OData.Edm.IEdmType GetEdmType (Microsoft.OData.Edm.IEdmType previousEdmType)
	public virtual Microsoft.OData.Edm.IEdmNavigationSource GetNavigationSource (Microsoft.OData.Edm.IEdmNavigationSource previousNavigationSource)
	public object GetParameterValue (string parameterName)
	public virtual string ToString ()
	public virtual bool TryMatch (ODataPathSegment pathSegment, System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] values)
}

public class System.Web.OData.Routing.BoundFunctionPathSegmentTemplate : ODataPathSegmentTemplate {
	public BoundFunctionPathSegmentTemplate (BoundFunctionPathSegment function)

	string FunctionName  { public get; }
	System.Collections.Generic.IDictionary`2[[System.String],[System.String]] ParameterMappings  { public get; }

	public virtual bool TryMatch (ODataPathSegment pathSegment, System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] values)
}

public class System.Web.OData.Routing.CastPathSegment : ODataPathSegment {
	public CastPathSegment (Microsoft.OData.Edm.IEdmEntityType castType)
	public CastPathSegment (string castTypeName)

	Microsoft.OData.Edm.IEdmEntityType CastType  { public get; }
	string CastTypeName  { public get; }
	string SegmentKind  { public virtual get; }

	public virtual Microsoft.OData.Edm.IEdmType GetEdmType (Microsoft.OData.Edm.IEdmType previousEdmType)
	public virtual Microsoft.OData.Edm.IEdmNavigationSource GetNavigationSource (Microsoft.OData.Edm.IEdmNavigationSource previousNavigationSource)
	public virtual string ToString ()
	public virtual bool TryMatch (ODataPathSegment pathSegment, System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] values)
}

public class System.Web.OData.Routing.ComplexCastPathSegment : ODataPathSegment {
	public ComplexCastPathSegment (Microsoft.OData.Edm.IEdmComplexType castType)
	public ComplexCastPathSegment (string castTypeName)

	Microsoft.OData.Edm.IEdmComplexType CastType  { public get; }
	string CastTypeName  { public get; }
	string SegmentKind  { public virtual get; }

	public virtual Microsoft.OData.Edm.IEdmType GetEdmType (Microsoft.OData.Edm.IEdmType previousEdmType)
	public virtual Microsoft.OData.Edm.IEdmNavigationSource GetNavigationSource (Microsoft.OData.Edm.IEdmNavigationSource previousNavigationSource)
	public virtual string ToString ()
	public virtual bool TryMatch (ODataPathSegment pathSegment, System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] values)
}

public class System.Web.OData.Routing.CountPathSegment : ODataPathSegment {
	public CountPathSegment ()

	string SegmentKind  { public virtual get; }

	public virtual Microsoft.OData.Edm.IEdmType GetEdmType (Microsoft.OData.Edm.IEdmType previousEdmType)
	public virtual Microsoft.OData.Edm.IEdmNavigationSource GetNavigationSource (Microsoft.OData.Edm.IEdmNavigationSource previousNavigationSource)
	public virtual string ToString ()
	public virtual bool TryMatch (ODataPathSegment pathSegment, System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] values)
}

public class System.Web.OData.Routing.DefaultODataPathHandler : IODataPathHandler, IODataPathTemplateHandler {
	public DefaultODataPathHandler ()

	public virtual string Link (ODataPath path)
	public virtual ODataPath Parse (Microsoft.OData.Edm.IEdmModel model, string serviceRoot, string odataPath)
	public virtual ODataPathTemplate ParseTemplate (Microsoft.OData.Edm.IEdmModel model, string odataPathTemplate)
}

public class System.Web.OData.Routing.DynamicPropertyPathSegment : ODataPathSegment {
	public DynamicPropertyPathSegment (string propertyName)

	string PropertyName  { public get; }
	string SegmentKind  { public virtual get; }

	public virtual Microsoft.OData.Edm.IEdmType GetEdmType (Microsoft.OData.Edm.IEdmType previousEdmType)
	public virtual Microsoft.OData.Edm.IEdmNavigationSource GetNavigationSource (Microsoft.OData.Edm.IEdmNavigationSource previousNavigationSource)
	public virtual string ToString ()
	public virtual bool TryMatch (ODataPathSegment pathSegment, System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] values)
}

public class System.Web.OData.Routing.DynamicPropertyPathSegmentTemplate : ODataPathSegmentTemplate {
	public DynamicPropertyPathSegmentTemplate (DynamicPropertyPathSegment dynamicPropertyPathSegment)

	string PropertyName  { public get; }

	public virtual bool TryMatch (ODataPathSegment pathSegment, System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] values)
}

public class System.Web.OData.Routing.EntitySetPathSegment : ODataPathSegment {
	public EntitySetPathSegment (Microsoft.OData.Edm.IEdmEntitySetBase entitySet)
	public EntitySetPathSegment (string entitySetName)

	Microsoft.OData.Edm.IEdmEntitySetBase EntitySetBase  { public get; }
	string EntitySetName  { public get; }
	string SegmentKind  { public virtual get; }

	public virtual Microsoft.OData.Edm.IEdmType GetEdmType (Microsoft.OData.Edm.IEdmType previousEdmType)
	public virtual Microsoft.OData.Edm.IEdmNavigationSource GetNavigationSource (Microsoft.OData.Edm.IEdmNavigationSource previousNavigationSource)
	public virtual string ToString ()
	public virtual bool TryMatch (ODataPathSegment pathSegment, System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] values)
}

public class System.Web.OData.Routing.KeyValuePathSegment : ODataPathSegment {
	public KeyValuePathSegment (string value)

	string SegmentKind  { public virtual get; }
	string Value  { public get; }

	public virtual Microsoft.OData.Edm.IEdmType GetEdmType (Microsoft.OData.Edm.IEdmType previousEdmType)
	public virtual Microsoft.OData.Edm.IEdmNavigationSource GetNavigationSource (Microsoft.OData.Edm.IEdmNavigationSource previousNavigationSource)
	public virtual string ToString ()
	public virtual bool TryMatch (ODataPathSegment pathSegment, System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] values)
}

public class System.Web.OData.Routing.KeyValuePathSegmentTemplate : ODataPathSegmentTemplate {
	public KeyValuePathSegmentTemplate (KeyValuePathSegment keyValueSegment)

	System.Collections.Generic.IDictionary`2[[System.String],[System.String]] ParameterMappings  { public get; }

	public virtual bool TryMatch (ODataPathSegment pathSegment, System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] values)
}

public class System.Web.OData.Routing.MetadataPathSegment : ODataPathSegment {
	public MetadataPathSegment ()

	string SegmentKind  { public virtual get; }

	public virtual Microsoft.OData.Edm.IEdmType GetEdmType (Microsoft.OData.Edm.IEdmType previousEdmType)
	public virtual Microsoft.OData.Edm.IEdmNavigationSource GetNavigationSource (Microsoft.OData.Edm.IEdmNavigationSource previousNavigationSource)
	public virtual string ToString ()
	public virtual bool TryMatch (ODataPathSegment pathSegment, System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] values)
}

public class System.Web.OData.Routing.NavigationPathSegment : ODataPathSegment {
	public NavigationPathSegment (Microsoft.OData.Edm.IEdmNavigationProperty navigationProperty)
	public NavigationPathSegment (string navigationPropertyName)

	Microsoft.OData.Edm.IEdmNavigationProperty NavigationProperty  { public get; }
	string NavigationPropertyName  { public get; }
	string SegmentKind  { public virtual get; }

	public virtual Microsoft.OData.Edm.IEdmType GetEdmType (Microsoft.OData.Edm.IEdmType previousEdmType)
	public virtual Microsoft.OData.Edm.IEdmNavigationSource GetNavigationSource (Microsoft.OData.Edm.IEdmNavigationSource previousNavigationSource)
	public virtual string ToString ()
	public virtual bool TryMatch (ODataPathSegment pathSegment, System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] values)
}

public class System.Web.OData.Routing.ODataActionSelector : IHttpActionSelector {
	public ODataActionSelector (System.Web.Http.Controllers.IHttpActionSelector innerSelector)

	public virtual System.Linq.ILookup`2[[System.String],[System.Web.Http.Controllers.HttpActionDescriptor]] GetActionMapping (System.Web.Http.Controllers.HttpControllerDescriptor controllerDescriptor)
	public virtual System.Web.Http.Controllers.HttpActionDescriptor SelectAction (System.Web.Http.Controllers.HttpControllerContext controllerContext)
}

[
ODataPathParameterBindingAttribute(),
]
public class System.Web.OData.Routing.ODataPath {
	public ODataPath (System.Collections.Generic.IList`1[[System.Web.OData.Routing.ODataPathSegment]] segments)
	public ODataPath (ODataPathSegment[] segments)

	Microsoft.OData.Edm.IEdmType EdmType  { public get; }
	Microsoft.OData.Edm.IEdmNavigationSource NavigationSource  { public get; }
	string PathTemplate  { public get; }
	System.Collections.ObjectModel.ReadOnlyCollection`1[[System.Web.OData.Routing.ODataPathSegment]] Segments  { public get; }

	public virtual string ToString ()
}

public class System.Web.OData.Routing.ODataPathRouteConstraint : IHttpRouteConstraint {
	public ODataPathRouteConstraint (IODataPathHandler pathHandler, Microsoft.OData.Edm.IEdmModel model, string routeName, System.Collections.Generic.IEnumerable`1[[System.Web.OData.Routing.Conventions.IODataRoutingConvention]] routingConventions)

	Microsoft.OData.Edm.IEdmModel EdmModel  { public get; }
	IODataPathHandler PathHandler  { public get; }
	string RouteName  { public get; }
	System.Collections.ObjectModel.Collection`1[[System.Web.OData.Routing.Conventions.IODataRoutingConvention]] RoutingConventions  { public get; }

	public virtual bool Match (System.Net.Http.HttpRequestMessage request, System.Web.Http.Routing.IHttpRoute route, string parameterName, System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] values, System.Web.Http.Routing.HttpRouteDirection routeDirection)
	protected virtual string SelectControllerName (ODataPath path, System.Net.Http.HttpRequestMessage request)
}

public class System.Web.OData.Routing.ODataPathSegmentTranslator : Microsoft.OData.Core.UriParser.Visitors.PathSegmentTranslator`1[[System.Collections.Generic.IEnumerable`1[[System.Web.OData.Routing.ODataPathSegment]]]] {
	public ODataPathSegmentTranslator (Microsoft.OData.Edm.IEdmModel model, bool enableUriTemplateParsing, System.Collections.Generic.IDictionary`2[[System.String],[Microsoft.OData.Core.UriParser.Semantic.SingleValueNode]] parameterAliasNodes)

	public virtual System.Collections.Generic.IEnumerable`1[[System.Web.OData.Routing.ODataPathSegment]] Translate (Microsoft.OData.Core.UriParser.Semantic.BatchReferenceSegment segment)
	public virtual System.Collections.Generic.IEnumerable`1[[System.Web.OData.Routing.ODataPathSegment]] Translate (Microsoft.OData.Core.UriParser.Semantic.BatchSegment segment)
	public virtual System.Collections.Generic.IEnumerable`1[[System.Web.OData.Routing.ODataPathSegment]] Translate (Microsoft.OData.Core.UriParser.Semantic.CountSegment segment)
	public virtual System.Collections.Generic.IEnumerable`1[[System.Web.OData.Routing.ODataPathSegment]] Translate (Microsoft.OData.Core.UriParser.Semantic.EntitySetSegment segment)
	public virtual System.Collections.Generic.IEnumerable`1[[System.Web.OData.Routing.ODataPathSegment]] Translate (Microsoft.OData.Core.UriParser.Semantic.KeySegment segment)
	public virtual System.Collections.Generic.IEnumerable`1[[System.Web.OData.Routing.ODataPathSegment]] Translate (Microsoft.OData.Core.UriParser.Semantic.MetadataSegment segment)
	public virtual System.Collections.Generic.IEnumerable`1[[System.Web.OData.Routing.ODataPathSegment]] Translate (Microsoft.OData.Core.UriParser.Semantic.NavigationPropertyLinkSegment segment)
	public virtual System.Collections.Generic.IEnumerable`1[[System.Web.OData.Routing.ODataPathSegment]] Translate (Microsoft.OData.Core.UriParser.Semantic.NavigationPropertySegment segment)
	public virtual System.Collections.Generic.IEnumerable`1[[System.Web.OData.Routing.ODataPathSegment]] Translate (Microsoft.OData.Core.UriParser.Semantic.OpenPropertySegment segment)
	public virtual System.Collections.Generic.IEnumerable`1[[System.Web.OData.Routing.ODataPathSegment]] Translate (Microsoft.OData.Core.UriParser.Semantic.OperationImportSegment segment)
	public virtual System.Collections.Generic.IEnumerable`1[[System.Web.OData.Routing.ODataPathSegment]] Translate (Microsoft.OData.Core.UriParser.Semantic.OperationSegment segment)
	public virtual System.Collections.Generic.IEnumerable`1[[System.Web.OData.Routing.ODataPathSegment]] Translate (Microsoft.OData.Core.UriParser.Semantic.PathTemplateSegment segment)
	public virtual System.Collections.Generic.IEnumerable`1[[System.Web.OData.Routing.ODataPathSegment]] Translate (Microsoft.OData.Core.UriParser.Semantic.PropertySegment segment)
	public virtual System.Collections.Generic.IEnumerable`1[[System.Web.OData.Routing.ODataPathSegment]] Translate (Microsoft.OData.Core.UriParser.Semantic.SingletonSegment segment)
	public virtual System.Collections.Generic.IEnumerable`1[[System.Web.OData.Routing.ODataPathSegment]] Translate (Microsoft.OData.Core.UriParser.Semantic.TypeSegment segment)
	public virtual System.Collections.Generic.IEnumerable`1[[System.Web.OData.Routing.ODataPathSegment]] Translate (Microsoft.OData.Core.UriParser.Semantic.ValueSegment segment)
	public static ODataPath TranslateODataLibPathToWebApiPath (Microsoft.OData.Core.UriParser.Semantic.ODataPath path, Microsoft.OData.Edm.IEdmModel model, UnresolvedPathSegment unresolvedPathSegment, Microsoft.OData.Core.UriParser.Semantic.KeySegment id, bool enableUriTemplateParsing, System.Collections.Generic.IDictionary`2[[System.String],[Microsoft.OData.Core.UriParser.Semantic.SingleValueNode]] parameterAliasNodes)
}

public class System.Web.OData.Routing.ODataPathTemplate {
	public ODataPathTemplate (System.Collections.Generic.IList`1[[System.Web.OData.Routing.ODataPathSegmentTemplate]] segments)
	public ODataPathTemplate (ODataPathSegmentTemplate[] segments)

	System.Collections.ObjectModel.ReadOnlyCollection`1[[System.Web.OData.Routing.ODataPathSegmentTemplate]] Segments  { public get; }

	public bool TryMatch (ODataPath path, System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] values)
}

public class System.Web.OData.Routing.ODataRoute : System.Web.Http.Routing.HttpRoute, IHttpRoute {
	public ODataRoute (string routePrefix, System.Web.Http.Routing.IHttpRouteConstraint routeConstraint)
	public ODataRoute (string routePrefix, ODataPathRouteConstraint pathConstraint)
	public ODataRoute (string routePrefix, System.Web.Http.Routing.IHttpRouteConstraint routeConstraint, System.Web.Http.Routing.HttpRouteValueDictionary defaults, System.Web.Http.Routing.HttpRouteValueDictionary constraints, System.Web.Http.Routing.HttpRouteValueDictionary dataTokens, System.Net.Http.HttpMessageHandler handler)
	public ODataRoute (string routePrefix, ODataPathRouteConstraint pathConstraint, System.Web.Http.Routing.HttpRouteValueDictionary defaults, System.Web.Http.Routing.HttpRouteValueDictionary constraints, System.Web.Http.Routing.HttpRouteValueDictionary dataTokens, System.Net.Http.HttpMessageHandler handler)

	ODataPathRouteConstraint PathRouteConstraint  { public get; }
	System.Web.Http.Routing.IHttpRouteConstraint RouteConstraint  { public get; }
	string RoutePrefix  { public get; }

	public virtual System.Web.Http.Routing.IHttpVirtualPathData GetVirtualPath (System.Net.Http.HttpRequestMessage request, System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] values)
	[
	ObsoleteAttribute(),
	]
	public ODataRoute HasRelaxedODataVersionConstraint ()
}

public class System.Web.OData.Routing.ODataVersionConstraint : IHttpRouteConstraint {
	public ODataVersionConstraint ()

	bool IsRelaxedMatch  { public get; public set; }
	Microsoft.OData.Core.ODataVersion Version  { public get; }

	public virtual bool Match (System.Net.Http.HttpRequestMessage request, System.Web.Http.Routing.IHttpRoute route, string parameterName, System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] values, System.Web.Http.Routing.HttpRouteDirection routeDirection)
}

public class System.Web.OData.Routing.PropertyAccessPathSegment : ODataPathSegment {
	public PropertyAccessPathSegment (Microsoft.OData.Edm.IEdmProperty property)
	public PropertyAccessPathSegment (string propertyName)

	Microsoft.OData.Edm.IEdmProperty Property  { public get; }
	string PropertyName  { public get; }
	string SegmentKind  { public virtual get; }

	public virtual Microsoft.OData.Edm.IEdmType GetEdmType (Microsoft.OData.Edm.IEdmType previousEdmType)
	public virtual Microsoft.OData.Edm.IEdmNavigationSource GetNavigationSource (Microsoft.OData.Edm.IEdmNavigationSource previousNavigationSource)
	public virtual string ToString ()
	public virtual bool TryMatch (ODataPathSegment pathSegment, System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] values)
}

public class System.Web.OData.Routing.RefPathSegment : ODataPathSegment {
	public RefPathSegment ()

	string SegmentKind  { public virtual get; }

	public virtual Microsoft.OData.Edm.IEdmType GetEdmType (Microsoft.OData.Edm.IEdmType previousEdmType)
	public virtual Microsoft.OData.Edm.IEdmNavigationSource GetNavigationSource (Microsoft.OData.Edm.IEdmNavigationSource previousNavigationSource)
	public virtual string ToString ()
	public virtual bool TryMatch (ODataPathSegment pathSegment, System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] values)
}

public class System.Web.OData.Routing.SingletonPathSegment : ODataPathSegment {
	public SingletonPathSegment (Microsoft.OData.Edm.IEdmSingleton singleton)
	public SingletonPathSegment (string singletonName)

	string SegmentKind  { public virtual get; }
	Microsoft.OData.Edm.IEdmSingleton Singleton  { public get; }
	string SingletonName  { public get; }

	public virtual Microsoft.OData.Edm.IEdmType GetEdmType (Microsoft.OData.Edm.IEdmType previousEdmType)
	public virtual Microsoft.OData.Edm.IEdmNavigationSource GetNavigationSource (Microsoft.OData.Edm.IEdmNavigationSource previousNavigationSource)
	public virtual string ToString ()
	public virtual bool TryMatch (ODataPathSegment pathSegment, System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] values)
}

public class System.Web.OData.Routing.UnboundActionPathSegment : ODataPathSegment {
	public UnboundActionPathSegment (Microsoft.OData.Edm.IEdmActionImport action)

	Microsoft.OData.Edm.IEdmActionImport Action  { public get; }
	string ActionName  { public get; }
	string SegmentKind  { public virtual get; }

	public virtual Microsoft.OData.Edm.IEdmType GetEdmType (Microsoft.OData.Edm.IEdmType previousEdmType)
	public virtual Microsoft.OData.Edm.IEdmNavigationSource GetNavigationSource (Microsoft.OData.Edm.IEdmNavigationSource previousNavigationSource)
	public virtual string ToString ()
	public virtual bool TryMatch (ODataPathSegment pathSegment, System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] values)
}

public class System.Web.OData.Routing.UnboundFunctionPathSegment : ODataPathSegment {
	public UnboundFunctionPathSegment (Microsoft.OData.Edm.IEdmFunctionImport function, Microsoft.OData.Edm.IEdmModel model, System.Collections.Generic.IDictionary`2[[System.String],[System.String]] parameterValues)

	Microsoft.OData.Edm.IEdmFunctionImport Function  { public get; }
	string FunctionName  { public get; }
	string SegmentKind  { public virtual get; }

	public virtual Microsoft.OData.Edm.IEdmType GetEdmType (Microsoft.OData.Edm.IEdmType previousEdmType)
	public virtual Microsoft.OData.Edm.IEdmNavigationSource GetNavigationSource (Microsoft.OData.Edm.IEdmNavigationSource previousNavigationSource)
	public object GetParameterValue (string parameterName)
	public virtual string ToString ()
	public virtual bool TryMatch (ODataPathSegment pathSegment, System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] values)
}

public class System.Web.OData.Routing.UnboundFunctionPathSegmentTemplate : ODataPathSegmentTemplate {
	public UnboundFunctionPathSegmentTemplate (UnboundFunctionPathSegment function)

	System.Collections.Generic.IDictionary`2[[System.String],[System.String]] ParameterMappings  { public get; }

	public virtual bool TryMatch (ODataPathSegment pathSegment, System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] values)
}

public class System.Web.OData.Routing.UnresolvedPathSegment : ODataPathSegment {
	public UnresolvedPathSegment (string segmentValue)

	string SegmentKind  { public virtual get; }
	string SegmentValue  { public get; }

	public virtual Microsoft.OData.Edm.IEdmType GetEdmType (Microsoft.OData.Edm.IEdmType previousEdmType)
	public virtual Microsoft.OData.Edm.IEdmNavigationSource GetNavigationSource (Microsoft.OData.Edm.IEdmNavigationSource previousNavigationSource)
	public virtual string ToString ()
	public virtual bool TryMatch (ODataPathSegment pathSegment, System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] values)
}

public class System.Web.OData.Routing.ValuePathSegment : ODataPathSegment {
	public ValuePathSegment ()

	string SegmentKind  { public virtual get; }

	public virtual Microsoft.OData.Edm.IEdmType GetEdmType (Microsoft.OData.Edm.IEdmType previousEdmType)
	public virtual Microsoft.OData.Edm.IEdmNavigationSource GetNavigationSource (Microsoft.OData.Edm.IEdmNavigationSource previousNavigationSource)
	public virtual string ToString ()
	public virtual bool TryMatch (ODataPathSegment pathSegment, System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] values)
}

[
AttributeUsageAttribute(),
]
public sealed class System.Web.OData.Routing.ODataPathParameterBindingAttribute : System.Web.Http.ParameterBindingAttribute, _Attribute {
	public ODataPathParameterBindingAttribute ()

	public virtual System.Web.Http.Controllers.HttpParameterBinding GetBinding (System.Web.Http.Controllers.HttpParameterDescriptor parameter)
}

[
AttributeUsageAttribute(),
]
public sealed class System.Web.OData.Routing.ODataRouteAttribute : System.Attribute, _Attribute {
	public ODataRouteAttribute ()
	public ODataRouteAttribute (string pathTemplate)

	string PathTemplate  { public get; }
}

[
AttributeUsageAttribute(),
]
public sealed class System.Web.OData.Routing.ODataRoutePrefixAttribute : System.Attribute, _Attribute {
	public ODataRoutePrefixAttribute (string prefix)

	string Prefix  { public get; }
}

public abstract class System.Web.OData.Formatter.Deserialization.ODataDeserializer {
	protected ODataDeserializer (Microsoft.OData.Core.ODataPayloadKind payloadKind)

	Microsoft.OData.Core.ODataPayloadKind ODataPayloadKind  { public get; }

	public virtual object Read (Microsoft.OData.Core.ODataMessageReader messageReader, System.Type type, ODataDeserializerContext readContext)
}

public abstract class System.Web.OData.Formatter.Deserialization.ODataDeserializerProvider {
	protected ODataDeserializerProvider ()

	public abstract ODataEdmTypeDeserializer GetEdmTypeDeserializer (Microsoft.OData.Edm.IEdmTypeReference edmType)
	public abstract ODataDeserializer GetODataDeserializer (Microsoft.OData.Edm.IEdmModel model, System.Type type, System.Net.Http.HttpRequestMessage request)
}

public abstract class System.Web.OData.Formatter.Deserialization.ODataEdmTypeDeserializer : ODataDeserializer {
	protected ODataEdmTypeDeserializer (Microsoft.OData.Core.ODataPayloadKind payloadKind)
	protected ODataEdmTypeDeserializer (Microsoft.OData.Core.ODataPayloadKind payloadKind, ODataDeserializerProvider deserializerProvider)

	ODataDeserializerProvider DeserializerProvider  { public get; }

	public virtual object ReadInline (object item, Microsoft.OData.Edm.IEdmTypeReference edmType, ODataDeserializerContext readContext)
}

public abstract class System.Web.OData.Formatter.Deserialization.ODataItemBase {
	protected ODataItemBase (Microsoft.OData.Core.ODataItem item)

	Microsoft.OData.Core.ODataItem Item  { public get; }
}

public class System.Web.OData.Formatter.Deserialization.DefaultODataDeserializerProvider : ODataDeserializerProvider {
	public DefaultODataDeserializerProvider ()

	DefaultODataDeserializerProvider Instance  { public static get; }

	public virtual ODataEdmTypeDeserializer GetEdmTypeDeserializer (Microsoft.OData.Edm.IEdmTypeReference edmType)
	public virtual ODataDeserializer GetODataDeserializer (Microsoft.OData.Edm.IEdmModel model, System.Type type, System.Net.Http.HttpRequestMessage request)
}

public class System.Web.OData.Formatter.Deserialization.ODataActionPayloadDeserializer : ODataDeserializer {
	public ODataActionPayloadDeserializer (ODataDeserializerProvider deserializerProvider)

	ODataDeserializerProvider DeserializerProvider  { public get; }

	public virtual object Read (Microsoft.OData.Core.ODataMessageReader messageReader, System.Type type, ODataDeserializerContext readContext)
}

public class System.Web.OData.Formatter.Deserialization.ODataCollectionDeserializer : ODataEdmTypeDeserializer {
	public ODataCollectionDeserializer (ODataDeserializerProvider deserializerProvider)

	public virtual object Read (Microsoft.OData.Core.ODataMessageReader messageReader, System.Type type, ODataDeserializerContext readContext)
	public virtual System.Collections.IEnumerable ReadCollectionValue (Microsoft.OData.Core.ODataCollectionValue collectionValue, Microsoft.OData.Edm.IEdmTypeReference elementType, ODataDeserializerContext readContext)
	public virtual object ReadInline (object item, Microsoft.OData.Edm.IEdmTypeReference edmType, ODataDeserializerContext readContext)
}

public class System.Web.OData.Formatter.Deserialization.ODataComplexTypeDeserializer : ODataEdmTypeDeserializer {
	public ODataComplexTypeDeserializer (ODataDeserializerProvider deserializerProvider)

	public virtual object Read (Microsoft.OData.Core.ODataMessageReader messageReader, System.Type type, ODataDeserializerContext readContext)
	public virtual object ReadComplexValue (Microsoft.OData.Core.ODataComplexValue complexValue, Microsoft.OData.Edm.IEdmComplexTypeReference complexType, ODataDeserializerContext readContext)
	public virtual object ReadInline (object item, Microsoft.OData.Edm.IEdmTypeReference edmType, ODataDeserializerContext readContext)
}

public class System.Web.OData.Formatter.Deserialization.ODataDeserializerContext {
	public ODataDeserializerContext ()

	Microsoft.OData.Edm.IEdmModel Model  { public get; public set; }
	ODataPath Path  { public get; public set; }
	System.Net.Http.HttpRequestMessage Request  { public get; public set; }
	System.Web.Http.Controllers.HttpRequestContext RequestContext  { public get; public set; }
	Microsoft.OData.Edm.IEdmTypeReference ResourceEdmType  { public get; public set; }
	System.Type ResourceType  { public get; public set; }
}

public class System.Web.OData.Formatter.Deserialization.ODataEntityDeserializer : ODataEdmTypeDeserializer {
	public ODataEntityDeserializer (ODataDeserializerProvider deserializerProvider)

	public virtual void ApplyNavigationProperties (object entityResource, ODataEntryWithNavigationLinks entryWrapper, Microsoft.OData.Edm.IEdmEntityTypeReference entityType, ODataDeserializerContext readContext)
	public virtual void ApplyNavigationProperty (object entityResource, ODataNavigationLinkWithItems navigationLinkWrapper, Microsoft.OData.Edm.IEdmEntityTypeReference entityType, ODataDeserializerContext readContext)
	public virtual void ApplyStructuralProperties (object entityResource, ODataEntryWithNavigationLinks entryWrapper, Microsoft.OData.Edm.IEdmEntityTypeReference entityType, ODataDeserializerContext readContext)
	public virtual void ApplyStructuralProperty (object entityResource, Microsoft.OData.Core.ODataProperty structuralProperty, Microsoft.OData.Edm.IEdmEntityTypeReference entityType, ODataDeserializerContext readContext)
	public virtual object CreateEntityResource (Microsoft.OData.Edm.IEdmEntityTypeReference entityType, ODataDeserializerContext readContext)
	public virtual object Read (Microsoft.OData.Core.ODataMessageReader messageReader, System.Type type, ODataDeserializerContext readContext)
	public virtual object ReadEntry (ODataEntryWithNavigationLinks entryWrapper, Microsoft.OData.Edm.IEdmEntityTypeReference entityType, ODataDeserializerContext readContext)
	public static ODataItemBase ReadEntryOrFeed (Microsoft.OData.Core.ODataReader reader)
	public virtual object ReadInline (object item, Microsoft.OData.Edm.IEdmTypeReference edmType, ODataDeserializerContext readContext)
}

public class System.Web.OData.Formatter.Deserialization.ODataEntityReferenceLinkBase : ODataItemBase {
	public ODataEntityReferenceLinkBase (Microsoft.OData.Core.ODataEntityReferenceLink item)

	Microsoft.OData.Core.ODataEntityReferenceLink EntityReferenceLink  { public get; }
}

public class System.Web.OData.Formatter.Deserialization.ODataEntityReferenceLinkDeserializer : ODataDeserializer {
	public ODataEntityReferenceLinkDeserializer ()

	public virtual object Read (Microsoft.OData.Core.ODataMessageReader messageReader, System.Type type, ODataDeserializerContext readContext)
}

public class System.Web.OData.Formatter.Deserialization.ODataEnumDeserializer : ODataEdmTypeDeserializer {
	public ODataEnumDeserializer ()

	public virtual object Read (Microsoft.OData.Core.ODataMessageReader messageReader, System.Type type, ODataDeserializerContext readContext)
	public virtual object ReadInline (object item, Microsoft.OData.Edm.IEdmTypeReference edmType, ODataDeserializerContext readContext)
}

public class System.Web.OData.Formatter.Deserialization.ODataFeedDeserializer : ODataEdmTypeDeserializer {
	public ODataFeedDeserializer (ODataDeserializerProvider deserializerProvider)

	public virtual System.Collections.IEnumerable ReadFeed (ODataFeedWithEntries feed, Microsoft.OData.Edm.IEdmEntityTypeReference elementType, ODataDeserializerContext readContext)
	public virtual object ReadInline (object item, Microsoft.OData.Edm.IEdmTypeReference edmType, ODataDeserializerContext readContext)
}

public class System.Web.OData.Formatter.Deserialization.ODataPrimitiveDeserializer : ODataEdmTypeDeserializer {
	public ODataPrimitiveDeserializer ()

	public virtual object Read (Microsoft.OData.Core.ODataMessageReader messageReader, System.Type type, ODataDeserializerContext readContext)
	public virtual object ReadInline (object item, Microsoft.OData.Edm.IEdmTypeReference edmType, ODataDeserializerContext readContext)
	public virtual object ReadPrimitive (Microsoft.OData.Core.ODataProperty primitiveProperty, ODataDeserializerContext readContext)
}

public sealed class System.Web.OData.Formatter.Deserialization.ODataEntryWithNavigationLinks : ODataItemBase {
	public ODataEntryWithNavigationLinks (Microsoft.OData.Core.ODataEntry item)

	Microsoft.OData.Core.ODataEntry Entry  { public get; }
	System.Collections.Generic.IList`1[[System.Web.OData.Formatter.Deserialization.ODataNavigationLinkWithItems]] NavigationLinks  { public get; }
}

public sealed class System.Web.OData.Formatter.Deserialization.ODataFeedWithEntries : ODataItemBase {
	public ODataFeedWithEntries (Microsoft.OData.Core.ODataFeed item)

	System.Collections.Generic.IList`1[[System.Web.OData.Formatter.Deserialization.ODataEntryWithNavigationLinks]] Entries  { public get; }
	Microsoft.OData.Core.ODataFeed Feed  { public get; }
}

public sealed class System.Web.OData.Formatter.Deserialization.ODataNavigationLinkWithItems : ODataItemBase {
	public ODataNavigationLinkWithItems (Microsoft.OData.Core.ODataNavigationLink item)

	Microsoft.OData.Core.ODataNavigationLink NavigationLink  { public get; }
	System.Collections.Generic.IList`1[[System.Web.OData.Formatter.Deserialization.ODataItemBase]] NestedItems  { public get; }
}

public abstract class System.Web.OData.Formatter.Serialization.ODataEdmTypeSerializer : ODataSerializer {
	protected ODataEdmTypeSerializer (Microsoft.OData.Core.ODataPayloadKind payloadKind)
	protected ODataEdmTypeSerializer (Microsoft.OData.Core.ODataPayloadKind payloadKind, ODataSerializerProvider serializerProvider)

	ODataSerializerProvider SerializerProvider  { public get; }

	public virtual Microsoft.OData.Core.ODataValue CreateODataValue (object graph, Microsoft.OData.Edm.IEdmTypeReference expectedType, ODataSerializerContext writeContext)
	internal virtual Microsoft.OData.Core.ODataProperty CreateProperty (object graph, Microsoft.OData.Edm.IEdmTypeReference expectedType, string elementName, ODataSerializerContext writeContext)
	public virtual void WriteObjectInline (object graph, Microsoft.OData.Edm.IEdmTypeReference expectedType, Microsoft.OData.Core.ODataWriter writer, ODataSerializerContext writeContext)
}

public abstract class System.Web.OData.Formatter.Serialization.ODataSerializer {
	protected ODataSerializer (Microsoft.OData.Core.ODataPayloadKind payloadKind)

	Microsoft.OData.Core.ODataPayloadKind ODataPayloadKind  { public get; }

	public virtual void WriteObject (object graph, System.Type type, Microsoft.OData.Core.ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
}

public abstract class System.Web.OData.Formatter.Serialization.ODataSerializerProvider {
	protected ODataSerializerProvider ()

	public abstract ODataEdmTypeSerializer GetEdmTypeSerializer (Microsoft.OData.Edm.IEdmTypeReference edmType)
	public abstract ODataSerializer GetODataPayloadSerializer (Microsoft.OData.Edm.IEdmModel model, System.Type type, System.Net.Http.HttpRequestMessage request)
}

public class System.Web.OData.Formatter.Serialization.DefaultODataSerializerProvider : ODataSerializerProvider {
	public DefaultODataSerializerProvider ()

	DefaultODataSerializerProvider Instance  { public static get; }

	public virtual ODataEdmTypeSerializer GetEdmTypeSerializer (Microsoft.OData.Edm.IEdmTypeReference edmType)
	public virtual ODataSerializer GetODataPayloadSerializer (Microsoft.OData.Edm.IEdmModel model, System.Type type, System.Net.Http.HttpRequestMessage request)
}

public class System.Web.OData.Formatter.Serialization.EntitySelfLinks {
	public EntitySelfLinks ()

	System.Uri EditLink  { public get; public set; }
	System.Uri IdLink  { public get; public set; }
	System.Uri ReadLink  { public get; public set; }
}

public class System.Web.OData.Formatter.Serialization.ODataCollectionSerializer : ODataEdmTypeSerializer {
	public ODataCollectionSerializer (ODataSerializerProvider serializerProvider)

	protected static void AddTypeNameAnnotationAsNeeded (Microsoft.OData.Core.ODataCollectionValue value, ODataMetadataLevel metadataLevel)
	public virtual Microsoft.OData.Core.ODataCollectionValue CreateODataCollectionValue (System.Collections.IEnumerable enumerable, Microsoft.OData.Edm.IEdmTypeReference elementType, ODataSerializerContext writeContext)
	public virtual Microsoft.OData.Core.ODataValue CreateODataValue (object graph, Microsoft.OData.Edm.IEdmTypeReference expectedType, ODataSerializerContext writeContext)
	internal virtual Microsoft.OData.Core.ODataProperty CreateProperty (object graph, Microsoft.OData.Edm.IEdmTypeReference expectedType, string elementName, ODataSerializerContext writeContext)
	public void WriteCollection (Microsoft.OData.Core.ODataCollectionWriter writer, object graph, Microsoft.OData.Edm.IEdmTypeReference collectionType, ODataSerializerContext writeContext)
	public virtual void WriteObject (object graph, System.Type type, Microsoft.OData.Core.ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
}

public class System.Web.OData.Formatter.Serialization.ODataComplexTypeSerializer : ODataEdmTypeSerializer {
	public ODataComplexTypeSerializer (ODataSerializerProvider serializerProvider)

	public virtual Microsoft.OData.Core.ODataComplexValue CreateODataComplexValue (object graph, Microsoft.OData.Edm.IEdmComplexTypeReference complexType, ODataSerializerContext writeContext)
	public virtual Microsoft.OData.Core.ODataValue CreateODataValue (object graph, Microsoft.OData.Edm.IEdmTypeReference expectedType, ODataSerializerContext writeContext)
	public virtual void WriteObject (object graph, System.Type type, Microsoft.OData.Core.ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
}

public class System.Web.OData.Formatter.Serialization.ODataDeltaFeedSerializer : ODataEdmTypeSerializer {
	public ODataDeltaFeedSerializer (ODataSerializerProvider serializerProvider)

	public virtual Microsoft.OData.Core.ODataDeltaFeed CreateODataDeltaFeed (System.Collections.IEnumerable feedInstance, Microsoft.OData.Edm.IEdmCollectionTypeReference feedType, ODataSerializerContext writeContext)
	public virtual void WriteDeltaDeletedEntry (object graph, Microsoft.OData.Core.ODataDeltaWriter writer, ODataSerializerContext writeContext)
	public virtual void WriteDeltaDeletedLink (object graph, Microsoft.OData.Core.ODataDeltaWriter writer, ODataSerializerContext writeContext)
	public virtual void WriteDeltaFeedInline (object graph, Microsoft.OData.Edm.IEdmTypeReference expectedType, Microsoft.OData.Core.ODataDeltaWriter writer, ODataSerializerContext writeContext)
	public virtual void WriteDeltaLink (object graph, Microsoft.OData.Core.ODataDeltaWriter writer, ODataSerializerContext writeContext)
	public virtual void WriteObject (object graph, System.Type type, Microsoft.OData.Core.ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
}

public class System.Web.OData.Formatter.Serialization.ODataEntityReferenceLinkSerializer : ODataSerializer {
	public ODataEntityReferenceLinkSerializer ()

	public virtual void WriteObject (object graph, System.Type type, Microsoft.OData.Core.ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
}

public class System.Web.OData.Formatter.Serialization.ODataEntityReferenceLinksSerializer : ODataSerializer {
	public ODataEntityReferenceLinksSerializer ()

	public virtual void WriteObject (object graph, System.Type type, Microsoft.OData.Core.ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
}

public class System.Web.OData.Formatter.Serialization.ODataEntityTypeSerializer : ODataEdmTypeSerializer {
	public ODataEntityTypeSerializer (ODataSerializerProvider serializerProvider)

	public virtual Microsoft.OData.Core.ODataEntry CreateEntry (SelectExpandNode selectExpandNode, EntityInstanceContext entityInstanceContext)
	public virtual string CreateETag (EntityInstanceContext entityInstanceContext)
	public virtual Microsoft.OData.Core.ODataNavigationLink CreateNavigationLink (Microsoft.OData.Edm.IEdmNavigationProperty navigationProperty, EntityInstanceContext entityInstanceContext)
	public virtual Microsoft.OData.Core.ODataAction CreateODataAction (Microsoft.OData.Edm.IEdmAction action, EntityInstanceContext entityInstanceContext)
	public virtual Microsoft.OData.Core.ODataFunction CreateODataFunction (Microsoft.OData.Edm.IEdmFunction function, EntityInstanceContext entityInstanceContext)
	public virtual SelectExpandNode CreateSelectExpandNode (EntityInstanceContext entityInstanceContext)
	public virtual Microsoft.OData.Core.ODataProperty CreateStructuralProperty (Microsoft.OData.Edm.IEdmStructuralProperty structuralProperty, EntityInstanceContext entityInstanceContext)
	public virtual void WriteDeltaObjectInline (object graph, Microsoft.OData.Edm.IEdmTypeReference expectedType, Microsoft.OData.Core.ODataDeltaWriter writer, ODataSerializerContext writeContext)
	public virtual void WriteObject (object graph, System.Type type, Microsoft.OData.Core.ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
	public virtual void WriteObjectInline (object graph, Microsoft.OData.Edm.IEdmTypeReference expectedType, Microsoft.OData.Core.ODataWriter writer, ODataSerializerContext writeContext)
}

public class System.Web.OData.Formatter.Serialization.ODataEnumSerializer : ODataEdmTypeSerializer {
	public ODataEnumSerializer ()

	public virtual Microsoft.OData.Core.ODataEnumValue CreateODataEnumValue (object graph, Microsoft.OData.Edm.IEdmEnumTypeReference enumType, ODataSerializerContext writeContext)
	public virtual Microsoft.OData.Core.ODataValue CreateODataValue (object graph, Microsoft.OData.Edm.IEdmTypeReference expectedType, ODataSerializerContext writeContext)
	public virtual void WriteObject (object graph, System.Type type, Microsoft.OData.Core.ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
}

public class System.Web.OData.Formatter.Serialization.ODataErrorSerializer : ODataSerializer {
	public ODataErrorSerializer ()

	public virtual void WriteObject (object graph, System.Type type, Microsoft.OData.Core.ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
}

public class System.Web.OData.Formatter.Serialization.ODataFeedSerializer : ODataEdmTypeSerializer {
	public ODataFeedSerializer (ODataSerializerProvider serializerProvider)

	public virtual Microsoft.OData.Core.ODataFeed CreateODataFeed (System.Collections.IEnumerable feedInstance, Microsoft.OData.Edm.IEdmCollectionTypeReference feedType, ODataSerializerContext writeContext)
	public virtual Microsoft.OData.Core.ODataOperation CreateODataOperation (Microsoft.OData.Edm.IEdmOperation operation, FeedContext feedContext, ODataSerializerContext writeContext)
	public virtual void WriteObject (object graph, System.Type type, Microsoft.OData.Core.ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
	public virtual void WriteObjectInline (object graph, Microsoft.OData.Edm.IEdmTypeReference expectedType, Microsoft.OData.Core.ODataWriter writer, ODataSerializerContext writeContext)
}

public class System.Web.OData.Formatter.Serialization.ODataMetadataSerializer : ODataSerializer {
	public ODataMetadataSerializer ()

	public virtual void WriteObject (object graph, System.Type type, Microsoft.OData.Core.ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
}

public class System.Web.OData.Formatter.Serialization.ODataPrimitiveSerializer : ODataEdmTypeSerializer {
	public ODataPrimitiveSerializer ()

	public virtual Microsoft.OData.Core.ODataPrimitiveValue CreateODataPrimitiveValue (object graph, Microsoft.OData.Edm.IEdmPrimitiveTypeReference primitiveType, ODataSerializerContext writeContext)
	public virtual Microsoft.OData.Core.ODataValue CreateODataValue (object graph, Microsoft.OData.Edm.IEdmTypeReference expectedType, ODataSerializerContext writeContext)
	public virtual void WriteObject (object graph, System.Type type, Microsoft.OData.Core.ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
}

public class System.Web.OData.Formatter.Serialization.ODataRawValueSerializer : ODataSerializer {
	public ODataRawValueSerializer ()

	public virtual void WriteObject (object graph, System.Type type, Microsoft.OData.Core.ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
}

public class System.Web.OData.Formatter.Serialization.ODataSerializerContext {
	public ODataSerializerContext ()
	public ODataSerializerContext (EntityInstanceContext entity, Microsoft.OData.Core.UriParser.Semantic.SelectExpandClause selectExpandClause, Microsoft.OData.Edm.IEdmNavigationProperty navigationProperty)

	EntityInstanceContext ExpandedEntity  { public get; public set; }
	System.Collections.Generic.IDictionary`2[[System.Object],[System.Object]] Items  { public get; }
	ODataMetadataLevel MetadataLevel  { public get; public set; }
	Microsoft.OData.Edm.IEdmModel Model  { public get; public set; }
	Microsoft.OData.Edm.IEdmNavigationProperty NavigationProperty  { public get; public set; }
	Microsoft.OData.Edm.IEdmNavigationSource NavigationSource  { public get; public set; }
	ODataPath Path  { public get; public set; }
	System.Net.Http.HttpRequestMessage Request  { public get; public set; }
	System.Web.Http.Controllers.HttpRequestContext RequestContext  { public get; public set; }
	string RootElementName  { public get; public set; }
	Microsoft.OData.Core.UriParser.Semantic.SelectExpandClause SelectExpandClause  { public get; public set; }
	bool SkipExpensiveAvailabilityChecks  { public get; public set; }
	System.Web.Http.Routing.UrlHelper Url  { public get; public set; }
}

public class System.Web.OData.Formatter.Serialization.ODataServiceDocumentSerializer : ODataSerializer {
	public ODataServiceDocumentSerializer ()

	public virtual void WriteObject (object graph, System.Type type, Microsoft.OData.Core.ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
}

public class System.Web.OData.Formatter.Serialization.SelectExpandNode {
	public SelectExpandNode ()
	public SelectExpandNode (Microsoft.OData.Edm.IEdmEntityType entityType, ODataSerializerContext writeContext)
	public SelectExpandNode (Microsoft.OData.Core.UriParser.Semantic.SelectExpandClause selectExpandClause, Microsoft.OData.Edm.IEdmEntityType entityType, Microsoft.OData.Edm.IEdmModel model)

	System.Collections.Generic.IDictionary`2[[Microsoft.OData.Edm.IEdmNavigationProperty],[Microsoft.OData.Core.UriParser.Semantic.SelectExpandClause]] ExpandedNavigationProperties  { public get; }
	bool SelectAllDynamicProperties  { public get; }
	System.Collections.Generic.ISet`1[[Microsoft.OData.Edm.IEdmAction]] SelectedActions  { public get; }
	System.Collections.Generic.ISet`1[[System.String]] SelectedDynamicProperties  { public get; }
	System.Collections.Generic.ISet`1[[Microsoft.OData.Edm.IEdmFunction]] SelectedFunctions  { public get; }
	System.Collections.Generic.ISet`1[[Microsoft.OData.Edm.IEdmNavigationProperty]] SelectedNavigationProperties  { public get; }
	System.Collections.Generic.ISet`1[[Microsoft.OData.Edm.IEdmStructuralProperty]] SelectedStructuralProperties  { public get; }
}

public class System.Web.OData.Query.Expressions.DynamicTypeWrapper {
	public DynamicTypeWrapper ()

	public virtual bool Equals (object obj)
	public virtual int GetHashCode ()
	public object GetPropertyValue (string propertyName)
	public void SetPropertyValue (string propertyName, object value)
	public bool TryGetPropertyValue (string propertyName, out System.Object& value)
}

public class System.Web.OData.Query.Validators.CountQueryValidator {
	public CountQueryValidator ()

	public virtual void Validate (CountQueryOption countQueryOption, ODataValidationSettings validationSettings)
}

public class System.Web.OData.Query.Validators.FilterQueryValidator {
	public FilterQueryValidator ()

	public virtual void Validate (FilterQueryOption filterQueryOption, ODataValidationSettings settings)
	public virtual void ValidateAllNode (Microsoft.OData.Core.UriParser.Semantic.AllNode allNode, ODataValidationSettings settings)
	public virtual void ValidateAnyNode (Microsoft.OData.Core.UriParser.Semantic.AnyNode anyNode, ODataValidationSettings settings)
	public virtual void ValidateArithmeticOperator (Microsoft.OData.Core.UriParser.Semantic.BinaryOperatorNode binaryNode, ODataValidationSettings settings)
	public virtual void ValidateBinaryOperatorNode (Microsoft.OData.Core.UriParser.Semantic.BinaryOperatorNode binaryOperatorNode, ODataValidationSettings settings)
	public virtual void ValidateCollectionPropertyAccessNode (Microsoft.OData.Core.UriParser.Semantic.CollectionPropertyAccessNode propertyAccessNode, ODataValidationSettings settings)
	public virtual void ValidateConstantNode (Microsoft.OData.Core.UriParser.Semantic.ConstantNode constantNode, ODataValidationSettings settings)
	public virtual void ValidateConvertNode (Microsoft.OData.Core.UriParser.Semantic.ConvertNode convertNode, ODataValidationSettings settings)
	public virtual void ValidateEntityCollectionCastNode (Microsoft.OData.Core.UriParser.Semantic.EntityCollectionCastNode entityCollectionCastNode, ODataValidationSettings settings)
	public virtual void ValidateLogicalOperator (Microsoft.OData.Core.UriParser.Semantic.BinaryOperatorNode binaryNode, ODataValidationSettings settings)
	public virtual void ValidateNavigationPropertyNode (Microsoft.OData.Core.UriParser.Semantic.QueryNode sourceNode, Microsoft.OData.Edm.IEdmNavigationProperty navigationProperty, ODataValidationSettings settings)
	public virtual void ValidateQueryNode (Microsoft.OData.Core.UriParser.Semantic.QueryNode node, ODataValidationSettings settings)
	public virtual void ValidateRangeVariable (Microsoft.OData.Core.UriParser.Semantic.RangeVariable rangeVariable, ODataValidationSettings settings)
	public virtual void ValidateSingleEntityCastNode (Microsoft.OData.Core.UriParser.Semantic.SingleEntityCastNode singleEntityCastNode, ODataValidationSettings settings)
	public virtual void ValidateSingleEntityFunctionCallNode (Microsoft.OData.Core.UriParser.Semantic.SingleEntityFunctionCallNode node, ODataValidationSettings settings)
	public virtual void ValidateSingleValueFunctionCallNode (Microsoft.OData.Core.UriParser.Semantic.SingleValueFunctionCallNode node, ODataValidationSettings settings)
	public virtual void ValidateSingleValuePropertyAccessNode (Microsoft.OData.Core.UriParser.Semantic.SingleValuePropertyAccessNode propertyAccessNode, ODataValidationSettings settings)
	public virtual void ValidateUnaryOperatorNode (Microsoft.OData.Core.UriParser.Semantic.UnaryOperatorNode unaryOperatorNode, ODataValidationSettings settings)
}

public class System.Web.OData.Query.Validators.ODataQueryValidator {
	public ODataQueryValidator ()

	public virtual void Validate (ODataQueryOptions options, ODataValidationSettings validationSettings)
}

public class System.Web.OData.Query.Validators.OrderByQueryValidator {
	public OrderByQueryValidator ()

	public virtual void Validate (OrderByQueryOption orderByOption, ODataValidationSettings validationSettings)
}

public class System.Web.OData.Query.Validators.SelectExpandQueryValidator {
	public SelectExpandQueryValidator ()

	public virtual void Validate (SelectExpandQueryOption selectExpandQueryOption, ODataValidationSettings validationSettings)
}

public class System.Web.OData.Query.Validators.SkipQueryValidator {
	public SkipQueryValidator ()

	public virtual void Validate (SkipQueryOption skipQueryOption, ODataValidationSettings validationSettings)
}

public class System.Web.OData.Query.Validators.TopQueryValidator {
	public TopQueryValidator ()

	public virtual void Validate (TopQueryOption topQueryOption, ODataValidationSettings validationSettings)
}

public interface System.Web.OData.Routing.Conventions.IODataRoutingConvention {
	string SelectAction (ODataPath odataPath, System.Web.Http.Controllers.HttpControllerContext controllerContext, System.Linq.ILookup`2[[System.String],[System.Web.Http.Controllers.HttpActionDescriptor]] actionMap)
	string SelectController (ODataPath odataPath, System.Net.Http.HttpRequestMessage request)
}

public abstract class System.Web.OData.Routing.Conventions.NavigationSourceRoutingConvention : IODataRoutingConvention {
	protected NavigationSourceRoutingConvention ()

	public abstract string SelectAction (ODataPath odataPath, System.Web.Http.Controllers.HttpControllerContext controllerContext, System.Linq.ILookup`2[[System.String],[System.Web.Http.Controllers.HttpActionDescriptor]] actionMap)
	public virtual string SelectController (ODataPath odataPath, System.Net.Http.HttpRequestMessage request)
}

public sealed class System.Web.OData.Routing.Conventions.ODataRoutingConventions {
	public static System.Collections.Generic.IList`1[[System.Web.OData.Routing.Conventions.IODataRoutingConvention]] CreateDefault ()
	public static System.Collections.Generic.IList`1[[System.Web.OData.Routing.Conventions.IODataRoutingConvention]] CreateDefaultWithAttributeRouting (System.Web.Http.HttpConfiguration configuration, Microsoft.OData.Edm.IEdmModel model)
}

public class System.Web.OData.Routing.Conventions.ActionRoutingConvention : NavigationSourceRoutingConvention, IODataRoutingConvention {
	public ActionRoutingConvention ()

	public virtual string SelectAction (ODataPath odataPath, System.Web.Http.Controllers.HttpControllerContext controllerContext, System.Linq.ILookup`2[[System.String],[System.Web.Http.Controllers.HttpActionDescriptor]] actionMap)
}

public class System.Web.OData.Routing.Conventions.AttributeRoutingConvention : IODataRoutingConvention {
	public AttributeRoutingConvention (Microsoft.OData.Edm.IEdmModel model, System.Collections.Generic.IEnumerable`1[[System.Web.Http.Controllers.HttpControllerDescriptor]] controllers)
	public AttributeRoutingConvention (Microsoft.OData.Edm.IEdmModel model, System.Web.Http.HttpConfiguration configuration)
	public AttributeRoutingConvention (Microsoft.OData.Edm.IEdmModel model, System.Collections.Generic.IEnumerable`1[[System.Web.Http.Controllers.HttpControllerDescriptor]] controllers, IODataPathTemplateHandler pathTemplateHandler)
	public AttributeRoutingConvention (Microsoft.OData.Edm.IEdmModel model, System.Web.Http.HttpConfiguration configuration, IODataPathTemplateHandler pathTemplateHandler)

	Microsoft.OData.Edm.IEdmModel Model  { public get; }
	IODataPathTemplateHandler ODataPathTemplateHandler  { public get; }

	public virtual string SelectAction (ODataPath odataPath, System.Web.Http.Controllers.HttpControllerContext controllerContext, System.Linq.ILookup`2[[System.String],[System.Web.Http.Controllers.HttpActionDescriptor]] actionMap)
	public virtual string SelectController (ODataPath odataPath, System.Net.Http.HttpRequestMessage request)
	public virtual bool ShouldMapController (System.Web.Http.Controllers.HttpControllerDescriptor controller)
}

public class System.Web.OData.Routing.Conventions.DynamicPropertyRoutingConvention : NavigationSourceRoutingConvention, IODataRoutingConvention {
	public DynamicPropertyRoutingConvention ()

	public virtual string SelectAction (ODataPath odataPath, System.Web.Http.Controllers.HttpControllerContext controllerContext, System.Linq.ILookup`2[[System.String],[System.Web.Http.Controllers.HttpActionDescriptor]] actionMap)
}

public class System.Web.OData.Routing.Conventions.EntityRoutingConvention : NavigationSourceRoutingConvention, IODataRoutingConvention {
	public EntityRoutingConvention ()

	public virtual string SelectAction (ODataPath odataPath, System.Web.Http.Controllers.HttpControllerContext controllerContext, System.Linq.ILookup`2[[System.String],[System.Web.Http.Controllers.HttpActionDescriptor]] actionMap)
}

public class System.Web.OData.Routing.Conventions.EntitySetRoutingConvention : NavigationSourceRoutingConvention, IODataRoutingConvention {
	public EntitySetRoutingConvention ()

	public virtual string SelectAction (ODataPath odataPath, System.Web.Http.Controllers.HttpControllerContext controllerContext, System.Linq.ILookup`2[[System.String],[System.Web.Http.Controllers.HttpActionDescriptor]] actionMap)
}

public class System.Web.OData.Routing.Conventions.FunctionRoutingConvention : NavigationSourceRoutingConvention, IODataRoutingConvention {
	public FunctionRoutingConvention ()

	public virtual string SelectAction (ODataPath odataPath, System.Web.Http.Controllers.HttpControllerContext controllerContext, System.Linq.ILookup`2[[System.String],[System.Web.Http.Controllers.HttpActionDescriptor]] actionMap)
}

public class System.Web.OData.Routing.Conventions.MetadataRoutingConvention : IODataRoutingConvention {
	public MetadataRoutingConvention ()

	public virtual string SelectAction (ODataPath odataPath, System.Web.Http.Controllers.HttpControllerContext controllerContext, System.Linq.ILookup`2[[System.String],[System.Web.Http.Controllers.HttpActionDescriptor]] actionMap)
	public virtual string SelectController (ODataPath odataPath, System.Net.Http.HttpRequestMessage request)
}

public class System.Web.OData.Routing.Conventions.NavigationRoutingConvention : NavigationSourceRoutingConvention, IODataRoutingConvention {
	public NavigationRoutingConvention ()

	public virtual string SelectAction (ODataPath odataPath, System.Web.Http.Controllers.HttpControllerContext controllerContext, System.Linq.ILookup`2[[System.String],[System.Web.Http.Controllers.HttpActionDescriptor]] actionMap)
}

public class System.Web.OData.Routing.Conventions.PropertyRoutingConvention : NavigationSourceRoutingConvention, IODataRoutingConvention {
	public PropertyRoutingConvention ()

	public virtual string SelectAction (ODataPath odataPath, System.Web.Http.Controllers.HttpControllerContext controllerContext, System.Linq.ILookup`2[[System.String],[System.Web.Http.Controllers.HttpActionDescriptor]] actionMap)
}

public class System.Web.OData.Routing.Conventions.RefRoutingConvention : NavigationSourceRoutingConvention, IODataRoutingConvention {
	public RefRoutingConvention ()

	public virtual string SelectAction (ODataPath odataPath, System.Web.Http.Controllers.HttpControllerContext controllerContext, System.Linq.ILookup`2[[System.String],[System.Web.Http.Controllers.HttpActionDescriptor]] actionMap)
}

public class System.Web.OData.Routing.Conventions.SingletonRoutingConvention : NavigationSourceRoutingConvention, IODataRoutingConvention {
	public SingletonRoutingConvention ()

	public virtual string SelectAction (ODataPath odataPath, System.Web.Http.Controllers.HttpControllerContext controllerContext, System.Linq.ILookup`2[[System.String],[System.Web.Http.Controllers.HttpActionDescriptor]] actionMap)
}

public class System.Web.OData.Routing.Conventions.UnmappedRequestRoutingConvention : NavigationSourceRoutingConvention, IODataRoutingConvention {
	public UnmappedRequestRoutingConvention ()

	public virtual string SelectAction (ODataPath odataPath, System.Web.Http.Controllers.HttpControllerContext controllerContext, System.Linq.ILookup`2[[System.String],[System.Web.Http.Controllers.HttpActionDescriptor]] actionMap)
}

