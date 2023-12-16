using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.MagicLeap;

public class CameraUtilities
{
    public static string debugTextString = "Debug Text";
    // The length to be used for the ray when no intersection with the WorldMesh is found.
    // This prevents the ray from "popping" or suddenly changing length visually.
    private static float _rayLength = 10;

    /// <summary>
    /// Casts a ray from a 2D screen pixel position to a point in world space.
    /// </summary>
    /// <param name="icp">Intrinsic Calibration parameters of the camera.</param>
    /// <param name="cameraTransformMatrix">Transform matrix of the camera.</param>
    /// <param name="screenPoint">2D screen point to be cast.</param>
    /// <returns>The world space position where the ray intersects with the WorldMesh.</returns>
    public static Vector3 CastRayFromScreenToWorldPoint(MLCamera.IntrinsicCalibrationParameters icp, Matrix4x4 cameraTransformMatrix, Vector2 screenPoint, float depth)
    {
        var width = icp.Width;
        var height = icp.Height;

        // Convert pixel coordinates to normalized viewport coordinates.
        var viewportPoint = new Vector2(screenPoint.x / width, screenPoint.y / height);

        return CastRayFromViewPortToWorldPoint(icp, cameraTransformMatrix, viewportPoint, depth);
    }

    /// <summary>
    /// Casts a ray from a 2D viewport position to a point in world space.
    /// This method is used as Unity's Camera.ScreenToWorld functions are limited to Unity's virtual cameras,
    /// whereas this method provides a raycast from the actual physical RGB camera.
    /// </summary>
    /// <param name="icp">Intrinsic Calibration parameters of the camera.</param>
    /// <param name="cameraTransformMatrix">Transform matrix of the camera.</param>
    /// <param name="viewportPoint">2D viewport point to be cast.</param>
    /// <returns>The world space position where the ray intersects with the WorldMesh.</returns>
    public static Vector3 CastRayFromViewPortToWorldPoint(MLCamera.IntrinsicCalibrationParameters icp, Matrix4x4 cameraTransformMatrix, Vector2 viewportPoint, float depth)
    {
        // Undistort the viewport point to account for lens distortion.
        var undistortedViewportPoint = UndistortViewportPoint(icp, viewportPoint);

        // Create a ray based on the undistorted viewport point that projects out of the RGB camera.
        Ray ray = RayFromViewportPoint(icp, undistortedViewportPoint, cameraTransformMatrix.GetPosition(), cameraTransformMatrix.rotation);

        // By default, set the hit point at a fixed length away.
        Vector3 hitPoint = ray.GetPoint(depth + 0.1f);

        // // Raycast against the WorldMesh to find where the ray intersects.
        // // TODO: Add a layer mask filter to prevent unwanted obstructions.
        // if (Physics.Raycast(ray, out RaycastHit hit, 10))
        // {
        //     hitPoint = hit.point;
        //     _rayLength = hit.distance;
        //     debugTextString = "Hit Point: " + hitPoint.ToString() + "\nRay Length: " + _rayLength.ToString();
        // }

        return hitPoint;
    }

    /// <summary>
    /// Undistorts a viewport point to account for lens distortion.
    /// https://en.wikipedia.org/wiki/Distortion_(optics)
    /// </summary>
    /// <param name="icp">Intrinsic Calibration parameters of the camera.</param>
    /// <param name="distortedViewportPoint">The viewport point that may have distortion.</param>
    /// <returns>The corrected/undistorted viewport point.</returns>
    public static Vector2 UndistortViewportPoint(MLCamera.IntrinsicCalibrationParameters icp, Vector2 distortedViewportPoint)
    {
        var normalizedToPixel = new Vector2(icp.Width / 2, icp.Height / 2).magnitude;
        var pixelToNormalized = Mathf.Approximately(normalizedToPixel, 0) ? float.MaxValue : 1 / normalizedToPixel;
        var viewportToNormalized = new Vector2(icp.Width * pixelToNormalized, icp.Height * pixelToNormalized);
        var normalizedPrincipalPoint = icp.PrincipalPoint * pixelToNormalized;
        var normalizedToViewport = new Vector2(1 / viewportToNormalized.x, 1 / viewportToNormalized.y);

        Vector2 d = Vector2.Scale(distortedViewportPoint, viewportToNormalized);
        Vector2 o = d - normalizedPrincipalPoint;

        // Distortion coefficients.
        float K1 = (float)icp.Distortion[0];
        float K2 = (float)icp.Distortion[1];
        float P1 = (float)icp.Distortion[2];
        float P2 = (float)icp.Distortion[3];
        float K3 = (float)icp.Distortion[4];

        float r2 = o.sqrMagnitude;
        float r4 = r2 * r2;
        float r6 = r2 * r4;

        float radial = K1 * r2 + K2 * r4 + K3 * r6;
        Vector3 u = d + o * radial;

        // Tangential distortion correction.
        if (!Mathf.Approximately(P1, 0) || !Mathf.Approximately(P2, 0))
        {
            u.x += P1 * (r2 + 2 * o.x * o.x) + 2 * P2 * o.x * o.y;
            u.y += P2 * (r2 + 2 * o.y * o.y) + 2 * P1 * o.x * o.y;
        }

        return Vector2.Scale(u, normalizedToViewport);
    }

    /// <summary>
    /// Creates a ray projecting out from the RGB camera based on a viewport point.
    /// </summary>
    /// <param name="icp">Intrinsic Calibration parameters of the camera.</param>
    /// <param name="viewportPoint">2D viewport point to create the ray from.</param>
    /// <param name="cameraPos">Position of the camera.</param>
    /// <param name="cameraRotation">Rotation of the camera.</param>
    /// <returns>The created ray based on the viewport point.</returns>
    public static Ray RayFromViewportPoint(MLCamera.IntrinsicCalibrationParameters icp, Vector2 viewportPoint, Vector3 cameraPos, Quaternion cameraRotation)
    {
        var width = icp.Width;
        var height = icp.Height;
        var principalPoint = icp.PrincipalPoint;
        var focalLength = icp.FocalLength;

        Vector2 pixelPoint = new Vector2(viewportPoint.x * width, viewportPoint.y * height);
        Vector2 offsetPoint = new Vector2(pixelPoint.x - principalPoint.x, pixelPoint.y - (height - principalPoint.y));
        Vector2 unitFocalLength = new Vector2(offsetPoint.x / focalLength.x, offsetPoint.y / focalLength.y);

        Vector3 rayDirection = cameraRotation * new Vector3(unitFocalLength.x, unitFocalLength.y, 1).normalized;

        return new Ray(cameraPos, rayDirection);
    }
}