---
layout: post
title: "11.1 OData V4 Web API Scaffolding"
description: "How to Use OData V4 Web API Scaffolding"
category: "11. Tools"
---
### Install Visual Studio Extension
The installer of OData V4 Web API Scaffolding can be downloaded from Visual Studio Gallery: [Microsoft Web API OData V4 Scaffolding](https://visualstudiogallery.msdn.microsoft.com/db6b8857-06cc-4f40-95dd-a379f0494f45). Double click to install, the extension now support the VS2013 and VS2015.

### Generate Controller Code With Scaffolding
The scaffolding is used to generated the controller code for existing model class. Before using scaffolding, you need to create a web api project and add a model class, as following:

Then, you can right click "Controller" folder, select "Add" -> "Controller". "Microsoft OData v4 Web API Controller" will be in the scaffolder list, as following:

Select scaffoler item, please choose a model class you want to generate the controller. You can also select the "Using Async" if your data need to be got in Async call.

After collect "Add", the controller will be genereted and added into your project. Meanwile, all reference needed, including OData Lib and OData Web API will be added into the project, too.

### Change WebApiConfig.cs File
After genreating the controller code, you may need to add some code in WebApiConfig.cs to generate model. Actually the code needed are in the comment of genreated controller:

Just need to copy/paste the code to WebApiConfig.cs.

### Add the Code to retrieve Data
As Scaffolding only genreate the frameowrk of controller code, data retrieve code should also be added into controller genreated. Here, we write a simple in memory data source and return all of them when call "GetProducts" method:
#### Add in ProductsController:
```
private static List<Product> products = new List<Product>()
{
  new Product() {Id = 1, Name = "Test1"},
};
```
#### Add in GetProducts Method:
```
return Ok(products);
```
