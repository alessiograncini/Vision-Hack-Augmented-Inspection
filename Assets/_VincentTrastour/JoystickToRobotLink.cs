using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoystickToRobotLink : MonoBehaviour
{
    public RobotControls robotControls;
    public RobotController robotController;

    void Start()
    {
        if (robotControls == null)
        {
            robotControls = FindObjectOfType<RobotControls>();
        }

        if (robotController == null)
        {
            robotController = FindObjectOfType<RobotController>();
        }
    }

    void Update()
    {
        if (robotControls == null || robotController == null)
        {
            return;
        }

        // Get joystick inputs from RobotControls
        float x_final = robotControls.GetFinalXInput();
        float y_final = robotControls.GetFinalYInput();

        // Get movement settings from RobotController
        float forwardSpeed = robotController.GetForwardSpeed();
        float rightSpeed = robotController.GetRightSpeed();
        float rotationSpeed = robotController.GetRotationSpeed();
        float defaultDuration = robotController.GetDefaultDuration();

        // Map joystick inputs to robot movement
        float v_x = y_final * forwardSpeed;
        float v_y = x_final * rightSpeed;
        float v_rot = x_final * rotationSpeed;

        // Send movement command to RobotController
        robotController.CustomMove(v_x, v_y, v_rot, defaultDuration);
    }
}