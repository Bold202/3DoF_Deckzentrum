using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using ZXing;

namespace D8PlanerXR.AR
{
    /// <summary>
    /// QR-Code Tracking für AR
    /// Erkennt QR-Codes in der Kamera und erstellt Spatial Anchors
    /// </summary>
    [RequireComponent(typeof(ARCameraManager))]
    public class QRCodeTracker : MonoBehaviour
    {
        [Header("AR Komponenten")]
        [SerializeField] private ARCameraManager arCameraManager;
        [SerializeField] private Camera arCamera;
        
        [Header("Tracking Einstellungen")]
        [SerializeField] private float scanInterval = 0.2f; // Scan alle 0.2 Sekunden (5 FPS)
        [SerializeField] private float minDetectionConfidence = 0.8f;
        [SerializeField] private int maxSimultaneousDetections = 5;
        
        [Header("Overlay Prefab")]
        [SerializeField] private GameObject ventilOverlayPrefab;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = true;
        [SerializeField] private bool simulateQRCodesInEditor = false;

        // QR-Code Reader
        private IBarcodeReader barcodeReader;
        
        // Aktive Overlays: Key = Ventilnummer, Value = Overlay GameObject
        private Dictionary<int, VentilOverlay> activeOverlays = new Dictionary<int, VentilOverlay>();
        
        // Zuletzt erkannte QR-Codes mit Timestamp
        private Dictionary<int, float> lastDetectionTime = new Dictionary<int, float>();
        
        // Timeout für Overlay-Ausblendung (wenn QR-Code nicht mehr sichtbar)
        private const float OVERLAY_TIMEOUT = 3f;

        // Events
        public event Action<int> OnVentilDetected;
        public event Action<int> OnVentilLost;

        private Coroutine scanCoroutine;
        private Texture2D cameraTexture;

        private void Start()
        {
            InitializeQRCodeReader();
            
            if (arCameraManager == null)
                arCameraManager = GetComponent<ARCameraManager>();
            
            if (arCamera == null)
                arCamera = Camera.main;

            StartScanning();
        }

        private void OnDestroy()
        {
            StopScanning();
        }

        /// <summary>
        /// Initialisiert den QR-Code Reader
        /// </summary>
        private void InitializeQRCodeReader()
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
            
            Debug.Log("QR-Code Reader initialisiert");
        }

        /// <summary>
        /// Startet das kontinuierliche Scannen
        /// </summary>
        public void StartScanning()
        {
            if (scanCoroutine == null)
            {
                scanCoroutine = StartCoroutine(ScanForQRCodes());
                Debug.Log("QR-Code Scanning gestartet");
            }
        }

        /// <summary>
        /// Stoppt das Scannen
        /// </summary>
        public void StopScanning()
        {
            if (scanCoroutine != null)
            {
                StopCoroutine(scanCoroutine);
                scanCoroutine = null;
                Debug.Log("QR-Code Scanning gestoppt");
            }
        }

        /// <summary>
        /// Hauptschleife für QR-Code Erkennung
        /// </summary>
        private IEnumerator ScanForQRCodes()
        {
            while (true)
            {
                yield return new WaitForSeconds(scanInterval);

#if UNITY_EDITOR
                if (simulateQRCodesInEditor)
                {
                    SimulateQRCodeDetection();
                    continue;
                }
#endif

                // Kamerabild abrufen und nach QR-Codes scannen
                if (arCameraManager != null && arCameraManager.TryAcquireLatestCpuImage(out var image))
                {
                    try
                    {
                        ProcessCameraImage(image);
                    }
                    finally
                    {
                        image.Dispose();
                    }
                }

                // Timeout-Check für nicht mehr sichtbare QR-Codes
                CheckOverlayTimeouts();
            }
        }

