@echo off
setlocal

rem First argument is the full path to the labs project root eg C:\Users\Tyler\source\repos\Discord.Net-Labs

if "%~1" == "" goto invalidLabsRoot
if not exist "%~1" goto invalidLabsRoot

set labsProjectRoot="%~1"
set libDir=Lib
set fruitBotDir=%~dp0

echo labs root is %labsProjectRoot%

pushd %labsProjectRoot%
git checkout ApplicationCommandService
git reset --hard HEAD
echo Pulling ApplicationCommandService branch
git pull
popd

dotnet build %labsProjectRoot%\Discord.Net.sln -c Release

xcopy %labsProjectRoot%\src\Discord.Net.Core\bin\Release\net5.0\Discord.Net.Core.dll %libDir%\ /Y
xcopy %labsProjectRoot%\src\Discord.Net.Core\bin\Release\net5.0\Discord.Net.Core.xml %libDir%\ /Y
xcopy %labsProjectRoot%\src\Discord.Net.Commands\bin\Release\net5.0\Discord.Net.Commands.dll %libDir%\ /Y
xcopy %labsProjectRoot%\src\Discord.Net.Commands\bin\Release\net5.0\Discord.Net.Commands.xml %libDir%\ /Y
xcopy %labsProjectRoot%\src\Discord.Net.Webhook\bin\Release\net5.0\Discord.Net.Webhook.dll %libDir%\ /Y
xcopy %labsProjectRoot%\src\Discord.Net.Webhook\bin\Release\net5.0\Discord.Net.Webhook.xml %libDir%\ /Y
xcopy %labsProjectRoot%\src\Discord.Net.WebSocket\bin\Release\net5.0\Discord.Net.WebSocket.dll %libDir%\ /Y
xcopy %labsProjectRoot%\src\Discord.Net.WebSocket\bin\Release\net5.0\Discord.Net.WebSocket.xml %libDir%\ /Y
xcopy %labsProjectRoot%\src\Discord.Net.Rest\bin\Release\net5.0\Discord.Net.Rest.dll %libDir%\ /Y
xcopy %labsProjectRoot%\src\Discord.Net.Rest\bin\Release\net5.0\Discord.Net.Rest.xml %libDir%\ /Y
xcopy %labsProjectRoot%\src\Discord.Net.Interactions\bin\Release\net5.0\Discord.Net.Interactions.dll %libDir%\ /Y
xcopy %labsProjectRoot%\src\Discord.Net.Interactions\bin\Release\net5.0\Discord.Net.Interactions.xml %libDir%\ /Y