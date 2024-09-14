using PolySpatial.Samples;
using UnityEngine;

public class RobotHandControl : MonoBehaviour
{
    JointVisuals jointVisuals;
    public Transform RobotArmXAxisJoint;
    public Transform RobotArmZAxisJoint;
    public Transform RobotHand;

    // Start is called before the first frame update
    void Start()
    {
        Invoke("CheckForJointVisuals", 1f);
    }

    void CheckForJointVisuals()
    {
        jointVisuals = FindObjectOfType<JointVisuals>();
        if (jointVisuals == null)
        {
            Invoke("CheckForJointVisuals", 1f);
        }
    }

    void Update()
    {
        if (jointVisuals)
        {
            if (HandVisualizer.RightHandJoints.ContainsKey(10))
            {
                Quaternion jointRotation = HandVisualizer.RightHandJoints[10].transform.rotation;
                Quaternion rotationAmount = Quaternion.Euler(270f, 0f, 0f);
                Quaternion xJointRotation = (jointRotation * rotationAmount);
                RobotArmXAxisJoint.rotation = Quaternion.Euler(
                    xJointRotation.eulerAngles.x,
                    0,
                    0
                );
                RobotArmZAxisJoint.rotation = Quaternion.Euler(
                                    0,
                                    0,
                                    xJointRotation.eulerAngles.z
                                );

                //jointVisuals.transform.rotation;
            }
        }
    }
}
