using System;
using UnityEngine;
using D8PlanerXR.AR;

namespace D8PlanerXR.Core
{
    /// <summary>
    /// Haupt-Controller für die D8-Planer XR Anwendung.
    /// Koordiniert alle Hauptkomponenten und initialisiert das System.
    /// Unterstützt VR-Modus (Viture) und Handy-Modus.
    /// 
    /// ============================================================
    /// SZENEN-HIERARCHIE (für Unity Inspector-Verknüpfung):
    /// ============================================================
    /// 
    /// 1. AppController (Empty GameObject)
    ///    → Dieses Skript hierher ziehen
    ///    → Steuert den globalen App-State
    /// 
    /// 2. UI_Canvas (Canvas)
    ///    ├── MainMenu_Panel (Panel)
    ///    │   → Sichtbar bei Start, enthält Startmenü-UI
    ///    │   → Verknüpfen mit "Main Menu Panel"
    ///    └── AR_Overlay_Panel (Panel)  
    ///        → Sichtbar nur im AR-Modus, enthält "Zurück"-Button
    ///        → Verknüpfen mit "AR Overlay Panel"
    /// 
    /// 3. Standard_Environment (Empty GameObject)
    ///    └── Menu Camera (Camera)
    ///        → Tag: "Untagged", Rendering für Menü/Hintergrund
    ///        → Aktiv beim Start
    ///    → Verknüpfen mit "Standard Environment"
    /// 
    /// 4. AR_Environment (Empty GameObject)
    ///    ├── AR Session (AR Foundation)
    ///    ├── XR Origin (AR Foundation)
    ///    │   └── Camera Offset
    ///    │       └── Main Camera (Camera)
    ///    │           → Tag: "MainCamera", AR-Rendering
    ///    ├── QRCodeTracker (Custom Script)
    ///    │   → Verwaltet QR-Erkennung
    ///    │   → Verknüpfen mit "QR Tracker"
    ///    └── VirtualDeckzentrum (Transform/Anchor)
    ///        → Das Zielobjekt, an dem AR-Inhalte ausgerichtet werden
    ///        → Verknüpfen mit "Virtual Deckzentrum"
    ///    → Verknüpfen mit "AR Environment"
    /// 
    /// ============================================================
    /// </summary>
    public class AppController : MonoBehaviour
    {
        #region Serialized Fields - Inspector Referenzen

        [Header("=== SZENEN-OBJEKTE ===")]
        [Tooltip("Das Standard_Environment GameObject (enthält Menu Camera). Aktiv im Menü-Modus.")]
        [SerializeField] private GameObject standardEnvironment;
        
        [Tooltip("Das AR_Environment GameObject (enthält AR Session, XR Origin, QRCodeTracker). Aktiv im AR-Modus.")]
        [SerializeField] private GameObject arEnvironment;

        [Header("=== UI PANELS ===")]
        [Tooltip("Das MainMenu_Panel unter UI_Canvas. Sichtbar beim Start.")]
        [SerializeField] private GameObject mainMenuPanel;
        
        [Tooltip("Das AR_Overlay_Panel unter UI_Canvas. Sichtbar nur im AR-Modus.")]
        [SerializeField] private GameObject arOverlayPanel;

        [Header("=== AR KOMPONENTEN ===")]
        [Tooltip("QRCodeTracker Komponente (liegt unter AR_Environment).")]
        [SerializeField] private QRCodeTracker qrTracker;
        
        [Tooltip("VirtualDeckzentrum Komponente (liegt unter AR_Environment).")]
        [SerializeField] private VirtualDeckzentrum virtualDeckzentrum;
        
        [Tooltip("DeviceModeManager (kann separat oder am AppController liegen).")]
        [SerializeField] private DeviceModeManager deviceModeManager;

        [Header("=== EINSTELLUNGEN ===")]
        [SerializeField] private bool initializeOnStart = true;
        [SerializeField] private string defaultCSVPath = "ImportDZ.csv";
        
        [Header("=== APP INFO ===")]
        [SerializeField] private string appVersion = "1.2.0";
        [SerializeField] private bool showVersionInLog = true;

