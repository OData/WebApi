---
title : "4.8 Operation paramters in untyped scenarios"
layout: post
category: "4. OData features"
---

In this page, we introduce the Function/Action parameter in untyped scenario. For CLR typed scenarios, please refer to [Function page](http://odata.github.io/WebApi/Complex-Entity-As-Function-Parameter/) and [Action page](http://odata.github.io/WebApi/Action-Parameter-Support/).

### Build Edm Model

Let's build the Edm Model from scratch:

{% highlight csharp %}
private static IEdmModel GetEdmModel()
{
    EdmModel model = new EdmModel();

    // Enum type "Color"
    EdmEnumType colorEnum = new EdmEnumType("NS", "Color");
    colorEnum.AddMember(new EdmEnumMember(colorEnum, "Red", new EdmIntegerConstant(0)));
    colorEnum.AddMember(new EdmEnumMember(colorEnum, "Blue", new EdmIntegerConstant(1)));
    colorEnum.AddMember(new EdmEnumMember(colorEnum, "Green", new EdmIntegerConstant(2)));
    model.AddElement(colorEnum);

    // complex type "Address"
    EdmComplexType address = new EdmComplexType("NS", "Address");
    address.AddStructuralProperty("Street", EdmPrimitiveTypeKind.String);
    address.AddStructuralProperty("City", EdmPrimitiveTypeKind.String);
    model.AddElement(address);

    // derived complex type "SubAddress"
    EdmComplexType subAddress = new EdmComplexType("NS", "SubAddress", address);
    subAddress.AddStructuralProperty("Code", EdmPrimitiveTypeKind.Double);
    model.AddElement(subAddress);

    // entity type "Customer"
    EdmEntityType customer = new EdmEntityType("NS", "Customer");
    customer.AddKeys(customer.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32));
    customer.AddStructuralProperty("Name", EdmPrimitiveTypeKind.String);
    model.AddElement(customer);

    // derived entity type special customer
    EdmEntityType subCustomer = new EdmEntityType("NS", "SubCustomer", customer);
    subCustomer.AddStructuralProperty("Price", EdmPrimitiveTypeKind.Double);
    model.AddElement(subCustomer);

    // entity sets
    EdmEntityContainer container = new EdmEntityContainer("NS", "Default");
    model.AddElement(container);
    container.AddEntitySet("Customers", customer);

    IEdmTypeReference intType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Int32, isNullable: true);
    EdmEnumTypeReference enumType = new EdmEnumTypeReference(colorEnum, isNullable: true);
    EdmComplexTypeReference complexType = new EdmComplexTypeReference(address, isNullable: true);
    EdmEntityTypeReference entityType = new EdmEntityTypeReference(customer, isNullable: true);

    // functions
    BuildFunction(model, "PrimitiveFunction", entityType, "param", intType);
    BuildFunction(model, "EnumFunction", entityType, "color", enumType);
    BuildFunction(model, "ComplexFunction", entityType, "address", complexType);
    BuildFunction(model, "EntityFunction", entityType, "customer", entityType);
    
    // actions
    BuildAction(model, "PrimitiveAction", entityType, "param", intType);
    BuildAction(model, "EnumAction", entityType, "color", enumType);
    BuildAction(model, "ComplexAction", entityType, "address", complexType);
    BuildAction(model, "EntityAction", entityType, "customer", entityType);
    return model;
}

private static void BuildFunction(EdmModel model, string funcName, IEdmEntityTypeReference bindingType, string paramName, IEdmTypeReference edmType)
{
    IEdmTypeReference returnType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Boolean, isNullable: false);

    EdmFunction boundFunction = new EdmFunction("NS", funcName, returnType, isBound: true, entitySetPathExpression: null, isComposable: false);
    boundFunction.AddParameter("entity", bindingType);
    boundFunction.AddParameter(paramName, edmType);
    boundFunction.AddParameter(paramName + "List", new EdmCollectionTypeReference(new EdmCollectionType(edmType)));
    model.AddElement(boundFunction);
}

