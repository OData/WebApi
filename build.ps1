# reference to System.*
$SysDirectory = [System.IO.Directory]
$SysPath = [System.IO.Path]
$SysFile = [System.IO.File]

# Default to Debug
$Configuration = 'Debug'

# Color
$Success = 'Green'
$Warning = 'Yellow'
$Err = 'Red'

# Helper methods for log
Function Error ($msg)
{
    Write-Host "[Error:]" $msg -ForegroundColor $Err
}

Function Warning ($msg)
{
    Write-Host "[Warning:]" $msg -ForegroundColor $Warning
}

Function Success ($msg)
{
    Write-Host "[Success:]" $msg -ForegroundColor $Success
    Write-Host
}

Function Info ($msg)
{
    Write-Host "[Info:]" $msg
}

Function WrongArgs()
{
    Write-Host ("Unknown args input. It can be empty or ") -ForegroundColor $Err
    Write-Host ("    1) build.cmd netfx|netcore")
    Write-Host ("    2) build.cmd -quick [netfx|netcore]")
    Write-Host ("    3) build.cmd DisableSkipStrongName|EnableSkipStrongName")
    exit
}

<#
    Process the args:
    args can be empty, "netcore", "quick netfx", "DisableSkipStrongName" ...
#>
$TestCategory = 'full' # run NETFX and NETCore
$TestType = 'Nightly'
 
if ($args.Count -eq 0)
{
    $Configuration = 'Release'
}
elseif ($args.Count -eq 1)
{
    if ($args[0] -match 'quick' -or ($args[0] -match '-q'))
    {
        $TestType = "Quick"
    }
    elseif ($args[0] -match 'netfx')
    {
        $TestCategory = 'netfx' # only run NetFX related in Nightly
    }
    elseif ($args[0] -match 'netcore')
    {
        $TestCategory = 'netcore' # only run NetCore related in Nightly
    }
    elseif ($args[0] -match 'DisableSkipStrongName')
    {
        $TestType = "DisableSkipStrongName"
    }
    elseif ($args[0] -match 'EnableSkipStrongName')
    {
        $TestType = "EnableSkipStrongName"
    }
    else
    {
        WrongArgs
    }
}
elseif ($args.Count -eq 2)
{
    if($args[0] -match 'quick' -or ($args[0] -match '-q'))
    {
        $TestType = "Quick"
        if ($args -contains 'netfx')
        {
            $TestCategory = 'netfx' # only run NetFX related in Quick
        }
        elseif ($args -contains 'netcore')
        {
            $TestCategory = 'netcore' # only run NetCore related
        }
        else
        {
            WrongArgs
        }
    }
}
else
{
    WrongArgs
}

$Build = 'build'
if ($args -contains 'rebuild')
{
    $Build = 'rebuild'
}

# variables
$PROGRAMFILESX86 = [Environment]::GetFolderPath("ProgramFilesX86")
$env:ENLISTMENT_ROOT = Split-Path -Parent $MyInvocation.MyCommand.Definition
$ENLISTMENT_ROOT = Split-Path -Parent $MyInvocation.MyCommand.Definition
$LOGDIR = $ENLISTMENT_ROOT + "\bin"

# Figure out the directory and path for SN.exe
$SN = $null
$SNx64 = $null
$SNVersions = @()
ForEach ($directory in $SysDirectory::EnumerateDirectories($PROGRAMFILESX86 + "\Microsoft SDKs\Windows", "*A"))
{
    # remove the first char 'v'
    $directoryName = $SysPath::GetFileName($directory).substring(1)

    # remove the last char 'A'
    $directoryName = $directoryName.substring(0, $directoryName.LastIndexOf('A'))

    # parse to double "10.0"
    $versionNo = [System.Double]::Parse($directoryName, [System.Globalization.CultureInfo]::InvariantCulture)

    $fileobject = $null
    $fileobject = New-Object System.Object
    $fileobject | Add-Member -type NoteProperty -Name version -Value $versionNo
    $fileobject | Add-Member -type NoteProperty -Name directory -Value $directory

    $SNVersions += $fileobject
}

# using the latest version
$SNVersions = $SNVersions | Sort-Object -Property version -Descending

