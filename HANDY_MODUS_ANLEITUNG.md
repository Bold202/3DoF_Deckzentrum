# Handy-Modus Benutzerhandbuch

> **D8-Planer XR auf dem Smartphone nutzen - ohne VR-Brille**

## 📱 Was ist der Handy-Modus?

Der Handy-Modus ermöglicht es, die D8-Planer XR App auf einem normalen Android-Smartphone zu verwenden, **ohne** die Viture-Brille oder anderes VR-Equipment.

### Vorteile des Handy-Modus

✅ **Kein VR-Equipment nötig** - Nur Smartphone erforderlich  
✅ **Einfachere Bedienung** - Touch-Steuerung statt Kopfbewegung  
✅ **Flexibler Einsatz** - Überall nutzbar, auch außerhalb des Stalls  
✅ **Schneller Check** - Ideal für kurze Kontrollen einzelner Ventile  
✅ **Geringere Kosten** - Nutzt vorhandene Smartphones

### Einschränkungen im Vergleich zu VR

⚠️ **Kein Virtuelles Deckzentrum** - Nur QR-basierte Einzelansichten  
⚠️ **Eingeschränktes Spatial Anchoring** - Overlay-Position weniger stabil  
⚠️ **Keine Freihändige Nutzung** - Smartphone muss gehalten werden  
⚠️ **Ein QR-Code gleichzeitig** - Nicht mehrere Overlays parallel

---

## 🚀 Schnellstart

### 1. App installieren

APK auf Smartphone installieren (siehe [APK_BUILD_ANLEITUNG.md](APK_BUILD_ANLEITUNG.md))

### 2. Modus einstellen

Die App erkennt automatisch, dass keine VR-Hardware verbunden ist und startet im Handy-Modus.

**Manueller Wechsel (falls nötig):**
1. App öffnen
2. Menü-Button (☰ oben rechts) tippen
3. "Einstellungen" → "Anzeigemodus"
4. "📱 Handy-Modus" auswählen

### 3. QR-Code scannen

1. Kamera auf QR-Code am Ventil richten
2. QR-Code wird automatisch erkannt
3. Overlay mit Sau-Informationen erscheint
4. Smartphone ca. 20-50 cm vom Code entfernt halten

### 4. Informationen ablesen

- **Ventilnummer** - Oben im Overlay
- **Sauen-Liste** - Mit Ampelfarben sortiert
- **Ohrmarkennummer** - Pro Sau
- **Tage seit Deckung** - Automatisch berechnet
- **Trächtigkeitsstatus** - Trächtig/Unsicher/etc.

---

## 📸 QR-Code Scanning Best Practices

### Optimale Scan-Bedingungen

✅ **Beleuchtung:**
- Tageslicht oder helle Stallbeleuchtung
- Keine starken Schatten auf dem QR-Code
- Kein direktes Gegenlicht

✅ **Abstand:**
- 20-50 cm vom QR-Code
- Code sollte ca. 1/3 des Bildschirms füllen

✅ **Winkel:**
- Frontal zum QR-Code (nicht schräg)
- Kamera parallel zur Code-Fläche

✅ **Stabilität:**
- Smartphone ruhig halten
- Bei Bedarf an Wand/Ventil abstützen

### Troubleshooting Scanning

**Problem: QR-Code wird nicht erkannt**

**Lösung 1: Sauberkeit prüfen**
- QR-Code mit trockenem Tuch reinigen
- Kameralinse säubern

**Lösung 2: Beleuchtung verbessern**
- Taschenlampe nutzen (aber nicht direkt auf Code)
- In helleren Bereich gehen

**Lösung 3: Abstand anpassen**
- Näher rangehen (ab 15 cm)
- Oder weiter weg (bis 80 cm)

**Lösung 4: Kamera-Berechtigung**
- Android-Einstellungen → Apps → D8-Planer XR
- Berechtigungen → Kamera → "Erlauben"

---

## 👆 Touch-Steuerung

### Grundlegende Gesten

#### Tippen (Single Tap)
- **Auf Sau:** Details anzeigen
- **Auf Overlay:** Menü öffnen
- **Neben Overlay:** Zurück

#### Wischen (Swipe)
- **Nach links/rechts:** Zwischen Sauen wechseln
- **Nach oben:** Mehr Informationen
- **Nach unten:** Overlay ausblenden

#### Zwei-Finger-Gesten
- **Pinch (Zusammenziehen):** Overlay verkleinern
- **Spread (Auseinanderziehen):** Overlay vergrößern
- **Zwei-Finger-Drag:** Overlay verschieben

#### Lange drücken (Long Press)
- **Auf Sau:** Kontext-Menü
- **Auf Ventilnummer:** Ventil-Optionen

### Touch-Menü

