using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.XR.MagicLeap;
using System;

public class CVImageUtils
{
    // Constants defining the dimensions and parameters of the depth image
    private const int depthImageWidth = 544;
    private const int depthImageHeight = 480;

    // Ratio to adjust the depth image dimensions
    private const float depthImageRatio = 1.415f;
    // Scale factor for resolution adjustment
    private const float resolutionScale = 1;
    // Offset values for aligning depth image
    private const int verticalOffset = 20;
    private const int horizontalOffset = 6;

    // Calculated dimensions of the depth image after applying ratio and scale
    private const float adjustedDepthWidth = depthImageWidth * depthImageRatio * resolutionScale;
    private const float adjustedDepthHeight = depthImageHeight * depthImageRatio * resolutionScale;

    /// <summary>
    /// Maps a pixel from the RGB image space to the depth image space.
    /// </summary>
    /// <param name="rgbPixel">The RGB pixel to be mapped.</param>
    /// <param name="rgbImageDimensions">The dimensions of the RGB image.</param>
    /// <returns>The corresponding pixel in depth image space.</returns>
    public Vector2 rgbPixelToDepthPixel(Vector2 rgbPixel, Vector2 rgbImageDimensions)
    {
        // Calculate the corresponding depth pixel based on the RGB pixel and image dimensions
        Vector2 depthPixel = new Vector2(
            (int)Mathf.Lerp(
                (adjustedDepthWidth - rgbImageDimensions.x) / 2 + horizontalOffset * resolutionScale,
                adjustedDepthWidth - (adjustedDepthWidth - rgbImageDimensions.x) / 2 + horizontalOffset * resolutionScale,
                rgbPixel.x / rgbImageDimensions.x
            ) / depthImageRatio,
            (int)Mathf.Lerp(
                (adjustedDepthHeight - rgbImageDimensions.y) / 2 + verticalOffset * resolutionScale,
                adjustedDepthHeight - (adjustedDepthHeight - rgbImageDimensions.y) / 2 + verticalOffset * resolutionScale,
                rgbPixel.y / rgbImageDimensions.y
            ) / depthImageRatio
        );
        return depthPixel;
    }

    /// <summary>
    /// Retrieves the depth value at a given pixel in the depth image.
    /// </summary>
    /// <param name="depthPixel">The pixel in the depth image.</param>
    /// <param name="depthMap">The depth map data.</param>
    /// <returns>The depth value at the specified pixel.</returns>
    public float getDepth(Vector2 depthPixel, MLDepthCamera.Data depthMap) {
        // Calculate the index in the depth data array
        int index = (int)depthPixel.y * (int)depthMap.DepthImage.Value.Width + (int)depthPixel.x;
        // Extract the depth value using BitConverter
        float depth = BitConverter.ToSingle(depthMap.DepthImage.Value.Data, index * 4);
        return depth;
    }
}
