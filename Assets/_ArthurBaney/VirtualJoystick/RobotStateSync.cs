using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotStateSync : MonoBehaviour
{
    public Transform FrontLeftUpperLeg;
    public Transform FrontLeftLowerLeg;

    public Transform FrontRightUpperLeg;
    public Transform FrontRightLowerLeg;

    public Transform BackLeftUpperLeg;
    public Transform BackLeftLowerLeg;

    public Transform BackRightUpperLeg;
    public Transform BackRightLowerLeg;

    public float FrontLeftUpperLegStartAngle;
    public float FrontLeftLowerLegStartAngle;

    public float FrontRightUpperLegStartAngle;
    public float FrontRightLowerLegStartAngle;

    public float BackLeftUpperLegStartAngle;
    public float BackLeftLowerLegStartAngle;

    public float BackRightUpperLegStartAngle;
    public float BackRightLowerLegStartAngle;

    [Range(-90, 90)]
    public float FrontLeftUpperLegAngle = 0;
    [Range(-90, 90)]
    public float FrontLeftLowerLegAngle = 0;

    [Range(-90, 90)]
    public float FrontRightUpperLegAngle = 0;
    [Range(-90, 90)]
    public float FrontRightLowerLegAngle = 0;

    [Range(-90, 90)]
    public float BackLeftUpperLegAngle = 0;
    [Range(-90, 90)]
    public float BackLeftLowerLegAngle = 0;

    [Range(-90, 90)]
    public float BackRightUpperLegAngle = 0;
    [Range(-90, 90)]
    public float BackRightLowerLegAngle = 0;

    void Start()
    {
        // Store the start x-rotation of each limb
        FrontLeftUpperLegStartAngle = FrontLeftUpperLeg.localRotation.eulerAngles.x;
        FrontLeftLowerLegStartAngle = FrontLeftLowerLeg.localRotation.eulerAngles.x;

        FrontRightUpperLegStartAngle = FrontRightUpperLeg.localRotation.eulerAngles.x;
        FrontRightLowerLegStartAngle = FrontRightLowerLeg.localRotation.eulerAngles.x;

        BackLeftUpperLegStartAngle = BackLeftUpperLeg.localRotation.eulerAngles.x;
        BackLeftLowerLegStartAngle = BackLeftLowerLeg.localRotation.eulerAngles.x;

        BackRightUpperLegStartAngle = BackRightUpperLeg.localRotation.eulerAngles.x;
        BackRightLowerLegStartAngle = BackRightLowerLeg.localRotation.eulerAngles.x;
    }

    private void Update()
    {
        // Apply rotation based on the start angle and the adjustable angle
        FrontLeftUpperLeg.localRotation = Quaternion.Euler(FrontLeftUpperLegStartAngle + FrontLeftUpperLegAngle, 0, 0);
        FrontLeftLowerLeg.localRotation = Quaternion.Euler(FrontLeftLowerLegStartAngle + FrontLeftLowerLegAngle, 0, 0);

        FrontRightUpperLeg.localRotation = Quaternion.Euler(FrontRightUpperLegStartAngle + FrontRightUpperLegAngle, 0, 0);
        FrontRightLowerLeg.localRotation = Quaternion.Euler(FrontRightLowerLegStartAngle + FrontRightLowerLegAngle, 0, 0);

        BackLeftUpperLeg.localRotation = Quaternion.Euler(BackLeftUpperLegStartAngle + BackLeftUpperLegAngle, 0, 0);
        BackLeftLowerLeg.localRotation = Quaternion.Euler(BackLeftLowerLegStartAngle + BackLeftLowerLegAngle, 0, 0);

        BackRightUpperLeg.localRotation = Quaternion.Euler(BackRightUpperLegStartAngle + BackRightUpperLegAngle, 0, 0);
        BackRightLowerLeg.localRotation = Quaternion.Euler(BackRightLowerLegStartAngle + BackRightLowerLegAngle, 0, 0);
    }
}
