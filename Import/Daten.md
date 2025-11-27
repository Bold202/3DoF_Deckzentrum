# MusterPlan.csv - Spaltenübersicht

Die CSV-Datei aus dem "DB Sauenplaner" Export hat folgende Spaltenstruktur:

## Header-Zeile
```
"Stichtag";"Abf.";"Wochen bis";"Sau-Nr.";"[Datum]";"TK";"Bucht";"Bel.Datum";"TRT";"Gruppe";"Eber";"Wurf";"Umr.";"vorauss.";"Abferkelung";"index";"Prod.";"Ampel"
```

## Relevante Spalten für die D8-Planer XR App

Die Daten in der Tabelle die wir extrahieren befinden sich in Spalte 2, 4 und 5 (SauNr, VentilNr, BelegDatum):

| Spalte (1-indexiert) | CSV-Header | Beschreibung |
|---------------------|------------|--------------|
| Spalte 2 | "Abf." | **Sauen Nr.** - Klare eindeutige Nummer die die Sau als Ohrmarke trägt (z.B. "602") |
| Spalte 4 | "Sau-Nr." | **VentilNr** - Das Futterventil, welches laut Fütterungsplan mit der genannten Nr angesteuert wird. An einem Ventil können je nach Aufstallung zwischen 1 bis 10 Sauen untergebracht sein (z.B. "165") |
| Spalte 5 | [Dynamisches Datum] | **Belegdatum** - Das Datum an dem die Sau besamt wurde. Ab hier dauert es 118 Tage bis sie abferkelt. Anhand dieses Datums zum IST-Datum wird der aktuelle Trächtigkeitstag berechnet und das Ampelsystem gesteuert (z.B. "13.07.2025") |

## Zusätzliche Spalten (informativ)

| Spalte | CSV-Header | Beschreibung |
|--------|------------|--------------|
| 1 | "Stichtag" | Wochen bis zur Abferkelung |
| 3 | "Wochen bis" | Status-Indikator ("+" = tragend) |
| 6 | "TK" | Trächtigkeitstag (Tage seit Belegung) |
| 7 | "Bucht" | Gruppen-/Chargen-Nummer |
| 8 | "Bel.Datum" | Eber-Name (Vatertier) |

## Hinweise

- Die CSV verwendet Semikolon (`;`) als Trennzeichen
- Werte sind in Anführungszeichen eingeschlossen
- Das Datumsformat ist DD.MM.YYYY
