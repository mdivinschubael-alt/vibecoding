@echo off
setlocal
echo ========================================================
echo   PharmaGestion - Script de Publication Rapide
echo ========================================================

rem Verifier que dotnet est dans le PATH
where dotnet >nul 2>nul
if %ERRORLEVEL% neq 0 (
    if exist "C:\Program Files\dotnet\dotnet.exe" (
        set "PATH=C:\Program Files\dotnet;%PATH%"
    ) else (
        echo [ERREUR] Le kit SDK .NET est introuvable.
        pause
        exit /b 1
    )
)

echo [1/3] Execution des tests unitaires...
dotnet test "%~dp0..\PharmaGestion.Tests\PharmaGestion.Tests.csproj" -c Release --nologo
if %ERRORLEVEL% neq 0 (
    echo [ERREUR] Echec des tests automatises.
    pause
    exit /b %ERRORLEVEL%
)

echo [2/3] Publication Autonome (Self-Contained win-x64)...
dotnet publish "%~dp0..\PharmaGestion.UI\PharmaGestion.UI.csproj" -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -p:DebugType=None -p:DebugSymbols=false -o "%~dp0..\publish\win-x64-standalone" --nologo
if %ERRORLEVEL% neq 0 (
    echo [ERREUR] Echec de la publication autonome.
    pause
    exit /b %ERRORLEVEL%
)

echo [3/3] Nettoyage des fichiers symboles PDB...
del /q /f "%~dp0..\publish\win-x64-standalone\*.pdb" 2>nul

echo.
echo ========================================================
echo   Publication terminee avec succes !
echo   Executable genere : publish\win-x64-standalone\PharmaGestion.UI.exe
echo ========================================================
pause
