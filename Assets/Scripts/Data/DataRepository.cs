using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace D8PlanerXR.Data
{
    /// <summary>
    /// Datenmodell für eine Sau (Schwein)
    /// </summary>
    [Serializable]
    public class SowData
    {
        public string earTagNumber;          // Ohrmarkennummer
        public int ventilNumber;             // Ventil-/Buchtennummer
        public DateTime matingDate;          // Deckdatum
        public string pregnancyStatus;       // Trächtigkeitsstatus
        public string healthStatus;          // Gesundheitszustand (für Medikations-Flag)
        public int daysSinceMating;          // Tage seit Deckung
        public TrafficLightColor trafficLight; // Ampelfarbe
        public Dictionary<string, string> additionalData; // Zusätzliche Daten aus CSV

        public enum TrafficLightColor
        {
            Green,      // Tragend <80 Tage
            Yellow,     // 80-106 Tage
            Red,        // 107+ Tage (kurz vor Abferkelung)
            Purple,     // Medikation erforderlich!
            Unknown
        }

        public SowData()
        {
            additionalData = new Dictionary<string, string>();
            trafficLight = TrafficLightColor.Unknown;
        }
    }

    /// <summary>
    /// Repository für CSV-Daten
    /// Verwaltet den Import, die Speicherung und den Zugriff auf Sauendaten
    /// </summary>
    public class DataRepository : MonoBehaviour
    {
        private static DataRepository instance;
        public static DataRepository Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject("DataRepository");
                    instance = go.AddComponent<DataRepository>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }

        // Haupt-Datenspeicher: Key = Ventilnummer, Value = Liste von Sauen
        private Dictionary<int, List<SowData>> ventilToSowsMap = new Dictionary<int, List<SowData>>();
        
        // Zusätzlicher Index: Key = Ohrmarkennummer
        private Dictionary<string, SowData> earTagToSowMap = new Dictionary<string, SowData>();

        // Compiled regex for number extraction (performance optimization)
        private static readonly Regex NumbersOnlyRegex = new Regex(@"[^\d]", RegexOptions.Compiled);

        // Statistiken
        public int TotalSows { get; private set; }
        public int TotalVentils { get; private set; }
        public DateTime LastImportDate { get; private set; }
        public string CurrentCSVPath { get; private set; }

        // Events
        public event Action OnDataImported;
        public event Action<string> OnImportError;

        // Ampel-Schwellwerte (Tage seit Deckung)
        // Grün: tragend <80 Tage, Gelb/Orange: <107 Tage, Rot: <118+ Tage
        [Header("Ampel-Logik")]
        [SerializeField] private int greenThresholdMin = 0;
        [SerializeField] private int greenThresholdMax = 79;
        [SerializeField] private int yellowThresholdMin = 80;
        [SerializeField] private int yellowThresholdMax = 106;
        [SerializeField] private int redThresholdMin = 107;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Importiert Daten aus einer CSV-Datei
        /// </summary>
        public bool ImportCSV(string csvFilePath, bool clearExistingData = true)
        {
            if (!File.Exists(csvFilePath))
            {
                string error = $"CSV-Datei nicht gefunden: {csvFilePath}";
                Debug.LogError(error);
                OnImportError?.Invoke(error);
                return false;
            }

            try
            {
                if (clearExistingData)
                {
                    ClearAllData();
                }

                CSVColumnConfig config = CSVConfigManager.Instance.CurrentConfig;
                if (config == null)
                {
                    // Create default MusterPlan config if none exists
                    Debug.Log("[DataRepository] Keine Konfiguration gefunden, erstelle MusterPlan-Konfiguration...");
                    CSVConfigManager.Instance.LoadMusterPlanConfig();
                    config = CSVConfigManager.Instance.CurrentConfig;
                    
                    if (config == null)
                    {
                        string error = "Keine CSV-Konfiguration geladen!";
                        Debug.LogError(error);
                        OnImportError?.Invoke(error);
                        return false;
                    }
                }

                // Encoding festlegen
                Encoding encoding = GetEncoding(config.encoding);
                string[] lines = File.ReadAllLines(csvFilePath, encoding);

                if (lines.Length == 0)
                {
                    string error = "CSV-Datei ist leer!";
                    Debug.LogError(error);
                    OnImportError?.Invoke(error);
                    return false;
                }

                // Header-Zeile verarbeiten
                int startLine = config.hasHeader ? 1 : 0;
                string[] headers = null;

                if (config.hasHeader)
                {
                    headers = ParseCSVLine(lines[0], config.delimiter);
                }
                else
                {
                    // Wenn keine Header-Zeile, verwende Spaltennamen aus Konfiguration
                    headers = new string[config.columns.Count];
                    for (int i = 0; i < config.columns.Count; i++)
                    {
                        headers[i] = config.columns[i].originalName;
                    }
                }
                
                Debug.Log($"[DataRepository] CSV Header mit {headers.Length} Spalten geladen");

                // Spalten-Indizes ermitteln - für MusterPlan verwende Index-basierte Zuordnung
                int ventilColumnIndex = GetColumnIndexWithFallback(headers, config.GetColumnByRole(CSVColumnConfig.ColumnRole.VentilNumber), 3);
                int earTagColumnIndex = GetColumnIndexWithFallback(headers, config.GetColumnByRole(CSVColumnConfig.ColumnRole.EarTagNumber), 1);
                int matingDateColumnIndex = GetColumnIndexWithFallback(headers, config.GetColumnByRole(CSVColumnConfig.ColumnRole.MatingDate), 4);
                int pregnancyStatusColumnIndex = GetColumnIndex(headers, config.GetColumnByRole(CSVColumnConfig.ColumnRole.PregnancyStatus));
                int healthStatusColumnIndex = GetColumnIndex(headers, config.GetColumnByRole(CSVColumnConfig.ColumnRole.HealthStatus));

                if (ventilColumnIndex < 0 || earTagColumnIndex < 0)
                {
                    string error = "Erforderliche Spalten (Ventilnummer, Ohrmarkennummer) nicht gefunden!";
                    Debug.LogError(error);
                    Debug.LogError($"  ventilColumnIndex={ventilColumnIndex}, earTagColumnIndex={earTagColumnIndex}");
                    OnImportError?.Invoke(error);
                    return false;
                }
                
                Debug.Log($"[DataRepository] Spalten-Indizes: Ventil={ventilColumnIndex}, Ohrmarke={earTagColumnIndex}, Belegdatum={matingDateColumnIndex}");

                // Datenzeilen verarbeiten
                int successCount = 0;
                int errorCount = 0;

                for (int i = startLine; i < lines.Length; i++)
                {
                    string line = lines[i].Trim();
                    if (string.IsNullOrEmpty(line)) continue;

                    try
                    {
                        string[] values = ParseCSVLine(line, config.delimiter);
                        
                        if (values.Length <= ventilColumnIndex || values.Length <= earTagColumnIndex)
                        {
                            Debug.LogWarning($"Zeile {i + 1}: Nicht genug Spalten ({values.Length})");
                            errorCount++;
                            continue;
                        }

                        SowData sowData = new SowData();

                        // Ventilnummer parsen (entferne Anführungszeichen und Leerzeichen)
                        string ventilStr = CleanValue(values[ventilColumnIndex]);
                        if (!int.TryParse(ventilStr, out sowData.ventilNumber))
                        {
                            // Versuche nur Zahlen zu extrahieren (using compiled regex for performance)
                            string numbersOnly = NumbersOnlyRegex.Replace(ventilStr, "");
                            if (!int.TryParse(numbersOnly, out sowData.ventilNumber))
                            {
                                if (i == startLine) // Nur bei der ersten Datenzeile warnen
                                    Debug.LogWarning($"Zeile {i + 1}: Ungültige Ventilnummer: '{ventilStr}'");
                                errorCount++;
                                continue;
                            }
                        }

                        // Ohrmarkennummer (entferne Anführungszeichen und Leerzeichen)
                        sowData.earTagNumber = CleanValue(values[earTagColumnIndex]);
                        if (string.IsNullOrEmpty(sowData.earTagNumber))
                        {
                            if (i == startLine) // Nur bei der ersten Datenzeile warnen
                                Debug.LogWarning($"Zeile {i + 1}: Leere Ohrmarkennummer");
                            errorCount++;
                            continue;
                        }

                        // Deckdatum parsen (optional)
                        if (matingDateColumnIndex >= 0 && matingDateColumnIndex < values.Length)
                        {
                            string dateStr = CleanValue(values[matingDateColumnIndex]);
                            if (DateTime.TryParse(dateStr, out DateTime matingDate))
                            {
                                sowData.matingDate = matingDate;
                                sowData.daysSinceMating = (DateTime.Now - matingDate).Days;
                            }
                        }

                        // Trächtigkeitsstatus (optional)
                        if (pregnancyStatusColumnIndex >= 0 && pregnancyStatusColumnIndex < values.Length)
                        {
                            sowData.pregnancyStatus = CleanValue(values[pregnancyStatusColumnIndex]);
                        }

                        // Gesundheitsstatus (optional)
                        if (healthStatusColumnIndex >= 0 && healthStatusColumnIndex < values.Length)
                        {
                            sowData.healthStatus = CleanValue(values[healthStatusColumnIndex]);
                        }

                        // Ampelfarbe berechnen (nach allen Daten)
                        sowData.trafficLight = CalculateTrafficLight(sowData);

                        // Alle Spalten als zusätzliche Daten speichern
                        for (int j = 0; j < headers.Length && j < values.Length; j++)
                        {
                            sowData.additionalData[CleanValue(headers[j])] = CleanValue(values[j]);
                        }

                        // Daten hinzufügen
                        AddSowData(sowData);
                        successCount++;
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"Zeile {i + 1}: Fehler beim Parsen: {e.Message}");
                        errorCount++;
                    }
                }

                // Statistiken aktualisieren
                UpdateStatistics();
                LastImportDate = DateTime.Now;
                CurrentCSVPath = csvFilePath;

                Debug.Log($"CSV Import abgeschlossen: {successCount} erfolgreich, {errorCount} Fehler");
                OnDataImported?.Invoke();
                return true;
            }
            catch (Exception e)
            {
                string error = $"Fehler beim Importieren der CSV: {e.Message}";
                Debug.LogError(error);
                OnImportError?.Invoke(error);
                return false;
            }
        }

        /// <summary>
        /// Fügt eine Sau zu den Daten hinzu
        /// </summary>
        private void AddSowData(SowData sowData)
        {
            // Zu Ventil-Map hinzufügen
            if (!ventilToSowsMap.ContainsKey(sowData.ventilNumber))
            {
                ventilToSowsMap[sowData.ventilNumber] = new List<SowData>();
            }
            ventilToSowsMap[sowData.ventilNumber].Add(sowData);

            // Zu EarTag-Map hinzufügen
            if (!earTagToSowMap.ContainsKey(sowData.earTagNumber))
            {
                earTagToSowMap[sowData.earTagNumber] = sowData;
            }
            else
            {
                Debug.LogWarning($"Doppelte Ohrmarkennummer gefunden: {sowData.earTagNumber}");
            }
        }

        /// <summary>
        /// Gibt alle Sauen an einem Ventil zurück
        /// </summary>
        public List<SowData> GetSowsByVentil(int ventilNumber)
        {
            if (ventilToSowsMap.ContainsKey(ventilNumber))
            {
                return new List<SowData>(ventilToSowsMap[ventilNumber]);
            }
            return new List<SowData>();
        }

        /// <summary>
        /// Gibt eine Sau anhand der Ohrmarkennummer zurück
        /// </summary>
        public SowData GetSowByEarTag(string earTagNumber)
        {
            if (earTagToSowMap.ContainsKey(earTagNumber))
            {
                return earTagToSowMap[earTagNumber];
            }
            return null;
        }

        /// <summary>
        /// Gibt alle verwendeten Ventilnummern zurück
        /// </summary>
        public List<int> GetAllVentilNumbers()
        {
            return new List<int>(ventilToSowsMap.Keys);
        }

        /// <summary>
        /// Berechnet die Ampelfarbe basierend auf Tagen seit Deckung und Gesundheitsstatus
        /// Lila = Medikation hat höchste Priorität!
        /// </summary>
        private SowData.TrafficLightColor CalculateTrafficLight(SowData sow)
        {
            // Priorität 1: Medikation (Lila)
            if (!string.IsNullOrEmpty(sow.healthStatus))
            {
                string health = sow.healthStatus.ToLower();
                if (health.Contains("medikation") || health.Contains("medication") || 
                    health.Contains("behandlung") || health.Contains("treatment") ||
                    health.Contains("krank") || health.Contains("sick"))
                {
                    return SowData.TrafficLightColor.Purple;
                }
            }

            // Priorität 2: Tage seit Deckung
            int days = sow.daysSinceMating;
            
            if (days >= greenThresholdMin && days <= greenThresholdMax)
                return SowData.TrafficLightColor.Green;
            
            if (days >= yellowThresholdMin && days <= yellowThresholdMax)
                return SowData.TrafficLightColor.Yellow;
            
            if (days >= redThresholdMin)
                return SowData.TrafficLightColor.Red;
            
            return SowData.TrafficLightColor.Unknown;
        }

        /// <summary>
        /// Ermittelt den Index einer Spalte anhand der Konfiguration
        /// </summary>
        private int GetColumnIndex(string[] headers, CSVColumnConfig.ColumnDefinition column)
        {
            if (column == null) return -1;

            // Zuerst versuche exakte Namenszuordnung (bereinige Anführungszeichen)
            for (int i = 0; i < headers.Length; i++)
            {
                string cleanHeader = CleanValue(headers[i]);
                if (cleanHeader.Equals(column.originalName, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }
            
            // Falls keine exakte Übereinstimmung, prüfe auf Index-Suffix (z.B. "Belegdatum_Spalte5")
            // Dies ermöglicht die Zuordnung von Spalten mit dynamischen Headern (wie Datums-Header)
            if (column.originalName.Contains("_Spalte"))
            {
                string suffix = column.originalName.Substring(column.originalName.LastIndexOf("_Spalte") + 7);
                if (int.TryParse(suffix, out int columnIndex))
                {
                    // Spaltenindex ist 1-basiert in der Konfiguration, 0-basiert im Array
                    int arrayIndex = columnIndex - 1;
                    if (arrayIndex >= 0 && arrayIndex < headers.Length)
                    {
                        return arrayIndex;
                    }
                }
            }
            
            return -1;
        }

        /// <summary>
        /// Ermittelt den Index einer Spalte, mit Fallback auf festen Index
        /// </summary>
        private int GetColumnIndexWithFallback(string[] headers, CSVColumnConfig.ColumnDefinition column, int fallbackIndex)
        {
            int index = GetColumnIndex(headers, column);
            if (index >= 0)
            {
                return index;
            }
            
            // Fallback auf festen Index (für MusterPlan.csv)
            if (fallbackIndex >= 0 && fallbackIndex < headers.Length)
            {
                Debug.Log($"[DataRepository] Verwende Fallback-Index {fallbackIndex} für Spalte '{column?.originalName}'");
                return fallbackIndex;
            }
            
            return -1;
        }

        /// <summary>
        /// Parst eine CSV-Zeile und berücksichtigt Anführungszeichen
        /// </summary>
        private string[] ParseCSVLine(string line, char delimiter)
        {
            List<string> result = new List<string>();
            bool inQuotes = false;
            StringBuilder currentField = new StringBuilder();
            
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == delimiter && !inQuotes)
                {
                    result.Add(currentField.ToString());
                    currentField.Clear();
                }
                else
                {
                    currentField.Append(c);
                }
            }
            
            // Letztes Feld hinzufügen
            result.Add(currentField.ToString());
            
            return result.ToArray();
        }

        /// <summary>
        /// Bereinigt einen Wert von Anführungszeichen und Leerzeichen
        /// </summary>
        private string CleanValue(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            
            // Entferne führende/nachfolgende Anführungszeichen und Leerzeichen
            string cleaned = value.Trim();
            if (cleaned.StartsWith("\"") && cleaned.EndsWith("\""))
            {
                cleaned = cleaned.Substring(1, cleaned.Length - 2);
            }
            cleaned = cleaned.Trim();
            
            return cleaned;
        }

        /// <summary>
        /// Gibt das Encoding-Objekt zurück
        /// </summary>
        private Encoding GetEncoding(string encodingName)
        {
            switch (encodingName)
            {
                case "UTF-8":
                    return Encoding.UTF8;
                case "ISO-8859-1":
                    return Encoding.GetEncoding("ISO-8859-1");
                case "Windows-1252":
                    return Encoding.GetEncoding("Windows-1252");
                default:
                    return Encoding.UTF8;
            }
        }

        /// <summary>
        /// Aktualisiert die Statistiken
        /// </summary>
        private void UpdateStatistics()
        {
            TotalSows = earTagToSowMap.Count;
            TotalVentils = ventilToSowsMap.Count;
        }

        /// <summary>
        /// Löscht alle Daten
        /// </summary>
        public void ClearAllData()
        {
            ventilToSowsMap.Clear();
            earTagToSowMap.Clear();
            TotalSows = 0;
            TotalVentils = 0;
        }

        /// <summary>
        /// Gibt Statistiken als String zurück
        /// </summary>
        public string GetStatistics()
        {
            return $"Sauen: {TotalSows}\n" +
                   $"Ventile: {TotalVentils}\n" +
                   $"Letzter Import: {LastImportDate:dd.MM.yyyy HH:mm}\n" +
                   $"CSV-Datei: {Path.GetFileName(CurrentCSVPath)}";
        }

        /// <summary>
        /// Sortiert Sauen an einem Ventil
        /// </summary>
        public enum SortOrder
        {
            EarTagAscending,
            EarTagDescending,
            MatingDateOldest,
            MatingDateNewest,
            TrafficLightRedFirst,
            TrafficLightGreenFirst
        }

        public List<SowData> GetSortedSowsByVentil(int ventilNumber, SortOrder sortOrder)
        {
            List<SowData> sows = GetSowsByVentil(ventilNumber);
            
            switch (sortOrder)
            {
                case SortOrder.EarTagAscending:
                    sows.Sort((a, b) => string.Compare(a.earTagNumber, b.earTagNumber));
                    break;
                case SortOrder.EarTagDescending:
                    sows.Sort((a, b) => string.Compare(b.earTagNumber, a.earTagNumber));
                    break;
                case SortOrder.MatingDateOldest:
                    sows.Sort((a, b) => a.matingDate.CompareTo(b.matingDate));
                    break;
                case SortOrder.MatingDateNewest:
                    sows.Sort((a, b) => b.matingDate.CompareTo(a.matingDate));
                    break;
                case SortOrder.TrafficLightRedFirst:
                    sows.Sort((a, b) => b.trafficLight.CompareTo(a.trafficLight));
                    break;
                case SortOrder.TrafficLightGreenFirst:
                    sows.Sort((a, b) => a.trafficLight.CompareTo(b.trafficLight));
                    break;
            }
            
            return sows;
        }

        /// <summary>
        /// Setzt die Ampel-Schwellwerte
        /// </summary>
        public void SetTrafficLightThresholds(int greenMin, int greenMax, int yellowMin, int yellowMax, int redMin)
        {
            greenThresholdMin = greenMin;
            greenThresholdMax = greenMax;
            yellowThresholdMin = yellowMin;
            yellowThresholdMax = yellowMax;
            redThresholdMin = redMin;

            // Alle Ampelfarben neu berechnen
            foreach (var sow in earTagToSowMap.Values)
            {
                sow.trafficLight = CalculateTrafficLight(sow);
            }
        }
    }
}
