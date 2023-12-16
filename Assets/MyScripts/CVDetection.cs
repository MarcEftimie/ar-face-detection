using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnityExample.DnnModel;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

public class CVDetection
{
    YuNetV2FaceDetector model;
    protected static readonly string FACE_DETECTION_MODEL_FILENAME = "OpenCVForUnity/dnn/face_detection_yunet_2023mar.onnx";
    string face_detection_model_filepath;

    int inputSizeW = 320;
    int inputSizeH = 320;
    float scoreThreshold = 0.6f;
    float nmsThreshold = 0.3f;
    int topK = 5000;

    public CVDetection()
    {
        initModel();
    }

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

    // public Mat findHeadCenter(MLCamera.PlaneInfo imagePlane){
    public Vector3[] findHeadCenter(MLCamera.PlaneInfo imagePlane){
        int imageWidth = (int)imagePlane.Width;
        int imageHeight = (int)imagePlane.Height;
        Mat rgbaMat = new Mat(imageHeight, imageWidth, CvType.CV_8UC4);
        rgbaMat.put(0, 0, imagePlane.Data);

        Mat bgrMat = new Mat(imageHeight, imageWidth, CvType.CV_8UC3);;
        Imgproc.cvtColor(rgbaMat, bgrMat, Imgproc.COLOR_RGBA2BGR);

        // return face_center;
        Mat faces = DetectFace(bgrMat);

        // return faces;

        Vector3[] face_centers;
        if (faces.rows() != 0) {
            face_centers = new Vector3[faces.rows()];
            for (int i = 0; i < faces.rows(); i++) {
                float[] box = new float[4];
                faces.get(i, 0, box);

                float top_left_x = box[0];
                float top_left_y = box[1];
                float bottom_right_x = box[0] + box[2];
                float bottom_right_y = box[1] + box[3];
                
                face_centers[i] = new Vector3((top_left_x + bottom_right_x) / 2, (top_left_y + bottom_right_y) / 2, (float)faces.get(i, 14)[0]);
            }
        } else {
            face_centers = new Vector3[0];
        }

        return face_centers;
    }

    public Mat DetectFace(Mat image)
    {
        if (model == null)
        {
            Debug.LogError("faceDetector is null. Please check the dnn library is loaded properly: " + FACE_DETECTION_MODEL_FILENAME);
            return null;
        }

        Mat faces = model.infer(image);

        return faces;
    }

    public void UndistortDepthImage(Mat distortedImage, ref Mat undistortedImage, MLDepthCamera.Intrinsics intrinsics) {

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

        Mat distortionCoefficients = new Mat(1, 5, CvType.CV_32FC1);

        distortionCoefficients.put(0, 0, intrinsics.Distortion.K1);
        distortionCoefficients.put(0, 1, intrinsics.Distortion.K2);
        distortionCoefficients.put(0, 2, intrinsics.Distortion.P1);
        distortionCoefficients.put(0, 3, intrinsics.Distortion.P2);
        distortionCoefficients.put(0, 4, intrinsics.Distortion.K3);

        OpenCVForUnity.Calib3dModule.Calib3d.undistort(distortedImage, undistortedImage, cameraMatrix, distortionCoefficients);
    }

}
