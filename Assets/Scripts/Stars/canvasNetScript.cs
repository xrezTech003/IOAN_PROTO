using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

/**
<summary>
	CD : canvasNetScript
	Sets transform data and text data in children
</summary>
**/
public class canvasNetScript : NetworkBehaviour
{
	#region PUBLIC_VAR
	/// <summary>
	///		VD : playerRef
	///		Player Reference Object
	/// </summary>
	public GameObject playerRef;
	#endregion

	#region UNITY_FUNCTIONS
	/**
	<summary>
		FD : Start()
		Sets Transform data and Text data in all children
	</summary>
	**/
	void Start ()
    {
		RectTransform myRect = gameObject.GetComponentInChildren<RectTransform>();

		myRect.localPosition = new Vector3(0f, 0.076f, 0.0185f);
		myRect.localRotation = Quaternion.Euler(45f, 180f, 0f);
        myRect.localScale = Vector3.one * .000076f;

        foreach (Transform childOuter in transform)
        {
			foreach (Transform childMid in childOuter.transform)
            {
				foreach (Transform childInner in childMid.transform)
                {
					if (childInner.GetComponent<Image>())
                    {
                        childInner.GetComponent<Image>().color = Color.white;

						if (childInner.GetComponentInChildren<Text>())
                        {
                            childInner.GetComponentInChildren<Text>().text = "";

							for (int i = 1; i < 10; i++)
                                childInner.GetComponentInChildren<Text>().text += "sample text ";
						}
					}

					if (childInner.GetComponent<Text>())
                    {
                        childInner.GetComponent<Text>().text = "";

						for (int i = 1; i < 10; i++)
                            childInner.GetComponent<Text>().text += "sample text ";
					}
				}
			}
		}
	}
	
    /**
	<summary>
		FD : Update()
		If gameObject has authority, set transform data to v:playerRef transform data
	</summary>
	**/
	void Update ()
    {
		if (!hasAuthority) return;
		transform.position = playerRef.transform.position;
		transform.rotation = playerRef.transform.rotation;
	}
    #endregion
}
