using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using D8PlanerXR.Data;

namespace D8PlanerXR.AR
{
    /// <summary>
    /// Virtuelles Deckzentrum - 3DoF Raumansicht aller Ventile
    /// Erstellt eine virtuelle Übersicht basierend auf gescannten QR-Code Positionen
    /// </summary>
    public class VirtualDeckzentrum : MonoBehaviour
    {
        [Header("Deckzentrum Einstellungen")]
        [SerializeField] private bool isActive = false;
        [SerializeField] private float ventilSpacing = 4f; // 400cm Abstand zwischen Ventilen
        [SerializeField] private float ventilHeight = 2.5f; // 250cm Höhe
        [SerializeField] private Vector3 centerPosition = new Vector3(0, 0, 5); // 5m vor Nutzer

        [Header("Prefabs")]
        [SerializeField] private GameObject ventilMarkerPrefab;
        [SerializeField] private GameObject overlayPrefab;

        [Header("Scan-basiert")]
        [SerializeField] private bool useScannedPositions = true;
        [SerializeField] private string setupFileName = "deckzentrum_setup.json";

        // Setup-Daten
        [Serializable]
        public class DeckzentrumSetup
        {
            public List<VentilPosition> ventilPositions = new List<VentilPosition>();
            public DateTime createdAt;
            public DateTime lastModified;
        }

        [Serializable]
        public class VentilPosition
        {
            public int ventilNumber;
            public Vector3 position;
            public Quaternion rotation;
            public bool wasScanned;
        }

        private DeckzentrumSetup currentSetup;
        private Dictionary<int, GameObject> ventilMarkers = new Dictionary<int, GameObject>();
        private Dictionary<int, VentilOverlay> ventilOverlays = new Dictionary<int, VentilOverlay>();
        private QRCodeTracker qrTracker;
        private bool isSetupMode = false;

        private void Start()
        {
            qrTracker = FindObjectOfType<QRCodeTracker>();
            if (qrTracker != null)
            {
                qrTracker.OnVentilDetected += OnVentilScanned;
            }

            LoadSetup();
        }

        private void OnDestroy()
        {
            if (qrTracker != null)
            {
                qrTracker.OnVentilDetected -= OnVentilScanned;
            }
        }

        /// <summary>
        /// Aktiviert/Deaktiviert das virtuelle Deckzentrum
        /// </summary>
        public void SetActive(bool active)
        {
            isActive = active;
            
            if (isActive)
            {
                ShowDeckzentrum();
            }
            else
            {
                HideDeckzentrum();
            }
        }

        /// <summary>
        /// Zeigt das virtuelle Deckzentrum an
        /// </summary>
        private void ShowDeckzentrum()
        {
            if (currentSetup == null || currentSetup.ventilPositions.Count == 0)
            {
                // Kein Setup vorhanden - erstelle automatisches Layout
                CreateAutomaticLayout();
            }
            else
            {
                // Zeige Setup-basiertes Layout
                DisplaySetupLayout();
            }
        }

        /// <summary>
        /// Versteckt das virtuelle Deckzentrum
        /// </summary>
        private void HideDeckzentrum()
        {
            foreach (var marker in ventilMarkers.Values)
            {
                if (marker != null)
                    marker.SetActive(false);
            }

            foreach (var overlay in ventilOverlays.Values)
            {
                if (overlay != null)
                    overlay.SetVisible(false);
            }
        }

        /// <summary>
        /// Erstellt automatisches Layout (wenn kein Setup vorhanden)
        /// </summary>
        private void CreateAutomaticLayout()
        {
            Debug.Log("Erstelle automatisches Deckzentrum-Layout...");

            List<int> allVentils = DataRepository.Instance.GetAllVentilNumbers();
            if (allVentils.Count == 0)
            {
                // Fallback: Zeige Ventile 1-199 (oder nur die mit Daten)
                Debug.LogWarning("Keine Ventil-Daten gefunden. Zeige nur Marker.");
                return;
            }

            allVentils.Sort();

            // Berechne Layout (z.B. in Reihen)
            int ventilsPerRow = 10;
            int currentRow = 0;
            int currentCol = 0;

            foreach (int ventilNum in allVentils)
            {
                Vector3 position = CalculateGridPosition(currentRow, currentCol);
                CreateVentilMarker(ventilNum, position, Quaternion.identity, false);

                currentCol++;
                if (currentCol >= ventilsPerRow)
                {
                    currentCol = 0;
                    currentRow++;
                }
            }
        }

