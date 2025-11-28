using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZXing;
using D8PlanerXR.Core;
using D8PlanerXR.Data;

namespace D8PlanerXR.Mobile
{
    /// <summary>
    /// Mobile Camera Controller - Manages camera for QR code scanning on smartphones
    /// This is a simpler implementation that works without full AR Foundation setup
    /// Designed for reliable phone-based QR code detection
    /// </summary>
    public class MobileCameraController : MonoBehaviour
    {
        [Header("Camera Settings")]
        [SerializeField] private RawImage cameraDisplay;
        [SerializeField] private AspectRatioFitter aspectRatioFitter;
        [SerializeField] private int requestedWidth = 1280;
        [SerializeField] private int requestedHeight = 720;
        [SerializeField] private int requestedFPS = 30;
        
        [Header("QR Scanner Settings")]
        [SerializeField] private float scanInterval = 0.3f;
        [SerializeField] private bool showScanArea = true;
        [SerializeField] private RectTransform scanAreaIndicator;
        
        [Header("UI References")]
        [SerializeField] private GameObject overlayContainer;
        [SerializeField] private GameObject ventilInfoPanel;
        [SerializeField] private TMPro.TextMeshProUGUI ventilNumberText;
        [SerializeField] private TMPro.TextMeshProUGUI sowInfoText;
        [SerializeField] private Image trafficLightImage;
        [SerializeField] private TMPro.TextMeshProUGUI statusText;
        [SerializeField] private TMPro.TextMeshProUGUI debugText;
        
        [Header("Colors")]
        [SerializeField] private Color greenColor = new Color(0.2f, 0.8f, 0.2f);
        [SerializeField] private Color yellowColor = new Color(0.9f, 0.9f, 0.2f);
        [SerializeField] private Color redColor = new Color(0.9f, 0.2f, 0.2f);
        [SerializeField] private Color purpleColor = new Color(0.7f, 0.2f, 0.9f);
        [SerializeField] private Color unknownColor = Color.gray;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = true;
        
        // Camera components
        private WebCamTexture webCamTexture;
        private WebCamDevice[] devices;
        private int currentCameraIndex = 0;
        
        // QR Code Reader
        private IBarcodeReader barcodeReader;
        private Coroutine scanCoroutine;
        
        // State
        private bool isScanning = false;
        private bool isCameraActive = false;
        private int lastDetectedVentil = -1;
        private float lastDetectionTime = 0f;
        private const float DETECTION_DISPLAY_DURATION = 5f;
        
        // Singleton
        private static MobileCameraController instance;
        public static MobileCameraController Instance => instance;
        
        // Events
        public event Action<int> OnVentilDetected;
        public event Action OnCameraStarted;
        public event Action OnCameraStopped;
        
        // Properties
        public bool IsCameraActive => isCameraActive;
        public bool IsScanning => isScanning;
        public int LastDetectedVentil => lastDetectedVentil;
        
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
            InitializeBarcodeReader();
            
            // Auto-start camera if in mobile mode
            if (DeviceModeManager.Instance != null && DeviceModeManager.Instance.IsMobileMode)
            {
                StartCoroutine(AutoStartCamera());
            }
            
            // Hide info panel initially
            if (ventilInfoPanel != null)
            {
                ventilInfoPanel.SetActive(false);
            }
            
            UpdateStatusText("Bereit zum Scannen...");
        }
        
        private IEnumerator AutoStartCamera()
        {
            // Wait a frame for everything to initialize
            yield return null;
            StartCamera();
        }
        
