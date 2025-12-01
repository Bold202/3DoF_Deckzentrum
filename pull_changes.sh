#!/bin/bash
# ============================================================
# D8-Planer XR - Git Pull Script (macOS/Linux)
# ============================================================
#
# Repository: https://github.com/Bold202/3DoF_Deckzentrum.git
#
# Dieses Script holt die neuesten Aenderungen aus dem
# Git-Repository ins lokale Verzeichnis.
#
# Verwendung:
#   ./pull_changes.sh
#
# Voraussetzungen:
#   - Git muss installiert sein
#   - Das Script liegt im Projektverzeichnis
#
# Falls noch nicht geklont:
#   git clone https://github.com/Bold202/3DoF_Deckzentrum.git
#
# ============================================================

set -e

echo "============================================================"
echo "   D8-Planer XR - Git Pull"
echo "============================================================"
echo ""

# ============================================================
# Projektverzeichnis ermitteln
# ============================================================

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

echo "[INFO] Projektverzeichnis: $SCRIPT_DIR"
echo ""

# ============================================================
# Git pruefen
# ============================================================

if ! command -v git &> /dev/null; then
    echo ""
    echo "[FEHLER] Git konnte nicht gefunden werden!"
    echo ""
    echo "Bitte installiere Git:"
    echo "  macOS: brew install git"
    echo "  Linux: sudo apt install git"
    echo ""
    exit 1
fi

echo "[OK] Git gefunden."
echo ""

# ============================================================
# Ins Projektverzeichnis wechseln
# ============================================================

cd "$SCRIPT_DIR"

# ============================================================
# Repository URL
# ============================================================

REPO_URL="https://github.com/Bold202/3DoF_Deckzentrum.git"

# ============================================================
# Pruefen ob es ein Git-Repository ist
# ============================================================

if [ ! -d ".git" ]; then
    echo ""
    echo "[INFO] Kein Git-Repository gefunden!"
    echo ""
    echo "Es sieht so aus, als haettest du das Repository als ZIP"
    echo "heruntergeladen statt es zu klonen."
    echo ""
    echo "Das Script kann das Repository jetzt initialisieren."
    echo ""
    read -p "Git-Repository initialisieren? (j/n): " INIT_REPO
    if [ "$INIT_REPO" != "j" ] && [ "$INIT_REPO" != "J" ]; then
        echo ""
        echo "[INFO] Alternativ kannst du das Repository neu klonen:"
        echo ""
        echo "  git clone $REPO_URL"
        echo ""
        exit 1
    fi
    
    echo ""
    echo "[INFO] Initialisiere Git-Repository..."
    if ! git init; then
        echo "[FEHLER] Git init fehlgeschlagen!"
        exit 1
    fi
    
    echo "[INFO] Fuege Remote-Repository hinzu..."
    if ! git remote add origin "$REPO_URL"; then
        echo "[FEHLER] Git remote add fehlgeschlagen!"
        exit 1
    fi
    
    echo "[INFO] Hole Repository-Daten..."
    if ! git fetch origin; then
        echo "[FEHLER] Git fetch fehlgeschlagen!"
        exit 1
    fi
    
    echo "[INFO] Setze Branch auf main..."
    if ! git reset --mixed origin/main; then
        echo "[FEHLER] Git reset fehlgeschlagen!"
        exit 1
    fi
    
    if ! git branch -M main; then
        echo "[FEHLER] Git branch -M fehlgeschlagen!"
        exit 1
    fi
    
    if ! git branch --set-upstream-to=origin/main main; then
        echo "[FEHLER] Git branch --set-upstream-to fehlgeschlagen!"
        exit 1
    fi
    
    echo ""
    echo "[OK] Git-Repository erfolgreich initialisiert!"
    echo ""
fi

echo "[OK] Git-Repository erkannt."
echo ""

# ============================================================
# Aktuellen Branch anzeigen
# ============================================================

echo "[INFO] Aktueller Branch:"
CURRENT_BRANCH=$(git branch --show-current 2>/dev/null || echo "unbekannt")
echo "$CURRENT_BRANCH"
echo ""

