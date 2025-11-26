# APK Build Anleitung - D8-Planer XR

> **Vollständige Anleitung zum Erstellen einer installierbaren APK-Datei für Android-Geräte**

## 📋 Inhaltsverzeichnis

1. [Voraussetzungen](#voraussetzungen)
2. [Unity-Projekt vorbereiten](#unity-projekt-vorbereiten)
3. [Android Build Settings konfigurieren](#android-build-settings-konfigurieren)
4. [Player Settings anpassen](#player-settings-anpassen)
5. [APK erstellen](#apk-erstellen)
6. [APK auf Gerät installieren](#apk-auf-gerät-installieren)
7. [Fehlerbehebung](#fehlerbehebung)
8. [Handy-Modus vs. VR-Modus](#handy-modus-vs-vr-modus)

---

## 1. Voraussetzungen

### Software-Anforderungen

- **Unity Hub** (neueste Version)
  - Download: https://unity.com/download
  
- **Unity Editor 2022.3 LTS oder höher**
  - Bei Installation folgende Module auswählen:
    - ✅ Android Build Support
    - ✅ Android SDK & NDK Tools
    - ✅ OpenJDK
  
- **Android SDK** (wird meist mit Unity installiert)
  - Mindest-API Level: 24 (Android 7.0)
  - Empfohlenes Target API Level: 33 (Android 13)

### Hardware-Anforderungen

- **Entwicklungsrechner:**
  - Windows 10/11, macOS 10.15+, oder Linux
  - Mindestens 8 GB RAM (16 GB empfohlen)
  - 10 GB freier Speicherplatz
  
- **Android-Gerät:**
  - Android 7.0 (API Level 24) oder höher
  - ARCore-Unterstützung (für AR-Features)
  - USB-Debugging aktiviert (für direkte Installation)

---

## 2. Unity-Projekt vorbereiten

### Schritt 1: Projekt öffnen

1. **Unity Hub** starten
2. Auf **"Add"** oder **"Öffnen"** klicken
3. Projektordner auswählen: `3DoF_Deckzentrum`
4. Unity Version **2022.3 LTS** auswählen
5. Projekt wird geladen (kann einige Minuten dauern)

### Schritt 2: Pakete überprüfen

1. Im Unity Editor: **Window** → **Package Manager** öffnen
2. Folgende Pakete müssen installiert sein:
   - ✅ AR Foundation (v5.1.0 oder höher)
   - ✅ ARCore XR Plugin (v5.1.0 oder höher)
   - ✅ TextMeshPro (v3.0.6 oder höher)
   - ✅ UI (ugui)

3. Falls ein Paket fehlt:
   - Auf **"+"** (oben links) klicken
   - **"Add package by name"** wählen
   - Paketnamen eingeben (z.B. `com.unity.xr.arfoundation`)
   - **"Add"** klicken

### Schritt 3: Szene vorbereiten

1. Hauptszene öffnen:
   - **Assets/Scenes/** im Project-Fenster
   - Hauptszene doppelklicken (z.B. `MainScene.unity`)

2. Überprüfen, dass folgende GameObjects vorhanden sind:
   - `AppController`
   - `AR Session`
   - `AR Session Origin` oder `XR Origin`
   - `AR Camera`
   - `QRCodeTracker`
   - `VirtualDeckzentrum`

---

## 3. Android Build Settings konfigurieren

### Schritt 1: Build Settings öffnen

1. Im Unity Editor: **File** → **Build Settings** (oder `Ctrl+Shift+B` / `Cmd+Shift+B`)
2. Dialog "Build Settings" öffnet sich

### Schritt 2: Platform wechseln

1. In der Platform-Liste **"Android"** auswählen
2. Auf **"Switch Platform"** klicken
   - ⚠️ Dies kann 10-20 Minuten dauern beim ersten Mal
   - Unity konvertiert alle Assets für Android
3. Warten bis "Android" als aktuelle Platform angezeigt wird

### Schritt 3: Szenen hinzufügen

1. Im Bereich "Scenes in Build":
2. Auf **"Add Open Scenes"** klicken
3. Oder Szenen aus dem Project-Fenster per Drag & Drop hinzufügen
4. Hauptszene sollte Index 0 haben (wird als erstes geladen)

### Schritt 4: Build Settings konfigurieren

- ✅ **Compression Method:** LZ4 (schneller) oder Default
- ✅ **Development Build:** Nur für Testing aktivieren
- ✅ **Autoconnect Profiler:** Nur für Debugging

---

## 4. Player Settings anpassen

### Schritt 1: Player Settings öffnen

1. Im Build Settings Dialog: **"Player Settings..."** klicken
2. Oder: **Edit** → **Project Settings** → **Player**

### Schritt 2: Company & Product Name

**Settings → Player → Company Name / Product Name**

```
Company Name: [Ihre Firma]
Product Name: D8-Planer XR
```

### Schritt 3: Icon & Splash Screen

**Settings → Player → Icon**

1. **Default Icon** setzen (quadratisches Logo, mindestens 1024x1024 px)
2. **Adaptive Icon** (optional, für moderne Android-Versionen)
   - Foreground: Ihr Logo (transparenter Hintergrund)
   - Background: Einfarbiger oder gemusterter Hintergrund

**Settings → Player → Splash Image**

- Splash Screen aktivieren/deaktivieren
- Unity Logo ausblenden: `Show Unity Logo` deaktivieren (Pro-Lizenz erforderlich)

### Schritt 4: Android-spezifische Einstellungen

**Settings → Player → Android Tab → Other Settings**

#### Package Name (wichtig!)
```
com.IhreFirma.D8PlanerXR
```
- Format: `com.firmenname.appname`
- Nur Kleinbuchstaben, Zahlen, Punkte
- Keine Sonderzeichen oder Umlaute
- **Muss eindeutig sein** (später nicht mehr änderbar!)

#### Version
```
Version: 1.0.0
Bundle Version Code: 1
```
- Bei jedem Update erhöhen!

#### Minimum API Level
```
Minimum API Level: Android 7.0 'Nougat' (API Level 24)
Target API Level: Android 13 (API Level 33) oder höher
```

#### Scripting Backend
```
Scripting Backend: IL2CPP (empfohlen für bessere Performance)
```

#### Target Architectures
✅ **ARMv7** (für ältere Geräte)  
✅ **ARM64** (für moderne Geräte, ab 2019 Pflicht für Google Play)

#### Other Settings (wichtig für AR!)

**Graphics APIs:**
- ✅ OpenGLES3
- ✅ Vulkan (optional, für neuere Geräte)
- ⚠️ OpenGLES2 entfernen (wird für AR nicht benötigt)

**Configuration:**
```
Install Location: Automatic
Internet Access: Require (für potentielle Updates)
Write Permission: External (SD Card) - für CSV-Import
```

### Schritt 5: XR Plugin Management

**Settings → XR Plug-in Management → Android Tab**

1. Falls nicht vorhanden: **"Install XR Plugin Management"** klicken
2. ✅ **ARCore** aktivieren
3. ARCore-Einstellungen prüfen:
   - Depth: Optional
   - Plane Detection: Required
   - Face Tracking: None

### Schritt 6: Quality Settings (optional)

**Settings → Quality**

Für bessere Performance auf Mobilgeräten:
- **Texture Quality:** Full Res
- **Anti Aliasing:** 2x Multi Sampling
- **Shadows:** Soft Shadows (Medium Distance)
- **VSync:** Don't Sync (für AR empfohlen)

---

## 5. APK erstellen

### Methode 1: Build APK (empfohlen für Testing)

1. **File** → **Build Settings**
2. Alle obigen Einstellungen prüfen
3. Auf **"Build"** klicken
4. Speicherort wählen (z.B. `Builds/Android/`)
5. Dateinamen eingeben (z.B. `D8-Planer-XR-v1.0.apk`)
6. **"Save"** klicken
7. Build-Prozess startet (kann 10-30 Minuten dauern)
8. Bei Erfolg: APK-Datei im gewählten Ordner

### Methode 2: Build and Run (direkt auf Gerät)

**Voraussetzungen:**
- Android-Gerät per USB verbunden
- USB-Debugging aktiviert
- Gerät in "adb devices" sichtbar

**Schritte:**
1. **File** → **Build Settings**
2. Gerät auswählen (falls mehrere verbunden)
3. Auf **"Build and Run"** klicken
4. APK wird gebaut UND automatisch installiert
5. App startet automatisch auf dem Gerät

### Methode 3: App Bundle (für Google Play Store)

**⚠️ Nur für Veröffentlichung im Play Store notwendig!**

1. **File** → **Build Settings**
2. ✅ **"Build App Bundle (Google Play)"** aktivieren
3. **"Build"** klicken
4. `.aab` Datei wird erstellt (Android App Bundle)
5. Diese Datei im Play Store Developer Console hochladen

---

## 6. APK auf Gerät installieren

### Methode 1: USB-Installation (adb)

**Voraussetzungen:**
- Android SDK Platform Tools installiert
- USB-Debugging am Gerät aktiviert

**Schritte:**

1. **Android-Gerät vorbereiten:**
   - **Einstellungen** → **Über das Telefon** → 7x auf **"Build-Nummer"** tippen
   - **Entwickleroptionen** aktiviert
   - **USB-Debugging** aktivieren
   - **Installation aus unbekannten Quellen** erlauben

2. **Gerät per USB verbinden**

3. **Terminal/Kommandozeile öffnen:**

   **Windows:**
   ```cmd
   cd C:\Users\[IhrName]\AppData\Local\Android\Sdk\platform-tools
   adb devices
   ```

   **macOS/Linux:**
   ```bash
   adb devices
   ```

4. **APK installieren:**
   ```bash
   adb install -r "Pfad/zur/D8-Planer-XR-v1.0.apk"
   ```
   - `-r` überschreibt vorhandene Installation

5. **Installation prüfen:**
   - App sollte im App-Drawer erscheinen

### Methode 2: Direkte Installation (ohne PC)

1. **APK auf Gerät übertragen:**
   - Per E-Mail
   - Cloud-Dienst (Google Drive, Dropbox)
   - Direkter Download
   - USB-Stick mit OTG-Adapter

2. **Installation auf Android-Gerät:**
   - APK-Datei im Dateimanager öffnen
   - **"Installieren"** tippen
   - Warnung bestätigen ("Aus unbekannten Quellen")
   - Installation abschließen

3. **Sicherheitshinweis bestätigen**
   - Android warnt bei Apps außerhalb des Play Stores
   - **"Trotzdem installieren"** wählen

### Methode 3: Viture Neckband Pro (speziell)

**Falls das Viture Neckband ein vollwertiges Android-Gerät ist:**

1. **Viture per USB mit PC verbinden**
2. **adb devices** ausführen → Viture sollte erscheinen
3. **adb install** wie oben
   
**Alternative:**
- APK auf SD-Karte/USB-Stick
- In Viture File Manager öffnen
- Installieren

---

## 7. Fehlerbehebung

### Problem: "Build failed" - Unity kann nicht bauen

**Lösung 1: Android SDK prüfen**
```
Edit → Preferences → External Tools
```
- Prüfen ob Android SDK/NDK Pfade korrekt sind
- Ggf. neu herunterladen via Unity Hub

**Lösung 2: Gradle-Cache löschen**
```
Windows: C:\Users\[Name]\.gradle\caches
macOS/Linux: ~/.gradle/caches
```
- Ordner "caches" löschen
- Unity neu starten

**Lösung 3: Reimport aller Assets**
```
Assets → Reimport All
```
- Kann 10-30 Minuten dauern

### Problem: "Minimum API level error"

**Fehler:**
```
Minimum supported Gradle version is X.X.X
```

**Lösung:**
```
Edit → Project Settings → Player → Android → Other Settings
Minimum API Level: Android 7.0 (API Level 24) oder höher
```

### Problem: APK installiert nicht auf Gerät

**Lösung 1: Signatur-Konflikt**
- Alte Version deinstallieren
- Neu installieren

**Lösung 2: Nicht genug Speicher**
- Speicherplatz auf Gerät prüfen
- Mindestens 500 MB frei

**Lösung 3: APK beschädigt**
- APK erneut von PC übertragen
- Hash-Wert prüfen

### Problem: App stürzt beim Start ab

**Lösung 1: Logs abrufen**
```bash
adb logcat -s Unity
```
- Fehlermeldungen analysieren

**Lösung 2: Berechtigungen prüfen**
- Kamera-Berechtigung in Android-Einstellungen aktivieren
- Speicher-Berechtigung aktivieren

**Lösung 3: ARCore**
- Google Play Store öffnen
- "ARCore" suchen und aktualisieren
- Falls nicht installiert → installieren

### Problem: AR funktioniert nicht

**Prüfungen:**
1. ✅ Gerät ist ARCore-kompatibel
   - Liste: https://developers.google.com/ar/devices
   
2. ✅ ARCore ist installiert und aktuell
   - Google Play Store → "ARCore"
   
3. ✅ Kamera-Berechtigung erteilt
   - Android Einstellungen → Apps → D8-Planer XR → Berechtigungen

4. ✅ Build Settings korrekt:
   - XR Plugin Management → ARCore aktiviert
   - Minimum API Level 24+
   - ARMv7 + ARM64 aktiviert

### Problem: CSV-Datei wird nicht gefunden

**Lösung:**

Die App sucht CSV-Dateien in:
```
Android/data/com.IhreFirma.D8PlanerXR/files/
```

**Schritte:**
1. Dateimanager auf Android öffnen
2. Zu "Interner Speicher" navigieren
3. Ordner `Android/data/com.IhreFirma.D8PlanerXR/files/` erstellen
4. `ImportDZ.csv` dort ablegen

**Alternative via adb:**
```bash
adb push ImportDZ.csv /sdcard/Android/data/com.IhreFirma.D8PlanerXR/files/
```

---

## 8. Handy-Modus vs. VR-Modus

Ab Version 1.1 unterstützt die App zwei Modi:

### 🕶️ VR-Modus (Viture-Brille)
- Für Viture Neckband Pro + Luma Ultra
- Volle AR-Funktionalität
- Kopfbewegung-Tracking
- Spatial Anchoring

### 📱 Handy-Modus (Smartphone)
- Für normale Android-Smartphones
- Nur Kamera-basiertes AR
- QR-Code Scanning
- Overlay-Anzeige

### Modus wechseln

**In der App:**
1. App starten
2. Hauptmenü → **"Einstellungen"**
3. **"Anzeigemodus"** wählen:
   - 🕶️ VR-Modus
   - 📱 Handy-Modus
4. App neu starten

**Automatische Erkennung:**
- App erkennt automatisch ob Viture-Hardware verbunden ist
- Falls nicht: Handy-Modus wird verwendet

### Unterschiede in den Modi

| Feature | VR-Modus | Handy-Modus |
|---------|----------|-------------|
| QR-Code Scanning | ✅ | ✅ |
| Overlay-Anzeige | ✅ | ✅ |
| Spatial Anchoring | ✅ | ⚠️ Eingeschränkt |
| Virtuelles Deckzentrum | ✅ | ❌ |
| Kopf-Tracking | ✅ | ❌ |
| Freihändige Bedienung | ✅ | ❌ |
| Touch-Steuerung | ❌ | ✅ |

---

## 9. Erweiterte Build-Optionen

### Code-Optimierung für Release

**Settings → Player → Other Settings → Optimization:**

```
Stripping Level: High (reduziert APK-Größe)
Script Compilation: Release
IL2CPP Code Generation: Faster runtime
Managed Stripping Level: High
```

⚠️ **Achtung:** Hohe Stripping-Levels können Reflection-basierten Code brechen!

### Keystores für Signierung

**Für Veröffentlichung (Google Play, Enterprise):**

1. **Keystore erstellen:**
   ```
   Player Settings → Publishing Settings → Keystore Manager
   → Create New Keystore
   ```

2. **Keystore-Daten eingeben:**
   - Passwort (gut merken/sichern!)
   - Alias-Name
   - Passwort für Alias
   - Gültigkeit: 25+ Jahre

3. **Keystore sichern:**
   - `.keystore` Datei backup erstellen
   - Passwörter sicher aufbewahren
   - **Niemals verlieren!** (Kann nicht wiederhergestellt werden)

### Multi-APK für verschiedene Architekturen

**Für Google Play:**

Separate APKs für ARM64 und ARMv7:

1. **Build Settings:**
   ```
   ✅ Split APKs by target architecture
   ```

2. **Resultat:**
   - `app-arm64-v8a-release.apk`
   - `app-armeabi-v7a-release.apk`
   - Google Play wählt automatisch passende Version

---

## 10. Checkliste vor Veröffentlichung

### Technisch
- ✅ Alle Features getestet
- ✅ Keine Konsolenfehler
- ✅ Performance gut (>30 FPS)
- ✅ APK-Größe unter 100 MB
- ✅ Auf 3+ Geräten getestet
- ✅ ARCore funktioniert
- ✅ CSV-Import funktioniert
- ✅ QR-Codes werden erkannt

### Konfiguration
- ✅ Version-Nummer erhöht
- ✅ Keystore erstellt und gesichert
- ✅ Package Name korrekt
- ✅ Icons gesetzt
- ✅ Berechtigungen minimal
- ✅ Target API Level aktuell

### Dokumentation
- ✅ Benutzerhandbuch erstellt
- ✅ CSV-Format dokumentiert
- ✅ QR-Codes generiert und gedruckt
- ✅ Support-Kontakt angegeben

---

## 11. Nützliche Links

- **Unity Android Build:** https://docs.unity3d.com/Manual/android-BuildProcess.html
- **AR Foundation:** https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@5.1/manual/
- **ARCore Supported Devices:** https://developers.google.com/ar/devices
- **Android Developer Docs:** https://developer.android.com/studio/build/building-cmdline

---

## 12. Support & Kontakt

Bei Problemen:
1. Diese Anleitung vollständig durchlesen
2. Fehlermeldungen notieren
3. GitHub Issues erstellen
4. Support kontaktieren

**Viel Erfolg beim Bauen Ihrer APK!** 🚀

---

*Version: 1.1 | Letzte Aktualisierung: 2025-11-26*
