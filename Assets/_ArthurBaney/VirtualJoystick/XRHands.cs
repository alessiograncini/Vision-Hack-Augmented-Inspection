using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Management;
#if UNITY_INCLUDE_XR_HANDS
using UnityEngine.XR.Hands;
using UnityEngine.XR.Management;
#endif

public class XRHands : MonoBehaviour
{
    public float radius = 3.5f;

    public Follow follow;

    public Material defaultJoystickMaterial;
    public Material hoverJoystickMaterial;
    public Material pinchingJoystickMaterial;
    public Transform debugSphere;
    [SerializeField]
    GameObject m_RightSpawnPrefab;

    [SerializeField]
    GameObject m_LeftSpawnPrefab;

    [SerializeField]
    Transform m_PolySpatialCameraTransform;

    public Transform joystick;
    MeshRenderer joystickMeshRenderer;

    public Vector3 joystickStartPosition;

    Vector3 joystickStartScale;

    public float JoystickX
    {
        get
        {
            return (joystick.localPosition.x - joystickStartPosition.x) / radius;
        }
    }

    public float JoystickY
    {
        get
        {
            return (joystick.localPosition.z - joystickStartPosition.z) / radius;
        }
    }

    Vector3 startPinchPosition;
    Vector3 pinchOffset;
    bool pinching;

    XRHandSubsystem m_HandSubsystem;
    XRHandJoint m_RightIndexTipJoint;
    XRHandJoint m_RightThumbTipJoint;
    XRHandJoint m_LeftIndexTipJoint;
    XRHandJoint m_LeftThumbTipJoint;
    bool m_ActiveRightPinch;
    bool m_ActiveLeftPinch;
    float m_ScaledThreshold;

    bool handOverJoystick = false;

    const float k_PinchThreshold = 0.02f;

    void Start()
    {
        joystickStartScale = joystick.localScale;
        joystickMeshRenderer = joystick.GetComponent<MeshRenderer>();

        GetHandSubsystem();
        m_ScaledThreshold = k_PinchThreshold / m_PolySpatialCameraTransform.localScale.x;
        joystickStartPosition = joystick.localPosition;
    }

    void Update()
    {
        if (!CheckHandSubsystem())
        return;

        var updateSuccessFlags = m_HandSubsystem.TryUpdateHands(XRHandSubsystem.UpdateType.Dynamic);

        if ((updateSuccessFlags & XRHandSubsystem.UpdateSuccessFlags.RightHandRootPose) != 0)
        {
            // assign joint values
            m_RightIndexTipJoint = m_HandSubsystem.rightHand.GetJoint(XRHandJointID.IndexTip);
            m_RightThumbTipJoint = m_HandSubsystem.rightHand.GetJoint(XRHandJointID.ThumbTip);

            DetectPinch(m_RightIndexTipJoint, m_RightThumbTipJoint, ref m_ActiveRightPinch, true);
        }

        if ((updateSuccessFlags & XRHandSubsystem.UpdateSuccessFlags.LeftHandRootPose) != 0)
        {
            // assign joint values
            m_LeftIndexTipJoint = m_HandSubsystem.leftHand.GetJoint(XRHandJointID.IndexTip);
            m_LeftThumbTipJoint = m_HandSubsystem.leftHand.GetJoint(XRHandJointID.ThumbTip);

            DetectPinch(m_LeftIndexTipJoint, m_LeftThumbTipJoint, ref m_ActiveLeftPinch, false);
        }

        if (!pinching)
        {
            joystick.localPosition = (joystickStartPosition + joystick.localPosition) / 2f;
        }
    }

    void GetHandSubsystem()
    {
        var xrGeneralSettings = XRGeneralSettings.Instance;
        if (xrGeneralSettings == null)
        {
            Debug.LogError("XR general settings not set");
        }

        var manager = xrGeneralSettings.Manager;
        if (manager != null)
        {
            var loader = manager.activeLoader;
            if (loader != null)
            {
                m_HandSubsystem = loader.GetLoadedSubsystem<XRHandSubsystem>();
                if (!CheckHandSubsystem())
                    return;

                m_HandSubsystem.Start();
            }
        }
    }

    bool CheckHandSubsystem()
    {
        if (m_HandSubsystem == null)
        {
#if !UNITY_EDITOR
                Debug.LogError("Could not find Hand Subsystem");
#endif
            enabled = false;
            return false;
        }

        return true;
    }

    void DetectPinch(XRHandJoint index, XRHandJoint thumb, ref bool activeFlag, bool right)
    {
        if (index.trackingState != XRHandJointTrackingState.None &&
            thumb.trackingState != XRHandJointTrackingState.None)
        {

            Vector3 indexPOS = Vector3.zero;
            Vector3 thumbPOS = Vector3.zero;

            if (index.TryGetPose(out Pose indexPose))
            {
                // adjust transform relative to the PolySpatial Camera transform
                indexPOS = m_PolySpatialCameraTransform.InverseTransformPoint(indexPose.position);
            }

            if (thumb.TryGetPose(out Pose thumbPose))
            {
                // adjust transform relative to the PolySpatial Camera adjustments
                thumbPOS = m_PolySpatialCameraTransform.InverseTransformPoint(thumbPose.position);
            }

            if (right)
            {
                Vector3 pinchPosition = indexPOS;
                // Convert pinchPosition to local space of joystick's parent transform
                Vector3 pinchPositionLocal = joystick.InverseTransformPoint(pinchPosition);

                //debugSphere.position = pinchPosition; //uncomment to debug pinch position

                if (Vector3.Distance(pinchPosition, joystick.position) < joystick.lossyScale.x)
                {
                    handOverJoystick = true;
                    joystick.localScale += ((joystickStartScale * 1.2f) - joystick.localScale) * Time.deltaTime;
                    joystickMeshRenderer.material = this.hoverJoystickMaterial;
                }
                else
                {
                    joystick.localScale += (joystickStartScale - joystick.localScale) * Time.deltaTime;
                    joystickMeshRenderer.material = this.defaultJoystickMaterial;
                }

                if (pinching)
                {
                    joystickMeshRenderer.material = this.pinchingJoystickMaterial;

                    joystick.position = pinchPosition;

                    joystick.localPosition = new Vector3(
                        Mathf.Clamp(
                            joystick.localPosition.x + pinchOffset.x ,
                            joystickStartPosition.x - radius,
                            joystickStartPosition.x + radius),
                        joystickStartPosition.y,
                        Mathf.Clamp(
                            joystick.localPosition.z + pinchOffset.z,
                            joystickStartPosition.z - radius,
                            joystickStartPosition.z + radius)
                    );
                }

                var pinchDistance = Vector3.Distance(indexPOS, thumbPOS);

                if (pinchDistance <= m_ScaledThreshold)
                {
                    if (!activeFlag)
                    {
                        if (handOverJoystick)
                        {
                            startPinchPosition = pinchPositionLocal;
                            pinching = true;
                            pinchOffset = joystick.localPosition - pinchPositionLocal;
                            follow.enabled = false;
                        }

                        //Instantiate(spawnObject, indexPOS, Quaternion.identity);
                        activeFlag = true;
                    }
                }
                else
                {
                    activeFlag = false;
                    pinching = false;
                    follow.enabled = true;
                }
            }
        }
    }
}

