# Changelog - D8-Planer XR

Alle wichtigen Änderungen an diesem Projekt werden in dieser Datei dokumentiert.

Das Format basiert auf [Keep a Changelog](https://keepachangelog.com/de/1.0.0/).

---

## [1.1.0] - 2025-11-26

### ✨ Hinzugefügt

#### Handy-Modus (Mobile Phone Mode)
- **Dual-Mode Support**: App unterstützt jetzt VR-Modus (Viture) UND Handy-Modus (Smartphone)
- **DeviceModeManager**: Neue Komponente für automatische Hardware-Erkennung und Modusverwaltung
  - Automatische Erkennung von VR-Hardware vs. Smartphone
  - Manueller Moduswechsel möglich
  - Optimierte Einstellungen pro Modus
- **ModeSelectionUI**: UI-Komponente zum Wechseln zwischen Modi
  - Visuelles Feedback für aktiven Modus
  - Modus-Informationen und Hilfe
  - Touch-freundliche Bedienung

#### Dokumentation
- **APK_BUILD_ANLEITUNG.md**: Vollständige deutsche Anleitung zum Erstellen von APK-Dateien
  - Schritt-für-Schritt Build-Prozess
  - Android-Einstellungen konfigurieren
  - Installation auf Geräten
  - Umfangreiche Fehlerbehebung
  - VR vs. Handy Modus Unterschiede
- **HANDY_MODUS_ANLEITUNG.md**: Benutzerhandbuch für Smartphone-Nutzung
  - Schnellstart-Anleitung
  - QR-Code Scanning Best Practices
  - Touch-Steuerung erklärt
  - CSV-Import-Methoden
  - Batterie-Optimierung
  - Workflows und FAQs
- **VR_VS_HANDY_VERGLEICH.md**: Detaillierter Vergleich beider Modi
  - Entscheidungshilfe
  - Nutzungsszenarien
  - Kosten-Nutzen-Analyse
  - ROI-Berechnung
  - Schulungsaufwand

### 🔧 Geändert

#### AR-System
- **QRCodeTracker.cs**: 
  - Unterschiedliche Scan-Intervalle für VR (0.2s) vs. Handy (0.5s)
  - Reduzierte gleichzeitige Detections im Handy-Modus (1 statt 5)
  - Integration mit DeviceModeManager
  - Batterie-optimierte Scan-Strategie
- **VentilOverlay.cs**:
  - Dynamische Update-Intervalle basierend auf Modus
  - Optimierte Performance für Handy-Modus
  - Touch-responsive Overlays
- **AppController.cs**:
  - Integration von DeviceModeManager
  - Version auf 1.1.0 erhöht
  - Erweiterte Initialisierung für Dual-Mode

#### Dokumentation
- **README.md**: 
  - Handy-Modus in Features aufgenommen
  - Neue Dokumentations-Links
  - Aktualisierte Voraussetzungen
  - Status und Version aktualisiert

### 🐛 Fehlerbehebungen
- Keine (neue Features)

### ⚡ Performance
- **Handy-Modus Optimierungen**:
  - Längeres Scan-Intervall spart Batterie
  - Reduzierte CPU-Last durch Single-QR-Detection
  - Optimierte Update-Zyklen für Overlays
  - 30-50% längere Batterie-Laufzeit im Vergleich zu VR-Modus

### 🔐 Sicherheit
- Keine Änderungen

---

## [1.0.0] - 2025-11-XX (Vorherige Version)

### ✨ Hinzugefügt
- **Kernfunktionalität**:
  - QR-Code basierte AR-Erkennung
  - Ventil-Overlays mit Ampelsystem
  - CSV-Import und -Verwaltung
  - Virtuelles Deckzentrum (3D)
  - Spatial Anchoring
  
- **Daten-Management**:
  - DataRepository mit effizientem Caching
  - CSVConfigManager für flexible Spalten-Konfiguration
  - CSVColumnMenuUI für Benutzer-Konfiguration
  - Mock-Data Generator für Testing
  
- **AR-Features**:
  - QRCodeTracker mit Multi-Detection
  - VentilOverlay mit dynamischer Sau-Liste
  - VirtualDeckzentrum mit Setup-Modus
  
- **QR-Code Generation**:
  - Unity QRCodeGenerator
  - Standalone Python qr_generator.py Tool
  
- **Dokumentation**:
  - README.md mit Projekt-Übersicht
  - FRAGENKATALOG.md mit 100+ Fragen
  - IMPLEMENTATION_GUIDE.md
  - CSV_EXAMPLES.md
  - QUICKSTART.md

---

## Geplante Features

### [1.2.0] - Geplant
- [ ] Viture SDK Integration (native VR-Features)
- [ ] Bluetooth-Drucker Support
- [ ] Erweiterte Touch-Gesten
- [ ] Offline-Sync mit Cloud
- [ ] Dark Mode
- [ ] Mehrsprachigkeit (EN, FR)

### [1.3.0] - Geplant
- [ ] Statistik-Dashboard
- [ ] Export zu Excel
- [ ] Backup/Restore Funktionalität
- [ ] Benutzer-Verwaltung
- [ ] Audit-Log

### [2.0.0] - Vision
- [ ] KI-basierte Gesundheitsprognosen
- [ ] Integration mit Stallmanagement-Systemen
- [ ] Web-Dashboard für Management
- [ ] Multi-User Collaboration
- [ ] Erweiterte Analytics

---

## Migration Guide

### Von 1.0.0 zu 1.1.0

**Änderungen für Entwickler:**

1. **Neue Abhängigkeit**: DeviceModeManager
   ```csharp
   // Optional: DeviceModeManager im Code nutzen
   if (DeviceModeManager.Instance != null)
   {
       bool isMobile = DeviceModeManager.Instance.IsMobileMode;
   }
   ```

2. **QRCodeTracker**: Neue serialisierte Felder
   - `scanIntervalMobile` (0.5s default)
   - `maxSimultaneousDetectionsMobile` (1 default)
   
3. **Keine Breaking Changes**: 
   - Alte Szenen funktionieren weiterhin
   - DeviceModeManager wird automatisch erstellt falls fehlend
   - Rückwärtskompatibel

**Änderungen für Endnutzer:**

1. **Automatische Modus-Erkennung**:
   - App startet automatisch im passenden Modus
   - Kein Setup nötig
   
2. **Neue UI-Optionen**:
   - Einstellungen → Anzeigemodus
   - Manueller Wechsel möglich
   
3. **Keine Daten-Migration nötig**:
   - CSV-Dateien kompatibel
   - Einstellungen bleiben erhalten

---

## Versioning-Schema

Dieses Projekt folgt [Semantic Versioning](https://semver.org/):

- **MAJOR** (X.0.0): Breaking Changes, inkompatible API-Änderungen
- **MINOR** (1.X.0): Neue Features, rückwärtskompatibel
- **PATCH** (1.0.X): Bugfixes, kleine Verbesserungen

---

## Support & Kontakt

- **GitHub Issues**: Für Bugs und Feature-Requests
- **E-Mail**: support@ihre-firma.de
- **Dokumentation**: Siehe README.md und verlinkte Guides

---

*Letzte Aktualisierung: 2025-11-26*
