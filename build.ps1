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
    $TestType = 'UnitTest'
    $Configuration = 'Release'
}
elseif ($args[0] -match 'full')
{
    $TestType = "FullTest"
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
	Error("Unknown input ""$args"". It can be empty or ""full|DisableSkipStrongName|EnableSkipStrongName"".")
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
$FXCOPDIR = $PROGRAMFILESX86 + "\Microsoft Visual Studio 14.0\Team Tools\Static Analysis Tools\FxCop"
$SN = $PROGRAMFILESX86 + "\Microsoft SDKs\Windows\v8.1A\bin\NETFX 4.5.1 Tools\sn.exe"
$SNx64 = $PROGRAMFILESX86 + "\Microsoft SDKs\Windows\v8.1A\bin\NETFX 4.5.1 Tools\x64\sn.exe"

# Use Visual Studio 2017 compiler for .NET Core and .NET Standard. Because VS2017 has different paths for different
# versions, we have to check for each version. Meanwhile, the dotnet CLI is required to run the .NET Core unit tests in this script.
$VS15VERSIONS = "Enterprise", "Professional", "Community"
$VS15MSBUILD = $null
ForEach ($version in $VS15VERSIONS)
{
    $tempMSBuildPath = ($PROGRAMFILESX86 + "\Microsoft Visual Studio\2017\{0}\MSBuild\15.0\Bin\MSBuild.exe") -f $version
    if([System.IO.File]::Exists($tempMSBuildPath))
    {
        $VS15MSBUILD = $tempMSBuildPath
        break
    }
}

$DOTNETDIR = "C:\Program Files\dotnet\"
$DOTNETTEST = $null
if ([System.IO.File]::Exists($DOTNETDIR + "dotnet.exe"))
{
    $DOTNETTEST = $DOTNETDIR + "dotnet.exe"
}
else
{
   Error("The dotnet CLI must be installed to run any .NET Core tests.")
   exit
}

# Other variables
$FXCOP = $FXCOPDIR + "\FxCopCmd.exe"
$BUILDLOG = $LOGDIR + "\msbuild.log"
$TESTLOG = $LOGDIR + "\mstest.log"
$TESTDIR = $ENLISTMENT_ROOT + "\bin\$Configuration\UnitTest"
$NETCORETESTDIR = $ENLISTMENT_ROOT + "\bin\$Configuration\Test\.NETPortable\netcoreapp1.0"
$PRODUCTDIR = $ENLISTMENT_ROOT + "\bin\$Configuration\Product\Desktop"
$NUGETEXE = $ENLISTMENT_ROOT + "\sln\.nuget\NuGet.exe"
$NUGETPACK = $ENLISTMENT_ROOT + "\sln\packages"
$XUNITADAPTER = "/TestAdapterPath:" + $NUGETPACK + "\xunit.runner.visualstudio.2.3.1\build\_common"

$NugetRestoreSolutions = "WebApiOData.AspNet.sln", "WebApiOData.E2E.AspNet.sln", "WebApiOData.AspNetCore.sln", "WebApiOData.E2E.AspNetCore.sln"

$ClassicProductDIR = $ENLISTMENT_ROOT + "\bin\$Configuration"
$ClassicProductDlls = "Microsoft.AspNet.OData.dll"
$ClassicUnitTestDIR = $ENLISTMENT_ROOT + "\bin\$Configuration\UnitTest"
$ClassicUnitTestDlls = "Microsoft.AspNet.OData.Test.dll"

#$ClassicE2ETestDlls = "Microsoft.Test.E2E.AspNet.OData.dll"

$NetCoreProductDIR = $ENLISTMENT_ROOT + "\bin\$Configuration\netstandard2.0"
$NetCoreProductDlls = "Microsoft.AspNetCore.OData.dll"
$NetCoreUnitTestDIR = $ENLISTMENT_ROOT + "\bin\$Configuration\netcoreapp2.0"
$NetCoreUnitTestDlls = "Microsoft.AspNetCore.OData.Test.dll"

#$NetCoreE2ETEstDlls = "Microsoft.Test.E2E.AspNetCore.OData.dll"

# .NET Core tests are different and require the dotnet tool. The tool references the .csproj (VS2017) files instead of dlls
$NetCoreUnitTestProjs = "\test\UnitTest\Microsoft.AspNetCore.OData.Test\Microsoft.AspNetCore.OData.Test.csproj"
$NetCoreE2ETestProjs = "\test\E2ETest\Microsoft.Test.E2E.AspNet.OData\Build.AspNetCore\Microsoft.Test.E2E.AspNetCore.OData.csproj"

$ClassicUnitTestSuite = @()
ForEach($dll in $ClassicUnitTestDlls)
{
    $ClassicUnitTestSuite += $ClassicUnitTestDIR + "\" + $dll
}

$FxCopRulesOptions = "/rule:$FxCopDir\Rules\DesignRules.dll",
    "/rule:$FxCopDir\Rules\NamingRules.dll",
    "/rule:$FxCopDir\Rules\PerformanceRules.dll",
    "/rule:$FxCopDir\Rules\SecurityRules.dll",
    "/rule:$FxCopDir\Rules\GlobalizationRules.dll",
    "/dictionary:$ENLISTMENT_ROOT\src\CustomDictionary.xml",
    "/ruleid:-Microsoft.Design#CA1006",
    "/ruleid:-Microsoft.Design#CA1016",
    "/ruleid:-Microsoft.Design#CA1020",
    "/ruleid:-Microsoft.Design#CA1021",
    "/ruleid:-Microsoft.Design#CA1045",
    "/ruleid:-Microsoft.Design#CA2210",
    "/ruleid:-Microsoft.Performance#CA1814"
$DataWebRulesOption = "/rule:$TESTDIR\DataWebRules.dll"

Function GetDlls
{
    $dlls = @()

	$dlls += $CLASSICPRODUCTDIR + "\" + $ClassicProductDlls
	$dlls += $NetCoreProductDIR + "\" + $NetCoreProductDlls
	
    $dlls += $ClassicUnitTestDIR + "\" + $ClassicUnitTestDlls
	$dlls += $NetCoreUnitTestDIR + "\" + $NetCoreUnitTestDlls

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
	Success("Pull Nuget Packages Success")
}

Function FailedTestLog ($playlist , $reruncmd , $failedtest1 ,$failedtest2)
{
    Write-Output "<Playlist Version=`"1.0`">" | Out-File $playlist
    Write-Output "@echo off" | Out-File -Encoding ascii $reruncmd
    Write-Output "cd $TESTDIR" | Out-File -Append -Encoding ascii $reruncmd
    $rerun = "`"$VSTEST`""
    if ($TestType -eq 'FullTest')
    {
        foreach ($dll in $FullTestSuite) 
        {
            $rerun += " $dll" 
        }
    }
    else
    {
        foreach ($dll in $UnitTestSuite) 
        {
            $rerun += " $dll" 
        }
    }
    if ($failedtest1.count -gt 0)
    {
        $rerun += " /Tests:"
    }
    foreach($case in $failedtest1)
    {
        $name = $case.split('.')[-1]
        $rerun += $name + ","
        $output = "<Add Test=`"" + $case + "`" />"
        Write-Output $output  | Out-File -Append $playlist
    } 
    # build the command only if failed tests exist
    if ($failedtest1.count -gt 0)
    {
        $rerun += " " + $XUNITADAPTER
        Write-Output $rerun | Out-File -Append -Encoding ascii $reruncmd
    }
    $rerun = "`"$VSTEST`""
    foreach ($dll in $E2eTestSuite)
    {
        $rerun += " $dll" 
    }
    if ($failedtest2.count -gt 0)
    {
        $rerun += " /Tests:"
    }
    foreach($case in $failedtest2)
    {
        $name = $case.split('.')[-1]
        $rerun += $name + ","
        $output = "<Add Test=`"" + $case + "`" />"
        Write-Output $output  | Out-File -Append $playlist
    }
    # build the command only if failed tests exist
    if ($failedtest2.count -gt 0)
    {
        Write-Output $rerun | Out-File -Append -Encoding ascii $reruncmd
    }
    Write-Output "cd $LOGDIR" | Out-File -Append -Encoding ascii $reruncmd
    Write-Output "</Playlist>" | Out-File -Append $playlist
    Write-Host "There are some test cases failed!" -ForegroundColor $Err
    Write-Host "To replay failed tests, please open the following playlist file:" -ForegroundColor $Err
    Write-Host $playlist -ForegroundColor $Err
    Write-Host "To rerun failed tests, please run the following script:" -ForegroundColor $Err
    Write-Host $reruncmd -ForegroundColor $Err
}

Function TestSummary
{
    Write-Host 'Collecting test results'
    $playlist = "$LOGDIR\FailedTests.playlist"
    $reruncmd = "$LOGDIR\rerun.cmd"
    if (Test-Path $playlist)
    {
        rm $playlist
    }
    if (Test-Path $reruncmd)
    {
        rm $reruncmd
    }
    
    $file = Get-Content -Path $TESTLOG
    $pass = 0
    $skipped = 0
    $fail = 0
    $trxfile = New-Object -TypeName System.Collections.ArrayList
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
        elseif ($line -match "^Results file: (.*)")
        {
            [void]$trxfile.Add($Matches[1])
            $part = 2
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
    Write-Host "For more information, please open the following test result files:"
    foreach ($trx in $trxfile)
    {
        Write-Host $trx
    }
    if ($fail -gt 0)
    {
        FailedTestLog -playlist $playlist -reruncmd $reruncmd -failedtest1 $failedtest1 -failedtest2 $failedtest2 
    }
    else
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
	
    & $MSBUILD $slnpath /t:$Build /m /nr:false /fl "/p:Platform=Any CPU" $Conf /p:Desktop=true `
        /flp:LogFile=$LOGDIR/msbuild.log /flp:Verbosity=Normal 1>$null 2>$null
    if($LASTEXITCODE -eq 0)
    {
        Success("Build $sln SUCCESS")
    }
    else
    {
        Error("Build $sln FAILED")
        Info("For more information, please open the following test result files:")
        Info("$LOGDIR\msbuild.log")
        exit
    }
}

Function BuildProcess
{
    Info("Start To build the whole Asp.Net OData Project...")
    
    $script:BUILD_START_TIME = Get-Date
    if (Test-Path $BUILDLOG)
    {
        rm $BUILDLOG
    }

	# Asp.Net Classic (Product & Unit Test)
	RunBuild ('WebApiOData.AspNet.sln')
		
	# Asp.Net Core (Product & Unit Test)
	RunBuild ('WebApiOData.AspNetCore.sln') -vsToolVersion '15.0'

    if ($TestType -eq 'FullTest')
    {
	    # Asp.Net Classic (Product & Unit Test & E2E)
        RunBuild ('WebApiOData.E2E.AspNet.sln')
		
		# Asp.Net Core (Product & Unit Test & E2E)
        # RunBuild ('WebApiOData.E2E.AspNetCore.sln') -vsToolVersion '15.0'
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
            & $DOTNETTEST "test" ($ENLISTMENT_ROOT + $testProj) $Conf "--no-build" >> $TESTLOG
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
    Info("Start to run the test case...")
    if (Test-Path $TESTLOG)
    {
        rm $TESTLOG
    }
    $script:TEST_START_TIME = Get-Date
    cd $TESTDIR
	
	RunTest -title 'AspNetClassic UnitTest' -testdir $ClassicUnitTestSuite
		
	RunTest -title 'AspNetCore UnitTest' -testdir $NetCoreUnitTestProjs -framework 'dotnet'
		
    if ($TestType -eq 'FullTest')
    {
		# So far, it can't work for e2e.
        # RunTest -title 'AspNetClassic E2ETests' -testdir $FullTestSuite
				
		# RunTest -title 'AspNetCore E2ETests' -testdir $NetCoreE2ETestProjs -framework 'dotnet'
    }

    Success("Test Done!")
    TestSummary
    $script:TEST_END_TIME = Get-Date
    cd $ENLISTMENT_ROOT
}

Function FxCopProcess
{
    Info("Start To FxCop ...")
	
    & $FXCOP "/f:$ProductDir\Microsoft.AspNet.OData.dll" "/o:$LOGDIR\AspNetODataFxCopReport.xml" $DataWebRulesOption `
        $FxCopRulesOptions 1>$null 2>$null
    & $FXCOP "/f:$ProductDir\Microsoft.AspNetCore.OData.dll" "/o:$LOGDIR\AspNetCoreODataFxCopReport.xml" $DataWebRulesOption `
	    $FxCopRulesOptions 1>$null 2>$null
		
    Info "For more information, please open the following test result files:"
    Info "$LOGDIR\AspNetODataFxCopReport.xml"
    Info "$LOGDIR\AspNetCoreODataFxCopReport.xml"
    Success "FxCop Done"
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
FxCopProcess

$buildTime = New-TimeSpan $script:BUILD_START_TIME -end $script:BUILD_END_TIME
$testTime = New-TimeSpan $script:TEST_START_TIME -end $script:TEST_END_TIME
Write-Host("Build time:`t" + $buildTime)
Write-Host("Test time:`t" + $testTime)