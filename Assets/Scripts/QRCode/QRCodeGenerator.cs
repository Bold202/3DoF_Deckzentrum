using System;
using System.IO;
using UnityEngine;
using ZXing;
using ZXing.QrCode;
using ZXing.Common;

namespace D8PlanerXR.QRCode
{
    /// <summary>
    /// QR-Code Generator für Ventilnummern
    /// Erstellt QR-Codes für Ventile 1-199 und speichert sie als Bilddateien
    /// </summary>
    public class QRCodeGenerator : MonoBehaviour
    {
        [Header("QR-Code Einstellungen")]
        [SerializeField] private int qrCodeWidth = 512;
        [SerializeField] private int qrCodeHeight = 512;
        [SerializeField] private int margin = 1;
        
        [Header("Fehlerkorrektur")]
        [Tooltip("L=7%, M=15%, Q=25%, H=30% - Höher = besser für verschmutzte Umgebung")]
        [SerializeField] private ErrorCorrectionLevel errorCorrectionLevel = ErrorCorrectionLevel.H;
        
        [Header("Ventil-Bereich")]
        [SerializeField] private int startVentilNumber = 1;
        [SerializeField] private int endVentilNumber = 199;
        
        [Header("Ausgabe")]
        [SerializeField] private string outputFolder = "QRCodes";
        [SerializeField] private string filePrefix = "Ventil_";
        [SerializeField] private string fileExtension = ".png";
        
        [Header("QR-Code Inhalt")]
        [Tooltip("Template für QR-Code Inhalt. {0} wird durch Ventilnummer ersetzt")]
        [SerializeField] private string contentTemplate = "VENTIL-{0:000}";
        
        [Header("Label-Optionen")]
        [SerializeField] private bool addHumanReadableNumber = true;
        [SerializeField] private int labelHeight = 50;
        [SerializeField] private int fontSize = 32;

        private string fullOutputPath;

        /// <summary>
        /// Generiert alle QR-Codes für den angegebenen Ventil-Bereich
        /// </summary>
        public void GenerateAllQRCodes()
        {
            // Ausgabeordner erstellen
            fullOutputPath = Path.Combine(Application.persistentDataPath, outputFolder);
            if (!Directory.Exists(fullOutputPath))
            {
                Directory.CreateDirectory(fullOutputPath);
            }

            Debug.Log($"Starte QR-Code Generierung für Ventile {startVentilNumber} bis {endVentilNumber}");
            Debug.Log($"Ausgabeordner: {fullOutputPath}");

            int successCount = 0;
            int errorCount = 0;

            for (int ventilNumber = startVentilNumber; ventilNumber <= endVentilNumber; ventilNumber++)
            {
                try
                {
                    GenerateSingleQRCode(ventilNumber);
                    successCount++;
                    
                    if (ventilNumber % 10 == 0)
                    {
                        Debug.Log($"Fortschritt: {ventilNumber}/{endVentilNumber} QR-Codes erstellt");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Fehler bei Ventil {ventilNumber}: {e.Message}");
                    errorCount++;
                }
            }

            Debug.Log($"QR-Code Generierung abgeschlossen!");
            Debug.Log($"Erfolgreich: {successCount}, Fehler: {errorCount}");
            Debug.Log($"Dateien gespeichert in: {fullOutputPath}");

#if UNITY_EDITOR
            // Im Editor den Ordner öffnen
            UnityEditor.EditorUtility.RevealInFinder(fullOutputPath);
#endif
        }

        /// <summary>
        /// Generiert einen einzelnen QR-Code für eine Ventilnummer
        /// </summary>
        public Texture2D GenerateSingleQRCode(int ventilNumber)
        {
            // QR-Code Inhalt erstellen
            string content = string.Format(contentTemplate, ventilNumber);
            
            // QR-Code generieren
            BarcodeWriter writer = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new QrCodeEncodingOptions
                {
                    Width = qrCodeWidth,
                    Height = qrCodeHeight,
                    Margin = margin,
                    ErrorCorrection = errorCorrectionLevel
                }
            };

            // QR-Code als Color32 Array generieren
            Color32[] pixels = writer.Write(content);
            
            // Textur erstellen
            Texture2D qrTexture = new Texture2D(qrCodeWidth, qrCodeHeight, TextureFormat.RGBA32, false);
            qrTexture.SetPixels32(pixels);
            qrTexture.Apply();

            // Optional: Menschenlesbare Nummer hinzufügen
            if (addHumanReadableNumber)
            {
                qrTexture = AddLabelToQRCode(qrTexture, ventilNumber);
            }

            // Als Datei speichern
            SaveQRCodeToFile(qrTexture, ventilNumber);

            return qrTexture;
        }

        /// <summary>
        /// Fügt eine menschenlesbare Nummer unter dem QR-Code hinzu
        /// </summary>
        private Texture2D AddLabelToQRCode(Texture2D qrCode, int ventilNumber)
        {
            int totalHeight = qrCode.height + labelHeight;
            Texture2D labeledTexture = new Texture2D(qrCode.width, totalHeight, TextureFormat.RGBA32, false);

            // QR-Code kopieren (oben)
            Color[] qrPixels = qrCode.GetPixels();
            labeledTexture.SetPixels(0, labelHeight, qrCode.width, qrCode.height, qrPixels);

            // Weißen Hintergrund für Label (unten)
            Color[] whitePixels = new Color[qrCode.width * labelHeight];
            for (int i = 0; i < whitePixels.Length; i++)
            {
                whitePixels[i] = Color.white;
            }
            labeledTexture.SetPixels(0, 0, qrCode.width, labelHeight, whitePixels);

            labeledTexture.Apply();

            // Hinweis: Für echten Text würde man TextMesh Pro oder eine Rendering-Bibliothek nutzen
            // Hier zeichnen wir eine einfache visuelle Markierung
            DrawSimpleNumberIndicator(labeledTexture, ventilNumber);

            return labeledTexture;
        }