        #endregion

        #region Singleton

        private static AppController instance;
        public static AppController Instance => instance;

        #endregion

        #region State

        /// <summary>
        /// Definiert die möglichen App-Zustände
        /// </summary>
        public enum AppState
        {
            /// <summary>Standard-Menü-Modus, AR ist deaktiviert</summary>
            Menu,
            /// <summary>AR-Modus ist aktiv</summary>
            AR
        }

        private AppState currentState = AppState.Menu;

        /// <summary>Aktueller App-Zustand</summary>
        public AppState CurrentState => currentState;

        /// <summary>Gibt true zurück, wenn AR aktiv ist</summary>
        public bool IsARActive => currentState == AppState.AR;

        #endregion

        #region Events

        /// <summary>Wird ausgelöst, wenn AR gestartet wird</summary>
        public event Action OnARStarted;

        /// <summary>Wird ausgelöst, wenn AR gestoppt wird</summary>
        public event Action OnARStopped;

        /// <summary>Wird ausgelöst, wenn sich der App-Zustand ändert</summary>
        public event Action<AppState> OnStateChanged;

        #endregion

        #region Public Properties

        public string AppVersion => appVersion;
        public QRCodeTracker QRTracker => qrTracker;
        public VirtualDeckzentrum VirtualDeckzentrum => virtualDeckzentrum;
        public DeviceModeManager DeviceModeManager => deviceModeManager;
        
        /// <summary>Gibt true zurück, wenn alle Referenzen korrekt gesetzt sind</summary>
        public bool IsConfiguredCorrectly { get; private set; }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // Singleton Pattern
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);

            // Referenzen validieren
            ValidateReferences();

