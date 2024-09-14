
using UnityEngine.Events;

public class Button3D : PolySpatial.Samples.HubButton
{
    public UnityEvent onClick;
    
    public override void Press()
    {
        base.Press();
        onClick.Invoke();
    }
}
