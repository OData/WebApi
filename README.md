## OData Web API

### Introduction
OData Web API (i.e., ASP.NET Web API OData) is a server library built upon [ODataLib](https://github.com/OData/odata.net/) and [Web API](http://www.asp.net/web-api).

### Project structure
The project currently has the following branches:

**[master](https://github.com/OData/Webapi/tree/master) branch**

This is the active development branch for OData WebApi and it is currently most actively iterated. The package name is Microsoft.AspNet.OData. The is the OData WebApi for ODL v7.x releases which contain breaking changes against ODL v6.

**[release](https://github.com/OData/Webapi/tree/release) branch**

This is the release branch for OData WebApi, contains code base up to most recently stable WebApi release. The latest release version is [6.0](https://www.nuget.org/packages/Microsoft.AspNet.OData/6.0.0).

**[gh-pages](https://github.com/OData/WebApi/tree/gh-pages) branch**

The gh-pages branch contains documentation source for OData WebApi - tutorials, guides, etc.  The documention source is in Markdown format. It is hosted at [ODataLib Pages](http://odata.github.io/WebApi/ "ODataLib Pages").

**[maintenance-aspnetcore](https://github.com/OData/Webapi/tree/maintenance-aspnetcore)**

This is the maintenance branch for OData WebApi with ASP.NET Core support. The package name is Microsoft.AspNetCore.OData.

**[maintenance-V4](https://github.com/OData/Webapi/tree/maintenance-V4) branch**

This is the maintenance branch for OData WebApi based on ODL 6.x, which implements the ODataV4 protocol. The package name is Microsoft.AspNet.OData, with latest maintenance release version [5.10](https://www.nuget.org/packages/Microsoft.AspNet.OData/5.10.0).

**[maintenance-V3](https://github.com/OData/Webapi/tree/maintenance-V3) branch** 

This is the maintenance branch for OData WebApi based on ODL 5.x, which implements the ODataV3 protocol. The package name is Microsoft.AspNet.WebApi.OData, with latest maintenance release version [5.7](https://www.nuget.org/packages/Microsoft.AspNet.WebApi.OData/5.7.0).

**[maintenance-dnx](https://github.com/OData/Webapi/tree/maintenance-dnx) branch**

This is maintenance branch for an early prototype version of OData WebApi based on original ASP.NET Core, aka DNX. Package name is Microsoft.AspNet.OData. This is for project archive purpose only, is not active and doesn't accept contributions. It has only one release with information available [here](http://odata.github.io/WebApi/#07-07-6-0-0-alpha1).

**[odata-v5.3-rtm](https://github.com/OData/WebApi/tree/odata-v5.3-rtm) [v2.0-rtm](https://github.com/OData/WebApi/tree/v2.0-rtm) [v3-rtm](https://github.com/OData/WebApi/tree/odata-v3-rtm) [v3.1-rtm](https://github.com/OData/WebApi/tree/v3.1-rtm) [v3.2-rtm](https://github.com/OData/WebApi/tree/v3.2-rtm) branches**

These are maintenance branches for previous RTMs. Project archives only, contributions not accepted.
 
### Building
```
cd OData
build
```

### Contribution
Please refer to the [CONTRIBUTION.md](https://github.com/OData/WebApi/blob/master/CONTRIBUTION.md).

### Samples
Please refer to the [ODataSamples WebApi](https://github.com/OData/ODataSamples/tree/master/WebApi).

### Debug
Please refer to the [How to debug](http://odata.github.io/WebApi/10-01-debug-webapi-source).
