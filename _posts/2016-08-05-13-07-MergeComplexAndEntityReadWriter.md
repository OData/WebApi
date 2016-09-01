---
layout: post
title : "13.7 Merge complex & entity value serialize/deserialize"
description: "Merge complex & entity"
category: "13. 6.x Features "
---

You can refer the detail in: [complex type and entity type](http://odata.github.io/odata.net/v7/#10-42-merge-complex-and-entity) in ODL 7.0 doc.

In Web API OData 6.x, we do the following to support the merge of complex type and entity type:

* Rename **ODataFeed** to **ODataResourceSet**
* Rename **ODataEntry** to **ODataResource**  
* Rename **ODataNavigationLink**  to  **ODataNestedResourceInfo**  
* Rename **ODataPayloadKind.Entry**  to  **ODataPayloadKind.Resource**
* Rename **ODataPayloadKind.Feed**  to  **ODataPayloadKind.ResourceSet**
* Rename **ODataEntityTypeSerializer**  to  **ODataResourceSerializer** 
* Rename **ODataFeedSerializer**  to  **ODataResourceSetSerizlier** 
* Rename **ODataEntityDeserializer**  to  **ODataResourceDeserializer** 
* Rename **ODataFeedDeserializer**  to  **ODataResourceSetDeserializer**
* Remove **ODataComplexValue**  
* Remove **ODataComplexSerializer/ODataComplexTypeDeserializer** 

So, for any complex value, it will use **ODataResourceSerializer** and **ODataResourceDeserializer** to read and write.
for any complex collection value, it will use **ODataResourceSetSerializer** and **ODataResourceSetDeserializer** to read and write.

