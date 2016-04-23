using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNetCore.OData
{
	public class AssemblyNames
	{
		public List<string> Names { get; set; }

		public AssemblyNames(params string[] names) : this(names.AsEnumerable())
		{
		}

		public AssemblyNames(IEnumerable<string> names)
		{
			if (names == null) throw new ArgumentNullException(nameof(names));
			Names = new List<string>();
			Names.AddRange(names);
		}
	}
}