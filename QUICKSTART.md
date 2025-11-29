# D8-Planer XR - Schnellstart-Anleitung

## 📱 Handy-Modus FIRST! (Empfohlen zum Testen)

Die App startet automatisch im Handy-Modus. Dieser Modus funktioniert auf jedem Android-Smartphone und ist der schnellste Weg zum Testen.

---

## 🚀 Schnellstart Handy-Modus (3 Schritte)

### Schritt 1: APK installieren
```bash
adb install D8PlanerXR.apk
```

### Schritt 2: CSV-Datei kopieren
Kopieren Sie die `MusterPlan.csv` in:
```
/Android/data/com.d8planerxr.app/files/MusterPlan.csv
```

### Schritt 3: App starten & QR-Code scannen
1. App öffnen (startet automatisch im Handy-Modus)
2. Kamera wird aktiviert
3. QR-Code auf Ventil scannen
4. Sau-Daten werden als Tabelle angezeigt

**Das war's! 🎉**

---

## 📊 Ampel-Bedeutung

| Farbe | Bedeutung | Tage seit Deckung |
|-------|-----------|-------------------|
| 🟢 Grün | Normal, tragend | 0-79 Tage |
| 🟡 Gelb | Mittelfristig | 80-106 Tage |
| 🔴 Rot | Kurz vor Abferkelung | 107+ Tage |
| 🟣 Lila | **Medikation erforderlich!** | (Priorität) |

---

## 🔧 Handy-Modus Bedienung

### QR-Code scannen
1. Kamera auf QR-Code richten (Abstand 20-50cm)
2. Grüner Rahmen zeigt Scan-Bereich
3. Bei Erkennung: Vibration + Overlay erscheint

### Anzeige verstehen
```
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Ventil 165
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Status  │ Ohrmarke │ Tage │ Datum
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
🟣      │ 602      │  134 │ 13.07
🔴      │ 450      │  133 │ 14.07
🟡      │ 646      │   85 │ 14.07
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Gesamt: 3 Sauen
  🟣 Medikation: 1
  🔴 Kurz vor Abferkelung: 1
  🟡 Mittelfristig: 1
```

### Menü öffnen
- Tippen auf ☰ (oben rechts)
- CSV neu laden
- Modus wechseln (VR ↔ Handy)

---

## 🕶️ VR-Modus (Viture Neckband)

### Wechsel zu VR-Modus
1. Menü öffnen (☰)
2. "Einstellungen" → "VR-Modus aktivieren"
3. Oder: Automatische Erkennung bei Viture-Hardware

### VR-Modus Features
- Mehrere QR-Codes gleichzeitig
- Spatial Anchoring (Overlays bleiben am Ventil)
- Virtuelles Deckzentrum (3D-Übersicht)

---

## 📋 Voraussetzungen

### Hardware (Handy-Modus)
- ✅ Android Smartphone (Android 10+)
- ✅ Kamera

### Hardware (VR-Modus)
- ✅ Viture Neckband Pro (Android 12/13)
- ✅ Viture Luma Ultra Brille
- ✅ Powerbank für dauerhaften Betrieb

### Software (für Entwicklung)
- Unity 2022 LTS oder neuer
- Android SDK
- Python 3.7+ (für QR-Code Generierung)

---

## 🔨 Entwickler-Schnellstart (Unity Setup)

### QR-Codes generieren

```bash
cd Tools
pip install qrcode[pil] pillow
python3 qr_generator_batch.py
```

**Ergebnis:** 199 QR-Codes im Ordner `Tools/QRCodes/` (1.jpg - 199.jpg)

---

### MusterPlan.csv Format (DB Sauenplaner Export)

Die App unterstützt das native Export-Format des DB Sauenplaners:

```csv
"Stichtag";"Abf.";"Wochen bis";"Sau-Nr.";"[Datum]";"TK";"Bucht";"Bel.Datum";"TRT";"Gruppe";"Eber";"Wurf";"Umr.";"vorauss.";"Abferkelung"
" -3";"     602";"+";"165   ";"13.07.2025";" 134";"202529";"    M 88";"11";"  ";" ";"";"05.11.2025";"29.6"
```

**Wichtige Spalten:**
- Spalte 2 (Abf.) = Ohrmarke
- Spalte 4 (Sau-Nr.) = Ventil/Bucht für QR-Code Zuordnung
- Spalte 5 = Belegdatum
- Spalte 6 (TK) = Tage trächtig

---

### Handy-Szene erstellen (Unity)

**Empfohlen:** Nutze den Editor-Helfer:
1. Menü: **D8-Planer → Handy-Szene erstellen**
2. Fertig! Szene wird automatisch erstellt mit:
   - Kamera-Anzeige
   - QR-Scanner
   - UI-Overlays
   - Menü-System

**Alternativ manuell:**
1. File → New Scene → "MobileScene"
2. Füge hinzu:
   - Main Camera
   - Canvas mit RawImage (für Kamerabild)
   - MobileCameraController
   - AppBootstrap (Core/AppBootstrap.cs)

---

### Unity Szene für VR einrichten

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
   - Default CSV Path: "MusterPlan.csv"
   - App Version: "1.2.0"

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

### Handy-Modus Test
- [ ] App startet im Handy-Modus (Kamera automatisch aktiv)
- [ ] CSV-Daten werden geladen (oder Mock-Daten generiert)
- [ ] QR-Code wird erkannt (Vibration + Overlay)
- [ ] Sau-Daten werden als Tabelle angezeigt
- [ ] Ampelfarben sind korrekt (Grün/Gelb/Rot/Lila)
- [ ] Menü öffnet sich mit ☰ Button

### VR-Modus Test
- [ ] Moduswechsel funktioniert (Menü → VR-Modus)
- [ ] Mehrere QR-Codes gleichzeitig erkennbar
- [ ] Overlays bleiben am Ventil (Spatial Anchoring)
- [ ] Virtuelles Deckzentrum funktioniert

### Editor-Test (ohne Hardware)
- [ ] AppBootstrap initialisiert ohne Fehler
- [ ] Mock-Daten werden generiert
- [ ] CSV-Import funktioniert
- [ ] Ampel-Farben werden korrekt berechnet
- [ ] Unit Tests bestehen (Tests/EditMode)

---

## 🔧 Troubleshooting

### Problem: "Kamera startet nicht"
**Lösung:**
1. Prüfe Kamera-Berechtigung in Android-Einstellungen
2. Starte App neu
3. Bei erster Installation: Berechtigung manuell erteilen

### Problem: "Keine Daten bei QR-Scan"
**Lösung:**
1. Prüfe ob CSV geladen wurde (Menü → Info)
2. CSV-Datei in persistentDataPath kopieren:
   ```
   /Android/data/com.d8planerxr.app/files/MusterPlan.csv
   ```
3. App neu starten

### Problem: "QR-Codes werden nicht erkannt"
**Lösung:**
1. Abstand zum QR-Code: 20-50cm
2. QR-Code frontal scannen (nicht schräg)
3. Gute Beleuchtung wichtig
4. Gedruckte QR-Codes funktionieren besser als Bildschirm

### Problem: "Falsche Ampelfarben"
**Lösung:**
1. Prüfe Belegdatum in CSV
2. Ampel basiert auf Tagen seit Deckung:
   - Grün: 0-79 Tage
   - Gelb: 80-106 Tage
   - Rot: 107+ Tage
   - Lila: Medikation (Priorität!)

### Problem: "Build-Fehler"
**Lösung:**
1. Unity Version: 2022 LTS empfohlen
2. Android SDK korrekt installieren
3. Scripting Backend: IL2CPP
4. Clean Build: Library-Ordner löschen

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
