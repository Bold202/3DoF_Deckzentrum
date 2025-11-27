using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System;
using System.IO;
using System.Linq;

namespace D8PlanerXR.Editor
{
    /// <summary>
    /// Unity Editor Script für Command-Line Builds.
    /// Ermöglicht automatisierte APK-Builds über Batch-Scripts.
    /// 
    /// Verwendung:
    ///   Unity.exe -quit -batchmode -projectPath "PFAD" -executeMethod D8PlanerXR.Editor.CommandLineBuild.BuildAndroid -buildOutput "output.apk"
    /// </summary>
    public static class CommandLineBuild
    {
        #region Konstanten
        
        private const string DEFAULT_OUTPUT_FILENAME = "D8-Planer-XR.apk";
        private const string BUILD_SCENES_FALLBACK = "Assets/Scenes/MainScene.unity";
        
        #endregion
        
        #region Öffentliche Build-Methoden
        
        /// <summary>
        /// Baut das Android-Projekt über Command-Line.
        /// Wird von build_apk.bat / build_apk.sh aufgerufen.
        /// </summary>
        public static void BuildAndroid()
        {
            Debug.Log("[CommandLineBuild] Starte Android Build...");
            
            try
            {
                // Command-Line Argumente parsen
                string outputPath = GetBuildOutputPath();
                
                Debug.Log($"[CommandLineBuild] Output-Pfad: {outputPath}");
                
                // Build-Optionen konfigurieren
                BuildPlayerOptions buildOptions = ConfigureBuildOptions(outputPath);
                
                // Build ausführen
                BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
                BuildSummary summary = report.summary;
                
                // Ergebnis ausgeben
                if (summary.result == BuildResult.Succeeded)
                {
                    Debug.Log($"[CommandLineBuild] ✓ Build erfolgreich!");
                    Debug.Log($"[CommandLineBuild] Ausgabepfad: {summary.outputPath}");
                    Debug.Log($"[CommandLineBuild] Größe: {summary.totalSize / (1024 * 1024)} MB");
                    Debug.Log($"[CommandLineBuild] Dauer: {summary.totalTime.TotalMinutes:F1} Minuten");
                    
                    // Erfolgreichen Exit-Code setzen
                    EditorApplication.Exit(0);
                }
                else
                {
                    Debug.LogError($"[CommandLineBuild] ✗ Build fehlgeschlagen!");
                    Debug.LogError($"[CommandLineBuild] Fehler: {summary.totalErrors}");
                    Debug.LogError($"[CommandLineBuild] Warnungen: {summary.totalWarnings}");
                    
                    // Fehler-Details ausgeben
                    foreach (var step in report.steps)
                    {
                        foreach (var message in step.messages)
                        {
                            if (message.type == LogType.Error || message.type == LogType.Exception)
                            {
                                Debug.LogError($"[CommandLineBuild] {message.content}");
                            }
                        }
                    }
                    
                    // Fehler-Exit-Code setzen
                    EditorApplication.Exit(1);
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                Debug.LogError($"[CommandLineBuild] Unerwarteter Fehler: {ex.Message}");
                EditorApplication.Exit(1);
            }
        }
        
        /// <summary>
        /// Baut das Android-Projekt im Debug-Modus (Development Build).
        /// </summary>
        public static void BuildAndroidDebug()
        {
            Debug.Log("[CommandLineBuild] Starte Android Debug Build...");
            
            try
            {
                string outputPath = GetBuildOutputPath();
                
                // Debug-Suffix hinzufügen
                if (outputPath.EndsWith(".apk", StringComparison.OrdinalIgnoreCase))
                {
                    outputPath = outputPath.Replace(".apk", "-debug.apk");
                }
                
                BuildPlayerOptions buildOptions = ConfigureBuildOptions(outputPath);
                buildOptions.options |= BuildOptions.Development;
                buildOptions.options |= BuildOptions.AllowDebugging;
                
                BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
                
                if (report.summary.result == BuildResult.Succeeded)
                {
                    Debug.Log("[CommandLineBuild] ✓ Debug Build erfolgreich!");
                    EditorApplication.Exit(0);
                }
                else
                {
                    Debug.LogError("[CommandLineBuild] ✗ Debug Build fehlgeschlagen!");
                    EditorApplication.Exit(1);
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                EditorApplication.Exit(1);
            }
        }
        
        #endregion
        
        #region Editor Menü Integration
        
        /// <summary>
        /// Unity Menü: Baut APK für Release
        /// </summary>
        [MenuItem("D8-Planer/Build APK (Release)", false, 300)]
        public static void MenuBuildRelease()
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmm");
            string outputPath = Path.Combine(desktopPath, $"D8-Planer-XR_{timestamp}.apk");
            
            if (EditorUtility.DisplayDialog(
                "APK Build",
                $"APK wird erstellt:\n{outputPath}\n\nDies kann einige Minuten dauern.",
                "Build starten",
                "Abbrechen"))
            {
                BuildAndroidWithPath(outputPath, false);
            }
        }
        
        /// <summary>
        /// Unity Menü: Baut APK für Debug
        /// </summary>
        [MenuItem("D8-Planer/Build APK (Debug)", false, 301)]
        public static void MenuBuildDebug()
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmm");
            string outputPath = Path.Combine(desktopPath, $"D8-Planer-XR_{timestamp}-debug.apk");
            
            if (EditorUtility.DisplayDialog(
                "APK Debug Build",
                $"Debug APK wird erstellt:\n{outputPath}\n\nDies kann einige Minuten dauern.",
                "Build starten",
                "Abbrechen"))
            {
                BuildAndroidWithPath(outputPath, true);
            }
        }
        
