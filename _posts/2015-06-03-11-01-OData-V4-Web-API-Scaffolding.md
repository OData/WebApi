---
layout: post
title: "11.1 OData V4 Web API Scaffolding"
description: "How to Use OData V4 Web API Scaffolding"
category: "11. Tools"
---
### Install Visual Studio Extension
The installer of OData V4 Web API Scaffolding can be downloaded from Visual Studio Gallery: [Microsoft Web API OData V4 Scaffolding](https://visualstudiogallery.msdn.microsoft.com/db6b8857-06cc-4f40-95dd-a379f0494f45). Double click vsix to install, the extension supports the VS2013 and VS2015, now.

### Generate Controller Code With Scaffolding
The scaffolding is used to generate controller code for model class. Before using scaffolding, you need to create a web api project and add model classes, the following is a sample:

![](https://github.com/LianwMS/WebApiF/blob/gh-pages/img/11-01-ProjAndClass.PNG)

Then, you can right click "Controller" folder in solution explorer, select "Add" -> "Controller". "Microsoft OData v4 Web API Controller" will be in the scaffolder list, as following:

![](https://github.com/LianwMS/WebApiF/blob/gh-pages/img/11-01-SelectController.PNG)

Select scaffoler item, then choose a model class you want to generate the controller. You can also select the "Using Async" if your data need to be got in Async call.

![](https://github.com/LianwMS/WebApiF/blob/gh-pages/img/11-01-SelectModelClass.PNG)

After click "Add", the controller will be genereted and added into your project. Meanwhile, all reference needed, including OData Lib and OData Web API, will be added into the project, too.

![](https://github.com/LianwMS/WebApiF/blob/gh-pages/img/11-01-Complete.PNG)

### Change WebApiConfig.cs File
After generating the controller code, you may need to add some code in WebApiConfig.cs to generate model. Actually the code needed are in the comment of generated controller:

![](https://github.com/LianwMS/WebApiF/blob/gh-pages/img/11-01-ChangeWebApiConfig.PNG)

Just need to copy/paste the code to WebApiConfig.cs.

### Add the Code to retrieve Data
As Scaffolding only genreate the frameowrk of controller code, data retrieve part should also be added into controller generated. Here, we write a simple in-memory data source and return all of them when call "GetProducts" method:
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
