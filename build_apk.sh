#!/bin/bash
# ============================================================
# D8-Planer XR - Automatischer APK Build Script (macOS/Linux)
# ============================================================
#
# Dieses Script baut automatisch eine APK-Datei und exportiert
# sie auf den Desktop.
#
# Verwendung:
#   chmod +x build_apk.sh
#   ./build_apk.sh
#
# Voraussetzungen:
#   - Unity 2022.3 LTS mit Android Build Support installiert
#   - Projekt wurde mindestens einmal in Unity geoeffnet
#   - Setup-Wizard wurde ausgefuehrt
#
# ============================================================

set -e

# Farben fuer Ausgabe
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}============================================================${NC}"
echo -e "${BLUE}   D8-Planer XR - Automatischer APK Build${NC}"
echo -e "${BLUE}============================================================${NC}"
echo ""

# ============================================================
# Projektverzeichnis ermitteln
# ============================================================

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_PATH="$SCRIPT_DIR"

echo -e "${YELLOW}[INFO]${NC} Projektverzeichnis: $PROJECT_PATH"
echo ""

# ============================================================
# Unity Installation finden
# ============================================================

UNITY_PATH=""

# Funktion um Unity zu finden
find_unity() {
    local search_paths=()
    
    # macOS Unity Hub Pfade
    if [[ "$OSTYPE" == "darwin"* ]]; then
        search_paths+=(
            "/Applications/Unity/Hub/Editor"
            "$HOME/Applications/Unity/Hub/Editor"
        )
    fi
    
    # Linux Unity Hub Pfade
    if [[ "$OSTYPE" == "linux-gnu"* ]]; then
        search_paths+=(
            "$HOME/Unity/Hub/Editor"
            "/opt/Unity/Hub/Editor"
        )
    fi
    
    # Suche nach Unity 2022.3 LTS zuerst
    for base_path in "${search_paths[@]}"; do
        if [[ -d "$base_path" ]]; then
            for version_dir in "$base_path"/2022.3*; do
                if [[ -d "$version_dir" ]]; then
                    if [[ "$OSTYPE" == "darwin"* ]]; then
                        unity_exe="$version_dir/Unity.app/Contents/MacOS/Unity"
                    else
                        unity_exe="$version_dir/Editor/Unity"
                    fi
                    
                    if [[ -f "$unity_exe" ]]; then
                        UNITY_PATH="$unity_exe"
                        echo -e "${GREEN}[OK]${NC} Unity 2022.3 LTS gefunden: $UNITY_PATH"
                        return 0
                    fi
                fi
            done
        fi
    done
    
    # Falls keine 2022.3 gefunden, suche nach anderen Versionen
    for base_path in "${search_paths[@]}"; do
        if [[ -d "$base_path" ]]; then
            for version_dir in "$base_path"/*; do
                if [[ -d "$version_dir" ]]; then
                    if [[ "$OSTYPE" == "darwin"* ]]; then
                        unity_exe="$version_dir/Unity.app/Contents/MacOS/Unity"
                    else
                        unity_exe="$version_dir/Editor/Unity"
                    fi
                    
                    if [[ -f "$unity_exe" ]]; then
                        UNITY_PATH="$unity_exe"
                        echo -e "${YELLOW}[WARNUNG]${NC} Unity gefunden (nicht 2022.3 LTS): $UNITY_PATH"
                        return 0
                    fi
                fi
            done
        fi
    done
    
    return 1
}

# Unity suchen
if ! find_unity; then
    echo ""
    echo -e "${RED}[FEHLER]${NC} Unity konnte nicht gefunden werden!"
    echo ""
    echo "Bitte installiere Unity 2022.3 LTS ueber Unity Hub"
    echo "oder setze den Pfad manuell in diesem Script:"
    echo ""
    if [[ "$OSTYPE" == "darwin"* ]]; then
        echo "  UNITY_PATH=\"/Applications/Unity/Hub/Editor/2022.3.XX/Unity.app/Contents/MacOS/Unity\""
    else
        echo "  UNITY_PATH=\"/home/$USER/Unity/Hub/Editor/2022.3.XX/Editor/Unity\""
    fi
    echo ""
    exit 1
fi

echo ""

# ============================================================
# Build-Konfiguration
# ============================================================

# Desktop-Pfad ermitteln
if [[ "$OSTYPE" == "darwin"* ]]; then
    DESKTOP_PATH="$HOME/Desktop"
elif [[ "$XDG_DESKTOP_DIR" ]]; then
    DESKTOP_PATH="$XDG_DESKTOP_DIR"
else
    DESKTOP_PATH="$HOME/Desktop"
fi

# Falls Desktop nicht existiert, nutze Home-Verzeichnis
if [[ ! -d "$DESKTOP_PATH" ]]; then
    DESKTOP_PATH="$HOME"
    echo -e "${YELLOW}[WARNUNG]${NC} Desktop-Verzeichnis nicht gefunden. APK wird in $DESKTOP_PATH gespeichert."
fi

# APK-Dateiname mit Datum/Zeit
TIMESTAMP=$(date +%Y%m%d_%H%M)
APK_NAME="D8-Planer-XR_${TIMESTAMP}.apk"
APK_PATH="$DESKTOP_PATH/$APK_NAME"

# Build-Log Pfad
BUILD_LOG="$PROJECT_PATH/Logs/build.log"

echo -e "${YELLOW}[INFO]${NC} APK wird exportiert nach: $APK_PATH"
echo ""

# ============================================================
# Unity Build starten
# ============================================================

echo -e "${YELLOW}[INFO]${NC} Starte Unity Build-Prozess..."
echo -e "${YELLOW}[INFO]${NC} Dies kann einige Minuten dauern. Bitte warten..."
echo ""

# Logs-Verzeichnis erstellen falls nicht vorhanden
mkdir -p "$PROJECT_PATH/Logs"

# Unity im Batch-Modus starten
BUILD_RESULT=0
"$UNITY_PATH" \
    -quit \
    -batchmode \
    -nographics \
    -projectPath "$PROJECT_PATH" \
    -executeMethod D8PlanerXR.Editor.CommandLineBuild.BuildAndroid \
    -buildOutput "$APK_PATH" \
    -logFile "$BUILD_LOG" || BUILD_RESULT=$?

echo ""

if [[ $BUILD_RESULT -eq 0 ]] && [[ -f "$APK_PATH" ]]; then
    echo -e "${GREEN}============================================================${NC}"
    echo -e "${GREEN}   BUILD ERFOLGREICH!${NC}"
    echo -e "${GREEN}============================================================${NC}"
    echo ""
    echo -e "${GREEN}[OK]${NC} APK wurde erstellt: $APK_PATH"
    echo ""
    
    # Dateigroesse anzeigen
    if [[ "$OSTYPE" == "darwin"* ]]; then
        SIZE=$(stat -f%z "$APK_PATH" 2>/dev/null || echo "0")
    else
        SIZE=$(stat -c%s "$APK_PATH" 2>/dev/null || echo "0")
    fi
    SIZE_MB=$((SIZE / 1048576))
    echo -e "${YELLOW}[INFO]${NC} Dateigroesse: ${SIZE_MB} MB"
    echo ""
    
    echo "Naechste Schritte:"
    echo "  1. APK auf Android-Geraet kopieren"
    echo "  2. APK installieren: adb install \"$APK_PATH\""
    echo "  3. App testen"
    echo ""
else
    echo -e "${RED}============================================================${NC}"
    echo -e "${RED}   BUILD FEHLGESCHLAGEN!${NC}"
    echo -e "${RED}============================================================${NC}"
    echo ""
    echo -e "${RED}[FEHLER]${NC} Build-Prozess hat Fehler zurueckgegeben: $BUILD_RESULT"
    echo ""
    echo "Pruefe das Build-Log fuer Details:"
    echo "  $BUILD_LOG"
    echo ""
    echo "Haeufige Fehlerursachen:"
    echo "  - Unity wurde noch nie mit diesem Projekt geoeffnet"
    echo "  - Android Build Support fehlt"
    echo "  - Setup-Wizard wurde nicht ausgefuehrt"
    echo "  - XR Plugin Management nicht aktiviert"
    echo ""
    echo "Loesungsschritte:"
    echo "  1. Oeffne das Projekt in Unity"
    echo "  2. Warte bis alle Assets importiert sind"
    echo "  3. Fuehre den Setup-Wizard aus: D8-Planer > Alle Einstellungen anwenden"
    echo "  4. Aktiviere ARCore: Edit > Project Settings > XR Plug-in Management"
    echo "  5. Fuehre dieses Script erneut aus"
    echo ""
    exit $BUILD_RESULT
fi