        /// <summary>
        /// Verarbeitet ein Kamerabild und sucht nach QR-Codes
        /// </summary>
        private void ProcessCameraImage(XRCpuImage image)
        {
            // Konvertiere XRCpuImage zu Texture2D
            if (cameraTexture == null || cameraTexture.width != image.width || cameraTexture.height != image.height)
            {
                cameraTexture = new Texture2D(image.width, image.height, TextureFormat.RGBA32, false);
            }

            // Konvertierung (vereinfacht - in Produktion optimieren)
            var conversionParams = new XRCpuImage.ConversionParams
            {
                inputRect = new RectInt(0, 0, image.width, image.height),
                outputDimensions = new Vector2Int(image.width, image.height),
                outputFormat = TextureFormat.RGBA32,
                transformation = XRCpuImage.Transformation.None
            };

            int size = image.GetConvertedDataSize(conversionParams);
            var buffer = new NativeArray<byte>(size, Allocator.Temp);
            image.Convert(conversionParams, new IntPtr(buffer.GetUnsafePtr()), buffer.Length);
            
            cameraTexture.LoadRawTextureData(buffer);
            cameraTexture.Apply();
            buffer.Dispose();

            // MULTI-QR-Code dekodieren (unterstützt bis zu maxSimultaneousDetections)
            DecodeMultipleQRCodes(cameraTexture);
        }

