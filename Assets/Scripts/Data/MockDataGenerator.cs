using System;
using System.IO;
using UnityEngine;

namespace D8PlanerXR.Data
{
    /// <summary>
    /// Generator für Mock-Testdaten
    /// Erstellt realistische CSV-Daten für Tests
    /// </summary>
    public class MockDataGenerator : MonoBehaviour
    {
        [Header("Mock-Daten Einstellungen")]
        [SerializeField] private int numberOfSows = 50;
        [SerializeField] private int numberOfVentils = 20;
        [SerializeField] private string mockDataFileName = "mock_sauenplaner.csv";

        /// <summary>
        /// Generiert Mock-CSV-Daten
        /// </summary>
        public string GenerateMockCSV()
        {
            System.Text.StringBuilder csv = new System.Text.StringBuilder();
            
            // Header
            csv.AppendLine("Sauennummer;Ventilnummer;Deckdatum;Trächtigkeitsstatus;Gesundheitsstatus;Bemerkungen");

            System.Random random = new System.Random();
            DateTime today = DateTime.Now;

            for (int i = 1; i <= numberOfSows; i++)
            {
                // Zufällige Ventilnummer (1 bis numberOfVentils)
                int ventilNumber = random.Next(1, numberOfVentils + 1);

                // Ohrmarkennummer (z.B. "DE1234567890")
                string earTag = $"DE{1000000000 + i:D10}";

                // Deckdatum (zufällig in den letzten 120 Tagen)
                int daysAgo = random.Next(1, 120);
                DateTime matingDate = today.AddDays(-daysAgo);
                string matingDateStr = matingDate.ToString("dd.MM.yyyy");

                // Trächtigkeitsstatus
                string pregnancyStatus;
                if (daysAgo < 21)
                    pregnancyStatus = "unbestätigt";
                else if (daysAgo < 107)
                    pregnancyStatus = "tragend";
                else
                    pregnancyStatus = "hochträchtig";

                // Gesundheitsstatus (90% gesund, 10% Medikation)
                string healthStatus = "";
                if (random.Next(100) < 10)
                {
                    string[] conditions = { "Medikation", "Behandlung", "Krank - Medikation" };
                    healthStatus = conditions[random.Next(conditions.Length)];
                }

                // Bemerkungen (optional)
                string notes = "";
                if (random.Next(100) < 20)
                {
                    string[] notesList = { 
                        "Frisst gut", 
                        "Lahmheit beobachtet", 
                        "Kontrolle empfohlen",
                        "Sehr aktiv",
                        "Ruhig"
                    };
                    notes = notesList[random.Next(notesList.Length)];
                }

                // CSV-Zeile
                csv.AppendLine($"{earTag};{ventilNumber};{matingDateStr};{pregnancyStatus};{healthStatus};{notes}");
            }

            return csv.ToString();
        }

        /// <summary>
        /// Speichert Mock-Daten als CSV-Datei
        /// </summary>
        public void SaveMockDataToFile()
        {
            string csvContent = GenerateMockCSV();
            string filePath = Path.Combine(Application.persistentDataPath, mockDataFileName);
            
            try
            {
                File.WriteAllText(filePath, csvContent, System.Text.Encoding.UTF8);
                Debug.Log($"Mock-Daten gespeichert: {filePath}");
                Debug.Log($"{numberOfSows} Sauen an {numberOfVentils} Ventilen generiert.");
            }
            catch (Exception e)
            {
                Debug.LogError($"Fehler beim Speichern der Mock-Daten: {e.Message}");
            }
        }

        /// <summary>
        /// Erstellt mehrere Test-Szenarien
        /// </summary>
        public void CreateTestScenarios()
        {
            // Szenario 1: Normale Daten
            CreateScenario("test_normal.csv", 30, 10, false, false);

            // Szenario 2: Leere Datei
            CreateScenario("test_empty.csv", 0, 0, false, false);

            // Szenario 3: Mit fehlerhaften Zeilen
            CreateScenario("test_corrupted.csv", 20, 5, true, false);

            // Szenario 4: Viele Sauen (Stress-Test)
            CreateScenario("test_large.csv", 500, 50, false, false);

            // Szenario 5: Alle Sauen mit Medikation
            CreateScenario("test_all_medication.csv", 20, 5, false, true);

            Debug.Log("Alle Test-Szenarien erstellt!");
        }

