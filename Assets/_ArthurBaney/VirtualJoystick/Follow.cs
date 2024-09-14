using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follow : MonoBehaviour
{
    public Transform followObject;
    Vector3 target;
    float targetRotation;
    // Start is called before the first frame update
    void Start()
    {
        target = followObject.position;
        targetRotation = followObject.rotation.eulerAngles.y;
    }

    // Update is called once per frame
    void Update()
    {
        if (GetAngularDistance(targetRotation, followObject.rotation.eulerAngles.y) > 15f)
        {
            targetRotation = followObject.rotation.eulerAngles.y;
        }

        if (Vector3.Distance(target, followObject.position) > .15f)
        {
            target = followObject.position;
        }

        // Move towards the target position
        transform.position += (target - transform.position) * Time.deltaTime;

        float currentRotation = transform.rotation.eulerAngles.y;

        // Calculate the shortest angular distance
        float shortestAngle = Mathf.DeltaAngle(currentRotation, targetRotation);

        // Rotate towards the target by the shortest angular distance
        transform.rotation = Quaternion.Euler(
            0,
            currentRotation + shortestAngle * Time.deltaTime,
            0
        );
    }


    float GetAngularDistance(float angle1, float angle2)
    {
        // Normalize the angles to the range of -180 to 180
        float difference = Mathf.DeltaAngle(angle1, angle2);

        // Return the absolute value of the difference
        return Mathf.Abs(difference);
    }
}
