using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.MagicLeap;
using TMPro;
using System.Data;

public class DepthCamera : MonoBehaviour
{
    public TextMeshProUGUI debugText;
    private readonly MLPermissions.Callbacks permissionCallbacks = new();
    private bool permissionGranted;
    private MLDepthCamera.Data lastData = null;
    private MLDepthCamera.Stream stream = MLDepthCamera.Stream.LongRange;
    private MLDepthCamera.CaptureFlags captureFlag = MLDepthCamera.CaptureFlags.DepthImage;
    void Awake()
    {
        permissionCallbacks.OnPermissionGranted += OnPermissionGranted;
        permissionCallbacks.OnPermissionDenied += OnPermissionDenied;
        permissionCallbacks.OnPermissionDeniedAndDontAskAgain += OnPermissionDenied;
    }

    void Start()
    {
        MLPermissions.RequestPermission(MLPermission.DepthCamera, permissionCallbacks);
    }

    public MLDepthCamera.Data GetDepthData()
    {
        if (!permissionGranted || !MLDepthCamera.IsConnected)
        {
            return null;
        }
        
        var result = MLDepthCamera.GetLatestDepthData(700, out MLDepthCamera.Data data);

        if (result.IsOk)
        {
            lastData = data;
        }

        if (lastData == null)
        {
            return null;
        }

        switch (captureFlag)
        {
            case MLDepthCamera.CaptureFlags.DepthImage:
                if (lastData.DepthImage != null)
                {
                    return lastData;
                }
                break;
        }
        return null;
    }

    private void OnDestroy()
    {
        permissionCallbacks.OnPermissionGranted -= OnPermissionGranted;
        permissionCallbacks.OnPermissionDenied -= OnPermissionDenied;
        permissionCallbacks.OnPermissionDeniedAndDontAskAgain -= OnPermissionDenied;
        if (MLDepthCamera.IsConnected)
        {
            DisonnectCamera();
        }
    }

    private void ConfigureCamera()
    {
        permissionGranted = true;

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

        MLDepthCamera.SetSettings(settings);

        ConnectCamera();
        UpdateSettings();
    }

    public void ConnectCamera()
    {
        var result = MLDepthCamera.Connect();
        if (result.IsOk && MLDepthCamera.IsConnected)
        {
            debugText.text = "Connected to depth camera with stream";
        }
        else
        {
            debugText.text = "Failed to connect to camera.";
        }
    }

    public void DisonnectCamera()
    {
        var result = MLDepthCamera.Disconnect();
        if (result.IsOk && !MLDepthCamera.IsConnected)
        {
            debugText.text = "Disconnected to depth camera with stream";
        }
        else
        {
            debugText.text = "Failed to disconnect to camera.";
        }
    }

    private void UpdateSettings()
    {
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

    private void OnPermissionGranted(string permission)
    {
        debugText.text = $"Granted {permission}.";
        ConfigureCamera();
    }

    private void OnPermissionDenied(string permission)
    {
        if (permission == MLPermission.Camera)
        {
            MLPluginLog.Error($"{permission} denied, example won't function.");
            debugText.text = $"Denied {permission}.";
        }
        else if (permission == MLPermission.DepthCamera)
        {
            MLPluginLog.Error($"{permission} denied, example won't function.");
            debugText.text = $"Denied {permission}.";
        }
    }
}