# Default to Debug
$Configuration = 'Debug'

# Color
$Success = 'Green'
$Warning = 'Yellow'
$Err = 'Red'

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
}

Function Info ($msg)
{
    Write-Host "[Info:]" $msg
}

if ($args.Count -eq 0)
{
    $TestType = 'Nightly'
    $Configuration = 'Release'
}
elseif ($args[0] -match 'quick' -or ($args[0] -match '-q')) 
{
    $TestType = "Quick"
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
    Error("Unknown input ""$args"". It can be empty or ""quick|DisableSkipStrongName|EnableSkipStrongName"".")
    exit
}

$Build = 'build'
if ($args -contains 'rebuild')
{
    $Build = 'rebuild'
}

$PROGRAMFILESX86 = [Environment]::GetFolderPath("ProgramFilesX86")
$env:ENLISTMENT_ROOT = Split-Path -Parent $MyInvocation.MyCommand.Definition
$ENLISTMENT_ROOT = Split-Path -Parent $MyInvocation.MyCommand.Definition
$LOGDIR = $ENLISTMENT_ROOT + "\bin"

# Default to use Visual Studio 2015
$VS14MSBUILD=$PROGRAMFILESX86 + "\MSBuild\14.0\Bin\MSBuild.exe"
$VSTEST = $PROGRAMFILESX86 + "\Microsoft Visual Studio 14.0\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe"

# Figure out the directory and path for SN.exe
$SN = $null
$SNx64 = $null
ForEach ($directory in [System.IO.Directory]::EnumerateDirectories($PROGRAMFILESX86 + "\Microsoft SDKs\Windows"))
{
    ForEach($sdkVersionDirectory in [System.IO.Directory]::EnumerateDirectories($directory + "\bin"))
    {
        if([System.IO.File]::Exists($sdkVersionDirectory + "\sn.exe") -and [System.IO.File]::Exists($sdkVersionDirectory + "\x64\sn.exe"))
        {
            $SN = $sdkVersionDirectory + "\sn.exe"
            $SNx64 = $sdkVersionDirectory + "\x64\sn.exe"
            break
        }
    }
}

# Use Visual Studio 2017 compiler for .NET Core and .NET Standard. Because VS2017 has different paths for different
# versions, we have to check for each version. Meanwhile, the dotnet CLI is required to run the .NET Core unit tests in this script.
# Furthurmore, Visual Studio 2017 has a Preview version as well which uses Microsoft Visual Studio\Preview as path instead of \2017
$VS15VARIANTS = "2017", "Preview"
$VS15VARIANTPATH = $null
ForEach ($variant in $VS15VARIANTS)
{
    $tempVSPath = ($PROGRAMFILESX86 + "\Microsoft Visual Studio\{0}") -f $variant
	if([System.IO.Directory]::Exists($tempVSPath))
    {
        $VS15VARIANTPATH = $tempVSPath
        break
    }
}

$VS15VERSIONS = "Enterprise", "Professional", "Community"
$VS15MSBUILD = $null
ForEach ($version in $VS15VERSIONS)
{
    $tempMSBuildPath = ($VS15VARIANTPATH + "\{0}\MSBuild\15.0\Bin\MSBuild.exe") -f $version
    if([System.IO.File]::Exists($tempMSBuildPath))
    {
        $VS15MSBUILD = $tempMSBuildPath
        $VSTEST = ($VS15VARIANTPATH + "\{0}\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe") -f $version
        break
    }
}

$DOTNETDIR = "C:\Program Files\dotnet\"
$DOTNETEXE = $null
if ([System.IO.File]::Exists($DOTNETDIR + "dotnet.exe"))
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

$ClassicUnitTestSLN = "WebApiOData.AspNet.sln"
$ClassicE2ETestSLN = "WebApiOData.E2E.AspNet.sln"
$NetCoreUnitTesSLN = "WebApiOData.AspNetCore.sln"
$NetCoreE2ETestSLN = "WebApiOData.E2E.AspNetCore.sln"
$NugetRestoreSolutions = $ClassicUnitTestSLN, $ClassicE2ETestSLN, $NetCoreUnitTesSLN, $NetCoreE2ETestSLN

$NetCoreProductPROJ = "\src\Microsoft.AspNetCore.OData\Microsoft.AspNetCore.OData.csproj "
$NetCoreUnitTestPROJ = "\test\UnitTest\Microsoft.AspNetCore.OData.Test\Microsoft.AspNetCore.OData.Test.csproj"
$NetCoreE2ETestPROJ = "\test\E2ETest\Microsoft.Test.E2E.AspNet.OData\Build.AspNetCore\Microsoft.Test.E2E.AspNetCore.OData.csproj"
$NugetRestoreDotNetProjects = $NetCoreProductPROJ, $NetCoreUnitTestPROJ, $NetCoreE2ETestPROJ

$ClassicProductDIR = $ENLISTMENT_ROOT + "\bin\$Configuration"
$ClassicProductDlls = "Microsoft.AspNet.OData.dll"
$ClassicUnitTestDIR = $ENLISTMENT_ROOT + "\bin\$Configuration\UnitTest\AspNet"
$ClassicUnitTestDlls = "Microsoft.AspNet.OData.Test.dll"
$ClassicE2ETestDIR = $ENLISTMENT_ROOT + "\bin\$Configuration\E2ETest\AspNet"
$ClassicE2ETestDlls = "Microsoft.Test.E2E.AspNet.OData.dll"

