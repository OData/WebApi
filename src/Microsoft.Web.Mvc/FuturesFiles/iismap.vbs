Const HKEY_LOCAL_MACHINE = &H80000002
Const MACHINE_NAME = "localhost"
Const DEFAULT_PATH = "W3SVC"
Const SCRIPT_MAPS = "ScriptMaps"

Function ExtensionExists(extension, fxVersion)
    Dim scriptExtension
    iisScriptMaps = GetScriptMaps()
    
    For scriptIndex = 0 To UBound(iisScriptMaps)
        scriptMap = iisScriptMaps(scriptIndex)
        decomposedScriptMap = Split(scriptMap, ",")
        scriptExtension = Right(decomposedScriptMap(0), Len(decomposedScriptMap(0))-1)
        If StrComp(LCase(scriptExtension), LCase(extension)) = 0 Then
          If InStr(scriptMap, fxVersion) > 0 Then
              ExtensionExists = true
            End If
        End If
    Next
    
    ExtensionExists = false
End Function

Function GetFrameworkPath(fxVersion)
    strComputer = "."
    Set registryObj = GetObject("winmgmts:{impersonationLevel=impersonate}!\\" & strComputer & "\root\default:StdRegProv")
 
    strKeyPath = "SOFTWARE\Microsoft\.NETFramework"
    strValueName = "InstallRoot"
    registryObj.GetStringValue HKEY_LOCAL_MACHINE, strKeyPath, strValueName, strValue

    GetFrameworkPath = strValue & fxVersion
End Function

Function GetFrameworkVersions()
    strComputer = "."
    Set registryObj = GetObject("winmgmts:{impersonationLevel=impersonate}!\\" & strComputer & "\root\default:StdRegProv")
    strKeyPath = "SOFTWARE\Microsoft\.NETFramework"
    registryObj.EnumKey HKEY_LOCAL_MACHINE, strKeyPath, arrSubKeys
    Set regex = New RegExp
    regex.Pattern = "^(v(2|4)\.\d+\.\d+(\.\d+)?)$"
    regex.Global = True
    ReDim fxVersions(0)

    For Each key in arrSubKeys
        If regex.Test(key) Then
            ReDim Preserve fxVersions(UBound(fxVersions)+1)
            fxVersions(UBound(fxVersions)-1) = key
        End If
    Next
   
    If UBound(fxVersions) > 0 Then
        Redim Preserve fxVersions(UBound(fxVersions)-1)
    End If
  
    GetFrameworkVersions = fxVersions
End Function

Function IsValidFrameworkVersion(fxVersion)
    strComputer = "."
    Set registryObj = GetObject("winmgmts:{impersonationLevel=impersonate}!\\" & strComputer & "\root\default:StdRegProv")
    strKeyPath = "SOFTWARE\Microsoft\.NETFramework\" & fxVersion
    registryObj.EnumKey HKEY_LOCAL_MACHINE, strKeyPath, arrSubKeys
    
    IsValidFrameworkVersion = Not IsNull(arrSubKeys)
End Function

Function GetScriptMaps()
    Dim iisObject
    Set iisObject = GetObject("IIS://" & MACHINE_NAME & "/" & DEFAULT_PATH)   
    GetScriptMaps = iisObject.Get(SCRIPT_MAPS)
End Function

Sub RegisterExtension(extension, fxVersion)
    Dim iisScriptMaps

    Set iisObject = GetObject("IIS://" & MACHINE_NAME & "/" & DEFAULT_PATH)
    iisScriptMaps = GetScriptMaps()
    
    If ExtensionExists(extension, fxVersion) Then
        WScript.Echo extension & " is already registered for .NET Framework " & fxVersion & "."
        WScript.Quit
    End If
    
    ReDim Preserve iisScriptMaps(UBound(iisScriptMaps)+1)
    
    iisScriptMaps(UBound(iisScriptMaps)) = "." & extension & "," & GetFrameworkPath(fxVersion) & "\aspnet_isapi.dll,1,GET,HEAD,POST"
    iisObject.Put "ScriptMaps", iisScriptMaps
    iisObject.Setinfo
End Sub

Sub SetScriptMaps(ScriptMaps)
    Dim iisObject
    
    Set iisObject = GetObject("ISS://" & MACHINE_NAME & "/" & DEFAULT_PATH)
    
    iisObject.Put SCRIPT_MAPS, ScriptMaps
    iisObject.Setinfo
End Sub

Sub UnregisterExtension(extension, fxVersion)
    Dim newScriptMaps
    
    Set iisObject = GetObject("IIS://" & MACHINE_NAME & "/" & DEFAULT_PATH)
    iisScriptMaps = GetScriptMaps()
    
    If Not ExtensionExists(extension, fxVersion) Then
        WScript.Echo extension & " is not registered for .NET Framework " & fxVersion & "."
        WScript.Quit
    End If

    ReDim newScriptMaps(UBound(iisScriptMaps)-1)
    Dim newScriptIndex
    newScriptIndex = 0

    For scriptIndex = 0 To UBound(iisScriptMaps)
        scriptMap = iisScriptMaps(scriptIndex)
        decomposedScriptMap = Split(scriptMap, ",")
        scriptExtension = Right(decomposedScriptMap(0), Len(decomposedScriptMap(0))-1)
        If (StrComp(LCase(scriptExtension), LCase(extension)) <> 0) Or ((StrComp(LCase(scriptExtension), LCase(extension)) = 0) And (InStr(scriptMap, fxVersion) = 0)) Then
            newScriptMaps(newScriptIndex) = scriptMap
            newScriptIndex = newScriptIndex + 1
        End If
    Next
    
    iisObject.Put "ScriptMaps", newScriptMaps
    iisObject.Setinfo
End Sub