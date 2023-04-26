@echo off
SETLOCAL EnableDelayedExpansion
cd %~dp0
SET ROOT=%~dp0
set ZIPFILE=%ROOT%publish\XmlDiff.zip
for /f "usebackq" %%i in (%ROOT%src\Common\version.txt) do (
    set VERSION=%%i
)

if "%VERSION%" == "" goto :noversion

echo ### Publishing version %VERSION% ...
set GITRELEASE=1

:parse
if "%1"=="/norelease" set GITRELEASE=0
if "%1"=="" goto :done
shift
goto :parse

:done

git clean -dfx
pwsh -f %ROOT%src\Common\fix_version.ps1

nuget restore src\XmlDiff.sln
if ERRORLEVEL 1 goto :norestore
msbuild /target:rebuild src\XmlDiff.sln /p:Configuration=Release "/p:Platform=Any CPU"
if ERRORLEVEL 1 goto :buildfailed

pwsh -f %ROOT%src\Common\full_sign.ps1
if ERRORLEVEL 1 goto :err_sign

nuget pack src\XmlDiffView\XmlDiff.nuspec

if exist publish rd /s /q publish
mkdir publish
pwsh -command "Compress-Archive -Path src\XmlDiffView\bin\Release\* -DestinationPath %ZIPFILE%"
if ERRORLEVEL 1 goto :err_compress

if "%GITRELEASE%" == "0" goto :finished

echo Creating new release for version %VERSION%
gh release create %VERSION% "LovettSoftware.XmlDiff.%VERSION%.nupkg" "%ZIPFILE%" --title "Xml Diff %VERSION%"

:finished
call gitweb
goto :eof

:norestore
echo Error restoring XmlDiff.sln
exit /b 1

:buildfailed
echo Error building XmlDiff.sln
exit /b 1

:err_sign
echo Signing failed, try building inside XmlNotepadSetup.sln
exit /b 1

:err_compress
echo Error creating zip file
exit /b 1
