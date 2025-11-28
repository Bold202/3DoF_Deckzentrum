using System;
using System.Collections;
using UnityEngine;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif

namespace D8PlanerXR.Core
{
    /// <summary>
    /// Android Permission Handler
    /// Handles runtime permission requests for camera and storage on Android devices
    /// </summary>
    public class AndroidPermissionHandler : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool requestOnStart = true;
        [SerializeField] private bool showRationaleDialog = true;
        
        [Header("UI References")]
        [SerializeField] private GameObject permissionDialogPanel;
        [SerializeField] private UnityEngine.UI.Button grantPermissionButton;
        [SerializeField] private UnityEngine.UI.Button denyPermissionButton;
        [SerializeField] private TMPro.TextMeshProUGUI permissionMessageText;
        
        // Singleton
        private static AndroidPermissionHandler instance;
        public static AndroidPermissionHandler Instance => instance;
        
        // Permission status
        public bool HasCameraPermission { get; private set; }
        public bool HasStoragePermission { get; private set; }
        public bool AllPermissionsGranted => HasCameraPermission && HasStoragePermission;
        
        // Events
        public event Action OnPermissionsGranted;
        public event Action<string> OnPermissionDenied;
        public event Action OnPermissionCheckComplete;
        
        // Internal state
        private bool isCheckingPermissions = false;
        private Action<bool> currentCallback;
        
        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        private void Start()
        {
            // Setup button listeners
            if (grantPermissionButton != null)
            {
                grantPermissionButton.onClick.AddListener(OnGrantButtonClicked);
            }
            if (denyPermissionButton != null)
            {
                denyPermissionButton.onClick.AddListener(OnDenyButtonClicked);
            }
            
            // Hide dialog initially
            if (permissionDialogPanel != null)
            {
                permissionDialogPanel.SetActive(false);
            }
            
            if (requestOnStart)
            {
                StartCoroutine(CheckAndRequestPermissions());
            }
        }
        
        /// <summary>
        /// Check and request all required permissions
        /// </summary>
        public void RequestAllPermissions(Action<bool> callback = null)
        {
            currentCallback = callback;
            StartCoroutine(CheckAndRequestPermissions());
        }
        
        /// <summary>
        /// Check current permission status without requesting
        /// </summary>
        public void CheckPermissionStatus()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            HasCameraPermission = Permission.HasUserAuthorizedPermission(Permission.Camera);
            HasStoragePermission = Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead);
#else
            // On other platforms, assume permissions are granted
            HasCameraPermission = true;
            HasStoragePermission = true;
#endif
            
            Debug.Log($"[PermissionHandler] Camera: {HasCameraPermission}, Storage: {HasStoragePermission}");
        }
        
        /// <summary>
        /// Coroutine to check and request permissions
        /// </summary>
        private IEnumerator CheckAndRequestPermissions()
        {
            if (isCheckingPermissions)
            {
                yield break;
            }
            
            isCheckingPermissions = true;
            Debug.Log("[PermissionHandler] Starting permission check...");
            
#if UNITY_ANDROID && !UNITY_EDITOR
            // Check camera permission
            if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                Debug.Log("[PermissionHandler] Requesting camera permission...");
                
                if (showRationaleDialog && ShouldShowRationale(Permission.Camera))
                {
                    ShowPermissionRationale("Kamera", "Die App benötigt Zugriff auf die Kamera, um QR-Codes zu scannen.");
                    yield return new WaitUntil(() => permissionDialogPanel == null || !permissionDialogPanel.activeSelf);
                }
                
                bool cameraCallbackReceived = false;
                
                var callbacks = new PermissionCallbacks();
                callbacks.PermissionGranted += (p) => { OnCameraPermissionGranted(p); cameraCallbackReceived = true; };
                callbacks.PermissionDenied += (p) => { OnCameraPermissionDenied(p); cameraCallbackReceived = true; };
                callbacks.PermissionDeniedAndDontAskAgain += (p) => { OnCameraPermissionDeniedDontAsk(p); cameraCallbackReceived = true; };
                
                Permission.RequestUserPermission(Permission.Camera, callbacks);
                
                // Wait for callback with timeout (max 30 seconds)
                float timeout = 30f;
                while (!cameraCallbackReceived && timeout > 0)
                {
                    yield return new WaitForSeconds(0.5f);
                    timeout -= 0.5f;
                }
            }
            
            HasCameraPermission = Permission.HasUserAuthorizedPermission(Permission.Camera);
            
            // Check storage permission (for reading CSV files)
            if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
            {
                Debug.Log("[PermissionHandler] Requesting storage permission...");
                
                if (showRationaleDialog && ShouldShowRationale(Permission.ExternalStorageRead))
                {
                    ShowPermissionRationale("Speicher", "Die App benötigt Zugriff auf den Speicher, um CSV-Dateien zu lesen.");
                    yield return new WaitUntil(() => permissionDialogPanel == null || !permissionDialogPanel.activeSelf);
                }
                
                bool storageCallbackReceived = false;
                
                var callbacks = new PermissionCallbacks();
                callbacks.PermissionGranted += (p) => { OnStoragePermissionGranted(p); storageCallbackReceived = true; };
                callbacks.PermissionDenied += (p) => { OnStoragePermissionDenied(p); storageCallbackReceived = true; };
                callbacks.PermissionDeniedAndDontAskAgain += (p) => { OnStoragePermissionDeniedDontAsk(p); storageCallbackReceived = true; };
                
                Permission.RequestUserPermission(Permission.ExternalStorageRead, callbacks);
                
                // Wait for callback with timeout (max 30 seconds)
                float timeout = 30f;
                while (!storageCallbackReceived && timeout > 0)
                {
                    yield return new WaitForSeconds(0.5f);
                    timeout -= 0.5f;
                }
            }
            
            HasStoragePermission = Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead);
