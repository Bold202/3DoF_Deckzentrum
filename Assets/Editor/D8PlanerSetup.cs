@echo off
REM ============================================================
REM D8-Planer XR - Automatischer APK Build Script (Windows)
REM ============================================================
REM 
REM Dieses Script baut automatisch eine APK-Datei und exportiert
REM sie auf den Desktop. Doppelklicken zum Ausfuehren.
REM
REM Voraussetzungen:
REM    - Unity 2022.3 LTS mit Android Build Support installiert
REM    - Projekt wurde mindestens einmal in Unity geoeffnet
REM    - Assets/Editor/D8PlanerSetup.cs muss im Projekt existieren
REM
REM ============================================================

setlocal enabledelayedexpansion

REM Farbige Ausgabe aktivieren (falls Terminal Support)
color 0B

echo ============================================================
echo    D8-Planer XR - Automatischer APK Build
echo ============================================================
echo.

REM Projektverzeichnis ermitteln (wo dieses Script liegt)
set "PROJECT_PATH=%~dp0"
set "PROJECT_PATH=%PROJECT_PATH:~0,-1%"

echo [INFO] Projektverzeichnis: %PROJECT_PATH%
echo.

REM ============================================================
REM Unity Installation finden
REM ============================================================

set "UNITY_PATH="

REM Suche in Standard Unity Hub Installationspfaden
set "UNITY_HUB_PATH=%PROGRAMFILES%\Unity\Hub\Editor"

REM Pruefe ob Unity Hub Editor Verzeichnis existiert
if exist "%UNITY_HUB_PATH%" (
    echo [INFO] Unity Hub gefunden in: %UNITY_HUB_PATH%
    
    REM Finde die neueste 2022.3 LTS Version
    for /d %%d in ("%UNITY_HUB_PATH%\2022.3*") do (
        if exist "%%d\Editor\Unity.exe" (
            set "UNITY_PATH=%%d\Editor\Unity.exe"
            echo [INFO] Unity gefunden: !UNITY_PATH!
        )
    )
)

REM Fallback: Wenn keine 2022.3 im Hub gefunden wurde, suche allgemein
if "!UNITY_PATH!"=="" (
    if exist "%UNITY_HUB_PATH%" (
        for /d %%d in ("%UNITY_HUB_PATH%\*") do (
            if exist "%%d\Editor\Unity.exe" (
                set "UNITY_PATH=%%d\Editor\Unity.exe"
                echo [INFO] Unity (Fallback) gefunden: !UNITY_PATH!
            )
        )
    )
)

REM Fallback: Alte Pfade
if "!UNITY_PATH!"=="" (
    if exist "%PROGRAMFILES%\Unity\Editor\Unity.exe" set "UNITY_PATH=%PROGRAMFILES%\Unity\Editor\Unity.exe"
    if exist "%PROGRAMFILES(x86)%\Unity\Editor\Unity.exe" set "UNITY_PATH=%PROGRAMFILES(x86)%\Unity\Editor\Unity.exe"
)

REM Abbruch wenn nichts gefunden
if "!UNITY_PATH!"=="" (
    echo.
    echo [FEHLER] Unity konnte nicht gefunden werden!
    echo Bitte Pfad im Script manuell anpassen.
    pause
    exit /b 1
)

echo.
echo [OK] Nutze Unity Version in: !UNITY_PATH!
echo.

REM ============================================================
REM Build-Konfiguration
REM ============================================================

REM Desktop-Pfad ermitteln
set "DESKTOP_PATH=%USERPROFILE%\Desktop"

REM APK-Dateiname mit Datum/Zeit
for /f "tokens=2 delims==" %%a in ('wmic OS Get localdatetime /value') do set "dt=%%a"
set "TIMESTAMP=%dt:~0,4%%dt:~4,2%%dt:~6,2%_%dt:~8,2%%dt:~10,2%"
set "APK_NAME=D8-Planer-XR_%TIMESTAMP%.apk"
set "APK_PATH=%DESKTOP_PATH%\%APK_NAME%"

REM Log Pfad
if not exist "%PROJECT_PATH%\Logs" mkdir "%PROJECT_PATH%\Logs"
set "BUILD_LOG=%PROJECT_PATH%\Logs\build.log"

echo [INFO] Ziel-Datei: %APK_PATH%
echo.

REM ============================================================
REM Unity Build starten
REM ============================================================

echo [INFO] Starte Unity Build-Prozess (Batchmode)...
echo [INFO] Dies kann einige Minuten dauern (Kaffee holen!)...
echo.

REM WICHTIG: -executeMethod muss exakt zum C#-Script passen (D8PlanerSetup.BuildAPK)
"!UNITY_PATH!" ^
    -quit ^
    -batchmode ^
    -nographics ^
    -projectPath "%PROJECT_PATH%" ^
    -executeMethod D8PlanerSetup.BuildAPK ^
    -logFile "%BUILD_LOG%"

set "BUILD_RESULT=%ERRORLEVEL%"

echo.
if %BUILD_RESULT% EQU 0 (
    echo ============================================================
    echo    BUILD ERFOLGREICH!
    echo ============================================================
    echo.
    echo [OK] APK liegt hier: %APK_PATH%
    echo.
) else (
    echo ============================================================
    echo    BUILD FEHLGESCHLAGEN! (Fehlercode: %BUILD_RESULT%)
    echo ============================================================
    echo.
    echo [FEHLER] Details siehe Logdatei:
    echo %BUILD_LOG%
    echo.
)

echo Druecke eine Taste zum Beenden...
pause >nul
exit /b %BUILD_RESULT%