ForEach ($ver in $SNVersions)
{
    # only care about the folder has "bin" subfolder
    $snBinDirectory = $ver.directory + "\bin"
    if(!$SysDirectory::Exists($snBinDirectory))
    {
        continue
    }

    if($SysFile::Exists($snBinDirectory + "\sn.exe") -and $SysFile::Exists($snBinDirectory + "\x64\sn.exe"))
    {
        $SN = $snBinDirectory + "\sn.exe"
        $SNx64 = $snBinDirectory + "\x64\sn.exe"
        break
    }
    else
    {
        ForEach ($netFxDirectory in $SysDirectory::EnumerateDirectories($snBinDirectory, "NETFX * Tools") | Sort -Descending)
        {
            # currently, sorting descending for the NETFX version looks good.
            if($SysFile::Exists($netFxDirectory + "\sn.exe") -and $SysFile::Exists($netFxDirectory + "\x64\sn.exe"))
            {
                $SN = $netFxDirectory + "\sn.exe"
                $SNx64 = $netFxDirectory + "\x64\sn.exe"
                break
            }
        }
    }
    
    if ($SN -ne $null -and $SNx64 -ne $null)
    {
        break
    }
}

# Default to use Visual Studio 2019
$VS16MSBUILD=$PROGRAMFILESX86 + "\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\amd64\MSBuild.exe"
$VSTEST = $PROGRAMFILESX86 + "\Microsoft Visual Studio\2019\Enterprise\Common7\IDE\Extensions\TestPlatform\vstest.console.exe"

if (!$SysFile::Exists($VS16MSBUILD) -or !$SysFile::Exists($VSTEST))
{
    # Use Visual Studio 2019 compiler for .NET Core and .NET Standard. 
    # Visual Studio 2019 is required to build the .NET Core. 
    # Because VS2019 has different paths for different versions, we have to check for each version.
    # Meanwhile, the dotnet CLI is required to run the .NET Core unit tests in this script.
    # Furthurmore, Visual Studio 2019 has a Preview version as well which uses Microsoft Visual Studio\Preview as path instead of \2019
    $VS16VARIANTPATH = $null
    ForEach ($variant in "2019", "Preview")
    {
        $tempVSPath = ($PROGRAMFILESX86 + "\Microsoft Visual Studio\{0}") -f $variant
        if($SysDirectory::Exists($tempVSPath))
        {
            $VS16VARIANTPATH = $tempVSPath
            break
        }
    }

    ForEach ($version in "Enterprise", "Professional", "Community")
    {
        $tempMSBuildPath = ($VS16VARIANTPATH + "\{0}\MSBuild\Current\Bin\amd64\MSBuild.exe") -f $version
        if($SysFile::Exists($tempMSBuildPath))
        {
            $VS16MSBUILD = $tempMSBuildPath
            $VSTEST = ($VS16VARIANTPATH + "\{0}\Common7\IDE\Extensions\TestPlatform\vstest.console.exe") -f $version
            break
        }
    }
}

$DOTNETDIR = "C:\Program Files\dotnet\"
$DOTNETEXE = $null
if ($SysFile::Exists($DOTNETDIR + "dotnet.exe"))
{
    $DOTNETEXE = $DOTNETDIR + "dotnet.exe"
}
else
{
   Error("The dotnet CLI must be installed to run any .NET Core tests.")
   exit
}

# Other variables
$BUILDLOG = $LOGDIR + "\msbuild.log"
$TESTLOG = $LOGDIR + "\mstest.log"
$NUGETEXE = $ENLISTMENT_ROOT + "\sln\.nuget\NuGet.exe"
$NUGETPACK = $ENLISTMENT_ROOT + "\sln\packages"
$XUNITADAPTER = "/TestAdapterPath:" + $NUGETPACK + "\xunit.runner.visualstudio.2.3.1\build\_common"

# Solution files
$ClassicUnitTestSLN = "WebApiOData.AspNet.sln"
$ClassicE2ETestSLN = "WebApiOData.E2E.AspNet.sln"
$NetCoreUnitTesSLN = "WebApiOData.AspNetCore.sln"
$NetCoreE2ETestSLN = "WebApiOData.E2E.AspNetCore.sln"
$NugetRestoreSolutions = $ClassicUnitTestSLN, $ClassicE2ETestSLN, $NetCoreUnitTesSLN, $NetCoreE2ETestSLN

