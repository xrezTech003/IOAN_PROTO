using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
<summary>
	CD : controllerController
	Sets touchpad buttons based on what is pressed I_A ::: IV: This could be elsewhere NDH
</summary>
**/
public class controllerController : MonoBehaviour
{
    private delegate void UpdateEvent();
    private UpdateEvent update;

    public delegate void ButtonEvent(int i);
    public ButtonEvent OnUpPress, OnDownPress, OnRightPress, OnLeftPress, OnRelease;

    #region PUBLIC_VAR
    [Header("Misc")]
	public int padStatus = 0;

	public float unpressedScale = 1.25f;
    public float halfpressedScale = 1.0f;
    public float fullpressedScale = 0.75f;

	public GameObject trigger;

    public static SteamVR_Controller.Device deviceA;
    public static SteamVR_Controller.Device deviceB;

    public iOANHeteroPlayer player;

    [Header("TouchPad Mats")]
    public Material paramTouchMat;
    public Material menuTouchMat;
    public Material questionMat;
    public Material crossMat;

    [Header("Center Sphere")]
    public GameObject centerSphere;
    public Material[] mats;
    public Material matNull;

    [Header("Text Pages")]
    public GameObject centerButton;
    public TextPagesHandler textPages;
    #endregion

    #region PRIVATE_VAR
    private bool useCenterButton = true;
    private GameObject touchpadGroup;
    private GameObject[] q = new GameObject[4];
    private bool inNavigationMode = false;
	#endregion

	#region UNITY_FUNC
	/**
	<summary>
		FD : Start()
		Set q GameObjects to children of this gameObject
		Assign all q GameObjects localScale
	</summary>
	**/
	void Start()
	{
        textPages.Initialize(this);

		touchpadGroup = transform.Find("touchpad").gameObject;

        for (int i = 0; i < 4; i++)
        {
            q[i] = transform.Find("touchpad/q" + (i + 1)).gameObject;
            q[i].transform.localScale = new Vector3(1, unpressedScale, 1);
            q[i].GetComponent<MeshRenderer>().material = paramTouchMat;
        }

        useCenterButton = Config.Instance.Data.controllerTextFlag;
        if (useCenterButton) centerButton.GetComponent<MeshRenderer>().material = questionMat;
        else centerButton.SetActive(false);

        OnUpPress = ParamPress;
        OnDownPress = ParamPress;
        OnRightPress = ParamPress;
        OnLeftPress = ParamPress;
        OnRelease = ParamRelease;

        OnUpPress?.Invoke(0);
    }

	/**
	<summary>
		FD : Update()
		Act depending on what button is down
	</summary>
	**/
	void Update()
	{
        /*
        if (deviceA == null || deviceB == null) return;

        bool aPress = deviceA.GetPressDown(SteamVR_Controller.ButtonMask.Touchpad);
        bool bPress = deviceB.GetPressDown(SteamVR_Controller.ButtonMask.Touchpad);

        bool aRelease = deviceA.GetPressUp(SteamVR_Controller.ButtonMask.Touchpad);
        bool bRelease = deviceB.GetPressUp(SteamVR_Controller.ButtonMask.Touchpad);

        if (aPress || bPress)
        {
            SteamVR_Controller.Device device = (aPress) ? deviceA : deviceB;
            Vector2 touchpad = device.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0);

            if (Mathf.Abs(touchpad.x) < 0.25f && Mathf.Abs(touchpad.y) < 0.25f) OnCenterPress();
            else if (touchpad.y > touchpad.x && touchpad.y > -touchpad.x) OnUpPress?.Invoke(0);
            else if (touchpad.y < touchpad.x && touchpad.y < -touchpad.x) OnDownPress?.Invoke(2);
            else if (touchpad.y < touchpad.x && touchpad.y > -touchpad.x) OnRightPress?.Invoke(1);
            else if (touchpad.y > touchpad.x && touchpad.y < -touchpad.x) OnLeftPress?.Invoke(3);
        }
        else if (aRelease || bRelease) OnRelease?.Invoke(0);
        */

        update?.Invoke();
    }
    #endregion

    #region PUBLIC_FUNC
    /**
    <summary>
        FD : SetTriggerRotation(float)
        Set trigger transform to xRotation
        <param name="xRotation"/>
    </summary>
    **/
    public void SetTriggerRotation(float xRotation)
    {
        // -16 to 0 degrees seems like the right range
        trigger.transform.localEulerAngles = Vector3.right * xRotation;
    }
    #endregion

    #region PRIVATE_FUNC
    public void OnCenterPress()
    {
        //TEXT PAGES
        if (useCenterButton)
        {
            inNavigationMode = !inNavigationMode;
            if (inNavigationMode)
            {
                OnUpPress = null;
                OnDownPress = null;
                OnRightPress = PagePress;
                OnLeftPress = PagePress;
                OnRelease = (i) => OnRelease = PageRelease;

                //Debug.Log("SENDING SIGNAL TO PLAYER");
                SetCenterSphere(-1);

                for(int i = 0; i < 4; i++)
                    q[i].GetComponent<MeshRenderer>().material = menuTouchMat;

                q[0].transform.localScale = new Vector3(1, halfpressedScale, 1);
                q[2].transform.localScale = new Vector3(1, halfpressedScale, 1);

                q[1].transform.localScale = new Vector3(1, unpressedScale, 1);
                q[3].transform.localScale = new Vector3(1, unpressedScale, 1);

                centerButton.GetComponent<MeshRenderer>().material = crossMat;

                textPages.ManualSet();
            }
            else
            {
                OnUpPress = ParamPress;
                OnDownPress = ParamPress;
                OnRightPress = ParamPress;
                OnLeftPress = ParamPress;
                OnRelease = (i) => OnRelease = ParamRelease;

                for (int i = 0; i < 4; i++)
                {
                    q[i].GetComponent<MeshRenderer>().material = paramTouchMat;
                    q[i].transform.localScale = new Vector3(1, unpressedScale, 1);
                }
                centerButton.GetComponent<MeshRenderer>().material = questionMat;

                ParamPress(padStatus);
                textPages.Disable();
            }
        }
    }