# ============================================================
# Status vor Pull anzeigen
# ============================================================

echo "[INFO] Pruefe lokale Aenderungen..."
git status --short 2>/dev/null || true

# Warnung bei lokalen Aenderungen
LOCAL_CHANGES=$(git status --porcelain 2>/dev/null || echo "")
if [ -n "$LOCAL_CHANGES" ]; then
    echo ""
    echo "[WARNUNG] Es gibt lokale Aenderungen im Repository."
    echo "          Diese koennten beim Pull zu Konflikten fuehren."
    echo ""
    read -p "Trotzdem fortfahren? (j/n): " CONTINUE
    if [ "$CONTINUE" != "j" ] && [ "$CONTINUE" != "J" ]; then
        echo ""
        echo "[ABBRUCH] Pull wurde abgebrochen."
        echo ""
        exit 0
    fi
fi

echo ""

# ============================================================
# Git Pull ausfuehren
# ============================================================

echo "[INFO] Hole neueste Aenderungen vom Remote-Repository..."
echo ""

if git pull; then
    echo ""
    echo "============================================================"
    echo "   ERFOLGREICH!"
    echo "============================================================"
    echo ""
    echo "[OK] Aenderungen wurden erfolgreich geholt."
    echo ""
    
    # Zeige letzte Commits
    echo "[INFO] Letzte 3 Commits:"
    echo ""
    git --no-pager log --oneline -3
    echo ""
    
    # ============================================================
    # Unity automatisch starten
    # ============================================================
    
    echo "[INFO] Starte Unity mit dem Projekt..."
    echo ""
    
    # Unity-Pfad suchen
    UNITY_PATH=""
    
    # macOS Unity Hub Pfade
    if [[ "$OSTYPE" == "darwin"* ]]; then
        for version_dir in /Applications/Unity/Hub/Editor/2022.3*; do
            if [[ -d "$version_dir" ]]; then
                unity_exe="$version_dir/Unity.app/Contents/MacOS/Unity"
                if [[ -f "$unity_exe" ]]; then
                    UNITY_PATH="$unity_exe"
                    break
                fi
            fi
        done
    fi
    
    # Linux Unity Hub Pfade
    if [[ "$OSTYPE" == "linux-gnu"* ]]; then
        for version_dir in "$HOME/Unity/Hub/Editor"/2022.3*; do
            if [[ -d "$version_dir" ]]; then
                unity_exe="$version_dir/Editor/Unity"
                if [[ -f "$unity_exe" ]]; then
                    UNITY_PATH="$unity_exe"
                    break
                fi
            fi
        done
    fi
    
    if [[ -n "$UNITY_PATH" ]]; then
        echo "[OK] Unity gefunden: $UNITY_PATH"
        echo "[INFO] Oeffne Unity-Projekt..."
        echo ""
        # Unity im Hintergrund starten, damit das Script beendet werden kann
        "$UNITY_PATH" -projectPath "$SCRIPT_DIR" &
        echo "[OK] Unity wird gestartet. Das Projekt wird geoeffnet."
        echo "     Du kannst dann auf 'Build' klicken wenn du moechtest."
    else
        echo "[INFO] Unity nicht automatisch gefunden."
        echo "       Oeffne das Projekt manuell in Unity Hub."
    fi
    echo ""
    
    exit 0
fi

echo ""
echo "============================================================"
echo "   FEHLER BEIM PULL!"
echo "============================================================"
echo ""
echo "[FEHLER] Git Pull hat Fehler zurueckgegeben."
echo ""
echo "Moegliche Ursachen:"
echo "  - Merge-Konflikte mit lokalen Aenderungen"
echo "  - Keine Netzwerkverbindung"
echo "  - Authentifizierungsproblem"
echo ""
echo "Loesungsvorschlaege:"
echo "  1. Sichere deine lokalen Aenderungen: git stash"
echo "  2. Fuehre den Pull erneut aus: ./pull_changes.sh"
echo "  3. Wende Aenderungen wieder an: git stash pop"
echo ""
exit 1
