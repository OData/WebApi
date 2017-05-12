// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.OData.Core;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNet.OData.Formatter.Serialization
{
    /// <summary>
    /// Represents an <see cref="Microsoft.OData.Core.ODataSerializer"/> for serializing $ref response for a collection navigation property.
    /// </summary>
    public class ODataEntityReferenceLinksSerializer : ODataSerializer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataEntityReferenceLinksSerializer"/> class.
        /// </summary>
        public ODataEntityReferenceLinksSerializer()
            : base(ODataPayloadKind.EntityReferenceLinks)
        {
        }

        /// <inheridoc />
        public override void WriteObject(object graph, Type type, ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
        {
            if (messageWriter == null)
            {
                throw Error.ArgumentNull("messageWriter");
            }

            if (writeContext == null)
            {
                throw Error.ArgumentNull("writeContext");
            }

            if (graph != null)
            {
                ODataEntityReferenceLinks entityReferenceLinks = graph as ODataEntityReferenceLinks;
                if (entityReferenceLinks == null)
                {
                    IEnumerable<Uri> uris = graph as IEnumerable<Uri>;
                    if (uris == null)
                    {
                        throw new SerializationException(Error.Format(SRResources.CannotWriteType, GetType().Name, graph.GetType().FullName));
                    }

                    entityReferenceLinks = new ODataEntityReferenceLinks
                    {
                        Links = uris.Select(uri => new ODataEntityReferenceLink { Url = uri })
                    };

                    if (writeContext.Request != null)
                    {
                        entityReferenceLinks.Count = writeContext.Request.ODataProperties().TotalCount;
                    }
                }

                messageWriter.WriteEntityReferenceLinks(entityReferenceLinks);
            }
        }
    }
}
