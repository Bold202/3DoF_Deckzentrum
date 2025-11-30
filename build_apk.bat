@echo off
REM ============================================================
REM D8-Planer XR - Automatischer APK Build Script (Windows)
REM ============================================================
REM
REM Dieses Script baut automatisch eine APK-Datei und exportiert
REM sie auf den Desktop.
REM
REM Verwendung:
REM   Doppelklick auf build_apk.bat
REM   ODER
REM   In Kommandozeile: build_apk.bat
REM
REM Voraussetzungen:
REM   - Unity 2022.3 LTS mit Android Build Support installiert
REM   - Projekt wurde mindestens einmal in Unity geoeffnet
REM   - Setup-Wizard wurde ausgefuehrt
REM
REM ============================================================

setlocal enabledelayedexpansion

echo ============================================================
echo    D8-Planer XR - Automatischer APK Build
echo ============================================================
echo.

REM ============================================================
REM Projektverzeichnis ermitteln
REM ============================================================

set "PROJECT_PATH=%~dp0"
REM Entferne abschliessenden Backslash
set "PROJECT_PATH=%PROJECT_PATH:~0,-1%"

echo [INFO] Projektverzeichnis: %PROJECT_PATH%
echo.

REM ============================================================
REM Unity Installation finden
REM ============================================================

set "UNITY_PATH="

REM Suche nach Unity 2022.3 LTS in verschiedenen Pfaden
call :FindUnity "C:\Program Files\Unity\Hub\Editor"
if defined UNITY_PATH goto :UnityFound

call :FindUnity "%LOCALAPPDATA%\Unity\Editor"
if defined UNITY_PATH goto :UnityFound

call :FindUnity "%USERPROFILE%\Unity\Hub\Editor"
if defined UNITY_PATH goto :UnityFound

REM Falls keine Unity 2022.3 gefunden, suche nach anderen Versionen
call :FindUnityAny "C:\Program Files\Unity\Hub\Editor"
if defined UNITY_PATH goto :UnityFoundWarning

call :FindUnityAny "%LOCALAPPDATA%\Unity\Editor"
if defined UNITY_PATH goto :UnityFoundWarning

call :FindUnityAny "%USERPROFILE%\Unity\Hub\Editor"
if defined UNITY_PATH goto :UnityFoundWarning

REM Unity nicht gefunden
echo.
echo [FEHLER] Unity konnte nicht gefunden werden!
echo.
echo Bitte installiere Unity 2022.3 LTS ueber Unity Hub
echo oder setze den Pfad manuell in diesem Script:
echo.
echo   set "UNITY_PATH=C:\Program Files\Unity\Hub\Editor\2022.3.XX\Editor\Unity.exe"
echo.
goto :Error

:UnityFound
echo [OK] Unity 2022.3 LTS gefunden: %UNITY_PATH%
goto :Continue

:UnityFoundWarning
echo [WARNUNG] Unity gefunden (nicht 2022.3 LTS): %UNITY_PATH%
goto :Continue

:Continue
echo.

REM ============================================================
REM Build-Konfiguration
REM ============================================================

REM Desktop-Pfad ermitteln
set "DESKTOP_PATH=%USERPROFILE%\Desktop"

REM Falls Desktop nicht existiert, nutze Home-Verzeichnis
if not exist "%DESKTOP_PATH%" (
    set "DESKTOP_PATH=%USERPROFILE%"
    echo [WARNUNG] Desktop-Verzeichnis nicht gefunden. APK wird in %DESKTOP_PATH% gespeichert.
)

REM APK-Dateiname mit Datum/Zeit
for /f "tokens=2 delims==" %%a in ('wmic OS Get localdatetime /value') do set "datetime=%%a"
set "TIMESTAMP=%datetime:~0,8%_%datetime:~8,4%"
set "APK_NAME=D8-Planer-XR_%TIMESTAMP%.apk"
set "APK_PATH=%DESKTOP_PATH%\%APK_NAME%"

REM Build-Log Pfad
set "BUILD_LOG=%PROJECT_PATH%\Logs\build.log"

echo [INFO] APK wird exportiert nach: %APK_PATH%
echo.

REM ============================================================
REM Unity Build starten
REM ============================================================

echo [INFO] Starte Unity Build-Prozess...
echo [INFO] Dies kann einige Minuten dauern. Bitte warten...
echo.

REM Logs-Verzeichnis erstellen falls nicht vorhanden
if not exist "%PROJECT_PATH%\Logs" mkdir "%PROJECT_PATH%\Logs"

REM Unity im Batch-Modus starten
"%UNITY_PATH%" ^
    -quit ^
    -batchmode ^
    -nographics ^
    -projectPath "%PROJECT_PATH%" ^
    -executeMethod D8PlanerXR.Editor.CommandLineBuild.BuildAndroid ^
    -buildOutput "%APK_PATH%" ^
    -logFile "%BUILD_LOG%"

set "BUILD_RESULT=%ERRORLEVEL%"

echo.

if %BUILD_RESULT% equ 0 (
    if exist "%APK_PATH%" (
        echo ============================================================
        echo    BUILD ERFOLGREICH!
        echo ============================================================
        echo.
        echo [OK] APK wurde erstellt: %APK_PATH%
        echo.
        
        REM Dateigroesse anzeigen
        for %%A in ("%APK_PATH%") do (
            set /a SIZE_MB=%%~zA / 1048576
            echo [INFO] Dateigroesse: !SIZE_MB! MB
        )
        echo.
        
        echo Naechste Schritte:
        echo   1. APK auf Android-Geraet kopieren
        echo   2. APK installieren: adb install "%APK_PATH%"
        echo   3. App testen
        echo.
        goto :Success
    )
)

echo ============================================================
echo    BUILD FEHLGESCHLAGEN!
echo ============================================================
echo.
echo [FEHLER] Build-Prozess hat Fehler zurueckgegeben: %BUILD_RESULT%
echo.
echo Pruefe das Build-Log fuer Details:
echo   %BUILD_LOG%
echo.
echo Haeufige Fehlerursachen:
echo   - Unity wurde noch nie mit diesem Projekt geoeffnet
echo   - Android Build Support fehlt
echo   - Setup-Wizard wurde nicht ausgefuehrt
echo   - XR Plugin Management nicht aktiviert
echo.
echo Loesungsschritte:
echo   1. Oeffne das Projekt in Unity
echo   2. Warte bis alle Assets importiert sind
echo   3. Fuehre den Setup-Wizard aus: D8-Planer ^> Alle Einstellungen anwenden
echo   4. Aktiviere ARCore: Edit ^> Project Settings ^> XR Plug-in Management
echo   5. Fuehre dieses Script erneut aus
echo.
goto :Error

REM ============================================================
REM Hilfsfunktionen
REM ============================================================

:FindUnity
REM Sucht nach Unity 2022.3 LTS
if not exist "%~1" exit /b 0
for /d %%D in ("%~1\2022.3*") do (
    if exist "%%D\Editor\Unity.exe" (
        set "UNITY_PATH=%%D\Editor\Unity.exe"
        exit /b 0
    )
)
exit /b 0

:FindUnityAny
REM Sucht nach beliebiger Unity-Version
if not exist "%~1" exit /b 0
for /d %%D in ("%~1\*") do (
    if exist "%%D\Editor\Unity.exe" (
        set "UNITY_PATH=%%D\Editor\Unity.exe"
        exit /b 0
    )
)
exit /b 0

:Success
echo Druecke eine beliebige Taste zum Beenden...
pause >nul
exit /b 0

:Error
echo Druecke eine beliebige Taste zum Beenden...
pause >nul
exit /b 1
