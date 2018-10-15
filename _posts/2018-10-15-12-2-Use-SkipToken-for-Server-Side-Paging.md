---
layout: post
title: "12.2 Use $skiptoken for server side paging "
description: "WebAPI to use $skiptoken for server side paging"
category: "12. Design"
---
### Background
Loading large data can be slow and Paging can be server-driven or client-driven.
#### Client-driven paging
In client-driven, the client decides how many records it wants to load and asks the server that many records. That is achieved by using $skip and $top tokens in conjuction.
#### Server-driven paging
In server-driven paging, the client asks for a collection of entities, and the server sends back partial results as well as a nextlink to use to retrieve more results. The nextlink is an opaque link which may use $skiptoken to identify the last loaded record.
### Problem
Currently, WebAPI uses $skip for server-driven paging which is a slight deviation from the OData standard and can be problematic when the data source can get updated concurrently. For instance, a deletion of a record may cause the last record to be send down to the client twice. 
### Proposed Solution
WebAPI will now implement $skiptoken. When a collection of entity is requested which requires paging, we will assign the key value of the last sent entity to $skiptoken in the nextlink url. While processing a request with $skiptoken, we will add another condition (the key of the entity to be greater than the value specied to the skiptoken) to the predicate. 
### Techinical details
