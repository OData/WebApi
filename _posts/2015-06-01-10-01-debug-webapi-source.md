---
layout: post
title: "10.1 How To Debug"
description: "how to debug odata webapi source"
category: "10. Others"
---

If you want to debug **OData Lib, WebAPI, Restier** source, open `DEBUG` -> `Options and Settings` in VS, configure below things in `General` tab:

1. Uncheck `Enable Just My Code (Managed only)`.
2. Uncheck `Enable .NET Framework source stepping`.
3. One can find the source code for particular releases at https://github.com/OData/WebApi/tags. You can use these source files to properly step through your debugging session.
4. Mark sure `Enable Source Link support` is checked.

Setup your symbol source in `Symbols` tab:

1. Check `Microsoft Symbol Servers`.
    * For versions of OData below 6.x, use the following
        * Add location: http://srv.symbolsource.org/pdb/Public (For preview/public releases in nuget.org).
        * Add location: http://srv.symbolsource.org/pdb/MyGet (For nightly build, and preview releases in myget.org).
    * For versions of OData 6.x and above, use the following
        * Add location: https://nuget.smbsrc.net/
        * To check for the existence of the symbols for your particular version, you can run the following command using [NuGet.exe](https://www.nuget.org/downloads): `nuget.exe list <namespace> -AllVersion -source https://nuget.smbsrc.net/`. (Example: `nuget.exe list Microsoft.AspNet.OData -AllVersion -source https://nuget.smbsrc.net/`)
2. Set the cache symbols directory in your, the path should be as short as it can be.

Turn on the CLR first change exception to do a quick debug, open `DEBUG` -> `Exceptions` in VS, check the `Common Language Runtime Exceptions`.