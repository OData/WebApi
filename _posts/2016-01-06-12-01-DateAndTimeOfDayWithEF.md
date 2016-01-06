---
layout: post
title: "12.1 Edm.Date and Edm.TimeOfDay with EF"
description: "How to Use Edm.Date and Edm.TimeOfDay with EntityFramework"
category: "12. Design"
---
### Problem
The Transact-SQL has __date (Format: YYYY-MM-DD)__ type, but there isn’t a CLR type representing date type. Entity Framework (EF) only supports to use `System.DateTime` CLR type to map the __date__ type. 

OData V4 lib provides a CLR `struct Date` type and the corresponding primitive type kind __Edm.Date__. Web API OData V4 supports it. However, EF doesn’t recognize this CLR type, and it can’t map `struct Date` directly to _date_ type.

So, this doc describes the solution about how to support __Edm.Date__ type with Entity Framework. Meanwhile, this doc also covers the __Edm.TimeOfDay_ type with EF.

### Scopes

It should support to map the type between __date__ type in Database and __Edm.Date__ type through the CLR `System.DateTime` type. The map is shown in the following figure:

![]({{site.baseurl}}/img/12-01-DateTypeMapping.png)

So, it should provide the below functionalities for the developer:

1.	Can configure the `System.DateTime`/`System.TimeSpan` property to __Edm.Date__/ __Edm.TimeOfDay__.
2.	Can serialize the __date__/ __time__ value in the DB as __Edm.Date__ /__Edm.TimeOfDay__ value format.
3.	Can de-serialize the __Edm.Date__/__Edm.TimeOfDay__ value as __date__/ __time__ value into DB.
4.	Can do query option on the ___date__/ __time__ value.


Most important, EF doesn’t support the primitive collection. So, Collection of date is not in the scope. The developer can use navigation property to work around.


### Detail Design

#### Date & Time type in SQL DB

Below is the date & time type mapping between DB and .NET:

![]({{site.baseurl}}/img/12-01-mapping3.png) 


So, From .NET view, only `System.DateTime` is used to represent the _date_ value, meanwhile only `System.TimeSpan` is used to represent the __time__ value.


#### Date & time mapping with EF 

In EF Code First, the developer can use two methodologies to map `System.DateTime` property to __date__ column in DB:

1 Data Annotation

The users can use the <strong>_Column_</strong> Data Annotation to modify the data type of columns. For example:

{% highlight csharp %}
  [Column(TypeName = "date")]
  public DateTime Birthday { get; set; }
{% endhighlight %}
“date” is case-insensitive.

2 Fluent API

`HasColumnName` is the Fluent API used to specify a column data type for a property. For example:
{% highlight csharp %}
modelBuilder.EntityType<Customer>()
            .Property(c => c.Birthday)
            .HasColumnType("date");
{% endhighlight %}

For __time__ type, it implicitly maps the `System.TimeSpan` to represent the __time__ value. However, you can use string literal "time"  in DataAnnotation or fluent API explicitly.


#### CLR Date Type in ODL

OData Library defines one `struct` to hold the value of __Edm.Date (Format: YYYY-MM-DD)__.

{% highlight csharp %}
namespace Microsoft.OData.Edm.Library
{
    // Summary:
    //     Date type for Edm.Date
    public struct Date : IComparable, IComparable<Date>, IEquatable<Date>
   {
         …
   }
}
{% endhighlight %}

Where, __Edm.Date__ is the corresponding primitive type Kind.

OData Library also defines one _struct_ to hold the value of __Edm.TimeOfDay (Format: HH:MM:SS. fractionalSeconds, where fractionalSeconds =1*12DIGIT)__.
{% highlight csharp %}
namespace Microsoft.OData.Edm.Library
{
    // Summary:
    //     Date type for Edm.TimeOfDay 
    public struct TimeOfDay  : IComparable , IComparable<TimeOfDay>, IEquatable<TimeOfDay>
   {
         …
   }
}
{% endhighlight %}
Where, __Edm.TimeOfDay__ is the corresponding primitive type Kind.

