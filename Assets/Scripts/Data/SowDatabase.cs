using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace D8PlanerXR.Data
{
    /// <summary>
    /// Persistent database for sow data using PlayerPrefs-based JSON storage
    /// Handles CSV imports with deduplication over a 6-month window
    /// </summary>
    public class SowDatabase : MonoBehaviour
    {
        private static SowDatabase instance;
        public static SowDatabase Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject("SowDatabase");
                    instance = go.AddComponent<SowDatabase>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }

        // Database file name
        private const string DB_FILE_NAME = "sow_database.json";
        private const string IMPORT_HISTORY_KEY = "csv_import_history";
        private const int RETENTION_MONTHS = 6;

        // Internal data structure for serialization
        [Serializable]
        private class DatabaseContent
        {
            public List<SowDataEntry> entries = new List<SowDataEntry>();
            public string lastUpdated;
        }

        [Serializable]
        private class SowDataEntry
        {
            public string uniqueKey;           // Composite key for deduplication
            public string earTagNumber;
            public int ventilNumber;
            public string matingDateStr;       // Store as string for serialization
            public string pregnancyStatus;
            public string healthStatus;
            public int daysSinceMating;
            public int trafficLight;           // Stored as int for serialization
            public string additionalDataJson;  // JSON string of additional data
            public string importDate;          // When this record was imported
            public string sourceFile;          // Source CSV file name
        }

        [Serializable]
        private class ImportHistoryEntry
        {
            public string fileName;
            public string importDate;
            public int recordCount;
        }

        [Serializable]
        private class ImportHistory
        {
            public List<ImportHistoryEntry> imports = new List<ImportHistoryEntry>();
        }

        // Current database content
        private DatabaseContent database;
        private ImportHistory importHistory;
        private string databasePath;

        // Events
        public event Action OnDatabaseUpdated;
        public event Action<string> OnImportCompleted;
        public event Action<string> OnError;

        // Properties
        public int TotalRecords => database?.entries?.Count ?? 0;
        public DateTime LastUpdated => database != null && !string.IsNullOrEmpty(database.lastUpdated) 
            ? DateTime.Parse(database.lastUpdated) 
            : DateTime.MinValue;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeDatabase();
        }

        /// <summary>
        /// Initialize the database
        /// </summary>
        private void InitializeDatabase()
        {
            databasePath = Path.Combine(Application.persistentDataPath, DB_FILE_NAME);
            LoadDatabase();
            LoadImportHistory();
            CleanupOldRecords();
            
            Debug.Log($"[SowDatabase] Initialized with {TotalRecords} records");
            Debug.Log($"[SowDatabase] Database path: {databasePath}");
        }

        /// <summary>
        /// Load the database from disk
        /// </summary>
        private void LoadDatabase()
        {
            try
            {
                if (File.Exists(databasePath))
                {
                    string json = File.ReadAllText(databasePath, Encoding.UTF8);
                    database = JsonUtility.FromJson<DatabaseContent>(json);
                    if (database == null)
                    {
                        database = new DatabaseContent();
                    }
                    Debug.Log($"[SowDatabase] Loaded {database.entries.Count} entries from database");
                }
                else
                {
                    database = new DatabaseContent();
                    Debug.Log("[SowDatabase] Created new empty database");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SowDatabase] Error loading database: {e.Message}");
                database = new DatabaseContent();
                OnError?.Invoke($"Fehler beim Laden der Datenbank: {e.Message}");
            }
        }

        /// <summary>
        /// Save the database to disk
        /// </summary>
        private void SaveDatabase()
        {
            try
            {
                database.lastUpdated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string json = JsonUtility.ToJson(database, true);
                File.WriteAllText(databasePath, json, Encoding.UTF8);
                Debug.Log($"[SowDatabase] Saved {database.entries.Count} entries to database");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SowDatabase] Error saving database: {e.Message}");
                OnError?.Invoke($"Fehler beim Speichern der Datenbank: {e.Message}");
            }
        }

        /// <summary>
        /// Load import history from PlayerPrefs
        /// </summary>
        private void LoadImportHistory()
        {
            try
            {
                string json = PlayerPrefs.GetString(IMPORT_HISTORY_KEY, "");
                if (!string.IsNullOrEmpty(json))
                {
                    importHistory = JsonUtility.FromJson<ImportHistory>(json);
                }
                if (importHistory == null)
                {
                    importHistory = new ImportHistory();
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SowDatabase] Error loading import history: {e.Message}");
                importHistory = new ImportHistory();
            }
        }

        /// <summary>
        /// Save import history to PlayerPrefs
        /// </summary>
        private void SaveImportHistory()
        {
            try
            {
                string json = JsonUtility.ToJson(importHistory);
                PlayerPrefs.SetString(IMPORT_HISTORY_KEY, json);
                PlayerPrefs.Save();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SowDatabase] Error saving import history: {e.Message}");
            }
        }

        /// <summary>
        /// Import data from a CSV file into the database with deduplication
        /// </summary>
        public bool ImportFromCSV(string csvFilePath)
        {
            if (!File.Exists(csvFilePath))
            {
                string error = $"CSV-Datei nicht gefunden: {csvFilePath}";
                Debug.LogError($"[SowDatabase] {error}");
                OnError?.Invoke(error);
                return false;
            }

            try
            {
                CSVColumnConfig config = CSVConfigManager.Instance.CurrentConfig;
                if (config == null)
                {
                    CSVConfigManager.Instance.LoadMusterPlanConfig();
                    config = CSVConfigManager.Instance.CurrentConfig;
                }

                Encoding encoding = GetEncoding(config.encoding);
                string[] lines = File.ReadAllLines(csvFilePath, encoding);

                if (lines.Length == 0)
                {
                    OnError?.Invoke("CSV-Datei ist leer!");
                    return false;
                }

                int startLine = config.hasHeader ? 1 : 0;
                string[] headers = config.hasHeader ? ParseCSVLine(lines[0], config.delimiter) : null;

                string fileName = Path.GetFileName(csvFilePath);
                string importDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                
                int newRecords = 0;
                int updatedRecords = 0;
                int skippedRecords = 0;

                // Get column indices
                int ventilColumnIndex = GetColumnIndexWithFallback(headers, config.GetColumnByRole(CSVColumnConfig.ColumnRole.VentilNumber), 3, config);
                int earTagColumnIndex = GetColumnIndexWithFallback(headers, config.GetColumnByRole(CSVColumnConfig.ColumnRole.EarTagNumber), 1, config);
                int matingDateColumnIndex = GetColumnIndexWithFallback(headers, config.GetColumnByRole(CSVColumnConfig.ColumnRole.MatingDate), 4, config);
                int pregnancyStatusColumnIndex = GetColumnIndex(headers, config.GetColumnByRole(CSVColumnConfig.ColumnRole.PregnancyStatus));
                int healthStatusColumnIndex = GetColumnIndex(headers, config.GetColumnByRole(CSVColumnConfig.ColumnRole.HealthStatus));

                for (int i = startLine; i < lines.Length; i++)
                {
                    string line = lines[i].Trim();
                    if (string.IsNullOrEmpty(line)) continue;

                    try
                    {
                        string[] values = ParseCSVLine(line, config.delimiter);
                        
                        if (values.Length <= ventilColumnIndex || values.Length <= earTagColumnIndex)
                        {
                            skippedRecords++;
                            continue;
                        }

                        SowDataEntry entry = new SowDataEntry();

                        // Parse ventil number
                        string ventilStr = CleanValue(values[ventilColumnIndex]);
                        if (!int.TryParse(ventilStr, out entry.ventilNumber))
                        {
                            string numbersOnly = System.Text.RegularExpressions.Regex.Replace(ventilStr, @"[^\d]", "");
                            if (!int.TryParse(numbersOnly, out entry.ventilNumber))
                            {
                                skippedRecords++;
                                continue;
                            }
                        }

                        // Parse ear tag number
                        entry.earTagNumber = CleanValue(values[earTagColumnIndex]);
                        if (string.IsNullOrEmpty(entry.earTagNumber))
                        {
                            skippedRecords++;
                            continue;
                        }

                        // Parse mating date
                        if (matingDateColumnIndex >= 0 && matingDateColumnIndex < values.Length)
                        {
                            string dateStr = CleanValue(values[matingDateColumnIndex]);
                            if (DateTime.TryParse(dateStr, out DateTime matingDate))
                            {
                                entry.matingDateStr = matingDate.ToString("yyyy-MM-dd");
                                entry.daysSinceMating = (DateTime.Now - matingDate).Days;
                            }
                        }

                        // Parse pregnancy status
                        if (pregnancyStatusColumnIndex >= 0 && pregnancyStatusColumnIndex < values.Length)
                        {
                            entry.pregnancyStatus = CleanValue(values[pregnancyStatusColumnIndex]);
                        }

                        // Parse health status
                        if (healthStatusColumnIndex >= 0 && healthStatusColumnIndex < values.Length)
                        {
                            entry.healthStatus = CleanValue(values[healthStatusColumnIndex]);
                        }

                        // Calculate traffic light
                        entry.trafficLight = (int)CalculateTrafficLight(entry);

                        // Store additional data
                        if (headers != null)
                        {
                            var additionalData = new Dictionary<string, string>();
                            for (int j = 0; j < headers.Length && j < values.Length; j++)
                            {
                                additionalData[CleanValue(headers[j])] = CleanValue(values[j]);
                            }
                            entry.additionalDataJson = JsonUtility.ToJson(new SerializableDictionary(additionalData));
                        }

                        // Create unique key for deduplication
                        entry.uniqueKey = CreateUniqueKey(entry);
                        entry.importDate = importDate;
                        entry.sourceFile = fileName;

                        // Check for duplicates and update or add
                        int existingIndex = FindExistingEntryIndex(entry.uniqueKey);
                        if (existingIndex >= 0)
                        {
                            // Update existing record
                            database.entries[existingIndex] = entry;
                            updatedRecords++;
                        }
                        else
                        {
                            // Add new record
                            database.entries.Add(entry);
                            newRecords++;
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"[SowDatabase] Error parsing line {i + 1}: {e.Message}");
                        skippedRecords++;
                    }
                }

                // Save database
                SaveDatabase();

                // Update import history
                var historyEntry = new ImportHistoryEntry
                {
                    fileName = fileName,
                    importDate = importDate,
                    recordCount = newRecords + updatedRecords
                };
                importHistory.imports.Add(historyEntry);
                SaveImportHistory();

                // Cleanup old records
                CleanupOldRecords();

                // Notify listeners
                string message = $"Import abgeschlossen: {newRecords} neu, {updatedRecords} aktualisiert, {skippedRecords} übersprungen";
                Debug.Log($"[SowDatabase] {message}");
                OnImportCompleted?.Invoke(message);
                OnDatabaseUpdated?.Invoke();

                // Also update DataRepository with current data
                SyncToDataRepository();

                return true;
            }
            catch (Exception e)
            {
                string error = $"Fehler beim Import: {e.Message}";
                Debug.LogError($"[SowDatabase] {error}");
                OnError?.Invoke(error);
                return false;
            }
        }

        /// <summary>
        /// Create a unique key for deduplication based on ear tag and mating date
        /// </summary>
        private string CreateUniqueKey(SowDataEntry entry)
        {
            // Unique key: EarTag + MatingDate (same sow with same mating is same record)
            return $"{entry.earTagNumber}_{entry.matingDateStr ?? "unknown"}";
        }

        /// <summary>
        /// Find existing entry index by unique key
        /// </summary>
        private int FindExistingEntryIndex(string uniqueKey)
        {
            for (int i = 0; i < database.entries.Count; i++)
            {
                if (database.entries[i].uniqueKey == uniqueKey)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Cleanup records older than 6 months
        /// </summary>
        private void CleanupOldRecords()
        {
            DateTime cutoffDate = DateTime.Now.AddMonths(-RETENTION_MONTHS);
            int removedCount = 0;

            for (int i = database.entries.Count - 1; i >= 0; i--)
            {
                var entry = database.entries[i];
                if (!string.IsNullOrEmpty(entry.importDate))
                {
                    if (DateTime.TryParse(entry.importDate, out DateTime importDate))
                    {
                        if (importDate < cutoffDate)
                        {
                            database.entries.RemoveAt(i);
                            removedCount++;
                        }
                    }
                }
            }

            if (removedCount > 0)
            {
                SaveDatabase();
                Debug.Log($"[SowDatabase] Cleaned up {removedCount} records older than {RETENTION_MONTHS} months");
            }
        }

        /// <summary>
        /// Sync database content to DataRepository for app usage
        /// </summary>
        public void SyncToDataRepository()
        {
            DataRepository.Instance.ClearAllData();

            foreach (var entry in database.entries)
            {
                SowData sowData = new SowData
                {
                    earTagNumber = entry.earTagNumber,
                    ventilNumber = entry.ventilNumber,
                    pregnancyStatus = entry.pregnancyStatus,
                    healthStatus = entry.healthStatus,
                    daysSinceMating = entry.daysSinceMating,
                    trafficLight = (SowData.TrafficLightColor)entry.trafficLight
                };

                if (!string.IsNullOrEmpty(entry.matingDateStr))
                {
                    if (DateTime.TryParse(entry.matingDateStr, out DateTime matingDate))
                    {
                        sowData.matingDate = matingDate;
                    }
                }

                // Add to repository using reflection or internal method
                AddSowDataToRepository(sowData);
            }

            Debug.Log($"[SowDatabase] Synced {database.entries.Count} entries to DataRepository");
        }

        /// <summary>
        /// Add sow data to the DataRepository
        /// </summary>
        private void AddSowDataToRepository(SowData sowData)
        {
            // Use DataRepository's public method to add sow data
            try
            {
                DataRepository.Instance.AddSowDataPublic(sowData);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SowDatabase] Error adding sow to repository: {e.Message}");
            }
        }

        /// <summary>
        /// Get all sow data from the database
        /// </summary>
        public List<SowData> GetAllSowData()
        {
            List<SowData> result = new List<SowData>();

            foreach (var entry in database.entries)
            {
                SowData sowData = new SowData
                {
                    earTagNumber = entry.earTagNumber,
                    ventilNumber = entry.ventilNumber,
                    pregnancyStatus = entry.pregnancyStatus,
                    healthStatus = entry.healthStatus,
                    daysSinceMating = entry.daysSinceMating,
                    trafficLight = (SowData.TrafficLightColor)entry.trafficLight
                };

                if (!string.IsNullOrEmpty(entry.matingDateStr))
                {
                    if (DateTime.TryParse(entry.matingDateStr, out DateTime matingDate))
                    {
                        sowData.matingDate = matingDate;
                    }
                }

                result.Add(sowData);
            }

            return result;
        }

        /// <summary>
        /// Get sow data by ventil number
        /// </summary>
        public List<SowData> GetSowsByVentil(int ventilNumber)
        {
            List<SowData> result = new List<SowData>();

            foreach (var entry in database.entries)
            {
                if (entry.ventilNumber == ventilNumber)
                {
                    SowData sowData = new SowData
                    {
                        earTagNumber = entry.earTagNumber,
                        ventilNumber = entry.ventilNumber,
                        pregnancyStatus = entry.pregnancyStatus,
                        healthStatus = entry.healthStatus,
                        daysSinceMating = entry.daysSinceMating,
                        trafficLight = (SowData.TrafficLightColor)entry.trafficLight
                    };

                    if (!string.IsNullOrEmpty(entry.matingDateStr))
                    {
                        if (DateTime.TryParse(entry.matingDateStr, out DateTime matingDate))
                        {
                            sowData.matingDate = matingDate;
                        }
                    }

                    result.Add(sowData);
                }
            }

            return result;
        }

        /// <summary>
        /// Get import history
        /// </summary>
        public List<string> GetImportHistory()
        {
            List<string> history = new List<string>();
            foreach (var entry in importHistory.imports)
            {
                history.Add($"{entry.importDate}: {entry.fileName} ({entry.recordCount} Datensätze)");
            }
            return history;
        }

        /// <summary>
        /// Clear all data in the database
        /// </summary>
        public void ClearDatabase()
        {
            database = new DatabaseContent();
            SaveDatabase();
            importHistory = new ImportHistory();
            SaveImportHistory();
            OnDatabaseUpdated?.Invoke();
            Debug.Log("[SowDatabase] Database cleared");
        }

        /// <summary>
        /// Get database statistics
        /// </summary>
        public string GetStatistics()
        {
            int uniqueVentils = new HashSet<int>(database.entries.ConvertAll(e => e.ventilNumber)).Count;
            return $"Datenbank:\n" +
                   $"  Datensätze: {TotalRecords}\n" +
                   $"  Ventile: {uniqueVentils}\n" +
                   $"  Letzte Aktualisierung: {(LastUpdated > DateTime.MinValue ? LastUpdated.ToString("dd.MM.yyyy HH:mm") : "Nie")}\n" +
                   $"  Imports: {importHistory.imports.Count}";
        }

        #region Helper Methods

        private SowData.TrafficLightColor CalculateTrafficLight(SowDataEntry entry)
        {
            // Priority 1: Medication (Purple)
            if (!string.IsNullOrEmpty(entry.healthStatus))
            {
                string health = entry.healthStatus.ToLower();
                if (health.Contains("medikation") || health.Contains("medication") ||
                    health.Contains("behandlung") || health.Contains("treatment") ||
                    health.Contains("krank") || health.Contains("sick"))
                {
                    return SowData.TrafficLightColor.Purple;
                }
            }

            // Priority 2: Days since mating
            int days = entry.daysSinceMating;

            if (days >= 0 && days <= 79)
                return SowData.TrafficLightColor.Green;

            if (days >= 80 && days <= 106)
                return SowData.TrafficLightColor.Yellow;

            if (days >= 107)
                return SowData.TrafficLightColor.Red;

            return SowData.TrafficLightColor.Unknown;
        }

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

            result.Add(currentField.ToString());
            return result.ToArray();
        }

        private string CleanValue(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";

            string cleaned = value.Trim();
            if (cleaned.StartsWith("\"") && cleaned.EndsWith("\""))
            {
                cleaned = cleaned.Substring(1, cleaned.Length - 2);
            }
            cleaned = cleaned.Trim();

            return cleaned;
        }

        private int GetColumnIndex(string[] headers, CSVColumnConfig.ColumnDefinition column)
        {
            if (column == null || headers == null) return -1;

            for (int i = 0; i < headers.Length; i++)
            {
                string cleanHeader = CleanValue(headers[i]);
                if (cleanHeader.Equals(column.originalName, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return -1;
        }

        private int GetColumnIndexWithFallback(string[] headers, CSVColumnConfig.ColumnDefinition column, int fallbackIndex, CSVColumnConfig config)
        {
            int index = GetColumnIndex(headers, column);
            if (index >= 0)
            {
                return index;
            }

            if (headers != null && fallbackIndex >= 0 && fallbackIndex < headers.Length)
            {
                return fallbackIndex;
            }

            return -1;
        }

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

        #endregion

        /// <summary>
        /// Helper class for serializing dictionaries
        /// </summary>
        [Serializable]
        private class SerializableDictionary
        {
            public List<string> keys = new List<string>();
            public List<string> values = new List<string>();

            public SerializableDictionary() { }

            public SerializableDictionary(Dictionary<string, string> dict)
            {
                foreach (var kvp in dict)
                {
                    keys.Add(kvp.Key);
                    values.Add(kvp.Value);
                }
            }

            public Dictionary<string, string> ToDictionary()
            {
                var result = new Dictionary<string, string>();
                for (int i = 0; i < keys.Count && i < values.Count; i++)
                {
                    result[keys[i]] = values[i];
                }
                return result;
            }
        }
    }
}
