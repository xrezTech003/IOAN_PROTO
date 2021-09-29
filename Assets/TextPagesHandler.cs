using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextPagesHandler : MonoBehaviour
{
    private delegate void UpdateEvent();
    private UpdateEvent update;

    public GameObject page;

    private int index = 0;
    private TextMesh pageText;
    private LineRenderer box;
    private Color paramCol;
    private controllerController cont;

    private void Start()
    {
        Initialize(cont);
    }

    private void Update()
    {
        update?.Invoke();
    }

    public void Initialize(controllerController controller)
    {
        if (!page) page = gameObject.transform.Find("TextPage").gameObject;
        pageText = page.GetComponent<TextMesh>();
        box = page.transform.GetChild(0).GetComponent<LineRenderer>();
        paramCol = new Color();

        cont = controller;
        Disable();
    }

    public void SetParamPress(int param)
    {
        update = null;

        switch (param)
        {
            case 0:
                pageText.text = "Object lightcurve\nroot mean square(RMS)\nvalue is < 0.15";
                paramCol = Color.white;
                break;
            case 1:
                pageText.text = "Object Lomb-Scargle\nperiodogram\nsignal to noise ratio(SNR)\nvalue is > 12";
                paramCol = Color.cyan;
                break;
            case 2:
                pageText.text = "Astrometric pseudocolor\ncross-reference\nfrom GAIA DR2";
                paramCol = Color.magenta;
                break;
            case 3:
                pageText.text = "Confirmed or potential\nvariable object";
                paramCol = Color.yellow;
                break;
            default:
                break;
        }

        pageText.color = paramCol;
        box.material.SetColor("_Color", paramCol);
    }

    public void StartParamPressFade()
    {
        float totalFadeTime = 1.5f;

        update += () =>
        {
            float t = Time.deltaTime;
            totalFadeTime -= t;

            if (totalFadeTime >= 1.0f) return;
            if (totalFadeTime <= 0.0f) update = null;

            Color newCol = Color.Lerp(Color.clear, paramCol, totalFadeTime / 1.0f);
            pageText.color = newCol;
            box.material.SetColor("_Color", newCol);
        };
    }

    public void ManualSet(int mod = 0)
    {
        int numPages = 7;

        index += mod;
        if (index < 0) index = numPages - 1;
        else index = index % numPages;

        switch (index)
        {
            case 0:
                pageText.text = "Tap to preview object\ndata and sound";
                break;
            case 1:
                pageText.text = "Pick up and position\nobject and trigger to\nactivate and release\nits data and sound";
                break;
            case 2:
                pageText.text = "Tap active object\nto replay sound";
                break;
            case 3:
                pageText.text = "Double strike to\nfilter objects for\nRMS, variability,\npseudocolor, or SNR";
                break;
            case 4:
                pageText.text = "Pick up and move object\nwith trigger to view it\nor exchange with another\nplayer";
                break;
            case 5:
                pageText.text = "Scrub across\nmultiple objects\nfor data and\nsound preview";
                break;
            case 6:
                pageText.text = "Colored light tendrils\nand particles\nhighlight objects similar\nto the activated object";
                break;
            default:
                break;
        }

        pageText.color = Color.white;
        box.material.SetColor("_Color", Color.white);

        float totalFadeTime = 10f;
        update = () =>
        {
            float t = Time.deltaTime;
            totalFadeTime -= t;

            if (totalFadeTime <= 2.5f)
            {
                Color newCol = Color.white * (totalFadeTime / 2.5f);
                pageText.color = newCol;
                box.material.SetColor("_Color", newCol);
            }

            if (totalFadeTime <= 0.0f)
            {
                update = null;
                Disable();
                cont.OnCenterPress();
            }
        };
    }

    public void Shift(bool right)
    {
        if (right) ManualSet(1);
        else ManualSet(-1);
    }

    public void Disable()
    {
        index = 0;
        pageText.color = Color.clear;
        box.material.SetColor("_Color", Color.clear);
    }
}

