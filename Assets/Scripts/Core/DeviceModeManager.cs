using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System;

namespace D8PlanerXR.Core
{
    /// <summary>
    /// Verwaltet den Gerätemodus (VR-Brille vs. Handy-Modus)
    /// Erkennt automatisch die verfügbare Hardware und passt die AR-Funktionalität an
    /// </summary>
    public class DeviceModeManager : MonoBehaviour
    {
        /// <summary>
        /// Verfügbare Geräte-Modi
        /// </summary>
        public enum DeviceMode
        {
            /// <summary>Automatische Erkennung basierend auf Hardware</summary>
            Auto,
            /// <summary>VR-Modus für Viture Neckband Pro + Luma Ultra</summary>
            VRMode,
            /// <summary>Handy-Modus für normale Smartphones</summary>
            MobileMode
        }

        [Header("Modus-Einstellungen")]
        [SerializeField] private DeviceMode deviceMode = DeviceMode.Auto;
        [SerializeField] private bool allowManualModeSwitch = true;
        
        [Header("VR-Modus Features")]
        [SerializeField] private bool enableSpatialAnchoring = true;
        [SerializeField] private bool enableVirtualDeckzentrum = true;
        [SerializeField] private bool enableHeadTracking = true;
        
        [Header("Handy-Modus Features")]
        [SerializeField] private bool enableTouchControls = true;
        [SerializeField] private bool simplifiedOverlays = true;
        [SerializeField] private float mobileUpdateInterval = 0.5f; // Längeres Intervall für bessere Performance
        
        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = true;

        // Singleton
        private static DeviceModeManager instance;
        public static DeviceModeManager Instance => instance;

        // Aktueller aktiver Modus
        private DeviceMode currentActiveMode;
        
        // Events
        public event Action<DeviceMode> OnModeChanged;

        // Eigenschaften
        public DeviceMode CurrentMode => currentActiveMode;
        public bool IsVRMode => currentActiveMode == DeviceMode.VRMode;
        public bool IsMobileMode => currentActiveMode == DeviceMode.MobileMode;
        public bool IsSpatialAnchoringEnabled => IsVRMode && enableSpatialAnchoring;
        public bool IsVirtualDeckzentrumEnabled => IsVRMode && enableVirtualDeckzentrum;
        public bool IsTouchControlEnabled => IsMobileMode && enableTouchControls;
        public float UpdateInterval => IsMobileMode ? mobileUpdateInterval : 0.2f;

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

            // Modus initialisieren
            InitializeDeviceMode();
        }

        private void Start()
        {
            ApplyModeSettings();
        }

        /// <summary>
        /// Initialisiert den Gerätemodus
        /// </summary>
        private void InitializeDeviceMode()
        {
            if (deviceMode == DeviceMode.Auto)
            {
                // Automatische Erkennung
                currentActiveMode = DetectDeviceMode();
            }
            else
            {
                // Manuell gesetzter Modus
                currentActiveMode = deviceMode;
            }

            if (showDebugInfo)
            {
                Debug.Log($"[DeviceModeManager] Modus initialisiert: {currentActiveMode}");
                Debug.Log($"[DeviceModeManager] Plattform: {Application.platform}");
                Debug.Log($"[DeviceModeManager] Gerät: {SystemInfo.deviceModel}");
            }
        }

        /// <summary>
        /// Erkennt automatisch den Gerätemodus basierend auf Hardware
        /// </summary>
        private DeviceMode DetectDeviceMode()
        {
            // Prüfe auf Viture Hardware oder VR-spezifische Merkmale
            if (IsVitureHardwareDetected())
            {
                if (showDebugInfo)
                    Debug.Log("[DeviceModeManager] Viture Hardware erkannt → VR-Modus");
                return DeviceMode.VRMode;
            }

            // Prüfe auf VR-Headset (allgemein)
            if (IsVRHeadsetConnected())
            {
                if (showDebugInfo)
                    Debug.Log("[DeviceModeManager] VR-Headset erkannt → VR-Modus");
                return DeviceMode.VRMode;
            }

            // Standard: Handy-Modus
            if (showDebugInfo)
                Debug.Log("[DeviceModeManager] Kein VR-Gerät erkannt → Handy-Modus");
            return DeviceMode.MobileMode;
        }

