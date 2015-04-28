param($configFile, $key, $value)

Set-ItemProperty $configFile -name IsReadOnly -value $false

[xml]$xml = New-Object XML
$xml.Load($configFile)

foreach($node in $xml.selectnodes("/configuration/appSettings/add"))
{
    if ($node.key.Equals($key))
    {
        $node.value = [string]$value
    }
}

$xml.Save($configFile)
Set-ItemProperty $configFile -name IsReadOnly -value $true