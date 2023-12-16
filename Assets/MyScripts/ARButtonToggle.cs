using UnityEngine;
using TMPro;

public class ARButtonToggle : ARButton
{
    public CV CV;
    public Renderer one;
    public TextMeshProUGUI debugText;
    private bool toggle = false;
    protected override void OnButtonClicked()
    {
            debugText.text = "Button clicked";
        
        if (toggle)
        {
            one.enabled = true;
            toggle = false;
            CV.undistort = true;
        } else
        {
            one.enabled = false;
            toggle = true;
            CV.undistort = false;
        }
    }
}
