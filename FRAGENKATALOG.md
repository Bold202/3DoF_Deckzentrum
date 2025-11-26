# Fragenkatalog - 3DoF_Deckzentrum Projekt

> **Zweck**: Dieser Fragenkatalog sammelt alle offenen Fragen zum Projekt "D8-Planer XR" für das Viture Neckband Pro + Viture Luma Ultra.  
> **Anleitung**: Bitte beantworten Sie jede Frage so detailliert wie möglich. Fügen Sie Ihre Antworten direkt unter jeder Frage ein.

---

## 1. HARDWARE & GERÄTE

### 1.1 Viture Hardware
**Frage 1.1.1**: Welche exakte Android-Version läuft auf dem Viture Neckband Pro?
- [ ] Android 11
- [x ] Android 12
- [x ] Android 13
- [x ] Andere: neueste

**Antwort**: 


**Frage 1.1.2**: Gibt es ein offizielles Viture SDK oder Entwickler-Dokumentation? Falls ja, bitte Link oder Zugriffsinformationen angeben.

**Antwort**: https://static.viture.dev/external-file/sdk/viture_android_sdk_v1.0.7.tar.xz


**Frage 1.1.3**: Unterstützt das Viture System AR Foundation, ARCore oder ein proprietäres XR-Framework?

**Antwort**: ?


**Frage 1.1.4**: Welche Auflösung und Sichtfeld (Field of View) hat die Viture Luma Ultra Brille?

**Antwort**: siehe specs


**Frage 1.1.5**: Gibt es Einschränkungen bei der Akkulaufzeit des Neckbands bei kontinuierlicher AR-Nutzung? Wie lange sollte die App im Dauerbetrieb laufen können?

**Antwort**: neun, wird durch Powerbank permanent geladen


### 1.2 Bluetooth Label Printer
**Frage 1.2.1**: Welches genaue Modell des Bluetooth Label Printers soll verwendet werden?

**Antwort**: Katasymbol T50M Pro


**Frage 1.2.2**: Welche Druckauflösung und Label-Größe werden benötigt für die QR-Codes?
- Breite: 50 mm
- Höhe: 80 mm
- Auflösung: 800 DPI

**Antwort**: s.o.


**Frage 1.2.3**: Gibt es eine bevorzugte Druckprotokoll-Sprache (ESC/POS, ZPL, CPCL)?

**Antwort**: ?


**Frage 1.2.4**: Soll das QR-Code-Generator-Tool als separate Android-App oder als Desktop-Anwendung (Windows/Mac/Linux) entwickelt werden?

**Antwort**: Android


---

## 2. QR-CODE SYSTEM

### 2.1 QR-Code Spezifikationen
**Frage 2.1.1**: Welches QR-Code-Format soll verwendet werden?
- [ ] Standard QR Code (Version 1-40)
- [ ] Micro QR Code
- [ ] Andere: ___________

**Antwort**: Standart, 50x50. gerne sehr grob für gute Erkennung über Entfernung.


**Frage 2.1.2**: Welche Fehlerkorrektur-Stufe ist erforderlich für die Stallumgebung (Verschmutzung)?
- [ ] L (Low ~7%)
- [ ] M (Medium ~15%)
- [ ] Q (Quartile ~25%)
- [ ] H (High ~30%)

**Antwort**: ?


**Frage 2.1.3**: Welcher Inhalt soll im QR-Code kodiert werden?
- [ ] Nur Ventilnummer (z.B. "42")
- [ ] Präfix + Nummer (z.B. "VENTIL-042")
- [ ] JSON-Struktur: {"type": "ventil", "number": 42}
- [ ] Andere: ___________

**Antwort**: Ventilnummer


**Frage 2.1.4**: Wie viele Ventile gibt es maximal im Stall? (Sie erwähnten 1-199, ist das korrekt?)

**Antwort**: 1 - 199


**Frage 2.1.5**: Sollen die QR-Codes zusätzlich eine für Menschen lesbare Nummer enthalten (unter dem QR-Code gedruckt)?

**Antwort**: ja, 45x45 der QR, darunter die Nummer, weil ist ja 50x80


