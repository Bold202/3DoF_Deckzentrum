using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using D8PlanerXR.Data;

namespace D8PlanerXR.UI
{
    /// <summary>
    /// UI-Manager für das CSV-Spalten-Konfigurationsmenü
    /// Ermöglicht das Hinzufügen, Entfernen, Umbenennen und Neuordnen von Spalten
    /// </summary>
    public class CSVColumnMenuUI : MonoBehaviour
    {
        [Header("UI-Referenzen")]
        [SerializeField] private GameObject menuPanel;
        [SerializeField] private Transform columnListContainer;
        [SerializeField] private GameObject columnItemPrefab;
        
        [Header("Buttons")]
        [SerializeField] private Button addColumnButton;
        [SerializeField] private Button saveButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private Button importFromCSVButton;
        [SerializeField] private Button saveAsProfileButton;
        [SerializeField] private Button loadProfileButton;
        
        [Header("Input Fields")]
        [SerializeField] private TMP_InputField newColumnNameInput;
        [SerializeField] private TMP_Dropdown columnTypeDropdown;
        [SerializeField] private TMP_Dropdown columnRoleDropdown;
        [SerializeField] private TMP_InputField csvFilePathInput;
        [SerializeField] private TMP_InputField profileNameInput;
        [SerializeField] private TMP_Dropdown profileSelectionDropdown;
        
        [Header("Einstellungen")]
        [SerializeField] private TMP_Dropdown delimiterDropdown;
        [SerializeField] private Toggle hasHeaderToggle;
        [SerializeField] private TMP_Dropdown encodingDropdown;
        
        [Header("Feedback")]
        [SerializeField] private TextMeshProUGUI statusText;
        
        private List<ColumnItemUI> columnItems = new List<ColumnItemUI>();
        private CSVColumnConfig workingConfig;

        private void Start()
        {
            InitializeUI();
            SetupEventListeners();
            menuPanel.SetActive(false);
        }

        /// <summary>
        /// Initialisiert die UI-Komponenten
        /// </summary>
        private void InitializeUI()
        {
            // Dropdown für Spaltentyp initialisieren
            if (columnTypeDropdown != null)
            {
                columnTypeDropdown.ClearOptions();
                List<string> typeOptions = new List<string>();
                foreach (CSVColumnConfig.ColumnType type in System.Enum.GetValues(typeof(CSVColumnConfig.ColumnType)))
                {
                    typeOptions.Add(type.ToString());
                }
                columnTypeDropdown.AddOptions(typeOptions);
            }

            // Dropdown für Spaltenrolle initialisieren
            if (columnRoleDropdown != null)
            {
                columnRoleDropdown.ClearOptions();
                List<string> roleOptions = new List<string>();
                foreach (CSVColumnConfig.ColumnRole role in System.Enum.GetValues(typeof(CSVColumnConfig.ColumnRole)))
                {
                    roleOptions.Add(role.ToString());
                }
                columnRoleDropdown.AddOptions(roleOptions);
            }

            // Dropdown für Delimiter initialisieren
            if (delimiterDropdown != null)
            {
                delimiterDropdown.ClearOptions();
                List<string> delimiterOptions = new List<string> { "Semikolon (;)", "Komma (,)", "Tabulator" };
                delimiterDropdown.AddOptions(delimiterOptions);
            }

            // Dropdown für Encoding initialisieren
            if (encodingDropdown != null)
            {
                encodingDropdown.ClearOptions();
                List<string> encodingOptions = new List<string> { "UTF-8", "ISO-8859-1", "Windows-1252" };
                encodingDropdown.AddOptions(encodingOptions);
            }
        }

        /// <summary>
        /// Richtet Event Listener ein
        /// </summary>
        private void SetupEventListeners()
        {
            if (addColumnButton != null)
                addColumnButton.onClick.AddListener(OnAddColumn);
            
            if (saveButton != null)
                saveButton.onClick.AddListener(OnSave);
            
            if (cancelButton != null)
                cancelButton.onClick.AddListener(OnCancel);
            
            if (importFromCSVButton != null)
                importFromCSVButton.onClick.AddListener(OnImportFromCSV);
            
            if (saveAsProfileButton != null)
                saveAsProfileButton.onClick.AddListener(OnSaveAsProfile);
            
            if (loadProfileButton != null)
                loadProfileButton.onClick.AddListener(OnLoadProfile);
            
            if (delimiterDropdown != null)
                delimiterDropdown.onValueChanged.AddListener(OnDelimiterChanged);
            
            if (hasHeaderToggle != null)
                hasHeaderToggle.onValueChanged.AddListener(OnHeaderToggleChanged);
            
            if (encodingDropdown != null)
                encodingDropdown.onValueChanged.AddListener(OnEncodingChanged);
        }

