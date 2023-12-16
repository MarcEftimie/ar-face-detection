using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.MagicLeap;
using UnityEngine.XR;

/// <summary>
/// Tracks hand movements and positions using Magic Leap's hand tracking capabilities.
/// </summary>
public class HandTracker : MonoBehaviour
{
    // Initialize hand tracking when the script starts
    void Start()
    {
        // Check and start hand tracking if the necessary permission is granted
        if (MLPermissions.CheckPermission(MLPermission.HandTracking).IsOk)
        {
            InputSubsystem.Extensions.MLHandTracking.StartTracking();
        }
    }

    private InputDevice leftHandDevice; // Device representing the left hand
    private InputDevice rightHandDevice; // Device representing the right hand

    // Lists to hold the bones of the index fingers for tracking
    private List<Bone> leftIndexFingerBones = new List<Bone>();
    private List<Bone> rightIndexFingerBones = new List<Bone>();

    // Public variables to hold the positions of the hands and index fingers
    public Vector3 leftHandPosition;
    public Vector3 rightHandPosition;
    public Vector3 leftIndexPosition;
    public Vector3 rightIndexPosition;

    // Update is called once per frame
    void Update()
    {
        // Check if the hand devices are valid; if not, find and assign them
        if (!leftHandDevice.isValid || !rightHandDevice.isValid)
        {
            leftHandDevice = InputSubsystem.Utils.FindMagicLeapDevice(InputDeviceCharacteristics.HandTracking | InputDeviceCharacteristics.Left);
            rightHandDevice = InputSubsystem.Utils.FindMagicLeapDevice(InputDeviceCharacteristics.HandTracking | InputDeviceCharacteristics.Right);
            return;
        }

        // Retrieve and process hand data for the left hand
        if (leftHandDevice.TryGetFeatureValue(CommonUsages.handData, out Hand leftHand))
        {
            leftHand.TryGetFingerBones(HandFinger.Index, leftIndexFingerBones);
        }

        // Retrieve and process hand data for the right hand
        if (rightHandDevice.TryGetFeatureValue(CommonUsages.handData, out Hand rightHand))
        {
            rightHand.TryGetFingerBones(HandFinger.Index, rightIndexFingerBones);
        }

        // Find and update the position of the left index finger
        for (int i = 0; i < leftIndexFingerBones.Count; ++i)
        {
            if (leftIndexFingerBones[i].TryGetPosition(out Vector3 leftHandBonePosition))
            {
                leftIndexPosition = leftHandBonePosition;
                break;
            }
        }

        // Find and update the position of the right index finger
        for (int i = 0; i < rightIndexFingerBones.Count; ++i)
        {
            if (rightIndexFingerBones[i].TryGetPosition(out Vector3 rightHandBonePosition))
            {
                rightIndexPosition = rightHandBonePosition;
                break;
            }
        }

        // Update the overall position of the left and right hands
        leftHandDevice.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 leftHandPositionOut);
        rightHandDevice.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 rightHandPositionOut);

        leftHandPosition = leftHandPositionOut;
        rightHandPosition = rightHandPositionOut;
    }
}
