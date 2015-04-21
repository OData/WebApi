---
title : "4.5 Abstract entity types"
layout: post
category: "4. OData features"
---

Since [Web API OData V5.5-beta](http://www.nuget.org/packages/Microsoft.AspNet.OData/5.5.0-beta), it is allowed to:

1. define abstract entity types without keys. 
2. define abstract type (entity & complex) without any properties.
3. define derived entity types with their own keys.

Let's see some examples:

#### Entity type example:

The CLR model is shown as below:
{% highlight csharp %}
public abstract class Animal
{
}

public class Dog : Animal
{
  public int DogId { get; set; }
}

public class Pig : Animal
{
  public int PigId { get; set; }
}
{% endhighlight %}

We can use the following codes to build Edm Model:
{% highlight csharp %}
  var builder = new ODataConventionModelBuilder();
  builder.EntityType<Animal>();
  builder.EntitySet<Dog>("Dogs");
  builder.EntitySet<Pig>("Pigs");
  IEdmModel model = builder.GetEdmModel()
{% endhighlight %}

Then, we can get the metadata document for *Animal* as:
{% highlight xml %}
<EntityType Name="Animal" Abstract="true" />
<EntityType Name="Dog" BaseType="NS.Animal">
    <Key>
        <PropertyRef Name="DogId" />
    </Key>
    <Property Name="DogId" Type="Edm.Int32" Nullable="false" />
</EntityType>
<EntityType Name="Pig" BaseType="NS.Animal">
    <Key>
        <PropertyRef Name="PigId" />
    </Key>
    <Property Name="PigId" Type="Edm.Int32" Nullable="false" />
</EntityType>
{% endhighlight %}

Note:
1. *Animal* is an abstract entity type without any keys and any properties
2. *Dog* & *Pig* are two sub entity types derived from *Animal* with own keys. 

However, it's obvious that abstract entity type without keys can't be used to define any navigation sources (entity set or singleton). 
So, if you try to:
{% highlight csharp %}
builder.EntitySet<Animal>("Animals");
{% endhighlight %}

you will get the following exception:
{% highlight csharp %}
System.InvalidOperationException: The entity set or singlet on 'Animals' is based on type 'NS.Animal' that has no keys defined.
{% endhighlight %}

#### Complex type example

Let's see a complex example. The CLR model is shown as below:

{% highlight csharp %}
public abstract class Graph
{ }

public class Point : Graph
{
  public int X { get; set; }
  public int Y { get; set; }
}

public class Line : Graph
{
  public IList<Point> Vertexes { get; set; }
}
{% endhighlight %}    
We can use the following codes to build Edm Model:
{% highlight csharp %}
  var builder = new ODataConventionModelBuilder();
  builder.ComplexType<Graph>();
  IEdmModel model = builder.GetEdmModel()
{% endhighlight %}

Then, we can get the metadata document for *Graph* as:
{% highlight xml %}
<ComplexType Name="Graph" Abstract="true" />
<ComplexType Name="Point" BaseType="NS.Graph">
    <Property Name="X" Type="Edm.Int32" Nullable="false" />
    <Property Name="Y" Type="Edm.Int32" Nullable="false" />
</ComplexType>
<ComplexType Name="Line" BaseType="NS.Graph">
    <Property Name="Vertexes" Type="Collection(NS.Point)" />
</ComplexType>
{% endhighlight %}

Where, *Graph* is an abstract complex type without any properties.      

Thanks.
