# Beispiel CSV-Dateien für D8-Planer XR

## Standard-Format (Semikolon-getrennt)

### beispiel_sauen.csv
```csv
Ventilnummer;Ohrmarkennummer;Deckdatum;Trächtigkeitsstatus;Bemerkungen
1;DE0123456;2024-01-15;Trächtig;
1;DE0123457;2024-01-20;Trächtig;Kontrolle morgen
1;DE0123458;2024-02-05;Unsicher;
2;DE0123459;2024-01-18;Trächtig;
2;DE0123460;2024-02-01;Trächtig;
3;DE0123461;2024-01-10;Trächtig;Besondere Beobachtung
4;DE0123462;2024-02-12;Unsicher;
5;DE0123463;2024-01-25;Trächtig;
5;DE0123464;2024-02-08;Trächtig;
5;DE0123465;2024-01-30;Trächtig;
```

### Speicherpfad auf Android:
```
/sdcard/Android/data/com.ihrefirma.d8planerxr/files/sauen.csv
```

oder intern:
```
/data/data/com.ihrefirma.d8planerxr/files/sauen.csv
```

## Erweiterte Formate

### beispiel_erweitert.csv
```csv
Ventilnummer;Ohrmarkennummer;Deckdatum;Trächtigkeitsstatus;Geburtsdatum;Gesundheitsstatus;Letzter_Check;Bemerkungen
1;DE0123456;2024-01-15;Trächtig;2022-05-10;Gut;2024-03-20;
1;DE0123457;2024-01-20;Trächtig;2021-08-15;Sehr gut;2024-03-21;Kontrolle morgen
2;DE0123459;2024-01-18;Trächtig;2022-03-20;Gut;2024-03-19;
3;DE0123461;2024-01-10;Trächtig;2023-01-05;Ausreichend;2024-03-18;Besondere Beobachtung
```

## Testdaten

### test_leer.csv
Leere Datei für Fehlerbehandlungs-Tests

### test_fehlerhafte_zeilen.csv
```csv
Ventilnummer;Ohrmarkennummer;Deckdatum;Trächtigkeitsstatus
1;DE0123456;2024-01-15;Trächtig
FEHLER;DE0123457;ungültig;Test
2;DE0123458;2024-02-01;
3;;2024-01-10;Trächtig
```

### test_sonderzeichen.csv
```csv
Ventilnummer;Ohrmarkennummer;Deckdatum;Trächtigkeitsstatus;Bemerkungen
1;DE0123456;2024-01-15;Trächtig;Müller's Sau
2;DE0123457;2024-01-20;Trächtig;Über 10kg übergewichtig
3;DE0123458;2024-02-05;Unsicher;Spezielle Diät: Äpfel & Möhren
```

### test_gross.csv
Generiert 500+ Zeilen für Performance-Tests
```csv
Ventilnummer;Ohrmarkennummer;Deckdatum;Trächtigkeitsstatus
1;DE0100001;2024-01-01;Trächtig
1;DE0100002;2024-01-02;Trächtig
...
[bis 500+ Einträge]
```

## Verschiedene Delimiter

### beispiel_komma.csv (Komma-getrennt)
```csv
Ventilnummer,Ohrmarkennummer,Deckdatum,Trächtigkeitsstatus
1,DE0123456,2024-01-15,Trächtig
2,DE0123457,2024-01-20,Trächtig
```

### beispiel_tab.csv (Tabulator-getrennt)
```csv
Ventilnummer	Ohrmarkennummer	Deckdatum	Trächtigkeitsstatus
1	DE0123456	2024-01-15	Trächtig
2	DE0123457	2024-01-20	Trächtig
```

## Encoding-Varianten

### beispiel_utf8.csv
Standard UTF-8 Encoding mit Umlauten

### beispiel_iso88591.csv
ISO-8859-1 (Latin-1) Encoding

### beispiel_windows1252.csv
Windows-1252 Encoding

## Verwendung in der App

1. CSV-Datei auf das Android-Gerät kopieren
2. App starten
3. Menü → "CSV importieren"
4. Datei auswählen oder Pfad eingeben
5. Delimiter und Encoding anpassen falls nötig
6. Spalten-Mapping prüfen
7. Importieren

## CSV-Export aus DB Sauenplaner

Anleitung für Export aus der Ursprungs-Software:

1. DB Sauenplaner öffnen
2. Gewünschte Daten filtern/auswählen
3. Export → CSV
4. Delimiter: Semikolon (;)
5. Encoding: UTF-8
6. Header: Ja
7. Dateiname: sauen.csv
8. Speichern

## Automatisierung

### Windows Batch-Script (csv_transfer.bat)
```batch
@echo off
echo Kopiere CSV auf Android-Gerät...
adb push sauen.csv /sdcard/Android/data/com.ihrefirma.d8planerxr/files/
echo Fertig!
pause
```

### Linux/Mac Shell-Script (csv_transfer.sh)
```bash
#!/bin/bash
echo "Kopiere CSV auf Android-Gerät..."
adb push sauen.csv /sdcard/Android/data/com.ihrefirma.d8planerxr/files/
echo "Fertig!"
```
