---
layout: post
title: "10.1 How To Debug"
description: "how to debug odata webapi source"
category: "10. Others"
---

If you want to debug **OData WebAPI** source, open `DEBUG` -> `Options and Settings` in VS, configure below things in `General` tab:

1. Uncheck `Enable Just My Code (Managed only)`.
2. Uncheck `Enable .NET Framework source stepping`.
3. Check `Enable source server support`.

Setup your symbol source in `Symbols` tab:

1. Check `Microsoft Symbol Servers`.
2. Add location: http://srv.symbolsource.org/pdb/Public (For preview/public releases in nuget.org).
3. Add location: http://srv.symbolsource.org/pdb/MyGet (For nightly build, and preview releases in myget.org).
4. Set the cache symbols directory in your, the path should be as short as it can be.

Turn on the CLR first change exception to do a quick debug, open `DEBUG` -> `Exceptions` in VS, check the `Common Language Runtime Exceptions`.