        /// <summary>
        /// Zeichnet eine simple visuelle Nummer-Indikation
        /// (Für Produktionsumgebung sollte echtes Text-Rendering verwendet werden)
        /// </summary>
        private void DrawSimpleNumberIndicator(Texture2D texture, int number)
        {
            // Einfache Balken-Kodierung für die Zahl
            int barWidth = 3;
            int spacing = 2;
            int startX = texture.width / 2 - 50;
            int startY = 10;

            string numberStr = number.ToString("000");
            int xPos = startX;

            foreach (char digit in numberStr)
            {
                int digitValue = digit - '0';
                
                // Zeichne vertikale Balken entsprechend dem Zahlenwert
                for (int i = 0; i < digitValue; i++)
                {
                    for (int y = startY; y < startY + 30; y++)
                    {
                        for (int x = xPos + i * (barWidth + spacing); x < xPos + i * (barWidth + spacing) + barWidth; x++)
                        {
                            if (x >= 0 && x < texture.width && y >= 0 && y < texture.height)
                            {
                                texture.SetPixel(x, y, Color.black);
                            }
                        }
                    }
                }
                
                xPos += 35;
            }

            texture.Apply();
        }

        /// <summary>
        /// Speichert den QR-Code als PNG-Datei
        /// </summary>
        private void SaveQRCodeToFile(Texture2D texture, int ventilNumber)
        {
            byte[] bytes = texture.EncodeToPNG();
            string fileName = $"{filePrefix}{ventilNumber:000}{fileExtension}";
            string filePath = Path.Combine(fullOutputPath, fileName);
            
            File.WriteAllBytes(filePath, bytes);
        }

        /// <summary>
        /// Generiert einen einzelnen QR-Code und gibt die Datei-URL zurück
        /// </summary>
        public string GenerateAndGetPath(int ventilNumber)
        {
            fullOutputPath = Path.Combine(Application.persistentDataPath, outputFolder);
            if (!Directory.Exists(fullOutputPath))
            {
                Directory.CreateDirectory(fullOutputPath);
            }

            GenerateSingleQRCode(ventilNumber);
            
            string fileName = $"{filePrefix}{ventilNumber:000}{fileExtension}";
            return Path.Combine(fullOutputPath, fileName);
        }

        /// <summary>
        /// Dekodiert einen QR-Code aus einer Textur
        /// </summary>
        public string DecodeQRCode(Texture2D texture)
        {
            try
            {
                BarcodeReader reader = new BarcodeReader();
                var result = reader.Decode(texture.GetPixels32(), texture.width, texture.height);
                
                if (result != null)
                {
                    return result.Text;
                }
                return null;
            }
            catch (Exception e)
            {
                Debug.LogError($"Fehler beim Dekodieren des QR-Codes: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Extrahiert die Ventilnummer aus dem QR-Code Inhalt
        /// </summary>
        public int ExtractVentilNumber(string qrContent)
        {
            try
            {
                // Versuche verschiedene Formate
                // Format: "VENTIL-042"
                if (qrContent.Contains("VENTIL-"))
                {
                    string numberPart = qrContent.Replace("VENTIL-", "");
                    return int.Parse(numberPart);
                }
                
                // Format: Nur Zahl "42"
                if (int.TryParse(qrContent, out int number))
                {
                    return number;
                }

                Debug.LogWarning($"Unbekanntes QR-Code Format: {qrContent}");
                return -1;
            }
            catch (Exception e)
            {
                Debug.LogError($"Fehler beim Extrahieren der Ventilnummer: {e.Message}");
                return -1;
            }
        }

        /// <summary>
        /// Generiert einen Test-QR-Code als Sprite für UI-Anzeige
        /// </summary>
        public Sprite GenerateQRCodeSprite(int ventilNumber)
        {
            Texture2D qrTexture = GenerateSingleQRCode(ventilNumber);
            Sprite sprite = Sprite.Create(
                qrTexture,
                new Rect(0, 0, qrTexture.width, qrTexture.height),
                new Vector2(0.5f, 0.5f)
            );
            return sprite;
        }

#if UNITY_EDITOR
        [ContextMenu("Generiere alle QR-Codes (1-199)")]
        private void EditorGenerateAll()
        {
            GenerateAllQRCodes();
        }

        [ContextMenu("Generiere Test QR-Code (Ventil 1)")]
        private void EditorGenerateTest()
        {
            fullOutputPath = Path.Combine(Application.persistentDataPath, outputFolder);
            if (!Directory.Exists(fullOutputPath))
            {
                Directory.CreateDirectory(fullOutputPath);
            }
            GenerateSingleQRCode(1);
            Debug.Log($"Test QR-Code erstellt: {fullOutputPath}");
        }
#endif
    }

    /// <summary>
    /// Enum für Fehlerkorrektur-Level
    /// Höhere Stufen erlauben mehr beschädigte Bereiche, benötigen aber mehr Daten
    /// </summary>
    public enum ErrorCorrectionLevel
    {
        L = 0,  // ~7% Wiederherstellung
        M = 1,  // ~15% Wiederherstellung
        Q = 2,  // ~25% Wiederherstellung
        H = 3   // ~30% Wiederherstellung (Empfohlen für Stallumgebung)
    }
}
