using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARButtonDecrement : ARButton
{
    public CVManager cvManager;

    protected override void OnButtonClicked()
    {
        cvManager.updateTime -= 0.05f;
    }
}