        /// <summary>
        /// Berechnet Position in einem Gitter-Layout
        /// </summary>
        private Vector3 CalculateGridPosition(int row, int col)
        {
            float x = centerPosition.x + (col - 5) * ventilSpacing; // Zentriert um x=0
            float y = centerPosition.y + ventilHeight - (row * 0.5f); // Leicht versetzt nach unten
            float z = centerPosition.z;
            return new Vector3(x, y, z);
        }

        /// <summary>
        /// Zeigt Setup-basiertes Layout (aus gescannten Positionen)
        /// </summary>
        private void DisplaySetupLayout()
        {
            Debug.Log("Zeige Deckzentrum aus gespeichertem Setup...");

            foreach (var ventilPos in currentSetup.ventilPositions)
            {
                CreateVentilMarker(ventilPos.ventilNumber, ventilPos.position, ventilPos.rotation, ventilPos.wasScanned);
            }
        }

        /// <summary>
        /// Erstellt einen Ventil-Marker und Overlay
        /// </summary>
        private void CreateVentilMarker(int ventilNumber, Vector3 position, Quaternion rotation, bool wasScanned)
        {
            // Marker erstellen
            if (ventilMarkerPrefab != null && !ventilMarkers.ContainsKey(ventilNumber))
            {
                GameObject marker = Instantiate(ventilMarkerPrefab, position, rotation, transform);
                marker.name = $"Ventil_{ventilNumber}_Marker";
                
                // Marker mit Nummer beschriften
                var textMesh = marker.GetComponentInChildren<TMPro.TextMeshPro>();
                if (textMesh != null)
                {
                    textMesh.text = ventilNumber.ToString();
                }

                ventilMarkers[ventilNumber] = marker;
            }

            // Overlay erstellen
            if (overlayPrefab != null && !ventilOverlays.ContainsKey(ventilNumber))
            {
                Vector3 overlayPos = position + Vector3.down * 0.3f;
                GameObject overlayObj = Instantiate(overlayPrefab, overlayPos, Quaternion.identity, transform);
                overlayObj.name = $"Ventil_{ventilNumber}_Overlay";
                
                VentilOverlay overlay = overlayObj.GetComponent<VentilOverlay>();
                if (overlay != null)
                {
                    overlay.Initialize(ventilNumber);
                    ventilOverlays[ventilNumber] = overlay;
                }
            }
        }

        /// <summary>
        /// Callback wenn ein Ventil gescannt wird (für Setup-Modus)
        /// </summary>
        private void OnVentilScanned(int ventilNumber)
        {
            if (!isSetupMode) return;

            // Position des gescannten QR-Codes speichern
            var activeOverlays = qrTracker.GetActiveOverlays();
            if (activeOverlays.ContainsKey(ventilNumber))
            {
                VentilOverlay overlay = activeOverlays[ventilNumber];
                Vector3 position = overlay.transform.position;
                Quaternion rotation = overlay.transform.rotation;

                AddOrUpdateVentilPosition(ventilNumber, position, rotation, true);
                Debug.Log($"Ventil {ventilNumber} Position gespeichert: {position}");
            }
        }

