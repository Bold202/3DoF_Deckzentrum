using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using D8PlanerXR.Data;
using D8PlanerXR.Core;

namespace D8PlanerXR.AR
{
    /// <summary>
    /// Ventil-Overlay Komponente
    /// Zeigt Informationen über Sauen an einem Ventil als AR-Overlay an
    /// Unterstützt VR-Modus und Handy-Modus mit unterschiedlichen Update-Intervallen
    /// </summary>
    public class VentilOverlay : MonoBehaviour
    {
        [Header("UI-Komponenten")]
        [SerializeField] private Canvas canvas;
        [SerializeField] private TextMeshProUGUI ventilNumberText;
        [SerializeField] private Transform sowListContainer;
        [SerializeField] private GameObject sowItemPrefab;
        
        [Header("Overlay-Einstellungen")]
        [SerializeField] private float offsetFromQRCode = 0.3f; // Abstand unter QR-Code
        [SerializeField] private float updateInterval = 1f;
        [SerializeField] private bool alwaysFaceCamera = true;
        
        private int ventilNumber;
        private List<SowItemUI> sowItems = new List<SowItemUI>();
        private float lastUpdateTime;
        private Camera mainCamera;

        private void Start()
        {
            mainCamera = Camera.main;
            
            if (canvas != null)
            {
                canvas.worldCamera = mainCamera;
            }
        }

        private void Update()
        {
            // Canvas zur Kamera ausrichten
            if (alwaysFaceCamera && mainCamera != null)
            {
                transform.LookAt(mainCamera.transform);
                transform.Rotate(0, 180, 0); // Umdrehen, damit Text richtig herum ist
            }

            // Update-Intervall basierend auf Gerätemodus
            float currentUpdateInterval = updateInterval;
            if (DeviceModeManager.Instance != null)
            {
                currentUpdateInterval = DeviceModeManager.Instance.UpdateInterval;
            }

            // Periodisches Update der Daten
            if (Time.time - lastUpdateTime > currentUpdateInterval)
            {
                RefreshData();
                lastUpdateTime = Time.time;
            }
        }

        /// <summary>
        /// Initialisiert das Overlay für eine Ventilnummer
        /// </summary>
        public void Initialize(int ventilNumber)
        {
            this.ventilNumber = ventilNumber;
            
            if (ventilNumberText != null)
            {
                ventilNumberText.text = $"Ventil {ventilNumber}";
            }

            RefreshData();
        }

        /// <summary>
        /// Aktualisiert die angezeigten Daten
        /// </summary>
        public void RefreshData()
        {
            // Sauen für dieses Ventil abrufen
            List<SowData> sows = DataRepository.Instance.GetSortedSowsByVentil(
                ventilNumber, 
                DataRepository.SortOrder.TrafficLightRedFirst
            );

            // Alte UI-Items entfernen
            foreach (var item in sowItems)
            {
                if (item != null)
                    Destroy(item.gameObject);
            }
            sowItems.Clear();

            // Neue UI-Items erstellen
            foreach (var sow in sows)
            {
                CreateSowItem(sow);
            }

            // Wenn keine Sauen vorhanden, Hinweis anzeigen
            if (sows.Count == 0)
            {
                CreateEmptyMessage();
            }
        }

        /// <summary>
        /// Erstellt ein UI-Item für eine Sau
        /// </summary>
        private void CreateSowItem(SowData sow)
        {
            if (sowItemPrefab == null || sowListContainer == null)
            {
                Debug.LogError("Sow Item Prefab oder Container fehlt!");
                return;
            }

            GameObject itemObj = Instantiate(sowItemPrefab, sowListContainer);
            SowItemUI itemUI = itemObj.GetComponent<SowItemUI>();
            
            if (itemUI != null)
            {
                itemUI.SetData(sow);
                sowItems.Add(itemUI);
            }
        }

        /// <summary>
        /// Erstellt eine Nachricht wenn keine Sauen vorhanden sind
        /// </summary>
        private void CreateEmptyMessage()
        {
            if (sowItemPrefab == null || sowListContainer == null) return;

            GameObject itemObj = Instantiate(sowItemPrefab, sowListContainer);
            TextMeshProUGUI text = itemObj.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = "Keine Sauen zugeordnet";
                text.color = Color.gray;
            }
        }

        /// <summary>
        /// Aktualisiert die Position des Overlays
        /// </summary>
        public void UpdatePosition(Vector3 newPosition)
        {
            transform.position = newPosition + Vector3.down * offsetFromQRCode;
        }

        /// <summary>
        /// Zeigt oder versteckt das Overlay
        /// </summary>
        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }
    }

    /// <summary>
    /// UI-Komponente für eine einzelne Sau in der Liste
    /// </summary>
    public class SowItemUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI earTagText;
        [SerializeField] private TextMeshProUGUI daysText;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private Image trafficLightIcon;
        [SerializeField] private Image backgroundImage;

        // Ampel-Farben
        private static readonly Color GREEN_COLOR = new Color(0.3f, 0.8f, 0.3f);
        private static readonly Color YELLOW_COLOR = new Color(0.9f, 0.9f, 0.3f);
        private static readonly Color RED_COLOR = new Color(0.9f, 0.3f, 0.3f);
        private static readonly Color PURPLE_COLOR = new Color(0.7f, 0.2f, 0.9f); // Lila für Medikation!
        private static readonly Color UNKNOWN_COLOR = Color.gray;

        /// <summary>
        /// Setzt die Daten für die Anzeige
        /// </summary>
        public void SetData(SowData sow)
        {
            if (earTagText != null)
            {
                earTagText.text = sow.earTagNumber;
            }

            if (daysText != null)
            {
                daysText.text = $"{sow.daysSinceMating} Tage";
            }

            if (statusText != null)
            {
                statusText.text = string.IsNullOrEmpty(sow.pregnancyStatus) 
                    ? "-" 
                    : sow.pregnancyStatus;
            }

            // Ampelfarbe setzen
            Color trafficColor = GetTrafficLightColor(sow.trafficLight);
            
            if (trafficLightIcon != null)
            {
                trafficLightIcon.color = trafficColor;
            }

            if (backgroundImage != null)
            {
                // Leicht transparenter Hintergrund in Ampelfarbe
                backgroundImage.color = new Color(
                    trafficColor.r, 
                    trafficColor.g, 
                    trafficColor.b, 
                    0.3f
                );
            }
        }

        /// <summary>
        /// Gibt die Farbe für die Ampel zurück
        /// </summary>
        private Color GetTrafficLightColor(SowData.TrafficLightColor trafficLight)
        {
            switch (trafficLight)
            {
                case SowData.TrafficLightColor.Green:
                    return GREEN_COLOR;
                case SowData.TrafficLightColor.Yellow:
                    return YELLOW_COLOR;
                case SowData.TrafficLightColor.Red:
                    return RED_COLOR;
                case SowData.TrafficLightColor.Purple:
                    return PURPLE_COLOR;
                default:
                    return UNKNOWN_COLOR;
            }
        }
    }
}
