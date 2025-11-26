# D8-Planer XR - Schnellstart-Anleitung

## Priorität #1: XR Vision im Deckzentrum

Diese Anleitung führt Sie durch den schnellsten Weg, um die App zum Laufen zu bringen.

---

## 📋 Voraussetzungen

### Hardware
- ✅ Viture Neckband Pro (Android 12/13)
- ✅ Viture Luma Ultra Brille
- ✅ Powerbank für dauerhaften Betrieb
- ✅ Katasymbol T50M Pro Bluetooth Label Printer (optional für QR-Code Druck)

### Software
- Unity 2022 LTS oder neuer
- Android SDK
- Python 3.7+ (für QR-Code Generierung)

---

## 🚀 Schnellstart (5 Schritte)

### Schritt 1: QR-Codes generieren

```bash
cd Tools
pip install qrcode[pil] pillow
python3 qr_generator_batch.py
```

**Ergebnis:** 199 QR-Codes im Ordner `Tools/QRCodes/` (1.jpg - 199.jpg)

---

### Schritt 2: Mock-Daten erstellen

**Option A: In Unity (empfohlen)**
1. Öffne Unity Projekt
2. Erstelle leeres GameObject: "DataGenerator"
3. Füge Component hinzu: `MockDataGenerator`
4. Right-Click → Context Menu → "Mock-Daten generieren"
5. Prüfe: `Application.persistentDataPath` Ordner

**Option B: Manuell**
Erstelle Datei `ImportDZ.csv` mit diesem Inhalt:

```csv
Sauennummer;Ventilnummer;Deckdatum;Trächtigkeitsstatus;Gesundheitsstatus;Bemerkungen
DE1234567890;1;26.08.2024;tragend;;Test Sau 1
DE1234567891;1;15.09.2024;tragend;;Test Sau 2
DE1234567892;2;01.10.2024;tragend;Medikation;Behandlung erforderlich
DE1234567893;3;10.11.2024;unbestätigt;;Frisch gedeckt
```

Speichere in: `<Android Device>/Android/data/com.d8planerxr.app/files/ImportDZ.csv`

---

### Schritt 3: Unity Szene einrichten

**3.1 Neue Szene erstellen**
- File → New Scene → "MainARScene"
- Speichern unter: `Assets/Scenes/MainARScene.unity`

**3.2 AR Kamera einrichten**
1. Lösche Main Camera
2. GameObject → XR → AR Session
3. GameObject → XR → AR Session Origin
4. AR Session Origin → Add Component → AR Camera Manager

**3.3 Core-System einrichten**
1. GameObject → Create Empty → "AppController"
2. Add Component → `AppController.cs`
3. Inspector:
   - Initialize On Start: ✓
   - Default CSV Path: "ImportDZ.csv"
   - App Version: "1.0.0"

**3.4 QR-Code Tracker einrichten**
1. GameObject → Create Empty → "QRCodeTracker"
2. Add Component → `QRCodeTracker.cs`
3. Drag AR Camera Manager zur Referenz
4. Inspector:
   - Scan Interval: 0.2 (5 FPS)
   - Max Simultaneous Detections: 5
   - Show Debug Info: ✓

**3.5 Virtuelles Deckzentrum**
1. GameObject → Create Empty → "VirtualDeckzentrum"
2. Add Component → `VirtualDeckzentrum.cs`
3. Inspector:
   - Ventil Spacing: 4.0 (400cm)
   - Ventil Height: 2.5 (250cm)

---

### Schritt 4: Prefabs erstellen

**4.1 Ventil Overlay Prefab**
1. GameObject → UI → Canvas → "VentilOverlayCanvas"
2. Canvas Inspector:
   - Render Mode: World Space
   - Width: 300, Height: 600 (30cm x 60cm)
3. Add Child → TextMeshPro → "VentilNumberText"
4. Add Child → Scroll View → "SowList"
5. Add Component → `VentilOverlay.cs`
6. Drag in Project → Create Prefab: `Assets/Prefabs/VentilOverlay.prefab`

**4.2 Sow Item Prefab (für Listen-Einträge)**
1. GameObject → UI → Panel → "SowItem"
2. Add Children:
   - TextMeshPro: "EarTagText"
   - TextMeshPro: "DaysText"
   - Image: "TrafficLightIcon"
3. Save as Prefab: `Assets/Prefabs/SowItem.prefab`

**4.3 Prefabs zuweisen**
- QRCodeTracker: Ventil Overlay Prefab → `VentilOverlay.prefab`
- VentilOverlay: Sow Item Prefab → `SowItem.prefab`

---

### Schritt 5: Build & Test

**5.1 Build Settings**
1. File → Build Settings
2. Platform: Android
3. Switch Platform
4. Add Open Scenes
5. Player Settings:
   - Company Name: "D8PlanerXR"
   - Product Name: "D8-Planer XR"
   - Package Name: "com.d8planerxr.app"
   - Minimum API Level: Android 12 (API 31)
   - Target API Level: Android 13 (API 33)
   - Scripting Backend: IL2CPP
   - Target Architectures: ARM64

