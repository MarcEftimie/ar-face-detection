using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Represents a basic AR button that responds to hand tracking input.
/// It can be extended to create buttons with specific functionalities.
/// </summary>
public class ARButton : MonoBehaviour
{
    public HandTracker handTracker; // Reference to the HandTracker to detect hand interactions
    private CapsuleCollider buttonCollider; // Collider to detect interaction
    private Image buttonImage; // UI Image component of the button

    // Start is called before the first frame update
    void Start()
    {
        // Initialize button components
        buttonCollider = GetComponent<CapsuleCollider>();
        buttonImage = GetComponent<Image>();
    }

    private Bounds buttonBounds; // Bounds of the button collider
    private bool currentlyClicked = false; // Flag to check if the button is currently clicked
    private bool isButtonActive = true; // Flag to control button's active state

    /// <summary>
    /// Virtual method to be overridden in derived classes to define button click behavior.
    /// </summary>
    protected virtual void OnButtonClicked() {}

    void Update()
    {
        // Exit early if the button is not active
        if (!isButtonActive) return;

        // Update the bounds of the button
        buttonBounds = buttonCollider.bounds;

        // Check if either hand's index finger is within the button bounds
        if (buttonBounds.Contains(handTracker.leftIndexPosition) || buttonBounds.Contains(handTracker.rightIndexPosition))
        {
            // Trigger click action if not currently clicked
            if (!currentlyClicked)
            {
                currentlyClicked = true;
                OnButtonClicked(); // Call the button click event handler
            }
            // Change the button color to indicate interaction
            buttonImage.color = new Color(0.5f, 0.5f, 0.5f);
        }
        else if (currentlyClicked)
        {
            // If the button was clicked and the hand is no longer interacting, reset the button
            StartCoroutine(ButtonCooldown());
            currentlyClicked = false;
            buttonImage.color = new Color(1.0f, 1.0f, 1.0f); // Reset button color
        }
        else
        {
            currentlyClicked = false;
        }
    }

    /// <summary>
    /// Cooldown coroutine to temporarily disable the button after being clicked.
    /// </summary>
    IEnumerator ButtonCooldown()
    {
        isButtonActive = false; // Deactivate the button

        yield return new WaitForSeconds(0.2f); // Wait for a short duration

        isButtonActive = true; // Reactivate the button
    }
}
