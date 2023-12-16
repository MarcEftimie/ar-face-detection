using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ARButton : MonoBehaviour
{
    public HandTracker handTracker;
    private CapsuleCollider buttonCollider;
    private Image buttonImage;

    // Start is called before the first frame update
    void Start()
    {
        buttonCollider = GetComponent<CapsuleCollider>();
        buttonImage = GetComponent<Image>();
    }

    private Bounds buttonBounds;
    private bool currentlyClicked = false;
    private bool isButtonActive = true;

    protected virtual void OnButtonClicked() {}

    void Update()
    {
        if (!isButtonActive) return;

        buttonBounds = buttonCollider.bounds;

        if (buttonBounds.Contains(handTracker.leftIndexPosition) || buttonBounds.Contains(handTracker.rightIndexPosition))
        {
            if (!currentlyClicked)
            {
                currentlyClicked = true;
                OnButtonClicked();
            }
            buttonImage.color = new Color(0.5f, 0.5f, 0.5f);
        }
        else if (currentlyClicked)
        {
            StartCoroutine(ButtonCooldown());
            currentlyClicked = false;
            buttonImage.color = new Color(1.0f, 1.0f, 1.0f);
        }
        else
        {
            currentlyClicked = false;
        }
    }

    IEnumerator ButtonCooldown()
    {
        isButtonActive = false;

        yield return new WaitForSeconds(0.2f);

        isButtonActive = true;
    }
}