### 2.2 QR-Code Erkennung
**Frage 2.2.1**: Aus welcher Entfernung sollen die QR-Codes erkannt werden?
- Minimale Distanz: 200 cm
- Maximale Distanz: 600 cm
- Optimale Distanz: 400 cm

**Antwort**: s.o.


**Frage 2.2.2**: Wie sind die Lichtverhältnisse im Stall?
- [ ] Gut beleuchtet (Tageslicht + künstliche Beleuchtung)
- [x ] Gedämpft (nur künstliche Beleuchtung)
- [ ] Wechselhaft
- [ ] Andere: ___________

**Antwort**: 


**Frage 2.2.3**: Soll die App mehrere QR-Codes gleichzeitig erkennen und anzeigen können, oder immer nur den aktuell fokussierten Code?

**Antwort**: 


**Frage 2.2.4**: Wo genau befinden sich die Ventile (Wandhöhe, Abstand zueinander)?
- Höhe vom Boden: _____ cm
- Abstand zwischen Ventilen: _____ cm

**Antwort**: 


---

## 3. CSV-DATEI & DATENSTRUKTUR

### 3.1 CSV-Format
**Frage 3.1.1**: Können Sie eine Beispiel-CSV-Datei aus dem "DB Sauenplaner" bereitstellen? (Bitte mit anonymisierten/Beispieldaten)

**Antwort**: 


**Frage 3.1.2**: Welche Spalten enthält die CSV-Datei aktuell? Bitte alle Spaltennamen auflisten:
1. ___________
2. ___________
3. ___________
4. ___________ (etc.)

**Antwort**: 


**Frage 3.1.3**: Welche Spalten sind zwingend erforderlich für die Kernfunktion?
- Ventilnummer / Buchtennummer: ___________
- Ohrmarkennummer der Sau: ___________
- Trächtigkeitsstatus / Decktag: ___________
- Andere wichtige Spalten: ___________

**Antwort**: 


**Frage 3.1.4**: Welches Trennzeichen wird verwendet?
- [ ] Komma (,)
- [ ] Semikolon (;)
- [ ] Tabulator
- [ ] Andere: ___________

**Antwort**: 


**Frage 3.1.5**: Gibt es eine Header-Zeile (Spaltenüberschriften) in der CSV?
- [ ] Ja
- [ ] Nein

**Antwort**: 


**Frage 3.1.6**: Welches Zeichenkodierung wird verwendet (UTF-8, ISO-8859-1, Windows-1252)?

**Antwort**: 


**Frage 3.1.7**: Wie oft wird die CSV-Datei aktualisiert?
- [ ] Täglich
- [ ] Wöchentlich
- [ ] Bei Bedarf manuell
- [ ] Andere: ___________

**Antwort**: 


### 3.2 Datenlogik
**Frage 3.2.1**: Wie viele Sauen können maximal an einem Ventil stehen? (Sie erwähnten "bis zu 10")

**Antwort**: 


**Frage 3.2.2**: Wie wird die Zuordnung Sau → Ventil in der CSV dargestellt?
- [ ] Jede Zeile = eine Sau mit Ventilnummer-Spalte
- [ ] Gruppiert nach Ventilen
- [ ] Andere Struktur: ___________

**Antwort**: 


**Frage 3.2.3**: Können mehrere Sauen die gleiche Ohrmarkennummer haben? (Duplikatprüfung nötig?)

**Antwort**: 


**Frage 3.2.4**: Gibt es leere Ventile (ohne zugeordnete Sauen)? Wie sollen diese angezeigt werden?

**Antwort**: 


---

## 4. AMPELSYSTEM & TRÄCHTIGKEIT

### 4.1 Ampel-Logik
**Frage 4.1.1**: Welche Farben hat das Ampelsystem und was bedeuten sie?
- Grün: ___________
- Gelb/Orange: ___________
- Rot: ___________
- Weitere Farben?: ___________

**Antwort**: 


**Frage 4.1.2**: Basiert die Ampel-Farbe auf:
- [ ] Tage seit Deckung
- [ ] Trächtigkeitsstatus (bestätigt/unbestätigt)
- [ ] Gesundheitszustand
- [ ] Kombination aus mehreren Faktoren
- [ ] Andere: ___________

**Antwort**: 


