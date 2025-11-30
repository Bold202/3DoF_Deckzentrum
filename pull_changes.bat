@echo off
REM ============================================================
REM D8-Planer XR - Git Pull Script (Windows)
REM ============================================================
REM
REM Dieses Script holt die neuesten Aenderungen aus dem
REM Git-Repository ins lokale Verzeichnis.
REM
REM Verwendung:
REM   Doppelklick auf pull_changes.bat
REM   ODER
REM   In Kommandozeile: pull_changes.bat
REM
REM Voraussetzungen:
REM   - Git muss installiert sein
REM   - Das Script liegt im Projektverzeichnis
REM
REM ============================================================

setlocal enabledelayedexpansion

echo ============================================================
echo    D8-Planer XR - Git Pull
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
REM Git pruefen
REM ============================================================

where git >nul 2>nul
if %ERRORLEVEL% neq 0 (
    echo.
    echo [FEHLER] Git konnte nicht gefunden werden!
    echo.
    echo Bitte installiere Git von: https://git-scm.com/downloads
    echo.
    goto :Error
)

echo [OK] Git gefunden.
echo.

REM ============================================================
REM Ins Projektverzeichnis wechseln
REM ============================================================

cd /d "%PROJECT_PATH%"
if %ERRORLEVEL% neq 0 (
    echo.
    echo [FEHLER] Konnte nicht ins Projektverzeichnis wechseln!
    echo.
    goto :Error
)

REM ============================================================
REM Pruefen ob es ein Git-Repository ist
REM ============================================================

if not exist ".git" (
    echo.
    echo [FEHLER] Kein Git-Repository gefunden!
    echo.
    echo Dieses Verzeichnis ist kein Git-Repository.
    echo Bitte klone das Repository zuerst oder verschiebe
    echo dieses Script in das richtige Verzeichnis.
    echo.
    goto :Error
)

echo [OK] Git-Repository erkannt.
echo.

REM ============================================================
REM Aktuellen Branch anzeigen
REM ============================================================

echo [INFO] Aktueller Branch:
git branch --show-current
echo.

REM ============================================================
REM Status vor Pull anzeigen
REM ============================================================

echo [INFO] Pruefe lokale Aenderungen...
git status --short

REM Warnung bei lokalen Aenderungen
for /f %%i in ('git status --porcelain') do (
    echo.
    echo [WARNUNG] Es gibt lokale Aenderungen im Repository.
    echo           Diese koennten beim Pull zu Konflikten fuehren.
    echo.
    set /p CONTINUE="Trotzdem fortfahren? (j/n): "
    if /i "!CONTINUE!" neq "j" (
        echo.
        echo [ABBRUCH] Pull wurde abgebrochen.
        echo.
        goto :Cancelled
    )
    goto :DoPull
)

:DoPull
echo.

REM ============================================================
REM Git Pull ausfuehren
REM ============================================================

echo [INFO] Hole neueste Aenderungen vom Remote-Repository...
echo.

git pull

set "PULL_RESULT=%ERRORLEVEL%"

echo.

if %PULL_RESULT% equ 0 (
    echo ============================================================
    echo    ERFOLGREICH!
    echo ============================================================
    echo.
    echo [OK] Aenderungen wurden erfolgreich geholt.
    echo.
    
    REM Zeige letzte Commits
    echo [INFO] Letzte 3 Commits:
    echo.
    git --no-pager log --oneline -3
    echo.
    
    goto :Success
)

echo ============================================================
echo    FEHLER BEIM PULL!
echo ============================================================
echo.
echo [FEHLER] Git Pull hat Fehler zurueckgegeben: %PULL_RESULT%
echo.
echo Moegliche Ursachen:
echo   - Merge-Konflikte mit lokalen Aenderungen
echo   - Keine Netzwerkverbindung
echo   - Authentifizierungsproblem
echo.
echo Loesungsvorschlaege:
echo   1. Sichere deine lokalen Aenderungen: git stash
echo   2. Fuehre den Pull erneut aus
echo   3. Wende Aenderungen wieder an: git stash pop
echo.
goto :Error

:Success
echo Druecke eine beliebige Taste zum Beenden...
pause >nul
exit /b 0

:Cancelled
echo Druecke eine beliebige Taste zum Beenden...
pause >nul
exit /b 0

:Error
echo Druecke eine beliebige Taste zum Beenden...
pause >nul
exit /b 1