            if (initializeOnStart)
            {
                Initialize();
            }
        }

        private void Start()
        {
            // Initial-State setzen: Menü-Modus
            SetInitialState();
        }

        #endregion

        #region Reference Validation

        /// <summary>
        /// Validiert alle SerializeField-Referenzen und gibt Fehlermeldungen aus.
        /// </summary>
        private void ValidateReferences()
        {
            IsConfiguredCorrectly = true;
            
            Debug.Log("[AppController] Validiere Referenzen...");

            // Umgebungs-Objekte prüfen
            if (standardEnvironment == null)
            {
                Debug.LogError("[AppController] ✗ FEHLER: 'Standard Environment' ist nicht zugewiesen! " +
                    "Bitte das Standard_Environment GameObject im Inspector verknüpfen.");
                IsConfiguredCorrectly = false;
            }
            else
            {
                Debug.Log("[AppController] ✓ Standard Environment gefunden");
            }

            if (arEnvironment == null)
            {
                Debug.LogError("[AppController] ✗ FEHLER: 'AR Environment' ist nicht zugewiesen! " +
                    "Bitte das AR_Environment GameObject im Inspector verknüpfen.");
                IsConfiguredCorrectly = false;
            }
            else
            {
                Debug.Log("[AppController] ✓ AR Environment gefunden");
            }

            // UI Panels prüfen
            if (mainMenuPanel == null)
            {
                Debug.LogError("[AppController] ✗ FEHLER: 'Main Menu Panel' ist nicht zugewiesen! " +
                    "Bitte das MainMenu_Panel aus UI_Canvas im Inspector verknüpfen.");
                IsConfiguredCorrectly = false;
            }
            else
            {
                Debug.Log("[AppController] ✓ Main Menu Panel gefunden");
            }

            if (arOverlayPanel == null)
            {
                Debug.LogError("[AppController] ✗ FEHLER: 'AR Overlay Panel' ist nicht zugewiesen! " +
                    "Bitte das AR_Overlay_Panel aus UI_Canvas im Inspector verknüpfen.");
                IsConfiguredCorrectly = false;
            }
            else
            {
                Debug.Log("[AppController] ✓ AR Overlay Panel gefunden");
            }

            // AR Komponenten prüfen (Warnung, da diese auch automatisch gefunden werden können)
            if (qrTracker == null)
            {
                // Auch in inaktiven GameObjects suchen
                qrTracker = FindObjectOfType<QRCodeTracker>(true);
                if (qrTracker != null)
                {
                    Debug.Log("[AppController] ✓ QR Tracker automatisch gefunden");
                }
                else
                {
                    Debug.LogWarning("[AppController] ! QR Tracker nicht gefunden - wird später erstellt oder ist optional.");
                }
            }
            else
            {
                Debug.Log("[AppController] ✓ QR Tracker zugewiesen");
            }

            if (virtualDeckzentrum == null)
            {
                virtualDeckzentrum = FindObjectOfType<VirtualDeckzentrum>(true);
                if (virtualDeckzentrum != null)
                {
                    Debug.Log("[AppController] ✓ Virtual Deckzentrum automatisch gefunden");
                }
            }
            else
            {
                Debug.Log("[AppController] ✓ Virtual Deckzentrum zugewiesen");
            }

            if (deviceModeManager == null)
            {
                deviceModeManager = FindObjectOfType<DeviceModeManager>(true);
                if (deviceModeManager == null)
                {
                    // DeviceModeManager erstellen falls nicht vorhanden
                    GameObject dmObj = new GameObject("DeviceModeManager");
                    deviceModeManager = dmObj.AddComponent<DeviceModeManager>();
                    Debug.Log("[AppController] ✓ Device Mode Manager erstellt");
                }
                else
                {
                    Debug.Log("[AppController] ✓ Device Mode Manager automatisch gefunden");
                }
            }
            else
            {
                Debug.Log("[AppController] ✓ Device Mode Manager zugewiesen");
            }

            if (IsConfiguredCorrectly)
            {
                Debug.Log("[AppController] ✓ Alle Pflicht-Referenzen sind korrekt gesetzt!");
            }
            else
            {
                Debug.LogError("[AppController] ✗ Einige Referenzen fehlen! Die App funktioniert möglicherweise nicht korrekt.");
            }
        }

        #endregion

        #region State Machine

        /// <summary>
        /// Setzt den initialen Zustand: Menü-Modus aktiv, AR deaktiviert.
        /// </summary>
        private void SetInitialState()
        {
            Debug.Log("[AppController] Setze Initial-State: Menü-Modus");
            
            // Standard Environment aktivieren
            if (standardEnvironment != null)
            {
                standardEnvironment.SetActive(true);
            }

            // AR Environment deaktivieren (spart Akku, verhindert sofortige Permission-Anfragen)
            if (arEnvironment != null)
            {
                arEnvironment.SetActive(false);
            }

            // UI auf Menü-Modus setzen
            if (mainMenuPanel != null)
            {
                mainMenuPanel.SetActive(true);
            }

            if (arOverlayPanel != null)
            {
                arOverlayPanel.SetActive(false);
            }

            currentState = AppState.Menu;
        }

        /// <summary>
        /// Startet den AR-Modus.
        /// Deaktiviert Standard_Environment, aktiviert AR_Environment.
        /// Wechselt UI zu AR_Overlay.
        /// </summary>
        public void StartAR()
        {
            if (currentState == AppState.AR)
            {
                Debug.Log("[AppController] AR ist bereits aktiv.");
                return;
            }

            Debug.Log("[AppController] === STARTE AR-MODUS ===");

            // Standard Environment deaktivieren
            if (standardEnvironment != null)
            {
                standardEnvironment.SetActive(false);
                Debug.Log("[AppController] Standard Environment deaktiviert");
            }

            // AR Environment aktivieren
            if (arEnvironment != null)
            {
                arEnvironment.SetActive(true);
                Debug.Log("[AppController] AR Environment aktiviert");
            }

            // UI wechseln
            if (mainMenuPanel != null)
            {
                mainMenuPanel.SetActive(false);
            }

            if (arOverlayPanel != null)
            {
                arOverlayPanel.SetActive(true);
            }

            // QR-Tracker starten (falls vorhanden)
            if (qrTracker != null)
            {
                qrTracker.StartScanning();
                Debug.Log("[AppController] QR-Code Scanning gestartet");
            }

            // State aktualisieren
            currentState = AppState.AR;
            
            // Events auslösen
            OnARStarted?.Invoke();
            OnStateChanged?.Invoke(currentState);

            Debug.Log("[AppController] === AR-MODUS AKTIV ===");
        }

        /// <summary>
        /// Stoppt den AR-Modus und kehrt zum Menü zurück.
        /// Deaktiviert AR_Environment, aktiviert Standard_Environment.
        /// Wechselt UI zu MainMenu.
        /// </summary>
        public void StopAR()
        {
            if (currentState == AppState.Menu)
            {
                Debug.Log("[AppController] AR ist bereits deaktiviert.");
                return;
            }

            Debug.Log("[AppController] === STOPPE AR-MODUS ===");

            // QR-Tracker stoppen (falls vorhanden)
            if (qrTracker != null)
            {
                qrTracker.StopScanning();
                qrTracker.ClearAllOverlays();
                Debug.Log("[AppController] QR-Code Scanning gestoppt");
            }

            // AR Environment deaktivieren
            if (arEnvironment != null)
            {
                arEnvironment.SetActive(false);
                Debug.Log("[AppController] AR Environment deaktiviert");
            }

            // Standard Environment aktivieren
            if (standardEnvironment != null)
            {
                standardEnvironment.SetActive(true);
                Debug.Log("[AppController] Standard Environment aktiviert");
            }

            // UI wechseln
            if (arOverlayPanel != null)
            {
                arOverlayPanel.SetActive(false);
            }

            if (mainMenuPanel != null)
            {
                mainMenuPanel.SetActive(true);
            }

            // State aktualisieren
            currentState = AppState.Menu;
            
            // Events auslösen
            OnARStopped?.Invoke();
            OnStateChanged?.Invoke(currentState);

            Debug.Log("[AppController] === MENÜ-MODUS AKTIV ===");
        }

        /// <summary>
        /// Wechselt zwischen AR- und Menü-Modus.
        /// </summary>
        public void ToggleAR()
        {
            if (currentState == AppState.Menu)
            {
                StartAR();
            }
            else
            {
                StopAR();
            }
        }

        #endregion

        #region Initialization

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

            // Standard-CSV laden falls vorhanden
            if (!string.IsNullOrEmpty(defaultCSVPath))
            {
                LoadDefaultCSV();
            }

            if (showVersionInLog)
            {
                Debug.Log($"✓ D8-Planer XR v{appVersion} erfolgreich initialisiert");
                Debug.Log($"  Referenzen OK: {IsConfiguredCorrectly}");
                if (deviceModeManager != null)
                {
                    Debug.Log($"  Modus: {deviceModeManager.CurrentMode}");
                }
            }
            
            Debug.Log("=================================================");
        }

        #endregion

        #region CSV Import

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

        #endregion

        #region Virtual Deckzentrum Controls

        /// <summary>
        /// Schaltet das virtuelle Deckzentrum an/aus
        /// </summary>
        public void ToggleVirtualDeckzentrum()
        {
            if (virtualDeckzentrum != null)
            {
                bool currentActiveState = virtualDeckzentrum.gameObject.activeSelf;
                virtualDeckzentrum.SetActive(!currentActiveState);
                Debug.Log($"Virtuelles Deckzentrum: {(!currentActiveState ? "AN" : "AUS")}");
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

        #endregion

        #region Debug / System Info

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
            Debug.Log($"Aktueller State: {currentState}");
            Debug.Log($"AR aktiv: {IsARActive}");
            Debug.Log($"Referenzen OK: {IsConfiguredCorrectly}");
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

        #endregion

#if UNITY_EDITOR
        [ContextMenu("State: Starte AR")]
        private void EditorStartAR()
        {
            StartAR();
        }

        [ContextMenu("State: Stoppe AR")]
        private void EditorStopAR()
        {
            StopAR();
        }

        [ContextMenu("Validiere Referenzen")]
        private void EditorValidateReferences()
        {
            ValidateReferences();
        }
#endif
    }
}
