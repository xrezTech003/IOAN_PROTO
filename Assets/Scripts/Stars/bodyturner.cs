using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/**
<summary>
    CD : bodyturner
    Sets and resets gameObject position and updates the shadeSwapper bodyPos Array
</summary>
**/
public class bodyturner : NetworkBehaviour 
{
    #region PUBLIC_VAR
    /// <summary>
    ///     VD : tracker1
    ///     Set to object with "MainCamera" tag
    /// </summary>
    public GameObject tracker1;

    /// <summary>
    ///     VD : initRef
    ///     Set to Object with "init" tag
    /// </summary>
	public GameObject initRef;

    /// <summary>
    ///     VD : clothPlaneRef
    ///     Initialized but never used
    /// </summary>
	public GameObject clothPlaneRef;
    #endregion

    #region PRIVATE_VAR
    /// <summary>
    ///     VD : lateStartFlag
    ///     Boolean switch for triggering section of f:Update()
    /// </summary>
    private bool lateStartFlag;
    #endregion

    #region UNITY_FUNCTIONS
    /**
    <summary>
    FD : Start()
    Init v:lateStartFlag to false
    Set localPosition to zero vector
    clothPlaneRef is set to Object in world with clothPlane tag
    </summary>
    **/
	void Start()
	{
		lateStartFlag = false;
		transform.localPosition = Vector3.zero;
		clothPlaneRef = GameObject.FindGameObjectWithTag("clothPlane");
	}

    /**
    <summary>
    FD : Update()
    If gameObject has authority
        And if v:tracker1 isn't null
            Set gameObject transform data to v:tracker1 transform data
        And if v:lateStartFlag isn't true
            sets v:tracker1 to "MainCamera" tagged object in world
            sets v:lateStartFlag to true
        Makes gameObject look at Main Camera
    If v:initRef is null
        set v:initRef to "init" tagged object in world
    Else
        UpdateBodyPosArray var of shadeswapper component of v:initRef gameObject to position
    </summary>
    **/
    void Update()
    {
        if (hasAuthority)
        {
            if (tracker1)
            {
                transform.position = tracker1.transform.position;
                transform.rotation = tracker1.transform.rotation;
            }

            if (!lateStartFlag)
            {
                tracker1 = GameObject.FindGameObjectWithTag("MainCamera");
                lateStartFlag = true;
            }

            transform.LookAt(Camera.main.transform);
        }

        if (!initRef) initRef = GameObject.FindGameObjectWithTag("init");
        else initRef.GetComponent<shadeSwapper>().UpdateBodyPosArray(transform.position);
    }
    #endregion
}
