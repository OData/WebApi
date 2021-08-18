//-----------------------------------------------------------------------------
// <copyright file="Person.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace AspNetCoreODataSample.Web.Models
{
    public class Person
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        //[NotMapped]
        public IDictionary<string, object> DynamicProperties { get; set; }

        public Level MyLevel { get; set; }
    }
}
