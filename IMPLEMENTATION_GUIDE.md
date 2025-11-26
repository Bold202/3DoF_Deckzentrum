# D8-Planer XR - Implementierungsleitfaden

## Übersicht

Dieser Leitfaden beschreibt die vollständige Implementierung der D8-Planer XR Anwendung für das Viture Neckband Pro mit Viture Luma Ultra Brille.

## 📁 Projektstruktur

```
3DoF_Deckzentrum/
├── Assets/
│   ├── Scripts/
│   │   ├── Core/              # Kernfunktionalität
│   │   │   └── AppController.cs
│   │   ├── Data/              # Datenverwaltung
│   │   │   ├── CSVColumnConfig.cs
│   │   │   ├── CSVConfigManager.cs
│   │   │   └── DataRepository.cs
│   │   ├── AR/                # AR-Funktionalität
│   │   │   ├── QRCodeTracker.cs
│   │   │   └── VentilOverlay.cs
│   │   ├── UI/                # Benutzeroberfläche
│   │   │   └── CSVColumnMenuUI.cs
│   │   ├── QRCode/            # QR-Code Generierung
│   │   │   └── QRCodeGenerator.cs
│   │   └── Utils/             # Hilfsfunktionen
│   ├── Resources/             # Ressourcen
│   ├── Scenes/                # Unity Szenen
│   ├── Prefabs/               # Prefabs
│   └── Plugins/               # Externe Bibliotheken
├── ProjectSettings/           # Unity Projekt-Einstellungen
├── Packages/                  # Package Manager Pakete
├── FRAGENKATALOG.md          # Umfassender Fragenkatalog
└── README.md                 # Diese Datei
```

## 🎯 Implementierte Features

### 1. ✅ CSV-Spalten-Konfigurationsmenü

**Dateien:**
- `Assets/Scripts/Data/CSVColumnConfig.cs`
- `Assets/Scripts/Data/CSVConfigManager.cs`
- `Assets/Scripts/UI/CSVColumnMenuUI.cs`

**Funktionen:**
- ✅ Spalten hinzufügen, entfernen, umbenennen
- ✅ Reihenfolge der Spalten ändern
- ✅ Spaltentypen festlegen (Text, Nummer, Datum, Boolean)
- ✅ Spaltenrollen zuweisen (Ventilnummer, Ohrmarkennummer, etc.)
- ✅ Sichtbarkeit von Spalten ein-/ausschalten
- ✅ Konfigurationsprofile speichern und laden
- ✅ Spalten aus CSV-Datei automatisch importieren
- ✅ CSV-Delimiter konfigurieren (;, ,, Tab)
- ✅ Encoding-Einstellungen (UTF-8, ISO-8859-1, Windows-1252)

### 2. ✅ Daten-Repository

**Dateien:**
- `Assets/Scripts/Data/DataRepository.cs`

**Funktionen:**
- ✅ CSV-Import mit konfigurierbaren Spalten
- ✅ Daten-Caching in HashMap (O(1) Zugriff)
- ✅ Ventil → Sauen Zuordnung
- ✅ Ohrmarkennummer → Sau Zuordnung
- ✅ Automatische Ampel-Berechnung (Grün/Gelb/Rot)
- ✅ Sortierung nach verschiedenen Kriterien
- ✅ Fehlertolerante CSV-Verarbeitung
- ✅ Statistiken (Anzahl Sauen, Ventile, etc.)

### 3. ✅ QR-Code System

**Dateien:**
- `Assets/Scripts/QRCode/QRCodeGenerator.cs`
- `Assets/Scripts/AR/QRCodeTracker.cs`

**Funktionen:**
- ✅ Generierung von QR-Codes für Ventile 1-199
- ✅ Speicherung als PNG/JPEG Dateien
- ✅ Konfigurierbare Fehlerkorrektur (L/M/Q/H)
- ✅ Optional: Menschenlesbare Nummer unter QR-Code
- ✅ QR-Code Erkennung via Kamera (AR)
- ✅ Mehrfach-QR-Code Erkennung
- ✅ Automatische Timeout-Verwaltung

### 4. ✅ AR-Overlay System

**Dateien:**
- `Assets/Scripts/AR/VentilOverlay.cs`

**Funktionen:**
- ✅ Virtuelle Overlays unter Ventilen
- ✅ Ampelsystem-Anzeige (Grün/Gelb/Rot)
- ✅ Liste aller Sauen pro Ventil
- ✅ Sortierung nach Priorität (Rot zuerst)
- ✅ Anzeige von Tagen seit Deckung
- ✅ Canvas orientiert sich zur Kamera
- ✅ Automatische Daten-Aktualisierung

### 5. ✅ Umfassender Fragenkatalog

**Datei:**
- `FRAGENKATALOG.md`

