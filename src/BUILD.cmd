@echo off

WHERE msbuild >NUL 2>NUL
IF ERRORLEVEL 1 goto :setup
goto :powershell

:setup
if "%VS140COMNTOOLS%" NEQ "" (
	call "%VS140COMNTOOLS%vsvars32.bat"
	goto :powershell
)
if "%VS150COMNTOOLS%" NEQ "" (
	call "%VS150COMNTOOLS%VsDevCmd.bat"
	goto :powershell
)
if "%VS160COMNTOOLS%" NEQ "" (
	call "%VS160COMNTOOLS%VsDevCmd.bat"
	goto :powershell
) else (
	echo "Run this script from the Visual Studio Developer Command Prompt"
	exit /B 1
)

:powershell
WHERE pwsh >NUL 2>NUL
IF ERRORLEVEL 1 (
	echo "Installing 'pwsh' (PowerShell Core)"
	dotnet tool install --global powershell
)

pwsh -f Common/fix_versions.ps1

REM Restore NuGet packages:
msbuild -t:restore XmlDiff.sln /verbosity:minimal /p:Configuration=Release "/p:Platform=Any CPU"
if ERRORLEVEL 1 exit /B 1

REM Build XmlDiff:
msbuild XmlDiff.sln /verbosity:minimal /p:Configuration=Release "/p:Platform=Any CPU"
if ERRORLEVEL 1 exit /B 1

if "%MyKeyFile%" == "" goto :eof

REM sign assemblies
pwsh -f Common\full_sign.ps1

REM create .nupkg
nuget pack XmlDiffView\XmlDiff.nuspec
