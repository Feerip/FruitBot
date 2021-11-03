@echo off
setlocal

rem First argument is the full path to the labs project root eg C:\Users\Tyler\source\repos\Discord.Net-Labs
rem Second argument is the full path to nuget.exe. If nuget.exe is in your PATH you can just pass in nuget.exe

if "%~1" == "" goto invalidLabsRoot
if not exist "%~1" goto invalidLabsRoot
if "%~2" == "" goto invalidNugetPath
if not exist "%~2" goto invalidNugetPath

set labsProjectRoot="%~1"
set nugetPath="%~2"
set libDir=Lib
set fruitBotDir=%~dp0

echo labs root is %labsProjectRoot%
echo nuget path is %nugetPath%

echo Packing projects...
call dotnet pack %labsProjectRoot%\src\Discord.Net.Core\Discord.Net.Core.csproj -o %fruitBotDir%%libDir% -c Release --version-suffix vaught
call dotnet pack %labsProjectRoot%\src\Discord.Net.Commands\Discord.Net.Commands.csproj -o %fruitBotDir%%libDir% -c Release --version-suffix vaught
call dotnet pack %labsProjectRoot%\src\Discord.Net.Webhook\Discord.Net.Webhook.csproj -o %fruitBotDir%%libDir% -c Release --version-suffix vaught
call dotnet pack %labsProjectRoot%\src\Discord.Net.WebSocket\Discord.Net.WebSocket.csproj -o %fruitBotDir%%libDir% -c Release --version-suffix vaught
call dotnet pack %labsProjectRoot%\src\Discord.Net.Rest\Discord.Net.Rest.csproj -o %fruitBotDir%%libDir% -c Release --version-suffix vaught
call dotnet pack %labsProjectRoot%\src\Discord.Net.Interactions\Discord.Net.Interactions.csproj -o %fruitBotDir%%libDir% -c Release --version-suffix vaught
echo Packing Complete!

echo Restoring Fruitbot sln...
call %nugetPath% restore FruitBot.sln
echo Restore Complete!

goto success

:invalidLabsRoot
echo Invalid labs project root directory
goto end

:invalidNugetPath
echo Invalid nuget path
goto end

:success
echo Success

:end

endlocal