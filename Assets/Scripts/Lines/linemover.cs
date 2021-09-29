using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
<summary>
	CD : linemover
	Set Pos of Line Renderer to the telescope position on update
</summary>
**/
public class linemover : MonoBehaviour 
{
	/// <summary>
	///		VD : telescope
	/// </summary>
	public GameObject telescope;

    //REVISIONSTEST
    public bool bottom;

    private LineRenderer line;
    private readonly float min = 0f;
    private readonly float max = 2f;

    private void Start()
    {
        line = GetComponent<LineRenderer>();

        //REVISIONSTEST
        //line.startWidth = min;
        //line.endWidth = max;
    }

    /**
	<summary>
		FD : Update()
		Set Pos of Line Renderer to the telescope position
	</summary>
	**/
    private void Update()
	{
        //REVISIONSTESTING
        //TeeterLine();

		line.SetPosition(0, telescope.transform.position);
	}

    //REVISIONSTEST
    private bool up = true;

    private void TeeterLine()
    {
        float mod = .01f;

        if (!up) mod *= -1f;

        line.startWidth += mod;
        line.endWidth -= mod;

        if (line.startWidth >= max) up = false;
        else if (line.startWidth <= min) up = true;
    }
}