        /// <summary>
        /// Fügt eine Ventil-Position zum Setup hinzu oder aktualisiert sie
        /// </summary>
        private void AddOrUpdateVentilPosition(int ventilNumber, Vector3 position, Quaternion rotation, bool wasScanned)
        {
            if (currentSetup == null)
            {
                currentSetup = new DeckzentrumSetup();
                currentSetup.createdAt = DateTime.Now;
            }

            VentilPosition existing = currentSetup.ventilPositions.Find(v => v.ventilNumber == ventilNumber);
            if (existing != null)
            {
                existing.position = position;
                existing.rotation = rotation;
                existing.wasScanned = wasScanned;
            }
            else
            {
                currentSetup.ventilPositions.Add(new VentilPosition
                {
                    ventilNumber = ventilNumber,
                    position = position,
                    rotation = rotation,
                    wasScanned = wasScanned
                });
            }

            currentSetup.lastModified = DateTime.Now;
        }

        /// <summary>
        /// Startet den Setup-Modus (Positionen erfassen)
        /// </summary>
        public void StartSetupMode()
        {
            isSetupMode = true;
            currentSetup = new DeckzentrumSetup();
            currentSetup.createdAt = DateTime.Now;
            Debug.Log("Setup-Modus gestartet. Scanne jetzt alle Ventile im Stall.");
        }

        /// <summary>
        /// Beendet den Setup-Modus und speichert
        /// </summary>
        public void FinishSetupMode()
        {
            isSetupMode = false;
            SaveSetup();
            Debug.Log($"Setup-Modus beendet. {currentSetup.ventilPositions.Count} Ventile gespeichert.");
        }

        /// <summary>
        /// Speichert das Setup
        /// </summary>
        private void SaveSetup()
        {
            if (currentSetup == null) return;

            try
            {
                string json = JsonUtility.ToJson(currentSetup, true);
                string filePath = Path.Combine(Application.persistentDataPath, setupFileName);
                File.WriteAllText(filePath, json);
                Debug.Log($"Deckzentrum-Setup gespeichert: {filePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Fehler beim Speichern des Setups: {e.Message}");
            }
        }

        /// <summary>
        /// Lädt das Setup
        /// </summary>
        private void LoadSetup()
        {
            string filePath = Path.Combine(Application.persistentDataPath, setupFileName);
            
            if (File.Exists(filePath))
            {
                try
                {
                    string json = File.ReadAllText(filePath);
                    currentSetup = JsonUtility.FromJson<DeckzentrumSetup>(json);
                    Debug.Log($"Deckzentrum-Setup geladen: {currentSetup.ventilPositions.Count} Ventile");
                }
                catch (Exception e)
                {
                    Debug.LogError($"Fehler beim Laden des Setups: {e.Message}");
                    currentSetup = null;
                }
            }
            else
            {
                Debug.Log("Kein Setup gefunden. Verwende automatisches Layout.");
                currentSetup = null;
            }
        }

        /// <summary>
        /// Löscht das aktuelle Setup
        /// </summary>
        public void ClearSetup()
        {
            currentSetup = null;
            string filePath = Path.Combine(Application.persistentDataPath, setupFileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            Debug.Log("Setup gelöscht.");
        }

        /// <summary>
        /// Gibt Statistiken zurück
        /// </summary>
        public string GetStatistics()
        {
            if (currentSetup == null)
            {
                return "Kein Setup vorhanden";
            }

            int scannedCount = currentSetup.ventilPositions.FindAll(v => v.wasScanned).Count;
            return $"Ventile gesamt: {currentSetup.ventilPositions.Count}\n" +
                   $"Gescannt: {scannedCount}\n" +
                   $"Erstellt: {currentSetup.createdAt:dd.MM.yyyy HH:mm}\n" +
                   $"Geändert: {currentSetup.lastModified:dd.MM.yyyy HH:mm}";
        }

#if UNITY_EDITOR
        [ContextMenu("Setup-Modus starten")]
        private void EditorStartSetup()
        {
            StartSetupMode();
        }

        [ContextMenu("Setup-Modus beenden")]
        private void EditorFinishSetup()
        {
            FinishSetupMode();
        }

        [ContextMenu("Deckzentrum anzeigen")]
        private void EditorShowDeckzentrum()
        {
            SetActive(true);
        }

        [ContextMenu("Deckzentrum verstecken")]
        private void EditorHideDeckzentrum()
        {
            SetActive(false);
        }
#endif
    }
}