#region OLD_CODE
/*
private delegate void UpdateEvent();
private UpdateEvent update;

public GameObject[] pages;

private int index = 0;
private float targetRot = 0.0f;
private readonly float speed = 1f;

private Color paramCol;

private void Start()
{
    Disable();
}

private void Update()
{
    update?.Invoke();
}

public void Set(int param)
{
    index = 0;
    TextMesh page = pages[index].GetComponent<TextMesh>();
    LineRenderer box = pages[index].transform.GetChild(0).GetComponent<LineRenderer>();

    switch (param)
    {
        case 0:
            page.text = "RMS\nRoot Mean\nSquare";
            paramCol = Color.white;
            break;
        case 1:
            page.text = "SNR\nSignal-to-Noise\nRatio";
            paramCol = Color.cyan;
            break;
        case 2:
            page.text = "AstroColor\nAstrometric Pseudocolor";
            paramCol = Color.magenta;
            break;
        case 3:
            page.text = "Variability\nTwinkling Star?";
            paramCol = Color.yellow;
            break;
        default:
            break;
    }

    targetRot = 0.0f;
    transform.localRotation = Quaternion.Euler(Vector3.zero);

    Color col = paramCol;
    col.a = 0.0f;

    if (update != null) return;
    update = () =>
    {
        col.a += speed * Time.deltaTime;
        page.color = col;
        box.material.SetColor("_Color", col);

        if (col.a >= 1.0f) update = null;
    };
}

public void Shift(bool clockwise)
{
    if (update != null) return;

    int j;
    float mod = 360.0f / pages.Length;
    if (clockwise)
    {
        targetRot = (targetRot + mod) % 360.0f;
        j = (index - 1 < 0) ? pages.Length - 1 : index - 1;
    }
    else
    {
        targetRot = (targetRot - mod < 0.0f) ? 360.0f - mod : targetRot - mod;
        j = (index + 1) % pages.Length;
    }

    TextMesh oldPage = pages[index].GetComponent<TextMesh>();
    LineRenderer oldBox = pages[index].transform.GetChild(0).GetComponent<LineRenderer>();
    TextMesh newPage = pages[j].GetComponent<TextMesh>();
    LineRenderer newBox = pages[j].transform.GetChild(0).GetComponent<LineRenderer>();

    update = () =>
    {
        float dist = Mathf.Min(Mathf.Abs(transform.localRotation.eulerAngles.y - targetRot),
                        Mathf.Abs(transform.localRotation.eulerAngles.y - (targetRot + 360.0f)));

        Color iCol = (index == 0) ? paramCol : Color.white;
        iCol.a = Mathf.Lerp(0.0f, 1.0f, dist / mod);
        oldPage.color = iCol;
        oldBox.material.SetColor("_Color", iCol);

        Color jCol = (j == 0) ? paramCol : Color.white;
        jCol.a = Mathf.Lerp(1.0f, 0.0f, dist / mod);
        newPage.color = jCol;
        newBox.material.SetColor("_Color", jCol);

        transform.Rotate(Vector3.up * speed * Time.deltaTime * mod * (clockwise ? 1.0f : -1.0f));

        if (dist < 2.5f)
        {
            oldPage.color = Color.clear;
            oldBox.material.SetColor("_Color", oldPage.color);
            newPage.color = (j == 0) ? paramCol : Color.white;
            newBox.material.SetColor("_Color", newPage.color);
            transform.localRotation = Quaternion.Euler(Vector3.up * targetRot);

            index = j;
            update = null;
        }
    };
}

public void Disable()
{
    update = null;

    foreach (GameObject p in pages)
    {
        TextMesh t = p.GetComponent<TextMesh>();
        t.color = Color.clear;

        LineRenderer l = p.transform.GetChild(0).GetComponent<LineRenderer>();
        l.material.SetColor("_Color", Color.clear);
    }
}
*/
#endregion