**Frage 4.1.3**: Wenn basierend auf Tagen seit Deckung, welche Schwellwerte gelten?
- Grün: Tag _____ bis Tag _____
- Gelb: Tag _____ bis Tag _____
- Rot: Tag _____ bis Tag _____

**Antwort**: 


**Frage 4.1.4**: Wie wird das Deckdatum in der CSV gespeichert (Format)?
- [ ] DD.MM.YYYY (z.B. 15.03.2024)
- [ ] YYYY-MM-DD (z.B. 2024-03-15)
- [ ] Tage seit Epoche (Unix-Zeit)
- [ ] Andere: ___________

**Antwort**: 


**Frage 4.1.5**: Soll das System automatisch die heutige Tageszeit berücksichtigen oder nur Datumsvergleiche?

**Antwort**: 


### 4.2 Anzeigedetails
**Frage 4.2.6**: Welche Informationen sollen für jede Sau angezeigt werden?
- [ ] Ohrmarkennummer
- [ ] Ampelfarbe (Icon/Hintergrund)
- [ ] Tage seit Deckung
- [ ] Deckdatum
- [ ] Andere: ___________

**Antwort**: 


**Frage 4.2.7**: In welcher Reihenfolge sollen die Sauen an einem Ventil sortiert werden?
- [ ] Alphabetisch nach Ohrmarkennummer
- [ ] Nach Deckdatum (älteste zuerst)
- [ ] Nach Ampelstatus (Rot zuerst)
- [ ] Andere: ___________

**Antwort**: 


**Frage 4.2.8**: Sollen historische Daten gespeichert werden (z.B. vergangene Abferkelungen)?

**Antwort**: 


---

## 5. BENUTZEROBERFLÄCHE & UX

### 5.1 AR-Overlay
**Frage 5.1.1**: Wie groß soll das virtuelle Overlay unter jedem Ventil sein (in cm oder Pixel)?
- Breite: _____ cm / px
- Höhe: _____ cm / px

**Antwort**: 


**Frage 5.1.2**: Soll das Overlay immer sichtbar sein oder nur beim direkten Anschauen des Ventils (Gaze-basiert)?

**Antwort**: 


**Frage 5.1.3**: Wie weit unter dem QR-Code soll das Overlay angezeigt werden?
- Abstand: _____ cm

**Antwort**: 


**Frage 5.1.4**: Soll das Overlay "einfrieren" (statisch an der Position bleiben) oder dem Kopf folgen?
- [ ] Statisch im Raum (6DOF Spatial Anchor)
- [ ] Folgt dem Kopf (3DOF)
- [ ] Hybrid: ___________

**Antwort**: 


**Frage 5.1.5**: Welche Schriftgröße ist aus der Arbeitsposition gut lesbar?
- Mindest-Schriftgröße: _____ pt/cm

**Antwort**: 


### 5.2 Hauptmenü & Navigation
**Frage 5.2.1**: Soll es ein Hauptmenü geben? Falls ja, welche Funktionen?
- [ ] CSV-Datei neu importieren
- [ ] CSV-Spalten konfigurieren (Ihre Anforderung)
- [ ] Statistiken anzeigen (z.B. Anzahl Sauen pro Status)
- [ ] Einstellungen (Schriftgröße, Farben anpassen)
- [ ] Über/Hilfe
- [ ] Andere: ___________

**Antwort**: 


**Frage 5.2.2**: Wie soll das Menü aufgerufen werden?
- [ ] Gesten (z.B. Wischbewegung)
- [ ] Sprachbefehl
- [ ] Fester Button im Sichtfeld
- [ ] QR-Code für Menü
- [ ] Andere: ___________

**Antwort**: 


**Frage 5.2.3**: Soll es ein "Virtuelles Deckzentrum" geben (statische Gesamtansicht aller Ventile)? Was soll dort angezeigt werden?

**Antwort**: 


### 5.3 CSV-Spalten-Menü (Ihre spezifische Anforderung)
**Frage 5.3.1**: Sollen Nutzer nur die Anzeige-Spalten konfigurieren oder auch die Datenlogik (welche Spalte = Ventilnummer, welche = Deckdatum)?

**Antwort**: 