**5.2 XR Plugin Management**
1. Edit → Project Settings → XR Plugin Management
2. Android Tab: Enable ARCore

**5.3 Permissions**
- AndroidManifest.xml:
  ```xml
  <uses-permission android:name="android.permission.CAMERA" />
  <uses-permission android:name="android.permission.BLUETOOTH" />
  <uses-permission android:name="android.permission.BLUETOOTH_ADMIN" />
  ```

**5.4 Build APK**
1. Build Settings → Build
2. Speichern als: `D8PlanerXR_v1.0.apk`

**5.5 Auf Gerät installieren**
```bash
adb install D8PlanerXR_v1.0.apk
```

---

## ✅ Test-Checkliste

### Editor-Test (ohne Hardware)
- [ ] AppController initialisiert ohne Fehler
- [ ] Mock-Daten werden generiert
- [ ] CSV-Import funktioniert
- [ ] Ampel-Farben werden korrekt berechnet
- [ ] QR-Code Simulation zeigt Overlay

### Build-Test (Android)
- [ ] App startet ohne Crash
- [ ] CSV-Datei wird gefunden und geladen
- [ ] AR-Kamera aktiviert sich
- [ ] Logging zeigt System-Info

### Hardware-Test (Viture)
- [ ] App läuft auf Viture Neckband
- [ ] QR-Codes werden erkannt (teste mit gedruckten Codes)
- [ ] Overlays erscheinen am richtigen Ort
- [ ] Mehrere QR-Codes gleichzeitig erkennbar
- [ ] Virtuelles Deckzentrum funktioniert

---

## 🔧 Troubleshooting

### Problem: "CSV nicht gefunden"
**Lösung:**
```csharp
Debug.Log(Application.persistentDataPath);
```
Kopiere CSV in diesen Ordner.

### Problem: "QR-Codes werden nicht erkannt"
**Lösung:**
1. Prüfe Kamera-Permission
2. Teste mit gedruckten QR-Codes (nicht Bildschirm)
3. Aktiviere Debug-Logs im QRCodeTracker
4. Prüfe Beleuchtung

### Problem: "Overlays erscheinen nicht"
**Lösung:**
1. Prüfe Prefab-Zuweisung
2. Prüfe AR Camera Manager
3. Aktiviere "Simulate QR Codes in Editor"

### Problem: "Build-Fehler"
**Lösung:**
1. Prüfe Unity Version (2022 LTS empfohlen)
2. Prüfe Android SDK Installation
3. Prüfe ARCore Plugin aktiviert
4. Clean Build (Delete Library folder)

---

## 📊 Erwartete Ergebnisse

Nach erfolgreichem Setup:

1. **QR-Code Scan**
   - Erkennungsrate: >90% bei guten Bedingungen
   - Erkennungszeit: <2 Sekunden
   - Mehrfach-Erkennung: 2-5 Codes gleichzeitig

2. **Overlay-Anzeige**
   - Position: 10cm unter QR-Code
   - Größe: 30cm x 60cm
   - Aktualisierung: Alle 1 Sekunde

3. **Ampelsystem**
   - Grün: 0-79 Tage
   - Gelb: 80-106 Tage
   - Rot: 107+ Tage
   - Lila: Medikation (höchste Priorität)

4. **Performance**
   - FPS: >30 FPS
   - Akkulaufzeit: Unbegrenzt (Powerbank)
   - QR-Scan-Rate: 5 FPS

---

## 📱 Bedienung

### Erste Schritte im Stall
1. Viture Neckband einschalten
2. Brille aufsetzen
3. App starten
4. Auf Ventil schauen → QR-Code wird erkannt
5. Overlay erscheint automatisch mit Sau-Daten

### Virtuelles Deckzentrum
1. Sprachbefehl: "Deckzentrum anzeigen"
2. Oder: Menu → Virtuelles Deckzentrum
3. Sehe alle Ventile auf einmal in 3D-Ansicht

### CSV aktualisieren
1. Neue CSV-Datei speichern als "ImportDZ.csv"
2. App → Menu → CSV neu importieren
3. Bestätigung abwarten

---

## 🎯 Next Steps

Nach erfolgreichem Test:
1. Setup-Modus durchlaufen (Ventile im Stall scannen)
2. Echte CSV-Daten aus DB Sauenplaner importieren
3. QR-Codes auf Etiketten drucken und anbringen
4. Erste Arbeitsrunde im Stall

---

## 📞 Support

Bei Problemen:
1. Prüfe Console-Logs in Unity
2. Nutze `AppController.ShowSystemInfo()` für Diagnose
3. Prüfe `IMPLEMENTATION_GUIDE.md` für Details

**Viel Erfolg! 🐷**
