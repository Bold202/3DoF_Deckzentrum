using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using D8PlanerXR.Mobile;

namespace D8PlanerXR.Core
{
    /// <summary>
    /// Runtime Mode Controller - Handles switching between Mobile and VR modes at runtime
    /// Provides a simple interface for mode switching with proper cleanup and initialization
    /// </summary>
    public class RuntimeModeController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button modeToggleButton;
        [SerializeField] private TextMeshProUGUI modeStatusText;
        [SerializeField] private GameObject mobileIndicator;
        [SerializeField] private GameObject vrIndicator;
        
        [Header("Mode Containers")]
        [SerializeField] private GameObject mobileUIContainer;
        [SerializeField] private GameObject vrUIContainer;
        
        [Header("Settings")]
        [SerializeField] private float transitionDelay = 0.5f;
        [SerializeField] private bool showModeNotification = true;
        
        private static RuntimeModeController instance;
        public static RuntimeModeController Instance => instance;
        
        private bool isTransitioning = false;
        
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
            // Setup button listener
            if (modeToggleButton != null)
            {
                modeToggleButton.onClick.AddListener(ToggleMode);
            }
            
            // Subscribe to mode changes
            if (DeviceModeManager.Instance != null)
            {
                DeviceModeManager.Instance.OnModeChanged += OnModeChanged;
            }
            
