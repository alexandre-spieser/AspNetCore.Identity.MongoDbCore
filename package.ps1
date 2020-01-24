
$project="./src/AspNetCore.Identity.MongoDbCore.csproj"
$testProject="./test/AspNetCore.Identity.MongoDbCore.IntegrationTests/AspNetCore.Identity.MongoDbCore.IntegrationTests.csproj"
$configuration="Release"
$nuspecFile="AspnetCore.Identity.MongoDbCore.nuspec"
$output="./nuget"

dotnet build
dotnet test $testProject
dotnet pack --no-restore --no-build $project --configuration $configuration -p:NuspecFile=$nuspecFile -o $output