# C# project files
$NetCoreProductPROJ = "\src\Microsoft.AspNetCore.OData\Microsoft.AspNetCore.OData.csproj "
$NetCoreUnitTestPROJ = "\test\UnitTest\Microsoft.AspNetCore.OData.Test\Microsoft.AspNetCore.OData.Test.csproj"
$NetCoreE2ETestPROJ = "\test\E2ETest\Microsoft.Test.E2E.AspNet.OData\Build.AspNetCore\Microsoft.Test.E2E.AspNetCore.OData.csproj"
$NetCore3xE2ETestPROJ = "\test\E2ETest\Microsoft.Test.E2E.AspNet.OData\Build.AspNetCore3x\Microsoft.Test.E2E.AspNetCore3x.OData.csproj"
$NugetRestoreNetCoreProjects = $NetCoreProductPROJ, $NetCoreUnitTestPROJ, $NetCoreE2ETestPROJ, $NetCore3xE2ETestPROJ

# ASP.NET Classic OData output variables
$ClassicProductDIR = $ENLISTMENT_ROOT + "\bin\$Configuration"
$ClassicProductDlls = "Microsoft.AspNet.OData.dll"
$ClassicUnitTestDIR = $ENLISTMENT_ROOT + "\bin\$Configuration\UnitTest\AspNet"
$ClassicUnitTestDlls = "Microsoft.AspNet.OData.Test.dll"
$ClassicE2ETestDIR = $ENLISTMENT_ROOT + "\bin\$Configuration\E2ETest\AspNet"
$ClassicE2ETestDlls = "Microsoft.Test.E2E.AspNet.OData.dll"

# ASP.NET Core OData output variables
$NetCoreProductDIR = $ENLISTMENT_ROOT + "\bin\$Configuration\netstandard2.0"
$NetCoreProductDlls = "Microsoft.AspNetCore.OData.dll"

$NetCore3xProductDIR = $ENLISTMENT_ROOT + "\bin\$Configuration\netcoreapp3.0"
$NetCore3xProductDlls = "Microsoft.AspNetCore.OData.dll"

$NetCoreUnitTestDIR = $ENLISTMENT_ROOT + "\bin\$Configuration\UnitTest\AspNetCore\netcoreapp2.0"
$NetCoreUnitTestDlls = "Microsoft.AspNetCore.OData.Test.dll"
$NetCore3xUnitTestDIR = $ENLISTMENT_ROOT + "\bin\$Configuration\UnitTest\AspNetCore\netcoreapp3.0"
$NetCore3xUnitTestDlls = "Microsoft.AspNetCore.OData.Test.dll"

$NetCoreE2ETestDIR = $ENLISTMENT_ROOT + "\bin\$Configuration\E2ETest\AspNetCore"
$NetCoreE2ETestDlls = "Microsoft.Test.E2E.AspNetCore.OData.dll"

$NetCore3xE2ETestDIR = $ENLISTMENT_ROOT + "\bin\$Configuration\E2ETest\AspNetCore\netcoreapp3.0"
$NetCore3xE2ETestDlls = "Microsoft.Test.E2E.AspNetCore.OData.dll"

# .NET Core tests are different and require the dotnet tool. The tool references the .csproj (VS2019) files instead of dlls
# However, the AspNetCore E2E tests still use EF6 and are not compiled to a .NetCore binary.
$ClassicUnitTestFramework = "netfx"
$ClassicUnitTestSuite = @()
ForEach($dll in $ClassicUnitTestDlls)
{
    $ClassicUnitTestSuite += $ClassicUnitTestDIR + "\" + $dll
}

$NetCoreUnitTestFramework = "dotnet"
$NetCoreUnitTestSuite = @()
$NetCoreUnitTestProjs = $ENLISTMENT_ROOT + "\test\UnitTest\Microsoft.AspNetCore.OData.Test\Microsoft.AspNetCore.OData.Test.csproj"
ForEach($proj in $NetCoreUnitTestProjs)
{
    $NetCoreUnitTestSuite += $proj
}

$ClassicE2ETestFramework = "netfx"
$ClassicE2ETestSuite = @()
ForEach($dll in $ClassicE2ETestDlls)
{
    $ClassicE2ETestSuite += $ClassicE2ETestDIR + "\" + $dll
}

$NetCoreE2ETestFramework = "netfx"
$NetCoreE2ETestSuite = @()
ForEach($dll in $NetCoreE2ETestDlls)
{
    $NetCoreE2ETestSuite += $NetCoreE2ETestDIR + "\" + $dll
}

