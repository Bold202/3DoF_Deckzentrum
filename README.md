# 3DoF_Deckzentrum - D8-Planer XR

> **Augmented Reality Anwendung für das Schweinedeckzentrum**  
> Viture Neckband Pro + Viture Luma Ultra Brille

[![Unity](https://img.shields.io/badge/Unity-2022.3%20LTS-blue)](https://unity.com/)
[![Platform](https://img.shields.io/badge/Platform-Android-green)](https://www.android.com/)
[![License](https://img.shields.io/badge/License-Proprietary-red)]()

## 📋 Projektübersicht

Die D8-Planer XR App ermöglicht die Visualisierung von Sauendaten im Deckzentrum mittels Augmented Reality. Während der Arbeit im Stall werden automatisch Informationen zu den Sauen an jedem Ventil als virtuelles Overlay eingeblendet.

**NEU in Version 1.1:** 📱 **Handy-Modus** - Die App kann jetzt auch auf normalen Smartphones ohne VR-Hardware genutzt werden!

### Kernfunktionen

✅ **Dual-Mode Support**
- 🕶️ VR-Modus für Viture Neckband Pro + Luma Ultra
- 📱 Handy-Modus für normale Android-Smartphones
- 🔄 Automatische Hardware-Erkennung

✅ **QR-Code basierte AR-Erkennung**
- Automatische Erkennung von Ventilen via QR-Code
- Virtuelle Overlays unter jedem Ventil
- Spatial Anchoring für stabile Anzeige (VR-Modus)

✅ **Intelligentes Ampelsystem**
- Grün/Gelb/Rot basierend auf Trächtigkeitsstatus
- Automatische Berechnung der Tage seit Deckung
- Prioritätsbasierte Sortierung (Rot zuerst)

✅ **Flexibles CSV-Management**
- Konfigurierbare Spalten-Struktur
- Import/Export von Konfigurationsprofilen
- Unterstützung für verschiedene CSV-Formate (Delimiter, Encoding)
- Fehlertoleranter Parser

✅ **Offline-First Design**
- Keine Internetverbindung erforderlich
- Lokale Datenspeicherung
- Schneller Zugriff durch Caching (HashMap)

✅ **QR-Code Generator**
- Standalone Python-Tool
- Generiert QR-Codes für Ventile 1-199
- Druckerfreundliches Format
- Hohe Fehlerkorrektur (Level H - 30%)

## 🗂 Projektstruktur

```
3DoF_Deckzentrum/
├── Assets/
│   ├── Scripts/
│   │   ├── Core/              # AppController
│   │   ├── Data/              # CSV-Verwaltung, DataRepository
│   │   ├── AR/                # QR-Tracking, Overlays
│   │   ├── UI/                # CSV-Spalten-Menü
│   │   ├── QRCode/            # QR-Generator (Unity)
│   │   └── Utils/             # Hilfsfunktionen
│   ├── Scenes/                # Unity-Szenen
│   ├── Prefabs/               # UI-Prefabs
│   └── Resources/             # Assets
├── Tools/
│   ├── qr_generator.py        # Standalone QR-Generator
│   └── README.md              # Tool-Dokumentation
├── FRAGENKATALOG.md           # Umfassender Fragenkatalog (12 Kategorien, 100+ Fragen)
├── IMPLEMENTATION_GUIDE.md    # Detaillierte Implementierungsanleitung
├── CSV_EXAMPLES.md            # CSV-Format-Beispiele
└── README.md                  # Diese Datei
```

## 🚀 Quick Start

### Voraussetzungen

- Unity 2022.3 LTS oder höher
- Android SDK (API Level 24+)
- Python 3.6+ (für QR-Generator Tool)
- **Optional:** Viture Neckband Pro + Luma Ultra Brille (für VR-Modus)
- **Alternativ:** Android-Smartphone mit ARCore-Support (für Handy-Modus)

### Installation

1. **Repository klonen**
   ```bash
   git clone https://github.com/IhreFirma/3DoF_Deckzentrum.git
   cd 3DoF_Deckzentrum
   ```

2. **Unity-Projekt öffnen**
   - Unity Hub → "Add" → Projektordner auswählen
   - Unity Version 2022.3 LTS wählen

3. **Pakete installieren**
   - AR Foundation (via Package Manager)
   - ARCore XR Plugin
   - TextMeshPro

4. **QR-Codes generieren**
   ```bash
   cd Tools
   pip install qrcode[pil] pillow
   python qr_generator.py
   ```

5. **APK bauen**
   - File → Build Settings → Android
   - Build

### Erste Schritte

1. **CSV-Datei vorbereiten**
   - Siehe `CSV_EXAMPLES.md` für Formatierung
   - Auf Android-Gerät kopieren

2. **App starten**
   - APK auf Viture Neckband installieren
   - App öffnen

3. **Spalten konfigurieren**
   - Menü → CSV-Spalten konfigurieren
   - Spalten nach Bedarf anpassen
   - Speichern

4. **QR-Codes scannen**
   - An Ventil vorbeigehen
   - Overlay erscheint automatisch

## 📖 Dokumentation

| Dokument | Beschreibung |
|----------|--------------|
| [APK_BUILD_ANLEITUNG.md](APK_BUILD_ANLEITUNG.md) | **Vollständige APK-Build-Anleitung** für Android-Geräte |
| [HANDY_MODUS_ANLEITUNG.md](HANDY_MODUS_ANLEITUNG.md) | **Benutzerhandbuch für Handy-Modus** (ohne VR-Hardware) |
| [FRAGENKATALOG.md](FRAGENKATALOG.md) | **100+ Fragen** in 12 Kategorien zu allen Projektaspekten |
| [IMPLEMENTATION_GUIDE.md](IMPLEMENTATION_GUIDE.md) | Vollständige Implementierungsanleitung mit Setup, Prefabs, Testing |
| [CSV_EXAMPLES.md](CSV_EXAMPLES.md) | CSV-Format-Beispiele und Import-Anleitung |
| [Tools/README.md](Tools/README.md) | QR-Code Generator Tool Dokumentation |

## 🎯 Features im Detail

### 1. Dual-Mode Betrieb (NEU in v1.1)

**VR-Modus (🕶️ Viture Neckband Pro + Luma Ultra):**
- Volle AR-Funktionalität mit 6DoF Head-Tracking
- Spatial Anchoring für stabile Overlays
- Virtuelles Deckzentrum (3D-Übersicht)
- Multi-QR-Code Erkennung (bis zu 5 gleichzeitig)
- Freihändige Bedienung

**Handy-Modus (📱 Android-Smartphone):**
- Touch-basierte Steuerung
- Einzeln-QR-Code Scanning
- Batterie-optimiert
- Keine zusätzliche Hardware nötig
- Ideal für schnelle Kontrollen

**Automatische Erkennung:**
- App erkennt verfügbare Hardware
- Wechsel zwischen Modi jederzeit möglich
- Optimierte Einstellungen pro Modus

### 2. CSV-Spalten-Konfigurationsmenü

**Vollständig implementiert** in `Assets/Scripts/UI/CSVColumnMenuUI.cs`

- ✅ Spalten hinzufügen, entfernen, umbenennen
- ✅ Reihenfolge ändern (Drag & Drop)
- ✅ Spaltentypen (Text, Nummer, Datum, Boolean)
- ✅ Rollen zuweisen (Ventilnummer, Ohrmarkennummer, etc.)
- ✅ Sichtbarkeit ein-/ausschalten
- ✅ Profile speichern/laden
- ✅ Auto-Import aus CSV

### 3. Daten-Repository

**Implementiert** in `Assets/Scripts/Data/DataRepository.cs`

- ✅ Effizientes Caching (O(1) Zugriff)
- ✅ Ventil → Sauen Zuordnung
- ✅ Ohrmarkennummer → Sau Zuordnung
- ✅ Automatische Ampel-Berechnung
- ✅ Flexible Sortierung
- ✅ Fehlertoleranter Import

### 4. QR-Code System

**Dual-Implementierung:**
- Unity: `Assets/Scripts/QRCode/QRCodeGenerator.cs`
- Standalone: `Tools/qr_generator.py`

Features:
- ✅ QR-Codes für Ventile 1-199
- ✅ Fehlerkorrektur Level H (30%)
- ✅ Menschenlesbare Labels
- ✅ Druckerfreundliches Format
- ✅ Modus-optimierte Scan-Intervalle

### 5. AR-Overlay

**Implementiert** in `Assets/Scripts/AR/VentilOverlay.cs`

- ✅ Spatial Anchoring (VR-Modus)
- ✅ Ampel-Anzeige
- ✅ Dynamische Sau-Liste
- ✅ Auto-Update mit Modus-optimierten Intervallen
- ✅ Kamera-orientiert
- ✅ Touch-responsiv (Handy-Modus)

## 🔧 Konfiguration

### CSV-Format

```csv
Ventilnummer;Ohrmarkennummer;Deckdatum;Trächtigkeitsstatus
1;DE123456;2024-01-15;Trächtig
1;DE123457;2024-01-20;Trächtig
2;DE123458;2024-02-10;Unsicher
```

### Ampel-Schwellwerte

Konfigurierbar in `DataRepository.cs`:
```csharp
greenThresholdMin = 0;    // Grün: Tag 0-21
greenThresholdMax = 21;
yellowThresholdMin = 22;  // Gelb: Tag 22-28
yellowThresholdMax = 28;
redThresholdMin = 29;     // Rot: Tag 29+
```

## 📊 Nächste Schritte

Nach Beantwortung des [Fragenkatalogs](FRAGENKATALOG.md):

1. ✅ Dual-Mode Support (VR + Handy) - **IMPLEMENTIERT**
2. ✅ APK Build Anleitung - **DOKUMENTIERT**
3. ⏳ Viture SDK Integration (optional für erweiterte VR-Features)
4. ⏳ Bluetooth Drucker-Integration
5. ⏳ Erweiterte UI/UX Features
6. ⏳ Beta-Testing im Stall (beide Modi)
7. ⏳ Performance-Optimierung

## 🤝 Beitragen

Bitte konsultieren Sie den [FRAGENKATALOG.md](FRAGENKATALOG.md) für offene Fragen und Anforderungen.

## 📝 Lizenz

[Ihre Lizenz hier einfügen]

## 📞 Support

Bei Fragen:
1. Fragenkatalog konsultieren
2. Implementierungsleitfaden lesen
3. GitHub Issues erstellen

---

**Status:** ✅ Kernfunktionalität implementiert, VR + Handy-Modus verfügbar  
**Version:** 1.1  
**Letzte Aktualisierung:** 2025-11-26