**Kategorien:**
- Hardware & Geräte (Viture, Bluetooth Printer)
- QR-Code System (Spezifikationen, Erkennung)
- CSV-Datei & Datenstruktur
- Ampelsystem & Trächtigkeit
- Benutzeroberfläche & UX
- Datenimport & Verwaltung
- Technische Details
- Qualitätssicherung & Tests
- Deployment & Wartung
- Zusätzliche Features
- Sicherheit
- Projektzeitplan

## 🛠 Erforderliche Unity-Pakete

### Über Package Manager installieren:

1. **AR Foundation** (com.unity.xr.arfoundation)
   ```
   Version: 5.x oder höher
   ```

2. **ARCore XR Plugin** (com.unity.xr.arcore)
   ```
   Version: 5.x oder höher
   ```

3. **TextMeshPro** (com.unity.textmeshpro)
   ```
   Version: 3.x oder höher
   ```

4. **ZXing.Net** (NuGet oder manuell)
   ```
   Für QR-Code Generierung/Erkennung
   Download: https://github.com/micjahn/ZXing.Net
   ```

### Manuell zu installierende Bibliotheken:

1. **ZXing.Unity.dll**
   - Download von GitHub: https://github.com/micjahn/ZXing.Net/releases
   - In `Assets/Plugins/` ablegen

2. **Viture SDK** (falls vorhanden)
   - Vom Hersteller beziehen
   - Dokumentation beachten

## 📋 Setup-Anleitung

### Schritt 1: Unity-Projekt erstellen

```bash
# Unity Version: 2022 LTS oder höher empfohlen
# Template: 3D (URP empfohlen für bessere Performance)
```

### Schritt 2: Pakete installieren

1. Öffne Package Manager (Window → Package Manager)
2. Installiere AR Foundation
3. Installiere ARCore XR Plugin
4. Installiere TextMeshPro

### Schritt 3: Projekteinstellungen

**Player Settings:**
```
- Company Name: Ihr Firmenname
- Product Name: D8-Planer XR
- Package Name: com.ihrefirma.d8planerxr
- Minimum API Level: Android 7.0 (API Level 24)
- Target API Level: Android 13 (API Level 33)
```

**XR Settings:**
```
- Initialize XR on Startup: Enabled
- ARCore: Enabled
```

**Permissions (AndroidManifest.xml):**
```xml
<uses-permission android:name="android.permission.CAMERA" />
<uses-permission android:name="android.permission.READ_EXTERNAL_STORAGE" />
<uses-permission android:name="android.permission.WRITE_EXTERNAL_STORAGE" />
<uses-permission android:name="android.permission.BLUETOOTH" />
<uses-permission android:name="android.permission.BLUETOOTH_ADMIN" />
```

### Schritt 4: Szenen-Setup

**Haupt-Szene (MainScene.unity):**

1. **AR Session:**
   - GameObject → XR → AR Session

2. **AR Session Origin:**
   - GameObject → XR → AR Session Origin
   - Mit AR Camera

3. **App Controller:**
   - Leeres GameObject erstellen
   - AppController.cs Script hinzufügen

4. **QR Code Tracker:**
   - Zum AR Session Origin hinzufügen
   - QRCodeTracker.cs Script hinzufügen

5. **UI Canvas:**
   - UI → Canvas (World Space)
   - CSVColumnMenuUI.cs Script hinzufügen

### Schritt 5: Prefabs erstellen

**VentilOverlay Prefab:**
```
VentilOverlay
├── Canvas (World Space)
│   └── Panel
│       ├── VentilNumberText (TextMeshPro)
│       └── SowListContainer (Vertical Layout Group)
└── VentilOverlay.cs
```

**SowItem Prefab:**
```
SowItem
├── Background (Image)
├── TrafficLightIcon (Image)
├── EarTagText (TextMeshPro)
├── DaysText (TextMeshPro)
└── StatusText (TextMeshPro)
```

**ColumnItem Prefab:**
```
ColumnItem
├── ColumnNameText (TextMeshPro)
├── DisplayNameInput (TMP_InputField)
├── VisibilityToggle (Toggle)
├── MoveUpButton (Button)
├── MoveDownButton (Button)
└── RemoveButton (Button)
```

## 🚀 Verwendung

### CSV-Datei vorbereiten

1. Exportieren Sie Daten aus dem "DB Sauenplaner"
2. Speichern Sie die CSV-Datei auf dem Gerät:
   ```
   /sdcard/Android/data/com.ihrefirma.d8planerxr/files/sauen.csv
   ```

### Spalten konfigurieren

1. App starten
2. Menü öffnen (Button oder Geste)
3. "CSV-Spalten konfigurieren" wählen
4. Spalten anpassen:
   - Hinzufügen: Name eingeben → "Hinzufügen"
   - Umbenennen: In "Anzeigename" ändern
   - Reihenfolge: Pfeiltasten verwenden
   - Entfernen: "X" Button klicken
5. Speichern

### QR-Codes generieren

**Im Unity Editor:**
```csharp
// QRCodeGenerator Component im Inspector
// → Context Menu → "Generiere alle QR-Codes (1-199)"
```