**Hauptmenü (☰):**
- 📁 CSV importieren
- 🔄 Daten neu laden
- ⚙️ Einstellungen
- ℹ️ Über die App
- 🚪 Beenden

**Kontext-Menü (auf Sau):**
- 📝 Details anzeigen
- 📋 Historie
- 🖨️ Drucken (falls Drucker verbunden)

---

## 💾 CSV-Daten importieren

### Methode 1: Manuell kopieren

1. **USB-Verbindung:**
   - Smartphone mit PC verbinden
   - "Dateiübertragung" wählen

2. **Datei ablegen:**
   ```
   Interner Speicher/Android/data/com.IhreFirma.D8PlanerXR/files/ImportDZ.csv
   ```

3. **In App importieren:**
   - Menü → "CSV importieren"
   - Datei auswählen
   - Import bestätigen

### Methode 2: Cloud-Import (optional)

Falls implementiert:
- Menü → "Cloud-Import"
- Google Drive/Dropbox verbinden
- Datei auswählen

### Methode 3: WiFi-Transfer (optional)

Falls implementiert:
- Menü → "WiFi-Import"
- Gleiche WiFi wie PC
- Datei hochladen

---

## ⚙️ Einstellungen

### Anzeigemodus

**Auto (🔄):**
- Erkennt automatisch Hardware
- Empfohlen für Standard-Nutzung

**VR-Modus (🕶️):**
- Für Viture-Brille
- Alle Features verfügbar

**Handy-Modus (📱):**
- Für Smartphone
- Touch-optimiert
- Batterie-schonend

### Scan-Einstellungen

**Scan-Intervall:**
- Standard: 0.5 Sekunden
- Schneller: 0.3 Sekunden (höherer Batterieverbrauch)
- Langsamer: 1.0 Sekunden (Batterie-schonend)

**Multi-Scan:**
- An: Mehrere Codes gleichzeitig (nur VR)
- Aus: Nur ein Code (Handy-Standard)

### Overlay-Einstellungen

**Größe:**
- Klein: Kompakte Ansicht
- Mittel: Standard
- Groß: Gut lesbar aus Distanz

**Transparenz:**
- 0%: Undurchsichtig
- 50%: Halb-transparent (Standard)
- 80%: Sehr transparent

**Sortierung:**
- Rot zuerst (Standard)
- Nach Ohrmarkennummer
- Nach Deckdatum
- Alphabetisch

---

## 🔋 Batterie-Optimierung

### Empfohlene Einstellungen für langen Betrieb

```
Scan-Intervall: 1.0 Sekunden
Bildschirmhelligkeit: 50-70%
Overlay-Größe: Mittel
Multi-Scan: Aus
AR-Plane-Detection: Aus
```

### Weitere Tipps

✅ **Flugmodus + WiFi:**
- Mobile Daten aus
- WiFi nur wenn nötig
- Bluetooth aus

✅ **Bildschirm-Timeout:**
- Auf 30 Sekunden stellen
- Screen dimmt automatisch

✅ **Hintergrund-Apps:**
- Andere Apps schließen
- RAM freihalten

### Geschätzte Laufzeit

**Typical Use (Standard-Einstellungen):**
- 3-4 Stunden kontinuierlicher Betrieb
- 6-8 Stunden intermittierender Betrieb

**Power-Saving Mode:**
- 5-6 Stunden kontinuierlicher Betrieb
- 10-12 Stunden intermittierender Betrieb

**Mit Powerbank (10.000 mAh):**
- Ganzer Arbeitstag (8+ Stunden)

---

## 🎯 Typische Workflows

### Workflow 1: Einzelventil-Kontrolle

1. App öffnen
2. Zu Ventil gehen
3. QR-Code scannen
4. Sau-Status prüfen
5. Ggf. Notizen machen
6. Weiter zum nächsten Ventil

**Zeit pro Ventil:** ~10-20 Sekunden

### Workflow 2: Reihen-Kontrolle

1. App öffnen, Screen-Timeout erhöhen
2. Systematisch Reihe abgehen
3. Nur rote/gelbe Ventile genauer prüfen
4. Grüne Ventile nur kurz scannen
5. Notizen für Auffälligkeiten

**Zeit pro Reihe (10 Ventile):** ~3-5 Minuten

### Workflow 3: Gezielte Sau-Suche

1. Menü → "Sau suchen"
2. Ohrmarkennummer eingeben
3. Ventilnummer wird angezeigt
4. Zu Ventil gehen
5. Details prüfen

**Zeit:** ~1-2 Minuten

### Workflow 4: Daten-Export

1. Kontrolle durchführen
2. Notizen/Änderungen in App
3. Menü → "Daten exportieren"
4. CSV speichern
5. Am PC weiterverarbeiten

---

## 📊 Unterschiede VR vs. Handy

