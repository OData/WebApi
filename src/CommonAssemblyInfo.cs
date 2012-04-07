// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;

[assembly: AssemblyCompany("Microsoft Corporation")]
[assembly: AssemblyCopyright("© Microsoft Corporation. All rights reserved.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyTrademark("")]
[assembly: ComVisible(false)]
[assembly: CLSCompliant(true)]

#if ASPNETMVC && ASPNETWEBPAGES
#error Runtime projects cannot define both ASPNETMVC and ASPNETWEBPAGES
#endif

#if ASPNETMVC
[assembly: AssemblyVersion("4.0.0.0")]
[assembly: AssemblyFileVersion("4.0.0.0")]
[assembly: AssemblyProduct("Microsoft ASP.NET MVC")]
#elif ASPNETWEBPAGES
[assembly: AssemblyVersion("2.0.0.0")]
[assembly: AssemblyFileVersion("2.0.0.0")]
[assembly: AssemblyProduct("Microsoft ASP.NET Web Pages")]
#else
#error Runtime projects must define either ASPNETMVC or ASPNETWEBPAGES
#endif

[assembly: NeutralResourcesLanguage("en-US")]
