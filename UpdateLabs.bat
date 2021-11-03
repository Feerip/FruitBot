@echo off
setlocal

rem set labsProjectRoot = %1
set labsProjectRoot=C:\Users\Tyler\source\repos\Discord.Net-Labs
set nugetPath=C:\nuget.exe
set libDir=Lib
set fruitBotDir=%~dp0

call dotnet pack %labsProjectRoot%\src\Discord.Net.Core\Discord.Net.Core.csproj -o %fruitBotDir%%libDir% -c Release --version-suffix vaught
call dotnet pack %labsProjectRoot%\src\Discord.Net.Commands\Discord.Net.Commands.csproj -o %fruitBotDir%%libDir% -c Release --version-suffix vaught
call dotnet pack %labsProjectRoot%\src\Discord.Net.Webhook\Discord.Net.Webhook.csproj -o %fruitBotDir%%libDir% -c Release --version-suffix vaught
call dotnet pack %labsProjectRoot%\src\Discord.Net.WebSocket\Discord.Net.WebSocket.csproj -o %fruitBotDir%%libDir% -c Release --version-suffix vaught
call dotnet pack %labsProjectRoot%\src\Discord.Net.Rest\Discord.Net.Rest.csproj -o %fruitBotDir%%libDir% -c Release --version-suffix vaught
call dotnet pack %labsProjectRoot%\src\Discord.Net.Interactions\Discord.Net.Interactions.csproj -o %fruitBotDir%%libDir% -c Release --version-suffix vaught

rem call %nugetPath% pack Lib\Discord.Net.nuspec -BasePath %labsProjectRoot%\src -OutputDirectory %libDir%

endlocal