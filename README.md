## OData Web API
 Build  | Status
--------|---------
Rolling | <img src="https://identitydivision.visualstudio.com/_apis/public/build/definitions/2cfe7ec3-b94f-4ab9-85ab-2ebff928f3fd/108/badge"/>
Nightly | <img src="https://identitydivision.visualstudio.com/_apis/public/build/definitions/2cfe7ec3-b94f-4ab9-85ab-2ebff928f3fd/109/badge"/>

### Introduction
[OData Web API](http://odata.github.io/WebApi) (i.e., ASP.NET Web API OData) is a server library built upon [ODataLib](https://github.com/OData/odata.net/) and [Web API](http://www.asp.net/web-api).

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
build.cmd
```

### Testing
Each solution contains some test projects. Test projects use xUnit runner nuget package.

Tests will not run correctly unless SkipStrongNames is Enabled. Please run
```
build.cmd EnableSkipStrongNames
```

#### Run tests in cmd
To run end-to-end tests, you need to open an **elevated** - Run as administrator - command prompt

* `build.cmd` build projects, run unit tests, and OData end-to-end tests.

* `build.cmd quick` build project, and run unit tests

To disable the SkipStrongNames:
```
build.cmd DisableSkipStrongNames
```

#### Run tests in Visual Studio
Open the project, build it, and then test cases should appear in test explorer. If not, this is because the assemblies are delay signed and you're missing the private key so xunit will not load them in Visual Studio. To fix, please run `build.cmd EnableSkipStrongNames`. Run all the tests in the test explorer. For running end-to-end tests you must open the solution as *Administrator*. More detail at [this](http://odata.github.io/WebApi/#09-01-unittest-e2etest).

### Nightly builds
The nightly build process will upload a NuGet packages for WebApi to the [MyGet.org webapinightly feed](https://www.myget.org/gallery/webapinightly).

To connect to webapinightly feed, use this feed URL: [webapinightly MyGet feed URL](https://www.myget.org/F/webapinightly).

You can query the latest nightly NuGet packages using this query: [MAGIC WebApi query](https://www.myget.org/F/webapinightly/Packages?$select=Id,Version&$orderby=Version%20desc&$top=4&$format=application/json)

### Contribution
Please refer to the [CONTRIBUTION.md](https://github.com/OData/WebApi/blob/master/.github/CONTRIBUTION.md).

### Documentation
Please visit the [OData Web API pages](http://odata.github.io/WebApi).

### Samples
Please refer to the [ODataSamples WebApi](https://github.com/OData/ODataSamples/tree/master/WebApi).

### Debug
Please refer to the [How to debug](http://odata.github.io/WebApi/10-01-debug-webapi-source).
