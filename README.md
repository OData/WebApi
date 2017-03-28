## OData Web API - test

### Introduction
[OData Web API](http://odata.github.io/WebApi) (i.e., ASP.NET Web API OData) is a server library built upon [ODataLib](https://github.com/OData/odata.net/) and [Web API](http://www.asp.net/web-api).

### Project structure
The project has a few branches, such as master, vNext, gh-pages.

#### master
The master branch has the following libraries, and the packages are available from NuGet or MyGet:
 - [OData v4 Web API](https://www.nuget.org/packages/Microsoft.AspNet.OData/) (namespace `System.Web.OData`) 
 - [OData v3 Web API](https://www.nuget.org/packages/Microsoft.AspNet.WebApi.OData/) (namespace `System.Web.Http.OData`)
 - [vNext](http://odata.github.io/WebApi/#07-07-6-0-0-alpha1) (namespace [`Microsoft.AspNet.OData`](https://github.com/OData/WebApi/tree/master/vNext))

#### vNext 
The [vNext](https://github.com/OData/WebApi/tree/vNext/vNext) branch contains the latest code of OData vNext Web API.

#### gh-pages
The [gh-pages](https://github.com/OData/WebApi/tree/gh-pages) branch contains the documenation source - in Markdown format - of the OData Web API.

### Building
```
cd OData
build.cmd
```

### Testing
Each solution contains some test projects. Test projects use xUnit runner nuget package.

Tests will not run correctly unless SkipStrongNames is Enabled. Please run
```
build.cmd EnableSkipStrongNames
```

#### Run tests in cmd
* `build.cmd` build project, and run unit tests.

To run end-to-end tests, you need to open an **elevated** - Run as administrator - command prompt
* `build.cmd e2eV4` build projects, run unit tests, and OData **v4** end-to-end tests.
* `build.cmd e2eV3` build projects, run unit tests, and OData **v3** end-to-end tests.
* `build.cmd full` build projects, run unit tests, OData **v4 and v3** end-to-end tests.

To disable the SkipStrongNames:
```
build.cmd DisableSkipStrongNames
```

#### Run tests in Visual Studio
Open the project, build it, and then test cases should appear in test explorer. If not, this is because the assemblies are delay signed and you're missing the private key so xunit will not load them in Visual Studio. To fix, please run `build.cmd EnableSkipStrongNames`. Run all the tests in the test explorer. For running end-to-end tests you must open the solution as *Administrator*. More detail at [this](http://odata.github.io/WebApi/#09-01-unittest-e2etest).

### Nightly builds
1.	In your NuGet Package Manager settings add the following package source:
  * https://www.myget.org/F/aspnetwebstacknightly/
2.	Package IDs
  * Choose: Include Prerelease
  * OData v4: [Microsoft.AspNet.OData](https://www.myget.org/F/aspnetwebstacknightly/Packages?$filter=Id%20eq%20%27Microsoft.AspNet.OData%27&$select=Id,Version&$orderby=Version%20desc&$top=4&$format=application/json)
  * OData v3: [Microsoft.AspNet.WebApi.OData](https://www.myget.org/F/aspnetwebstacknightly/Packages?$filter=Id%20eq%20%27Microsoft.AspNet.WebApi.OData%27&$select=Id,Version&$orderby=Version%20desc&$top=4&$format=application/json)

### Contribution
Please refer to the [CONTRIBUTION.md](https://github.com/OData/WebApi/blob/master/.github/CONTRIBUTION.md).

### Documentation
Please visit the [OData Web API pages](http://odata.github.io/WebApi).

### Samples
Please refer to the [ODataSamples WebApi](https://github.com/OData/ODataSamples/tree/master/WebApi).

### Debug
Please refer to the [How to debug](http://odata.github.io/WebApi/10-01-debug-webapi-source).
