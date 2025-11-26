using UnityEngine;
using UnityEngine.UI;
using TMPro;
using D8PlanerXR.Core;

namespace D8PlanerXR.UI
{
    /// <summary>
    /// UI-Controller für den Moduswechsel zwischen VR und Handy
    /// Zeigt den aktuellen Modus an und erlaubt Wechsel
    /// </summary>
    public class ModeSelectionUI : MonoBehaviour
    {
        [Header("UI-Referenzen")]
        [SerializeField] private GameObject modeSelectionPanel;
        [SerializeField] private TextMeshProUGUI currentModeText;
        [SerializeField] private TextMeshProUGUI modeInfoText;
        [SerializeField] private Button vrModeButton;
        [SerializeField] private Button mobileModeButton;
        [SerializeField] private Button autoModeButton;
        [SerializeField] private Button closePanelButton;
        
        [Header("Einstellungen")]
        [SerializeField] private bool showOnStart = false;
        [SerializeField] private KeyCode toggleKey = KeyCode.M; // 'M' für Modus
        
        [Header("Texte")]
        [SerializeField] private string vrModeLabel = "🕶️ VR-Modus";
        [SerializeField] private string mobileModeLabel = "📱 Handy-Modus";
        [SerializeField] private string autoModeLabel = "🔄 Automatisch";
        
        private DeviceModeManager modeManager;

        private void Start()
        {
            // Mode Manager finden
            modeManager = DeviceModeManager.Instance;
            
            if (modeManager == null)
            {
                Debug.LogError("[ModeSelectionUI] DeviceModeManager nicht gefunden!");
                enabled = false;
                return;
            }

            // Event-Listener registrieren
            modeManager.OnModeChanged += OnModeChanged;

            // Button-Listener
            if (vrModeButton != null)
                vrModeButton.onClick.AddListener(() => SwitchToMode(DeviceModeManager.DeviceMode.VRMode));
            
            if (mobileModeButton != null)
                mobileModeButton.onClick.AddListener(() => SwitchToMode(DeviceModeManager.DeviceMode.MobileMode));
            
            if (autoModeButton != null)
                autoModeButton.onClick.AddListener(() => SwitchToMode(DeviceModeManager.DeviceMode.Auto));
            
            if (closePanelButton != null)
                closePanelButton.onClick.AddListener(ClosePanel);

            // Panel initial verstecken
            if (modeSelectionPanel != null)
            {
                modeSelectionPanel.SetActive(showOnStart);
            }

            // Initiale Anzeige aktualisieren
            UpdateDisplay();
        }

        private void OnDestroy()
        {
            if (modeManager != null)
            {
                modeManager.OnModeChanged -= OnModeChanged;
            }
        }

        private void Update()
        {
            // Toggle-Taste prüfen
            if (Input.GetKeyDown(toggleKey))
            {
                TogglePanel();
            }
        }

        /// <summary>
        /// Wechselt den Modus
        /// </summary>
        private void SwitchToMode(DeviceModeManager.DeviceMode mode)
        {
            if (modeManager != null)
            {
                modeManager.SwitchMode(mode);
                
                // Optional: Panel schließen nach Wechsel
                // ClosePanel();
            }
        }

        /// <summary>
        /// Callback wenn der Modus gewechselt wurde
        /// </summary>
        private void OnModeChanged(DeviceModeManager.DeviceMode newMode)
        {
            UpdateDisplay();
            
            // Feedback an User
            Debug.Log($"[ModeSelectionUI] Modus gewechselt zu: {newMode}");
            
            // Optional: Toast/Notification anzeigen
            ShowModeChangeNotification(newMode);
        }

        /// <summary>
        /// Aktualisiert die UI-Anzeige
        /// </summary>
        private void UpdateDisplay()
        {
            if (modeManager == null) return;

            // Aktuellen Modus anzeigen
            if (currentModeText != null)
            {
                string modeIcon = modeManager.IsVRMode ? "🕶️" : "📱";
                string modeName = modeManager.IsVRMode ? "VR-Modus" : "Handy-Modus";
                currentModeText.text = $"{modeIcon} {modeName}";
            }

            // Modus-Informationen anzeigen
            if (modeInfoText != null)
            {
                modeInfoText.text = modeManager.GetModeInfo();
            }

            // Buttons aktualisieren (aktiven Modus hervorheben)
            UpdateButtonStates();
        }

