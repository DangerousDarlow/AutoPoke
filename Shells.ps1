$dirs = "Server\bin\Debug\net7.0\", "Client\bin\Debug\net7.0\", "Client\bin\Debug\net7.0\", "Scripts\BeginSession\bin\Debug\net7.0\"

foreach ($dir in $dirs)
{
    Start-Process powershell.exe -ArgumentList "-NoExit", "cd $dir"
}