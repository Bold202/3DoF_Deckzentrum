using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace D8PlanerXR.Editor
{
    /// <summary>
    /// Setup-Wizard für das D8-Planer XR Projekt
    /// Konfiguriert automatisch alle Unity-Einstellungen für die AR-Anwendung
    /// Unterstützt Viture Neckband Pro + Handy-Modus (Dual-Mode)
    /// </summary>
    public class UnityProjectSetup : EditorWindow
    {
        #region Konstanten und Konfiguration
        
        // Projekt-Informationen
        private const string COMPANY_NAME = "D8-Planer";
        private const string PRODUCT_NAME = "D8-Planer XR";
        private const string PACKAGE_NAME = "com.d8planer.deckzentrum";
        private const string VERSION = "1.2.0";
        
        // Android API Levels
        private const int MIN_API_LEVEL = 24;  // Android 7.0
        private const int TARGET_API_LEVEL = 33;  // Android 13
        
        // Erforderliche Pakete
        private static readonly string[] REQUIRED_PACKAGES = new string[]
        {
            "com.unity.xr.arfoundation",
            "com.unity.xr.arcore",
            "com.unity.textmeshpro"
        };
        
        // Szenen-Pfade
        private const string MAIN_SCENE_PATH = "Assets/Scenes/MainScene.unity";
        private const string SCENES_FOLDER = "Assets/Scenes";
        
        // Cache-Einstellungen
        private const double MANIFEST_CACHE_TIMEOUT_SECONDS = 5.0;
        
        // Komponenten-Typnamen für Szenen-Erstellung
        // Die Typnamen werden dynamisch aufgelöst, um mit verschiedenen Assembly-Konfigurationen zu funktionieren
        private static readonly string[] APP_CONTROLLER_ASSEMBLIES = { "Assembly-CSharp", "D8PlanerXR" };
        private static readonly string[] DEVICE_MODE_MANAGER_ASSEMBLIES = { "Assembly-CSharp", "D8PlanerXR" };
        private static readonly string[] VIRTUAL_DECKZENTRUM_ASSEMBLIES = { "Assembly-CSharp", "D8PlanerXR" };
        
        private const string APP_CONTROLLER_TYPENAME = "D8PlanerXR.Core.AppController";
        private const string DEVICE_MODE_MANAGER_TYPENAME = "D8PlanerXR.Core.DeviceModeManager";
        private const string VIRTUAL_DECKZENTRUM_TYPENAME = "D8PlanerXR.AR.VirtualDeckzentrum";
        
        #endregion
        
        #region UI Status-Tracking
        
        private Vector2 scrollPosition;
        private bool showPackageSection = true;
        private bool showBuildSection = true;
        private bool showPlayerSection = true;
        private bool showXRSection = true;
        private bool showQualitySection = true;
        private bool showManifestSection = true;
        private bool showSceneSection = true;
        
        // Setup-Status
        private SetupStatus packageStatus = SetupStatus.NotChecked;
        private SetupStatus buildStatus = SetupStatus.NotChecked;
        private SetupStatus playerStatus = SetupStatus.NotChecked;
        private SetupStatus xrStatus = SetupStatus.NotChecked;
        private SetupStatus qualityStatus = SetupStatus.NotChecked;
        private SetupStatus manifestStatus = SetupStatus.NotChecked;
        private SetupStatus sceneStatus = SetupStatus.NotChecked;
        
        private enum SetupStatus
        {
            NotChecked,
            OK,
            NeedsConfiguration,
            Error
        }
        
        #endregion
        
        #region Unity Editor Menü Integration
        
        /// <summary>
        /// Öffnet den Setup-Wizard über das Unity-Menü
        /// </summary>
        [MenuItem("D8-Planer/Setup Wizard", false, 100)]
        public static void ShowWindow()
        {
            var window = GetWindow<UnityProjectSetup>("D8-Planer Setup");
            window.minSize = new Vector2(500, 600);
            window.Show();
        }
        
        /// <summary>
        /// Schnellzugriff: Alle Einstellungen anwenden
        /// </summary>
        [MenuItem("D8-Planer/Alle Einstellungen anwenden", false, 101)]
        public static void ApplyAllSettingsFromMenu()
        {
            if (EditorUtility.DisplayDialog(
                "D8-Planer Setup",
                "Alle Projekteinstellungen für D8-Planer XR anwenden?\n\n" +
                "Dies konfiguriert:\n" +
                "• Android Build Settings\n" +
                "• Player Settings (Package Name, API Levels)\n" +
                "• XR Plugin Management\n" +
                "• Quality Settings\n" +
                "• AndroidManifest.xml",
                "Ja, anwenden",
                "Abbrechen"))
            {
                ApplyAllSettings();
            }
        }
        
        /// <summary>
        /// Prüft den aktuellen Setup-Status
        /// </summary>
        [MenuItem("D8-Planer/Setup-Status prüfen", false, 102)]
        public static void CheckSetupStatus()
        {
            var window = GetWindow<UnityProjectSetup>("D8-Planer Setup");
            window.CheckAllStatus();
            window.Show();
        }
        
        [MenuItem("D8-Planer/Hauptszene erstellen", false, 200)]
        public static void CreateMainSceneFromMenu()
        {
            CreateMainScene();
        }
        
        [MenuItem("D8-Planer/AndroidManifest erstellen", false, 201)]
        public static void CreateManifestFromMenu()
        {
            CreateAndroidManifest();
            AssetDatabase.Refresh();
        }
        
        #endregion
        
        #region Editor Window GUI
        
        private void OnEnable()
        {
            CheckAllStatus();
        }
        
        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            // Header
            DrawHeader();
            
            EditorGUILayout.Space(10);
            
            // Schnellaktionen
            DrawQuickActions();
            
            EditorGUILayout.Space(10);
            
            // Detaillierte Sektionen
            DrawPackageSection();
            DrawBuildSettingsSection();
            DrawPlayerSettingsSection();
            DrawXRSettingsSection();
            DrawQualitySettingsSection();
            DrawManifestSection();
            DrawSceneSection();
            
            EditorGUILayout.Space(20);
            
            // Footer
            DrawFooter();
            
            EditorGUILayout.EndScrollView();
        }
        
        /// <summary>
        /// Zeichnet den Header mit Logo und Titel
        /// </summary>
        private void DrawHeader()
        {
            EditorGUILayout.Space(10);
            
            // Titel
            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 18,
                alignment = TextAnchor.MiddleCenter
            };
            EditorGUILayout.LabelField("D8-Planer XR Setup Wizard", titleStyle);
            
            // Untertitel
            GUIStyle subtitleStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 11,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Italic
            };
            EditorGUILayout.LabelField("AR-Anwendung für Viture Neckband Pro + Handy-Modus", subtitleStyle);
            
            EditorGUILayout.Space(5);
            
            // Version Info
            EditorGUILayout.LabelField($"Version: {VERSION}", EditorStyles.centeredGreyMiniLabel);
        }
        
        /// <summary>
        /// Zeichnet die Schnellaktionen-Buttons
        /// </summary>
        private void DrawQuickActions()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Schnellaktionen", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            // Status prüfen
            if (GUILayout.Button("Status prüfen", GUILayout.Height(30)))
            {
                CheckAllStatus();
            }
            
            // Alles anwenden
            GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);
            if (GUILayout.Button("Alle Einstellungen anwenden", GUILayout.Height(30)))
            {
                ApplyAllSettings();
                CheckAllStatus();
            }
            GUI.backgroundColor = Color.white;
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }
        
        /// <summary>
        /// Zeichnet die Package-Sektion
        /// </summary>
        private void DrawPackageSection()
        {
            showPackageSection = DrawSectionHeader("1. Package-Überprüfung", packageStatus, showPackageSection);
            
            if (showPackageSection)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                foreach (var package in REQUIRED_PACKAGES)
                {
                    bool isInstalled = IsPackageInstalled(package);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(GetStatusIcon(isInstalled), GUILayout.Width(20));
                    EditorGUILayout.LabelField(package);
                    EditorGUILayout.EndHorizontal();
                }
                
                EditorGUILayout.Space(5);
                EditorGUILayout.HelpBox(
                    "Die Pakete werden über die manifest.json im Packages-Ordner verwaltet. " +
                    "Bei fehlenden Paketen nutze den Package Manager (Window > Package Manager).",
                    MessageType.Info);
                
                EditorGUILayout.EndVertical();
            }
        }
        
        /// <summary>
        /// Zeichnet die Build Settings Sektion
        /// </summary>
        private void DrawBuildSettingsSection()
        {
            showBuildSection = DrawSectionHeader("2. Android Build Settings", buildStatus, showBuildSection);
            
            if (showBuildSection)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                // Aktuelle Einstellungen anzeigen
                bool isAndroid = EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android;
                DrawSettingRow("Platform", isAndroid ? "Android ✓" : "Nicht Android ✗", isAndroid);
                
                var textureCompression = EditorUserBuildSettings.androidBuildSubtarget;
                bool isASTC = textureCompression == MobileTextureSubtarget.ASTC;
                DrawSettingRow("Texture Compression", textureCompression.ToString(), isASTC);
                
                EditorGUILayout.Space(5);
                
                if (GUILayout.Button("Build Settings konfigurieren"))
                {
                    ConfigureBuildSettings();
                    CheckBuildStatus();
                }
                
                EditorGUILayout.EndVertical();
            }
        }
        
        /// <summary>
        /// Zeichnet die Player Settings Sektion
        /// </summary>
        private void DrawPlayerSettingsSection()
        {
            showPlayerSection = DrawSectionHeader("3. Player Settings", playerStatus, showPlayerSection);
            
            if (showPlayerSection)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                // Aktuelle Einstellungen anzeigen
                DrawSettingRow("Company Name", PlayerSettings.companyName, 
                    PlayerSettings.companyName == COMPANY_NAME);
                DrawSettingRow("Product Name", PlayerSettings.productName, 
                    PlayerSettings.productName == PRODUCT_NAME);
                DrawSettingRow("Package Name", PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android), 
                    PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android) == PACKAGE_NAME);
                DrawSettingRow("Version", PlayerSettings.bundleVersion, 
                    PlayerSettings.bundleVersion == VERSION);
                DrawSettingRow("Min API Level", ((int)PlayerSettings.Android.minSdkVersion).ToString(), 
                    (int)PlayerSettings.Android.minSdkVersion == MIN_API_LEVEL);
                DrawSettingRow("Target API Level", ((int)PlayerSettings.Android.targetSdkVersion).ToString(), 
                    (int)PlayerSettings.Android.targetSdkVersion == TARGET_API_LEVEL);
                DrawSettingRow("Scripting Backend", PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android).ToString(),
                    PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android) == ScriptingImplementation.IL2CPP);
                
                // Architektur prüfen
                bool hasARM64 = (PlayerSettings.Android.targetArchitectures & AndroidArchitecture.ARM64) != 0;
                bool hasARMv7 = (PlayerSettings.Android.targetArchitectures & AndroidArchitecture.ARMv7) != 0;
                DrawSettingRow("Target Architectures", 
                    $"ARM64: {(hasARM64 ? "✓" : "✗")}, ARMv7: {(hasARMv7 ? "✓" : "✗")}", 
                    hasARM64 && hasARMv7);
                
                EditorGUILayout.Space(5);
                
                if (GUILayout.Button("Player Settings konfigurieren"))
                {
                    ConfigurePlayerSettings();
                    CheckPlayerStatus();
                }
                
                EditorGUILayout.EndVertical();
            }
        }
        
        /// <summary>
        /// Zeichnet die XR Settings Sektion
        /// </summary>
        private void DrawXRSettingsSection()
        {
            showXRSection = DrawSectionHeader("4. XR Plugin Management", xrStatus, showXRSection);
            
            if (showXRSection)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                EditorGUILayout.HelpBox(
                    "XR Plugin Management wird über das Unity XR Plugin System konfiguriert.\n" +
                    "Gehe zu: Edit > Project Settings > XR Plug-in Management",
                    MessageType.Info);
                
                EditorGUILayout.Space(5);
                
                EditorGUILayout.BeginHorizontal();
                
                if (GUILayout.Button("XR Plugin Settings öffnen"))
                {
                    SettingsService.OpenProjectSettings("Project/XR Plug-in Management");
                }
                
                if (GUILayout.Button("ARCore aktivieren"))
                {
                    ConfigureXRSettings();
                    CheckXRStatus();
                }
                
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.EndVertical();
            }
        }
        
        /// <summary>
        /// Zeichnet die Quality Settings Sektion
        /// </summary>
        private void DrawQualitySettingsSection()
        {
            showQualitySection = DrawSectionHeader("5. Quality Settings", qualityStatus, showQualitySection);
            
            if (showQualitySection)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                // Aktuelle Quality-Einstellungen anzeigen
                DrawSettingRow("Anti-Aliasing", QualitySettings.antiAliasing.ToString(), 
                    QualitySettings.antiAliasing == 2);
                DrawSettingRow("VSync", QualitySettings.vSyncCount.ToString(), 
                    QualitySettings.vSyncCount == 1);
                DrawSettingRow("Shadow Quality", QualitySettings.shadows.ToString(), 
                    QualitySettings.shadows == ShadowQuality.HardOnly);
                DrawSettingRow("Texture Quality", QualitySettings.globalTextureMipmapLimit.ToString(), 
                    QualitySettings.globalTextureMipmapLimit == 0);
                
                EditorGUILayout.Space(5);
                
                if (GUILayout.Button("Quality Settings für Mobile optimieren"))
                {
                    ConfigureQualitySettings();
                    CheckQualityStatus();
                }
                
                EditorGUILayout.EndVertical();
            }
        }
        
        /// <summary>
        /// Zeichnet die AndroidManifest Sektion
        /// </summary>
        private void DrawManifestSection()
        {
            showManifestSection = DrawSectionHeader("6. AndroidManifest.xml", manifestStatus, showManifestSection);
            
            if (showManifestSection)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                string manifestPath = "Assets/Plugins/Android/AndroidManifest.xml";
                bool manifestExists = File.Exists(manifestPath);
                
                DrawSettingRow("AndroidManifest.xml", manifestExists ? "Vorhanden" : "Nicht vorhanden", manifestExists);
                
                if (manifestExists)
                {
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField("Erforderliche Berechtigungen:", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField("• android.permission.CAMERA");
                    EditorGUILayout.LabelField("• android.permission.READ_EXTERNAL_STORAGE");
                    EditorGUILayout.LabelField("• android.permission.WRITE_EXTERNAL_STORAGE");
                    EditorGUILayout.LabelField("• android.permission.INTERNET");
                    EditorGUILayout.LabelField("• android.hardware.camera.ar (ARCore)");
                }
                
                EditorGUILayout.Space(5);
                
                if (GUILayout.Button(manifestExists ? "AndroidManifest.xml aktualisieren" : "AndroidManifest.xml erstellen"))
                {
                    CreateAndroidManifest();
                    CheckManifestStatus();
                    AssetDatabase.Refresh();
                }
                
                EditorGUILayout.EndVertical();
            }
        }
        
        /// <summary>
        /// Zeichnet die Scene Setup Sektion
        /// </summary>
        private void DrawSceneSection()
        {
            showSceneSection = DrawSectionHeader("7. Szenen-Setup", sceneStatus, showSceneSection);
            
            if (showSceneSection)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                bool sceneExists = File.Exists(MAIN_SCENE_PATH);
                DrawSettingRow("Hauptszene", sceneExists ? "Vorhanden" : "Nicht vorhanden", sceneExists);
                
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Die Hauptszene enthält:", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("• AR Session");
                EditorGUILayout.LabelField("• XR Origin (ARSessionOrigin)");
                EditorGUILayout.LabelField("• AR Camera mit QR-Code Tracker");
                EditorGUILayout.LabelField("• AppController");
                EditorGUILayout.LabelField("• DeviceModeManager");
                EditorGUILayout.LabelField("• UI Canvas");
                
                EditorGUILayout.Space(5);
                
                if (GUILayout.Button(sceneExists ? "Hauptszene neu erstellen" : "Hauptszene erstellen"))
                {
                    if (sceneExists)
                    {
                        if (!EditorUtility.DisplayDialog("Szene überschreiben?",
                            "Die Hauptszene existiert bereits. Möchtest du sie überschreiben?",
                            "Ja, überschreiben", "Abbrechen"))
                        {
                            return;
                        }
                    }
                    CreateMainScene();
                    CheckSceneStatus();
                }
                
                EditorGUILayout.EndVertical();
            }
        }
        
        /// <summary>
        /// Zeichnet den Footer mit zusätzlichen Informationen
        /// </summary>
        private void DrawFooter()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.LabelField("Zusätzliche Informationen", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            
            EditorGUILayout.LabelField("Nach dem Setup:");
            EditorGUILayout.LabelField("1. Build und Run über File > Build Settings");
            EditorGUILayout.LabelField("2. CSV-Datei in persistentDataPath ablegen");
            EditorGUILayout.LabelField("3. QR-Codes für Ventile mit dem Generator erstellen");
            
            EditorGUILayout.Space(10);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Build Settings öffnen"))
            {
                EditorApplication.ExecuteMenuItem("File/Build Settings...");
            }
            
            if (GUILayout.Button("Project Settings öffnen"))
            {
                SettingsService.OpenProjectSettings();
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }
        
        #endregion
        
        #region Helper-Methoden für GUI
        
        /// <summary>
        /// Zeichnet einen Sektions-Header mit Foldout
        /// </summary>
        private bool DrawSectionHeader(string title, SetupStatus status, bool isExpanded)
        {
            EditorGUILayout.Space(5);
            
            EditorGUILayout.BeginHorizontal();
            
            // Status-Icon
            string statusIcon = GetStatusIcon(status);
            EditorGUILayout.LabelField(statusIcon, GUILayout.Width(25));
            
            // Foldout
            isExpanded = EditorGUILayout.Foldout(isExpanded, title, true, EditorStyles.foldoutHeader);
            
            EditorGUILayout.EndHorizontal();
            
            return isExpanded;
        }
        
        /// <summary>
        /// Zeichnet eine Einstellungs-Zeile
        /// </summary>
        private void DrawSettingRow(string label, string value, bool isCorrect)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(GetStatusIcon(isCorrect), GUILayout.Width(20));
            EditorGUILayout.LabelField(label, GUILayout.Width(150));
            EditorGUILayout.LabelField(value);
            EditorGUILayout.EndHorizontal();
        }
        
        /// <summary>
        /// Gibt das Status-Icon für einen Boolean zurück
        /// </summary>
        private string GetStatusIcon(bool isOK)
        {
            return isOK ? "✓" : "✗";
        }
        
        /// <summary>
        /// Gibt das Status-Icon für einen SetupStatus zurück
        /// </summary>
        private string GetStatusIcon(SetupStatus status)
        {
            switch (status)
            {
                case SetupStatus.OK:
                    return "✓";
                case SetupStatus.NeedsConfiguration:
                    return "⚠";
                case SetupStatus.Error:
                    return "✗";
                default:
                    return "?";
            }
        }
        
        #endregion
        
        #region Status-Prüfungen
        
        /// <summary>
        /// Prüft alle Status-Werte
        /// </summary>
        private void CheckAllStatus()
        {
            CheckPackageStatus();
            CheckBuildStatus();
            CheckPlayerStatus();
            CheckXRStatus();
            CheckQualityStatus();
            CheckManifestStatus();
            CheckSceneStatus();
            
            Debug.Log("[D8-Planer Setup] Status-Prüfung abgeschlossen");
        }
        
        private void CheckPackageStatus()
        {
            bool allInstalled = true;
            foreach (var package in REQUIRED_PACKAGES)
            {
                if (!IsPackageInstalled(package))
                {
                    allInstalled = false;
                    break;
                }
            }
            packageStatus = allInstalled ? SetupStatus.OK : SetupStatus.NeedsConfiguration;
        }
        
        private void CheckBuildStatus()
        {
            bool isAndroid = EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android;
            bool isASTC = EditorUserBuildSettings.androidBuildSubtarget == MobileTextureSubtarget.ASTC;
            
            buildStatus = (isAndroid && isASTC) ? SetupStatus.OK : SetupStatus.NeedsConfiguration;
        }
        
        private void CheckPlayerStatus()
        {
            bool companyOK = PlayerSettings.companyName == COMPANY_NAME;
            bool productOK = PlayerSettings.productName == PRODUCT_NAME;
            bool packageOK = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android) == PACKAGE_NAME;
            bool versionOK = PlayerSettings.bundleVersion == VERSION;
            bool minApiOK = (int)PlayerSettings.Android.minSdkVersion == MIN_API_LEVEL;
            bool targetApiOK = (int)PlayerSettings.Android.targetSdkVersion == TARGET_API_LEVEL;
            bool backendOK = PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android) == ScriptingImplementation.IL2CPP;
            
            bool archOK = (PlayerSettings.Android.targetArchitectures & AndroidArchitecture.ARM64) != 0 &&
                          (PlayerSettings.Android.targetArchitectures & AndroidArchitecture.ARMv7) != 0;
            
            playerStatus = (companyOK && productOK && packageOK && versionOK && 
                           minApiOK && targetApiOK && backendOK && archOK) 
                           ? SetupStatus.OK : SetupStatus.NeedsConfiguration;
        }
        
        private void CheckXRStatus()
        {
            // XR-Status-Prüfung
            // Hinweis: Vollständige XR-Prüfung erfordert XRGeneralSettings, 
            // die zur Compile-Zeit nicht immer verfügbar sind.
            // Diese Funktion gibt bewusst NeedsConfiguration zurück,
            // da XR Plugin Management manuell in Project Settings aktiviert werden muss.
            xrStatus = SetupStatus.NeedsConfiguration;
        }
        
        private void CheckQualityStatus()
        {
            bool aaOK = QualitySettings.antiAliasing == 2;
            bool vsyncOK = QualitySettings.vSyncCount == 1;
            bool shadowOK = QualitySettings.shadows == ShadowQuality.HardOnly;
            
            qualityStatus = (aaOK && vsyncOK && shadowOK) ? SetupStatus.OK : SetupStatus.NeedsConfiguration;
        }
        
        private void CheckManifestStatus()
        {
            string manifestPath = "Assets/Plugins/Android/AndroidManifest.xml";
            manifestStatus = File.Exists(manifestPath) ? SetupStatus.OK : SetupStatus.NeedsConfiguration;
        }
        
        private void CheckSceneStatus()
        {
            sceneStatus = File.Exists(MAIN_SCENE_PATH) ? SetupStatus.OK : SetupStatus.NeedsConfiguration;
        }
        
        /// <summary>
        /// Prüft ob ein Package installiert ist
        /// Cache für effiziente Package-Prüfungen
        /// </summary>
        private static string cachedManifestContent = null;
        private static double manifestCacheTime = 0;
        
        private bool IsPackageInstalled(string packageName)
        {
            string manifestPath = "Packages/manifest.json";
            
            // Cache die manifest.json für Effizienz
            double currentTime = EditorApplication.timeSinceStartup;
            if (cachedManifestContent == null || currentTime - manifestCacheTime > MANIFEST_CACHE_TIMEOUT_SECONDS)
            {
                if (File.Exists(manifestPath))
                {
                    cachedManifestContent = File.ReadAllText(manifestPath);
                    manifestCacheTime = currentTime;
                }
                else
                {
                    cachedManifestContent = "";
                }
            }
            
            // Prüfe auf exaktes Package-Format: "packageName": "version"
            // Dies verhindert falsche Matches bei ähnlichen Package-Namen
            string searchPattern = $"\"{packageName}\":";
            return cachedManifestContent.Contains(searchPattern);
        }
        
        /// <summary>
        /// Leert den Manifest-Cache (für Tests oder bei Änderungen)
        /// </summary>
        private static void ClearManifestCache()
        {
            cachedManifestContent = null;
            manifestCacheTime = 0;
        }
        
        #endregion
        
        #region Konfigurationsmethoden
        
        /// <summary>
        /// Wendet alle Einstellungen an
        /// </summary>
        public static void ApplyAllSettings()
        {
            Debug.Log("[D8-Planer Setup] Beginne vollständige Konfiguration...");
            
            ConfigureBuildSettings();
            ConfigurePlayerSettings();
            ConfigureXRSettings();
            ConfigureQualitySettings();
            CreateAndroidManifest();
            
            AssetDatabase.Refresh();
            
            Debug.Log("[D8-Planer Setup] ✓ Alle Einstellungen wurden angewendet!");
            EditorUtility.DisplayDialog(
                "D8-Planer Setup",
                "Alle Einstellungen wurden erfolgreich angewendet!\n\n" +
                "Nächster Schritt: Erstelle die Hauptszene über\n" +
                "D8-Planer > Hauptszene erstellen",
                "OK");
        }
        
        /// <summary>
        /// Konfiguriert die Build Settings
        /// </summary>
        public static void ConfigureBuildSettings()
        {
            Debug.Log("[D8-Planer Setup] Konfiguriere Build Settings...");
            
            // Auf Android wechseln
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
                Debug.Log("[D8-Planer Setup] ✓ Build Target auf Android gewechselt");
            }
            
            // Texture Compression: ASTC
            EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.ASTC;
            Debug.Log("[D8-Planer Setup] ✓ Texture Compression auf ASTC gesetzt");
            
            // Build System: Gradle (Standard in neueren Unity-Versionen)
            EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
            Debug.Log("[D8-Planer Setup] ✓ Build System auf Gradle gesetzt");
            
            // Development Build deaktivieren für Release
            EditorUserBuildSettings.development = false;
        }
        
        /// <summary>
        /// Konfiguriert die Player Settings
        /// </summary>
        public static void ConfigurePlayerSettings()
        {
            Debug.Log("[D8-Planer Setup] Konfiguriere Player Settings...");
            
            // Allgemeine Einstellungen
            PlayerSettings.companyName = COMPANY_NAME;
            PlayerSettings.productName = PRODUCT_NAME;
            PlayerSettings.bundleVersion = VERSION;
            Debug.Log($"[D8-Planer Setup] ✓ Projekt-Info: {COMPANY_NAME} - {PRODUCT_NAME} v{VERSION}");
            
            // Android-spezifische Einstellungen
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, PACKAGE_NAME);
            Debug.Log($"[D8-Planer Setup] ✓ Package Name: {PACKAGE_NAME}");
            
            // API Levels
            PlayerSettings.Android.minSdkVersion = (AndroidSdkVersions)MIN_API_LEVEL;
            PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)TARGET_API_LEVEL;
            Debug.Log($"[D8-Planer Setup] ✓ API Levels: Min {MIN_API_LEVEL}, Target {TARGET_API_LEVEL}");
            
            // Scripting Backend: IL2CPP
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            Debug.Log("[D8-Planer Setup] ✓ Scripting Backend auf IL2CPP gesetzt");
            
            // Target Architectures: ARM64 + ARMv7
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;
            Debug.Log("[D8-Planer Setup] ✓ Target Architectures: ARM64 + ARMv7");
            
            // Graphics APIs: OpenGLES3, Vulkan
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new UnityEngine.Rendering.GraphicsDeviceType[]
            {
                UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3,
                UnityEngine.Rendering.GraphicsDeviceType.Vulkan
            });
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
            Debug.Log("[D8-Planer Setup] ✓ Graphics APIs: OpenGLES3, Vulkan");
            
            // Orientierung: Landscape
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            Debug.Log("[D8-Planer Setup] ✓ Orientierung: Landscape");
            
            // ARCore Unterstützung
            PlayerSettings.Android.ARCoreEnabled = true;
            Debug.Log("[D8-Planer Setup] ✓ ARCore aktiviert");
        }
        
        /// <summary>
        /// Konfiguriert die XR Settings
        /// </summary>
        public static void ConfigureXRSettings()
        {
            Debug.Log("[D8-Planer Setup] Konfiguriere XR Settings...");
            
            // XR Plugin Management konfigurieren
            // Hinweis: Die vollständige XR-Konfiguration erfordert XRGeneralSettings
            // und XRPackageMetadataStore, die zur Laufzeit nicht immer verfügbar sind
            
            Debug.Log("[D8-Planer Setup] ⚠ XR Plugin Management muss manuell aktiviert werden:");
            Debug.Log("  1. Öffne Edit > Project Settings > XR Plug-in Management");
            Debug.Log("  2. Wähle Android Tab");
            Debug.Log("  3. Aktiviere 'ARCore'");
            Debug.Log("  4. Aktiviere 'Initialize XR on Startup'");
        }
        
        /// <summary>
        /// Konfiguriert die Quality Settings für Mobile
        /// </summary>
        public static void ConfigureQualitySettings()
        {
            Debug.Log("[D8-Planer Setup] Konfiguriere Quality Settings für Mobile...");
            
            // Anti-Aliasing: 2x MSAA (guter Kompromiss zwischen Qualität und Performance)
            QualitySettings.antiAliasing = 2;
            Debug.Log("[D8-Planer Setup] ✓ Anti-Aliasing: 2x MSAA");
            
            // VSync: An (reduziert Tearing)
            QualitySettings.vSyncCount = 1;
            Debug.Log("[D8-Planer Setup] ✓ VSync: Aktiviert");
            
            // Shadows: Hard Only (spart Performance)
            QualitySettings.shadows = ShadowQuality.HardOnly;
            QualitySettings.shadowResolution = ShadowResolution.Low;
            QualitySettings.shadowDistance = 15f;
            Debug.Log("[D8-Planer Setup] ✓ Shadows: Hard Only, Low Resolution");
            
            // Texture Quality: Full Res
            QualitySettings.globalTextureMipmapLimit = 0;
            Debug.Log("[D8-Planer Setup] ✓ Texture Quality: Full Resolution");
            
            // Anisotropic Textures: Global aktiviert (guter Kompromiss für Qualität)
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;
            Debug.Log("[D8-Planer Setup] ✓ Anisotropic Filtering: Aktiviert (Global)");
            
            // Soft Particles: Aus (spart Performance)
            QualitySettings.softParticles = false;
            Debug.Log("[D8-Planer Setup] ✓ Soft Particles: Deaktiviert");
            
            // LOD Bias: Standard
            QualitySettings.lodBias = 1.0f;
            Debug.Log("[D8-Planer Setup] ✓ LOD Bias: 1.0");
            
            // Pixel Light Count: Reduziert für Mobile
            QualitySettings.pixelLightCount = 2;
            Debug.Log("[D8-Planer Setup] ✓ Pixel Light Count: 2");
        }
        
        /// <summary>
        /// Erstellt die AndroidManifest.xml mit allen erforderlichen Berechtigungen
        /// </summary>
        public static void CreateAndroidManifest()
        {
            Debug.Log("[D8-Planer Setup] Erstelle AndroidManifest.xml...");
            
            string manifestDir = "Assets/Plugins/Android";
            string manifestPath = Path.Combine(manifestDir, "AndroidManifest.xml");
            
            // Verzeichnis erstellen falls nicht vorhanden
            if (!Directory.Exists(manifestDir))
            {
                Directory.CreateDirectory(manifestDir);
                Debug.Log($"[D8-Planer Setup] ✓ Verzeichnis erstellt: {manifestDir}");
            }
            
            // AndroidManifest.xml Inhalt mit interpolierten Konstanten
            string manifestContent = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<manifest xmlns:android=""http://schemas.android.com/apk/res/android""
    package=""{PACKAGE_NAME}""
    android:versionCode=""1""
    android:versionName=""{VERSION}"">

    <!-- Erforderliche Berechtigungen für D8-Planer XR -->
    
    <!-- Kamera für AR und QR-Code Erkennung -->
    <uses-permission android:name=""android.permission.CAMERA"" />
    <uses-feature android:name=""android.hardware.camera"" android:required=""true"" />
    <uses-feature android:name=""android.hardware.camera.autofocus"" android:required=""false"" />
    
    <!-- ARCore Unterstützung -->
    <uses-feature android:name=""android.hardware.camera.ar"" android:required=""true"" />
    
    <!-- Speicherzugriff für CSV-Dateien -->
    <uses-permission android:name=""android.permission.READ_EXTERNAL_STORAGE"" />
    <uses-permission android:name=""android.permission.WRITE_EXTERNAL_STORAGE"" />
    
    <!-- Für Android 10+ Scoped Storage -->
    <uses-permission android:name=""android.permission.MANAGE_EXTERNAL_STORAGE"" />
    
    <!-- Internet (optional, für Updates) -->
    <uses-permission android:name=""android.permission.INTERNET"" />
    <uses-permission android:name=""android.permission.ACCESS_NETWORK_STATE"" />
    
    <!-- VR/XR Hardware Features -->
    <uses-feature android:name=""android.hardware.sensor.accelerometer"" android:required=""true"" />
    <uses-feature android:name=""android.hardware.sensor.gyroscope"" android:required=""true"" />
    
    <!-- OpenGL ES 3.0 für AR Rendering -->
    <uses-feature android:glEsVersion=""0x00030000"" android:required=""true"" />

    <application
        android:allowBackup=""true""
        android:icon=""@mipmap/app_icon""
        android:label=""@string/app_name""
        android:theme=""@style/UnityThemeSelector""
        android:hardwareAccelerated=""true""
        android:requestLegacyExternalStorage=""true"">
        
        <!-- ARCore Metadaten -->
        <meta-data android:name=""com.google.ar.core"" android:value=""required"" />
        
        <!-- Unity Player Activity -->
        <activity
            android:name=""com.unity3d.player.UnityPlayerActivity""
            android:configChanges=""mcc|mnc|locale|touchscreen|keyboard|keyboardHidden|navigation|orientation|screenLayout|uiMode|screenSize|smallestScreenSize|fontScale|layoutDirection|density""
            android:hardwareAccelerated=""true""
            android:launchMode=""singleTask""
            android:screenOrientation=""landscape""
            android:theme=""@style/UnityThemeSelector""
            android:exported=""true"">
            <intent-filter>
                <action android:name=""android.intent.action.MAIN"" />
                <category android:name=""android.intent.category.LAUNCHER"" />
            </intent-filter>
            
            <!-- ARCore Intent Filter -->
            <intent-filter>
                <action android:name=""android.intent.action.VIEW"" />
                <category android:name=""android.intent.category.DEFAULT"" />
            </intent-filter>
        </activity>
        
        <!-- Viture SDK Service (falls benötigt) -->
        <!-- 
        <service
            android:name=""com.viture.sdk.VitureService""
            android:enabled=""true""
            android:exported=""false"" />
        -->
        
    </application>
    
</manifest>";
            
            File.WriteAllText(manifestPath, manifestContent);
            Debug.Log($"[D8-Planer Setup] ✓ AndroidManifest.xml erstellt: {manifestPath}");
        }
        
        /// <summary>
        /// Versucht einen Typ aus mehreren möglichen Assemblies zu laden
        /// </summary>
        private static System.Type TryGetType(string typeName, string[] assemblies)
        {
            // Zuerst versuchen ohne Assembly-Angabe
            var type = System.Type.GetType(typeName);
            if (type != null) return type;
            
            // Dann mit verschiedenen Assembly-Namen
            foreach (var assembly in assemblies)
            {
                type = System.Type.GetType($"{typeName}, {assembly}");
                if (type != null) return type;
            }
            
            return null;
        }
        
        /// <summary>
        /// Versucht eine Komponente sicher zu einem GameObject hinzuzufügen
        /// </summary>
        private static bool TryAddComponent<T>(GameObject obj, string componentName) where T : Component
        {
            try
            {
                obj.AddComponent<T>();
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[D8-Planer Setup] ⚠ {componentName} konnte nicht hinzugefügt werden: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Erstellt die Hauptszene mit allen erforderlichen GameObjects
        /// </summary>
        public static void CreateMainScene()
        {
            Debug.Log("[D8-Planer Setup] Erstelle Hauptszene...");
            
            // Szenen-Verzeichnis erstellen
            if (!Directory.Exists(SCENES_FOLDER))
            {
                Directory.CreateDirectory(SCENES_FOLDER);
                Debug.Log($"[D8-Planer Setup] ✓ Verzeichnis erstellt: {SCENES_FOLDER}");
            }
            
            // Neue Szene erstellen
            var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(
                UnityEditor.SceneManagement.NewSceneSetup.EmptyScene,
                UnityEditor.SceneManagement.NewSceneMode.Single);
            
            bool arFoundationAvailable = true;
            bool xrCoreUtilsAvailable = true;
            
            // 1. AR Session erstellen (mit Fehlerbehandlung)
            GameObject arSessionObj = new GameObject("AR Session");
            try
            {
                arSessionObj.AddComponent<UnityEngine.XR.ARFoundation.ARSession>();
                arSessionObj.AddComponent<UnityEngine.XR.ARFoundation.ARInputManager>();
                Debug.Log("[D8-Planer Setup] ✓ AR Session erstellt");
            }
            catch (System.Exception ex)
            {
                arFoundationAvailable = false;
                Debug.LogWarning($"[D8-Planer Setup] ⚠ AR Session konnte nicht erstellt werden. " +
                    $"Stelle sicher, dass AR Foundation installiert ist. Fehler: {ex.Message}");
            }
            
            // 2. XR Origin erstellen (mit Fehlerbehandlung)
            GameObject xrOriginObj = new GameObject("XR Origin");
            GameObject cameraOffset = new GameObject("Camera Offset");
            cameraOffset.transform.SetParent(xrOriginObj.transform);
            
            // AR Camera erstellen
            GameObject arCameraObj = new GameObject("AR Camera");
            arCameraObj.transform.SetParent(cameraOffset.transform);
            arCameraObj.tag = "MainCamera";
            
            Camera arCamera = arCameraObj.AddComponent<Camera>();
            arCamera.clearFlags = CameraClearFlags.SolidColor;
            arCamera.backgroundColor = Color.black;
            arCamera.nearClipPlane = 0.1f;
            arCamera.farClipPlane = 100f;
            
            try
            {
                var xrOrigin = xrOriginObj.AddComponent<Unity.XR.CoreUtils.XROrigin>();
                xrOrigin.CameraFloorOffsetObject = cameraOffset;
                xrOrigin.Camera = arCamera;
                Debug.Log("[D8-Planer Setup] ✓ XR Origin erstellt");
            }
            catch (System.Exception ex)
            {
                xrCoreUtilsAvailable = false;
                Debug.LogWarning($"[D8-Planer Setup] ⚠ XR Origin konnte nicht erstellt werden. " +
                    $"Stelle sicher, dass XR Core Utilities installiert ist. Fehler: {ex.Message}");
            }
            
            // AR Camera Manager Komponenten (mit Fehlerbehandlung)
            if (arFoundationAvailable)
            {
                try
                {
                    arCameraObj.AddComponent<UnityEngine.XR.ARFoundation.ARCameraManager>();
                    arCameraObj.AddComponent<UnityEngine.XR.ARFoundation.ARCameraBackground>();
                    arCameraObj.AddComponent<UnityEngine.XR.ARFoundation.TrackedPoseDriver>();
                    Debug.Log("[D8-Planer Setup] ✓ AR Camera Komponenten hinzugefügt");
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[D8-Planer Setup] ⚠ AR Camera Komponenten konnten nicht hinzugefügt werden: {ex.Message}");
                }
            }
            
            // 3. AR Plane Manager (optional, für erweiterte Features, mit Fehlerbehandlung)
            if (arFoundationAvailable)
            {
                try
                {
                    xrOriginObj.AddComponent<UnityEngine.XR.ARFoundation.ARPlaneManager>();
                    xrOriginObj.AddComponent<UnityEngine.XR.ARFoundation.ARRaycastManager>();
                    xrOriginObj.AddComponent<UnityEngine.XR.ARFoundation.ARAnchorManager>();
                    Debug.Log("[D8-Planer Setup] ✓ AR Manager Komponenten hinzugefügt");
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[D8-Planer Setup] ⚠ AR Manager Komponenten konnten nicht hinzugefügt werden: {ex.Message}");
                }
            }
            
            // 4. App Controller erstellen (mit flexibler Typsuche)
            GameObject appControllerObj = new GameObject("AppController");
            System.Type appControllerType = TryGetType(APP_CONTROLLER_TYPENAME, APP_CONTROLLER_ASSEMBLIES);
            if (appControllerType != null)
            {
                appControllerObj.AddComponent(appControllerType);
                Debug.Log("[D8-Planer Setup] ✓ AppController erstellt");
            }
            else
            {
                Debug.LogWarning("[D8-Planer Setup] ⚠ AppController-Typ nicht gefunden. " +
                    "Füge die Komponente manuell hinzu nach dem Kompilieren.");
            }
            
            // 5. Device Mode Manager erstellen (mit flexibler Typsuche)
            GameObject deviceModeObj = new GameObject("DeviceModeManager");
            System.Type deviceModeType = TryGetType(DEVICE_MODE_MANAGER_TYPENAME, DEVICE_MODE_MANAGER_ASSEMBLIES);
            if (deviceModeType != null)
            {
                deviceModeObj.AddComponent(deviceModeType);
                Debug.Log("[D8-Planer Setup] ✓ DeviceModeManager erstellt");
            }
            else
            {
                Debug.LogWarning("[D8-Planer Setup] ⚠ DeviceModeManager-Typ nicht gefunden. " +
                    "Füge die Komponente manuell hinzu nach dem Kompilieren.");
            }
            
            // 6. Directional Light erstellen (Hard Shadows für Mobile-Performance)
            GameObject lightObj = new GameObject("Directional Light");
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1f;
            light.shadows = LightShadows.Hard; // Hard Shadows für bessere Mobile-Performance
            lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);
            Debug.Log("[D8-Planer Setup] ✓ Directional Light erstellt (Hard Shadows)");
            
            // 7. UI Canvas erstellen
            GameObject canvasObj = new GameObject("UI Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            
            // Event System erstellen
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            Debug.Log("[D8-Planer Setup] ✓ UI Canvas und EventSystem erstellt");
            
            // 8. Virtual Deckzentrum Container erstellen (mit flexibler Typsuche)
            GameObject deckzentrumContainer = new GameObject("VirtualDeckzentrum Container");
            System.Type virtualDeckzentrumType = TryGetType(VIRTUAL_DECKZENTRUM_TYPENAME, VIRTUAL_DECKZENTRUM_ASSEMBLIES);
            if (virtualDeckzentrumType != null)
            {
                deckzentrumContainer.AddComponent(virtualDeckzentrumType);
                Debug.Log("[D8-Planer Setup] ✓ VirtualDeckzentrum Container erstellt");
            }
            else
            {
                Debug.LogWarning("[D8-Planer Setup] ⚠ VirtualDeckzentrum-Typ nicht gefunden. " +
                    "Füge die Komponente manuell hinzu nach dem Kompilieren.");
            }
            
            // Szene speichern
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, MAIN_SCENE_PATH);
            Debug.Log($"[D8-Planer Setup] ✓ Szene gespeichert: {MAIN_SCENE_PATH}");
            
            // Szene zu Build Settings hinzufügen
            AddSceneToBuildSettings(MAIN_SCENE_PATH);
            
            AssetDatabase.Refresh();
            
            // Warnungen in der Bestätigungsmeldung anzeigen
            string warnings = "";
            if (!arFoundationAvailable)
                warnings += "\n⚠ AR Foundation nicht verfügbar - manuell hinzufügen!";
            if (!xrCoreUtilsAvailable)
                warnings += "\n⚠ XR Core Utilities nicht verfügbar - manuell hinzufügen!";
            
            Debug.Log("[D8-Planer Setup] ✓ Hauptszene erfolgreich erstellt!");
            
            EditorUtility.DisplayDialog(
                "Hauptszene erstellt",
                $"Die Hauptszene wurde erfolgreich erstellt:\n{MAIN_SCENE_PATH}\n\n" +
                "Die Szene enthält:\n" +
                "• AR Session\n" +
                "• XR Origin mit AR Camera\n" +
                "• AppController\n" +
                "• DeviceModeManager\n" +
                "• UI Canvas\n" +
                "• VirtualDeckzentrum Container" + warnings,
                "OK");
        }
        
        /// <summary>
        /// Fügt eine Szene zu den Build Settings hinzu
        /// </summary>
        private static void AddSceneToBuildSettings(string scenePath)
        {
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            
            // Prüfen ob Szene bereits existiert
            foreach (var scene in scenes)
            {
                if (scene.path == scenePath)
                {
                    Debug.Log($"[D8-Planer Setup] Szene bereits in Build Settings: {scenePath}");
                    return;
                }
            }
            
            // Szene hinzufügen
            scenes.Insert(0, new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            Debug.Log($"[D8-Planer Setup] ✓ Szene zu Build Settings hinzugefügt: {scenePath}");
        }
        
        #endregion
    }
}
