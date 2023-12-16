using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.MagicLeap;
using TMPro;

/// <summary>
/// Manages the camera functionalities for computer vision tasks,
/// handles permissions, connectivity, and image capture processes.
/// </summary>
public class CVCamera : MonoBehaviour
{
    // Delegate declaration for camera update events
    public delegate void OnCameraUpdatedEventHandler(MLCamera.CameraOutput capturedFrame, MLCamera.ResultExtras resultExtras, MLCamera.Metadata metadataHandle);
    public event OnCameraUpdatedEventHandler OnCameraOutput; // Event fired when camera output is updated

    public TextMeshProUGUI debugText; // Text display for debugging and status messages

    [SerializeField, Tooltip("Desired width for the camera capture")]
    private int captureWidth = 1280;
    [SerializeField, Tooltip("Desired height for the camera capture")]
    private int captureHeight = 720;

    // Callbacks for handling ML permissions
    private readonly MLPermissions.Callbacks permissionCallbacks = new MLPermissions.Callbacks();
    private Coroutine enableCameraCoroutine; // Coroutine for enabling the camera

    // The identifier can either target the Main or CV cameras.
    private MLCamera.Identifier identifier = MLCamera.Identifier.CV;

    // Cached version of the MLCamera instance.
    private MLCamera camera;

    // Is true if the camera is ready to be connected.
    private bool cameraDeviceAvailable;

    // Cache the capture configure for later use.
    private MLCamera.CaptureConfig _captureConfig;

    void Awake()
    {
        // Subscribe to permission events
        permissionCallbacks.OnPermissionGranted += OnPermissionGranted;
        permissionCallbacks.OnPermissionDenied += OnPermissionDenied;
        permissionCallbacks.OnPermissionDeniedAndDontAskAgain += OnPermissionDenied;
    }

    void Start()
    {
        // Request camera permission at start
        MLPermissions.RequestPermission(MLPermission.Camera, permissionCallbacks);
    }

    /// <summary>
    /// Returns the configured image dimensions of the camera capture.
    /// </summary>
    /// <returns>Vector2 representing width and height.</returns>
    public Vector2 getImageDimensions()
    {
        return new Vector2(captureWidth, captureHeight);
    }

    /// <summary>
    /// Coroutine to enable the camera once the device is available.
    /// </summary>
    private IEnumerator EnableMLCamera()
    {
        // Check the camera's availability status
        while (!cameraDeviceAvailable)
        {
            MLResult result = MLCamera.GetDeviceAvailabilityStatus(MLCamera.Identifier.CV, out cameraDeviceAvailable);
            if (!(result.IsOk && cameraDeviceAvailable))
            {
                debugText.text = "Camera device not available";
                yield return new WaitForSeconds(0.5f);
            }
        }

        debugText.text = "Camera device available";
        yield return new WaitForSeconds(0.2f);

        // Connect to the camera
        ConnectCamera();
    }

    /// <summary>
    /// Connects to the camera and configures it.
    /// </summary>
    private void ConnectCamera()
    {
        if(cameraDeviceAvailable)
        {
            // Connect to the camera with the specified identifier
            MLCamera.ConnectContext connectContext = MLCamera.ConnectContext.Create();
            connectContext.CamId = identifier;
            connectContext.EnableVideoStabilization = true;

            camera = MLCamera.CreateAndConnect(connectContext);
            if (camera != null)
            {
                debugText.text = "Camera device connected";
                ConfigureCameraInput();
                SetCameraCallbacks();
            }
        }
    }

    /// <summary>
    /// Configures the camera input settings based on the desired capabilities.
    /// </summary>
    private void ConfigureCameraInput()
    {
        // Get camera stream capabilities
        MLCamera.StreamCapability[] streamCapabilities = MLCamera.GetImageStreamCapabilitiesForCamera(camera, MLCamera.CaptureType.Video);

        if(streamCapabilities.Length == 0)
            return;

        // Select the best fitting stream capability
        MLCamera.StreamCapability defaultCapability = streamCapabilities[0];
        if (MLCamera.TryGetBestFitStreamCapabilityFromCollection(streamCapabilities, captureWidth, captureHeight, MLCamera.CaptureType.Video, out MLCamera.StreamCapability selectedCapability))
        {
            defaultCapability = selectedCapability;
        }

        // Set up the capture configuration
        _captureConfig = new MLCamera.CaptureConfig();
        _captureConfig.CaptureFrameRate = MLCamera.CaptureFrameRate._30FPS;
        _captureConfig.StreamConfigs = new MLCamera.CaptureStreamConfig[1];
        _captureConfig.StreamConfigs[0] = MLCamera.CaptureStreamConfig.Create(defaultCapability, MLCamera.OutputFormat.RGBA_8888);

        // Start the video capture
        StartVideoCapture();
    }

    /// <summary>
    /// Starts the video capture process and sets the camera for capturing images.
    /// </summary>
    private void StartVideoCapture()
    {
        MLResult result = camera.PrepareCapture(_captureConfig, out MLCamera.Metadata metaData);
        if (result.IsOk)
        {
            camera.PreCaptureAEAWB(); // Prepare auto exposure and white balance
            result = camera.CaptureVideoStart(); // Start capturing video

            debugText.text = result.IsOk ? "Video capture started!" : "Failed to start video capture!";
        }
    }

    /// <summary>
    /// Sets the camera callback for handling raw video frames.
    /// </summary>
    private void SetCameraCallbacks()
    {
        camera.OnRawVideoFrameAvailable += RawVideoFrameAvailable; // Subscribe to the raw video frame event
    }

    /// <summary>
    /// Invoked when a raw video frame is available from the camera.
    /// </summary>
    void RawVideoFrameAvailable(MLCamera.CameraOutput output, MLCamera.ResultExtras extras, MLCamera.Metadata metadataHandle)
    {
        if (output.Format == MLCamera.OutputFormat.RGBA_8888)
        {
            OnCameraOutput?.Invoke(output, extras, metadataHandle);
        }
    }

    private void OnPermissionGranted(string permission)
    {
        // Start the camera enabling coroutine when permission is granted
        enableCameraCoroutine = StartCoroutine(EnableMLCamera());
    }

    private void OnPermissionDenied(string permission)
    {
        // Update the debug text if permission is denied
        debugText.text = $"{permission} denied.";
    }
}
