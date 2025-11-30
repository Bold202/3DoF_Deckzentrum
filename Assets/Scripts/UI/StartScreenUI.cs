using System;
using System.IO;
using System.Collections.Generic;
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
    /// - Access to app-specific external folder for file organization
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
        [SerializeField] private TextMeshProUGUI currentPathText;
        [SerializeField] private Button navigateUpButton;
        
        [Header("Settings")]
        [SerializeField] private string appTitle = "D8-Planer XR";
        [SerializeField] private string appFolderName = "D8PlanerXR";
        [SerializeField] private string[] csvSearchPaths = new string[]
        {
            "", // persistentDataPath will be added
            "Download",
            "Documents",
            "DCIM"
        };
        
        // External app folder path (accessible from file managers)
        private string externalAppFolder = "";
        private string currentBrowsePath = "";
        
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
            
            // Setup external app folder for file organization
            SetupExternalAppFolder();
            
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
            
            // Update status with folder info
            string statusMessage = "Bereit. Bitte CSV-Datei importieren oder Modus starten.";
            if (!string.IsNullOrEmpty(externalAppFolder))
            {
                statusMessage = $"{statusMessage}\nCSV-Ordner: {externalAppFolder}";
            }
            UpdateStatus(statusMessage);
            
            isInitialized = true;
            Debug.Log("[StartScreenUI] Initialized");
        }
        
        /// <summary>
        /// Setup external app folder that's accessible from file managers
        /// </summary>
        private void SetupExternalAppFolder()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                // Get external storage directory accessible to file managers
                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    using (var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                    {
                        // Get app-specific external files directory (accessible without special permissions on Android 10+)
                        using (var externalFilesDir = currentActivity.Call<AndroidJavaObject>("getExternalFilesDir", (string)null))
                        {
                            if (externalFilesDir != null)
                            {
                                externalAppFolder = externalFilesDir.Call<string>("getAbsolutePath");
                            }
                        }
                    }
                }
                
                // Create CSV subfolder
                if (!string.IsNullOrEmpty(externalAppFolder))
                {
                    string csvFolder = Path.Combine(externalAppFolder, "CSV");
                    if (!Directory.Exists(csvFolder))
                    {
                        Directory.CreateDirectory(csvFolder);
                        Debug.Log($"[StartScreenUI] Created CSV folder: {csvFolder}");
                    }
                    
                    // Create a README file to help users find the folder
                    string readmePath = Path.Combine(csvFolder, "ANLEITUNG.txt");
                    if (!File.Exists(readmePath))
                    {
                        string readme = "D8-Planer XR - CSV Import Ordner\n" +
                                       "================================\n\n" +
                                       "Legen Sie Ihre CSV-Dateien aus dem DB Sauenplaner hier ab.\n" +
                                       "Die App findet sie automatisch beim Öffnen.\n\n" +
                                       "Format: MusterPlan.csv (Semikolon-getrennt)\n";
                        File.WriteAllText(readmePath, readme);
                    }
                }
                
                Debug.Log($"[StartScreenUI] External app folder: {externalAppFolder}");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[StartScreenUI] Could not setup external folder: {e.Message}");
                externalAppFolder = Application.persistentDataPath;
            }
#else
            // On non-Android platforms, use persistent data path
            externalAppFolder = Application.persistentDataPath;
            
            // Create CSV subfolder
            string csvFolder = Path.Combine(externalAppFolder, "CSV");
            if (!Directory.Exists(csvFolder))
            {
                Directory.CreateDirectory(csvFolder);
            }
