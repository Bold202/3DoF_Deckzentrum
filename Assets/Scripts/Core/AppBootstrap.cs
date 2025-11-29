using System.Collections;
using UnityEngine;
using D8PlanerXR.Data;
using D8PlanerXR.Mobile;

namespace D8PlanerXR.Core
{
    /// <summary>
    /// AppBootstrap - Ensures proper initialization of the D8-Planer XR application
    /// This component handles the startup sequence and ensures mobile mode is correctly configured
    /// </summary>
    [DefaultExecutionOrder(-100)] // Run before other scripts
    public class AppBootstrap : MonoBehaviour
    {
        [Header("Startup Settings")]
        [SerializeField] private bool autoStartMobileMode = true;
        [SerializeField] private bool loadDefaultCSVOnStart = true;
        [SerializeField] private string defaultCSVFileName = "MusterPlan.csv";
        [SerializeField] private float startupDelay = 0.5f;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        
        private static AppBootstrap instance;
        public static AppBootstrap Instance => instance;
        
        private bool isInitialized = false;
        public bool IsInitialized => isInitialized;
        
        // Events
        public event System.Action OnBootstrapComplete;
        public event System.Action<string> OnBootstrapError;
        
        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            LogDebug("AppBootstrap Awake - Starting initialization sequence");
        }
        
        private void Start()
        {
            StartCoroutine(InitializationSequence());
        }
        
        /// <summary>
        /// Main initialization sequence
        /// </summary>
        private IEnumerator InitializationSequence()
        {
            LogDebug("=== D8-Planer XR Startup ===");
            LogDebug($"Platform: {Application.platform}");
            LogDebug($"Device: {SystemInfo.deviceModel}");
            
            // Wait for Unity to fully initialize
            yield return new WaitForSeconds(startupDelay);
            
            // Step 1: Initialize Device Mode Manager
            LogDebug("Step 1: Initializing Device Mode Manager...");
            yield return InitializeDeviceModeManager();
            
            // Step 2: Initialize Data Systems
            LogDebug("Step 2: Initializing Data Systems...");
            yield return InitializeDataSystems();
            
            // Step 3: Load CSV Data
            if (loadDefaultCSVOnStart)
            {
                LogDebug("Step 3: Loading CSV Data...");
                yield return LoadCSVData();
            }
            
            // Step 4: Initialize Mobile Camera (if in mobile mode)
            if (autoStartMobileMode && DeviceModeManager.Instance != null && DeviceModeManager.Instance.IsMobileMode)
            {
                LogDebug("Step 4: Initializing Mobile Camera...");
                yield return InitializeMobileCamera();
            }
            
            // Step 5: Request Permissions
            LogDebug("Step 5: Checking Permissions...");
            yield return CheckAndRequestPermissions();
            
            isInitialized = true;
            LogDebug("=== Initialization Complete ===");
            OnBootstrapComplete?.Invoke();
        }
        
        /// <summary>
        /// Initialize the DeviceModeManager
        /// </summary>
        private IEnumerator InitializeDeviceModeManager()
        {
            // Ensure DeviceModeManager exists
            if (DeviceModeManager.Instance == null)
            {
                GameObject dmObj = new GameObject("DeviceModeManager");
                dmObj.AddComponent<DeviceModeManager>();
                LogDebug("  DeviceModeManager created");
            }
            else
            {
                LogDebug("  DeviceModeManager already exists");
            }
            
            // Force mobile mode if auto-start is enabled
            if (autoStartMobileMode)
            {
                DeviceModeManager.Instance.SwitchMode(DeviceModeManager.DeviceMode.MobileMode);
                LogDebug("  Mobile mode activated");
            }
            
            yield return null;
        }
        
