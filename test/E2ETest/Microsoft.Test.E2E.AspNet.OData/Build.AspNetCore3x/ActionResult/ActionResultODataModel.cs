using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Builder;

namespace Microsoft.Test.E2E.AspNet.OData.ActionResult
{
    public class Customer
    {
        [Key]
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [Contained]
        [DataMember(Name = "books")]
        public IEnumerable<Book> Books { get; set; }
    }

    public class Book
    {
        [Key]
        [DataMember(Name = "id")]
        public string Id { get; set; }
    }
}
