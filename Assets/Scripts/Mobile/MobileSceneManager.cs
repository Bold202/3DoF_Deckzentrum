using UnityEngine;
using UnityEngine.UI;
using TMPro;
using D8PlanerXR.Core;
using D8PlanerXR.Data;

namespace D8PlanerXR.Mobile
{
    /// <summary>
    /// Mobile Scene Manager - Sets up and manages the mobile scanning scene
    /// Creates the camera display, UI elements, and handles scene initialization
    /// </summary>
    public class MobileSceneManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MobileCameraController cameraController;
        [SerializeField] private Canvas mainCanvas;
        [SerializeField] private RawImage cameraDisplay;
        [SerializeField] private GameObject menuPanel;
        [SerializeField] private Button menuButton;
        [SerializeField] private Button loadCSVButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private TMPro.TextMeshProUGUI appTitleText;
        
        [Header("Info Panel")]
        [SerializeField] private GameObject infoPanel;
        [SerializeField] private TMPro.TextMeshProUGUI infoText;
        
        [Header("Settings")]
        [SerializeField] private bool autoLoadDefaultCSV = true;
        [SerializeField] private string defaultCSVFileName = "MusterPlan.csv";
        
        private static MobileSceneManager instance;
        public static MobileSceneManager Instance => instance;
        
        private bool isMenuOpen = false;
        
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
            InitializeScene();
            
            if (autoLoadDefaultCSV)
            {
                LoadDefaultCSV();
            }
            
            // Setup button listeners
            SetupButtonListeners();
            