**Zur Laufzeit:**
```csharp
QRCodeGenerator generator = GetComponent<QRCodeGenerator>();
generator.GenerateAllQRCodes();
// Codes werden gespeichert in: Application.persistentDataPath/QRCodes/
```

### QR-Codes drucken

Die generierten PNG-Dateien können über:
- USB auf PC übertragen → Bluetooth Drucker
- Direkt vom Android-Gerät über Bluetooth drucken (Drucker-App erforderlich)

## 💾 Datenpersistenz

**Konfigurationsdateien:**
```
Application.persistentDataPath/
├── CSVConfigs/
│   ├── current_config.json
│   ├── Profil1.json
│   └── Profil2.json
├── QRCodes/
│   ├── Ventil_001.png
│   ├── Ventil_002.png
│   └── ...
└── sauen.csv (vom Nutzer abgelegt)
```

## 🔍 Testing

### Unit Tests erstellen

```csharp
// Tests/CSVColumnConfigTests.cs
[Test]
public void TestAddColumn()
{
    var config = new CSVColumnConfig();
    config.AddColumn("TestSpalte", ColumnType.Text);
    Assert.AreEqual(1, config.columns.Count);
}
```

### Mock-CSV erstellen

```csv
Ventilnummer;Ohrmarkennummer;Deckdatum;Trächtigkeitsstatus
1;DE123456;2024-01-15;Trächtig
1;DE123457;2024-01-20;Trächtig
2;DE123458;2024-02-10;Unsicher
```

### Editor-Simulation

```csharp
// Im QRCodeTracker:
[SerializeField] private bool simulateQRCodesInEditor = true;
// Aktivieren für Tests ohne echte Kamera
```

## 📱 APK Build

### Build-Einstellungen

1. File → Build Settings
2. Platform: Android
3. Texture Compression: ASTC
4. Build System: Gradle
5. Create Symbols.zip: Enabled (für Debugging)

### Signierung

1. Player Settings → Publishing Settings
2. Keystore erstellen oder vorhandenen auswählen
3. Keystore-Passwort eingeben

### Build-Prozess

```bash
# In Unity:
File → Build Settings → Build
# → D8PlanerXR.apk wird erstellt
```

### Installation

```bash
# Via ADB:
adb install D8PlanerXR.apk

# Via USB:
APK auf Gerät kopieren → Datei-Manager → Installieren
```

## 🐛 Troubleshooting

### Problem: QR-Codes werden nicht erkannt
**Lösung:**
- Beleuchtung verbessern
- Fehlerkorrektur-Level erhöhen (H)
- QR-Code-Größe anpassen
- Kamera-Auflösung prüfen

### Problem: CSV-Import schlägt fehl
**Lösung:**
- Delimiter prüfen (;, ,, Tab)
- Encoding prüfen (UTF-8)
- Spalten-Konfiguration validieren
- Logs prüfen: `adb logcat -s Unity`

### Problem: Overlays falsch positioniert
**Lösung:**
- AR-Tracking kalibrieren
- Offset-Werte anpassen
- Spatial Anchor-Logik prüfen

## 📊 Performance-Optimierung

### Empfohlene Einstellungen

```csharp
// QRCodeTracker
scanInterval = 0.2f; // 5 FPS ausreichend
maxSimultaneousDetections = 5; // Nicht zu viele gleichzeitig

// DataRepository
// Daten werden gecacht, kein ständiges Neueinlesen

// VentilOverlay
updateInterval = 1f; // 1 Sekunde Update-Intervall
```

### Rendering-Optimierung

- Low Poly UI-Elemente verwenden
- Texture-Atlassing für Icons
- Object Pooling für Sauen-Items
- Occlusion Culling aktivieren

## 🔐 Sicherheit

### Datenschutz

- Keine persönlichen Daten in die Cloud
- Verschlüsselung der CSV-Dateien optional
- Zugriffsrechte minimieren

### Best Practices

```csharp
// Keine Hardcoded Credentials
// Eingaben validieren
if (!config.Validate(out string error)) {
    Debug.LogError(error);
    return;
}
```

## 📝 Nächste Schritte

Nach Beantwortung des Fragenkatalogs:

1. ✅ Viture SDK Integration
2. ✅ Bluetooth Drucker-Integration
3. ✅ Feinabstimmung der Ampel-Logik
4. ✅ UI/UX Verbesserungen
5. ✅ Beta-Testing im Stall
6. ✅ Performance-Profiling
7. ✅ Finale Optimierungen

## 📞 Support

Bei Fragen oder Problemen:

1. Fragenkatalog konsultieren: `FRAGENKATALOG.md`
2. Logs prüfen: `adb logcat -s Unity`
3. Unity Console prüfen
4. GitHub Issues erstellen

## 📄 Lizenz

[Ihre Lizenz hier einfügen]

---

**Version:** 1.0  
**Datum:** 2025-11-26  
**Status:** Implementierung abgeschlossen, bereit für Testing
