# Unity Setup Anleitung - D8-Planer XR

> **Schnellanleitung zur Einrichtung von Unity mit den Projekt-Voreinstellungen**

---

## 📋 Inhaltsverzeichnis

1. [Voraussetzungen](#1-voraussetzungen)
2. [Unity Installation](#2-unity-installation)
3. [Projekt öffnen](#3-projekt-öffnen)
4. [Setup-Wizard ausführen](#4-setup-wizard-ausführen)
5. [Manuelle Schritte](#5-manuelle-schritte)
6. [Automatischer APK-Build](#6-automatischer-apk-build)
7. [Fehlerbehebung](#7-fehlerbehebung)

---

## 1. Voraussetzungen

### Software

| Software | Version | Download |
|----------|---------|----------|
| Unity Hub | Neueste | https://unity.com/download |
| Unity Editor | 2022.3 LTS | Via Unity Hub |
| Android SDK | API 24+ | Via Unity Installation |
| Git | Neueste | https://git-scm.com/ |

### Bei Unity Installation auswählen

✅ **Android Build Support**
- ✅ Android SDK & NDK Tools
- ✅ OpenJDK

---

## 2. Unity Installation

### Schritt 1: Unity Hub installieren

1. Lade Unity Hub von https://unity.com/download
2. Installiere Unity Hub
3. Melde dich mit deinem Unity-Konto an (kostenlos)

### Schritt 2: Unity Editor installieren

1. Öffne Unity Hub
2. Gehe zu **Installs** → **Install Editor**
3. Wähle **Unity 2022.3 LTS** (empfohlen)
4. **WICHTIG:** Aktiviere folgende Module:
   - ✅ **Android Build Support**
   - ✅ **Android SDK & NDK Tools**  
   - ✅ **OpenJDK**
5. Klicke **Install** und warte (~10-15 Minuten)

### Schritt 3: Android SDK Pfad prüfen

Nach der Installation:
1. Unity Hub → **Preferences** → **External Tools**
2. Prüfe, dass Android SDK, NDK und JDK Pfade korrekt sind
3. Falls leer: Klicke **Browse** und wähle die installierten Pfade

---

## 3. Projekt öffnen

### Schritt 1: Repository klonen (falls noch nicht geschehen)

```bash
git clone https://github.com/Bold202/3DoF_Deckzentrum.git
cd 3DoF_Deckzentrum
```

### Schritt 2: Projekt in Unity Hub hinzufügen

1. Öffne **Unity Hub**
2. Klicke **Add** → **Add project from disk**
3. Navigiere zum Projektordner `3DoF_Deckzentrum`
4. Klicke **Add Project**

### Schritt 3: Projekt öffnen

1. Klicke auf das Projekt in der Liste
2. Wähle **Unity 2022.3 LTS** als Editor-Version
3. Klicke **Open**
4. ⏳ **Bitte warten** - Der erste Import dauert 5-15 Minuten!

---

## 4. Setup-Wizard ausführen

### Der automatische Weg (empfohlen)

Das Projekt enthält einen **Setup-Wizard**, der alle Einstellungen automatisch konfiguriert:

1. Im Unity Editor: **D8-Planer** → **Setup Wizard**
2. Klicke **"Alle Einstellungen anwenden"**
3. Bestätige den Dialog mit **"Ja, anwenden"**
4. Warte bis alle Einstellungen übernommen wurden

### Was der Setup-Wizard konfiguriert

| Einstellung | Wert |
|-------------|------|
| Build Target | Android |
| Package Name | com.d8planer.deckzentrum |
| Min API Level | Android 7.0 (API 24) |
| Target API Level | Android 13 (API 33) |
| Scripting Backend | IL2CPP |
| Target Architectures | ARM64 + ARMv7 |
| Graphics APIs | OpenGLES3, Vulkan |
| Texture Compression | ASTC |
| ARCore | Aktiviert |

### Hauptszene erstellen

Nach dem Setup-Wizard:
1. **D8-Planer** → **Hauptszene erstellen**
2. Die Szene wird unter `Assets/Scenes/MainScene.unity` erstellt
3. Sie enthält alle benötigten GameObjects:
   - AR Session
   - XR Origin mit AR Camera
   - AppController
   - DeviceModeManager
   - UI Canvas

---

## 5. Manuelle Schritte

### 5.1 XR Plugin Management aktivieren

Der Setup-Wizard kann XR Plugin Management nicht vollautomatisch aktivieren. Dies muss manuell erfolgen:

1. **Edit** → **Project Settings** → **XR Plug-in Management**
2. Wähle den **Android** Tab (Android-Symbol)
3. Aktiviere ✅ **ARCore**
4. Aktiviere ✅ **Initialize XR on Startup**
5. Schließe die Project Settings

### 5.2 Pakete prüfen

Falls Fehlermeldungen erscheinen:

1. **Window** → **Package Manager**
2. Prüfe, dass diese Pakete installiert sind:
   - ✅ AR Foundation (5.1.0+)
   - ✅ ARCore XR Plugin (5.1.0+)
   - ✅ TextMeshPro (3.0.6+)
3. Falls fehlt: **+** → **Add package by name** → Paketnamen eingeben

### 5.3 Szene zu Build Settings hinzufügen

1. Öffne die Hauptszene: **Assets/Scenes/MainScene.unity**
2. **File** → **Build Settings**
3. Klicke **Add Open Scenes**
4. Stelle sicher, dass die Szene Index 0 hat

---

## 6. Automatischer APK-Build

### 🚀 Ein-Klick-Build (Desktop-Script)

Das Projekt enthält Build-Scripts für automatisierten APK-Export:

#### Windows

1. Doppelklicke auf `build_apk.bat` im Projektordner
2. Das Script:
   - Findet Unity automatisch
   - Kompiliert das Projekt
   - Exportiert die APK auf deinen Desktop
3. **Fertig!** APK liegt auf deinem Desktop

#### macOS / Linux

1. Öffne Terminal im Projektordner
2. Führe aus:
   ```bash
   chmod +x build_apk.sh
   ./build_apk.sh
   ```
3. APK wird auf deinen Desktop exportiert

### Manuelle APK-Build-Optionen

Falls das Script nicht funktioniert:

1. **File** → **Build Settings**
2. Wähle **Android**
3. Klicke **Build**
4. Wähle Desktop als Speicherort
5. Name: `D8-Planer-XR.apk`

---

## 7. Fehlerbehebung

### Problem: "Android SDK not found"

**Lösung:**
1. Unity Hub → **Preferences** → **External Tools**
2. Prüfe Android SDK Pfad
3. Falls leer: Installiere Android Build Support über Unity Hub

### Problem: "Gradle build failed"

**Lösung:**
```bash
# Windows
del /s /q %USERPROFILE%\.gradle\caches

# macOS/Linux  
rm -rf ~/.gradle/caches
```
Dann Unity neustarten.

### Problem: "Package not found" Fehler

**Lösung:**
1. **Window** → **Package Manager**
2. Lösche Package Cache: **+** → **Clear Cache**
3. Unity neustarten

### Problem: "XR Plugin Management not found"

**Lösung:**
1. **Window** → **Package Manager**
2. Suche nach **XR Plugin Management**
3. Installiere das Paket
4. Unity neustarten
5. Dann: **Edit** → **Project Settings** → **XR Plug-in Management**

### Problem: ARCore nicht erkannt

**Lösung:**
1. Prüfe ob ARCore-Paket installiert ist
2. **Edit** → **Project Settings** → **XR Plug-in Management** → **Android**
3. Aktiviere **ARCore**
4. Falls grau: Wechsle zuerst zu Android Platform (**File** → **Build Settings** → **Android** → **Switch Platform**)

### Problem: Build-Script findet Unity nicht

**Lösung (Windows):**
Editiere `build_apk.bat` und setze den Unity-Pfad manuell:
```batch
set UNITY_PATH=C:\Program Files\Unity\Hub\Editor\2022.3.10f1\Editor\Unity.exe
```

**Lösung (macOS/Linux):**
Editiere `build_apk.sh` und setze den Unity-Pfad:
```bash
UNITY_PATH="/Applications/Unity/Hub/Editor/2022.3.10f1/Unity.app/Contents/MacOS/Unity"
```

---

## 📊 Checkliste nach Setup

Prüfe vor dem ersten Build:

- [ ] Unity 2022.3 LTS installiert mit Android Support
- [ ] Projekt erfolgreich geöffnet (keine roten Fehler in Console)
- [ ] Setup-Wizard ausgeführt: **D8-Planer** → **Alle Einstellungen anwenden**
- [ ] XR Plugin Management → ARCore aktiviert
- [ ] Hauptszene erstellt: **D8-Planer** → **Hauptszene erstellen**
- [ ] Szene in Build Settings hinzugefügt
- [ ] Test-Build erfolgreich (keine Build-Fehler)

---

## 🎯 Nächste Schritte

Nach erfolgreichem Setup:

1. **APK auf Gerät installieren:**
   ```bash
   adb install D8-Planer-XR.apk
   ```

2. **CSV-Testdaten kopieren:**
   - Kopiere `Import/MusterPlan.csv` auf das Gerät
   - Pfad: `/sdcard/Android/data/com.d8planer.deckzentrum/files/`

3. **QR-Codes generieren:**
   ```bash
   cd Tools
   pip install qrcode[pil] pillow
   python qr_generator_batch.py
   ```

4. **App testen:**
   - App starten
   - QR-Codes scannen
   - Overlays prüfen

---

## 📞 Support

Bei Problemen:
1. Prüfe diese Anleitung
2. Lese `APK_BUILD_ANLEITUNG.md` für detaillierte Build-Infos
3. Lese `IMPLEMENTATION_GUIDE.md` für technische Details
4. Erstelle ein GitHub Issue

---

*Version: 1.0 | Letzte Aktualisierung: 2025-11-27*
