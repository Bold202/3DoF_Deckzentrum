using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using D8PlanerXR.Core;
using D8PlanerXR.Data;

namespace D8PlanerXR.UI
{
    /// <summary>
    /// Start Screen UI Controller
    /// Provides:
    /// - "Datei Öffnen" button to import CSV files into the database
    /// - "Modus starten" button to start VR/Camera mode based on device
    /// </summary>
    public class StartScreenUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject startScreenPanel;
        [SerializeField] private Button openFileButton;
        [SerializeField] private Button startModeButton;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private TextMeshProUGUI databaseInfoText;
        [SerializeField] private TextMeshProUGUI modeButtonText;
        
        [Header("File Picker UI")]
        [SerializeField] private GameObject filePickerPanel;
        [SerializeField] private ScrollRect fileListScrollView;
        [SerializeField] private Transform fileListContent;
        [SerializeField] private GameObject fileItemPrefab;
        [SerializeField] private Button filePickerCloseButton;
        [SerializeField] private TextMeshProUGUI filePickerTitleText;
        
        [Header("Settings")]
        [SerializeField] private string appTitle = "D8-Planer XR";
        [SerializeField] private string[] csvSearchPaths = new string[]
        {
            "", // persistentDataPath will be added
            "Download",
            "Documents",
            "DCIM"
        };
        
        [Header("References")]
        [SerializeField] private GameObject mainCameraScene;
        [SerializeField] private GameObject vrScene;
        
        // Internal state
        private bool isInitialized = false;
        private string selectedFilePath = "";
        
        // Singleton
        private static StartScreenUI instance;
        public static StartScreenUI Instance => instance;
        
        // Events
        public event Action OnModeStarted;
        public event Action<string> OnFileImported;
        
        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
        }
        
        private void Start()
        {
            Initialize();
        }
        
        /// <summary>
        /// Initialize the start screen
        /// </summary>
        private void Initialize()
        {
            if (isInitialized) return;
            
            Debug.Log("[StartScreenUI] Initializing...");
            
            // Set title
            if (titleText != null)
            {
                titleText.text = appTitle;
            }
            
            // Setup button listeners
            SetupButtonListeners();
            
            // Update mode button text based on device
            UpdateModeButtonText();
            
            // Update database info
            UpdateDatabaseInfo();
            
            // Subscribe to database events
            if (SowDatabase.Instance != null)
            {
                SowDatabase.Instance.OnDatabaseUpdated += OnDatabaseUpdated;
                SowDatabase.Instance.OnImportCompleted += OnImportCompleted;
                SowDatabase.Instance.OnError += OnDatabaseError;
            }
            
            // Hide file picker initially
            if (filePickerPanel != null)
            {
                filePickerPanel.SetActive(false);
            }
            
            // Show start screen
            if (startScreenPanel != null)
            {
                startScreenPanel.SetActive(true);
            }
            
            // Update status
            UpdateStatus("Bereit. Bitte CSV-Datei importieren oder Modus starten.");
            
            isInitialized = true;
            Debug.Log("[StartScreenUI] Initialized");
        }
        
        /// <summary>
        /// Setup button click listeners
        /// </summary>
        private void SetupButtonListeners()
        {
            if (openFileButton != null)
            {
                openFileButton.onClick.AddListener(OnOpenFileClicked);
            }
            
            if (startModeButton != null)
            {
                startModeButton.onClick.AddListener(OnStartModeClicked);
            }
            
            if (filePickerCloseButton != null)
            {
                filePickerCloseButton.onClick.AddListener(CloseFilePicker);
            }
        }
        
        /// <summary>
        /// Update the mode button text based on detected device
        /// </summary>
        private void UpdateModeButtonText()
        {
            if (modeButtonText == null) return;
            
            bool isVRDevice = IsVRDevice();
            
            if (isVRDevice)
            {
                modeButtonText.text = "🕶️ VR-Modus starten";
            }
            else
            {
                modeButtonText.text = "📱 Kamera-Modus starten";
            }
        }
        
        /// <summary>
        /// Check if running on a VR device
        /// </summary>
        private bool IsVRDevice()
        {
            if (DeviceModeManager.Instance != null)
            {
                return DeviceModeManager.Instance.IsVRMode;
            }
            
            // Fallback detection
            string deviceModel = SystemInfo.deviceModel.ToLower();
            return deviceModel.Contains("viture") || deviceModel.Contains("neckband");
        }
        
        /// <summary>
        /// Update database info display
        /// </summary>
        private void UpdateDatabaseInfo()
        {
            if (databaseInfoText == null) return;
            
            if (SowDatabase.Instance != null)
            {
                databaseInfoText.text = SowDatabase.Instance.GetStatistics();
            }
            else
            {
                databaseInfoText.text = "Datenbank nicht verfügbar";
            }
        }
        
        /// <summary>
        /// Update status text
        /// </summary>
        private void UpdateStatus(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }
            Debug.Log($"[StartScreenUI] Status: {message}");
        }
        
        /// <summary>
        /// Handle "Datei Öffnen" button click
        /// </summary>
        private void OnOpenFileClicked()
        {
            Debug.Log("[StartScreenUI] Open file clicked");
            
#if UNITY_ANDROID && !UNITY_EDITOR
            // On Android, use native file picker
            OpenAndroidFilePicker();
#else
            // On other platforms, show our custom file picker
            ShowFilePicker();
#endif
        }
        
        /// <summary>
        /// Open Android native file picker
        /// </summary>
        private void OpenAndroidFilePicker()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                UpdateStatus("Datei-Auswahl wird geöffnet...");
                
                // Use Unity's native file picker through Android Intent
                AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent");
                AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent");
                
                // ACTION_GET_CONTENT
                intent.Call<AndroidJavaObject>("setAction", "android.intent.action.GET_CONTENT");
                intent.Call<AndroidJavaObject>("setType", "*/*");
                intent.Call<AndroidJavaObject>("addCategory", "android.intent.category.OPENABLE");
                
                // Use NativeFilePicker plugin or fallback to custom picker
                ShowFilePicker();
            }
            catch (Exception e)
            {
                Debug.LogError($"[StartScreenUI] Error opening Android file picker: {e.Message}");
                // Fallback to custom file picker
                ShowFilePicker();
            }
