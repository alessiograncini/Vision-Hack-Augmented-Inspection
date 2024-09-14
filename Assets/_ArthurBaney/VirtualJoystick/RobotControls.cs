using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotControls : MonoBehaviour
{
    public float rotateSpeed = 90; //Degrees per second
    public float moveSpeed = 1; //Meters per second

    RobotStateSync rss;

    float legRotation;
    float legRotationDir = 1;

    float legSpeed = 90;

    XRHands xrhands;

    RobotController robotController;

    float x_final;
    float y_final;

    void Start()
    {
        robotController = FindObjectOfType<RobotController>();
        xrhands = FindObjectOfType<XRHands>();
        rss = GetComponent<RobotStateSync>();
        Invoke("UpdateRobot", .5f);
    }

    void UpdateRobot()
    {
        if (Mathf.Abs(x_final) > 0 || Mathf.Abs(y_final) > 0)
        {
            robotController.Move(y_final, 0, -x_final, .5f);
        }

        Invoke("UpdateRobot", .5f);
    }

    void Update()
    {
        //Arrow Keys Input
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        //Game Controller Input
        //float x_joystick = Input.GetAxis("JoystickAxis1");
        //float y_joystick = -Input.GetAxis("JoystickAxis2");

        //Virtual Joystick Controller Input
        float x_virtual_joystick = xrhands.JoystickX;
        float y_virtual_joystick = xrhands.JoystickY;

        //Final Controller Input
        x_final = x /*+ x_joystick*/ + x_virtual_joystick;
        y_final = z /*+ y_joystick*/ + y_virtual_joystick;

        //Clamp Input Values between -1 and 1
        x_final = Mathf.Clamp(x_final, -1, 1);
        y_final = Mathf.Clamp(y_final, -1, 1);

        bool moving = false;
        bool rotating = false;

        if (Mathf.Abs(x_final) > .01f)
        {
            transform.Rotate(new Vector3(0, x_final * Time.deltaTime * rotateSpeed, 0));
            rotating = true;
        }

        if (Mathf.Abs(y_final) > .01f)
        {
            transform.position += transform.forward * y_final * Time.deltaTime * moveSpeed;

            moving = true;
        }

        if (moving || rotating)
        {
            legRotation += legRotationDir * Mathf.Max(
                Mathf.Abs(x_final),
                Mathf.Abs(y_final)) * Time.deltaTime * legSpeed;

            //Animate virtual robots leg rotations
            //(Later set these rotations directly from API calls to the robot)
            if (legRotation > 20)
            {
                legRotationDir = -1;
            }

            if (legRotation < -20)
            {
                legRotationDir = 1;
            }
        }
        else
        {
            //If not moving then animate leg rotations back to default positions
            //(Only do this for virtual robot, not real robot)
            if (legRotation > 0)
            {
                legRotation -= Mathf.Min(legSpeed * Time.deltaTime, (legRotation));
            }
            if (legRotation < 0)
            {
                legRotation += Mathf.Min(legSpeed * Time.deltaTime, (-legRotation));
            }
        }

        rss.FrontLeftUpperLegAngle = legRotation;
        rss.FrontRightUpperLegAngle = -legRotation;
        rss.BackLeftUpperLegAngle = -legRotation;
        rss.BackRightUpperLegAngle = legRotation;
    }

    public float GetFinalXInput()
    {
        float x = Input.GetAxis("Horizontal");
        float x_joystick = Input.GetAxis("JoystickAxis1");
        float x_virtual_joystick = xrhands.JoystickX;
        float x_final = x + x_joystick + x_virtual_joystick;
        return Mathf.Clamp(x_final, -1, 1);
    }

    public float GetFinalYInput()
    {
        float z = Input.GetAxis("Vertical");
        float y_joystick = -Input.GetAxis("JoystickAxis2");
        float y_virtual_joystick = xrhands.JoystickY;
        float y_final = z + y_joystick + y_virtual_joystick;
        return Mathf.Clamp(y_final, -1, 1);
    }
}
