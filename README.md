# 3DoF_Deckzentrum - D8-Planer XR

> **Augmented Reality Anwendung für das Schweinedeckzentrum**  
> Viture Neckband Pro + Viture Luma Ultra Brille

[![Unity](https://img.shields.io/badge/Unity-2022.3%20LTS-blue)](https://unity.com/)
[![Platform](https://img.shields.io/badge/Platform-Android-green)](https://www.android.com/)
[![License](https://img.shields.io/badge/License-Proprietary-red)]()

## 📋 Projektübersicht

Die D8-Planer XR App ermöglicht die Visualisierung von Sauendaten im Deckzentrum mittels Augmented Reality. Während der Arbeit im Stall werden automatisch Informationen zu den Sauen an jedem Ventil als virtuelles Overlay eingeblendet.

### Kernfunktionen

✅ **QR-Code basierte AR-Erkennung**
- Automatische Erkennung von Ventilen via QR-Code
- Virtuelle Overlays unter jedem Ventil
- Spatial Anchoring für stabile Anzeige

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
- Viture Neckband Pro + Luma Ultra Brille

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
| [FRAGENKATALOG.md](FRAGENKATALOG.md) | **100+ Fragen** in 12 Kategorien zu allen Projektaspekten |
| [IMPLEMENTATION_GUIDE.md](IMPLEMENTATION_GUIDE.md) | Vollständige Implementierungsanleitung mit Setup, Prefabs, Testing |
| [CSV_EXAMPLES.md](CSV_EXAMPLES.md) | CSV-Format-Beispiele und Import-Anleitung |
| [Tools/README.md](Tools/README.md) | QR-Code Generator Tool Dokumentation |

## 🎯 Features im Detail

### 1. CSV-Spalten-Konfigurationsmenü

**Vollständig implementiert** in `Assets/Scripts/UI/CSVColumnMenuUI.cs`

- ✅ Spalten hinzufügen, entfernen, umbenennen
- ✅ Reihenfolge ändern (Drag & Drop)
- ✅ Spaltentypen (Text, Nummer, Datum, Boolean)
- ✅ Rollen zuweisen (Ventilnummer, Ohrmarkennummer, etc.)
- ✅ Sichtbarkeit ein-/ausschalten
- ✅ Profile speichern/laden
- ✅ Auto-Import aus CSV

### 2. Daten-Repository

**Implementiert** in `Assets/Scripts/Data/DataRepository.cs`

- ✅ Effizientes Caching (O(1) Zugriff)
- ✅ Ventil → Sauen Zuordnung
- ✅ Ohrmarkennummer → Sau Zuordnung
- ✅ Automatische Ampel-Berechnung
- ✅ Flexible Sortierung
- ✅ Fehlertoleranter Import

### 3. QR-Code System

**Dual-Implementierung:**
- Unity: `Assets/Scripts/QRCode/QRCodeGenerator.cs`
- Standalone: `Tools/qr_generator.py`

Features:
- ✅ QR-Codes für Ventile 1-199
- ✅ Fehlerkorrektur Level H (30%)
- ✅ Menschenlesbare Labels
- ✅ Druckerfreundliches Format

### 4. AR-Overlay

**Implementiert** in `Assets/Scripts/AR/VentilOverlay.cs`

- ✅ Spatial Anchoring
- ✅ Ampel-Anzeige
- ✅ Dynamische Sau-Liste
- ✅ Auto-Update
- ✅ Kamera-orientiert

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

1. ⏳ Viture SDK Integration
2. ⏳ Bluetooth Drucker-Integration
3. ⏳ Erweiterte UI/UX Features
4. ⏳ Beta-Testing im Stall
5. ⏳ Performance-Optimierung

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

**Status:** ✅ Kernfunktionalität implementiert, bereit für Testing  
**Version:** 1.0  
**Letzte Aktualisierung:** 2025-11-26
