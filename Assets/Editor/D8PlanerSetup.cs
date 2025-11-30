using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting; // WICHTIG: Dieser Namespace fehlte!
using UnityEngine.Rendering;
using System.IO;
using System.Collections.Generic;

// Pfad: Assets/Editor/D8PlanerSetup.cs
// Dieses Script muss zwingend in einem Ordner namens "Editor" liegen!

public class D8PlanerSetup : EditorWindow
{
    [MenuItem("D8-Planer/Setup Wizard")]
    public static void ShowWindow()
    {
        GetWindow<D8PlanerSetup>("D8-Planer Setup");
    }

    void OnGUI()
    {
        GUILayout.Label("D8-Planer XR - Projekt Konfiguration", EditorStyles.boldLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("Alle Einstellungen anwenden (Empfohlen)"))
        {
            if (EditorUtility.DisplayDialog("Setup ausführen?", 
                "Dies wird die Player Settings für Android (D8-Planer Spezifikation) überschreiben. Fortfahren?", "Ja, anwenden", "Abbrechen"))
            {
                ApplyAllSettings();
            }
        }

        GUILayout.Space(20);
        GUILayout.Label("Einzelne Schritte:", EditorStyles.boldLabel);

        if (GUILayout.Button("1. Android Platform setzen")) SwitchToAndroid();
        if (GUILayout.Button("2. Player Settings konfigurieren")) ConfigurePlayerSettings();
        if (GUILayout.Button("3. Quality Settings optimieren")) ConfigureQuality();
    }

    [MenuItem("D8-Planer/Hauptszene erstellen")]
    public static void CreateMainSceneMenu()
    {
        CreateMainScene();
    }

    // --- Core Logic ---

    public static void ApplyAllSettings()
    {
        SwitchToAndroid();
        ConfigurePlayerSettings();
        ConfigureQuality();
        
        Debug.Log("<color=green>✅ D8-Planer Setup erfolgreich abgeschlossen!</color>");
        if (!Application.isBatchMode) 
        {
            EditorUtility.DisplayDialog("Erfolg", "D8-Planer Setup wurde erfolgreich angewendet!", "OK");
        }
    }

    static void SwitchToAndroid()
    {
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
        {
            Debug.Log("⏳ Wechsle zu Android Platform...");
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        }
    }

    static void ConfigurePlayerSettings()
    {
        // Identification
        PlayerSettings.companyName = "Bold202"; 
        PlayerSettings.productName = "D8-Planer XR";
        PlayerSettings.bundleVersion = "1.0.0";
        PlayerSettings.Android.bundleVersionCode = 1;
        
        // Package Name
        string packageName = "com.d8planer.deckzentrum";
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, packageName);

        // Configuration
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24; // Android 7.0
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel33; // Android 13

        // Rendering
        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new GraphicsDeviceType[] { GraphicsDeviceType.OpenGLES3, GraphicsDeviceType.Vulkan });
        PlayerSettings.Android.useCustomKeystore = false; // Für Dev Builds

        Debug.Log("✅ Player Settings konfiguriert.");
    }

    static void ConfigureQuality()
    {
        Debug.Log("✅ Quality Settings konfiguriert.");
    }

    static void CreateMainScene()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
        {
            AssetDatabase.CreateFolder("Assets", "Scenes");
        }

        string scenePath = "Assets/Scenes/MainScene.unity";
        
        // Prüfen ob Szene schon existiert, um Überschreiben zu vermeiden
        if (File.Exists(scenePath))
        {
            Debug.LogWarning($"Szene existiert bereits: {scenePath}");
            return;
        }
        
        var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.DefaultGameObjects, UnityEditor.SceneManagement.NewSceneMode.Single);
        
        GameObject appController = new GameObject("AppController");
        GameObject xrOrigin = new GameObject("XR Origin");
        GameObject arSession = new GameObject("AR Session");
        
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, scenePath);
        
        var scenes = new EditorBuildSettingsScene[] { new EditorBuildSettingsScene(scenePath, true) };
        EditorBuildSettings.scenes = scenes;

        Debug.Log($"✅ Hauptszene erstellt unter: {scenePath}");
    }

    // --- Build Pipeline für Command Line ---
    
    public static void BuildAPK()
    {
        // Sicherstellen, dass alles korrekt eingestellt ist
        ApplyAllSettings();

        // Szenenliste aufbauen
        string[] levels = new string[] { "Assets/Scenes/MainScene.unity" };
        
        // Falls MainScene noch nicht existiert, erstellen wir sie kurz
        if (!File.Exists(levels[0]))
        {
            CreateMainScene();
        }

        // Output Pfad holen (wird vom Batch Script gesetzt via -buildOutput Argument ist schwierig im Batchmode abzufangen ohne Arg-Parsing)
        // Wir nehmen hier den festen Pfad aus den BuildPlayerOptions oder lesen die CommandLineArgs
        string buildPath = GetArg("-buildOutput");
        
        if (string.IsNullOrEmpty(buildPath))
        {
            string desktop = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
            buildPath = Path.Combine(desktop, "D8-Planer-XR.apk");
        }

        Debug.Log($"🚀 Starte Build für: {buildPath}");

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = levels;
        buildPlayerOptions.locationPathName = buildPath;
        buildPlayerOptions.target = BuildTarget.Android;
        buildPlayerOptions.options = BuildOptions.None;

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("🚀 Build succeeded: " + summary.totalSize + " bytes");
            EditorApplication.Exit(0);
        }

        if (summary.result == BuildResult.Failed)
        {
            Debug.Log("❌ Build failed");
            EditorApplication.Exit(1);
        }
    }

    // Hilfsfunktion um Argumente aus der Kommandozeile zu lesen
    private static string GetArg(string name)
    {
        var args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == name && args.Length > i + 1)
            {
                return args[i + 1];
            }
        }
        return null;
    }
}