#else
            // On non-Android platforms, permissions are not needed
            HasCameraPermission = true;
            HasStoragePermission = true;
            yield return null;
#endif
            
            isCheckingPermissions = false;
            
            // Log results
            Debug.Log($"[PermissionHandler] Permission check complete:");
            Debug.Log($"  Camera: {HasCameraPermission}");
            Debug.Log($"  Storage: {HasStoragePermission}");
            
            // Invoke callback
            currentCallback?.Invoke(AllPermissionsGranted);
            currentCallback = null;
            
            // Invoke events
            OnPermissionCheckComplete?.Invoke();
            
            if (AllPermissionsGranted)
            {
                OnPermissionsGranted?.Invoke();
            }
        }
        
#if UNITY_ANDROID && !UNITY_EDITOR
        /// <summary>
        /// Check if we should show rationale for a permission
        /// </summary>
        private bool ShouldShowRationale(string permission)
        {
            try
            {
                using (var activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    return activity.Call<bool>("shouldShowRequestPermissionRationale", permission);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[PermissionHandler] Error checking rationale: {e.Message}");
                return false;
            }
        }
#endif
        
        /// <summary>
        /// Show permission rationale dialog
        /// </summary>
        private void ShowPermissionRationale(string permissionName, string message)
        {
            if (permissionDialogPanel == null)
            {
                Debug.Log($"[PermissionHandler] Rationale: {message}");
                return;
            }
            
            if (permissionMessageText != null)
            {
                permissionMessageText.text = $"Berechtigung erforderlich: {permissionName}\n\n{message}";
            }
            
            permissionDialogPanel.SetActive(true);
        }
        
        /// <summary>
        /// Handle grant button click
        /// </summary>
        private void OnGrantButtonClicked()
        {
            if (permissionDialogPanel != null)
            {
                permissionDialogPanel.SetActive(false);
            }
        }
        
        /// <summary>
        /// Handle deny button click
        /// </summary>
        private void OnDenyButtonClicked()
        {
            if (permissionDialogPanel != null)
            {
                permissionDialogPanel.SetActive(false);
            }
            isCheckingPermissions = false;
        }
        
        #region Permission Callbacks
        
        private void OnCameraPermissionGranted(string permission)
        {
            Debug.Log("[PermissionHandler] Camera permission granted");
            HasCameraPermission = true;
        }
        
        private void OnCameraPermissionDenied(string permission)
        {
            Debug.LogWarning("[PermissionHandler] Camera permission denied");
            HasCameraPermission = false;
            OnPermissionDenied?.Invoke("Kamera");
        }
        
        private void OnCameraPermissionDeniedDontAsk(string permission)
        {
            Debug.LogWarning("[PermissionHandler] Camera permission denied (don't ask again)");
            HasCameraPermission = false;
            OnPermissionDenied?.Invoke("Kamera (Dauerhaft verweigert)");
            
            // Optionally, guide user to app settings
            ShowOpenSettingsPrompt("Kamera");
        }
        
        private void OnStoragePermissionGranted(string permission)
        {
            Debug.Log("[PermissionHandler] Storage permission granted");
            HasStoragePermission = true;
        }
        
        private void OnStoragePermissionDenied(string permission)
        {
            Debug.LogWarning("[PermissionHandler] Storage permission denied");
            HasStoragePermission = false;
            OnPermissionDenied?.Invoke("Speicher");
        }
        
        private void OnStoragePermissionDeniedDontAsk(string permission)
        {
            Debug.LogWarning("[PermissionHandler] Storage permission denied (don't ask again)");
            HasStoragePermission = false;
            OnPermissionDenied?.Invoke("Speicher (Dauerhaft verweigert)");
            
            // Optionally, guide user to app settings
            ShowOpenSettingsPrompt("Speicher");
        }
        
        #endregion
        
        /// <summary>
        /// Show prompt to open app settings
        /// </summary>
        private void ShowOpenSettingsPrompt(string permissionName)
        {
            Debug.Log($"[PermissionHandler] {permissionName} permission permanently denied. User should enable in settings.");
            
            // Could show a dialog here prompting user to open app settings
        }
        
        /// <summary>
        /// Open Android app settings
        /// </summary>
        public void OpenAppSettings()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    using (var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                    {
                        using (var intentClass = new AndroidJavaClass("android.content.Intent"))
                        {
                            using (var settingsClass = new AndroidJavaClass("android.provider.Settings"))
                            {
                                string actionAppSettings = settingsClass.GetStatic<string>("ACTION_APPLICATION_DETAILS_SETTINGS");
                                using (var intent = new AndroidJavaObject("android.content.Intent", actionAppSettings))
                                {
                                    string packageName = currentActivity.Call<string>("getPackageName");
                                    using (var uriClass = new AndroidJavaClass("android.net.Uri"))
                                    {
                                        using (var uri = uriClass.CallStatic<AndroidJavaObject>("parse", "package:" + packageName))
                                        {
                                            intent.Call<AndroidJavaObject>("setData", uri);
                                            currentActivity.Call("startActivity", intent);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[PermissionHandler] Error opening app settings: {e.Message}");
            }
#endif
        }
        
        /// <summary>
        /// Request camera permission specifically
        /// </summary>
        public void RequestCameraPermission(Action<bool> callback = null)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                HasCameraPermission = true;
                callback?.Invoke(true);
                return;
            }
            
            var callbacks = new PermissionCallbacks();
            callbacks.PermissionGranted += (p) => {
                HasCameraPermission = true;
                callback?.Invoke(true);
            };
            callbacks.PermissionDenied += (p) => {
                HasCameraPermission = false;
                callback?.Invoke(false);
            };
            callbacks.PermissionDeniedAndDontAskAgain += (p) => {
                HasCameraPermission = false;
                callback?.Invoke(false);
            };
            
            Permission.RequestUserPermission(Permission.Camera, callbacks);
#else
            HasCameraPermission = true;
            callback?.Invoke(true);
#endif
        }
        
        /// <summary>
        /// Request storage permission specifically
        /// </summary>
        public void RequestStoragePermission(Action<bool> callback = null)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
            {
                HasStoragePermission = true;
                callback?.Invoke(true);
                return;
            }
            
            var callbacks = new PermissionCallbacks();
            callbacks.PermissionGranted += (p) => {
                HasStoragePermission = true;
                callback?.Invoke(true);
            };
            callbacks.PermissionDenied += (p) => {
                HasStoragePermission = false;
                callback?.Invoke(false);
            };
            callbacks.PermissionDeniedAndDontAskAgain += (p) => {
                HasStoragePermission = false;
                callback?.Invoke(false);
            };
            
            Permission.RequestUserPermission(Permission.ExternalStorageRead, callbacks);
#else
            HasStoragePermission = true;
            callback?.Invoke(true);
#endif
        }
        
#if UNITY_EDITOR
        [ContextMenu("Check Permissions")]
        private void EditorCheckPermissions()
        {
            CheckPermissionStatus();
            Debug.Log($"Camera: {HasCameraPermission}, Storage: {HasStoragePermission}");
        }
        
        [ContextMenu("Request All Permissions")]
        private void EditorRequestPermissions()
        {
            RequestAllPermissions(result => {
                Debug.Log($"Permission request result: {result}");
            });
        }
#endif
    }
}
