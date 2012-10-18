// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNet.Mvc.Facebook.Attributes;

namespace Microsoft.AspNet.Mvc.Facebook.Models
{
    [FacebookUser]
    public class FacebookUser
    {
        [FacebookField(FieldName = "id", JsonField = "id")]
        public virtual string FacebookId { get; set; }

        //TODO: (ErikPo) Once we switch over to .NET 4.5 the dependency on EF should be removed
        [NotMapped]
        [FacebookField(Ignore = true)]
        public virtual dynamic Data { get; set; }
    }
}
