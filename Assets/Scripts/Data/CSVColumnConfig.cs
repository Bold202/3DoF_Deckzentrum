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