#endif
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
            
            if (navigateUpButton != null)
            {
                navigateUpButton.onClick.AddListener(OnNavigateUpClicked);
            }
        }
        
        /// <summary>
        /// Navigate up one directory level
        /// </summary>
        private void OnNavigateUpClicked()
        {
            if (string.IsNullOrEmpty(currentBrowsePath)) return;
            
            string parentPath = Directory.GetParent(currentBrowsePath)?.FullName;
            if (!string.IsNullOrEmpty(parentPath) && Directory.Exists(parentPath))
            {
                currentBrowsePath = parentPath;
                PopulateFileList();
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
                modeButtonText.text = "[VR] Modus starten";
            }
            else
            {
                modeButtonText.text = "[CAM] Kamera-Modus starten";
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
            
            // Start from external app folder if available, otherwise persistent data path
            if (string.IsNullOrEmpty(currentBrowsePath))
            {
                currentBrowsePath = !string.IsNullOrEmpty(externalAppFolder) 
                    ? externalAppFolder 
                    : Application.persistentDataPath;
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
            
            // Update current path display
            if (currentPathText != null)
            {
                currentPathText.text = currentBrowsePath ?? "Alle Ordner";
            }
            
            // If we have a specific browse path, show that folder
            if (!string.IsNullOrEmpty(currentBrowsePath) && Directory.Exists(currentBrowsePath))
            {
                PopulateFromPath(currentBrowsePath);
            }
            else
            {
                // Show all known paths with CSV files
                PopulateFromAllKnownPaths();
            }
        }
        
        /// <summary>
        /// Populate file list from a specific path
        /// </summary>
        private void PopulateFromPath(string path)
        {
            if (!Directory.Exists(path)) return;
            
            try
            {
                // Show directories first
                string[] directories = Directory.GetDirectories(path);
                foreach (string dirPath in directories)
                {
                    CreateDirectoryListItem(dirPath);
                }
                
                // Then show CSV files
                string[] csvFiles = Directory.GetFiles(path, "*.csv", SearchOption.TopDirectoryOnly);
                foreach (string filePath in csvFiles)
                {
                    CreateFileListItem(filePath);
                }
                
                if (directories.Length == 0 && csvFiles.Length == 0)
                {
                    CreateInfoItem("Keine CSV-Dateien in diesem Ordner");
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[StartScreenUI] Error populating from {path}: {e.Message}");
                CreateInfoItem($"Fehler: {e.Message}");
            }
        }
        
        /// <summary>
        /// Populate from all known paths
        /// </summary>
        private void PopulateFromAllKnownPaths()
        {
            // Search for CSV files in known paths
            string[] searchPaths = GetSearchPaths();
            bool foundAnyFile = false;
            
            foreach (string basePath in searchPaths)
            {
                if (!Directory.Exists(basePath)) continue;
                
                try
                {
                    string[] csvFiles = Directory.GetFiles(basePath, "*.csv", SearchOption.TopDirectoryOnly);
                    
                    foreach (string filePath in csvFiles)
                    {
                        CreateFileListItem(filePath);
                        foundAnyFile = true;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[StartScreenUI] Error searching {basePath}: {e.Message}");
                }
            }
            
#if UNITY_EDITOR
            // Only check Import folder in editor mode where Application.dataPath is valid
            string importPath = Path.Combine(Application.dataPath, "..", "Import");
            if (Directory.Exists(importPath))
            {
                try
                {
                    string[] csvFiles = Directory.GetFiles(importPath, "*.csv");
                    foreach (string filePath in csvFiles)
                    {
                        CreateFileListItem(filePath);
                        foundAnyFile = true;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[StartScreenUI] Error searching Import folder: {e.Message}");
                }
            }
#endif
            
            if (!foundAnyFile)
            {
                CreateInfoItem("Keine CSV-Dateien gefunden.\nLegen Sie Dateien im App-Ordner ab.");
            }
        }
        
        /// <summary>
        /// Create an info/message item in the list
        /// </summary>
        private void CreateInfoItem(string message)
        {
            if (fileListContent == null) return;
            
            GameObject item = new GameObject("InfoItem");
            item.transform.SetParent(fileListContent);
            
            var text = item.AddComponent<TextMeshProUGUI>();
            text.text = message;
            text.fontSize = 14;
            text.color = Color.gray;
            text.alignment = TextAlignmentOptions.Center;
            
            var rect = item.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(300, 60);
            rect.localScale = Vector3.one;
        }
        
        /// <summary>
        /// Create a directory item in the list
        /// </summary>
        private void CreateDirectoryListItem(string dirPath)
        {
            if (fileListContent == null) return;
            
            GameObject item;
            if (fileItemPrefab != null)
            {
                item = Instantiate(fileItemPrefab, fileListContent);
            }
            else
            {
                item = new GameObject("DirItem");
                item.transform.SetParent(fileListContent);
                
                var button = item.AddComponent<Button>();
                var text = item.AddComponent<TextMeshProUGUI>();
                text.text = "[Ordner] " + Path.GetFileName(dirPath);
                text.fontSize = 14;
                text.color = new Color(0.3f, 0.6f, 1f); // Blue for folders
                
                var rect = item.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(300, 40);
                rect.localScale = Vector3.one;
            }
            
            // Setup click handler
            var btn = item.GetComponent<Button>();
            if (btn != null)
            {
                string path = dirPath; // Capture for closure
                btn.onClick.AddListener(() => OnDirectorySelected(path));
            }
            
            // Set directory name text
            var textComponent = item.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = "📁 " + Path.GetFileName(dirPath);
                textComponent.color = new Color(0.3f, 0.6f, 1f);
            }
        }
        
        /// <summary>
        /// Handle directory selection (navigate into folder)
        /// </summary>
        private void OnDirectorySelected(string dirPath)
        {
            currentBrowsePath = dirPath;
            PopulateFileList();
        }
        
        /// <summary>
        /// Get search paths for CSV files
        /// </summary>
        private string[] GetSearchPaths()
        {
            var paths = new List<string>();
            
            // Add external app folder first (most accessible)
            if (!string.IsNullOrEmpty(externalAppFolder))
            {
                paths.Add(externalAppFolder);
                string csvSubfolder = Path.Combine(externalAppFolder, "CSV");
                if (Directory.Exists(csvSubfolder))
                {
                    paths.Add(csvSubfolder);
                }
            }
            
            // Add persistent data path (always available)
            paths.Add(Application.persistentDataPath);
            
            // Add streaming assets path
            if (!string.IsNullOrEmpty(Application.streamingAssetsPath))
            {
                paths.Add(Application.streamingAssetsPath);
            }
            
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
#elif UNITY_EDITOR
            // Add common paths for editor/desktop
            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            
            if (!string.IsNullOrEmpty(desktop))
                paths.Add(desktop);
            if (!string.IsNullOrEmpty(documents))
                paths.Add(documents);
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
