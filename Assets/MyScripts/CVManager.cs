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

public class CVManager : MonoBehaviour
{
    public TextMeshProUGUI debugText;
    public CameraUtilities CameraUtilities;
    private CVDetection cvDetection;
    public CVCamera cvCamera;
    public DepthCamera depthCamera;
    public List<GameObject> CenterObjects;

    private CVImageUtils cvImageUtils;

    private MLDepthCamera.Data depthCameraData = null;

    public float updateTime;

    private float lastUpdateTime = 0.0f;

    void Start()
    {
        updateTime = 0.3f;
        cvDetection = new CVDetection();
        cvImageUtils = new CVImageUtils();
        cvCamera.OnCameraOutput += HandleCameraOutput;
    }

    private void HandleCameraOutput(MLCamera.CameraOutput capturedFrame, MLCamera.ResultExtras resultExtras, MLCamera.Metadata metadataHandle)
    {
        if (Time.time - lastUpdateTime < updateTime)
        {
            return;
        }
        debugText.text = "Update Time: " + updateTime;
        // Update lastUpdateTime to current time
        lastUpdateTime = Time.time;

        Vector2 rgbImageDimensions = new Vector2((int)capturedFrame.Planes[0].Width, (int)capturedFrame.Planes[0].Height);
        Vector3[] headCenters = cvDetection.findHeadCenter(capturedFrame.Planes[0]);

        if (headCenters.Length == 0)
        {
            debugText.text += "\nNo faces detected";
            return;
        }

        if (headCenters.Length > CenterObjects.Count)
        {
            headCenters = headCenters.Take(CenterObjects.Count).ToArray();
        }

        depthCameraData = depthCamera.GetDepthData();

        if (depthCameraData == null)
        {
            debugText.text += "\nNo depth data";
            return;
        }

        for (int i = 0; i < headCenters.Length; i++)
        {
            Vector2 headCenterDepthPixel = cvImageUtils.rgbPixelToDepthPixel(headCenters[i], rgbImageDimensions);
            float headCenterDepth = cvImageUtils.getDepth(headCenterDepthPixel, depthCameraData);
            debugText.text += "\nHead Center: " + headCenters[i] + "\nHead Center Depth Pixel: " + headCenterDepthPixel + "\nHead Center Depth: " + headCenterDepth + "\nConfidence: " + headCenters[i].z;
            if (MLCVCamera.GetFramePose(resultExtras.VCamTimestamp, out Matrix4x4 cameraTransform).IsOk)
            {
                headCenters[i].y = rgbImageDimensions.y - headCenters[i].y;
                CenterObjects[i].transform.position = CameraUtilities.CastRayFromScreenToWorldPoint(resultExtras.Intrinsics.Value, cameraTransform, headCenters[i], headCenterDepth);
                
                // Ensure the confidence value stays within 0.5 to 1.0
                float confidence = Mathf.Clamp(headCenters[i].z, 0.6f, 1f);

                // GameObject childObject = CenterObjects[i].transform.Find("Model").gameObject;
                // Material objectMaterial = childObject.GetComponent<MeshRenderer>().material;

                // Adjust the lerp value so that it starts from 0 at confidence 0.5 and goes to 1 at confidence 1

                // Lerp the color between red and green based on lerpValue
                // objectMaterial.color = Color.Lerp(Color.red, Color.green, lerpValue);

                // Find the child object named "Model" under the parent GameObject CenterObjects[i]
                GameObject childObject = CenterObjects[i].transform.Find("Model").gameObject;
                if (childObject != null)
                {
                    MeshRenderer meshRenderer = childObject.GetComponent<MeshRenderer>();
                    if (meshRenderer != null)
                    {
                        Material objectMaterial = meshRenderer.material;

                        // Lerp the color between red and green based on confidence
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

                // Make sure the GameObject is active (visible)
                CenterObjects[i].SetActive(true);
            }
        }

        // Hide remaining GameObjects that don't have a corresponding head center
        for (int i = headCenters.Length; i < CenterObjects.Count; i++)
        {
            CenterObjects[i].SetActive(false);
        }

        debugText.text += "\nFaces Detected: " + headCenters.Length;
    }

}