#endif
        }
        
        /// <summary>
        /// Show custom file picker
        /// </summary>
        private void ShowFilePicker()
        {
            if (filePickerPanel == null)
            {
                // If no file picker panel, try to load from known paths
                TryLoadFromKnownPaths();
                return;
            }
            
            filePickerPanel.SetActive(true);
            
            if (filePickerTitleText != null)
            {
                filePickerTitleText.text = "CSV-Datei auswählen";
            }
            
            PopulateFileList();
        }
        
        /// <summary>
        /// Close file picker
        /// </summary>
        private void CloseFilePicker()
        {
            if (filePickerPanel != null)
            {
                filePickerPanel.SetActive(false);
            }
        }
        
        /// <summary>
        /// Populate file list in the file picker
        /// </summary>
        private void PopulateFileList()
        {
            if (fileListContent == null) return;
            
            // Clear existing items
            foreach (Transform child in fileListContent)
            {
                Destroy(child.gameObject);
            }
            
            // Search for CSV files in known paths
            string[] searchPaths = GetSearchPaths();
            
            foreach (string basePath in searchPaths)
            {
                if (!Directory.Exists(basePath)) continue;
                
                try
                {
                    string[] csvFiles = Directory.GetFiles(basePath, "*.csv", SearchOption.TopDirectoryOnly);
                    
                    foreach (string filePath in csvFiles)
                    {
                        CreateFileListItem(filePath);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[StartScreenUI] Error searching {basePath}: {e.Message}");
                }
            }
            
            // Also check the Import folder in the app
            string importPath = Path.Combine(Application.dataPath, "..", "Import");
            if (Directory.Exists(importPath))
            {
                try
                {
                    string[] csvFiles = Directory.GetFiles(importPath, "*.csv");
                    foreach (string filePath in csvFiles)
                    {
                        CreateFileListItem(filePath);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[StartScreenUI] Error searching Import folder: {e.Message}");
                }
            }
        }
        
        /// <summary>
        /// Get search paths for CSV files
        /// </summary>
        private string[] GetSearchPaths()
        {
            var paths = new System.Collections.Generic.List<string>();
            
            // Add persistent data path
            paths.Add(Application.persistentDataPath);
            
#if UNITY_ANDROID && !UNITY_EDITOR
            // Add Android external storage paths
            string externalStorage = "/storage/emulated/0";
            paths.Add(externalStorage);
            paths.Add(Path.Combine(externalStorage, "Download"));
            paths.Add(Path.Combine(externalStorage, "Documents"));
            paths.Add(Path.Combine(externalStorage, "DCIM"));
            
            // Add app-specific external directory
            try
            {
                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    using (var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                    {
                        using (var externalFilesDir = currentActivity.Call<AndroidJavaObject>("getExternalFilesDir", (string)null))
                        {
                            if (externalFilesDir != null)
                            {
                                paths.Add(externalFilesDir.Call<string>("getAbsolutePath"));
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[StartScreenUI] Could not get external files dir: {e.Message}");
            }
#else
            // Add common paths for editor/desktop
            paths.Add(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
            paths.Add(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            paths.Add(Path.Combine(Application.dataPath, "..", "Import"));
#endif
            
            return paths.ToArray();
        }
        
        /// <summary>
        /// Create a file list item in the picker
        /// </summary>
        private void CreateFileListItem(string filePath)
        {
            if (fileListContent == null) return;
            
            GameObject item;
            if (fileItemPrefab != null)
            {
                item = Instantiate(fileItemPrefab, fileListContent);
            }
            else
            {
                // Create a simple button if no prefab
                item = new GameObject("FileItem");
                item.transform.SetParent(fileListContent);
                
                var button = item.AddComponent<Button>();
                var text = item.AddComponent<TextMeshProUGUI>();
                text.text = Path.GetFileName(filePath);
                text.fontSize = 14;
                text.color = Color.white;
                
                var rect = item.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(300, 40);
            }
            
            // Setup click handler
            var btn = item.GetComponent<Button>();
            if (btn != null)
            {
                string path = filePath; // Capture for closure
                btn.onClick.AddListener(() => OnFileSelected(path));
            }
            
            // Set file name text
            var textComponent = item.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = Path.GetFileName(filePath);
            }
        }
        
        /// <summary>
        /// Handle file selection
        /// </summary>
        private void OnFileSelected(string filePath)
        {
            Debug.Log($"[StartScreenUI] File selected: {filePath}");
            selectedFilePath = filePath;
            
            CloseFilePicker();
            ImportCSVFile(filePath);
        }
        
        /// <summary>
        /// Try to load CSV from known paths (fallback when no file picker)
        /// </summary>
        private void TryLoadFromKnownPaths()
        {
            string[] paths = GetSearchPaths();
            
            foreach (string basePath in paths)
            {
                if (!Directory.Exists(basePath)) continue;
                
                try
                {
                    string[] csvFiles = Directory.GetFiles(basePath, "*.csv", SearchOption.TopDirectoryOnly);
                    if (csvFiles.Length > 0)
                    {
                        // Ask user to confirm import of first found file
                        string firstFile = csvFiles[0];
                        UpdateStatus($"Gefunden: {Path.GetFileName(firstFile)}. Wird importiert...");
                        ImportCSVFile(firstFile);
                        return;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[StartScreenUI] Error: {e.Message}");
                }
            }
            
            UpdateStatus("Keine CSV-Dateien gefunden. Bitte Datei in App-Ordner kopieren.");
        }
        
        /// <summary>
        /// Import a CSV file into the database
        /// </summary>
        public void ImportCSVFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                UpdateStatus($"Datei nicht gefunden: {Path.GetFileName(filePath)}");
                return;
            }
            
            UpdateStatus($"Importiere {Path.GetFileName(filePath)}...");
            
            // Load MusterPlan config if not already loaded
            CSVConfigManager.Instance.LoadMusterPlanConfig();
            
            // Import to database
            bool success = SowDatabase.Instance.ImportFromCSV(filePath);
            
            if (success)
            {
                UpdateStatus($"Import erfolgreich: {Path.GetFileName(filePath)}");
                OnFileImported?.Invoke(filePath);
            }
            else
            {
                UpdateStatus($"Import fehlgeschlagen: {Path.GetFileName(filePath)}");
            }
            
            UpdateDatabaseInfo();
        }
        
        /// <summary>
        /// Handle "Modus starten" button click
        /// </summary>
        private void OnStartModeClicked()
        {
            Debug.Log("[StartScreenUI] Start mode clicked");
            
            // Check if we have data
            if (SowDatabase.Instance != null && SowDatabase.Instance.TotalRecords == 0)
            {
                // Warn user but allow to proceed
                UpdateStatus("Warnung: Keine Daten geladen. Importieren Sie zuerst eine CSV-Datei.");
            }
            
            // Sync database to DataRepository before starting
            if (SowDatabase.Instance != null)
            {
                SowDatabase.Instance.SyncToDataRepository();
            }
            
            // Hide start screen
            if (startScreenPanel != null)
            {
                startScreenPanel.SetActive(false);
            }
            
            // Start appropriate mode
            StartAppropriateMode();
            
            OnModeStarted?.Invoke();
        }
        
        /// <summary>
        /// Start the appropriate mode based on device
        /// </summary>
        private void StartAppropriateMode()
        {
            bool isVR = IsVRDevice();
            
            if (DeviceModeManager.Instance != null)
            {
                if (isVR)
                {
                    DeviceModeManager.Instance.SwitchMode(DeviceModeManager.DeviceMode.VRMode);
                    Debug.Log("[StartScreenUI] Starting VR mode");
                }
                else
                {
                    DeviceModeManager.Instance.SwitchMode(DeviceModeManager.DeviceMode.MobileMode);
                    Debug.Log("[StartScreenUI] Starting Mobile/Camera mode");
                }
            }
            
            // Activate the appropriate scene/camera
            if (isVR && vrScene != null)
            {
                vrScene.SetActive(true);
            }
            else if (!isVR && mainCameraScene != null)
            {
                mainCameraScene.SetActive(true);
            }
            
            // Start camera controller if in mobile mode
            if (!isVR)
            {
                var mobileCamera = FindObjectOfType<D8PlanerXR.Mobile.MobileCameraController>();
                if (mobileCamera != null)
                {
                    mobileCamera.StartCamera();
                }
            }
        }
        
        /// <summary>
        /// Show the start screen again
        /// </summary>
        public void ShowStartScreen()
        {
            if (startScreenPanel != null)
            {
                startScreenPanel.SetActive(true);
            }
            
            UpdateDatabaseInfo();
            UpdateStatus("Bereit.");
        }
        
        #region Event Handlers
        
        private void OnDatabaseUpdated()
        {
            UpdateDatabaseInfo();
        }
        
        private void OnImportCompleted(string message)
        {
            UpdateStatus(message);
        }
        
        private void OnDatabaseError(string error)
        {
            UpdateStatus($"Fehler: {error}");
        }
        
        #endregion
        
        private void OnDestroy()
        {
            if (SowDatabase.Instance != null)
            {
                SowDatabase.Instance.OnDatabaseUpdated -= OnDatabaseUpdated;
                SowDatabase.Instance.OnImportCompleted -= OnImportCompleted;
                SowDatabase.Instance.OnError -= OnDatabaseError;
            }
        }
        
#if UNITY_EDITOR
        [ContextMenu("Show Start Screen")]
        private void EditorShowStartScreen()
        {
            ShowStartScreen();
        }
        
        [ContextMenu("Update Database Info")]
        private void EditorUpdateDatabaseInfo()
        {
            UpdateDatabaseInfo();
        }
#endif
    }
}
