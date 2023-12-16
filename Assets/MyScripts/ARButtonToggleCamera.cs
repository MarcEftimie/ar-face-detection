using UnityEngine;
using TMPro;

/// <summary>
/// Manages the toggling between different camera states in an AR environment.
/// Inherits from ARButton to utilize its button-click functionality.
/// </summary>
public class ARButtonToggleCamera : ARButton
{
    public TextMeshProUGUI debugText; // UI text for displaying debug information
    public CVCamera cvCamera;         // Reference to the computer vision camera
    public DepthCamera depthCamera;   // Reference to the depth camera
    public int cameraID;              // Identifier to distinguish between different cameras

    private bool isDepthCameraDisconnected = false; // Tracks the connection status of the depth camera

    /// <summary>
    /// Overrides the OnButtonClicked method from ARButton to provide specific functionality 
    /// for toggling between different camera states.
    /// </summary>
    protected override void OnButtonClicked()
    {
        debugText.text = "Button clicked";

        if (cameraID == 0)
        {
            // If the cameraID is 0, specific actions for the cvCamera can be performed here.
            // Example: cvCamera.ApplyFineTranslationAdjustment(adjustment);
        } 
        else if (cameraID == 1)
        {
            // Toggle the depth camera's connection state
            if (isDepthCameraDisconnected) 
            {
                isDepthCameraDisconnected = false;
                depthCamera.ConnectCamera(); // Connect the depth camera if it's currently disconnected
            } 
            else 
            {
                isDepthCameraDisconnected = true;
                depthCamera.DisconnectCamera(); // Disconnect the depth camera if it's currently connected
            }
        }
    }
}
