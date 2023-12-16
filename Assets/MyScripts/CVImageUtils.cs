using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.XR.MagicLeap;
using System;

public class CVImageUtils
{
    private const int depthImageWidth = 544;
    private const int depthImageHeight = 480;

    private const float depthImageRatio = 1.415f;
    private const float resolutionScale = 1;
    private const int verticalOffset = 20;
    private const int horizontalOffset = 6;

    private const float adjustedDepthWidth = depthImageWidth * depthImageRatio * resolutionScale;
    private const float adjustedDepthHeight = depthImageHeight * depthImageRatio * resolutionScale;

    public Vector2 rgbPixelToDepthPixel(Vector2 rgbPixel, Vector2 rgbImageDimensions)
    {
        Vector2 depthPixel = new Vector2((int)Mathf.Lerp((adjustedDepthWidth-rgbImageDimensions.x)/2 + horizontalOffset*resolutionScale, adjustedDepthWidth-(adjustedDepthWidth-rgbImageDimensions.x)/2 + horizontalOffset*resolutionScale, rgbPixel.x / rgbImageDimensions.x) / depthImageRatio,
                                         (int)Mathf.Lerp((adjustedDepthHeight-rgbImageDimensions.y)/2 + verticalOffset*resolutionScale, adjustedDepthHeight-(adjustedDepthHeight-rgbImageDimensions.y)/2 + verticalOffset*resolutionScale, rgbPixel.y / rgbImageDimensions.y) / depthImageRatio);
        return depthPixel;
    }

    public float getDepth(Vector2 depthPixel, MLDepthCamera.Data depthMap) {
        int index = (int)depthPixel.y * (int)depthMap.DepthImage.Value.Width + (int)depthPixel.x;
        float depth = BitConverter.ToSingle(depthMap.DepthImage.Value.Data, index * 4);
        return depth;
    }
}
