using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.MagicLeap;
using TMPro;

public class CVCamera : MonoBehaviour
{
    public delegate void OnCameraUpdatedEventHandler(MLCamera.CameraOutput capturedFrame, MLCamera.ResultExtras resultExtras, MLCamera.Metadata metadataHandle);
    public event OnCameraUpdatedEventHandler OnCameraOutput;
    public TextMeshProUGUI debugText;
    [SerializeField, Tooltip("Desired width for the camera capture")]
    private int captureWidth = 1280;
    [SerializeField, Tooltip("Desired height for the camera capture")]
    private int captureHeight = 720;
    private readonly MLPermissions.Callbacks permissionCallbacks = new MLPermissions.Callbacks();
    private Coroutine enableCameraCoroutine;
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
        permissionCallbacks.OnPermissionGranted += OnPermissionGranted;
        permissionCallbacks.OnPermissionDenied += OnPermissionDenied;
        permissionCallbacks.OnPermissionDeniedAndDontAskAgain += OnPermissionDenied;
    }

    void Start()
    {
        MLPermissions.RequestPermission(MLPermission.Camera, permissionCallbacks);
    }

    public Vector2 getImageDimensions()
    {
        return new Vector2(captureWidth, captureHeight);
    }

    private IEnumerator EnableMLCamera()
    {
        // Checks the main camera's availability.
        while (!cameraDeviceAvailable)
        {
            MLResult result = MLCamera.GetDeviceAvailabilityStatus(MLCamera.Identifier.CV, out cameraDeviceAvailable);
            if (!(result.IsOk && cameraDeviceAvailable))
            {
                // Wait until camera device is available
                debugText.text = "Camera device not available";
                yield return new WaitForSeconds(0.5f);
            }
        }

        debugText.text = "Camera device available";
        yield return new WaitForSeconds(0.2f);

        ConnectCamera();
    }

    private void ConnectCamera()
    {
        // Once the camera is available, we can connect to it.
        if(cameraDeviceAvailable)
        {
            MLCamera.ConnectContext connectContext = MLCamera.ConnectContext.Create();
            connectContext.CamId = identifier;
            // The MLCamera.Identifier.Main is the only camera that can access the virtual and mixed reality flags
            // connectContext.Flags = MLCamera.ConnectFlag.CamOnly;
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

    private void ConfigureCameraInput()
    {
        // Gets the stream capabilities the selected camera. (Supported capture types, formats and resolutions)
        MLCamera.StreamCapability[] streamCapabilities = MLCamera.GetImageStreamCapabilitiesForCamera(camera, MLCamera.CaptureType.Video);

        if(streamCapabilities.Length == 0)
            return;

        // Set the default capability stream
        MLCamera.StreamCapability defaultCapability = streamCapabilities[0];

        // Try to get the stream that most closely matches the target width and height
        if (MLCamera.TryGetBestFitStreamCapabilityFromCollection(streamCapabilities, captureWidth, captureHeight,
                MLCamera.CaptureType.Video, out MLCamera.StreamCapability selectedCapability))
        {
            defaultCapability = selectedCapability;
        }

        // Initialize a new capture config.
        _captureConfig = new MLCamera.CaptureConfig();
        // Set RGBA video as the output
        MLCamera.OutputFormat outputFormat = MLCamera.OutputFormat.RGBA_8888;
        // Set the Frame Rate to 30fps
        _captureConfig.CaptureFrameRate = MLCamera.CaptureFrameRate._30FPS;
        // Initialize a camera stream config.
        _captureConfig.StreamConfigs = new MLCamera.CaptureStreamConfig[1];
        _captureConfig.StreamConfigs[0] = MLCamera.CaptureStreamConfig.Create(
            defaultCapability, outputFormat
        );
        StartVideoCapture();
    }

    private void StartVideoCapture()
    {
        MLResult result = camera.PrepareCapture(_captureConfig, out MLCamera.Metadata metaData);
        if (result.IsOk)
        {
        // Trigger auto exposure and auto white balance
        camera.PreCaptureAEAWB();
        // Starts video capture. This call can also be called asynchronously 
        // Images capture uses the CaptureImage function instead.
        result = camera.CaptureVideoStart();
        if (result.IsOk)
        {
            debugText.text = "Video capture started!";
        }
        else
        {
            debugText.text = "Failed to start video capture!";
        }
        }
    }

    private void SetCameraCallbacks()
    {
        //Provides frames in either YUV/RGBA format depending on the stream configuration
        camera.OnRawVideoFrameAvailable += RawVideoFrameAvailable;
    }

    void RawVideoFrameAvailable(MLCamera.CameraOutput output, MLCamera.ResultExtras extras, MLCamera.Metadata metadataHandle)
    {
        if (output.Format == MLCamera.OutputFormat.RGBA_8888)
        {
            OnCameraOutput?.Invoke(output, extras, metadataHandle);
        }
    }

    private void OnPermissionGranted(string permission)
    {
        enableCameraCoroutine = StartCoroutine(EnableMLCamera());
    }

    private void OnPermissionDenied(string permission)
    {
        debugText.text = $"{permission} denied.";
    }
}
