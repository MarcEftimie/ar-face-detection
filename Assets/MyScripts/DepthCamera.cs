using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.MagicLeap;
using TMPro;
using System.Data;

/// <summary>
/// Manages the depth camera in a Unity scene, handling permissions, connectivity, and data retrieval.
/// </summary>
public class DepthCamera : MonoBehaviour
{
    public TextMeshProUGUI debugText; // Text display for debugging and status messages

    // Callbacks for handling ML permissions
    private readonly MLPermissions.Callbacks permissionCallbacks = new();
    private bool permissionGranted; // Flag to track if permission is granted
    private MLDepthCamera.Data lastData = null; // Stores the last retrieved depth data
    private MLDepthCamera.Stream stream = MLDepthCamera.Stream.LongRange; // Set default stream to long range
    private MLDepthCamera.CaptureFlags captureFlag = MLDepthCamera.CaptureFlags.DepthImage; // Capture flag for depth image

    void Awake()
    {
        // Subscribe to permission events
        permissionCallbacks.OnPermissionGranted += OnPermissionGranted;
        permissionCallbacks.OnPermissionDenied += OnPermissionDenied;
        permissionCallbacks.OnPermissionDeniedAndDontAskAgain += OnPermissionDenied;
    }

    void Start()
    {
        // Request depth camera permission at start
        MLPermissions.RequestPermission(MLPermission.DepthCamera, permissionCallbacks);
    }

    /// <summary>
    /// Retrieves the latest depth data if permissions are granted and the camera is connected.
    /// </summary>
    /// <returns>The latest depth data or null if unavailable.</returns>
    public MLDepthCamera.Data GetDepthData()
    {
        // Check for required permissions and camera connection
        if (!permissionGranted || !MLDepthCamera.IsConnected)
        {
            return null;
        }
        
        // Attempt to get the latest depth data
        var result = MLDepthCamera.GetLatestDepthData(700, out MLDepthCamera.Data data);

        if (result.IsOk)
        {
            lastData = data;
        }

        // Return the last valid depth data if available
        if (lastData != null && lastData.DepthImage != null)
        {
            return lastData;
        }

        return null;
    }

    private void OnDestroy()
    {
        // Unsubscribe from permission events
        permissionCallbacks.OnPermissionGranted -= OnPermissionGranted;
        permissionCallbacks.OnPermissionDenied -= OnPermissionDenied;
        permissionCallbacks.OnPermissionDeniedAndDontAskAgain -= OnPermissionDenied;

        // Disconnect the camera if it's still connected
        if (MLDepthCamera.IsConnected)
        {
            DisconnectCamera();
        }
    }

    /// <summary>
    /// Configures the depth camera settings after permission is granted.
    /// </summary>
    private void ConfigureCamera()
    {
        permissionGranted = true;

        // Configure stream settings for both long and short-range frames
        MLDepthCamera.StreamConfig[] config = new MLDepthCamera.StreamConfig[2];

        int i = (int)MLDepthCamera.FrameType.LongRange;
        config[i].Flags = (uint)captureFlag;
        config[i].Exposure = 1600;
        config[i].FrameRateConfig = MLDepthCamera.FrameRate.FPS_5;

        i = (int)MLDepthCamera.FrameType.ShortRange;
        config[i].Flags = (uint)captureFlag;
        config[i].Exposure = 375;
        config[i].FrameRateConfig = MLDepthCamera.FrameRate.FPS_5;

        // Apply the configuration settings
        var settings = new MLDepthCamera.Settings()
        {
            Streams = stream,
            StreamConfig = config
        };

        MLDepthCamera.SetSettings(settings);

        // Connect to the camera and apply updated settings
        ConnectCamera();
        UpdateSettings();
    }

    /// <summary>
    /// Attempts to connect to the depth camera and updates the debug text based on the result.
    /// </summary>
    public void ConnectCamera()
    {
        var result = MLDepthCamera.Connect();
        debugText.text = result.IsOk && MLDepthCamera.IsConnected
            ? "Connected to depth camera with stream"
            : "Failed to connect to camera.";
    }

    /// <summary>
    /// Attempts to disconnect from the depth camera and updates the debug text based on the result.
    /// </summary>
    public void DisconnectCamera()
    {
        var result = MLDepthCamera.Disconnect();
        debugText.text = result.IsOk && !MLDepthCamera.IsConnected
            ? "Disconnected from depth camera with stream"
            : "Failed to disconnect from camera.";
    }

    /// <summary>
    /// Updates the depth camera settings.
    /// </summary>
    private void UpdateSettings()
    {
        // Similar configuration process as in ConfigureCamera()
        // This method is meant to apply settings updates while the camera is already running
        MLDepthCamera.StreamConfig[] config = new MLDepthCamera.StreamConfig[2];

        int i = (int)MLDepthCamera.FrameType.LongRange;
        config[i].Flags = (uint)captureFlag;
        config[i].Exposure = 1600;
        config[i].FrameRateConfig = MLDepthCamera.FrameRate.FPS_5;

        i = (int)MLDepthCamera.FrameType.ShortRange;
        config[i].Flags = (uint)captureFlag;
        config[i].Exposure = 375;
        config[i].FrameRateConfig = MLDepthCamera.FrameRate.FPS_5;

        var settings = new MLDepthCamera.Settings()
        {
            Streams = stream,
            StreamConfig = config
        };

        MLDepthCamera.UpdateSettings(settings);
    }

    // Permission event handlers
    private void OnPermissionGranted(string permission)
    {
        debugText.text = $"Granted {permission}.";
        ConfigureCamera();
    }

    private void OnPermissionDenied(string permission)
    {
        debugText.text = $"Denied {permission}.";
        MLPluginLog.Error($"{permission} denied, example won't function.");
    }
}