        /// <summary>
        /// Öffnet das Menü
        /// </summary>
        public void OpenMenu()
        {
            // Arbeitskopie der aktuellen Konfiguration erstellen
            workingConfig = CloneConfig(CSVConfigManager.Instance.CurrentConfig);
            
            RefreshUI();
            RefreshProfileList();
            menuPanel.SetActive(true);
            ShowStatus("CSV-Spalten-Konfiguration geöffnet", Color.white);
        }

        /// <summary>
        /// Schließt das Menü
        /// </summary>
        public void CloseMenu()
        {
            menuPanel.SetActive(false);
        }

        /// <summary>
        /// Aktualisiert die gesamte UI
        /// </summary>
        private void RefreshUI()
        {
            // Alle vorhandenen Items löschen
            foreach (var item in columnItems)
            {
                if (item != null)
                    Destroy(item.gameObject);
            }
            columnItems.Clear();

            // Neue Items erstellen
            foreach (var column in workingConfig.columns)
            {
                CreateColumnItem(column);
            }

            // Einstellungen aktualisieren
            UpdateSettingsUI();
        }

        /// <summary>
        /// Aktualisiert die Einstellungs-UI
        /// </summary>
        private void UpdateSettingsUI()
        {
            if (delimiterDropdown != null)
            {
                switch (workingConfig.delimiter)
                {
                    case ';': delimiterDropdown.value = 0; break;
                    case ',': delimiterDropdown.value = 1; break;
                    case '\t': delimiterDropdown.value = 2; break;
                }
            }

            if (hasHeaderToggle != null)
                hasHeaderToggle.isOn = workingConfig.hasHeader;

            if (encodingDropdown != null)
            {
                switch (workingConfig.encoding)
                {
                    case "UTF-8": encodingDropdown.value = 0; break;
                    case "ISO-8859-1": encodingDropdown.value = 1; break;
                    case "Windows-1252": encodingDropdown.value = 2; break;
                }
            }
        }

        /// <summary>
        /// Erstellt ein UI-Item für eine Spalte
        /// </summary>
        private void CreateColumnItem(CSVColumnConfig.ColumnDefinition column)
        {
            if (columnItemPrefab == null || columnListContainer == null)
            {
                Debug.LogError("Column Item Prefab oder Container fehlt!");
                return;
            }

            GameObject itemObj = Instantiate(columnItemPrefab, columnListContainer);
            ColumnItemUI itemUI = itemObj.GetComponent<ColumnItemUI>();
            
            if (itemUI != null)
            {
                itemUI.Initialize(column, this);
                columnItems.Add(itemUI);
            }
        }

        /// <summary>
        /// Fügt eine neue Spalte hinzu
        /// </summary>
        private void OnAddColumn()
        {
            if (newColumnNameInput == null || string.IsNullOrWhiteSpace(newColumnNameInput.text))
            {
                ShowStatus("Bitte geben Sie einen Spaltennamen ein!", Color.red);
                return;
            }

            string columnName = newColumnNameInput.text.Trim();
            CSVColumnConfig.ColumnType columnType = (CSVColumnConfig.ColumnType)columnTypeDropdown.value;
            CSVColumnConfig.ColumnRole columnRole = (CSVColumnConfig.ColumnRole)columnRoleDropdown.value;

            // Prüfen ob Spalte bereits existiert
            if (workingConfig.columns.Exists(c => c.originalName == columnName))
            {
                ShowStatus($"Spalte '{columnName}' existiert bereits!", Color.red);
                return;
            }

            workingConfig.AddColumn(columnName, columnType, columnRole);
            CreateColumnItem(workingConfig.columns[workingConfig.columns.Count - 1]);
            
            newColumnNameInput.text = "";
            ShowStatus($"Spalte '{columnName}' hinzugefügt", Color.green);
        }

        /// <summary>
        /// Entfernt eine Spalte
        /// </summary>
        public void RemoveColumn(string columnName)
        {
            if (workingConfig.RemoveColumn(columnName))
            {
                RefreshUI();
                ShowStatus($"Spalte '{columnName}' entfernt", Color.yellow);
            }
        }

        /// <summary>
        /// Benennt eine Spalte um
        /// </summary>
        public void RenameColumn(string originalName, string newDisplayName)
        {
            if (workingConfig.RenameColumn(originalName, newDisplayName))
            {
                ShowStatus($"Spalte umbenannt: {originalName} → {newDisplayName}", Color.green);
            }
        }

        /// <summary>
        /// Verschiebt eine Spalte nach oben
        /// </summary>
        public void MoveColumnUp(string columnName)
        {
            var column = workingConfig.columns.Find(c => c.originalName == columnName);
            if (column != null && column.displayOrder > 0)
            {
                workingConfig.ReorderColumn(columnName, column.displayOrder - 1);
                RefreshUI();
                ShowStatus($"Spalte '{columnName}' nach oben verschoben", Color.white);
            }
        }