#### Configure Date & Time in Web API by Fluent API

__By default__, Web API has the following mapping between CLR types and Edm types:

![]({{site.baseurl}}/img/12-01-mapping1.png) 

We should provide a methodology to map `System.DateTime` to __Edm.Date__ type, and `System.TimeSpan` to __Edm.TimeOfDay__ type as follows:

![]({{site.baseurl}}/img/12-01-mapping2.png)

#### Extension methods

We will add the following extension methods to re-configure `System.DateTime` & `System.TimeSpan` property:

{% highlight csharp %}
public static class PrimitivePropertyConfigurationExtensions 
{ 
  public static PrimitivePropertyConfiguration AsDate(this PrimitivePropertyConfiguration property)
  {…}

  public static PrimitivePropertyConfiguration AsTimeOfDay(this PrimitivePropertyConfiguration property)
  {…}
} 
{% endhighlight %}

For example, the developer can use the above extension methods as follows:
{% highlight csharp %}
public class Customer
{
   …
   public DateTime Birthday {get;set;}
   public TimeSpan CreatedTime {get;set;}
}

ODataModelBuilder builder = new ODataModelBuilder();
EntityTypeConfiguration<Customer> customer = builder.EntityType<Customer>();
customer.Property(c => c.Birthday).AsDate();
customer.Property(c => c.CreatedTime).AsTimeOfDay();
IEdmModel model = builder.GetEdmModel();
{% endhighlight %}

#### Configure Date & Time in Web API by Data Annotation

We should recognize the __Column__ Data annotation. So, we will add a convention class as follows:
{% highlight csharp %}
internal class ColumnAttributeEdmPropertyConvention : AttributeEdmPropertyConvention<PropertyConfiguration>
{
  …
}
{% endhighlight %}

In this class, it will identify the __Column__ attribute applied to `System.DateTime` or `System.TimeSpan` property, and call `AsDate(…)` or `AsTimeOfDay()` extension methods to add a _Date_ or _TimeOfDay_ mapped property. Be caution, EF supports the TypeName case-insensitive.

After insert the instance of ColumnAttributeEdmPropertyConvention into the conventions in the convention model builder:

![]({{site.baseurl}}/img/12-01-conventions.png)

For example, the developer can do as follows to build the Edm model:
{% highlight csharp %}
public class Customer
{
  public int Id { get; set; }
      
  [Column(TypeName=”date”)]
  public DateTime Birthday { get; set; }

  [Column(TypeName=”date”)]
  public DateTime? PublishDay { get; set; }

  [Column(TypeName=”time”)]
  public TimeSpan CreatedTime { get; set; }
 }
{% endhighlight %}

Now, the developer can call as follows to build the Edm model:
{% highlight csharp %}
ODataConventionModelBuilder builder = new ODataConventionModelBuilder();

builder.EntityType<Customer>();

IEdmModel model = builder.GetEdmModel();
{% endhighlight %}

#### Serialize 

##### `System.DateTime` value to Edm.Date

We should modify `ODataEntityTypeSerializer` and `ODataComplexTypeSerializer` to identify whether or not the `System.DataTime` is serialized to __Edm.Date__. So, we should add a function in `ODataPrimitiveSerializer`:
{% highlight csharp %}
internal static object ConvertUnsupportedPrimitives(object value, IEdmPrimitiveTypeReference primitiveType)
{
    Type type = value.GetType();
    if (primitiveType.IsDate() && TypeHelper.IsDateTime(type))
    {
         Date dt = (DateTime)value;
         return dt;
    }
    … 
}
{% endhighlight %}

##### `System.TimeSpan` value to Edm.TimeOfDay

Add the following codes into the above function:

{% highlight csharp %}
if (primitiveType.IsTimeOfDay() && TypeHelper.IsTimeSpan(type))
{
   TimeOfDay tod = (TimeSpan)value;
   return tod;
}
{% endhighlight %}

