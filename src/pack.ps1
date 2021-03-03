
Write-Output "Package AspNetCore.Identity.MongoDbCore"

Remove-Item -Path "./bin/Release" -Force -Recurse

& dotnet pack -c Release -p:NuspecFile=AspNetCore.Identity.MongoDbCore.nuspec