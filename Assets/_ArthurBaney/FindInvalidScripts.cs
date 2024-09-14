using UnityEditor;
using UnityEngine;

public class FindInvalidScripts : EditorWindow
{
    [MenuItem("Tools/Find Missing Scripts")]
    public static void ShowWindow()
    {
        GetWindow(typeof(FindInvalidScripts));
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Find GameObjects with Missing Scripts"))
        {
            FindMissingScriptsInAllScenes();
        }
    }

    private static void FindMissingScriptsInAllScenes()
    {
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
        int count = 0;

        foreach (GameObject go in allObjects)
        {
            Component[] components = go.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == null)
                {
                    Debug.LogWarning($"Missing script found on GameObject: {go.name}", go);
                    count++;
                }
            }
        }

        if (count == 0)
        {
            Debug.Log("No GameObjects with missing scripts found.");
        }
        else
        {
            Debug.Log($"Total GameObjects with missing scripts: {count}");
        }
    }
}
