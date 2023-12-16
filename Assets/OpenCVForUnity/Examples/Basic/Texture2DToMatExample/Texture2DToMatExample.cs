using UnityEngine;
using UnityEngine.SceneManagement;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.UnityUtils;

namespace OpenCVForUnityExample
{
    /// <summary>
    /// Texture2DToMat Example
    /// An example of converting a Texture2D image to OpenCV's Mat format.
    /// </summary>
    public class Texture2DToMatExample : MonoBehaviour
    {
        // Use this for initialization
        void Start()
        {
            // if true, The error log of the Native side OpenCV will be displayed on the Unity Editor Console.
            Utils.setDebugMode(true);

            // Dimensions of the texture
            int width = 100;
            int height = 100;

            // Create a new Texture2D
            Texture2D imgTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);

            // Create a byte array for CV_8UC4 format (4 channels)
            byte[] imageData = new byte[width * height * 4];

            for (int i = 0; i < imageData.Length; i += 4)
            {
                // Red channel
                imageData[i] = 255;
                // Green channel
                imageData[i + 1] = 0;
                // Blue channel
                imageData[i + 2] = 0;
                // Alpha channel
                imageData[i + 3] = 255;
            }

            // Load the image data into the texture
            imgTexture.LoadRawTextureData(imageData);
            imgTexture.Apply();

            // // Load the image texture from the Resources folder
            // Texture2D imgTexture = Resources.Load("face") as Texture2D;

            // Create a new Mat object with the same dimensions and color format as the texture
            Mat imgMat = new Mat(imgTexture.height, imgTexture.width, CvType.CV_8UC4);

            // // Convert the Texture2D image to Mat format
            Utils.texture2DToMat(imgTexture, imgMat);
            Debug.Log("imgMat.ToString() " + imgMat.ToString());

            // Create a new Texture2D with the same dimensions and color format as the Mat
            Texture2D texture = new Texture2D(imgMat.cols(), imgMat.rows(), TextureFormat.RGBA32, false);

            // Convert the Mat back to Texture2D format
            Utils.matToTexture2D(imgMat, texture);

            // Assign the created texture to the mainTexture of the game object's material
            gameObject.GetComponent<Renderer>().material.mainTexture = texture;


            Utils.setDebugMode(false);
        }

        // Update is called once per frame
        void Update()
        {
            // Update logic (not used in this example)
        }

        /// <summary>
        /// Raises the back button click event.
        /// </summary>
        public void OnBackButtonClick()
        {
            // Load the specified scene when the back button is clicked
            SceneManager.LoadScene("OpenCVForUnityExample");
        }
    }
}