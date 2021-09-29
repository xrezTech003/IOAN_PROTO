using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineTesting : MonoBehaviour
{
    public Material lineMat;

    private LineRenderer line;

    private void Start()
    {
        line = gameObject.AddComponent<LineRenderer>();

        Vector3[] points = new Vector3[]
        {
            Vector3.zero,
            new Vector3(0f, 0f, 10f)
        };

        line.positionCount = points.Length;
        line.SetPositions(points);

        line.startWidth = 0.25f;
        line.endWidth = 0.25f;

        line.material = lineMat;
    }
}