**Frage 5.3.2**: Soll die Spaltenkonfiguration pro CSV-Datei gespeichert werden (CSV-Profil) oder global für die App?

**Antwort**: 


**Frage 5.3.3**: Sollen Nutzer Spalten-Aliase definieren können? (z.B. "Ear_Tag" → "Ohrmarke")

**Antwort**: 


**Frage 5.3.4**: Müssen Nutzer Datentypen festlegen können (Text, Zahl, Datum)?

**Antwort**: 


**Frage 5.3.5**: Soll es Vorlagen geben für häufige CSV-Strukturen?

**Antwort**: 


---

## 6. DATENIMPORT & VERWALTUNG

### 6.1 CSV-Import
**Frage 6.1.1**: Wo soll die CSV-Datei abgelegt werden?
- [ ] Interner Speicher: /Android/data/com.ihr.app/files/
- [ ] SD-Karte
- [ ] App-spezifischer Ordner
- [ ] Freie Wahl durch Nutzer (File Picker)

**Antwort**: 


**Frage 6.1.2**: Wie soll die Datei heißen (fester Name oder variabel)?
- Fester Name: ___________
- [ ] Beliebiger Name, App erkennt .csv automatisch

**Antwort**: 


**Frage 6.1.3**: Was soll passieren, wenn die CSV-Datei fehlt oder beschädigt ist?
- [ ] Fehlermeldung anzeigen
- [ ] Notfall-Modus (letzte gecachte Daten)
- [ ] Leeres Overlay
- [ ] Andere: ___________

**Antwort**: 


**Frage 6.1.4**: Soll die App automatisch nach CSV-Updates suchen oder muss der Nutzer manuell aktualisieren?

**Antwort**: 


**Frage 6.1.5**: Soll eine Backup-Funktion für alte CSV-Versionen existieren?

**Antwort**: 


### 6.2 Offline-Funktionalität
**Frage 6.2.1**: Muss die App **komplett** ohne Internet funktionieren?
- [ ] Ja, niemals Internet nötig
- [ ] Nein, einmalige Ersteinrichtung online möglich
- [ ] Andere: ___________

**Antwort**: 


**Frage 6.2.2**: Gibt es eine Möglichkeit zur Datensynchronisation (USB, Bluetooth, WiFi Direct)?

**Antwort**: 


---

## 7. TECHNISCHE DETAILS

### 7.1 Unity & Entwicklung
**Frage 7.1.1**: Welche Unity-Version soll verwendet werden (LTS empfohlen)?
- [ ] Unity 2021 LTS
- [ ] Unity 2022 LTS
- [ ] Unity 2023 LTS
- [ ] Andere: ___________

**Antwort**: 


**Frage 7.1.2**: Soll die App in C# (Unity) oder nativem Android (Kotlin/Java) entwickelt werden?

**Antwort**: 


**Frage 7.1.3**: Gibt es Vorgaben zur Dateigröße der APK (z.B. max. 100 MB)?

**Antwort**: 


**Frage 7.1.4**: Soll die App mehrsprachig sein (Deutsch/Englisch)?

**Antwort**: 


### 7.2 Performance & Optimierung
**Frage 7.2.1**: Wie viele QR-Codes können gleichzeitig im Sichtfeld sein (maximale Anzahl)?

**Antwort**: 


**Frage 7.2.2**: Wie oft soll die QR-Code-Erkennung pro Sekunde laufen (Frame Rate für Detection)?
- [ ] Kontinuierlich (jedes Frame)
- [ ] 10 FPS
- [ ] 5 FPS
- [ ] Andere: ___________

**Antwort**: 


**Frage 7.2.3**: Soll die App im Hintergrund laufen können oder immer im Vordergrund?

**Antwort**: 


### 7.3 Berechtigungen
**Frage 7.3.1**: Welche Android-Berechtigungen werden benötigt?
- [ ] Kamera (für QR-Erkennung)
- [ ] Dateizugriff (für CSV)
- [ ] Bluetooth (für Drucker)
- [ ] Andere: ___________

**Antwort**: 


---

## 8. QUALITÄTSSICHERUNG & TESTS

### 8.1 Testszenarien
**Frage 8.1.1**: Sollen Mock-Daten für Tests erstellt werden?

