@echo off
rem The normal build renders the gallery using the same C# artwork as the pet.
dotnet build "%~dp0..\WinPet.csproj" %*
exit /b %errorlevel%
