using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.MagicLeap;
using TMPro;
using System.Data.Common;
using System;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.UnityUtils;
using System.Runtime.InteropServices;
using OpenCVForUnity.UtilsModule;
using System.Linq;

/// <summary>
/// Manages computer vision tasks within a Unity environment, specifically handling camera output,
/// face detection, depth data processing, and visualization of detected heads.
/// </summary>
public class CVManager : MonoBehaviour
{
    public TextMeshProUGUI debugText; // Text display for debugging purposes
    public CameraUtilities CameraUtilities; // Utilities for camera operations
    private CVDetection cvDetection; // Face detection utility
    public CVCamera cvCamera; // Reference to the CV camera component
    public DepthCamera depthCamera; // Depth camera component
    public List<GameObject> CenterObjects; // List of game objects to represent detected heads

    private CVImageUtils cvImageUtils; // Utility for image processing tasks

    private MLDepthCamera.Data depthCameraData = null; // Data from the depth camera

    public float updateTime; // Interval for updating the camera processing

    private float lastUpdateTime = 0.0f; // Time of the last update

    void Start()
    {
        // Initialize parameters and subscribe to the camera output event
        updateTime = 0.3f;
        cvDetection = new CVDetection();
        cvImageUtils = new CVImageUtils();
        cvCamera.OnCameraOutput += HandleCameraOutput;
    }

    /// <summary>
    /// Handles the output from the camera, processing the captured frame to detect heads and visualize them.
    /// </summary>
    /// <param name="capturedFrame">The captured frame from the camera.</param>
    /// <param name="resultExtras">Additional result data from the camera.</param>
    /// <param name="metadataHandle">Metadata associated with the captured frame.</param>
    private void HandleCameraOutput(MLCamera.CameraOutput capturedFrame, MLCamera.ResultExtras resultExtras, MLCamera.Metadata metadataHandle)
    {
        // Check update interval
        if (Time.time - lastUpdateTime < updateTime)
        {
            return;
        }
        debugText.text = "Update Time: " + updateTime;
        lastUpdateTime = Time.time;

        // Processing the captured frame
        Vector2 rgbImageDimensions = new Vector2((int)capturedFrame.Planes[0].Width, (int)capturedFrame.Planes[0].Height);
        Vector3[] headCenters = cvDetection.findHeadCenter(capturedFrame.Planes[0]);

        // Handle scenarios with different number of detected heads
        if (headCenters.Length == 0)
        {
            debugText.text += "\nNo faces detected";
            return;
        }

        // Limit the number of head centers to match the number of GameObjects
        if (headCenters.Length > CenterObjects.Count)
        {
            headCenters = headCenters.Take(CenterObjects.Count).ToArray();
        }

        // Get depth data
        depthCameraData = depthCamera.GetDepthData();
        if (depthCameraData == null)
        {
            debugText.text += "\nNo depth data";
            return;
        }

        // Process each detected head
        for (int i = 0; i < headCenters.Length; i++)
        {
            // Convert head center from RGB pixel to depth pixel
            Vector2 headCenterDepthPixel = cvImageUtils.rgbPixelToDepthPixel(headCenters[i], rgbImageDimensions);
            float headCenterDepth = cvImageUtils.getDepth(headCenterDepthPixel, depthCameraData);

            // Update debug text
            debugText.text += "\nHead Center: " + headCenters[i] + "\nHead Center Depth Pixel: " + headCenterDepthPixel + "\nHead Center Depth: " + headCenterDepth + "\nConfidence: " + headCenters[i].z;

            // Process frame pose and visual representation
            if (MLCVCamera.GetFramePose(resultExtras.VCamTimestamp, out Matrix4x4 cameraTransform).IsOk)
            {
                // Adjust head center position and set GameObject's position
                headCenters[i].y = rgbImageDimensions.y - headCenters[i].y;
                CenterObjects[i].transform.position = CameraUtilities.CastRayFromScreenToWorldPoint(resultExtras.Intrinsics.Value, cameraTransform, headCenters[i], headCenterDepth);
                
                // Calculate confidence and adjust the color of the GameObject
                float confidence = Mathf.Clamp(headCenters[i].z, 0.6f, 1f);
                GameObject childObject = CenterObjects[i].transform.Find("Model").gameObject;
                if (childObject != null)
                {
                    MeshRenderer meshRenderer = childObject.GetComponent<MeshRenderer>();
                    if (meshRenderer != null)
                    {
                        Material objectMaterial = meshRenderer.material;
                        float lerpValue = (confidence - 0.6f) / (1f - 0.6f);
                        objectMaterial.color = Color.Lerp(Color.red, Color.green, lerpValue);
                    }
                    else
                    {
                        Debug.LogError("MeshRenderer not found on the child object");
                    }
                }
                else
                {
                    Debug.LogError("Child object 'Model' not found");
                }

                // Activate the GameObject
                CenterObjects[i].SetActive(true);
            }
        }

        // Deactivate unused GameObjects
        for (int i = headCenters.Length; i < CenterObjects.Count; i++)
        {
            CenterObjects[i].SetActive(false);
        }

        // Update debug text with the number of faces detected
        debugText.text += "\nFaces Detected: " + headCenters.Length;
    }
}