**Antwort**: 


**Frage 8.1.2**: Welche Edge Cases sollen getestet werden?
- [ ] Leere CSV
- [ ] CSV mit 500+ Zeilen
- [ ] Korrupte Zeilen
- [ ] Sonderzeichen (Umlaute)
- [ ] Fehlende Spalten
- [ ] Andere: ___________

**Antwort**: 


**Frage 8.1.3**: Gibt es Akzeptanzkriterien? (z.B. "QR-Code muss innerhalb 2 Sekunden erkannt werden")

**Antwort**: 


### 8.2 Fehlerbehandlung
**Frage 8.2.1**: Wie sollen Fehler dem Nutzer kommuniziert werden?
- [ ] Toast-Benachrichtigungen
- [ ] Overlay-Fehlermeldung
- [ ] Logdatei
- [ ] Andere: ___________

**Antwort**: 


**Frage 8.2.2**: Soll es ein Debug-Modus geben (Logging, Diagnoseanzeige)?

**Antwort**: 


---

## 9. DEPLOYMENT & WARTUNG

### 9.1 Installation
**Frage 9.1.1**: Wie soll die APK verteilt werden?
- [ ] Direkte Installation via ADB
- [ ] Google Play Store (Closed Beta)
- [ ] Internes Download-Portal
- [ ] USB-Transfer
- [ ] Andere: ___________

**Antwort**: 


**Frage 9.1.2**: Soll die App signiert werden (Release Build)?

**Antwort**: 


### 9.2 Updates
**Frage 9.2.1**: Wie sollen Updates verteilt werden?

**Antwort**: 


**Frage 9.2.2**: Soll eine Versionsnummer sichtbar sein in der App?

**Antwort**: 


---

## 10. ZUSÄTZLICHE FEATURES

### 10.1 Erweiterte Funktionen
**Frage 10.1.1**: Sollen Nutzer Notizen zu einzelnen Sauen machen können?

**Antwort**: 


**Frage 10.1.2**: Soll es Alarme/Erinnerungen geben (z.B. "Sau X muss kontrolliert werden")?

**Antwort**: 


**Frage 10.1.3**: Soll es eine Export-Funktion geben (z.B. Nutzungsstatistiken, Logs)?

**Antwort**: 


**Frage 10.1.4**: Sollen mehrere Nutzer/Profile unterstützt werden?

**Antwort**: 


### 10.2 Sicherheit
**Frage 10.2.1**: Gibt es Datenschutz-Anforderungen (DSGVO)?

**Antwort**: 


**Frage 10.2.2**: Soll die App mit Passwort/PIN geschützt werden können?

**Antwort**: 


---

## 11. PROJEKTZEITPLAN & RESSOURCEN

**Frage 11.1**: Wann soll die App einsatzbereit sein? (Zeitrahmen)

**Antwort**: 


**Frage 11.2**: Gibt es bereits Code/Prototypen oder startet das Projekt von Null?

**Antwort**: 


**Frage 11.3**: Wer wird die App testen (Beta-Tester im Stall)?

**Antwort**: 


**Frage 11.4**: Gibt es ein Budget für Lizenzen (Unity Pro, Plugins, etc.)?

**Antwort**: 


---

## 12. SONSTIGES

**Frage 12.1**: Gibt es weitere spezielle Anforderungen, die noch nicht erwähnt wurden?

**Antwort**: 


**Frage 12.2**: Gibt es bestehende Systeme/Datenbanken, mit denen die App integriert werden soll?

**Antwort**: 


**Frage 12.3**: Welche Personen sollen Zugriff auf dieses Projekt haben (Entwickler, Stakeholder)?

**Antwort**: 


---

## ABSCHLUSS

**Prioritäten**: Bitte nummerieren Sie die 5 wichtigsten Features/Anforderungen nach Priorität:
1. ___________
2. ___________
3. ___________
4. ___________
5. ___________

**Anmerkungen**: 


---

*Vielen Dank für Ihre detaillierten Antworten! Diese Informationen sind essentiell für die erfolgreiche Entwicklung der D8-Planer XR App.*

**Erstellt am**: 2025-11-26  
**Version**: 1.0