        /// <summary>
        /// Verschiebt eine Spalte nach unten
        /// </summary>
        public void MoveColumnDown(string columnName)
        {
            var column = workingConfig.columns.Find(c => c.originalName == columnName);
            if (column != null && column.displayOrder < workingConfig.columns.Count - 1)
            {
                workingConfig.ReorderColumn(columnName, column.displayOrder + 1);
                RefreshUI();
                ShowStatus($"Spalte '{columnName}' nach unten verschoben", Color.white);
            }
        }

        /// <summary>
        /// Speichert die Konfiguration
        /// </summary>
        private void OnSave()
        {
            if (!workingConfig.Validate(out string errorMessage))
            {
                ShowStatus($"Ungültige Konfiguration: {errorMessage}", Color.red);
                return;
            }

            // Arbeitskopie zur aktuellen Konfiguration machen
            CSVConfigManager.Instance.CurrentConfig.columns = workingConfig.columns;
            CSVConfigManager.Instance.CurrentConfig.delimiter = workingConfig.delimiter;
            CSVConfigManager.Instance.CurrentConfig.hasHeader = workingConfig.hasHeader;
            CSVConfigManager.Instance.CurrentConfig.encoding = workingConfig.encoding;

            if (CSVConfigManager.Instance.SaveCurrentConfig())
            {
                ShowStatus("Konfiguration erfolgreich gespeichert!", Color.green);
                Invoke(nameof(CloseMenu), 1.5f);
            }
            else
            {
                ShowStatus("Fehler beim Speichern!", Color.red);
            }
        }

        /// <summary>
        /// Bricht die Bearbeitung ab
        /// </summary>
        private void OnCancel()
        {
            CloseMenu();
        }

        /// <summary>
        /// Importiert Spalten aus einer CSV-Datei
        /// </summary>
        private void OnImportFromCSV()
        {
            if (csvFilePathInput == null || string.IsNullOrWhiteSpace(csvFilePathInput.text))
            {
                ShowStatus("Bitte geben Sie einen CSV-Dateipfad ein!", Color.red);
                return;
            }

            string csvPath = csvFilePathInput.text.Trim();
            if (CSVConfigManager.Instance.ImportColumnsFromCSV(csvPath))
            {
                workingConfig = CloneConfig(CSVConfigManager.Instance.CurrentConfig);
                RefreshUI();
                ShowStatus($"Spalten aus CSV importiert: {csvPath}", Color.green);
            }
            else
            {
                ShowStatus("Fehler beim Importieren der CSV-Datei!", Color.red);
            }
        }

        /// <summary>
        /// Speichert die aktuelle Konfiguration als Profil
        /// </summary>
        private void OnSaveAsProfile()
        {
            if (profileNameInput == null || string.IsNullOrWhiteSpace(profileNameInput.text))
            {
                ShowStatus("Bitte geben Sie einen Profilnamen ein!", Color.red);
                return;
            }

            // Erst die aktuelle Arbeitskonfiguration speichern
            CSVConfigManager.Instance.CurrentConfig.columns = workingConfig.columns;
            CSVConfigManager.Instance.CurrentConfig.delimiter = workingConfig.delimiter;
            CSVConfigManager.Instance.CurrentConfig.hasHeader = workingConfig.hasHeader;
            CSVConfigManager.Instance.CurrentConfig.encoding = workingConfig.encoding;

            string profileName = profileNameInput.text.Trim();
            if (CSVConfigManager.Instance.SaveConfigAsProfile(profileName))
            {
                RefreshProfileList();
                profileNameInput.text = "";
                ShowStatus($"Profil '{profileName}' gespeichert!", Color.green);
            }
            else
            {
                ShowStatus("Fehler beim Speichern des Profils!", Color.red);
            }
        }

        /// <summary>
        /// Lädt ein Profil
        /// </summary>
        private void OnLoadProfile()
        {
            if (profileSelectionDropdown == null || profileSelectionDropdown.options.Count == 0)
            {
                ShowStatus("Keine Profile verfügbar!", Color.red);
                return;
            }

            string selectedProfile = profileSelectionDropdown.options[profileSelectionDropdown.value].text;
            if (CSVConfigManager.Instance.LoadConfigProfile(selectedProfile))
            {
                workingConfig = CloneConfig(CSVConfigManager.Instance.CurrentConfig);
                RefreshUI();
                ShowStatus($"Profil '{selectedProfile}' geladen!", Color.green);
            }
            else
            {
                ShowStatus("Fehler beim Laden des Profils!", Color.red);
            }
        }

