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

public class CV : MonoBehaviour
{
    public bool undistort;
    public TextMeshProUGUI debugText;
    public CameraUtilities CameraUtilities;
    private CVDetection cvDetection;
    public CVCamera cvCamera;
    public DepthCamera depthCamera;
    public GameObject CenterObject;
    public Renderer _screenRendererRGB;
    private Texture2D rawVideoTexturesRGBA;

    private float lastUpdateTime = 0.0f;
    private float DEPTHWIDTH = 544;
    private float DEPTHHEIGHT = 480;

    private float currentAspectRatio;

    private MLDepthCamera.Data depthCameraData = null;
    private MLDepthCamera.CaptureFlags captureFlag = MLDepthCamera.CaptureFlags.DepthImage;
    private Texture2D ImageTexture = null;
    public Renderer imgRenderer;

    void Start()
    {
        cvDetection = new CVDetection();
        cvCamera.OnCameraOutput += HandleCameraOutput;
        _screenRendererRGB.enabled = true;
    }

    private void HandleCameraOutput(MLCamera.CameraOutput capturedFrame, MLCamera.ResultExtras resultExtras, MLCamera.Metadata metadataHandle)
    {
        if (Time.time - lastUpdateTime < 0.5f)
        {
            // Less than a second has passed since last update, do nothing
            return;
        }

        // Update lastUpdateTime to current time
        lastUpdateTime = Time.time;

        // int cvwidth = (int)capturedFrame.Planes[0].Width;
        // int cvheight = (int)capturedFrame.Planes[0].Height;
        // Vector2 headCenter = cvDetection.findHeadCenter(capturedFrame.Planes[0]);

        // if (headCenter.x == -1 && headCenter.y == -1)
        // {
        //     debugText.text = "Head Center: Not Found";
        //     return;
        // }

        // if (headCenter.x < (cvwidth - DEPTHWIDTH) / 2 || headCenter.x > DEPTHWIDTH + ((cvwidth - DEPTHWIDTH) / 2) || headCenter.y < (cvheight - DEPTHHEIGHT) / 2 || headCenter.y > DEPTHHEIGHT + ((cvheight - DEPTHHEIGHT) / 2)) {
        //     debugText.text = "Head Center: Out of Bounds";
        //     return;
        // }

        // debugText.text = "Head Center: " + headCenter.ToString();

        // float depth = depthCamera.CaptureDepthAtPoint((int)headCenter.x, (int)headCenter.y);
        // MLDepthCamera.Data depthData = depthCamera.GetDepthData();

        // ----------------- RGB -----------------
        UpdateRGBTexture(ref rawVideoTexturesRGBA, capturedFrame.Planes[0], _screenRendererRGB);
        _screenRendererRGB.material.mainTextureScale = new Vector2(1.0f, -1.0f);
        StartCoroutine(ResetCapturedDataFlagAtEndOfFrame());
        depthCameraData = depthCamera.GetDepthData();
        CheckAndCreateTexture((int)depthCameraData.DepthImage.Value.Width, (int)depthCameraData.DepthImage.Value.Height);

    
        // debugText.text += "\nNew" + bytes[0] + " " + bytes[1] + " " + bytes[2] + " " + bytes[3];

        // debugText.text = BitConverter.ToSingle(lastData.DepthImage.Value.Data, 4*3000).ToString();
        // Texture2D texture = new Texture2D(depthImage.cols(), depthImage.rows(), TextureFormat.RGBA32, false);
        // Mat normalizedDepthMap = new Mat();
        // Core.normalize(depthImage, normalizedDepthMap, 0, 255, Core.NORM_MINMAX, CvType.CV_8UC1);

        // Create a Texture2D from the normalized Mat
        // Texture2D texture = new Texture2D(normalizedDepthMap.cols(), normalizedDepthMap.rows(), TextureFormat.R8, false);

        // Convert the Mat to Texture2D
        // Utils.matToTexture2D(normalizedDepthMap, texture);
        // Utils.matToTexture2D(depthImage, ImageTexture);
        // byte[] bytes = new byte[depthCameraData.DepthImage.Value.Data.Length * 4];
        // debugText.text = "data " + depthCameraData.DepthImage.Value.Data[0] + " " + depthCameraData.DepthImage.Value.Data[1] + " " + depthCameraData.DepthImage.Value.Data[2] + " " + depthCameraData.DepthImage.Value.Data[3] ;
        // Marshal.Copy(depthImage, bytes, 0, bytes.Length);

        // byte[] bytes = new byte[undistortedDepthImage.total() * undistortedDepthImage.channels()];
        // MatUtils.copyFromMat(undistortedDepthImage, bytes);
        ImageTexture.LoadRawTextureData(depthCameraData.DepthImage.Value.Data);
        ImageTexture.Apply();
        debugText.text = "applied";

        

        
        // debugText.text = "planes" + capturedFrame.Planes[0].Width.ToString() + " " + capturedFrame.Planes[0].Height.ToString();

        // if (MLCVCamera.GetFramePose(depthData.FrameTimestamp, out Matrix4x4 depthTransform).IsOk)
        // {
        //     MLCameraBase.IntrinsicCalibrationParameters depthCameraIntrinsics = new MLCameraBase.IntrinsicCalibrationParameters(width: depthData.Intrinsics.Width, 
        //                                                                                                                                  height: depthData.Intrinsics.Height,
        //                                                                                                                                  focalLength: depthData.Intrinsics.FocalLength,
        //                                                                                                                                  principalPoint: depthData.Intrinsics.PrincipalPoint,
        //                                                                                                                                  fov: depthData.Intrinsics.FoV, 
        //                                                                                                                                  distortion: new double[] {depthData.Intrinsics.Distortion.K1,
        //                                                                                                                                                            depthData.Intrinsics.Distortion.K2,
        //                                                                                                                                                            depthData.Intrinsics.Distortion.P1,
        //                                                                                                                                                            depthData.Intrinsics.Distortion.P2,
        //                                                                                                                                                            depthData.Intrinsics.Distortion.K3});
        //     float depth = depthData.DepthImage.Value.Data[(int)headCenter.y * (int)depthData.DepthImage.Value.Width + (int)headCenter.x];
        //     CenterObject.transform.position = CameraUtilities.CastRayFromScreenToWorldPoint(depthCameraIntrinsics, depthTransform, headCenter, depth);
        // }

        // headCenter.y = cvheight - headCenter.y;
        // if (MLCVCamera.GetFramePose(resultExtras.VCamTimestamp, out Matrix4x4 cameraTransform).IsOk)
        // {
        //     CenterObject.transform.position = CameraUtilities.CastRayFromScreenToWorldPoint(resultExtras.Intrinsics.Value, cameraTransform, headCenter, depth);
        // }
    }

