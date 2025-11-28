using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.IO;

namespace D8PlanerXR.Editor
{
    /// <summary>
    /// Creates a mobile-focused scene for QR code scanning
    /// This scene uses WebCamTexture instead of AR Foundation for simpler, more reliable camera access
    /// </summary>
    public class MobileSceneCreator : EditorWindow
    {
        private const string MOBILE_SCENE_PATH = "Assets/Scenes/MobileScene.unity";
        private const string SCENES_FOLDER = "Assets/Scenes";
        
        [MenuItem("D8-Planer/Handy-Szene erstellen", false, 300)]
        public static void CreateMobileScene()
        {
            // Confirm with user
            if (File.Exists(MOBILE_SCENE_PATH))
            {
                if (!EditorUtility.DisplayDialog(
                    "Szene überschreiben?",
                    $"Die Handy-Szene existiert bereits:\n{MOBILE_SCENE_PATH}\n\nMöchtest du sie überschreiben?",
                    "Ja, überschreiben",
                    "Abbrechen"))
                {
                    return;
                }
            }
            
            CreateMobileSceneInternal();
        }
        
        private static void CreateMobileSceneInternal()
        {
            Debug.Log("[MobileSceneCreator] Creating mobile scene...");
            
            // Ensure scenes folder exists
            if (!Directory.Exists(SCENES_FOLDER))
            {
                Directory.CreateDirectory(SCENES_FOLDER);
            }
            
            // Create new scene
            var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(
                UnityEditor.SceneManagement.NewSceneSetup.EmptyScene,
                UnityEditor.SceneManagement.NewSceneMode.Single);
            
            // 1. Create Main Camera (no AR, just regular camera for 3D UI)
            GameObject mainCameraObj = new GameObject("Main Camera");
            Camera mainCamera = mainCameraObj.AddComponent<Camera>();
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = new Color(0.1f, 0.1f, 0.15f); // Dark blue-gray
            mainCamera.nearClipPlane = 0.1f;
            mainCamera.farClipPlane = 100f;
            mainCameraObj.tag = "MainCamera";
            mainCameraObj.AddComponent<AudioListener>();
            Debug.Log("[MobileSceneCreator] ✓ Main Camera created");
            
            // 2. Create Directional Light
            GameObject lightObj = new GameObject("Directional Light");
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1f;
            light.shadows = LightShadows.None;
            lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);
            Debug.Log("[MobileSceneCreator] ✓ Directional Light created");
            
            // 3. Create Main Canvas for camera display
            GameObject canvasObj = new GameObject("Main Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            
            canvasObj.AddComponent<GraphicRaycaster>();
            Debug.Log("[MobileSceneCreator] ✓ Main Canvas created");
            
            // 4. Create Camera Display (RawImage for WebCamTexture)
            GameObject cameraDisplayObj = new GameObject("Camera Display");
            cameraDisplayObj.transform.SetParent(canvasObj.transform, false);
            
            RawImage cameraDisplay = cameraDisplayObj.AddComponent<RawImage>();
            cameraDisplay.color = Color.white;
            
            RectTransform cameraDisplayRect = cameraDisplayObj.GetComponent<RectTransform>();
            cameraDisplayRect.anchorMin = Vector2.zero;
            cameraDisplayRect.anchorMax = Vector2.one;
            cameraDisplayRect.offsetMin = Vector2.zero;
            cameraDisplayRect.offsetMax = Vector2.zero;
            
            AspectRatioFitter aspectFitter = cameraDisplayObj.AddComponent<AspectRatioFitter>();
            aspectFitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            aspectFitter.aspectRatio = 16f / 9f;
            Debug.Log("[MobileSceneCreator] ✓ Camera Display created");
            
            // 5. Create Scan Area Indicator (visual feedback)
            GameObject scanAreaObj = new GameObject("Scan Area Indicator");
            scanAreaObj.transform.SetParent(canvasObj.transform, false);
            
            Image scanAreaImage = scanAreaObj.AddComponent<Image>();
            scanAreaImage.color = new Color(0, 1, 0, 0.2f); // Semi-transparent green
            
            RectTransform scanAreaRect = scanAreaObj.GetComponent<RectTransform>();
            scanAreaRect.anchorMin = new Vector2(0.25f, 0.25f);
            scanAreaRect.anchorMax = new Vector2(0.75f, 0.75f);
            scanAreaRect.offsetMin = Vector2.zero;
            scanAreaRect.offsetMax = Vector2.zero;
            
            // Add outline to scan area
            Outline scanOutline = scanAreaObj.AddComponent<Outline>();
            scanOutline.effectColor = new Color(0, 1, 0, 0.8f);
            scanOutline.effectDistance = new Vector2(3, 3);
            Debug.Log("[MobileSceneCreator] ✓ Scan Area Indicator created");
            
            // 6. Create UI Overlay Canvas
            GameObject overlayCanvasObj = new GameObject("UI Overlay Canvas");
            Canvas overlayCanvas = overlayCanvasObj.AddComponent<Canvas>();
            overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            overlayCanvas.sortingOrder = 10;
            
            CanvasScaler overlayScaler = overlayCanvasObj.AddComponent<CanvasScaler>();
            overlayScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            overlayScaler.referenceResolution = new Vector2(1920, 1080);
            overlayScaler.matchWidthOrHeight = 0.5f;
            
            overlayCanvasObj.AddComponent<GraphicRaycaster>();
            Debug.Log("[MobileSceneCreator] ✓ UI Overlay Canvas created");
            
            // 7. Create Header Panel
            GameObject headerPanel = CreatePanel(overlayCanvasObj.transform, "Header Panel", 
                new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -60), new Vector2(0, 0));
            
