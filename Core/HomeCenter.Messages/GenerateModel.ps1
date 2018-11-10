Function Execute-Command ($commandPath, $commandArguments)
{
    $pinfo = New-Object System.Diagnostics.ProcessStartInfo
    $pinfo.FileName = $commandPath
    $pinfo.RedirectStandardError = $true
    $pinfo.RedirectStandardOutput = $true
    $pinfo.UseShellExecute = $false
    $pinfo.Arguments = $commandArguments
    $p = New-Object System.Diagnostics.Process
    $p.StartInfo = $pinfo
    $p.Start() | Out-Null
    $p.WaitForExit()
    [pscustomobject]@{
        stdout = $p.StandardOutput.ReadToEnd()
        stderr = $p.StandardError.ReadToEnd()
        ExitCode = $p.ExitCode  
    }
}

$userPath = $env:USERPROFILE
$grpcRoot = $userPath + "\.nuget\packages\Grpc.Tools\"
$grpcActualVersion = Get-ChildItem $grpcRoot -directory | Sort-Object Name -Descending | Select-Object Name -First 1 |  Select-Object -ExpandProperty "Name"
$grpcToolsPath = $grpcRoot + $grpcActualVersion + "\tools\windows_x64\"
$protoc = $grpcToolsPath + "protoc"
$grpc = $grpcToolsPath + "grpc_csharp_plugin.exe"
$executionDir = $PSScriptRoot
$protoDir = $executionDir + "\Proto"
$modelDir = $executionDir + "\Models"

$protoFiles = get-childitem $protoDir -recurse -force -include *.proto     
foreach($protoFile in $protoFiles)
{ 
    $fileName = $protoFile.Name

    # $outFilePath = $modelFolder + "\" + $protoFile.BaseName + ".cs"  
    # if(!(Test-Path -Path $outFilePath)){  
        $processResult = Execute-Command -commandPath $protoc -commandArguments "$fileName --csharp_out=$modelDir --proto_path=$protoDir --csharp_opt=file_extension=.g.cs --grpc_out . --plugin=protoc-gen-grpc=$grpc"
    # }  
  
}  