$NetCore3xE2ETestFramework = "dotnet"
$NetCore3xE2ETestSuite = @()
$NetCore3xUnitTestProjs = $ENLISTMENT_ROOT + "\test\E2ETest\Microsoft.Test.E2E.AspNet.OData\Build.AspNetCore3x\Microsoft.Test.E2E.AspNetCore3x.OData.csproj"
ForEach($proj in $NetCore3xUnitTestProjs)
{
    $NetCore3xE2ETestSuite += $proj
}

Function GetDlls
{
    $dlls = @()
    
    # ASP.NET Classic/Core Product
    $dlls += $ClassicProductDIR + "\" + $ClassicProductDlls
    $dlls += $NetCoreProductDIR + "\" + $NetCoreProductDlls
    $dlls += $NetCore3xProductDIR + "\" + $NetCore3xProductDlls

    # Unit tests
    $dlls += $ClassicUnitTestDIR + "\" + $ClassicUnitTestDlls
    $dlls += $NetCoreUnitTestDIR + "\" + $NetCoreUnitTestDlls
    $dlls += $NetCore3xUnitTestDIR + "\" + $NetCore3xUnitTestDlls
    
    # E2E tests
    $dlls += $ClassicE2ETestDIR + "\" + $ClassicE2ETestDlls
    $dlls += $NetCoreE2ETestDIR + "\" + $NetCoreE2ETestDlls
    $dlls += $NetCore3xE2ETestDIR + "\" + $NetCore3xE2ETestDlls
    return $dlls
}

Function SkipStrongName
{
    $SnLog = $LOGDIR + "\SkipStrongName.log"
    Out-File $SnLog

    Info('Skip strong name validations for assemblies...')

    $dlls = GetDlls
    ForEach ($dll in $dlls)
    {
        & $SN /Vr $dll | Out-File $SnLog -Append
    }

    ForEach ($dll in $dlls)
    {
        & $SNx64 /Vr $dll | Out-File $SnLog -Append
    }

    Success("SkipStrongName Done")
}

Function DisableSkipStrongName
{
    $SnLog = $LOGDIR + "\DisableSkipStrongName.log"
    Out-File $SnLog

    Info("Disable skip strong name validations for assemblies...")

    $dlls = GetDlls
    ForEach ($dll in $dlls)
    {
        & $SN /Vu $dll | Out-File $SnLog -Append
    }

    ForEach ($dll in $dlls)
    {
        & $SNx64 /Vu $dll | Out-File $SnLog -Append
    }

    Success("DisableSkipStrongName Done")
}

