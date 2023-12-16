using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.MagicLeap;
using UnityEngine.XR;

public class HandTracker : MonoBehaviour
{

    void Start()
    {
        if (MLPermissions.CheckPermission(MLPermission.HandTracking).IsOk)
        {
            InputSubsystem.Extensions.MLHandTracking.StartTracking();
        }
    }

    private InputDevice leftHandDevice;
    private InputDevice rightHandDevice;
    private List<Bone> leftIndexFingerBones = new List<Bone>();
    private List<Bone> rightIndexFingerBones = new List<Bone>();
    public Vector3 leftHandPosition;
    public Vector3 rightHandPosition;
    public Vector3 leftIndexPosition;
    public Vector3 rightIndexPosition;
    // public GameObject leftIndexBox;
    // public GameObject rightIndexBox;

    void Update()
    {
        
        if (!leftHandDevice.isValid || !rightHandDevice.isValid)
        {
            leftHandDevice = InputSubsystem.Utils.FindMagicLeapDevice(InputDeviceCharacteristics.HandTracking | InputDeviceCharacteristics.Left);
            rightHandDevice = InputSubsystem.Utils.FindMagicLeapDevice(InputDeviceCharacteristics.HandTracking | InputDeviceCharacteristics.Right);
            return;
        }

        if (leftHandDevice.TryGetFeatureValue(CommonUsages.handData, out Hand leftHand))
        {
            leftHand.TryGetFingerBones(HandFinger.Index, leftIndexFingerBones);
        }
        
        if (rightHandDevice.TryGetFeatureValue(CommonUsages.handData, out Hand rightHand))
        {
            rightHand.TryGetFingerBones(HandFinger.Index, rightIndexFingerBones);
        }

        // int bonesCounted = 0;

        for (int i = 0; i < leftIndexFingerBones.Count; ++i)
        {
            if (leftIndexFingerBones[i].TryGetPosition(out Vector3 leftHandBonePosition))
            {
                // if (leftIndexFingerBones[i].TryGetRotation(out Quaternion leftHandBoneRotation))
                // {
                //     leftIndexBone1.transform.rotation = leftHandBoneRotation;
                //     Debug.Log(leftHandBoneRotation.eulerAngles);
                // }
                leftIndexPosition = leftHandBonePosition;
                // leftIndexBox.transform.position = leftHandBonePosition;
                // leftIndexBoxs[bonesCounted].transform.position = leftIndexPosition;
                // Debug.Log(leftIndexPosition);
                
                break;
                // bonesCounted += 1;
                
                // if (bonesCounted == 3) {
                //     break;
                // }
            }
        }

        for (int i = 0; i < rightIndexFingerBones.Count; ++i)
        {
            if (rightIndexFingerBones[i].TryGetPosition(out Vector3 rightHandBonePosition))
            {
                rightIndexPosition = rightHandBonePosition;
                // rightIndexBox.transform.position = rightHandBonePosition;

                break;
            }
        }

        leftHandDevice.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 leftHandPositionOut);
        rightHandDevice.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 rightHandPositionOut);

        leftHandPosition = leftHandPositionOut;
        rightHandPosition = rightHandPositionOut;
    }

}