private static void BuildAction(EdmModel model, string actName, IEdmEntityTypeReference bindingType, string paramName, IEdmTypeReference edmType)
{
    IEdmTypeReference returnType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Boolean, isNullable: false);

    EdmAction boundAction = new EdmAction("NS", actName, returnType, isBound: true, entitySetPathExpression: null);
    boundAction.AddParameter("entity", bindingType);
    boundAction.AddParameter(paramName, edmType);
    boundAction.AddParameter(paramName + "List", new EdmCollectionTypeReference(new EdmCollectionType(edmType)));
    model.AddElement(boundAction);
}
{% endhighlight %}

Here's the metadata document for this Edm Model:

{% highlight xml %}
<?xml version="1.0" encoding="utf-8"?>
<edmx:Edmx Version="4.0" xmlns:edmx="http://docs.oasis-open.org/odata/ns/edmx">
  <edmx:DataServices>
    <Schema Namespace="NS" xmlns="http://docs.oasis-open.org/odata/ns/edm">
      <EnumType Name="Color">
        <Member Name="Red" Value="0" />
        <Member Name="Blue" Value="1" />
        <Member Name="Green" Value="2" />
      </EnumType>
      <ComplexType Name="Address">
        <Property Name="Street" Type="Edm.String" />
        <Property Name="City" Type="Edm.String" />
      </ComplexType>
      <ComplexType Name="SubAddress" BaseType="NS.Address">
        <Property Name="Code" Type="Edm.Double" />
      </ComplexType>
      <EntityType Name="Customer">
        <Key>
          <PropertyRef Name="Id" />
        </Key>
        <Property Name="Id" Type="Edm.Int32" />
        <Property Name="Name" Type="Edm.String" />
      </EntityType>
      <EntityType Name="SubCustomer" BaseType="NS.Customer">
        <Property Name="Price" Type="Edm.Double" />
      </EntityType>
      <Function Name="PrimitiveFunction" IsBound="true">
        <Parameter Name="entity" Type="NS.Customer" />
        <Parameter Name="param" Type="Edm.Int32" />
        <Parameter Name="paramList" Type="Collection(Edm.Int32)" />
        <ReturnType Type="Edm.Boolean" Nullable="false" />
      </Function>
      <Function Name="EnumFunction" IsBound="true">
        <Parameter Name="entity" Type="NS.Customer" />
        <Parameter Name="color" Type="NS.Address" />
        <Parameter Name="colorList" Type="Collection(NS.Color)" />
        <ReturnType Type="Edm.Boolean" Nullable="false" />
      </Function>
      <Function Name="ComplexFunction" IsBound="true">
        <Parameter Name="entity" Type="NS.Customer" />
        <Parameter Name="address" Type="NS.Address" />
        <Parameter Name="addressList" Type="Collection(NS.Address)" />
        <ReturnType Type="Edm.Boolean" Nullable="false" />
      </Function>
      <Function Name="EntityFunction" IsBound="true">
        <Parameter Name="entity" Type="NS.Customer" />
        <Parameter Name="customer" Type="NS.Color" />
        <Parameter Name="customerList" Type="Collection(NS.Customer)" />
        <ReturnType Type="Edm.Boolean" Nullable="false" />
      </Function>
      <Action Name="PrimitiveAction" IsBound="true">
        <Parameter Name="entity" Type="NS.Customer" />
        <Parameter Name="param" Type="Edm.Int32" />
        <Parameter Name="paramList" Type="Collection(Edm.Int32)" />
        <ReturnType Type="Edm.Boolean" Nullable="false" />
      </Action>
      <Action Name="EnumAction" IsBound="true">
        <Parameter Name="entity" Type="NS.Customer" />
        <Parameter Name="color" Type="NS.Address" />
        <Parameter Name="colorList" Type="Collection(NS.Color)" />
        <ReturnType Type="Edm.Boolean" Nullable="false" />
      </Action>
      <Action Name="ComplexAction" IsBound="true">
        <Parameter Name="entity" Type="NS.Customer" />
        <Parameter Name="address" Type="NS.Address" />
        <Parameter Name="addressList" Type="Collection(NS.Address)" />
        <ReturnType Type="Edm.Boolean" Nullable="false" />
      </Action>
      <Action Name="EntityAction" IsBound="true">
        <Parameter Name="entity" Type="NS.Customer" />
        <Parameter Name="customer" Type="NS.Color" />
        <Parameter Name="customerList" Type="Collection(NS.Customer)" />
        <ReturnType Type="Edm.Boolean" Nullable="false" />
      </Action>
      <EntityContainer Name="Default">
        <EntitySet Name="Customers" EntityType="NS.Customer" />
      </EntityContainer>
    </Schema>
  </edmx:DataServices>
