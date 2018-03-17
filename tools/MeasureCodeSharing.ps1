# Copyright (c) Microsoft Corporation.  All rights reserved.
# Licensed under the MIT License.  See License.txt in the project root for license information.

Param (
    [Parameter(Mandatory=$true)]
    [string] $RootPath
)

if (Test-Path "$RootPath\src")
{
    $aspnetCount = Get-ChildItem -Path "$RootPath\src\Microsoft.AspNet.OData" -Filter "*.cs" -Recurse | Get-Content | Measure-Object -line
    $sharedCount = Get-ChildItem -Path "$RootPath\src\Microsoft.AspNet.OData.Shared" -Filter "*.cs" -Recurse | Get-Content | Measure-Object -line
    $coreCount = Get-ChildItem -Path "$RootPath\src\Microsoft.AspNetCore.OData" -Filter "*.cs" -Recurse | Get-Content | Measure-Object -line

    $aspnetUnique = $aspnetCount.Lines / ($aspnetCount.Lines + $sharedCount.Lines)
    $aspnetShared = $sharedCount.Lines / ($aspnetCount.Lines + $sharedCount.Lines)
    "OData WebApi for ASP.NET has {0,5:p} unique lines and {1,5:p} shared lines" -f $aspnetUnique, $aspnetShared

    $coreUnique = $coreCount.Lines / ($coreCount.Lines  + $sharedCount.Lines)
    $coreShared = $sharedCount.Lines / ($coreCount.Lines  + $sharedCount.Lines)
    "OData WebApi for ASP.NET Core has {0,5:p} unique lines and {1,5:p} shared lines" -f $coreUnique, $coreShared

    "OData WebApi has rougly {0,5:p} shared lines of product code" -f (($aspnetShared + $coreShared) / 2)
}
else
{
    "Cannot find source code in '{0}'" -f "$RootPath\src"
}

