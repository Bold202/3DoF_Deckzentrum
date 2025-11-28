using System;
using System.Collections.Generic;
using UnityEngine;

namespace D8PlanerXR.Data
{
    /// <summary>
    /// Konfigurationsklasse für CSV-Spalten
    /// Speichert Spaltendefinitionen und deren Mapping
    /// </summary>
    [Serializable]
    public class CSVColumnConfig
    {
        [Serializable]
        public class ColumnDefinition
        {
            public string originalName;      // Originaler Spaltenname aus CSV
            public string displayName;       // Anzeigename (kann vom Nutzer geändert werden)
            public ColumnType columnType;    // Datentyp der Spalte
            public bool isVisible;           // Sichtbarkeit in der Anzeige
            public int displayOrder;         // Anzeigereihenfolge
            public ColumnRole role;          // Rolle/Funktion der Spalte
            
            public ColumnDefinition()
            {
                originalName = "";
                displayName = "";
                columnType = ColumnType.Text;
                isVisible = true;
                displayOrder = 0;
                role = ColumnRole.None;
            }

            public ColumnDefinition(string name, ColumnType type = ColumnType.Text, ColumnRole role = ColumnRole.None)
            {
                this.originalName = name;
                this.displayName = name;
                this.columnType = type;
                this.isVisible = true;
                this.displayOrder = 0;
                this.role = role;
            }
        }

        /// <summary>
        /// Datentypen für Spalten
        /// </summary>
        public enum ColumnType
        {
            Text,           // Textdaten
            Number,         // Numerische Daten
            Date,           // Datumswerte
            Boolean,        // Ja/Nein Werte
            Custom          // Benutzerdefiniert
        }

        /// <summary>
        /// Rollen für Spalten (funktionale Zuordnung)
        /// </summary>
        public enum ColumnRole
        {
            None,               // Keine spezielle Rolle
            VentilNumber,       // Ventilnummer (Primary Key für Zuordnung)
            EarTagNumber,       // Ohrmarkennummer der Sau
            MatingDate,         // Deckdatum
            PregnancyStatus,    // Trächtigkeitsstatus
            BirthDate,          // Geburtsdatum
            HealthStatus,       // Gesundheitszustand
            Notes,              // Bemerkungen
            Custom              // Benutzerdefinierte Rolle
        }

        // Liste aller Spaltendefinitionen
        public List<ColumnDefinition> columns = new List<ColumnDefinition>();
        
        // CSV-Einstellungen
        public char delimiter = ';';
        public bool hasHeader = true;
        public string encoding = "UTF-8";
        
        // Metadaten
        public string configName = "Default";
        public string lastModified;
        public string csvFilePath = "";

        /// <summary>
        /// Gibt die Spalte mit der angegebenen Rolle zurück
        /// </summary>
        public ColumnDefinition GetColumnByRole(ColumnRole role)
        {
            return columns.Find(c => c.role == role);
        }

        /// <summary>
        /// Gibt alle sichtbaren Spalten in Anzeigereihenfolge zurück
        /// </summary>
        public List<ColumnDefinition> GetVisibleColumns()
        {
            List<ColumnDefinition> visible = columns.FindAll(c => c.isVisible);
            visible.Sort((a, b) => a.displayOrder.CompareTo(b.displayOrder));
            return visible;
        }

        /// <summary>
        /// Fügt eine neue Spalte hinzu
        /// </summary>
        public void AddColumn(string name, ColumnType type = ColumnType.Text, ColumnRole role = ColumnRole.None)
        {
            ColumnDefinition newCol = new ColumnDefinition(name, type, role);
            newCol.displayOrder = columns.Count;
            columns.Add(newCol);
        }

