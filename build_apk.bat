@echo off
REM ============================================================
REM D8-Planer XR - Automatischer APK Build Script (Windows)
REM ============================================================
REM 
REM Dieses Script baut automatisch eine APK-Datei und exportiert
REM sie auf den Desktop. Doppelklicken zum Ausfuehren.
REM
REM Voraussetzungen:
REM   - Unity 2022.3 LTS mit Android Build Support installiert
REM   - Projekt wurde mindestens einmal in Unity geoeffnet
REM   - Setup-Wizard wurde ausgefuehrt
REM
REM ============================================================

setlocal enabledelayedexpansion

REM Farbige Ausgabe aktivieren
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
    
    REM Falls keine 2022.3 gefunden, suche nach anderen Versionen
    if "!UNITY_PATH!"=="" (
        for /d %%d in ("%UNITY_HUB_PATH%\*") do (
            if exist "%%d\Editor\Unity.exe" (
                set "UNITY_PATH=%%d\Editor\Unity.exe"
                echo [INFO] Unity gefunden: !UNITY_PATH!
            )
        )
    )
)

REM Pruefe alternative Installationspfade
if "!UNITY_PATH!"=="" (
    REM Alte Unity Installationen
    if exist "%PROGRAMFILES%\Unity\Editor\Unity.exe" (
        set "UNITY_PATH=%PROGRAMFILES%\Unity\Editor\Unity.exe"
    )
    if exist "%PROGRAMFILES(x86)%\Unity\Editor\Unity.exe" (
        set "UNITY_PATH=%PROGRAMFILES(x86)%\Unity\Editor\Unity.exe"
    )
)

REM Pruefe ob Unity gefunden wurde
if "!UNITY_PATH!"=="" (
    echo.
    echo [FEHLER] Unity konnte nicht gefunden werden!
    echo.
    echo Bitte installiere Unity 2022.3 LTS ueber Unity Hub
    echo oder setze den Pfad manuell in diesem Script:
    echo.
    echo   set UNITY_PATH=C:\Program Files\Unity\Hub\Editor\2022.3.XX\Editor\Unity.exe
    echo.
    pause
    exit /b 1
)

echo.
echo [OK] Unity gefunden: !UNITY_PATH!
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
"!UNITY_PATH!" ^
    -quit ^
    -batchmode ^
    -nographics ^
    -projectPath "%PROJECT_PATH%" ^
    -executeMethod D8PlanerXR.Editor.CommandLineBuild.BuildAndroid ^
    -buildOutput "%APK_PATH%" ^
    -logFile "%BUILD_LOG%"

REM Exit-Code pruefen
set "BUILD_RESULT=%ERRORLEVEL%"

echo.
if %BUILD_RESULT% EQU 0 (
    echo ============================================================
    echo    BUILD ERFOLGREICH!
    echo ============================================================
    echo.
    echo [OK] APK wurde erstellt: %APK_PATH%
    echo.
    
    REM Dateigroesse anzeigen
    if exist "%APK_PATH%" (
        for %%A in ("%APK_PATH%") do (
            set /a SIZE=%%~zA / 1048576
            echo [INFO] Dateigroesse: !SIZE! MB
        )
    )
    echo.
    echo Naechste Schritte:
    echo   1. APK auf Android-Geraet kopieren
    echo   2. APK installieren
    echo   3. App testen
    echo.
) else (
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
)

echo.
echo Druecke eine Taste zum Beenden...
pause >nul
exit /b %BUILD_RESULT%