        private void OnDestroy()
        {
            StopCamera();
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                StopCamera();
            }
            else if (DeviceModeManager.Instance != null && DeviceModeManager.Instance.IsMobileMode)
            {
                StartCamera();
            }
        }
        
        private void Update()
        {
            // Auto-hide detection panel after timeout
            if (lastDetectedVentil >= 0 && Time.time - lastDetectionTime > DETECTION_DISPLAY_DURATION)
            {
                HideVentilInfo();
            }
            
            // Update debug info
            if (showDebugInfo && debugText != null)
            {
                UpdateDebugInfo();
            }
        }
        
        /// <summary>
        /// Initialize the ZXing barcode reader
        /// </summary>
        private void InitializeBarcodeReader()
        {
            barcodeReader = new BarcodeReader
            {
                AutoRotate = true,
                TryInverted = true,
                Options = new ZXing.Common.DecodingOptions
                {
                    PossibleFormats = new List<BarcodeFormat> { BarcodeFormat.QR_CODE },
                    TryHarder = true
                }
            };
            
            Debug.Log("[MobileCameraController] Barcode reader initialized");
        }
        
        /// <summary>
        /// Start the camera
        /// </summary>
        public void StartCamera()
        {
            if (isCameraActive)
            {
                Debug.LogWarning("[MobileCameraController] Camera is already active");
                return;
            }
            
            StartCoroutine(InitializeCamera());
        }
        
        /// <summary>
        /// Initialize and start the camera
        /// </summary>
        private IEnumerator InitializeCamera()
        {
            UpdateStatusText("Kamera wird initialisiert...");
            
            // Request camera permission
            if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
            {
                yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
            }
            
            if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
            {
                Debug.LogError("[MobileCameraController] Camera permission denied!");
                UpdateStatusText("Kamera-Berechtigung verweigert!");
                yield break;
            }
            
            // Get available cameras
            devices = WebCamTexture.devices;
            
            if (devices.Length == 0)
            {
                Debug.LogError("[MobileCameraController] No cameras found!");
                UpdateStatusText("Keine Kamera gefunden!");
                yield break;
            }
            
            // Find back camera (preferred for QR scanning)
            int backCameraIndex = -1;
            for (int i = 0; i < devices.Length; i++)
            {
                if (!devices[i].isFrontFacing)
                {
                    backCameraIndex = i;
                    break;
                }
            }
            
            // Use back camera if found, otherwise use first available
            currentCameraIndex = backCameraIndex >= 0 ? backCameraIndex : 0;
            
            Debug.Log($"[MobileCameraController] Using camera: {devices[currentCameraIndex].name}");
            
            // Create WebCamTexture
            webCamTexture = new WebCamTexture(
                devices[currentCameraIndex].name,
                requestedWidth,
                requestedHeight,
                requestedFPS
            );
            
            // Assign to RawImage for display
            if (cameraDisplay != null)
            {
                cameraDisplay.texture = webCamTexture;
                cameraDisplay.gameObject.SetActive(true);
            }
            
            // Start the camera
            webCamTexture.Play();
            
            // Wait for camera to start
            int timeout = 100; // 10 seconds timeout
            while (!webCamTexture.didUpdateThisFrame && timeout > 0)
            {
                yield return new WaitForSeconds(0.1f);
                timeout--;
            }
            
            if (timeout <= 0)
            {
                Debug.LogError("[MobileCameraController] Camera start timeout!");
                UpdateStatusText("Kamera-Start fehlgeschlagen!");
                yield break;
            }
            
            // Adjust aspect ratio
            if (aspectRatioFitter != null)
            {
                aspectRatioFitter.aspectRatio = (float)webCamTexture.width / webCamTexture.height;
            }
            
            // Handle camera rotation on mobile devices
            AdjustCameraRotation();
            
            isCameraActive = true;
            
            Debug.Log($"[MobileCameraController] Camera started: {webCamTexture.width}x{webCamTexture.height}");
            UpdateStatusText("Kamera aktiv - QR-Code scannen");
            
            // Start scanning
            StartScanning();
            
            OnCameraStarted?.Invoke();
        }
        
        /// <summary>
        /// Adjust camera display rotation for mobile devices
        /// </summary>
        private void AdjustCameraRotation()
        {
            if (cameraDisplay == null || webCamTexture == null) return;
            
            // Get video rotation angle
            int rotationAngle = webCamTexture.videoRotationAngle;
            
            // Apply rotation to the RawImage
            cameraDisplay.rectTransform.localEulerAngles = new Vector3(0, 0, -rotationAngle);
            
            // Handle mirroring for front camera
            if (devices[currentCameraIndex].isFrontFacing)
            {
                cameraDisplay.rectTransform.localScale = new Vector3(-1, 1, 1);
            }
            else
            {
                cameraDisplay.rectTransform.localScale = Vector3.one;
            }
            
            Debug.Log($"[MobileCameraController] Camera rotation adjusted: {rotationAngle}°");
        }
        
        /// <summary>
        /// Stop the camera
        /// </summary>
        public void StopCamera()
        {
            StopScanning();
            
            if (webCamTexture != null && webCamTexture.isPlaying)
            {
                webCamTexture.Stop();
            }
            
            if (cameraDisplay != null)
            {
                cameraDisplay.texture = null;
            }
            
            isCameraActive = false;
            
            Debug.Log("[MobileCameraController] Camera stopped");
            UpdateStatusText("Kamera gestoppt");
            
            OnCameraStopped?.Invoke();
        }
        
        /// <summary>
        /// Switch between front and back camera
        /// </summary>
        public void SwitchCamera()
        {
            if (devices == null || devices.Length <= 1)
            {
                Debug.LogWarning("[MobileCameraController] Cannot switch camera - only one camera available");
                return;
            }
            
            StopCamera();
            currentCameraIndex = (currentCameraIndex + 1) % devices.Length;
            StartCamera();
        }
        
        /// <summary>
        /// Start QR code scanning
        /// </summary>
        public void StartScanning()
        {
            if (isScanning) return;
            
            if (!isCameraActive)
            {
                Debug.LogWarning("[MobileCameraController] Cannot start scanning - camera not active");
                return;
            }
            
            scanCoroutine = StartCoroutine(ScanForQRCodes());
            isScanning = true;
            
            Debug.Log("[MobileCameraController] QR scanning started");
        }
        
        /// <summary>
        /// Stop QR code scanning
        /// </summary>
        public void StopScanning()
        {
            if (scanCoroutine != null)
            {
                StopCoroutine(scanCoroutine);
                scanCoroutine = null;
            }
            
            isScanning = false;
            
            Debug.Log("[MobileCameraController] QR scanning stopped");
        }
        
        /// <summary>
        /// Main QR code scanning coroutine
        /// </summary>
        private IEnumerator ScanForQRCodes()
        {
            while (isScanning && isCameraActive)
            {
                yield return new WaitForSeconds(scanInterval);
                
                if (webCamTexture != null && webCamTexture.isPlaying && webCamTexture.didUpdateThisFrame)
                {
                    try
                    {
                        DecodeQRCode();
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"[MobileCameraController] QR decode error: {e.Message}");
                    }
                }
            }
        }
        
        /// <summary>
        /// Decode QR code from camera texture
        /// </summary>
        private void DecodeQRCode()
        {
            if (webCamTexture == null) return;
            
            // Get pixels from camera texture
            Color32[] pixels = webCamTexture.GetPixels32();
            int width = webCamTexture.width;
            int height = webCamTexture.height;
            
            // Decode
            var result = barcodeReader.Decode(pixels, width, height);
            
            if (result != null)
            {
                ProcessQRCodeResult(result);
            }
        }
        
        /// <summary>
        /// Process detected QR code
        /// </summary>
        private void ProcessQRCodeResult(Result result)
        {
            int ventilNumber = ExtractVentilNumber(result.Text);
            
            if (ventilNumber < 0)
            {
                Debug.LogWarning($"[MobileCameraController] Invalid QR code: {result.Text}");
                return;
            }
            
            // Check if this is a new detection
            if (ventilNumber != lastDetectedVentil || Time.time - lastDetectionTime > 2f)
            {
                lastDetectedVentil = ventilNumber;
                lastDetectionTime = Time.time;
                
                Debug.Log($"[MobileCameraController] QR Code detected: Ventil {ventilNumber}");
                
                // Update UI
                ShowVentilInfo(ventilNumber);
                
                // Play haptic feedback on mobile
                PlayHapticFeedback();
                
                // Invoke event
                OnVentilDetected?.Invoke(ventilNumber);
            }
            else
            {
                // Update detection time for continuous scanning
                lastDetectionTime = Time.time;
            }
        }
        
        /// <summary>
        /// Extract ventil number from QR code content
        /// </summary>
        private int ExtractVentilNumber(string qrContent)
        {
            try
            {
                // Format: "VENTIL-042"
                if (qrContent.Contains("VENTIL-"))
                {
                    string numberPart = qrContent.Replace("VENTIL-", "").Trim();
                    return int.Parse(numberPart);
                }
                
                // Format: Just number
                if (int.TryParse(qrContent.Trim(), out int number))
                {
                    return number;
                }
                
                return -1;
            }
            catch
            {
                return -1;
            }
        }
        
        /// <summary>
        /// Show ventil information panel
        /// </summary>
        private void ShowVentilInfo(int ventilNumber)
        {
            if (ventilInfoPanel == null) return;
            
            ventilInfoPanel.SetActive(true);
            
            // Update ventil number
            if (ventilNumberText != null)
            {
                ventilNumberText.text = $"Ventil {ventilNumber}";
            }
            
            // Get sow data
            List<SowData> sows = DataRepository.Instance.GetSortedSowsByVentil(
                ventilNumber, 
                DataRepository.SortOrder.TrafficLightRedFirst
            );
            
            if (sows.Count > 0)
            {
                // Build info text
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                
                // Determine dominant traffic light color
                SowData.TrafficLightColor dominantColor = GetDominantTrafficLight(sows);
                UpdateTrafficLightDisplay(dominantColor);
                
                foreach (var sow in sows)
                {
                    string colorIndicator = GetColorIndicator(sow.trafficLight);
                    sb.AppendLine($"{colorIndicator} {sow.earTagNumber} - {sow.daysSinceMating} Tage");
                }
                
                if (sowInfoText != null)
                {
                    sowInfoText.text = sb.ToString();
                }
                
                UpdateStatusText($"Ventil {ventilNumber}: {sows.Count} Sauen gefunden");
            }
            else
            {
                if (sowInfoText != null)
                {
                    sowInfoText.text = "Keine Sauen zugeordnet";
                }
                
                UpdateTrafficLightDisplay(SowData.TrafficLightColor.Unknown);
                UpdateStatusText($"Ventil {ventilNumber}: Keine Daten");
            }
        }
        
        /// <summary>
        /// Get the dominant (highest priority) traffic light color
        /// </summary>
        private SowData.TrafficLightColor GetDominantTrafficLight(List<SowData> sows)
        {
            // Priority: Purple > Red > Yellow > Green > Unknown
            bool hasPurple = false;
            bool hasRed = false;
            bool hasYellow = false;
            bool hasGreen = false;
            
            foreach (var sow in sows)
            {
                switch (sow.trafficLight)
                {
                    case SowData.TrafficLightColor.Purple:
                        hasPurple = true;
                        break;
                    case SowData.TrafficLightColor.Red:
                        hasRed = true;
                        break;
                    case SowData.TrafficLightColor.Yellow:
                        hasYellow = true;
                        break;
                    case SowData.TrafficLightColor.Green:
                        hasGreen = true;
                        break;
                }
            }
            
            if (hasPurple) return SowData.TrafficLightColor.Purple;
            if (hasRed) return SowData.TrafficLightColor.Red;
            if (hasYellow) return SowData.TrafficLightColor.Yellow;
            if (hasGreen) return SowData.TrafficLightColor.Green;
            
            return SowData.TrafficLightColor.Unknown;
        }
        
        /// <summary>
        /// Update traffic light display
        /// </summary>
        private void UpdateTrafficLightDisplay(SowData.TrafficLightColor color)
        {
            if (trafficLightImage == null) return;
            
            trafficLightImage.color = GetTrafficLightColor(color);
        }
        
        /// <summary>
        /// Get color for traffic light
        /// </summary>
        private Color GetTrafficLightColor(SowData.TrafficLightColor trafficLight)
        {
            switch (trafficLight)
            {
                case SowData.TrafficLightColor.Green:
                    return greenColor;
                case SowData.TrafficLightColor.Yellow:
                    return yellowColor;
                case SowData.TrafficLightColor.Red:
                    return redColor;
                case SowData.TrafficLightColor.Purple:
                    return purpleColor;
                default:
                    return unknownColor;
            }
        }
        
        /// <summary>
        /// Get color indicator emoji for traffic light
        /// </summary>
        private string GetColorIndicator(SowData.TrafficLightColor color)
        {
            switch (color)
            {
                case SowData.TrafficLightColor.Green:
                    return "🟢";
                case SowData.TrafficLightColor.Yellow:
                    return "🟡";
                case SowData.TrafficLightColor.Red:
                    return "🔴";
                case SowData.TrafficLightColor.Purple:
                    return "🟣";
                default:
                    return "⚪";
            }
        }
        
        /// <summary>
        /// Hide ventil information panel
        /// </summary>
        public void HideVentilInfo()
        {
            if (ventilInfoPanel != null)
            {
                ventilInfoPanel.SetActive(false);
            }
            
            lastDetectedVentil = -1;
            UpdateStatusText("QR-Code scannen...");
        }
        
        /// <summary>
        /// Update status text
        /// </summary>
        private void UpdateStatusText(string text)
        {
            if (statusText != null)
            {
                statusText.text = text;
            }
        }
        
        /// <summary>
        /// Update debug information
        /// </summary>
        private void UpdateDebugInfo()
        {
            if (debugText == null) return;
            
            string info = $"Kamera: {(isCameraActive ? "Aktiv" : "Inaktiv")}\n" +
                         $"Scanning: {(isScanning ? "Ja" : "Nein")}\n";
            
            if (webCamTexture != null && webCamTexture.isPlaying)
            {
                info += $"Auflösung: {webCamTexture.width}x{webCamTexture.height}\n";
                info += $"FPS: {webCamTexture.requestedFPS}\n";
            }
            
            if (lastDetectedVentil >= 0)
            {
                info += $"Letztes Ventil: {lastDetectedVentil}\n";
            }
            
            debugText.text = info;
        }
        
        /// <summary>
        /// Play haptic feedback on successful scan
        /// </summary>
        private void PlayHapticFeedback()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    using (var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                    {
                        using (var vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator"))
                        {
                            vibrator.Call("vibrate", 50L); // 50ms vibration
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[MobileCameraController] Haptic feedback failed: {e.Message}");
            }
#endif
        }
        
        /// <summary>
        /// Manually trigger a test detection (for debugging)
        /// </summary>
        [ContextMenu("Test Detection (Ventil 1)")]
        public void TestDetection()
        {
            ShowVentilInfo(1);
            lastDetectedVentil = 1;
            lastDetectionTime = Time.time;
        }
    }
}