    public void SetCenterSphere(int i)
    {
        if (centerSphere && centerSphere.activeSelf)
        {
            if (i < 0) centerSphere.GetComponent<MeshRenderer>().material = matNull;
            else centerSphere.GetComponent<MeshRenderer>().material = mats[i];
        }
    }

    #region PARAM_PRESSES
    private void ParamPress(int i)
    {
        q[padStatus].transform.localScale = new Vector3(1, unpressedScale, 1);
        padStatus = i;
        q[padStatus].transform.localScale = new Vector3(1, fullpressedScale, 1);

        //Debug.Log("SENDING SIGNAL TO PLAYER");
        SetCenterSphere(i);
        textPages.SetParamPress(padStatus);
    }

    private void ParamRelease(int i)
    {
        q[padStatus].transform.localScale = new Vector3(1, halfpressedScale, 1);
        textPages.StartParamPressFade();
    }
    #endregion

    #region TEXTPAGE_PRESSES
    private bool rightPage;

    private void PagePress(int b)
    {
        rightPage = (b == 1);
        textPages.Shift((rightPage) ? true : false);

        if (rightPage) q[1].transform.localScale = new Vector3(1, halfpressedScale, 1);
        else q[3].transform.localScale = new Vector3(1, halfpressedScale, 1);
    }

    private void PageRelease(int b)
    {
        if (rightPage) q[1].transform.localScale = new Vector3(1, unpressedScale, 1);
        else q[3].transform.localScale = new Vector3(1, unpressedScale, 1);
    }
    #endregion
    #endregion
}

#region OLD_CODE
/*
[Header("Hovering Text")]
public bool displayText;
public TextMesh textBox;

[Header("Cube")]
public GameObject centerCube;
private bool cubeExpanded = false;
*/
//HOVERING TEXT
/*
switch (padStatus)
{
    case 0:
        SetText("RMS\nRoot Mean\nSquare");
        break;
    case 1:
        SetText("SNR\nSignal-to-Noise\nRatio");
        break;
    case 2:
        SetText("AstroColor\nAstrometric Pseudocolor");
        break;
    case 3:
        SetText("Variability\nTwinkling Star?");
        break;
    default:
        break;
}
*/

//CUBE
/*
if (centerCube && centerCube.activeSelf && !isExpanding)
{
    isExpanding = true;
    update -= Expansion;
    update += Expansion;
    eStartTime = Time.time;
}
*/
/*
#region CUBE
private bool isExpanding = false;
private float eStartTime;
private void Expansion()
{
    Vector3 smallPos = new Vector3(0, -0.0315f, 0.0118f);
    Vector3 largePos = new Vector3(0, 0.02f, 0.0925f);

    Quaternion start = Quaternion.Euler(57.5f, 0, 0);
    Quaternion half = start * Quaternion.AngleAxis(180.0f, Vector3.up);
    Quaternion dest = start * Quaternion.AngleAxis(360.0f, Vector3.up);

    float smallScale = 0.02f;
    float largeScale = 0.1f;

    float dur = 1.0f;
    float p = (Time.time - eStartTime) / dur;

    if (cubeExpanded)
    {
        centerCube.transform.localPosition = Vector3.Lerp(largePos, smallPos, p);
        centerCube.transform.localScale = Vector3.one * Mathf.Lerp(largeScale, smallScale, p);
        centerCube.transform.localRotation = Quaternion.Lerp(start, half, p);

        if (p >= 1.0f)
        {
            cubeExpanded = false;
            update -= Expansion;
            isExpanding = false;
        }
    }
    else
    {
        centerCube.transform.localPosition = Vector3.Lerp(smallPos, largePos, p);
        centerCube.transform.localScale = Vector3.one * Mathf.Lerp(smallScale, largeScale, p);
        centerCube.transform.localRotation = Quaternion.Lerp(start, half, p);

        if (p >= 1.0f)
        {
            cubeExpanded = true;
            update -= Expansion;
            isExpanding = false;
        }
    }
}
#endregion

#region SPHERE
#endregion

#region HOVERING TEXT
/// <summary>
///     FD : Will Start Text fade out on controller
/// </summary>
/// <param name="s"></param>
private void SetText(string s)
{
    if (!displayText) return;

    textBox.text = s;
    textBox.color = new Color(1, 1, 1, 1);
    update -= FadeOutText;
    update += FadeOutText;
}

/// <summary>
///     FD : Function for fading out text
/// </summary>
private void FadeOutText()
{
    if (textBox.color.a <= 0.0f) update -= FadeOutText;
    else textBox.color = new Color(1, 1, 1, textBox.color.a - 0.01f);
}
#endregion
    */
#endregion