##### Top level property

If the end user want to query the top level property, for example:
{% highlight csharp %}
   ~/Customers(1)/Birthday
{% endhighlight %} 
The developer must take responsibility to convert the value into its corresponding type.


#### De-serialize

##### Edm.Date to System.DateTime value

It’s easy to add the following code in `EdmPrimitiveHelpers` to convert `struct Date` to `System.DateTime`:

{% highlight csharp %}
if (value is Date)
{
    Date dt = (Date)value;
    return (DateTime)dt;
}
{% endhighlight %} 

##### Edm.TimeOfDay to System.TimeSpan value
Add codes in `EdmPrimitiveHelpers` to convert `struct TimeOfDay` to `System.TimeSpan`:

{% highlight csharp %}
else if(type == typeof(TimeSpan))
{
   if (value is TimeOfDay)
   {
       TimeOfDay tod = (TimeOfDay)value;
       return (TimeSpan)tod;
   }
}

{% endhighlight %} 

#### Query options on Date & Time

We should to support the following scenarios:

{% highlight csharp %}
• ~/Customers?$filter=Birthday eq 2015-12-14
• ~/Customers?$filter=year(Birthday) ne 2015
• ~/Customers?$filter=Publishday eq null
• ~/Customers?$orderby=Birthday desc
• ~/Customers?$select=Birthday
• ~/Customers?$filter=CreatedTime eq 04:03:05.0790000"
...
{% endhighlight %} 
Fortunately, Web API supports the most scenarios already, however, we should make some codes changes in `FilterBinder` class to make TimeOfDay scenario to work. 

#### Example

We re-use the Customer model in the Scope. We use the Lambda expression to build the Edm Model as:

{% highlight csharp %}
public IEdmModel GetEdmModel()
{
  ODataModelBuilder builder = new ODataModelBuilder();
  var customer = builder.EntitySet<Customer>(“Customers”).EntityType;
  customer.HasKey(c => c.Id);
  customer.Property(c => c.Birthday).AsDate();
  customer.Property(c => c.PublishDay).AsDate();
  return builder.GetEdmModel();
}
{% endhighlight %} 

Here’s the metadata document:
{% highlight xml %}
<?xml version="1.0" encoding="utf-8"?>
<edmx:Edmx Version="4.0" xmlns:edmx="http://docs.oasis-open.org/odata/ns/edmx">
  <edmx:DataServices>
    <Schema Namespace="NS" xmlns="http://docs.oasis-open.org/odata/ns/edm">
      <EntityType Name="Customer">
        <Key>
          <PropertyRef Name="Id" />
        </Key>
        <Property Name="Id" Type="Edm.Int32" Nullable="false" />
        <Property Name="Birthday" Type="Edm.Date" Nullable="false" />
        <Property Name="PublishDay" Type="Edm.Date" />
      </EntityType>
    </Schema>
    <Schema Namespace="Default" xmlns="http://docs.oasis-open.org/odata/ns/edm">
      <EntityContainer Name="Container">
        <EntitySet Name="Customers" EntityType="NS.Customer " />
      </EntityContainer>
    </Schema>
  </edmx:DataServices>
</edmx:Edmx>
{% endhighlight %} 

We can query:

__GET ~/Customers__

{% highlight JSON %}
{
  "@odata.context": "http://localhost/odata/$metadata#Customers",
  "value": [
    {
      "Id": 1,
      "Birthday": "2015-12-31",
      "PublishDay": null
    },
    …
  ]
}

{% endhighlight %} 

We can do filter:

__~/Customers?$filter=Birthday eq 2017-12-31__
{% highlight JSON %}
{
  "@odata.context": "http://localhost/odata/$metadata#Customers",
  "value": [
    {
      "Id": 2,
      "Birthday": "2017-12-31",
      "PublishDay": null
    }
  ]
}
{% endhighlight %} 

Thanks.
