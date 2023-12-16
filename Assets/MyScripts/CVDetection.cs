using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnityExample.DnnModel;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

/// <summary>
/// Class for handling computer vision detection tasks, specifically focusing on face detection using the YuNetV2 model.
/// </summary>
public class CVDetection
{
    // YuNet V2 face detector model
    YuNetV2FaceDetector model;

    // Filename for the face detection model
    protected static readonly string FACE_DETECTION_MODEL_FILENAME = "OpenCVForUnity/dnn/face_detection_yunet_2023mar.onnx";

    // File path for the face detection model
    string face_detection_model_filepath;

    // Parameters for the face detection model
    int inputSizeW = 320;
    int inputSizeH = 320;
    float scoreThreshold = 0.6f;
    float nmsThreshold = 0.3f;
    int topK = 5000;

    /// <summary>
    /// Constructor to initialize the face detection model.
    /// </summary>
    public CVDetection()
    {
        initModel();
    }

    /// <summary>
    /// Initializes the YuNet V2 face detection model.
    /// </summary>
    public void initModel()
    {
        face_detection_model_filepath = Utils.getFilePath(FACE_DETECTION_MODEL_FILENAME);

        if (string.IsNullOrEmpty(face_detection_model_filepath))
        {
            model = null;
        }
        else
        {
            model = new YuNetV2FaceDetector(face_detection_model_filepath, "", new Size(inputSizeW, inputSizeH), scoreThreshold, nmsThreshold, topK);
        }
    }

    /// <summary>
    /// Finds the center of heads in the given image plane using the face detection model.
    /// </summary>
    /// <param name="imagePlane">The image plane containing the image data.</param>
    /// <returns>An array of Vector3 representing the centers of detected faces.</returns>
    public Vector3[] findHeadCenter(MLCamera.PlaneInfo imagePlane){
        // Process the image and prepare for face detection
        int imageWidth = (int)imagePlane.Width;
        int imageHeight = (int)imagePlane.Height;
        Mat rgbaMat = new Mat(imageHeight, imageWidth, CvType.CV_8UC4);
        rgbaMat.put(0, 0, imagePlane.Data);

        Mat bgrMat = new Mat(imageHeight, imageWidth, CvType.CV_8UC3);;
        Imgproc.cvtColor(rgbaMat, bgrMat, Imgproc.COLOR_RGBA2BGR);

        // Detect faces in the image
        Mat faces = DetectFace(bgrMat);

        // Calculate face centers
        Vector3[] face_centers;
        if (faces.rows() != 0) {
            face_centers = new Vector3[faces.rows()];
            for (int i = 0; i < faces.rows(); i++) {
                float[] box = new float[4];
                faces.get(i, 0, box);

                // Calculate the center of the face bounding box
                float centerX = (box[0] + box[0] + box[2]) / 2;
                float centerY = (box[1] + box[1] + box[3]) / 2;
                face_centers[i] = new Vector3(centerX, centerY, (float)faces.get(i, 14)[0]);
            }
        } else {
            face_centers = new Vector3[0];
        }

        return face_centers;
    }

    /// <summary>
    /// Detects faces in the given image using the loaded model.
    /// </summary>
    /// <param name="image">The image to process.</param>
    /// <returns>A Mat object containing detected faces.</returns>
    public Mat DetectFace(Mat image)
    {
        if (model == null)
        {
            Debug.LogError("faceDetector is null. Please check the dnn library is loaded properly: " + FACE_DETECTION_MODEL_FILENAME);
            return null;
        }

        // Perform face detection
        Mat faces = model.infer(image);

        return faces;
    }

    /// <summary>
    /// Undistorts a given depth image using the specified camera intrinsics.
    /// </summary>
    /// <param name="distortedImage">The distorted image to be processed.</param>
    /// <param name="undistortedImage">The output undistorted image.</param>
    /// <param name="intrinsics">Camera intrinsics used for undistortion.</param>
    public void UndistortDepthImage(Mat distortedImage, ref Mat undistortedImage, MLDepthCamera.Intrinsics intrinsics) {

        // Create camera matrix from intrinsics
        Mat cameraMatrix = new Mat(3, 3, CvType.CV_32FC1);
        cameraMatrix.put(0, 0, intrinsics.FocalLength.x);
        cameraMatrix.put(0, 1, 0);
        cameraMatrix.put(0, 2, intrinsics.PrincipalPoint.x);
        cameraMatrix.put(1, 0, 0);
        cameraMatrix.put(1, 1, intrinsics.FocalLength.y);
        cameraMatrix.put(1, 2, intrinsics.PrincipalPoint.y);
        cameraMatrix.put(2, 0, 0);
        cameraMatrix.put(2, 1, 0);
        cameraMatrix.put(2, 2, 1);

        // Create distortion coefficients
        Mat distortionCoefficients = new Mat(1, 5, CvType.CV_32FC1);
        distortionCoefficients.put(0, 0, intrinsics.Distortion.K1);
        distortionCoefficients.put(0, 1, intrinsics.Distortion.K2);
        distortionCoefficients.put(0, 2, intrinsics.Distortion.P1);
        distortionCoefficients.put(0, 3, intrinsics.Distortion.P2);
        distortionCoefficients.put(0, 4, intrinsics.Distortion.K3);

        // Perform undistortion
        OpenCVForUnity.Calib3dModule.Calib3d.undistort(distortedImage, undistortedImage, cameraMatrix, distortionCoefficients);
    }
}
