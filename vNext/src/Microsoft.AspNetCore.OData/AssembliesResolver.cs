using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.AspNetCore.OData
{
	public class AssembliesResolver
	{
		public List<Assembly> Assemblies { get; set; }

		public AssembliesResolver(params Assembly[] names) : this(names.AsEnumerable())
		{
		}

		public AssembliesResolver(IEnumerable<Assembly> names)
		{
			if (names == null) throw new ArgumentNullException(nameof(names));
			Assemblies = new List<Assembly>();
			Assemblies.AddRange(names);
		}
	}
}