        /// <summary>
        /// Dekodiert mehrere QR-Codes aus einer Textur (Multi-Detection)
        /// </summary>
        private void DecodeMultipleQRCodes(Texture2D texture)
        {
            try
            {
                // Verwende Multi-Decode für gleichzeitige Erkennung
                var reader = new ZXing.Multi.GenericMultipleBarcodeReader(
                    new ZXing.QrCode.QRCodeReader()
                );

                var results = reader.DecodeMultiple(
                    new BitmapLuminanceSource(texture.GetPixels32(), texture.width, texture.height)
                );
                
                if (results != null && results.Length > 0)
                {
                    int detectionCount = Mathf.Min(results.Length, maxSimultaneousDetections);
                    for (int i = 0; i < detectionCount; i++)
                    {
                        ProcessDetectedQRCode(results[i]);
                    }

                    if (showDebugInfo && results.Length > maxSimultaneousDetections)
                    {
                        Debug.LogWarning($"Mehr als {maxSimultaneousDetections} QR-Codes erkannt. " +
                                       $"Nur die ersten {maxSimultaneousDetections} werden verarbeitet.");
                    }
                }
            }
            catch (Exception e)
            {
                // Fallback auf Single-Detection
                try
                {
                    var result = barcodeReader.Decode(texture.GetPixels32(), texture.width, texture.height);
                    if (result != null)
                    {
                        ProcessDetectedQRCode(result);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"QR-Code Dekodierung fehlgeschlagen: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Helper Klasse für ZXing Multi-Detection
        /// </summary>
        private class BitmapLuminanceSource : ZXing.BaseLuminanceSource
        {
            private readonly byte[] luminances;

            public BitmapLuminanceSource(Color32[] pixels, int width, int height) : base(width, height)
            {
                luminances = new byte[width * height];
                for (int i = 0; i < pixels.Length; i++)
                {
                    Color32 c = pixels[i];
                    // Graustufenkonvertierung
                    luminances[i] = (byte)((c.r + c.g + c.b) / 3);
                }
            }

            public override byte[] Matrix => luminances;

            protected override LuminanceSource CreateLuminanceSource(byte[] newLuminances, int width, int height)
            {
                // Nicht verwendet für QR-Code Detection
                return null;
            }
        }

        /// <summary>
        /// Verarbeitet einen erkannten QR-Code
        /// </summary>
        private void ProcessDetectedQRCode(Result qrResult)
        {
            // Ventilnummer extrahieren
            int ventilNumber = ExtractVentilNumber(qrResult.Text);
            
            if (ventilNumber < 0)
            {
                Debug.LogWarning($"Ungültiger QR-Code erkannt: {qrResult.Text}");
                return;
            }

            // Timestamp aktualisieren
            lastDetectionTime[ventilNumber] = Time.time;

            // Overlay erstellen oder aktualisieren
            if (!activeOverlays.ContainsKey(ventilNumber))
            {
                CreateVentilOverlay(ventilNumber, qrResult);
                OnVentilDetected?.Invoke(ventilNumber);
            }
            else
            {
                UpdateVentilOverlay(ventilNumber, qrResult);
            }

            if (showDebugInfo)
            {
                Debug.Log($"QR-Code erkannt: Ventil {ventilNumber}");
            }
        }

        /// <summary>
        /// Erstellt ein neues Ventil-Overlay
        /// </summary>
        private void CreateVentilOverlay(int ventilNumber, Result qrResult)
        {
            if (ventilOverlayPrefab == null)
            {
                Debug.LogError("Ventil Overlay Prefab fehlt!");
                return;
            }

            // Position berechnen (QR-Code Position im 3D-Raum)
            Vector3 overlayPosition = CalculateOverlayPosition(qrResult);
            
            // Overlay instantiieren
            GameObject overlayObj = Instantiate(ventilOverlayPrefab, overlayPosition, Quaternion.identity);
            VentilOverlay overlay = overlayObj.GetComponent<VentilOverlay>();
            
            if (overlay != null)
            {
                overlay.Initialize(ventilNumber);
                activeOverlays[ventilNumber] = overlay;
            }
            else
            {
                Debug.LogError("VentilOverlay Komponente fehlt am Prefab!");
                Destroy(overlayObj);
            }
        }

        /// <summary>
        /// Aktualisiert ein existierendes Ventil-Overlay
        /// </summary>
        private void UpdateVentilOverlay(int ventilNumber, Result qrResult)
        {
            if (activeOverlays.TryGetValue(ventilNumber, out VentilOverlay overlay))
            {
                Vector3 newPosition = CalculateOverlayPosition(qrResult);
                overlay.UpdatePosition(newPosition);
            }
        }

        /// <summary>
        /// Berechnet die 3D-Position für das Overlay basierend auf QR-Code Position
        /// </summary>
        private Vector3 CalculateOverlayPosition(Result qrResult)
        {
            // Vereinfachte Positionsberechnung
            // In Produktion: AR Raycast verwenden für präzise 3D-Position
            
            // Annahme: QR-Code ist 1.5m vor der Kamera, 30cm unterhalb
            Vector3 cameraForward = arCamera.transform.forward;
            Vector3 cameraPosition = arCamera.transform.position;
            
            Vector3 position = cameraPosition + cameraForward * 1.5f + Vector3.down * 0.3f;
            return position;
        }

        /// <summary>
        /// Prüft Timeouts für Overlays und entfernt nicht mehr sichtbare
        /// </summary>
        private void CheckOverlayTimeouts()
        {
            List<int> toRemove = new List<int>();
            
            foreach (var kvp in lastDetectionTime)
            {
                if (Time.time - kvp.Value > OVERLAY_TIMEOUT)
                {
                    toRemove.Add(kvp.Key);
                }
            }

            foreach (int ventilNumber in toRemove)
            {
                RemoveVentilOverlay(ventilNumber);
            }
        }

        /// <summary>
        /// Entfernt ein Ventil-Overlay
        /// </summary>
        private void RemoveVentilOverlay(int ventilNumber)
        {
            if (activeOverlays.TryGetValue(ventilNumber, out VentilOverlay overlay))
            {
                Destroy(overlay.gameObject);
                activeOverlays.Remove(ventilNumber);
                lastDetectionTime.Remove(ventilNumber);
                OnVentilLost?.Invoke(ventilNumber);
                
                if (showDebugInfo)
                {
                    Debug.Log($"Overlay entfernt: Ventil {ventilNumber}");
                }
            }
        }

        /// <summary>
        /// Extrahiert die Ventilnummer aus QR-Code Inhalt
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
                
                // Format: Nur Zahl
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
        /// Simuliert QR-Code Erkennung im Unity Editor
        /// </summary>
        private void SimulateQRCodeDetection()
        {
            // Für Testing: Simuliere Ventil 1 vor der Kamera
            if (!activeOverlays.ContainsKey(1))
            {
                var fakeResult = new Result("VENTIL-001", null, null, BarcodeFormat.QR_CODE);
                ProcessDetectedQRCode(fakeResult);
            }
        }

        /// <summary>
        /// Gibt alle aktiven Overlays zurück
        /// </summary>
        public Dictionary<int, VentilOverlay> GetActiveOverlays()
        {
            return new Dictionary<int, VentilOverlay>(activeOverlays);
        }

        /// <summary>
        /// Entfernt alle Overlays
        /// </summary>
        public void ClearAllOverlays()
        {
            List<int> allVentils = new List<int>(activeOverlays.Keys);
            foreach (int ventilNumber in allVentils)
            {
                RemoveVentilOverlay(ventilNumber);
            }
        }
    }
}