            // Subscribe to camera events
            if (cameraController != null)
            {
                cameraController.OnVentilDetected += OnVentilDetected;
            }
        }
        
        private void OnDestroy()
        {
            if (cameraController != null)
            {
                cameraController.OnVentilDetected -= OnVentilDetected;
            }
        }
        
        /// <summary>
        /// Initialize the mobile scene
        /// </summary>
        private void InitializeScene()
        {
            Debug.Log("[MobileSceneManager] Initializing mobile scene...");
            
            // Ensure we're in mobile mode
            if (DeviceModeManager.Instance != null)
            {
                if (!DeviceModeManager.Instance.IsMobileMode)
                {
                    DeviceModeManager.Instance.SwitchMode(DeviceModeManager.DeviceMode.MobileMode);
                }
            }
            
            // Hide menu initially
            if (menuPanel != null)
            {
                menuPanel.SetActive(false);
            }
            
            // Update app title
            if (appTitleText != null)
            {
                appTitleText.text = "D8-Planer XR";
            }
            
            Debug.Log("[MobileSceneManager] Mobile scene initialized");
        }
        
        /// <summary>
        /// Setup button click listeners
        /// </summary>
        private void SetupButtonListeners()
        {
            if (menuButton != null)
            {
                menuButton.onClick.AddListener(ToggleMenu);
            }
            
            if (loadCSVButton != null)
            {
                loadCSVButton.onClick.AddListener(OnLoadCSVClicked);
            }
            
            if (settingsButton != null)
            {
                settingsButton.onClick.AddListener(OnSettingsClicked);
            }
        }
        
        /// <summary>
        /// Load the default CSV file
        /// </summary>
        private void LoadDefaultCSV()
        {
            // Try different paths for the CSV file
            string[] possiblePaths = new string[]
            {
                System.IO.Path.Combine(Application.persistentDataPath, defaultCSVFileName),
                System.IO.Path.Combine(Application.streamingAssetsPath, defaultCSVFileName),
                System.IO.Path.Combine(Application.dataPath, "Import", defaultCSVFileName),
                // For editor testing
                System.IO.Path.Combine(Application.dataPath, "..", "Import", defaultCSVFileName)
            };
            
            foreach (string path in possiblePaths)
            {
                if (System.IO.File.Exists(path))
                {
                    Debug.Log($"[MobileSceneManager] Found CSV at: {path}");
                    
                    // Auto-configure CSV columns for MusterPlan format
                    ConfigureCSVColumnsForMusterPlan();
                    
                    bool success = DataRepository.Instance.ImportCSV(path);
                    if (success)
                    {
                        Debug.Log($"[MobileSceneManager] CSV loaded successfully: {DataRepository.Instance.TotalSows} Sauen, {DataRepository.Instance.TotalVentils} Ventile");
                        ShowInfo($"CSV geladen: {DataRepository.Instance.TotalSows} Sauen");
                    }
                    else
                    {
                        Debug.LogError("[MobileSceneManager] Failed to load CSV");
                        ShowInfo("Fehler beim Laden der CSV");
                    }
                    return;
                }
            }
            
            Debug.LogWarning($"[MobileSceneManager] Default CSV not found: {defaultCSVFileName}");
            ShowInfo($"CSV nicht gefunden. Bitte {defaultCSVFileName} kopieren.");
        }
        
        /// <summary>
        /// Configure CSV columns for the MusterPlan.csv format
        /// </summary>
        private void ConfigureCSVColumnsForMusterPlan()
        {
            // Use the pre-configured MusterPlan config
            CSVConfigManager.Instance.LoadMusterPlanConfig();
            Debug.Log("[MobileSceneManager] CSV columns configured for MusterPlan format");
        }
        
        /// <summary>
        /// Toggle the menu panel
        /// </summary>
        public void ToggleMenu()
        {
            isMenuOpen = !isMenuOpen;
            
            if (menuPanel != null)
            {
                menuPanel.SetActive(isMenuOpen);
            }
            
            // Pause scanning when menu is open
            if (cameraController != null)
            {
                if (isMenuOpen)
                {
                    cameraController.StopScanning();
                }
                else
                {
                    cameraController.StartScanning();
                }
            }
        }
        
        /// <summary>
        /// Handle load CSV button click
        /// </summary>
        private void OnLoadCSVClicked()
        {
            Debug.Log("[MobileSceneManager] Load CSV clicked");
            
            // On Android, we could open a file picker here
            // For now, just reload the default CSV
            LoadDefaultCSV();
            
            ToggleMenu();
        }
        
        /// <summary>
        /// Handle settings button click
        /// </summary>
        private void OnSettingsClicked()
        {
            Debug.Log("[MobileSceneManager] Settings clicked");
            ShowInfo("Einstellungen: Demnächst verfügbar");
            ToggleMenu();
        }
        
        /// <summary>
        /// Called when a ventil QR code is detected
        /// </summary>
        private void OnVentilDetected(int ventilNumber)
        {
            Debug.Log($"[MobileSceneManager] Ventil detected: {ventilNumber}");
            
            // Get sow data for this ventil
            var sows = DataRepository.Instance.GetSowsByVentil(ventilNumber);
            ShowInfo($"Ventil {ventilNumber}: {sows.Count} Sauen");
        }
        
        /// <summary>
        /// Show info message
        /// </summary>
        public void ShowInfo(string message, float duration = 3f)
        {
            if (infoPanel != null && infoText != null)
            {
                infoText.text = message;
                infoPanel.SetActive(true);
                
                CancelInvoke(nameof(HideInfo));
                Invoke(nameof(HideInfo), duration);
            }
        }
        
        /// <summary>
        /// Hide info message
        /// </summary>
        private void HideInfo()
        {
            if (infoPanel != null)
            {
                infoPanel.SetActive(false);
            }
        }
        
        /// <summary>
        /// Get statistics about loaded data
        /// </summary>
        public string GetDataStatistics()
        {
            return $"Sauen: {DataRepository.Instance.TotalSows}\n" +
                   $"Ventile: {DataRepository.Instance.TotalVentils}\n" +
                   $"Letzter Import: {DataRepository.Instance.LastImportDate:dd.MM.yyyy HH:mm}";
        }
    }
}