Function NugetRestoreSolution
{
    Info("Pull NuGet Packages...")
    
    foreach($solution in $NugetRestoreSolutions)
    {
        & $NUGETEXE "restore" ($ENLISTMENT_ROOT + "\sln\" + $solution)
    }
    
    # Starting with .NET Core 2.0 SDK, dotnet restore runs implicitly when you run dotnet build
    foreach($project in $NugetRestoreNetCoreProjects)
    {
        & $DOTNETEXE  "restore" ($ENLISTMENT_ROOT + $project)
    }
    
    Success("Pull Nuget Packages Success")
}

Function TestSummary
{
    Write-Host 'Collecting test results'

    if(!$SysFile::Exists($TESTLOG))
    {
        return
    }

    $file = Get-Content -Path $TESTLOG
    $pass = 0
    $skipped = 0
    $fail = 0
    $failedtest1 = New-Object -TypeName System.Collections.ArrayList
    $failedtest2 = New-Object -TypeName System.Collections.ArrayList
    $part = 1
    foreach ($line in $file)
    {
        # Consolidate logic for retrieving number of passed and skipped tests. Failed tests is separate due to the way
        # VSTest and DotNet (for .NET Core tests) report results differently.
        if ($line -match "^Total tests: .*") 
        {
            # The line is in this format:
            # Total tests: 5735. Passed: 5735. Failed: 0. Skipped: 0.
            # We want to extract the total passed and total skipped.
            
            # Extract total passed by taking the substring between "Passed: " and "."
            # The regex first extracts the string after the hardcoded "Passed: " (i.e. "#. Failed: #. Skipped: #.")
            # Then we tokenize by "." and retrieve the first token which is the number for passed.
            $pattern = "Passed: (.*)"
            $extractedNumber = [regex]::match($line, $pattern).Groups[1].Value.Split(".")[0]
            $pass += $extractedNumber
            
            # Extract total skipped by taking the substring between "Skipped: " and "."
            # The regex first extracts the string after the hardcoded "Skipped: " (i.e. "#.")
            # Then we tokenize by "." and retrieve the first token which is the number for skipped.
            $pattern = "Skipped: (.*)"
            $extractedNumber = [regex]::match($line, $pattern).Groups[1].Value.Split(".")[0]
            $skipped += $extractedNumber
        }
        elseif ($line -match "^Failed\s+(.*)")
        {
            $fail = $fail + 1
            if ($part -eq 1)
            {
                [void]$failedtest1.Add($Matches[1])
            }
            else
            {    
                [void]$failedtest2.Add($Matches[1])
            }
        }
    }

    Write-Host "Test summary:" -ForegroundColor $Success
    Write-Host "Passed :`t$pass"  -ForegroundColor $Success

    if ($skipped -ne 0)
    {
        Write-Host "Skipped:`t$skipped"  -ForegroundColor $Warning
    }

    $color = $Success
    if ($fail -ne 0)
    {
        $color = $Err
    }
    Write-Host "Failed :`t$fail"  -ForegroundColor $color
    Write-Host "---------------"  -ForegroundColor $Success
    Write-Host "Total :`t$($pass + $fail)"  -ForegroundColor $Success
    if ($fail -eq 0)
    {
        Write-Host "Congratulation! All of the tests passed!" -ForegroundColor $Success
    }
}

# Incremental build and rebuild
Function RunBuild($sln, $frameType)
{
    Write-Host -NoNewline "[Info:]" 
    Write-Host -NoNewline " Building " 
    Write-Host -NoNewline $sln -ForegroundColor 'Green'
    Write-Host -NoNewline " for " 
    Write-Host -NoNewline $frameType -ForegroundColor 'Red'
    Write-Host " ..."

    if ($frameType -eq 'netfx')
    {
        $slnpath = $ENLISTMENT_ROOT + "\sln\$sln"
        $Conf = "/p:Configuration=" + "$Configuration"

        & $VS16MSBUILD $slnpath /t:$Build /m /nr:false /fl "/p:Platform=Any CPU" $Conf /p:Desktop=true `
            /flp:LogFile=$LOGDIR/msbuild.log /flp:Verbosity=Normal 1>$null 2>$null
    }
    elseif ($frameType -eq 'netcore')
    {
        $fullNetCoreProj = $ENLISTMENT_ROOT + "\" + $sln

        & $DOTNETEXE build $fullNetCoreProj -c $Configuration /flp:v=diag /flp:logfile=$LOGDIR/corebuild.log
    }
    
    if($LASTEXITCODE -eq 0)
    {
        Success("Build $sln SUCCESS")
        Write-Host
    }
    else
    {
        Error("Build $sln FAILED")
        Info("For more information, please open the following log file:")
        if ($frameType -eq 'netfx')
        {
            Info("$LOGDIR\msbuild.log")
        }
        else
        {
            Info("$LOGDIR\corebuild.log")
        }
        exit
    }
}

<#
    Build the ASP.NET Classic
#>
Function BuildNetFxProcess
{
    # ASP.NET Classic (Product & Unit Test)
    RunBuild($ClassicUnitTestSLN) -frameType 'netfx'
	
    if ($TestType -ne 'Quick')
    {
        # ASP.NET Classic (Product & Unit Test & E2E)
        RunBuild ($ClassicE2ETestSLN) -frameType 'netfx'
    }
}

<#
    Build the ASP.NET Core
#>
Function BuildNetCoreProcess
{
    # ASP.Net Core (Product & Unit Test)
    RunBuild("test\UnitTest\Microsoft.AspNetCore.OData.Test\Microsoft.AspNetCore.OData.Test.csproj") -frameType 'netcore'
    
    if ($TestType -ne 'Quick')
    {
        # ASP.NET Core (Product & E2E)		
        RunBuild($NetCoreE2ETestSLN) -frameType 'netfx' # be noted, here's netfx

        RunBuild("test\E2ETest\Microsoft.Test.E2E.AspNet.OData\Build.AspNetCore3x\Microsoft.Test.E2E.AspNetCore3x.OData.csproj") -frameType 'netcore'
    }
}

<#
   Full build process
#>
Function BuildProcess
{
    Info ("Start Building ASP.NET OData Projects ...")

    $script:BUILD_START_TIME = Get-Date
    if (Test-Path $BUILDLOG)
    {
        rm $BUILDLOG
    }
    
    if ($TestCategory -eq 'netfx')
    {
        BuildNetFxProcess
    }
    elseif ($TestCategory -eq 'netcore')
    {
        BuildNetCoreProcess
    }
    else
    {
        BuildNetFxProcess
        BuildNetCoreProcess
    }

    Success("Build Done!")
    $script:BUILD_END_TIME = Get-Date
}

<#
    Run test
#>
Function RunTest($title, $testdir, $framework)
{
    Write-Host -NoNewline "[Info:]" 
    Write-Host -NoNewline " Running test " 
    Write-Host -NoNewline $title -ForegroundColor 'Green'
    Write-Host -NoNewline " for " 
    Write-Host -NoNewline $framework -ForegroundColor 'Red'
    Write-Host " ..."
    
    if ($framework -eq 'dotnet')
    {
        $Conf = "/p:Configuration=" + "$Configuration"
        foreach($testProj in $testdir)
        {
            Info("Launching $testProj...")
            
            & $DOTNETEXE "test" $testProj $Conf "--no-build"
        }
    }
    else
    {
        Info("Launching $testdir...")
        # & $VSTEST $testdir $XUNITADAPTER >> $TESTLOG
        & $VSTEST $testdir $XUNITADAPTER >> $TESTLOG
    }

    if($LASTEXITCODE -ne 0)
    {
        Error("Run $title FAILED")
    }
}

<#
    Run the ASP.NET Classic test
#>
Function RunNetFxTest
{
    # ASP.NET Classic (Product & Unit Test)
    RunTest -title 'AspNet Classic UnitTests' -testdir $ClassicUnitTestSuite -framework $ClassicUnitTestFramework

    if ($TestType -ne 'Quick')
    {
        # ASP.NET Classic (Product & E2E)
        RunTest -title 'AspNet Classic E2ETests' -testdir $ClassicE2ETestSuite -framework $ClassicE2ETestFramework
    }
}

<#
    Run the ASP.NET Core test
#>
Function RunNetCoreProcess
{
    # Asp.Net Core (Product & Unit Test)	
    # we can use --framework netcoreapp3.0 for certain framework, but here we run all the netcoreapp test cases
    RunTest -title 'AspNetCore UnitTest' -testdir $NetCoreUnitTestSuite -framework $NetCoreUnitTestFramework
    
    if ($TestType -ne 'Quick')
    {
        # Run ASP.NET Core 2.0 (Product & E2E)
        RunTest -title 'AspNetCore 2.0 E2ETests' -testdir $NetCoreE2ETestSuite -framework $NetCoreE2ETestFramework

        # Run ASP.NET Core 3.0 (Product & E2E)
        RunTest -title 'AspNetCore 3.0 E2ETests' -testdir $NetCore3xE2ETestSuite -framework $NetCore3xE2ETestFramework
    }
}

<#
    Main test process
#>
Function TestProcess
{
    Info("Testing ASP.NET OData Projects...")

    if (Test-Path $TESTLOG)
    {
        rm $TESTLOG
    }

    $script:TEST_START_TIME = Get-Date

    if ($TestCategory -eq 'netfx')
    {
        RunNetFxTest
    }
    elseif ($TestCategory -eq 'netcore')
    {
        RunNetCoreProcess
    }
    else
    {
        RunNetFxTest
        RunNetCoreProcess
    }

    Success("Test Done.")

    if ($TestCategory -ne 'netcore')
    {
        TestSummary
    }
    $script:TEST_END_TIME = Get-Date
}

<#
   Main Process
#>
if (! (Test-Path $LOGDIR))
{
    mkdir $LOGDIR 1>$null
}

Write-Host ("Test Type:`t" + $TestType) -ForegroundColor 'Green'
Write-Host ("Test Category:`t" + $TestCategory) -ForegroundColor 'Green'

if ($TestType -eq 'EnableSkipStrongName')
{
    NugetRestoreSolution
    BuildProcess
    SkipStrongName
    Exit
}
elseif ($TestType -eq 'DisableSkipStrongName')
{    
    NugetRestoreSolution
    BuildProcess
    DisableSkipStrongName
    Exit
}

NugetRestoreSolution
BuildProcess
SkipStrongName
TestProcess

$buildTime = New-TimeSpan $script:BUILD_START_TIME -end $script:BUILD_END_TIME
$testTime = New-TimeSpan $script:TEST_START_TIME -end $script:TEST_END_TIME
Write-Host("Build time:`t" + $buildTime)
Write-Host("Test time:`t" + $testTime)