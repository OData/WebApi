---
layout: post
title: "12.2 Use $skiptoken for server side paging "
description: "WebAPI to use $skiptoken for server side paging"
category: "12. Design"
---
# Use $skiptoken for server-driven paging

### Background
Loading large data can be slow. Services often rely on pagination to load the data incrementally to improve the response times and the user experience. Paging can be server-driven or client-driven:
#### Client-driven paging
In client-driven, the client decides how many records it wants to load and asks the server that many records. That is achieved by using $skip and $top tokens in conjunction.
#### Server-driven paging
In server-driven paging, the client asks for a collection of entities and the server sends back partial results as well as a nextlink to use to retrieve more results. The nextlink is an opaque link which may use $skiptoken to identify the last loaded record.
### Problem
Currently, WebAPI uses $skip for server-driven paging which is a slight deviation from the OData standard and can be problematic when the data source can get updated concurrently. For instance, a deletion of a record may cause the last record to be sent down to the client twice. 
### Proposed Solution
WebAPI will now implement $skiptoken. When a collection of entity is requested which requires paging, we will assign the key value of the last sent entity to $skiptoken in the nextlink url. While processing a request with $skiptoken, we will add another condition (the key of the entity to be greater than the value specified to the skiptoken) to the predicate. 

### Technical details
After all the query options have been applied, we determine if the results need pagination. If the results need pagination, we will pass the key value of the last result to the method that generates the nextpage link.   

#### Format of the nextlink
The nextlink may contain $skiptoken if the result needs to be paginated. In WebAPI the $skiptoken value will be the key property and key value separated by a delimiter(:). For entities with composite or multi-part keys, each key property and value pair will be comma separated.
```
~/Products?$skiptoken=Id:'27'
~/Books?$skiptoken=ISBN:'978-2-121-87758-1"',CopyNumber:'11'
~/Products?$skiptoken=Id:'25'&$top=40
```
We will not use $skiptoken if the requested resource is not an entity type. Rather, normal skip will be used. 

#### Generating the nextlink
The next link generation method in ___GetNextPageHelper___ static class will take in the $skiptoken value along with other query parameters and generate the link by doing special handling for $skip, $skiptoken and $top. It will pass on the other query options as they were in the original request.
##### 1. Handling $skip
We will omit the $skip value if the service is configured to support $skiptoken and a collection of entity is being requested. This is because the first response would have applied the $skip query option to the results already. 
##### 2. Handle $top
We will reduce the value of $top query option by the page size if it is greater than the page size.   
##### 3. Handle $skiptoken
The value for the $skiptoken will be updated to new value passed in which is the key value for the last record sent. If the skiptoken value is not sent, we will revert to the previous logic and use $skip for paging instead.

#### Parsing $skiptoken and generating the Linq expression
New classes will be created for ___SkipTokenQueryOption___ and __SkipTokenQueryValidator__. ___SkipTokenQuery___ option will contain the overridden methods to create LINQ expressions based on the $skiptoken value.
The ___SkipTokenQuery___ class will be utilized by ___ODataQueryOption___ to pass the token value to the nextlink generator helper methods.
In the process, ___IWebApiRequestMessage___ will be modified and GetNextPageLink method will now accept an optional parameter for the $skiptoken value.

#### Configuration to use $skiptoken or $skip for server-driven paging
We will allow services to configure if they want to use $skiptoken or $skip for paging.

### Additional obscured details and ongoing investigation
Consistently exposing the configuration for both Classic and Core.

Delimiter in the value of a key property. 

ODataFeedSerializer  

Stable sort. 





