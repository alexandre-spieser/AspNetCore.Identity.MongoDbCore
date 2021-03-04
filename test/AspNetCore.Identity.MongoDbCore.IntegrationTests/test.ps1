
Write-Output ""
Write-Output "Running Tests with Code Coverage"
Write-Output ""


& 'dotnet' test /p:CollectCoverage=true
