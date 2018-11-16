ECHO OFF

REM Set this path to the "SimpleSmtpInterceptor.Console" binaries after compiling
REM them or copying the compiled binaries elsewhere.
CD "..\SimpleSmtpInterceptor.Console\bin\Debug\netcoreapp2.1"

REM This is how you run a console application for dot net core.
dotnet SimpleSmtpInterceptor.ConsoleApp.dll

PAUSE