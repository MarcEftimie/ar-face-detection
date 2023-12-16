using UnityEngine;
using TMPro;

/// <summary>
/// Manages the toggling functionality for an AR button, including enabling/disabling a Renderer
/// and toggling a property within a CV (Computer Vision) component.
/// Inherits from ARButton to utilize its button-click handling.
/// </summary>
public class ARButtonToggle : ARButton
{
    public CV CV;                    // Reference to the CV (Computer Vision) component
    public Renderer one;             // The Renderer component that will be toggled
    public TextMeshProUGUI debugText; // UI text for displaying debug information

    private bool toggle = false;     // Boolean flag to manage toggle state

    /// <summary>
    /// Overrides the OnButtonClicked method from ARButton to provide specific toggle functionality.
    /// </summary>
    protected override void OnButtonClicked()
    {
        debugText.text = "Button clicked";

        // Toggle the state of the Renderer and the 'undistort' property in the CV component
        if (toggle)
        {
            one.enabled = true;      // Enable the Renderer
            toggle = false;          // Update the toggle flag
            CV.undistort = true;     // Set the 'undistort' property to true in the CV component
        } 
        else
        {
            one.enabled = false;     // Disable the Renderer
            toggle = true;           // Update the toggle flag
            CV.undistort = false;    // Set the 'undistort' property to false in the CV component
        }
    }
}