        /// <summary>
        /// Aktualisiert die Button-Zustände (visuelles Feedback)
        /// </summary>
        private void UpdateButtonStates()
        {
            if (modeManager == null) return;

            var currentMode = modeManager.CurrentMode;

            // VR Button
            if (vrModeButton != null)
            {
                var colors = vrModeButton.colors;
                colors.normalColor = currentMode == DeviceModeManager.DeviceMode.VRMode 
                    ? new Color(0.3f, 0.8f, 0.3f) // Grün wenn aktiv
                    : Color.white;
                vrModeButton.colors = colors;
            }

            // Mobile Button
            if (mobileModeButton != null)
            {
                var colors = mobileModeButton.colors;
                colors.normalColor = currentMode == DeviceModeManager.DeviceMode.MobileMode 
                    ? new Color(0.3f, 0.8f, 0.3f) // Grün wenn aktiv
                    : Color.white;
                mobileModeButton.colors = colors;
            }
        }

        /// <summary>
        /// Zeigt eine Benachrichtigung über den Moduswechsel
        /// </summary>
        private void ShowModeChangeNotification(DeviceModeManager.DeviceMode newMode)
        {
            // Hier könnte ein Toast/Popup/Notification angezeigt werden
            string message = newMode == DeviceModeManager.DeviceMode.VRMode
                ? "VR-Modus aktiviert - Volle AR-Features verfügbar"
                : "Handy-Modus aktiviert - Touch-Steuerung aktiv";
            
            Debug.Log($"[ModeSelectionUI] {message}");
        }

        /// <summary>
        /// Öffnet/Schließt das Panel
        /// </summary>
        public void TogglePanel()
        {
            if (modeSelectionPanel != null)
            {
                bool newState = !modeSelectionPanel.activeSelf;
                modeSelectionPanel.SetActive(newState);
                
                if (newState)
                {
                    UpdateDisplay();
                }
            }
        }

        /// <summary>
        /// Öffnet das Panel
        /// </summary>
        public void OpenPanel()
        {
            if (modeSelectionPanel != null)
            {
                modeSelectionPanel.SetActive(true);
                UpdateDisplay();
            }
        }

        /// <summary>
        /// Schließt das Panel
        /// </summary>
        public void ClosePanel()
        {
            if (modeSelectionPanel != null)
            {
                modeSelectionPanel.SetActive(false);
            }
        }

        /// <summary>
        /// Zeigt Info über Touch-Steuerung im Handy-Modus
        /// </summary>
        public void ShowMobileHelpText()
        {
            string helpText = @"
═══════════════════════════════════════
  📱 HANDY-MODUS BEDIENUNG
═══════════════════════════════════════

📸 QR-Code Scannen:
   - Kamera auf QR-Code richten
   - Overlay erscheint automatisch
   - Nur ein QR-Code gleichzeitig

👆 Touch-Steuerung:
   - Tippen: Sau-Details anzeigen
   - Wischen: Zwischen Sauen wechseln
   - Zwei-Finger-Zoom: Overlay vergrößern

⚙️ Einstellungen:
   - Menü-Button (oben rechts)
   - CSV importieren
   - Modus wechseln

💡 Tipps:
   - Gute Beleuchtung wichtig
   - QR-Code frontal scannen
   - Abstand ca. 20-50cm

═══════════════════════════════════════
            ";
            
            Debug.Log(helpText);
        }

#if UNITY_EDITOR
        [ContextMenu("Panel öffnen")]
        private void EditorOpenPanel()
        {
            OpenPanel();
        }

        [ContextMenu("Panel schließen")]
        private void EditorClosePanel()
        {
            ClosePanel();
        }

        [ContextMenu("Mobile Hilfe anzeigen")]
        private void EditorShowMobileHelp()
        {
            ShowMobileHelpText();
        }
#endif
    }
}