            // Update UI to reflect current mode
            UpdateModeUI();
        }
        
        private void OnDestroy()
        {
            if (DeviceModeManager.Instance != null)
            {
                DeviceModeManager.Instance.OnModeChanged -= OnModeChanged;
            }
        }
        
        /// <summary>
        /// Toggle between Mobile and VR modes
        /// </summary>
        public void ToggleMode()
        {
            if (isTransitioning) return;
            
            if (DeviceModeManager.Instance == null)
            {
                Debug.LogError("[RuntimeModeController] DeviceModeManager not found!");
                return;
            }
            
            StartCoroutine(PerformModeTransition());
        }
        
        /// <summary>
        /// Switch to a specific mode
        /// </summary>
        public void SwitchToMode(DeviceModeManager.DeviceMode targetMode)
        {
            if (isTransitioning) return;
            
            if (DeviceModeManager.Instance == null)
            {
                Debug.LogError("[RuntimeModeController] DeviceModeManager not found!");
                return;
            }
            
            if (DeviceModeManager.Instance.CurrentMode == targetMode)
            {
                Debug.Log($"[RuntimeModeController] Already in {targetMode}");
                return;
            }
            
            StartCoroutine(PerformModeTransition(targetMode));
        }
        
        /// <summary>
        /// Switch to Mobile mode
        /// </summary>
        public void SwitchToMobileMode()
        {
            SwitchToMode(DeviceModeManager.DeviceMode.MobileMode);
        }
        
        /// <summary>
        /// Switch to VR mode
        /// </summary>
        public void SwitchToVRMode()
        {
            SwitchToMode(DeviceModeManager.DeviceMode.VRMode);
        }
        
        /// <summary>
        /// Perform the mode transition with proper cleanup
        /// </summary>
        private IEnumerator PerformModeTransition(DeviceModeManager.DeviceMode? targetMode = null)
        {
            isTransitioning = true;
            
            // Determine target mode
            DeviceModeManager.DeviceMode newMode = targetMode ?? 
                (DeviceModeManager.Instance.IsMobileMode 
                    ? DeviceModeManager.DeviceMode.VRMode 
                    : DeviceModeManager.DeviceMode.MobileMode);
            
            Debug.Log($"[RuntimeModeController] Transitioning to {newMode}...");
            
            // Step 1: Cleanup current mode
            if (DeviceModeManager.Instance.IsMobileMode)
            {
                // Stop mobile camera
                var cameraController = FindObjectOfType<MobileCameraController>();
                if (cameraController != null)
                {
                    cameraController.StopCamera();
                }
            }
            
            // Fade out current UI (optional visual transition)
            yield return new WaitForSeconds(transitionDelay);
            
            // Step 2: Switch mode in DeviceModeManager
            DeviceModeManager.Instance.SwitchMode(newMode);
            
            // Step 3: Initialize new mode
            if (newMode == DeviceModeManager.DeviceMode.MobileMode)
            {
                // Start mobile camera
                var cameraController = FindObjectOfType<MobileCameraController>();
                if (cameraController != null)
                {
                    yield return new WaitForSeconds(0.1f);
                    cameraController.StartCamera();
                }
            }
            
            // Fade in new UI
            yield return new WaitForSeconds(transitionDelay);
            
            isTransitioning = false;
            
            // Show notification
            if (showModeNotification)
            {
                ShowModeNotification(newMode);
            }
            
            Debug.Log($"[RuntimeModeController] Transition to {newMode} complete");
        }
        
        /// <summary>
        /// Handle mode change event
        /// </summary>
        private void OnModeChanged(DeviceModeManager.DeviceMode newMode)
        {
            UpdateModeUI();
        }
        
        /// <summary>
        /// Update UI to reflect current mode
        /// </summary>
        private void UpdateModeUI()
        {
            if (DeviceModeManager.Instance == null) return;
            
            bool isMobile = DeviceModeManager.Instance.IsMobileMode;
            
            // Update status text
            if (modeStatusText != null)
            {
                modeStatusText.text = isMobile ? "📱 Handy-Modus" : "🕶️ VR-Modus";
            }
            
            // Update indicators
            if (mobileIndicator != null)
            {
                mobileIndicator.SetActive(isMobile);
            }
            
            if (vrIndicator != null)
            {
                vrIndicator.SetActive(!isMobile);
            }
            
            // Update containers
            if (mobileUIContainer != null)
            {
                mobileUIContainer.SetActive(isMobile);
            }
            
            if (vrUIContainer != null)
            {
                vrUIContainer.SetActive(!isMobile);
            }
            
            // Update button text
            if (modeToggleButton != null)
            {
                var buttonText = modeToggleButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = isMobile ? "Zu VR wechseln" : "Zu Handy wechseln";
                }
            }
        }
        
        /// <summary>
        /// Show a notification about the mode change
        /// </summary>
        private void ShowModeNotification(DeviceModeManager.DeviceMode mode)
        {
            string message = mode == DeviceModeManager.DeviceMode.MobileMode
                ? "📱 Handy-Modus aktiviert\nKamera-basiertes QR-Scanning"
                : "🕶️ VR-Modus aktiviert\nSpatial Anchoring & Multi-QR-Erkennung";
            
            Debug.Log($"[RuntimeModeController] {message}");
            
            // Try to show via MobileSceneManager if available
            var mobileScene = FindObjectOfType<MobileSceneManager>();
            if (mobileScene != null)
            {
                mobileScene.ShowInfo(message, 3f);
            }
        }
        
        /// <summary>
        /// Get current mode info as string
        /// </summary>
        public string GetModeInfo()
        {
            if (DeviceModeManager.Instance == null)
                return "Modus: Unbekannt";
                
            return DeviceModeManager.Instance.GetModeInfo();
        }
        
        /// <summary>
        /// Check if VR hardware is available
        /// </summary>
        public bool IsVRHardwareAvailable()
        {
            // Check for Viture or other VR hardware
            string deviceModel = SystemInfo.deviceModel.ToLower();
            string deviceName = SystemInfo.deviceName.ToLower();
            
            if (deviceModel.Contains("viture") || deviceName.Contains("viture"))
                return true;
            
            // Check XR subsystems
#if UNITY_2020_1_OR_NEWER
            var xrDisplaySubsystems = new System.Collections.Generic.List<UnityEngine.XR.XRDisplaySubsystem>();
            UnityEngine.SubsystemManager.GetInstances(xrDisplaySubsystems);
            
            foreach (var subsystem in xrDisplaySubsystems)
            {
                if (subsystem.running)
                    return true;
            }
#endif

#pragma warning disable 618
            if (UnityEngine.XR.XRDevice.isPresent)
                return true;
#pragma warning restore 618
            
            return false;
        }
        
#if UNITY_EDITOR
        [ContextMenu("Toggle Mode")]
        private void EditorToggleMode()
        {
            ToggleMode();
        }
        
        [ContextMenu("Switch to Mobile")]
        private void EditorSwitchToMobile()
        {
            SwitchToMobileMode();
        }
        
        [ContextMenu("Switch to VR")]
        private void EditorSwitchToVR()
        {
            SwitchToVRMode();
        }
#endif
    }
}
