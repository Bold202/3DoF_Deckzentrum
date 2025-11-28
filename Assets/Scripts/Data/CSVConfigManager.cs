using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;

namespace D8PlanerXR.Data
{
    /// <summary>
    /// Manager für das Speichern und Laden von CSV-Spaltenkonfigurationen
    /// </summary>
    public class CSVConfigManager : MonoBehaviour
    {
        private static CSVConfigManager instance;
        public static CSVConfigManager Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject("CSVConfigManager");
                    instance = go.AddComponent<CSVConfigManager>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }

        // Aktuelle Konfiguration
        private CSVColumnConfig currentConfig;
        public CSVColumnConfig CurrentConfig => currentConfig;

        // Gespeicherte Konfigurationen (Profile)
        private Dictionary<string, CSVColumnConfig> savedConfigs = new Dictionary<string, CSVColumnConfig>();

        // Pfad zum Konfigurationsordner
        private string configDirectory;
        private const string CONFIG_FOLDER = "CSVConfigs";
        private const string CURRENT_CONFIG_FILE = "current_config.json";

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeConfigDirectory();
            LoadCurrentConfig();
        }

        /// <summary>
        /// Initialisiert das Konfigurationsverzeichnis
        /// </summary>
        private void InitializeConfigDirectory()
        {
            configDirectory = Path.Combine(Application.persistentDataPath, CONFIG_FOLDER);
            if (!Directory.Exists(configDirectory))
            {
                Directory.CreateDirectory(configDirectory);
                Debug.Log($"CSV Config Directory erstellt: {configDirectory}");
            }
        }

        /// <summary>
        /// Lädt die aktuelle Konfiguration
        /// </summary>
        public void LoadCurrentConfig()
        {
            string filePath = Path.Combine(configDirectory, CURRENT_CONFIG_FILE);
            
            if (File.Exists(filePath))
            {
                try
                {
                    string json = File.ReadAllText(filePath);
                    currentConfig = JsonUtility.FromJson<CSVColumnConfig>(json);
                    Debug.Log($"Konfiguration geladen: {currentConfig.configName}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"Fehler beim Laden der Konfiguration: {e.Message}");
                    CreateDefaultConfig();
                }
            }
            else
            {
                CreateDefaultConfig();
            }
        }

        /// <summary>
        /// Speichert die aktuelle Konfiguration
        /// </summary>
        public bool SaveCurrentConfig()
        {
            if (currentConfig == null)
            {
                Debug.LogError("Keine Konfiguration zum Speichern vorhanden!");
                return false;
            }

            // Validierung
            if (!currentConfig.Validate(out string errorMessage))
            {
                Debug.LogError($"Ungültige Konfiguration: {errorMessage}");
                return false;
            }

            try
            {
                currentConfig.lastModified = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string json = JsonUtility.ToJson(currentConfig, true);
                string filePath = Path.Combine(configDirectory, CURRENT_CONFIG_FILE);
                File.WriteAllText(filePath, json);
                Debug.Log($"Konfiguration gespeichert: {filePath}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Fehler beim Speichern der Konfiguration: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Speichert eine Konfiguration als Profil
        /// </summary>
        public bool SaveConfigAsProfile(string profileName)
        {
            if (currentConfig == null)
            {
                Debug.LogError("Keine Konfiguration zum Speichern vorhanden!");
                return false;
            }

            if (string.IsNullOrEmpty(profileName))
            {
                Debug.LogError("Profilname darf nicht leer sein!");
                return false;
            }

            try
            {
                CSVColumnConfig profileConfig = CloneConfig(currentConfig);
                profileConfig.configName = profileName;
                profileConfig.lastModified = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                
                string json = JsonUtility.ToJson(profileConfig, true);
                string fileName = SanitizeFileName(profileName) + ".json";
                string filePath = Path.Combine(configDirectory, fileName);
                File.WriteAllText(filePath, json);
                
                savedConfigs[profileName] = profileConfig;
                Debug.Log($"Profil gespeichert: {profileName}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Fehler beim Speichern des Profils: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Lädt ein Konfigurationsprofil
        /// </summary>
        public bool LoadConfigProfile(string profileName)
        {
            try
            {
                string fileName = SanitizeFileName(profileName) + ".json";
                string filePath = Path.Combine(configDirectory, fileName);
                
                if (!File.Exists(filePath))
                {
                    Debug.LogError($"Profil nicht gefunden: {profileName}");
                    return false;
                }
                
                string json = File.ReadAllText(filePath);
                CSVColumnConfig loadedConfig = JsonUtility.FromJson<CSVColumnConfig>(json);
                
                if (loadedConfig.Validate(out string errorMessage))
                {
                    currentConfig = loadedConfig;
                    SaveCurrentConfig();
                    Debug.Log($"Profil geladen: {profileName}");
                    return true;
                }
                else
                {
                    Debug.LogError($"Ungültiges Profil: {errorMessage}");
                    return false;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Fehler beim Laden des Profils: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Löscht ein Konfigurationsprofil
        /// </summary>
        public bool DeleteConfigProfile(string profileName)
        {
            try
            {
                string fileName = SanitizeFileName(profileName) + ".json";
                string filePath = Path.Combine(configDirectory, fileName);
                
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    savedConfigs.Remove(profileName);
                    Debug.Log($"Profil gelöscht: {profileName}");
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                Debug.LogError($"Fehler beim Löschen des Profils: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Listet alle verfügbaren Profile auf
        /// </summary>
        public List<string> GetAvailableProfiles()
        {
            List<string> profiles = new List<string>();
            try
            {
                string[] files = Directory.GetFiles(configDirectory, "*.json");
                foreach (string file in files)
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    if (fileName != "current_config")
                    {
                        profiles.Add(fileName);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Fehler beim Auflisten der Profile: {e.Message}");
            }
            return profiles;
        }

        /// <summary>
        /// Erstellt eine Standard-Konfiguration
        /// </summary>
        private void CreateDefaultConfig()
        {
            currentConfig = CSVColumnConfig.CreateDefault();
            SaveCurrentConfig();
            Debug.Log("Standard-Konfiguration erstellt");
        }

        /// <summary>
        /// Lädt die MusterPlan-Konfiguration für DB Sauenplaner Export
        /// </summary>
        public void LoadMusterPlanConfig()
        {
            currentConfig = CSVColumnConfig.CreateMusterPlanConfig();
            SaveCurrentConfig();
            Debug.Log("MusterPlan-Konfiguration geladen");
        }

        /// <summary>
        /// Klont eine Konfiguration
        /// </summary>
        private CSVColumnConfig CloneConfig(CSVColumnConfig source)
        {
            string json = JsonUtility.ToJson(source);
            return JsonUtility.FromJson<CSVColumnConfig>(json);
        }

        /// <summary>
        /// Bereinigt Dateinamen von ungültigen Zeichen
        /// </summary>
        private string SanitizeFileName(string fileName)
        {
            char[] invalidChars = Path.GetInvalidFileNameChars();
            foreach (char c in invalidChars)
            {
                fileName = fileName.Replace(c, '_');
            }
            return fileName;
        }

        /// <summary>
        /// Importiert Spaltennamen aus einer CSV-Datei
        /// </summary>
        public bool ImportColumnsFromCSV(string csvFilePath)
        {
            if (!File.Exists(csvFilePath))
            {
                Debug.LogError($"CSV-Datei nicht gefunden: {csvFilePath}");
                return false;
            }

            try
            {
                string[] lines = File.ReadAllLines(csvFilePath);
                if (lines.Length == 0)
                {
                    Debug.LogError("CSV-Datei ist leer!");
                    return false;
                }

                // Erste Zeile als Header interpretieren
                string headerLine = lines[0];
                string[] columnNames = headerLine.Split(currentConfig.delimiter);

                // Neue Konfiguration erstellen
                CSVColumnConfig newConfig = new CSVColumnConfig();
                newConfig.configName = "Importiert_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
                newConfig.delimiter = currentConfig.delimiter;
                newConfig.hasHeader = true;
                newConfig.encoding = currentConfig.encoding;
                newConfig.csvFilePath = csvFilePath;

                for (int i = 0; i < columnNames.Length; i++)
                {
                    string colName = columnNames[i].Trim();
                    if (!string.IsNullOrEmpty(colName))
                    {
                        // Versuche automatisch Rolle zu erkennen
                        CSVColumnConfig.ColumnRole role = DetectColumnRole(colName);
                        CSVColumnConfig.ColumnType type = DetectColumnType(colName);
                        newConfig.AddColumn(colName, type, role);
                    }
                }

                currentConfig = newConfig;
                SaveCurrentConfig();
                Debug.Log($"Spalten aus CSV importiert: {columnNames.Length} Spalten");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Fehler beim Importieren der Spalten: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Versucht automatisch die Rolle einer Spalte zu erkennen
        /// </summary>
        private CSVColumnConfig.ColumnRole DetectColumnRole(string columnName)
        {
            string lower = columnName.ToLower();
            
            if (lower.Contains("ventil") || lower.Contains("bucht"))
                return CSVColumnConfig.ColumnRole.VentilNumber;
            if (lower.Contains("ohrmark") || lower.Contains("ear") || lower.Contains("tag"))
                return CSVColumnConfig.ColumnRole.EarTagNumber;
            if (lower.Contains("deck") || lower.Contains("mating"))
                return CSVColumnConfig.ColumnRole.MatingDate;
            if (lower.Contains("trächt") || lower.Contains("pregnan"))
                return CSVColumnConfig.ColumnRole.PregnancyStatus;
            if (lower.Contains("geburt") || lower.Contains("birth"))
                return CSVColumnConfig.ColumnRole.BirthDate;
            if (lower.Contains("gesund") || lower.Contains("health"))
                return CSVColumnConfig.ColumnRole.HealthStatus;
            if (lower.Contains("bemerk") || lower.Contains("note") || lower.Contains("comment"))
                return CSVColumnConfig.ColumnRole.Notes;
            
            return CSVColumnConfig.ColumnRole.None;
        }

        /// <summary>
        /// Versucht automatisch den Datentyp einer Spalte zu erkennen
        /// </summary>
        private CSVColumnConfig.ColumnType DetectColumnType(string columnName)
        {
            string lower = columnName.ToLower();
            
            if (lower.Contains("datum") || lower.Contains("date"))
                return CSVColumnConfig.ColumnType.Date;
            if (lower.Contains("nummer") || lower.Contains("number") || lower.Contains("anzahl"))
                return CSVColumnConfig.ColumnType.Number;
            if (lower.Contains("status") || lower.Contains("ja") || lower.Contains("nein"))
                return CSVColumnConfig.ColumnType.Boolean;
            
            return CSVColumnConfig.ColumnType.Text;
        }

        /// <summary>
        /// Exportiert die aktuelle Konfiguration als JSON-String
        /// </summary>
        public string ExportConfigAsJson()
        {
            if (currentConfig == null) return null;
            return JsonUtility.ToJson(currentConfig, true);
        }

        /// <summary>
        /// Setzt die aktuelle Konfiguration direkt
        /// </summary>
        public void SetConfig(CSVColumnConfig config)
        {
            if (config != null)
            {
                currentConfig = config;
                Debug.Log($"Konfiguration gesetzt: {config.configName}");
            }
        }

        /// <summary>
        /// Importiert eine Konfiguration aus JSON-String
        /// </summary>
        public bool ImportConfigFromJson(string json)
        {
            try
            {
                CSVColumnConfig importedConfig = JsonUtility.FromJson<CSVColumnConfig>(json);
                if (importedConfig.Validate(out string errorMessage))
                {
                    currentConfig = importedConfig;
                    SaveCurrentConfig();
                    Debug.Log("Konfiguration aus JSON importiert");
                    return true;
                }
                else
                {
                    Debug.LogError($"Ungültige Konfiguration: {errorMessage}");
                    return false;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Fehler beim Importieren der JSON-Konfiguration: {e.Message}");
                return false;
            }
        }
    }
}