        /// <summary>
        /// Initialize data systems (Repository, Database, Config)
        /// </summary>
        private IEnumerator InitializeDataSystems()
        {
            // Initialize CSVConfigManager
            var configManager = CSVConfigManager.Instance;
            LogDebug($"  CSVConfigManager initialized: {(configManager.CurrentConfig != null ? configManager.CurrentConfig.configName : "No config")}");
            
            // Initialize DataRepository
            var dataRepo = DataRepository.Instance;
            LogDebug($"  DataRepository initialized: {dataRepo.TotalSows} sows, {dataRepo.TotalVentils} ventils");
            
            // Initialize SowDatabase
            var sowDb = SowDatabase.Instance;
            LogDebug($"  SowDatabase initialized: {sowDb.TotalRecords} records");
            
            yield return null;
        }
        
        /// <summary>
        /// Load CSV data from default file
        /// </summary>
        private IEnumerator LoadCSVData()
        {
            // Configure for MusterPlan format first
            CSVConfigManager.Instance.LoadMusterPlanConfig();
            LogDebug("  MusterPlan configuration loaded");
            
            // Try to find and load the CSV file from various locations
            string[] possiblePaths = GetPossibleCSVPaths();
            
            bool loaded = false;
            foreach (string path in possiblePaths)
            {
                if (System.IO.File.Exists(path))
                {
                    LogDebug($"  Found CSV at: {path}");
                    
                    // Import via SowDatabase (which syncs to DataRepository)
                    bool success = SowDatabase.Instance.ImportFromCSV(path);
                    
                    if (success)
                    {
                        LogDebug($"  CSV loaded successfully: {DataRepository.Instance.TotalSows} sows, {DataRepository.Instance.TotalVentils} ventils");
                        loaded = true;
                        break;
                    }
                    else
                    {
                        LogDebug($"  Failed to load CSV from: {path}");
                    }
                }
            }
            
            if (!loaded)
            {
                LogDebug($"  CSV not found. Expected locations:");
                foreach (string path in possiblePaths)
                {
                    LogDebug($"    - {path}");
                }
                
                // Generate mock data for testing
                LogDebug("  Generating mock data for testing...");
                GenerateMockData();
            }
            
            yield return null;
        }
        
        /// <summary>
        /// Get possible CSV file paths
        /// </summary>
        private string[] GetPossibleCSVPaths()
        {
            return new string[]
            {
                // Primary location - persistent data path
                System.IO.Path.Combine(Application.persistentDataPath, defaultCSVFileName),
                
                // StreamingAssets (bundled with app)
                System.IO.Path.Combine(Application.streamingAssetsPath, defaultCSVFileName),
                
                // Import folder (Editor testing)
                System.IO.Path.Combine(Application.dataPath, "Import", defaultCSVFileName),
                
                // Project root Import folder (Editor)
                System.IO.Path.Combine(Application.dataPath, "..", "Import", defaultCSVFileName),
                
                // Legacy default name
                System.IO.Path.Combine(Application.persistentDataPath, "ImportDZ.csv")
            };
        }
        
        /// <summary>
        /// Generate mock data for testing
        /// </summary>
        private void GenerateMockData()
        {
            // Create some test data based on typical MusterPlan format
            for (int ventil = 1; ventil <= 5; ventil++)
            {
                for (int sow = 0; sow < 3; sow++)
                {
                    SowData data = new SowData
                    {
                        earTagNumber = $"{(600 + ventil * 10 + sow):000}",
                        ventilNumber = ventil,
                        matingDate = System.DateTime.Now.AddDays(-(80 + ventil * 10 + sow * 5)),
                        daysSinceMating = 80 + ventil * 10 + sow * 5,
                        pregnancyStatus = "tragend",
                        healthStatus = sow == 2 && ventil == 3 ? "Medikation" : ""
                    };
                    
                    // Calculate traffic light
                    if (!string.IsNullOrEmpty(data.healthStatus) && 
                        data.healthStatus.ToLower().Contains("medikation"))
                    {
                        data.trafficLight = SowData.TrafficLightColor.Purple;
                    }
                    else if (data.daysSinceMating >= 107)
                    {
                        data.trafficLight = SowData.TrafficLightColor.Red;
                    }
                    else if (data.daysSinceMating >= 80)
                    {
                        data.trafficLight = SowData.TrafficLightColor.Yellow;
                    }
                    else
                    {
                        data.trafficLight = SowData.TrafficLightColor.Green;
                    }
                    
                    DataRepository.Instance.AddSowDataPublic(data);
                }
            }
            
            LogDebug($"  Mock data generated: {DataRepository.Instance.TotalSows} sows, {DataRepository.Instance.TotalVentils} ventils");
        }
        
