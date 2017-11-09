using System;
using System.Linq;
using System.Collections.Generic;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebStack.QA.Test.OData.DateTimeOffsetSupport
{
    public class FilesContext : DbContext
    {
        public static string ConnectionString =
            @"Data Source=(LocalDb)\v11.0;;Integrated Security=True;Initial Catalog=DateTimeOffsetSupport";

        public FilesContext() : base(ConnectionString) { }

        public DbSet<File> Files { get; set; }
    }

    public class File
    {
        [Key]
        public int FileId { get; set; }

        public string Name { get; set; }

        public DateTimeOffset CreatedDate { get; set; }

        public DateTimeOffset? DeleteDate { get; set; }

        public override bool Equals(Object o)
        {
            var f = o as File;
            if (f == null)
            {
                return false;
            }

            var v1 = FileId.Equals(f.FileId);
            var v2 = Name.Equals(f.Name);
            var v3 = CreatedDate.Equals(f.CreatedDate);
            var v4 = Object.Equals(DeleteDate, f.DeleteDate);
            return v1 && v2 && v3 && v4;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
