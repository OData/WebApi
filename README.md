## OData Web API

 Build  | Status
--------|---------
Odata.WebApi Rolling Dotnet pipeline | <img src="https://dev.azure.com/dotnet/OData/_apis/build/status/OData.WebApi-Rolling?branchName=master"/> 
WebApi Rolling | <img src="https://identitydivision.visualstudio.com/OData/_apis/build/status/WebApi/WebApi-master-pipeline-Rolling"/> 
WebApi Nightly | <img src="https://dev.azure.com/dotnet/OData/_apis/build/status/OData.WebApi%20Nightly?branchName=master"/> 

### Introduction

[OData Web API](https://docs.microsoft.com/en-us/odata/webapi/getting-started) (i.e., ASP.NET Web API OData) is a server library built upon [ODataLib](https://github.com/OData/odata.net/) and [Web API](http://www.asp.net/web-api).

### Project structure

The project currently has the following branches:

**[master](https://github.com/OData/Webapi/tree/master) branch**

This is the active development branch for OData WebApi and it is currently most actively iterated. The package name is Microsoft.AspNet.OData. The is the OData WebApi for ODL v7.x releases which contain breaking changes against ODL v6.

**[release](https://github.com/OData/Webapi/tree/release) branch**

This is the release branch for OData WebApi, contains code base up to most recently stable WebApi release. The latest release version is [6.0](https://www.nuget.org/packages/Microsoft.AspNet.OData/6.0.0).

**[feature/netcore](https://github.com/OData/Webapi/tree/feature/netcore) branch**

This is the feature development branch for OData WebApi for AspNet and AspNetCore. The package names are Microsoft.AspNet.OData and Microsoft.AspNetCore.OData. The is the OData WebApi 7.0 release which contain breaking changes against OData WebApi 6.0.

**[gh-pages](https://github.com/OData/WebApi/tree/gh-pages) branch**

The gh-pages branch contains the old documentation source for OData WebApi - tutorials, guides, etc. For the most up-to-date documentation you should use [Microsoft docs](https://docs.microsoft.com/en-us/odata).

**[maintenance-aspnetcore](https://github.com/OData/Webapi/tree/maintenance-aspnetcore) branch**

This is the maintenance branch for OData WebApi with ASP.NET Core support. The package name is Microsoft.AspNetCore.OData.

**[maintenance-V4](https://github.com/OData/Webapi/tree/maintenance-V4) branch**

This is the maintenance branch for OData WebApi based on ODL 6.x, which implements the ODataV4 protocol. The package name is Microsoft.AspNet.OData, with latest maintenance release version [5.10](https://www.nuget.org/packages/Microsoft.AspNet.OData/5.10.0).

**[maintenance-V3](https://github.com/OData/Webapi/tree/maintenance-V3) branch**

This is the maintenance branch for OData WebApi based on ODL 5.x, which implements the ODataV3 protocol. The package name is Microsoft.AspNet.WebApi.OData, with latest maintenance release version [5.7](https://www.nuget.org/packages/Microsoft.AspNet.WebApi.OData/5.7.0).

**[maintenance-dnx](https://github.com/OData/Webapi/tree/maintenance-dnx) branch**

This is maintenance branch for an early prototype version of OData WebApi based on original ASP.NET Core, aka DNX. Package name is Microsoft.AspNet.OData. This is for project archive purpose only, is not active and doesn't accept contributions. It has only one release.

**[odata-v5.3-rtm](https://github.com/OData/WebApi/tree/odata-v5.3-rtm) [v2.0-rtm](https://github.com/OData/WebApi/tree/v2.0-rtm) [v3-rtm](https://github.com/OData/WebApi/tree/odata-v3-rtm) [v3.1-rtm](https://github.com/OData/WebApi/tree/v3.1-rtm) [v3.2-rtm](https://github.com/OData/WebApi/tree/v3.2-rtm) branches**

These are maintenance branches for previous RTMs. Project archives only, contributions not accepted.

### Building

```sh
build.cmd
```

### Testing

Each solution contains some test projects. Test projects use xUnit runner nuget package.

Tests will not run correctly unless SkipStrongNames is Enabled. Please run

```sh
build.cmd EnableSkipStrongNames
```

#### Run tests in cmd

To run end-to-end tests, you need to open an **elevated** - Run as administrator - command prompt

* `build.cmd` build projects, run unit tests, and OData end-to-end tests.

* `build.cmd quick` build project, and run unit tests

To disable the SkipStrongNames:

```sh
build.cmd DisableSkipStrongNames
```

#### Run tests in Visual Studio

Open the project, build it, and then test cases should appear in test explorer. If not, this is because the assemblies are delay signed and you're missing the private key so xunit will not load them in Visual Studio. To fix, please run `build.cmd EnableSkipStrongNames`. Run all the tests in the test explorer. For running end-to-end tests you must open the solution as *Administrator*. More detail at [this](https://docs.microsoft.com/en-us/odata/webapi/unittest-e2etest).

### Nightly builds

The nightly build process will upload a NuGet packages for WebApi to:
 v7.x.x: [MyGet.org webapinetcore feed](https://www.myget.org/gallery/webapinetcore)
 v6.x.x: [MyGet.org webapinightly feed](https://www.myget.org/gallery/webapinightly)

To connect to webapinightly feed, use this feed URL:
 v7.x.x: [webapinetcore MyGet feed URL](https://www.myget.org/F/webapinetcore)
 v6.x.x: [webapinightly MyGet feed URL](https://www.myget.org/F/webapinightly)

You can query the latest nightly NuGet packages using this query:
 v7.x.x: [MAGIC WebApi query](https://www.myget.org/F/webapinetcore/Packages?$select=Id,Version&$orderby=Version%20desc&$top=4&$format=application/json)
 v6.x.x: [MAGIC WebApi query](https://www.myget.org/F/webapinightly/Packages?$select=Id,Version&$orderby=Version%20desc&$top=4&$format=application/json)

### Contribution

Please refer to the [CONTRIBUTION.md](https://github.com/OData/WebApi/blob/master/.github/CONTRIBUTION.md).

### Documentation

Please visit the [OData Web API pages](https://docs.microsoft.com/en-us/odata/webapi/getting-started).

### Samples

Please refer to the [ODataSamples Repro](https://github.com/OData/ODataSamples).

* ASP.NET Core OData samples at [here](https://github.com/OData/ODataSamples/tree/master/WebApiCore)
* ASP.NET Classic OData samples at [here](https://github.com/OData/ODataSamples/tree/master/WebApiClassic)

### Debug

Please refer to the [How to debug](https://docs.microsoft.com/en-us/odata/webapi/debugging).

### Code of Conduct

This project has adopted the [.NET Foundation Contributor Covenant Code of Conduct](https://dotnetfoundation.org/about/code-of-conduct). For more information see the [Code of Conduct FAQ](https://dotnetfoundation.org/about/faq).

### .NET Foundation

This project is supported by the [.NET Foundation](https://dotnetfoundation.org).

WebApi is a Copyright of &copy; .NET Foundation and other contributors. It is licensed under [MIT License](https://github.com/OData/WebApi/blob/master/License.txt)