        /// <summary>
        /// Prüft ob Viture Hardware angeschlossen ist
        /// </summary>
        private bool IsVitureHardwareDetected()
        {
            // Prüfe Gerätemodell
            string deviceModel = SystemInfo.deviceModel.ToLower();
            if (deviceModel.Contains("viture") || deviceModel.Contains("neckband"))
            {
                return true;
            }

            // Prüfe Hersteller
            string deviceName = SystemInfo.deviceName.ToLower();
            if (deviceName.Contains("viture"))
            {
                return true;
            }

            // Weitere Viture-spezifische Checks könnten hier hinzugefügt werden
            // z.B. USB-Geräte, spezielle Sensoren, etc.

            return false;
        }

        /// <summary>
        /// Prüft ob ein VR-Headset verbunden ist (allgemein)
        /// </summary>
        private bool IsVRHeadsetConnected()
        {
            // XR-System prüfen
#if UNITY_2020_1_OR_NEWER
            var xrDisplaySubsystems = new System.Collections.Generic.List<UnityEngine.XR.XRDisplaySubsystem>();
            UnityEngine.SubsystemManager.GetInstances(xrDisplaySubsystems);
            
            foreach (var subsystem in xrDisplaySubsystems)
            {
                if (subsystem.running)
                {
                    return true;
                }
            }
#endif

            // Legacy VR Check
#pragma warning disable 618
            if (UnityEngine.XR.XRDevice.isPresent)
            {
                return true;
            }
#pragma warning restore 618

            return false;
        }

        /// <summary>
        /// Wendet die Einstellungen für den aktuellen Modus an
        /// </summary>
        private void ApplyModeSettings()
        {
            if (showDebugInfo)
            {
                Debug.Log($"[DeviceModeManager] Wende Einstellungen für {currentActiveMode} an...");
            }

            // AR Foundation Komponenten konfigurieren
            ConfigureARFoundation();

            // QR-Code Tracker konfigurieren
            ConfigureQRTracker();

            // Virtuelles Deckzentrum konfigurieren
            ConfigureVirtualDeckzentrum();

            // UI konfigurieren
            ConfigureUI();

            if (showDebugInfo)
            {
                Debug.Log($"[DeviceModeManager] ✓ Modus-Einstellungen angewendet");
                LogCurrentConfiguration();
            }
        }

        /// <summary>
        /// Konfiguriert AR Foundation für den aktuellen Modus
        /// </summary>
        private void ConfigureARFoundation()
        {
            var arSession = FindObjectOfType<ARSession>();
            if (arSession != null)
            {
                // AR Session ist in beiden Modi aktiv
                arSession.enabled = true;
            }

            // AR Plane Manager (nur für erweiterte Features)
            var planeManager = FindObjectOfType<ARPlaneManager>();
            if (planeManager != null)
            {
                // Im Handy-Modus: Plane Detection optional (spart Ressourcen)
                planeManager.enabled = IsVRMode || enableSpatialAnchoring;
            }

            // AR Anchor Manager
            var anchorManager = FindObjectOfType<ARAnchorManager>();
            if (anchorManager != null)
            {
                anchorManager.enabled = IsSpatialAnchoringEnabled;
            }
        }

        /// <summary>
        /// Konfiguriert den QR-Code Tracker für den aktuellen Modus
        /// </summary>
        private void ConfigureQRTracker()
        {
            var qrTracker = FindObjectOfType<AR.QRCodeTracker>();
            if (qrTracker != null)
            {
                // QR-Tracker ist in beiden Modi aktiv, aber mit unterschiedlichen Einstellungen
                // Diese werden direkt im QRCodeTracker über DeviceModeManager abgefragt
            }
        }

        /// <summary>
        /// Konfiguriert das virtuelle Deckzentrum für den aktuellen Modus
        /// </summary>
        private void ConfigureVirtualDeckzentrum()
        {
            var virtualDeckzentrum = FindObjectOfType<AR.VirtualDeckzentrum>();
            if (virtualDeckzentrum != null)
            {
                // Virtuelles Deckzentrum nur im VR-Modus
                if (!IsVirtualDeckzentrumEnabled)
                {
                    virtualDeckzentrum.SetActive(false);
                    if (showDebugInfo)
                        Debug.Log("[DeviceModeManager] Virtuelles Deckzentrum deaktiviert (Handy-Modus)");
                }
            }
        }

        /// <summary>
        /// Konfiguriert die UI für den aktuellen Modus
        /// </summary>
        private void ConfigureUI()
        {
            // Touch-Steuerung aktivieren/deaktivieren
            if (IsMobileMode)
            {
                // Mobile-spezifische UI aktivieren
                EnableMobileUI();
            }
            else
            {
                // VR-spezifische UI aktivieren
                EnableVRUI();
            }
        }