$NetCoreProductDIR = $ENLISTMENT_ROOT + "\bin\$Configuration\netstandard2.0"
$NetCoreProductDlls = "Microsoft.AspNetCore.OData.dll"
$NetCoreUnitTestDIR = $ENLISTMENT_ROOT + "\bin\$Configuration\UnitTest\AspNetCore\netcoreapp2.0"
$NetCoreUnitTestDlls = "Microsoft.AspNetCore.OData.Test.dll"
$NetCoreE2ETestDIR = $ENLISTMENT_ROOT + "\bin\$Configuration\E2ETest\AspNetCore"
$NetCoreE2ETestDlls = "Microsoft.Test.E2E.AspNetCore.OData.dll"

# .NET Core tests are different and require the dotnet tool. The tool references the .csproj (VS2017) files instead of dlls
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

$NetCoreE2ETestFramnework = "netfx"
$NetCoreE2ETestSuite = @()
ForEach($dll in $NetCoreE2ETestDlls)
{
    $NetCoreE2ETestSuite += $NetCoreE2ETestDIR + "\" + $dll
}

Function GetDlls
{
    $dlls = @()

    $dlls += $ClassicProductDIR + "\" + $ClassicProductDlls
    $dlls += $NetCoreProductDIR + "\" + $NetCoreProductDlls

    $dlls += $ClassicUnitTestDIR + "\" + $ClassicUnitTestDlls
    $dlls += $NetCoreUnitTestDIR + "\" + $NetCoreUnitTestDlls

    $dlls += $ClassicE2ETestDIR + "\" + $ClassicE2ETestDlls
    $dlls += $NetCoreE2ETestDIR + "\" + $NetCoreE2ETestDlls

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
    foreach($project in $NugetRestoreDotNetProjects)
    {
        & $DOTNETEXE  "restore" ($ENLISTMENT_ROOT + $project)
    }
    Success("Pull Nuget Packages Success")
}

Function TestSummary
{
    Write-Host 'Collecting test results'

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
Function RunBuild ($sln, $vsToolVersion)
{
    Info("Building $sln ...")
    $slnpath = $ENLISTMENT_ROOT + "\sln\$sln"
    $Conf = "/p:Configuration=" + "$Configuration"

    # Default to VS2015
    $MSBUILD = $VS14MSBUILD
    if($vsToolVersion -eq '15.0')
    {
        $MSBUILD=$VS15MSBUILD
    }

    # If VS2015 is not present, try to use VS2017
    if(![System.IO.File]::Exists($MSBUILD))
    {
        $MSBUILD = $VS15MSBUILD
    }

    & $MSBUILD $slnpath /t:$Build /m /nr:false /fl "/p:Platform=Any CPU" $Conf /p:Desktop=true `
        /flp:LogFile=$LOGDIR/msbuild.log /flp:Verbosity=Normal 1>$null 2>$null
    if($LASTEXITCODE -eq 0)
    {
        Success("Build $sln SUCCESS")
    }
    else
    {
        Error("Build $sln FAILED")
        Info("For more information, please open the following log file:")
        Info("$LOGDIR\msbuild.log")
        exit
    }
}

Function BuildProcess
{
    Info("Building Asp.Net OData Projects...")

    $script:BUILD_START_TIME = Get-Date
    if (Test-Path $BUILDLOG)
    {
        rm $BUILDLOG
    }

    # Asp.Net Classic (Product & Unit Test)
    RunBuild ($ClassicUnitTestSLN)

    # Asp.Net Core (Product & Unit Test)
    RunBuild ($NetCoreUnitTesSLN) -vsToolVersion '15.0'

    if ($TestType -ne 'Quick')
    {
        # Asp.Net Classic (Product & Unit Test & E2E)
        RunBuild ($ClassicE2ETestSLN)

        # Asp.Net Core (Product & Unit Test & E2E)
        RunBuild ($NetCoreE2ETestSLN) -vsToolVersion '15.0'
    }

    Success("Build Done!")
    Write-Host
    $script:BUILD_END_TIME = Get-Date
}

Function RunTest($title, $testdir, $framework)
{
    Info("Running test $title...")
    if ($framework -eq 'dotnet')
    {
        $Conf = "/p:Configuration=" + "$Configuration"
        foreach($testProj in $testdir)
        {
            Info("Launching $testProj...")
            & $DOTNETEXE "test" $testProj $Conf "--no-build" >> $TESTLOG
        }
    }
    else
    {
        Info("Launching $testdir...")
        & $VSTEST $testdir $XUNITADAPTER >> $TESTLOG
    }

    if($LASTEXITCODE -ne 0)
    {
        Error("Run $title FAILED")
    }
}

Function TestProcess
{
    Info("Testing Asp.Net OData Projects...")

    if (Test-Path $TESTLOG)
    {
        rm $TESTLOG
    }

    $script:TEST_START_TIME = Get-Date

    RunTest -title 'AspNetClassic UnitTest' -testdir $ClassicUnitTestSuite -framework $ClassicUnitTestFramework

    RunTest -title 'AspNetCore UnitTest' -testdir $NetCoreUnitTestSuite -framework $NetCoreUnitTestFramework

    if ($TestType -ne 'Quick')
    {
        RunTest -title 'AspNetClassic E2ETests' -testdir $ClassicE2ETestSuite -framework $ClassicE2ETestFramework

        RunTest -title 'AspNetCore E2ETests' -testdir $NetCoreE2ETestSuite -framework $NetCoreE2ETestFramework
    }

    Info("Test Done.")
    TestSummary
    $script:TEST_END_TIME = Get-Date
}

# Main Process

if (! (Test-Path $LOGDIR))
{
    mkdir $LOGDIR 1>$null
}

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