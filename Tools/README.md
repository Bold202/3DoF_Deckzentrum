# QR-Code Generator Tool

## Python Standalone Tool

### Installation

```bash
# Python 3.6+ erforderlich
pip install qrcode[pil] pillow
```

### Verwendung

#### Alle QR-Codes generieren (1-199)
```bash
python qr_generator.py
```

#### Bereich festlegen
```bash
python qr_generator.py --start 1 --end 50
```

#### Einzelnen QR-Code generieren
```bash
python qr_generator.py --single 42
```

#### Ohne menschenlesbare Nummer
```bash
python qr_generator.py --no-label
```

#### Eigenes Ausgabeverzeichnis
```bash
python qr_generator.py --output /pfad/zum/ordner
```

### Ausgabe

Die generierten QR-Codes werden als PNG-Dateien gespeichert:
- Format: `Ventil_XXX.png` (z.B. `Ventil_001.png`)
- Auflösung: ~290x350 Pixel (mit Label)
- Fehlerkorrektur: H (30% - optimal für verschmutzte Umgebung)

## Drucken der QR-Codes

### Option 1: Desktop-Drucker

1. QR-Codes auf PC generieren
2. In Word/Excel importieren
3. Auf Etiketten-Papier drucken (z.B. Avery Zweckform)

### Option 2: Bluetooth Label Printer

#### Benötigte Software:
- Android App für den spezifischen Drucker (z.B. "Brother iPrint&Label")
- Oder: Eigene App-Integration (siehe unten)

#### Manueller Workflow:
1. QR-Codes auf Android-Gerät kopieren
2. Drucker-App öffnen
3. Bilder auswählen
4. Drucken

### Option 3: Direkt aus Unity-App (Zukünftige Erweiterung)

```csharp
// Bluetooth Drucker Integration
// TODO: Implementierung nach Drucker-Modell-Spezifikation
```

## Technische Details

### QR-Code Format

- **Inhalt**: `VENTIL-XXX` (z.B. `VENTIL-042`)
- **Version**: QR Code Version 1 (21x21 Module)
- **Fehlerkorrektur**: Level H (30%)
- **Größe**: Skalierbar, Standard 290x290 Pixel

### Warum Fehlerkorrektur Level H?

- **L (7%)**: Gut für saubere Umgebungen
- **M (15%)**: Standard
- **Q (25%)**: Erhöhte Robustheit
- **H (30%)**: Optimal für Stall-Umgebung (Schmutz, Beschädigungen)

### Lesbarkeit

QR-Codes können gelesen werden:
- Aus 30-150 cm Entfernung
- Bei schlechten Lichtverhältnissen (dank hoher Fehlerkorrektur)
- Auch wenn bis zu 30% des Codes beschädigt sind

## Anpassungen

### QR-Code Inhalt ändern

In `qr_generator.py`:
```python
self.content_template = "VENTIL-{:03d}"  # Aktuelles Format
# Ändern zu:
self.content_template = "V{:03d}"        # Kürzer
self.content_template = "{}"             # Nur Nummer
```

### Größe anpassen

```python
self.box_size = 10  # Größer = höhere Auflösung
self.border = 1     # Mindestrand
```

### Label-Text anpassen

In `_add_label()` Methode:
```python
text = f"Ventil {ventil_number:03d}"  # Aktuell
# Ändern zu:
text = f"V-{ventil_number}"           # Kürzer
```

## Beispiel-Output

```
Starte QR-Code Generierung für Ventile 1 bis 199
Ausgabeverzeichnis: /pfad/zu/QRCodes
Fortschritt: 10/199 QR-Codes erstellt
Fortschritt: 20/199 QR-Codes erstellt
...
Fortschritt: 190/199 QR-Codes erstellt

QR-Code Generierung abgeschlossen!
Erfolgreich: 199, Fehler: 0
Dateien gespeichert in: /pfad/zu/QRCodes
```

## Fehlerbehebung

### ModuleNotFoundError: No module named 'qrcode'
```bash
pip install qrcode[pil]
```

### ModuleNotFoundError: No module named 'PIL'
```bash
pip install pillow
```

### Font-Warnung
Wenn kein TrueType-Font gefunden wird, wird der Standard-Font verwendet.
QR-Codes funktionieren trotzdem, nur der Text sieht anders aus.

## Integration in den Workflow

1. **Einmalig**: QR-Codes mit diesem Tool generieren
2. **Einmalig**: QR-Codes ausdrucken und an Ventilen anbringen
3. **Täglich**: CSV aus DB Sauenplaner exportieren
4. **Täglich**: CSV in App importieren
5. **Arbeit**: Mit XR-Brille durch Stall gehen, Overlays erscheinen automatisch

## Weitere Informationen

Siehe auch:
- `IMPLEMENTATION_GUIDE.md` - Unity-Integration
- `CSV_EXAMPLES.md` - CSV-Format
- `FRAGENKATALOG.md` - Offene Fragen