        /// <summary>
        /// Erstellt ein spezifisches Test-Szenario
        /// </summary>
        private void CreateScenario(string fileName, int sows, int ventils, bool addErrors, bool allMedication)
        {
            System.Text.StringBuilder csv = new System.Text.StringBuilder();
            csv.AppendLine("Sauennummer;Ventilnummer;Deckdatum;Trächtigkeitsstatus;Gesundheitsstatus;Bemerkungen");

            if (sows == 0) // Leere Datei
            {
                string filePath = Path.Combine(Application.persistentDataPath, fileName);
                File.WriteAllText(filePath, csv.ToString(), System.Text.Encoding.UTF8);
                return;
            }

            System.Random random = new System.Random();
            DateTime today = DateTime.Now;

            for (int i = 1; i <= sows; i++)
            {
                int ventilNumber = random.Next(1, ventils + 1);
                string earTag = $"DE{1000000000 + i:D10}";
                int daysAgo = random.Next(1, 120);
                DateTime matingDate = today.AddDays(-daysAgo);
                string matingDateStr = matingDate.ToString("dd.MM.yyyy");
                string pregnancyStatus = daysAgo < 21 ? "unbestätigt" : (daysAgo < 107 ? "tragend" : "hochträchtig");
                string healthStatus = allMedication ? "Medikation" : (random.Next(100) < 10 ? "Medikation" : "");
                string notes = "";

                // Fehlerhafte Zeile einfügen?
                if (addErrors && random.Next(100) < 20)
                {
                    // Verschiedene Fehlertypen
                    int errorType = random.Next(4);
                    switch (errorType)
                    {
                        case 0: // Fehlende Spalte
                            csv.AppendLine($"{earTag};{ventilNumber};{matingDateStr}");
                            break;
                        case 1: // Ungültige Ventilnummer
                            csv.AppendLine($"{earTag};INVALID;{matingDateStr};{pregnancyStatus};{healthStatus};{notes}");
                            break;
                        case 2: // Ungültiges Datum
                            csv.AppendLine($"{earTag};{ventilNumber};99.99.9999;{pregnancyStatus};{healthStatus};{notes}");
                            break;
                        case 3: // Leere Zeile
                            csv.AppendLine("");
                            break;
                    }
                }
                else
                {
                    csv.AppendLine($"{earTag};{ventilNumber};{matingDateStr};{pregnancyStatus};{healthStatus};{notes}");
                }
            }

            string path = Path.Combine(Application.persistentDataPath, fileName);
            File.WriteAllText(path, csv.ToString(), System.Text.Encoding.UTF8);
            Debug.Log($"Szenario erstellt: {fileName}");
        }

        /// <summary>
        /// Erstellt Beispiel-Daten mit spezifischen Ampel-Status
        /// </summary>
        public void CreateTrafficLightTestData()
        {
            System.Text.StringBuilder csv = new System.Text.StringBuilder();
            csv.AppendLine("Sauennummer;Ventilnummer;Deckdatum;Trächtigkeitsstatus;Gesundheitsstatus;Bemerkungen");

            DateTime today = DateTime.Now;

            // Grün: 0-79 Tage
            for (int i = 1; i <= 5; i++)
            {
                int days = i * 15; // 15, 30, 45, 60, 75
                DateTime matingDate = today.AddDays(-days);
                csv.AppendLine($"DE100000000{i};1;{matingDate:dd.MM.yyyy};tragend;;Grüner Status - {days} Tage");
            }

            // Gelb: 80-106 Tage
            for (int i = 1; i <= 5; i++)
            {
                int days = 80 + (i * 5); // 85, 90, 95, 100, 105
                DateTime matingDate = today.AddDays(-days);
                csv.AppendLine($"DE200000000{i};2;{matingDate:dd.MM.yyyy};tragend;;Gelber Status - {days} Tage");
            }

            // Rot: 107+ Tage
            for (int i = 1; i <= 5; i++)
            {
                int days = 107 + i; // 108, 109, 110, 111, 112
                DateTime matingDate = today.AddDays(-days);
                csv.AppendLine($"DE300000000{i};3;{matingDate:dd.MM.yyyy};hochträchtig;;Roter Status - {days} Tage");
            }

            // Lila: Medikation
            for (int i = 1; i <= 5; i++)
            {
                int days = 50 + (i * 10);
                DateTime matingDate = today.AddDays(-days);
                csv.AppendLine($"DE400000000{i};4;{matingDate:dd.MM.yyyy};tragend;Medikation;Lila Status - Behandlung nötig");
            }

            string filePath = Path.Combine(Application.persistentDataPath, "test_traffic_lights.csv");
            File.WriteAllText(filePath, csv.ToString(), System.Text.Encoding.UTF8);
            Debug.Log($"Ampel-Test-Daten erstellt: {filePath}");
        }

#if UNITY_EDITOR
        [ContextMenu("Mock-Daten generieren")]
        private void EditorGenerateMockData()
        {
            SaveMockDataToFile();
        }

        [ContextMenu("Alle Test-Szenarien erstellen")]
        private void EditorCreateTestScenarios()
        {
            CreateTestScenarios();
        }

        [ContextMenu("Ampel-Test-Daten erstellen")]
        private void EditorCreateTrafficLightData()
        {
            CreateTrafficLightTestData();
        }
#endif
    }
}
