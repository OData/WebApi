## OData Web API

### Introduction
OData Web API (i.e., ASP.NET Web API OData) is a server library built upon [ODataLib](https://github.com/OData/odata.net/) and [Web API](http://www.asp.net/web-api).

### Project structure
The master branch has the following libraries, and the packages are available from NuGet or MyGet:
 - [OData Web API v4](https://www.nuget.org/packages/Microsoft.AspNet.OData/) (namespace `System.Web.OData`) 
 - [OData Web API v3](https://www.nuget.org/packages/Microsoft.AspNet.WebApi.OData/) (namespace `System.Web.Http.OData`)
 - [vNext](http://odata.github.io/WebApi/#07-07-6-0-0-alpha1) (namespace [`Microsoft.AspNet.OData`](https://github.com/OData/WebApi/tree/master/vNext))

### Building
```
cd OData
build
```
### Testing
Each solution contains some test projects. Test projects use xUnit runner nuget package. Open the project, build it, and then test cases should appear in test explorer. Run all the tests in the test explorer. For running end-to-end tests you must open the solution as *Administrator*. More detail at [this](http://odata.github.io/WebApi/#09-01-unittest-e2etest).

### Nightly builds
1.	In your NuGet Package Manager settings add the following package source:
  * https://www.myget.org/F/aspnetwebstacknightly/
2.	Package IDs
  * Choose: Include Prerelease
  * OData v4: [Microsoft.AspNet.OData](https://www.myget.org/F/aspnetwebstacknightly/Packages?$filter=Id%20eq%20%27Microsoft.AspNet.OData%27&$select=Id,Version&$orderby=Version%20desc&$top=4&$format=application/json)
  * OData v3: [Microsoft.AspNet.WebApi.OData](https://www.myget.org/F/aspnetwebstacknightly/Packages?$filter=Id%20eq%20%27Microsoft.AspNet.WebApi.OData%27&$select=Id,Version&$orderby=Version%20desc&$top=4&$format=application/json)

### Contribution
Please refer to the [CONTRIBUTION.md](https://github.com/OData/WebApi/blob/master/CONTRIBUTION.md).

### Samples
Please refer to the [ODataSamples WebApi](https://github.com/OData/ODataSamples/tree/master/WebApi).

### Debug
Please refer to the [How to debug](http://odata.github.io/WebApi/10-01-debug-webapi-source).
