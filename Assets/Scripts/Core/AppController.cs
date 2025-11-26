using UnityEngine;
using D8PlanerXR.AR;

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
        [SerializeField] private string defaultCSVPath = "ImportDZ.csv";
        
        [Header("AR System")]
        [SerializeField] private QRCodeTracker qrTracker;
        [SerializeField] private VirtualDeckzentrum virtualDeckzentrum;
        
        [Header("App Info")]
        [SerializeField] private string appVersion = "1.0.0";
        [SerializeField] private bool showVersionInLog = true;
        
        private static AppController instance;
        public static AppController Instance => instance;

        // Öffentliche Properties
        public string AppVersion => appVersion;
        public QRCodeTracker QRTracker => qrTracker;
        public VirtualDeckzentrum VirtualDeckzentrum => virtualDeckzentrum;

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
            Debug.Log("=================================================");
            Debug.Log($"D8-Planer XR v{appVersion} wird initialisiert...");
            Debug.Log("=================================================");

            // Singletons initialisieren
            var configManager = Data.CSVConfigManager.Instance;
            var dataRepository = Data.DataRepository.Instance;

            // AR-Komponenten finden falls nicht zugewiesen
            if (qrTracker == null)
            {
                qrTracker = FindObjectOfType<QRCodeTracker>();
                if (qrTracker != null)
                {
                    Debug.Log("✓ QR-Code Tracker gefunden");
                }
                else
                {
                    Debug.LogWarning("! QR-Code Tracker nicht gefunden - Erstelle neue Instanz");
                }
            }

            if (virtualDeckzentrum == null)
            {
                virtualDeckzentrum = FindObjectOfType<VirtualDeckzentrum>();
                if (virtualDeckzentrum != null)
                {
                    Debug.Log("✓ Virtuelles Deckzentrum gefunden");
                }
            }

            // Standard-CSV laden falls vorhanden
            if (!string.IsNullOrEmpty(defaultCSVPath))
            {
                LoadDefaultCSV();
            }

            if (showVersionInLog)
            {
                Debug.Log($"✓ D8-Planer XR v{appVersion} erfolgreich initialisiert");
            }
            
            Debug.Log("=================================================");
        }

        /// <summary>
        /// Lädt die Standard-CSV-Datei
        /// </summary>
        private void LoadDefaultCSV()
        {
            string csvPath = System.IO.Path.Combine(Application.persistentDataPath, defaultCSVPath);
            
            if (System.IO.File.Exists(csvPath))
            {
                bool success = Data.DataRepository.Instance.ImportCSV(csvPath);
                if (success)
                {
                    Debug.Log($"✓ Standard-CSV geladen: {csvPath}");
                    Debug.Log($"  {Data.DataRepository.Instance.GetStatistics()}");
                }
                else
                {
                    Debug.LogError($"✗ Fehler beim Laden der Standard-CSV: {csvPath}");
                }
            }
            else
            {
                Debug.LogWarning($"! Standard-CSV nicht gefunden: {csvPath}");
                Debug.Log($"  Erwartet in: {Application.persistentDataPath}");
                Debug.Log($"  Lege die CSV-Datei '{defaultCSVPath}' in diesem Ordner ab.");
            }
        }

        /// <summary>
        /// Importiert eine neue CSV-Datei
        /// </summary>
        public bool ImportCSV(string filePath)
        {
            return Data.DataRepository.Instance.ImportCSV(filePath);
        }

        /// <summary>
        /// Schaltet das virtuelle Deckzentrum an/aus
        /// </summary>
        public void ToggleVirtualDeckzentrum()
        {
            if (virtualDeckzentrum != null)
            {
                bool currentState = virtualDeckzentrum.gameObject.activeSelf;
                virtualDeckzentrum.SetActive(!currentState);
                Debug.Log($"Virtuelles Deckzentrum: {(!currentState ? "AN" : "AUS")}");
            }
        }

        /// <summary>
        /// Startet den Setup-Modus für das Deckzentrum
        /// </summary>
        public void StartDeckzentrumSetup()
        {
            if (virtualDeckzentrum != null)
            {
                virtualDeckzentrum.StartSetupMode();
                Debug.Log("Deckzentrum Setup-Modus gestartet");
            }
        }

        /// <summary>
        /// Beendet den Setup-Modus
        /// </summary>
        public void FinishDeckzentrumSetup()
        {
            if (virtualDeckzentrum != null)
            {
                virtualDeckzentrum.FinishSetupMode();
                Debug.Log("Deckzentrum Setup-Modus beendet");
            }
        }

        /// <summary>
        /// Gibt Debug-Informationen aus
        /// </summary>
        [ContextMenu("Zeige System-Info")]
        public void ShowSystemInfo()
        {
            Debug.Log("=== D8-Planer XR System-Info ===");
            Debug.Log($"Version: {appVersion}");
            Debug.Log($"Platform: {Application.platform}");
            Debug.Log($"Data Path: {Application.persistentDataPath}");
            Debug.Log($"Unity Version: {Application.unityVersion}");
            Debug.Log("");
            Debug.Log("--- Daten-Repository ---");
            Debug.Log(Data.DataRepository.Instance.GetStatistics());
            Debug.Log("");
            Debug.Log("--- QR-Tracker ---");
            if (qrTracker != null)
            {
                var overlays = qrTracker.GetActiveOverlays();
                Debug.Log($"Aktive Overlays: {overlays.Count}");
            }
            else
            {
                Debug.Log("QR-Tracker nicht initialisiert");
            }
            Debug.Log("");
            Debug.Log("--- Virtuelles Deckzentrum ---");
            if (virtualDeckzentrum != null)
            {
                Debug.Log(virtualDeckzentrum.GetStatistics());
            }
            else
            {
                Debug.Log("Virtuelles Deckzentrum nicht initialisiert");
            }
            Debug.Log("================================");
        }

        /// <summary>
        /// Generiert Mock-Testdaten
        /// </summary>
        [ContextMenu("Generiere Mock-Daten")]
        public void GenerateMockData()
        {
            var mockGenerator = FindObjectOfType<Data.MockDataGenerator>();
            if (mockGenerator == null)
            {
                GameObject go = new GameObject("MockDataGenerator");
                mockGenerator = go.AddComponent<Data.MockDataGenerator>();
            }
            mockGenerator.SaveMockDataToFile();
            mockGenerator.CreateTestScenarios();
            Debug.Log("Mock-Daten generiert!");
        }
    }
}
