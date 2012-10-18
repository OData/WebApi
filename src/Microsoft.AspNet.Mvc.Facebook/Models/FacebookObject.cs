// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNet.Mvc.Facebook.Attributes;

namespace Microsoft.AspNet.Mvc.Facebook.Models
{
    public class FacebookObject
    {
        [FacebookField(FieldName = "id", JsonField = "id")]
        public string FacebookId { get; set; }

        //TODO: (ErikPo) Should be able to auto-ignore this one instead
        [FacebookField(Ignore = true)]
        public string FacebookUserId { get; set; }

        [NotMapped]
        [FacebookField(Ignore = true)]
        public dynamic Data { get; set; }
    }
}
