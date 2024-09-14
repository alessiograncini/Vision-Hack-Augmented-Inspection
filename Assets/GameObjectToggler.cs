using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectToggler : MonoBehaviour
{
    public GameObject gameObjectToToggle;

    public void Toggle()
    {
        gameObjectToToggle.SetActive(!gameObjectToToggle.activeSelf);
    }
}
