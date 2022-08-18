# Copyright (c) 2022 David Barker all rights reserved
# Licensed under MIT
param (
    [string] $dir = "./src",
    [string] $from = "net6.0",
    [string] $to = "net6.0",
    [switch] $migrate = $false,
    [switch] $update = $false,
    [switch] $clean = $false,
    [switch] $restore = $false
)

function Main([string] $dir, [string] $targetFramework)
{
    $validSettings = $restore -or $clean -or $migrate -or $update

    if ($validSettings -eq $false)
    {
        Write-Host ""
        Write-Host "Usage:"
        Write-Host "update-projects.ps1 -dir [dir] -clean -migrate -restore -update -from -to"
        Write-Host ""
        Write-Host " -dir : The directory to start the processing, will search recursively"
        Write-Host " -clean : removes bin and obj directories recursively"
        Write-Host " -update : re-adds the packages to force version update to latest package version"
        Write-Host " -restore : forces a dotnet restore on all the projects"
        Write-Host " -migrate : replaces the current framework with the target framework, must supply -from and -to"
        Write-Host " -from : the framework to update netstandard2.1, net6.0 etc"
        Write-Host " -to : the new framework version netstandard2.1, net6.0 etc"
        Write-Host ""
        Write-Host ""

        Dump-Settings

        return
    }

    Dump-Settings

    if ($clean)
    {
        Remove-Obj -directory $dir
        Remove-Bin -directory $dir
    }

    if ($migrate)
    {
        Update-Framework -directory $dir -targetFramework $targetFramework
        Update-Launch -directory $dir -targetFramework $targetFramework
    }

    if ($update)
    {
        Update-Packages -directory $dir -targetFramework $targetFramework
    }

    if ($restore)
    {
        Restore-Projects -directory $dir
    }
}

function Dump-Settings()
{
    Write-Host "Current Settings:"
    Write-Host "  -dir      : $dir"
    Write-Host "  -clean    : $clean"
    Write-Host "  -migrate  : $migrate"
    Write-Host "  -update   : $update"
    Write-Host "  -restore  : $restore"
    Write-Host "  -from     : $from"
    Write-Host "  -to       : $to"
}

function Restore-Projects([string] $dir)
{
    Write-Host "Restore Projects"

    $projectFiles = Get-ChildItem $dir -Recurse -Filter *.csproj

    foreach ($projectFile in $projectFiles)
    {
        Write-Host "  Project: $($projectFile.Name)"

        if ($update)
        {
            & dotnet restore "$($projectFile.Directory.FullName)"
        }
    }
}

function Update-Framework([string] $dir, [string] $targetFramework)
{
    if ($migrate -eq $true)
    {
        $projectFiles = Get-ChildItem $dir -Recurse -Filter *.csproj

        Write-Host "Find and update projects from $($from) to $($to)"

        foreach ($projectFile in $projectFiles)
        {
            $projectXml = [xml](Get-Content $projectFile.FullName)

            if ($projectXml.Project.Sdk -ne "")
            {
                Write-Host "  Project: $($projectFile.Name) Current Setting: $($projectXml.Project.PropertyGroup.TargetFramework)"
                $currentFramework = $projectXml.Project.PropertyGroup.TargetFramework;
                $upgrade = $currentFramework -eq $from -and $currentFramework -ne $to
                if ($upgrade)
                {
                    if ($null -ne $projectXml.Project.PropertyGroup[0].TargetFramework)
                    {
                        Write-Host "    Migrate Project to $targetFramework.."
                        $projectXml.Project.PropertyGroup[0].TargetFramework = $targetFramework
                    }
                    else
                    {
                        Write-Host "    Migrate Project to $targetFramework."
                        $projectXml.Project.PropertyGroup.TargetFramework = $targetFramework
                    }
                    $projectXml.Save($projectFile.FullName)
                }
                else
                {
                    Write-Host "   - Nothing to do! Skipping!"
                }

            }
        }
    }
}

function Update-Packages([string] $dir, [string] $targetFramework)
{
    if ($update -eq $true)
    {
        $projectFiles = Get-ChildItem $dir -Recurse -Filter *.csproj

        foreach ($projectFile in $projectFiles)
        {
            $projectXml = [xml](Get-Content $projectFile.FullName)

            if ($projectXml.Project.Sdk -ne "")
            {
                $itemGroups = $projectXml.Project.ItemGroup

                Write-Host "  Project: $($projectFile.Name) Current Setting: $($projectXml.Project.PropertyGroup.TargetFramework)"

                Write-Host "    Update Packages:"

                foreach ($itemGroup in $itemGroups)
                {
                    if ($itemGroup.PackageReference)
                    {
                        foreach ($packageRef in $itemGroup.PackageReference)
                        {
                            Write-Host "      Package: $($packageRef.Include)"
                            & dotnet add "$($projectFile)" package "$($packageRef.Include)" | Out-File -FilePath "dotnet.log" -Append
                        }
                    }
                }

            }
        }
    }
}
function Update-Launch([string] $dir, [string] $targetFramework)
{
    Write-Host "Migrate Launch Files"
    $launchFiles = Get-ChildItem $dir -Recurse -Force -Filter launch.json

    foreach ($launchFile in $launchFiles)
    {
        Write-Host "." -NoNewline

        $newContent = (Get-Content $launchFile -Raw) -replace "/$from/", "/$targetFramework/"

        $newContent | Set-Content -Path $launchFile.FullName
    }
    Write-Host " [done]"
}

function Remove-Bin([string] $dir)
{
    Write-Host "Remove Bin Directories"
    $binDirectories = Get-ChildItem $dir -Recurse -Force -Directory -Filter bin

    foreach ($binDirectory in $binDirectories)
    {
        Write-Host "." -NoNewline
        Remove-Item -Path $binDirectory.FullName -Recurse -Force
    }
    Write-Host " [done]"
}

function Remove-Obj([string] $dir)
{
    Write-Host "Remove Obj Directories"
    $binDirectories = Get-ChildItem $dir -Recurse -Force -Directory -Filter obj

    foreach ($binDirectory in $binDirectories)
    {
        Write-Host "." -NoNewline
        Remove-Item -Path $binDirectory.FullName -Recurse -Force
    }
    Write-Host " [done]"
}

$targetFramework = $to

Main -dir $dir -targetFramework $targetFramework