        /// <summary>
        /// Aktualisiert die Profil-Liste
        /// </summary>
        private void RefreshProfileList()
        {
            if (profileSelectionDropdown == null) return;

            profileSelectionDropdown.ClearOptions();
            List<string> profiles = CSVConfigManager.Instance.GetAvailableProfiles();
            
            if (profiles.Count > 0)
            {
                profileSelectionDropdown.AddOptions(profiles);
            }
            else
            {
                profileSelectionDropdown.AddOptions(new List<string> { "Keine Profile vorhanden" });
            }
        }

        /// <summary>
        /// Delimiter geändert
        /// </summary>
        private void OnDelimiterChanged(int value)
        {
            switch (value)
            {
                case 0: workingConfig.delimiter = ';'; break;
                case 1: workingConfig.delimiter = ','; break;
                case 2: workingConfig.delimiter = '\t'; break;
            }
        }

        /// <summary>
        /// Header Toggle geändert
        /// </summary>
        private void OnHeaderToggleChanged(bool value)
        {
            workingConfig.hasHeader = value;
        }

        /// <summary>
        /// Encoding geändert
        /// </summary>
        private void OnEncodingChanged(int value)
        {
            switch (value)
            {
                case 0: workingConfig.encoding = "UTF-8"; break;
                case 1: workingConfig.encoding = "ISO-8859-1"; break;
                case 2: workingConfig.encoding = "Windows-1252"; break;
            }
        }

        /// <summary>
        /// Zeigt eine Statusnachricht an
        /// </summary>
        private void ShowStatus(string message, Color color)
        {
            if (statusText != null)
            {
                statusText.text = message;
                statusText.color = color;
                Debug.Log($"[CSV Menu] {message}");
            }
        }

        /// <summary>
        /// Klont eine Konfiguration
        /// </summary>
        private CSVColumnConfig CloneConfig(CSVColumnConfig source)
        {
            string json = JsonUtility.ToJson(source);
            return JsonUtility.FromJson<CSVColumnConfig>(json);
        }

        /// <summary>
        /// Öffnet einen Datei-Browser (plattformabhängig)
        /// </summary>
        public void BrowseForCSV()
        {
            // Hier könnte ein plattformabhängiger File Picker implementiert werden
            // Für Android: Intent, für Unity Editor: EditorUtility.OpenFilePanel
            ShowStatus("Datei-Browser wird in der finalen Version implementiert", Color.yellow);
        }
    }

    /// <summary>
    /// UI-Komponente für ein einzelnes Spalten-Item in der Liste
    /// </summary>
    public class ColumnItemUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI columnNameText;
        [SerializeField] private TextMeshProUGUI columnTypeText;
        [SerializeField] private TextMeshProUGUI columnRoleText;
        [SerializeField] private TMP_InputField displayNameInput;
        [SerializeField] private Toggle visibilityToggle;
        [SerializeField] private Button moveUpButton;
        [SerializeField] private Button moveDownButton;
        [SerializeField] private Button removeButton;
        
        private CSVColumnConfig.ColumnDefinition column;
        private CSVColumnMenuUI menuUI;

        public void Initialize(CSVColumnConfig.ColumnDefinition column, CSVColumnMenuUI menuUI)
        {
            this.column = column;
            this.menuUI = menuUI;

            UpdateUI();
            SetupListeners();
        }

        private void UpdateUI()
        {
            if (columnNameText != null)
                columnNameText.text = column.originalName;
            
            if (columnTypeText != null)
                columnTypeText.text = column.columnType.ToString();
            
            if (columnRoleText != null)
                columnRoleText.text = column.role.ToString();
            
            if (displayNameInput != null)
                displayNameInput.text = column.displayName;
            
            if (visibilityToggle != null)
                visibilityToggle.isOn = column.isVisible;
        }

        private void SetupListeners()
        {
            if (displayNameInput != null)
                displayNameInput.onEndEdit.AddListener(OnDisplayNameChanged);
            
            if (visibilityToggle != null)
                visibilityToggle.onValueChanged.AddListener(OnVisibilityChanged);
            
            if (moveUpButton != null)
                moveUpButton.onClick.AddListener(OnMoveUp);
            
            if (moveDownButton != null)
                moveDownButton.onClick.AddListener(OnMoveDown);
            
            if (removeButton != null)
                removeButton.onClick.AddListener(OnRemove);
        }

        private void OnDisplayNameChanged(string newName)
        {
            column.displayName = newName;
            menuUI.RenameColumn(column.originalName, newName);
        }

        private void OnVisibilityChanged(bool isVisible)
        {
            column.isVisible = isVisible;
        }

        private void OnMoveUp()
        {
            menuUI.MoveColumnUp(column.originalName);
        }

        private void OnMoveDown()
        {
            menuUI.MoveColumnDown(column.originalName);
        }

        private void OnRemove()
        {
            menuUI.RemoveColumn(column.originalName);
        }
    }
}