        /// <summary>
        /// Aktiviert Mobile-spezifische UI-Elemente
        /// </summary>
        private void EnableMobileUI()
        {
            // Hier könnten mobile-spezifische UI-Elemente aktiviert werden
            // z.B. Touch-Buttons, Swipe-Gesten, etc.
            if (showDebugInfo)
                Debug.Log("[DeviceModeManager] Mobile UI aktiviert");
        }

        /// <summary>
        /// Aktiviert VR-spezifische UI-Elemente
        /// </summary>
        private void EnableVRUI()
        {
            // Hier könnten VR-spezifische UI-Elemente aktiviert werden
            // z.B. Head-Gaze Cursor, 3D-Menüs, etc.
            if (showDebugInfo)
                Debug.Log("[DeviceModeManager] VR UI aktiviert");
        }

        /// <summary>
        /// Wechselt manuell den Gerätemodus (falls erlaubt)
        /// </summary>
        public void SwitchMode(DeviceMode newMode)
        {
            if (!allowManualModeSwitch && newMode != DeviceMode.Auto)
            {
                Debug.LogWarning("[DeviceModeManager] Manueller Moduswechsel ist deaktiviert!");
                return;
            }

            if (newMode == DeviceMode.Auto)
            {
                // Automatische Erkennung erneut durchführen
                newMode = DetectDeviceMode();
            }

            if (newMode == currentActiveMode)
            {
                Debug.Log($"[DeviceModeManager] Modus ist bereits {newMode}");
                return;
            }

            DeviceMode oldMode = currentActiveMode;
            currentActiveMode = newMode;
            deviceMode = newMode;

            if (showDebugInfo)
            {
                Debug.Log($"[DeviceModeManager] Modus gewechselt: {oldMode} → {newMode}");
            }

            // Einstellungen neu anwenden
            ApplyModeSettings();

            // Event auslösen
            OnModeChanged?.Invoke(newMode);
        }

        /// <summary>
        /// Gibt die aktuelle Konfiguration im Debug-Log aus
        /// </summary>
        private void LogCurrentConfiguration()
        {
            Debug.Log("═══════════════════════════════════════");
            Debug.Log($"  D8-Planer XR - Gerätekonfiguration");
            Debug.Log("═══════════════════════════════════════");
            Debug.Log($"  Modus: {currentActiveMode}");
            Debug.Log($"  Plattform: {Application.platform}");
            Debug.Log($"  Gerät: {SystemInfo.deviceModel}");
            Debug.Log("───────────────────────────────────────");
            Debug.Log($"  Spatial Anchoring: {(IsSpatialAnchoringEnabled ? "✓" : "✗")}");
            Debug.Log($"  Virtuelles Deckzentrum: {(IsVirtualDeckzentrumEnabled ? "✓" : "✗")}");
            Debug.Log($"  Touch Controls: {(IsTouchControlEnabled ? "✓" : "✗")}");
            Debug.Log($"  Update-Intervall: {UpdateInterval}s");
            Debug.Log("═══════════════════════════════════════");
        }

        /// <summary>
        /// Gibt Modus-Informationen als String zurück
        /// </summary>
        public string GetModeInfo()
        {
            return $"Modus: {currentActiveMode}\n" +
                   $"VR-Features: {(IsVRMode ? "Aktiv" : "Inaktiv")}\n" +
                   $"Touch-Steuerung: {(IsTouchControlEnabled ? "Aktiv" : "Inaktiv")}\n" +
                   $"Spatial Anchoring: {(IsSpatialAnchoringEnabled ? "Aktiv" : "Inaktiv")}";
        }

#if UNITY_EDITOR
        [ContextMenu("Wechsel zu VR-Modus")]
        private void EditorSwitchToVR()
        {
            SwitchMode(DeviceMode.VRMode);
        }

        [ContextMenu("Wechsel zu Handy-Modus")]
        private void EditorSwitchToMobile()
        {
            SwitchMode(DeviceMode.MobileMode);
        }

        [ContextMenu("Automatische Erkennung")]
        private void EditorSwitchToAuto()
        {
            SwitchMode(DeviceMode.Auto);
        }

        [ContextMenu("Zeige Konfiguration")]
        private void EditorShowConfig()
        {
            LogCurrentConfiguration();
        }
#endif
    }
}