| Feature | VR-Modus 🕶️ | Handy-Modus 📱 |
|---------|-------------|----------------|
| **QR-Code Scanning** | ✅ Multi (5 gleichzeitig) | ✅ Single (1 gleichzeitig) |
| **Overlay-Anzeige** | ✅ 3D-positioniert | ✅ 2D-Overlay |
| **Spatial Anchoring** | ✅ Stabil im Raum | ⚠️ Folgt Kamera |
| **Virtuelles Deckzentrum** | ✅ 3D-Übersicht | ❌ Nicht verfügbar |
| **Kopf-Tracking** | ✅ 6DoF | ❌ Nur Kamera |
| **Freihändige Bedienung** | ✅ Ja | ❌ Smartphone in Hand |
| **Touch-Steuerung** | ❌ Nein | ✅ Ja |
| **Batterie-Laufzeit** | ~2-3h | ~4-6h |
| **Setup-Komplexität** | Hoch | Niedrig |
| **Kosten** | Hoch (Viture) | Niedrig (Smartphone) |

---

## 🆘 Häufige Probleme

### Problem: App startet im VR-Modus obwohl kein VR-Gerät

**Lösung:**
```
1. Menü → Einstellungen → Anzeigemodus
2. "Handy-Modus" auswählen
3. App neu starten
```

### Problem: Overlay verschwindet sofort

**Lösung:**
- QR-Code im Bild behalten
- Timeout in Einstellungen erhöhen
- Scan-Intervall verringern

### Problem: Touch-Gesten funktionieren nicht

**Lösung:**
- Handy-Modus aktiv prüfen
- Display-Schutzfolie entfernen/tauschen
- Touch-Empfindlichkeit in Android erhöhen

### Problem: Schlechte Performance

**Lösung:**
```
1. Andere Apps schließen
2. Smartphone neu starten
3. Scan-Intervall erhöhen
4. Overlay-Größe reduzieren
5. AR-Features reduzieren
```

### Problem: CSV wird nicht gefunden

**Lösung:**
```
Korrekte Pfade:
Android 11+: /storage/emulated/0/Android/data/com.IhreFirma.D8PlanerXR/files/
Android 10-: /storage/emulated/0/D8PlanerXR/

Berechtigung prüfen:
Einstellungen → Apps → D8-Planer → Berechtigungen → Speicher
```

---

## 🎓 Schulung & Einarbeitung

### Für neue Mitarbeiter (15 Minuten)

**Phase 1: Grundlagen (5 min)**
- App öffnen und navigieren
- Einen QR-Code scannen
- Ampelfarben verstehen

**Phase 2: Bedienung (5 min)**
- Touch-Gesten üben
- Menü erkunden
- Einstellungen anpassen

**Phase 3: Praxis (5 min)**
- 5-10 Ventile scannen
- Nach spezifischer Sau suchen
- CSV importieren

### Training-Checkliste

- [ ] App installiert und gestartet
- [ ] Kamera-Berechtigung erteilt
- [ ] QR-Code erfolgreich gescannt
- [ ] Overlay kann bedient werden
- [ ] Ampelfarben verstanden
- [ ] Touch-Gesten bekannt
- [ ] CSV importiert
- [ ] Ventil-Reihe kontrolliert

---

## 📞 Support

### Selbsthilfe

1. **Diese Anleitung lesen**
2. **FAQ prüfen** (siehe unten)
3. **Einstellungen zurücksetzen**
4. **App neu installieren**

### FAQ

**Q: Kann ich Handy- und VR-Modus gleichzeitig nutzen?**  
A: Nein, nur ein Modus ist aktiv. Wechsel jederzeit möglich.

**Q: Funktioniert die App offline?**  
A: Ja, komplett offline. Keine Internetverbindung nötig.

**Q: Wie oft CSV aktualisieren?**  
A: Täglich vor Arbeitsbeginn empfohlen.

**Q: Welche Smartphones werden unterstützt?**  
A: Android 7.0+, ARCore-kompatibel. Liste: https://developers.google.com/ar/devices

**Q: Kann ich eigene QR-Codes erstellen?**  
A: Ja, mit dem QR-Generator Tool (siehe Tools/README.md)

---

## 📝 Changelog

### Version 1.1.0 (2025-11-26)
- ✨ Handy-Modus hinzugefügt
- ✨ Touch-Steuerung implementiert
- ✨ Automatische Modus-Erkennung
- ✨ Optimierte Batterie-Nutzung
- 🐛 Diverse Bugfixes

### Version 1.0.0
- 🎉 Initiale Version
- VR-Modus für Viture

---

**Viel Erfolg mit dem Handy-Modus!** 📱✨

Bei Fragen: support@ihre-firma.de

---

*Version: 1.0 | Letzte Aktualisierung: 2025-11-26*
