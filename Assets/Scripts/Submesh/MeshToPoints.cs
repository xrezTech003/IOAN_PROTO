using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
<summary>
    CD : MeshToPoints
    Set mesh vertices to a million random Vector3 points on start
    Acts more as a function I_A :
    :: NDH IV: Also not refenced anywhere or by anything, probably depreciated
</summary>
**/
public class MeshToPoints : MonoBehaviour 
{
    /**
    <summary>
        FD : Start()
        Set mesh vertices to a million random Vector3 points
    </summary>
    **/
	void Start () 
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;

        int PointCount = 1000000;

        List<Vector3> points = new List<Vector3>();
        List<int> indices = new List<int>();

        Vector3 p = new Vector3();
        for (int i = 0; i < PointCount; i++) {

            p = new Vector3(Random.Range(-15f, 15f), 0f, Random.Range(-15f, 15f));
            
            points.Add(p);
            indices.Add(i);
        }

        mesh.vertices = points.ToArray();

        mesh.SetIndices( indices.ToArray(), MeshTopology.Points, 0);

        mesh.RecalculateBounds();
    }

    #region USELESS_CODE
    // Update is called once per frame
    void Update()
    {

    }
    #endregion
}
