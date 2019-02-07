# NAVIGATION PROPERTIES ON COMPLEX TYPES

## 1	OVERVIEW
### 1.1	INTRODUCTION
In WebAPI, navigation properties were only allowed on entity types. This design will now enable support for navigation properties on complex types as well. 
### 1.2	PROBLEM STATEMENT
We will ensure that the requests in the table below are supported.   

|Request	| Template |
|:-------:|:---------|
|GET |	{resource-path}?$expand=NavigationProperty |
|GET |	{resource-path}?$expand=Cast/NavigationProperty |
|GET |	{resource-path}?$expand=ComplexProperty/Cast/NavigationProperty |
|GET |	{resource-path}?$expand=ComplexProperty/NavigationProperty |
|GET | {resource-path}?$expand = ComplexProperty/NestedComplexProperty/NavigationProperty |
|GET |	{collection-resource-path}/{key}?$expand = ComplexProperty/NavigationProperty |
|GET | {collection-resource-path}/{key}?$expand = ComplexProp1/NavProp1, ComplexProp2/NavProp2 |
|GET| {resource-path}?$expand=ComplexProperty/NavigationProperty({query-options}) |
|GET| {resource-path}?$expand = ComplexProp/NestedComplexProperty/NavigationProperty {query-option} |
|POST|	{resource-path}/ComplexProperty/NavigationProperty/$ref|
|POST|	{collection-resource-path}/{key}/ ComplexProperty/NavigationProperty/$ref|

This list is not exhaustive by any means. It is just an indication of what kind of queries need to be supported. 
*	The GET requests currently fail on multiple levels (Serialization, SelectExpandNode creation, SelectExpandBinder etc.).
## 2	DESIGN
### Overview
From previous efforts, some of the things were already in place to support for navigation properties on complex types. For instance, convention model builder supported navigation properties on complex types, the deserialization of a payload to extract the navigation property worked correctly. 
This design will focus mainly on the things that need to be changed in order to support the above-mentioned requests and briefly touch upon other things.
### 2.1	Routing
Conventional routing does not support navigation properties on the complex types, and we will not enable that. The consumer is expected to use Attribute routing in order to route such requests. 
### 2.2	SelectExpandWrapper AND SelectExpandQueryOption
These classes assumed that the expand query could only be applied to an entity type, we will now require the type to be a structured type.
+	SelectExpandWrapper - This class is a wrapper class around the object on which select or expand is applied. Previously, the underlying object was TypedEdmEntityObject which we will change to TypedEdmStructuredObject.
+	SelectExpandQueryOption – We throw exceptions if the underlying type is not a structured type instead of entity type.  
### 2.3	SelectExpandClause Binding AND LINQ Expressions
The SelectExpandBinder expected an element type of entity type for a bunch of its methods. We will change it to a structured type.

In the method (CreatePropertyValueExpressionWithFilter) where we generate the LINQ expression, we simply assumed that the navigation property existed on the source object. Now, we pass in the ExpandedNavigationSelectItem to the LINQ generating and loop through the path to the navigation property to create an expression that can access properties on the nested properties.
### 2.4	SelectExpandNode
The SelectExpandNode had validation methods which prevented having a path in the expand query option. We will uplift that check. Along with that we added some more properties on this class, and we build expansions when there is no SelectExpandClause as well in certain cases which we will dive deeper into the serialization section.  
### 2.5	Serialization
When starting the serialization, the serializer context is passed with the SelectExpandClause from the request context which is the top-level SelectExpandClause.
Currently, we create a SelectExpandNode for each structured resource. SelectExpandNode has a property ExpandedNavigationProperties which is a dictionary of IEDMNavigationProperty and the ExpandedNavigationSelectItem from the SelectExpandClause. We use this dictionary to determine which navigation properties need to be written. 

However, for the IEDMNavigationProperty to be added to the dictionary, it needs to be present on the resource currently being serialized. 

For all such navigation properties that do not exist on the current resource, we will add them to the new dictionary property on the SelectExpandNode with IEDMProperty as the key and the List of ExpandedNavigationSelectItem being the value where the property will be next property to the current property in PathToNavigationProperty. For instance, if Entity/Prop1/Prop2/NavProp is the path to navigation property, we will store (Prop2 , List< ExpandedNavigationSelectItem >) in the dictionary when the SelectExpandNode is being created for Prop1.
So, when we come down to writing a complex property, we will check if our dictionary contains any select element in its list to the current property. We will pass that list down to the nested serializer context and use that to build and maintain both the above-mentioned dictionaries when we create the SelectExpandNode. 
### 2.6 Navigation Link
We update the navigation source for the complex property to be the navigation source of the parent property in the serializer context. We use that navigation source in GetNavigationSourceLinkBuilder extension method of the model to get the NavigationSourceLinkBuilder which generates the navigation link.


## Resources
If you find it easier to read code – [here is the link to the PR that has the prototype.](https://github.com/OData/WebApi/pull/1738) 
