using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
                    string error = "Keine CSV-Konfiguration geladen!";
                    Debug.LogError(error);
                    OnImportError?.Invoke(error);
                    return false;
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
                    headers = lines[0].Split(config.delimiter);
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

                // Spalten-Indizes ermitteln
                int ventilColumnIndex = GetColumnIndex(headers, config.GetColumnByRole(CSVColumnConfig.ColumnRole.VentilNumber));
                int earTagColumnIndex = GetColumnIndex(headers, config.GetColumnByRole(CSVColumnConfig.ColumnRole.EarTagNumber));
                int matingDateColumnIndex = GetColumnIndex(headers, config.GetColumnByRole(CSVColumnConfig.ColumnRole.MatingDate));
                int pregnancyStatusColumnIndex = GetColumnIndex(headers, config.GetColumnByRole(CSVColumnConfig.ColumnRole.PregnancyStatus));
                int healthStatusColumnIndex = GetColumnIndex(headers, config.GetColumnByRole(CSVColumnConfig.ColumnRole.HealthStatus));

                if (ventilColumnIndex < 0 || earTagColumnIndex < 0)
                {
                    string error = "Erforderliche Spalten (Ventilnummer, Ohrmarkennummer) nicht gefunden!";
                    Debug.LogError(error);
                    OnImportError?.Invoke(error);
                    return false;
                }

                // Datenzeilen verarbeiten
                int successCount = 0;
                int errorCount = 0;

                for (int i = startLine; i < lines.Length; i++)
                {
                    string line = lines[i].Trim();
                    if (string.IsNullOrEmpty(line)) continue;

                    try
                    {
                        string[] values = line.Split(config.delimiter);
                        
                        if (values.Length < headers.Length)
                        {
                            Debug.LogWarning($"Zeile {i + 1}: Nicht genug Spalten ({values.Length}/{headers.Length})");
                            errorCount++;
                            continue;
                        }

                        SowData sowData = new SowData();

                        // Ventilnummer parsen
                        if (!int.TryParse(values[ventilColumnIndex].Trim(), out sowData.ventilNumber))
                        {
                            Debug.LogWarning($"Zeile {i + 1}: Ungültige Ventilnummer: {values[ventilColumnIndex]}");
                            errorCount++;
                            continue;
                        }

                        // Ohrmarkennummer
                        sowData.earTagNumber = values[earTagColumnIndex].Trim();
                        if (string.IsNullOrEmpty(sowData.earTagNumber))
                        {
                            Debug.LogWarning($"Zeile {i + 1}: Leere Ohrmarkennummer");
                            errorCount++;
                            continue;
                        }

                        // Deckdatum parsen (optional)
                        if (matingDateColumnIndex >= 0 && matingDateColumnIndex < values.Length)
                        {
                            if (DateTime.TryParse(values[matingDateColumnIndex].Trim(), out DateTime matingDate))
                            {
                                sowData.matingDate = matingDate;
                                sowData.daysSinceMating = (DateTime.Now - matingDate).Days;
                            }
                        }

                        // Trächtigkeitsstatus (optional)
                        if (pregnancyStatusColumnIndex >= 0 && pregnancyStatusColumnIndex < values.Length)
                        {
                            sowData.pregnancyStatus = values[pregnancyStatusColumnIndex].Trim();
                        }

                        // Gesundheitsstatus (optional)
                        if (healthStatusColumnIndex >= 0 && healthStatusColumnIndex < values.Length)
                        {
                            sowData.healthStatus = values[healthStatusColumnIndex].Trim();
                        }

                        // Ampelfarbe berechnen (nach allen Daten)
                        sowData.trafficLight = CalculateTrafficLight(sowData);

                        // Alle Spalten als zusätzliche Daten speichern
                        for (int j = 0; j < headers.Length && j < values.Length; j++)
                        {
                            sowData.additionalData[headers[j].Trim()] = values[j].Trim();
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

            for (int i = 0; i < headers.Length; i++)
            {
                if (headers[i].Trim().Equals(column.originalName, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }
            return -1;
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