        /// <summary>
        /// Initialize mobile camera
        /// </summary>
        private IEnumerator InitializeMobileCamera()
        {
            // Find or create MobileCameraController
            MobileCameraController cameraController = FindObjectOfType<MobileCameraController>();
            
            if (cameraController != null)
            {
                LogDebug("  MobileCameraController found, starting camera...");
                cameraController.StartCamera();
            }
            else
            {
                LogDebug("  MobileCameraController not found in scene");
            }
            
            yield return null;
        }
        
        /// <summary>
        /// Check and request necessary permissions
        /// </summary>
        private IEnumerator CheckAndRequestPermissions()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            // Use AndroidPermissionHandler
            AndroidPermissionHandler permHandler = AndroidPermissionHandler.Instance;
            if (permHandler == null)
            {
                try
                {
                    GameObject phObj = new GameObject("AndroidPermissionHandler");
                    phObj.transform.SetParent(transform); // Parent to bootstrap for proper cleanup
                    permHandler = phObj.AddComponent<AndroidPermissionHandler>();
                    
                    // Wait a frame for initialization
                    yield return null;
                }
                catch (System.Exception e)
                {
                    LogDebug($"  Warning: Failed to create AndroidPermissionHandler: {e.Message}");
                    yield break;
                }
            }
            
            bool permissionsReady = false;
            bool permissionsGranted = false;
            
            permHandler.RequestAllPermissions((success) => {
                permissionsReady = true;
                permissionsGranted = success;
                if (success)
                {
                    LogDebug("  All permissions granted");
                }
                else
                {
                    LogDebug("  Some permissions were denied");
                }
            });
            
            // Wait for permissions with timeout
            float timeout = 30f;
            while (!permissionsReady && timeout > 0)
            {
                yield return new WaitForSeconds(0.1f);
                timeout -= 0.1f;
            }
#else
            LogDebug("  Permissions: Not required on this platform");
            yield return null;
#endif
        }
        
        /// <summary>
        /// Debug logging helper
        /// </summary>
        private void LogDebug(string message)
        {
            if (showDebugLogs)
            {
                Debug.Log($"[AppBootstrap] {message}");
            }
        }
        
        /// <summary>
        /// Force reload data from CSV
        /// </summary>
        public void ReloadCSVData()
        {
            StartCoroutine(LoadCSVData());
        }
        
        /// <summary>
        /// Get status information
        /// </summary>
        public string GetStatusInfo()
        {
            return $"D8-Planer XR Status:\n" +
                   $"  Initialized: {isInitialized}\n" +
                   $"  Mode: {(DeviceModeManager.Instance?.CurrentMode.ToString() ?? "Unknown")}\n" +
                   $"  Sows: {DataRepository.Instance?.TotalSows ?? 0}\n" +
                   $"  Ventils: {DataRepository.Instance?.TotalVentils ?? 0}";
        }
        
#if UNITY_EDITOR
        [ContextMenu("Show Status")]
        private void EditorShowStatus()
        {
            Debug.Log(GetStatusInfo());
        }
        
        [ContextMenu("Reload CSV")]
        private void EditorReloadCSV()
        {
            ReloadCSVData();
        }
        
        [ContextMenu("Generate Mock Data")]
        private void EditorGenerateMockData()
        {
            DataRepository.Instance.ClearAllData();
            GenerateMockData();
        }
#endif
    }
}
