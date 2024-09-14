using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public GameObject prefab;
    public int gridSize = 3;
    public float spacing = 1f;
    public float comparisonThreshold = 0.5f;
    public Transform otherParent;


    [SerializeField]
    private float _updateInterval = 10;

    void OnEnable()
    {
        InstantiateGrid();
        StartCoroutine(Compare());
    }

    IEnumerator Compare()
    {
        CompareAndUpdateGrid();
        yield return new WaitForSeconds(_updateInterval);
        StartCoroutine(Compare());
    }

    void InstantiateGrid()
    {
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                for (int z = 0; z < gridSize; z++)
                {
                    Vector3 position = new Vector3(x * spacing, y * spacing, z * spacing);
                    GameObject obj = Instantiate(prefab, position, Quaternion.identity, transform);
                    obj.SetActive(false);
                }
            }
        }
    }



    void CompareAndUpdateGrid()
    {
        foreach (Transform gridObject in transform)
        {

            foreach ( Transform activeObject in otherParent)
            {
                if (Vector3.Distance(gridObject.transform.position,activeObject.transform.position)<comparisonThreshold)
                {
                    gridObject.gameObject.SetActive(true);
                    gridObject.gameObject.GetComponent<MeshRenderer>().material 
                        = activeObject.GetComponent<MeshRenderer>().material;
                }
                else
                {
                    gridObject.gameObject.SetActive(false);
                }  
            }
        }
    }
}