        #endregion
        
        #region Private Hilfsmethoden
        
        /// <summary>
        /// Ermittelt den Output-Pfad aus Command-Line Argumenten.
        /// </summary>
        private static string GetBuildOutputPath()
        {
            string[] args = Environment.GetCommandLineArgs();
            
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-buildOutput" && i + 1 < args.Length)
                {
                    return args[i + 1];
                }
            }
            
            // Fallback: Desktop-Pfad
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            if (string.IsNullOrEmpty(desktopPath))
            {
                desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            }
            
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmm");
            return Path.Combine(desktopPath, $"D8-Planer-XR_{timestamp}.apk");
        }
        
        /// <summary>
        /// Konfiguriert die Build-Optionen für Android.
        /// </summary>
        private static BuildPlayerOptions ConfigureBuildOptions(string outputPath)
        {
            // Szenen für Build sammeln
            string[] scenes = GetBuildScenes();
            
            if (scenes.Length == 0)
            {
                Debug.LogWarning("[CommandLineBuild] Keine Szenen in Build Settings gefunden. Verwende Fallback-Szene.");
                scenes = new string[] { BUILD_SCENES_FALLBACK };
            }
            
            Debug.Log($"[CommandLineBuild] Build-Szenen: {string.Join(", ", scenes)}");
            
            // Output-Verzeichnis erstellen falls nicht vorhanden
            string outputDir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }
            
            // Build-Optionen erstellen
            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outputPath,
                target = BuildTarget.Android,
                options = BuildOptions.CompressWithLz4HC
            };
            
            return options;
        }
        
        /// <summary>
        /// Ermittelt die zu bauenden Szenen aus den Build Settings.
        /// </summary>
        private static string[] GetBuildScenes()
        {
            return EditorBuildSettings.scenes
                .Where(scene => scene.enabled && !string.IsNullOrEmpty(scene.path))
                .Select(scene => scene.path)
                .ToArray();
        }
        
        /// <summary>
        /// Baut Android APK mit angegebenem Pfad (für Editor-Menü).
        /// </summary>
        private static void BuildAndroidWithPath(string outputPath, bool isDebug)
        {
            try
            {
                // Fortschrittsanzeige
                EditorUtility.DisplayProgressBar("APK Build", "Bereite Build vor...", 0.1f);
                
                BuildPlayerOptions buildOptions = ConfigureBuildOptions(outputPath);
                
                if (isDebug)
                {
                    buildOptions.options |= BuildOptions.Development;
                    buildOptions.options |= BuildOptions.AllowDebugging;
                }
                
                EditorUtility.DisplayProgressBar("APK Build", "Build läuft...", 0.3f);
                
                BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
                BuildSummary summary = report.summary;
                
                EditorUtility.ClearProgressBar();
                
                if (summary.result == BuildResult.Succeeded)
                {
                    long sizeMB = summary.totalSize / (1024 * 1024);
                    
                    EditorUtility.DisplayDialog(
                        "Build erfolgreich!",
                        $"APK wurde erstellt:\n{outputPath}\n\n" +
                        $"Größe: {sizeMB} MB\n" +
                        $"Dauer: {summary.totalTime.TotalMinutes:F1} Minuten",
                        "OK");
                    
                    // Optional: Ordner öffnen
                    string directory = Path.GetDirectoryName(outputPath);
                    if (!string.IsNullOrEmpty(directory))
                    {
                        EditorUtility.RevealInFinder(outputPath);
                    }
                }
                else
                {
                    EditorUtility.DisplayDialog(
                        "Build fehlgeschlagen!",
                        $"Der Build ist fehlgeschlagen.\n\n" +
                        $"Fehler: {summary.totalErrors}\n" +
                        $"Warnungen: {summary.totalWarnings}\n\n" +
                        "Prüfe die Console für Details.",
                        "OK");
                }
            }
            catch (Exception ex)
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog(
                    "Build-Fehler",
                    $"Ein unerwarteter Fehler ist aufgetreten:\n\n{ex.Message}",
                    "OK");
                Debug.LogException(ex);
            }
        }
        
        #endregion
    }
}