            Image headerBg = headerPanel.GetComponent<Image>();
            headerBg.color = new Color(0, 0, 0, 0.7f);
            
            // App Title
            GameObject titleObj = new GameObject("App Title");
            titleObj.transform.SetParent(headerPanel.transform, false);
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "D8-Planer XR";
            titleText.fontSize = 32;
            titleText.fontStyle = TMPro.FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = Color.white;
            
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
            Debug.Log("[MobileSceneCreator] ✓ Header Panel created");
            
            // 8. Create Menu Button
            GameObject menuButtonObj = CreateButton(headerPanel.transform, "Menu Button", "☰", 
                new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-40, 0), new Vector2(60, 50));
            Debug.Log("[MobileSceneCreator] ✓ Menu Button created");
            
            // 9. Create Status Panel (bottom)
            GameObject statusPanel = CreatePanel(overlayCanvasObj.transform, "Status Panel",
                new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 80), new Vector2(0, 0));
            
            Image statusBg = statusPanel.GetComponent<Image>();
            statusBg.color = new Color(0, 0, 0, 0.7f);
            
            // Status Text
            GameObject statusTextObj = new GameObject("Status Text");
            statusTextObj.transform.SetParent(statusPanel.transform, false);
            TextMeshProUGUI statusText = statusTextObj.AddComponent<TextMeshProUGUI>();
            statusText.text = "QR-Code scannen...";
            statusText.fontSize = 24;
            statusText.alignment = TextAlignmentOptions.Center;
            statusText.color = Color.white;
            
            RectTransform statusRect = statusTextObj.GetComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(0, 0);
            statusRect.anchorMax = new Vector2(1, 1);
            statusRect.offsetMin = new Vector2(20, 10);
            statusRect.offsetMax = new Vector2(-20, -10);
            Debug.Log("[MobileSceneCreator] ✓ Status Panel created");
            
            // 10. Create Ventil Info Panel (overlay when QR is detected)
            GameObject ventilPanel = CreatePanel(overlayCanvasObj.transform, "Ventil Info Panel",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(400, 300));
            ventilPanel.SetActive(false); // Hidden by default
            
            Image ventilBg = ventilPanel.GetComponent<Image>();
            ventilBg.color = new Color(0.1f, 0.1f, 0.2f, 0.95f);
            
            // Traffic Light Circle
            GameObject trafficLightObj = new GameObject("Traffic Light");
            trafficLightObj.transform.SetParent(ventilPanel.transform, false);
            Image trafficLight = trafficLightObj.AddComponent<Image>();
            trafficLight.color = Color.green;
            
            RectTransform tlRect = trafficLightObj.GetComponent<RectTransform>();
            tlRect.anchorMin = new Vector2(0.5f, 1);
            tlRect.anchorMax = new Vector2(0.5f, 1);
            tlRect.pivot = new Vector2(0.5f, 1);
            tlRect.anchoredPosition = new Vector2(0, -20);
            tlRect.sizeDelta = new Vector2(80, 80);
            
            // Ventil Number Text
            GameObject ventilNumObj = new GameObject("Ventil Number");
            ventilNumObj.transform.SetParent(ventilPanel.transform, false);
            TextMeshProUGUI ventilNumText = ventilNumObj.AddComponent<TextMeshProUGUI>();
            ventilNumText.text = "Ventil 1";
            ventilNumText.fontSize = 36;
            ventilNumText.fontStyle = TMPro.FontStyles.Bold;
            ventilNumText.alignment = TextAlignmentOptions.Center;
            ventilNumText.color = Color.white;
            
            RectTransform vnRect = ventilNumObj.GetComponent<RectTransform>();
            vnRect.anchorMin = new Vector2(0, 1);
            vnRect.anchorMax = new Vector2(1, 1);
            vnRect.pivot = new Vector2(0.5f, 1);
            vnRect.anchoredPosition = new Vector2(0, -110);
            vnRect.sizeDelta = new Vector2(0, 50);
            
            // Sow Info Text
            GameObject sowInfoObj = new GameObject("Sow Info");
            sowInfoObj.transform.SetParent(ventilPanel.transform, false);
            TextMeshProUGUI sowInfoText = sowInfoObj.AddComponent<TextMeshProUGUI>();
            sowInfoText.text = "🟢 602 - 134 Tage\n🟡 450 - 133 Tage";
            sowInfoText.fontSize = 22;
            sowInfoText.alignment = TextAlignmentOptions.TopLeft;
            sowInfoText.color = Color.white;
            
            RectTransform siRect = sowInfoObj.GetComponent<RectTransform>();
            siRect.anchorMin = new Vector2(0, 0);
            siRect.anchorMax = new Vector2(1, 1);
            siRect.offsetMin = new Vector2(20, 20);
            siRect.offsetMax = new Vector2(-20, -170);
            Debug.Log("[MobileSceneCreator] ✓ Ventil Info Panel created");
            
            // 11. Create Menu Panel (side menu)
            GameObject menuPanel = CreatePanel(overlayCanvasObj.transform, "Menu Panel",
                new Vector2(0, 0), new Vector2(0.3f, 1), Vector2.zero, Vector2.zero);
            menuPanel.SetActive(false);
            
            Image menuBg = menuPanel.GetComponent<Image>();
            menuBg.color = new Color(0.1f, 0.1f, 0.15f, 0.98f);
            
            // Menu title
            GameObject menuTitleObj = new GameObject("Menu Title");
            menuTitleObj.transform.SetParent(menuPanel.transform, false);
            TextMeshProUGUI menuTitleText = menuTitleObj.AddComponent<TextMeshProUGUI>();
            menuTitleText.text = "Menü";
            menuTitleText.fontSize = 32;
            menuTitleText.fontStyle = TMPro.FontStyles.Bold;
            menuTitleText.alignment = TextAlignmentOptions.Center;
            menuTitleText.color = Color.white;
            
            RectTransform mtRect = menuTitleObj.GetComponent<RectTransform>();
            mtRect.anchorMin = new Vector2(0, 1);
            mtRect.anchorMax = new Vector2(1, 1);
            mtRect.pivot = new Vector2(0.5f, 1);
            mtRect.anchoredPosition = new Vector2(0, -30);
            mtRect.sizeDelta = new Vector2(0, 50);
            
            // Menu buttons
            CreateMenuButton(menuPanel.transform, "Load CSV Button", "📁 CSV laden", -100);
            CreateMenuButton(menuPanel.transform, "Settings Button", "⚙️ Einstellungen", -170);
            CreateMenuButton(menuPanel.transform, "Info Button", "ℹ️ Information", -240);
            CreateMenuButton(menuPanel.transform, "Close Button", "✖️ Schließen", -310);
            Debug.Log("[MobileSceneCreator] ✓ Menu Panel created");
            
            // 12. Create Debug Panel (optional, for development)
            GameObject debugPanel = CreatePanel(overlayCanvasObj.transform, "Debug Panel",
                new Vector2(0, 0), new Vector2(0, 0), new Vector2(200, 150), new Vector2(0, 0));
            
            Image debugBg = debugPanel.GetComponent<Image>();
            debugBg.color = new Color(0, 0, 0, 0.5f);
            
            GameObject debugTextObj = new GameObject("Debug Text");
            debugTextObj.transform.SetParent(debugPanel.transform, false);
            TextMeshProUGUI debugText = debugTextObj.AddComponent<TextMeshProUGUI>();
            debugText.text = "Debug Info";
            debugText.fontSize = 14;
            debugText.alignment = TextAlignmentOptions.TopLeft;
            debugText.color = Color.white;
            
            RectTransform dbRect = debugTextObj.GetComponent<RectTransform>();
            dbRect.anchorMin = Vector2.zero;
            dbRect.anchorMax = Vector2.one;
            dbRect.offsetMin = new Vector2(10, 10);
            dbRect.offsetMax = new Vector2(-10, -10);
            Debug.Log("[MobileSceneCreator] ✓ Debug Panel created");
            
            // 13. Create EventSystem
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            Debug.Log("[MobileSceneCreator] ✓ EventSystem created");
            
            // 14. Create Managers Container
            GameObject managersObj = new GameObject("Managers");
            
            // Add AppController
            try
            {
                var appControllerType = System.Type.GetType("D8PlanerXR.Core.AppController, Assembly-CSharp");
                if (appControllerType != null)
                {
                    managersObj.AddComponent(appControllerType);
                    Debug.Log("[MobileSceneCreator] ✓ AppController added");
                }
                else
                {
                    Debug.LogWarning("[MobileSceneCreator] AppController type not found - add manually");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[MobileSceneCreator] Could not add AppController: {ex.Message}");
            }
            
            // Add DeviceModeManager
            try
            {
                var deviceModeType = System.Type.GetType("D8PlanerXR.Core.DeviceModeManager, Assembly-CSharp");
                if (deviceModeType != null)
                {
                    managersObj.AddComponent(deviceModeType);
                    Debug.Log("[MobileSceneCreator] ✓ DeviceModeManager added");
                }
                else
                {
                    Debug.LogWarning("[MobileSceneCreator] DeviceModeManager type not found - add manually");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[MobileSceneCreator] Could not add DeviceModeManager: {ex.Message}");
            }
            
            // 15. Create Mobile Camera Controller
            GameObject mobileCameraObj = new GameObject("Mobile Camera Controller");
            try
            {
                var mobileCameraType = System.Type.GetType("D8PlanerXR.Mobile.MobileCameraController, Assembly-CSharp");
                if (mobileCameraType != null)
                {
                    var controller = mobileCameraObj.AddComponent(mobileCameraType);
                    
                    // Try to set serialized fields via reflection
                    SetFieldValue(controller, "cameraDisplay", cameraDisplay);
                    SetFieldValue(controller, "aspectRatioFitter", aspectFitter);
                    SetFieldValue(controller, "ventilInfoPanel", ventilPanel);
                    SetFieldValue(controller, "ventilNumberText", ventilNumText);
                    SetFieldValue(controller, "sowInfoText", sowInfoText);
                    SetFieldValue(controller, "trafficLightImage", trafficLight);
                    SetFieldValue(controller, "statusText", statusText);
                    SetFieldValue(controller, "debugText", debugText);
                    SetFieldValue(controller, "scanAreaIndicator", scanAreaRect);
                    
                    Debug.Log("[MobileSceneCreator] ✓ MobileCameraController added and configured");
                }
                else
                {
                    Debug.LogWarning("[MobileSceneCreator] MobileCameraController type not found - add manually");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[MobileSceneCreator] Could not add MobileCameraController: {ex.Message}");
            }
            
            // 16. Create Mobile Scene Manager
            try
            {
                var mobileSceneType = System.Type.GetType("D8PlanerXR.Mobile.MobileSceneManager, Assembly-CSharp");
                if (mobileSceneType != null)
                {
                    var sceneManager = managersObj.AddComponent(mobileSceneType);
                    
                    // Try to set serialized fields via reflection
                    if (mobileCameraObj.GetComponent<MonoBehaviour>() != null)
                    {
                        SetFieldValue(sceneManager, "cameraController", mobileCameraObj.GetComponent<MonoBehaviour>());
                    }
                    SetFieldValue(sceneManager, "mainCanvas", canvas);
                    SetFieldValue(sceneManager, "cameraDisplay", cameraDisplay);
                    SetFieldValue(sceneManager, "menuPanel", menuPanel);
                    
                    Debug.Log("[MobileSceneCreator] ✓ MobileSceneManager added and configured");
                }
                else
                {
                    Debug.LogWarning("[MobileSceneCreator] MobileSceneManager type not found - add manually");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[MobileSceneCreator] Could not add MobileSceneManager: {ex.Message}");
            }
            
            // Save the scene
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, MOBILE_SCENE_PATH);
            Debug.Log($"[MobileSceneCreator] ✓ Scene saved: {MOBILE_SCENE_PATH}");
            
            // Add to build settings
            AddSceneToBuildSettings(MOBILE_SCENE_PATH);
            
            AssetDatabase.Refresh();
            
            EditorUtility.DisplayDialog(
                "Handy-Szene erstellt",
                $"Die Handy-Szene wurde erfolgreich erstellt:\n{MOBILE_SCENE_PATH}\n\n" +
                "Die Szene enthält:\n" +
                "• Kamera-Anzeige (WebCamTexture)\n" +
                "• QR-Code Scan-Bereich\n" +
                "• Ventil-Info Overlay\n" +
                "• Status-Anzeige\n" +
                "• Menü-System\n\n" +
                "Baue die APK über File > Build Settings.",
                "OK");
        }
        
        /// <summary>
        /// Helper to create a UI panel
        /// </summary>
        private static GameObject CreatePanel(Transform parent, string name, 
            Vector2 anchorMin, Vector2 anchorMax, Vector2 sizeDelta, Vector2 pivot)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            
            Image image = panel.AddComponent<Image>();
            image.color = new Color(0, 0, 0, 0.8f);
            
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            
            if (sizeDelta != Vector2.zero)
            {
                rect.sizeDelta = sizeDelta;
            }
            else
            {
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }
            
            return panel;
        }
        
        /// <summary>
        /// Helper to create a UI button
        /// </summary>
        private static GameObject CreateButton(Transform parent, string name, string text,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            GameObject buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(parent, false);
            
            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.4f, 0.8f);
            
            Button button = buttonObj.AddComponent<Button>();
            
            RectTransform rect = buttonObj.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
            
            // Add text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = text;
            buttonText.fontSize = 28;
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.color = Color.white;
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            return buttonObj;
        }
        
        /// <summary>
        /// Helper to create menu buttons
        /// </summary>
        private static GameObject CreateMenuButton(Transform parent, string name, string text, float yPos)
        {
            GameObject buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(parent, false);
            
            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.15f, 0.15f, 0.2f);
            
            Button button = buttonObj.AddComponent<Button>();
            
            ColorBlock colors = button.colors;
            colors.highlightedColor = new Color(0.2f, 0.4f, 0.8f);
            colors.pressedColor = new Color(0.15f, 0.3f, 0.6f);
            button.colors = colors;
            
            RectTransform rect = buttonObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.1f, 1);
            rect.anchorMax = new Vector2(0.9f, 1);
            rect.pivot = new Vector2(0.5f, 1);
            rect.anchoredPosition = new Vector2(0, yPos);
            rect.sizeDelta = new Vector2(0, 50);
            
            // Add text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = text;
            buttonText.fontSize = 22;
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.color = Color.white;
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            return buttonObj;
        }
        
        /// <summary>
        /// Set field value via reflection
        /// </summary>
        private static void SetFieldValue(object obj, string fieldName, object value)
        {
            try
            {
                var field = obj.GetType().GetField(fieldName, 
                    System.Reflection.BindingFlags.NonPublic | 
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.Public);
                    
                if (field != null)
                {
                    field.SetValue(obj, value);
                }
            }
            catch (System.Exception)
            {
                // Ignore reflection errors
            }
        }
        
        /// <summary>
        /// Add scene to build settings
        /// </summary>
        private static void AddSceneToBuildSettings(string scenePath)
        {
            var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            
            // Check if already exists
            foreach (var scene in scenes)
            {
                if (scene.path == scenePath)
                {
                    Debug.Log($"[MobileSceneCreator] Scene already in build settings: {scenePath}");
                    return;
                }
            }
            
            // Add at the beginning (or after main scene)
            scenes.Insert(scenes.Count > 0 ? 1 : 0, new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            
            Debug.Log($"[MobileSceneCreator] ✓ Scene added to build settings: {scenePath}");
        }
        
        /// <summary>
        /// Import TextMeshPro namespace
        /// </summary>
        private class TextMeshProUGUI : TMPro.TextMeshProUGUI { }
    }
}