        /// <summary>
        /// Entfernt eine Spalte
        /// </summary>
        public bool RemoveColumn(string originalName)
        {
            int index = columns.FindIndex(c => c.originalName == originalName);
            if (index >= 0)
            {
                columns.RemoveAt(index);
                ReorderColumns();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Benennt eine Spalte um
        /// </summary>
        public bool RenameColumn(string originalName, string newDisplayName)
        {
            ColumnDefinition col = columns.Find(c => c.originalName == originalName);
            if (col != null)
            {
                col.displayName = newDisplayName;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Ändert die Reihenfolge einer Spalte
        /// </summary>
        public bool ReorderColumn(string originalName, int newOrder)
        {
            ColumnDefinition col = columns.Find(c => c.originalName == originalName);
            if (col != null && newOrder >= 0 && newOrder < columns.Count)
            {
                int oldOrder = col.displayOrder;
                col.displayOrder = newOrder;
                
                // Andere Spalten anpassen
                foreach (var c in columns)
                {
                    if (c != col)
                    {
                        if (newOrder < oldOrder)
                        {
                            if (c.displayOrder >= newOrder && c.displayOrder < oldOrder)
                                c.displayOrder++;
                        }
                        else
                        {
                            if (c.displayOrder > oldOrder && c.displayOrder <= newOrder)
                                c.displayOrder--;
                        }
                    }
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Setzt die Anzeigereihenfolge neu (nach Lücken)
        /// </summary>
        private void ReorderColumns()
        {
            columns.Sort((a, b) => a.displayOrder.CompareTo(b.displayOrder));
            for (int i = 0; i < columns.Count; i++)
            {
                columns[i].displayOrder = i;
            }
        }

        /// <summary>
        /// Erstellt eine Standardkonfiguration
        /// </summary>
        public static CSVColumnConfig CreateDefault()
        {
            CSVColumnConfig config = new CSVColumnConfig();
            config.configName = "Default";
            config.delimiter = ';';
            config.hasHeader = true;
            config.encoding = "UTF-8";
            
            // Standard-Spalten
            config.AddColumn("Ventilnummer", ColumnType.Number, ColumnRole.VentilNumber);
            config.AddColumn("Ohrmarkennummer", ColumnType.Text, ColumnRole.EarTagNumber);
            config.AddColumn("Deckdatum", ColumnType.Date, ColumnRole.MatingDate);
            config.AddColumn("Trächtigkeitsstatus", ColumnType.Text, ColumnRole.PregnancyStatus);
            config.AddColumn("Bemerkungen", ColumnType.Text, ColumnRole.Notes);
            
            config.lastModified = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            return config;
        }

        /// <summary>
        /// Erstellt eine Konfiguration für MusterPlan.csv (DB Sauenplaner Export)
        /// Basierend auf der Referenzdatei in Import/MusterPlan.csv
        /// 
        /// CSV Format Analyse:
        /// Header: "Stichtag";"Abf.";"Wochen bis";"Sau-Nr.";"[Datum]";"TK";"Bucht";"Bel.Datum";"TRT";"Gruppe";"Eber";"Wurf";"Umr.";"vorauss.";"Abferkelung";"index";"Prod.";"Ampel"
        /// Daten:  " -3";"     602";"+";"165   ";"13.07.2025";" 134";"202529";"    M 88";"11";"  ";" ";"";"05.11.2025";"29.6"
        /// 
        /// Wichtige Spalten für die App:
        /// - Index 1 ("Abf.") = Ohrmarkennummer (z.B. "602")
        /// - Index 3 ("Sau-Nr.") = Ventil/Bucht-Nummer für Zuordnung (z.B. "165")
        /// - Index 4 (dynamisches Datum) = Belegdatum (z.B. "13.07.2025")
        /// - Index 5 ("TK") = Tage trächtig (z.B. "134")
        /// - Index 6 ("Bucht") = Gruppen-Nummer (z.B. "202529")
        /// </summary>
        public static CSVColumnConfig CreateMusterPlanConfig()
        {
            CSVColumnConfig config = new CSVColumnConfig();
            config.configName = "MusterPlan";
            config.delimiter = ';';
            config.hasHeader = true;
            config.encoding = "UTF-8";
            
            // Index 0: Stichtag (Wochen bis Abferkelung relative Angabe)
            var stichtag = new ColumnDefinition("Stichtag", ColumnType.Text, ColumnRole.None);
            stichtag.displayName = "Stichtag";
            stichtag.isVisible = false;
            stichtag.displayOrder = 0;
            config.columns.Add(stichtag);
            
            // Index 1: Abf. = Ohrmarkennummer (WICHTIG!)
            var ohrmarke = new ColumnDefinition("Abf.", ColumnType.Text, ColumnRole.EarTagNumber);
            ohrmarke.displayName = "Ohrmarke";
            ohrmarke.isVisible = true;
            ohrmarke.displayOrder = 1;
            config.columns.Add(ohrmarke);
            
            // Index 2: Wochen bis = Status-Indikator (+ oder leer)
            var wochenBis = new ColumnDefinition("Wochen bis", ColumnType.Text, ColumnRole.None);
            wochenBis.displayName = "Status";
            wochenBis.isVisible = false;
            wochenBis.displayOrder = 2;
            config.columns.Add(wochenBis);
            
            // Index 3: Sau-Nr. = Ventil/Bucht-Nummer für QR-Code Zuordnung (WICHTIG!)
            var sauNr = new ColumnDefinition("Sau-Nr.", ColumnType.Number, ColumnRole.VentilNumber);
            sauNr.displayName = "Bucht (Ventil)";
            sauNr.isVisible = true;
            sauNr.displayOrder = 3;
            config.columns.Add(sauNr);
            
            // Index 4: Dynamisches Datum = Belegdatum (WICHTIG!)
            // HINWEIS: Der CSV-Header enthält ein dynamisches Datum (z.B. "24.11.2025")
            // das sich bei jedem Export ändert.
            var belegdatum = new ColumnDefinition("Belegdatum_Spalte5", ColumnType.Date, ColumnRole.MatingDate);
            belegdatum.displayName = "Belegdatum";
            belegdatum.isVisible = true;
            belegdatum.displayOrder = 4;
            config.columns.Add(belegdatum);
            
            // Index 5: TK = Tage trächtig (WICHTIG!)
            var tk = new ColumnDefinition("TK", ColumnType.Number, ColumnRole.None);
            tk.displayName = "Tage trächtig";
            tk.isVisible = true;
            tk.displayOrder = 5;
            config.columns.Add(tk);
            
            // Index 6: Bucht = Gruppen-/Chargen-Nummer
            var bucht = new ColumnDefinition("Bucht", ColumnType.Text, ColumnRole.None);
            bucht.displayName = "Gruppe";
            bucht.isVisible = false;
            bucht.displayOrder = 6;
            config.columns.Add(bucht);
            
            // Index 7: Bel.Datum = Eber-Name
            var eber = new ColumnDefinition("Bel.Datum", ColumnType.Text, ColumnRole.None);
            eber.displayName = "Eber";
            eber.isVisible = false;
            eber.displayOrder = 7;
            config.columns.Add(eber);
            
            // Index 8: TRT (Wurf-Anzahl)
            var trt = new ColumnDefinition("TRT", ColumnType.Number, ColumnRole.None);
            trt.displayName = "Wurf-Nr.";
            trt.isVisible = false;
            trt.displayOrder = 8;
            config.columns.Add(trt);
            
            // Index 9: Gruppe
            var gruppe = new ColumnDefinition("Gruppe", ColumnType.Text, ColumnRole.None);
            gruppe.displayName = "Gruppe-Code";
            gruppe.isVisible = false;
            gruppe.displayOrder = 9;
            config.columns.Add(gruppe);
            
            // Index 10: Eber
            var eberNr = new ColumnDefinition("Eber", ColumnType.Text, ColumnRole.None);
            eberNr.displayName = "Eber-Nr.";
            eberNr.isVisible = false;
            eberNr.displayOrder = 10;
            config.columns.Add(eberNr);
            
            // Index 11: Wurf
            var wurf = new ColumnDefinition("Wurf", ColumnType.Text, ColumnRole.None);
            wurf.isVisible = false;
            wurf.displayOrder = 11;
            config.columns.Add(wurf);
            
            // Index 12: Umr.
            var umr = new ColumnDefinition("Umr.", ColumnType.Text, ColumnRole.None);
            umr.isVisible = false;
            umr.displayOrder = 12;
            config.columns.Add(umr);
            
            // Index 13: vorauss. = Voraussichtliches Abferkeldatum
            var vorauss = new ColumnDefinition("vorauss.", ColumnType.Date, ColumnRole.BirthDate);
            vorauss.displayName = "Vorauss. Abferkelung";
            vorauss.isVisible = true;
            vorauss.displayOrder = 13;
            config.columns.Add(vorauss);
            
            // Index 14: Abferkelung
            var abferkelung = new ColumnDefinition("Abferkelung", ColumnType.Text, ColumnRole.None);
            abferkelung.isVisible = false;
            abferkelung.displayOrder = 14;
            config.columns.Add(abferkelung);
            
            config.lastModified = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            return config;
        }

        /// <summary>
        /// Validiert die Konfiguration
        /// </summary>
        public bool Validate(out string errorMessage)
        {
            errorMessage = "";
            
            // Prüfe, ob Ventilnummer definiert ist
            if (GetColumnByRole(ColumnRole.VentilNumber) == null)
            {
                errorMessage = "Ventilnummer-Spalte muss definiert sein!";
                return false;
            }
            
            // Prüfe, ob Ohrmarkennummer definiert ist
            if (GetColumnByRole(ColumnRole.EarTagNumber) == null)
            {
                errorMessage = "Ohrmarkennummer-Spalte muss definiert sein!";
                return false;
            }
            
            // Prüfe auf doppelte Original-Namen
            HashSet<string> names = new HashSet<string>();
            foreach (var col in columns)
            {
                if (names.Contains(col.originalName))
                {
                    errorMessage = $"Doppelter Spaltenname: {col.originalName}";
                    return false;
                }
                names.Add(col.originalName);
            }
            
            return true;
        }
    }
}