    private void UpdateRGBTexture(ref Texture2D videoTextureRGB, MLCamera.PlaneInfo imagePlane, Renderer renderer)
    {

        int actualWidth = (int)(imagePlane.Width * imagePlane.PixelStride);
        
        if (videoTextureRGB != null &&
            (videoTextureRGB.width != imagePlane.Width || videoTextureRGB.height != imagePlane.Height))
        {
            Destroy(videoTextureRGB);
            videoTextureRGB = null;
        }

        if (videoTextureRGB == null)
        {
            videoTextureRGB = new Texture2D((int)imagePlane.Width, (int)imagePlane.Height, TextureFormat.RGBA32, false);
            videoTextureRGB.filterMode = FilterMode.Bilinear;

            Material material = renderer.material;
            material.mainTexture = videoTextureRGB;
            material.mainTextureScale = new Vector2(1.0f, 1.0f);
        }

        SetProperRatio((int)imagePlane.Width, (int)imagePlane.Height, _screenRendererRGB);

        if (imagePlane.Stride != actualWidth)
        {
            var newTextureChannel = new byte[actualWidth * imagePlane.Height];
            for(int i = 0; i < imagePlane.Height; i++)
            {
                Buffer.BlockCopy(imagePlane.Data, (int)(i * imagePlane.Stride), newTextureChannel, i * actualWidth, actualWidth);
            }
            videoTextureRGB.LoadRawTextureData(newTextureChannel);
        }
        else
        {
            videoTextureRGB.LoadRawTextureData(imagePlane.Data);
        }
        videoTextureRGB.Apply();
    }

    private void SetProperRatio(int textureWidth, int textureHeight, Renderer renderer)
    {
        float ratio = textureWidth / (float)textureHeight;

        if (Math.Abs(currentAspectRatio - ratio) < float.Epsilon)
            return;

        currentAspectRatio = ratio;
        var localScale = renderer.transform.localScale;
        localScale = new Vector3(currentAspectRatio * localScale.y, localScale.y, 1);
        renderer.transform.localScale = localScale;
    }

    private IEnumerator ResetCapturedDataFlagAtEndOfFrame()
    {
        yield return new WaitForEndOfFrame();
    }

    private void CheckAndCreateTexture(int width, int height)
    {
        if (ImageTexture == null || (ImageTexture != null && (ImageTexture.width != width || ImageTexture.height != height)))
        {
            ImageTexture = new Texture2D(width, height, TextureFormat.RFloat, false);
            ImageTexture.filterMode = FilterMode.Bilinear;
            var material = imgRenderer.material;
            material.mainTexture = ImageTexture;
            material.mainTextureScale = new Vector2(1.0f, -1.0f);
        }
    }


}
