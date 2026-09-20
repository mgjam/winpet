@echo off
rem The normal build renders the gallery using the same C# artwork as the pet.
dotnet build "%~dp0..\src\WinPet.sln" %*
exit /b %errorlevel%
