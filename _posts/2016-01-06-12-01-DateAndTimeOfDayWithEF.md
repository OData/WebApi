---
layout: post
title: "12.1 Edm.Date and Edm.TimeOfDay with EF"
description: "How to Use Edm.Date and Edm.TimeOfDay with EntityFramework"
category: "12. Design"
---
### Problem
The Transact-SQL has <strong>date (Format: YYYY-MM-DD)</strong> type, but there isn’t a CLR type representing date type. Therefore, Entity Framework (EF) only supports to use <strong>System.DateTime</strong> CLR type to map the date type. 
OData V4 lib provides a CLR `struct Date` type and the corresponding primitive type kind <strong>Edm.Date</strong>. Web API OData V4 supports this type. However, EF doesn’t recognize this CLR type, that is why it can’t map `struct Date` directly to <strong>date</strong> type.
So, this doc describes the solution about how to support Edm.Date type with Entity Framework. Meantime, this doc also covers the <strong>Edm.TimeOfDay</strong> type with EF.

### Scopes

We should support to map the type between <strong>date</strong> type in Database and <strong>Edm.Date</strong> type through the CLR <strong>System.DateTime</strong> type. The map is shown in the following figure:

![]({{site.baseurl}}/img/12-01-DateTypeMapping.PNG)

So, we should provide the below functionalities for the developer:

1.	Can configure the System.DateTime property to Edm.Date
2.	Can serialize the date value in the DB as Edm.Date value format.
3.	Can de-serialize the Edm.Date value as date value into DB.
4.	Can do query option on the date value.

Most important, EF doesn’t support the collection. So, Collection of date is not in the scope. The developer can use navigation property to work around.

### Detail Design
#### Date & Time type in SQL DB
Below is the date & time type mapping between DB and .NET. 

|MySQL data types |	SSDL|	CSDL|	.NET|
|:-----------------|:------|:------|:---------|
|date, datetime, datetime2|	date, datetime, datetime2|	DateTime	|__System.DateTime__|
|time|	time|	Time|	__System.TimeSpan__|

So, From .NET view, only __System.DateTime__ is used to represent the _date_ value, meanwhile only __System.TimeSpan__ is used to represent the __time__ value.

#### Date & time mapping with EF 

In EF Code First, the developer can use two methodologies to map __System.DateTime__ property to __date__ column in DB:

1 Data Annotation

The users can use the <strong>_Column_</strong> Data Annotation to modify the data type of columns. For example:

The scaffolding is used to generate controller code for model class. Two kinds of scaffolders are provided: for model without entity framework(Microsoft OData v4 Web API Controller) and model using entity framework(Microsoft OData v4 Web API Controller Using Entity Framework). 
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
            .HasColumnType(“date”);
{% endhighlight %}

For __time type__, it implicitly maps the __System.TimeSpan__ to represent the __time__ value. However, you can use “time” string literal in DataAnnotation or fluent API explicitly.

#### CLR Date Type in ODL

OData Library defines one _struct_ to hold the value of __Edm.Date (Format: YYYY-MM-DD)__.

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

While, __Edm.Date__ is the corresponding primitive type Kind.

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

|Property CLR Type |	Property Edm Type|
|:-----------------|:------|
|System.DateTime|	Edm.DateTimeOffset|	
|System.TimeSpan|	Edm.Duration|

We should provide a methodology to map __System.DateTime__ to __Edm.Date__ type, and __System.TimeSpan__ to __Edm.TimeOfDay__ type as follows:

Select scaffoler item, then choose a model class you want to generate the controller. You can also select the "Using Async" if your data need to be got in Async call.

|Property CLR Type |	Property Edm Type| Methodology|
|:-----------------|:------|:----:|
|System.DateTime|	Edm.DateTimeOffset|	by default|
||	Edm.Date|	New |
|System.TimeSpan|	Edm.Duration|	by default|
||	Edm.TimeOfDay|New|

##### Extension methods
We will add the following extension methods to re-configure __System.DateTime__ & __System.TimeSpan__ property:

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

We should recognize the Column Data annotation. So, we will add a convention class as follows:
{% highlight csharp %}
internal class ColumnAttributeEdmPropertyConvention : AttributeEdmPropertyConvention<PropertyConfiguration>
{
  …
}
{% endhighlight %}

In this class, it will identify the __Column__ attribute applied to __System.DateTime__ or __System.TimeSpan__ property, and call __AsDate(…)__ or __AsTimeOfDay()__ extension methods to add a _Date_ or _TimeOfDay_ mapped property. Be caution, EF supports the TypeName case-insensitive.
After insert the instance of ColumnAttributeEdmPropertyConvention into the conventions in the convention model builder:

![]({{site.baseurl}}/img/12-01-conventions.PNG)

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



