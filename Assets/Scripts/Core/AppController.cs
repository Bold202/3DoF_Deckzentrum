using UnityEngine;

namespace D8PlanerXR.Core
{
    /// <summary>
    /// Haupt-Controller für die D8-Planer XR Anwendung
    /// Koordiniert alle Hauptkomponenten und initialisiert das System
    /// </summary>
    public class AppController : MonoBehaviour
    {
        [Header("Komponenten-Referenzen")]
        [SerializeField] private bool initializeOnStart = true;
        [SerializeField] private string defaultCSVPath = "";
        
        private static AppController instance;
        public static AppController Instance => instance;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);

            if (initializeOnStart)
            {
                Initialize();
            }
        }

        /// <summary>
        /// Initialisiert die Anwendung
        /// </summary>
        public void Initialize()
        {
            Debug.Log("D8-Planer XR wird initialisiert...");

            // Singletons initialisieren
            var configManager = Data.CSVConfigManager.Instance;
            var dataRepository = Data.DataRepository.Instance;

            // Standard-CSV laden falls vorhanden
            if (!string.IsNullOrEmpty(defaultCSVPath))
            {
                LoadDefaultCSV();
            }

            Debug.Log("D8-Planer XR Initialisierung abgeschlossen");
        }

        /// <summary>
        /// Lädt die Standard-CSV-Datei
        /// </summary>
        private void LoadDefaultCSV()
        {
            string csvPath = System.IO.Path.Combine(Application.persistentDataPath, defaultCSVPath);
            
            if (System.IO.File.Exists(csvPath))
            {
                Data.DataRepository.Instance.ImportCSV(csvPath);
                Debug.Log($"Standard-CSV geladen: {csvPath}");
            }
            else
            {
                Debug.LogWarning($"Standard-CSV nicht gefunden: {csvPath}");
            }
        }

        /// <summary>
        /// Gibt Debug-Informationen aus
        /// </summary>
        [ContextMenu("Zeige System-Info")]
        public void ShowSystemInfo()
        {
            Debug.Log("=== D8-Planer XR System-Info ===");
            Debug.Log($"Platform: {Application.platform}");
            Debug.Log($"Data Path: {Application.persistentDataPath}");
            Debug.Log($"Version: {Application.version}");
            Debug.Log(Data.DataRepository.Instance.GetStatistics());
            Debug.Log("================================");
        }
    }
}
