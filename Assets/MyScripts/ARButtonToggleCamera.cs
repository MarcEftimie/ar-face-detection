using UnityEngine;
using TMPro;

public class ARButtonToggleCamera : ARButton
{
    public TextMeshProUGUI debugText;
    public CVCamera cvCamera;
    public DepthCamera depthCamera;
    public int cameraID;
    private bool isDepthCameraDisconnected = false;
    protected override void OnButtonClicked()
    {
        debugText.text = "Button clicked";
        if (cameraID == 0)
        {
            // cvCamera.ApplyFineTranslationAdjustment(adjustment);
        } else if (cameraID == 1)
        {
            if (isDepthCameraDisconnected) {
                isDepthCameraDisconnected = false;
                depthCamera.ConnectCamera();
            } else {
                isDepthCameraDisconnected = true;
                depthCamera.DisonnectCamera();
            }
        }
    }
}