</edmx:Edmx>
{% endhighlight %}

### Controller & Routing
Let's add the following methods into `CustomersController`:
{% highlight csharp %}
[HttpGet]
public IHttpActionResult PrimitiveFunction(int key, int? param, [FromODataUri]IList<int?> paramList)
{
    ......
}

[HttpPost]
public IHttpActionResult PrimitiveAction(int key, ODataActionParameters parameters)
{
    ......
}

/* // will support in V5.5 RTM
[HttpGet]
public IHttpActionResult EnumFunction(int key, [FromODataUri]EdmEnumObject color, [FromODataUri]EdmEnumObjectCollection colorList)
{
    ......
}

[HttpPost]
public IHttpActionResult EnumAction(int key, ODataActionParameters parameters)
{
    ......
}
*/

[HttpGet]
public IHttpActionResult ComplexFunction(int key, [FromODataUri]EdmComplexObject address, [FromODataUri]EdmComplexObjectCollection addressList)
{
    ......
}

[HttpPost]
public IHttpActionResult ComplexAction(int key, ODataActionParameters parameters)
{
    ......
}

[HttpGet]
public IHttpActionResult EntityFunction(int key, [FromODataUri]EdmEntityObject customer, [FromODataUri]EdmEntityObjectCollection customerList)
{
    ......
}

[HttpPost]
public IHttpActionResult EntityAction(int key, ODataActionParameters parameters)
{
    ......
}
{% endhighlight %}
#### Request Samples

Now, We can invoke the function with the entity and collection of entity parameter as:
{% highlight csharp %}
~odata/Customers(1)/NS.EntityFunction(customer=@x,customerList=@y)?@x={\"@odata.type\":\"%23NS.Customer\",\"Id\":1,\"Name\":\"John\"}&@y={\"value\":[{\"@odata.type\":\"%23NS.Customer\",\"Id\":2, \"Name\":\"Mike\"},{\"@odata.type\":\"%23NS.SubCustomer\",\"Id\":3,\"Name\":\"Tony\", \"Price\":9.9}]}"
{% endhighlight %}

Also, We can invoke the action by issuing a Post on `~/odata/Customers(1)/NS.EntityAction` with the following request body:
{% highlight csharp %}
{
  "customer":{\"@odata.type\":\"#NS.Customer\",\"Id\":1,\"Name\":\"John\"},
  "customerList":[
    {\"@odata.type\":\"#NS.Customer\",\"Id\":2, \"Name\":\"Mike\"},
    {\"@odata.type\":\"#NS.SubCustomer\",\"Id\":3,\"Name\":\"Tony\", \"Price\":9.9}
  ]
}
{% endhighlight %}

For other request samples, please refer to [Function page](http://odata.github.io/WebApi/Complex-Entity-As-Function-Parameter/) and [Action page](http://odata.github.io/WebApi/Action-Parameter-Support/).

### Unbound function/action

Unbound function and action are similiar with bound function and action in the request format. But only attribute routing can be used for unbound function/action routing.

Thanks.


