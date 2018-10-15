---
layout: post
title: "12.2 Use $skiptoken for server side paging "
description: "WebAPI to use $skiptoken for server side paging"
category: "12. Design"
---
### Background
Paging can be server-driven or client-driven. 
In client-driven, the client decides how many records it wants to load and asks the server that many records. That is achieved by using $skip and $top tokens in conjuction where as in server-driven paging, the client asks for a collection of entities, and the server sends back partial results as well as a nextlink to use to retrieve more results. The nextlink is an opaque linke which may use $skiptoken to identify the last loaded record.

Scope
### Problem

