using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

/**
<summary>
    CD : canvasNewNet
    Update text values and other visual things I_A 
    ::: referenced by two scripts, a dead function in one, and that circles through newdbtester and back to a nulling function in here 
    ::: IV: NDH - My guess is this is dead but tied up a bit, ultimately all it will accomplish is activating and deactivating some loading frames, way too many refrences and variables, could be done way better
</summary>
**/
public class canvasNewNet : NetworkBehaviour
{
    #region PUBLIC_VAR
    /// <summary>
    ///     VD : playerID
    /// </summary>
    [SyncVar(hook = "OnChangePlayerID")]
    public int playerID;

    /// <summary>
    ///     VD Group : text strings
    ///     Members : text, text1, text2
    /// </summary>
    public string text;
    public string text1;
    public string text2;

    /// <summary>
    ///     VD Group : Colors
    ///     Members : line, line1, line2, panel, panel1, panel2, panel3, img4
    /// </summary>
    public Color line;
    public Color line1;
    public Color line2;
    public Color panel;
    public Color panel1;
    public Color panel2;
    public Color panel3;
    public Color img4;

    /// <summary>
    ///     VD : cursor
    /// </summary>
    public float cursor;

    /// <summary>
    ///     VD : idLabel
    /// </summary>
    public string idLabel;

    /// <summary>
    ///     VD Group : GameObjects
    ///     Members : t, t1, t2, l, l1, l2, p, p1, p2, p3, i4, c, idL
    /// </summary>
    public GameObject t;
    public GameObject t1;
    public GameObject t2;
    public GameObject l;
    public GameObject l1;
    public GameObject l2;
    public GameObject p;
    public GameObject p1;
    public GameObject p2;
    public GameObject p3;
    public GameObject i4;
    public GameObject c;
    public GameObject idL;

    /// <summary>
    ///     VD Group : loadingFrames
    ///     Members : loadingFrame, loadingFrame1, loadingFrame2
    ///     All set to false
    /// </summary>
    public bool loadingFrame = false;
    public bool loadingFrame1 = false;
    public bool loadingFrame2 = false;

    /// <summary>
    ///     VD : graphImgID
    /// </summary>
    public int graphImgID;

    /// <summary>
    ///     VD Group : Sprites
    ///     Members : graphImgList, graphImg[1-15]
    /// </summary>
    public Sprite[] graphImgList;
    public Sprite graphImg1;
    public Sprite graphImg2;
    public Sprite graphImg3;
    public Sprite graphImg4;
    public Sprite graphImg5;
    public Sprite graphImg6;
    public Sprite graphImg7;
    public Sprite graphImg8;
    public Sprite graphImg9;
    public Sprite graphImg10;
    public Sprite graphImg11;
    public Sprite graphImg12;
    public Sprite graphImg13;
    public Sprite graphImg14;
    public Sprite graphImg15;
    public Sprite graphImg16;

    /// <summary>
    ///     VD Group : cont GameObject
    ///     Members : rContObj, lContObj
    /// </summary>
    public GameObject rContObj;
    public GameObject lContObj;
    #endregion

    #region PRIVATE_VAR
    /// <summary>
    ///     VD : oscPacketBus
    /// </summary>
    private GameObject oscPacketBus;

    /// <summary>
    ///     VD Group : Data Arrays
    ///     Members : star4Labels, star4Data, star2Labels, star2Data, gaiaLabels, gaiaData
    /// </summary>
	private string[] star4Labels;
	private string[] star4Data;
	private string[] star2Labels;
	private string[] star2Data;
	private string[] gaiaLabels;
	private string[] gaiaData;

    /// <summary>
    ///     VD : idStr
    /// </summary>
	private string idStr;

    /// <summary>
    ///     VD : Alphabet
    ///     Alphanumeric string to hold all letters and digits
    /// </summary>
	private const string Alphabet = "abcdefghijklmnopqrstuvwyxzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    /// <summary>
    ///     VD : cTick
    /// </summary>
	private const float cTick = 0.006f;

    /// <summary>
    ///     VD : rand
    /// </summary>
	System.Random rand;

    /// <summary>
    ///     VD : loadCounter
    /// </summary>
	private int loadCounter = 90;

    /// <summary>
    ///     VD : loadTimer
    /// </summary>
	private float loadTimer = 0f;

    /// <summary>
    ///     VD : updateTicker
    /// </summary>
	private float updateTicker = 0f;

    /// <summary>
    ///     VD : startFlag
    /// </summary>
    [SyncVar]
	private bool startFlag = false;
    #endregion

    #region UNITY_FUNCTIONS
    /**
    <summary>
        FD : Start()
        Initialize most variables
    </summary>
    **/
    void Start()
    {
        //oscPacketBus = GameObject.FindGameObjectWithTag ("OSCPacketBus");
        graphImgList = new Sprite[16];
        graphImgList[0] = graphImg1;
        graphImgList[1] = graphImg2;
        graphImgList[2] = graphImg3;
        graphImgList[3] = graphImg4;
        graphImgList[4] = graphImg5;
        graphImgList[5] = graphImg6;
        graphImgList[6] = graphImg7;
        graphImgList[7] = graphImg8;
        graphImgList[8] = graphImg9;
        graphImgList[9] = graphImg10;
        graphImgList[10] = graphImg11;
        graphImgList[11] = graphImg12;
        graphImgList[12] = graphImg13;
        graphImgList[13] = graphImg14;
        graphImgList[14] = graphImg15;
        graphImgList[15] = graphImg16;
        i4.GetComponent<Image>().sprite = graphImgList[graphImgID];
        rand = new System.Random();
        RectTransform myRect = gameObject.GetComponentInChildren<RectTransform>();
        //myRect.localPosition = new Vector3 (0f, 0.076f, 0.0185f);
        //myRect.localRotation = Quaternion.Euler (45f, 180f, 0f);
        //myRect.localScale = new Vector3 (0.000076f,0.000076f,0.000076f);
        myRect.localPosition = new Vector3(0f, 0.08f, -0.02f);
        myRect.localRotation = Quaternion.Euler(45f, 0f, 0f);
        myRect.localScale = new Vector3(0.00008f, 0.00008f, 0.00008f);
        text = "";
        text1 = "";
        text2 = "";
        line = new Color(1f, 1f, 1f, 0f);
        line1 = new Color(1f, 1f, 1f, 0f);
        line2 = new Color(1f, 1f, 1f, 0f);
        panel = new Color(1f, 1f, 1f, 0f);
        panel1 = new Color(1f, 1f, 1f, 0f);
        panel2 = new Color(1f, 1f, 1f, 0f);
        panel3 = new Color(1f, 1f, 1f, 0f);
        img4 = new Color(1f, 1f, 1f, 0f);
        cursor = 0f;
        idLabel = "";
    }

    /**
    <summary>
        FD : Update()
        If v:startFlag is false and the gameObject has authority
            Set v:lContObj to GameObject with tag "controlLeft"
            Set v:lContObj component newdbtester var newCanvasReference to gameObject
        Decrement v:updateTicker by deltaTime
        If v:updateTicker < 0
            Set v:updateTicker to .02
            If v:loadingFrame is true: Set v:text to string of 23 random alphanumeric strings of random size between 1 and 20 seperated by newline
            Else if v:loadingFrame2 is true and v:cursor < .33: Set v:cursor to .33
            If v:loadingFrame1 is true: Set v:text1 to string of 26 random alphanumeric strings of random size seperated by newline
            Else if v:loadingFrame2 is true and v:cursor < .67: Set v:cursor to .67
            If v:loadingFrame2 is true: Increment v:cursor by v:cTick and Set v:text2 to string of 31 random alphanumeric strings of random size seperated by newline
        If v:loadCounter < 90
            Decrement v:loadTimer by deltaTime
            If v:loadTimer < .005
                Set v:loadTimer to 0
                If v:loadCounter equals 0 : Set v:loadingFrame, v:text, v:idLabel to null
                Else if v:loadCounter equals 23 : Set v:loadingFrame1 and v:text1 to null
                Else if v:loadCounter equals 49 : Set v:loadingFrame2 and v:text2 to null
                If v:loadCounter < v:idStr size : concatenate v:idStr at v:loadCounter to idLabel
        Set all GameObject values
        Return if gameObject doesn't have authority
        If v:lContObj isn't null: set transform data to v:lContObj transform data
    </summary>
    **/
    void Update()
    {
        if (!startFlag) /// IV : if (!startFlag && hasAuthority)
        {
            if (hasAuthority)
            {
                //Debug.Log ("start canvas");
                lContObj = GameObject.FindGameObjectWithTag("controlLeft");
                lContObj.GetComponent<newdbtester>().newCanvasReference = this.gameObject; /// IV : gameObject
                startFlag = true;
            }
        }

        updateTicker -= Time.deltaTime;

        if (updateTicker < 0)
        {
            updateTicker = 0.02f;

            if (loadingFrame)
            {
                text = "";
                for (int i = 0; i < 23; i++)
                {
                    int r = rand.Next(1, 20);
                    text = text + GenerateString(r) + "\n";
                }
            }
            else if (loadingFrame2)
            {
                if (cursor < 0.33f) cursor = 0.33f;
            }

            if (loadingFrame1)
            {
                text1 = "";
                for (int i = 0; i < 26; i++)
                {
                    int r = rand.Next(1, 20);
                    text1 = text1 + GenerateString(r) + "\n";
                }
            }
            else if (loadingFrame2)
            {
                if (cursor < 0.67f) cursor = 0.67f;
            }

            if (loadingFrame2)
            {
                cursor += cTick;
                text2 = "";
                for (int i = 0; i < 31; i++)
                {
                    int r = rand.Next(1, 20);
                    text2 = text2 + GenerateString(r) + "\n";
                }
            }
        }

        if (loadCounter < 90)
        {
            loadTimer -= Time.deltaTime;
            if (loadTimer < 0.005f)
            {
                loadTimer = 0f;
                if (loadCounter == 0)
                {
                    loadingFrame = false;
                    text = "";
                    idLabel = "";
                }
                else if (loadCounter == 23)
                {
                    loadingFrame1 = false;
                    text1 = "";
                }
                else if (loadCounter == 49)
                {
                    loadingFrame2 = false;
                    text2 = "";
                }

                if (loadCounter < idStr.Length) idLabel += idStr[loadCounter];

                if (loadCounter < 23)
                {
                    int i = loadCounter;
                    text = text + "<color=#cccccc>" + star4Labels[i] + " - </color><color=#ffffff>" + star4Data[i].Replace("_", " ") + "</color>\n";
                    line = new Color(1.0f, 1.0f, 1.0f, (float)i * .01023f);
                    loadCounter++;
                }
                else if (loadCounter >= 23 && loadCounter < 49)
                {
                    int i = loadCounter - 23;
                    text1 = text1 + "<color=#cccccc>" + star2Labels[i] + " - </color><color=#ffffff>" + star2Data[i].Replace("_", " ") + "</color>\n";
                    panel = new Color(0.0f, 1.0f, 1.0f, (float)i * (1f / 25f));
                    line1 = new Color(1.0f, 1.0f, 1.0f, (float)i * .00905f);
                    loadCounter++;
                }
                else if (loadCounter >= 49 && loadCounter < 80)
                {
                    int i = loadCounter - 49;
                    text2 = text2 + "<color=#cccccc>" + gaiaLabels[i] + " - </color><color=#ffffff>" + gaiaData[i].Replace("_", " ") + "</color>\n";
                    panel1 = new Color(1.0f, 0.0f, 1.0f, (float)i * (1f / 30f));
                    line2 = new Color(1.0f, 1.0f, 1.0f, (float)i * .00759f);
                    cursor += cTick;
                    loadCounter++;
                }
                else if (loadCounter >= 80 && loadCounter < 90)
                {
                    int i = loadCounter - 80;
                    panel2 = new Color(1.0f, 1.0f, 0.0f, (float)i * (1f / 9f));
                    loadCounter++;
                }

                if (loadCounter == 90)
                {
                    panel3 = new Color(0.0f, 0.0f, 1.0f, 1.0f);
                    img4 = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                    line = new Color(1.0f, 1.0f, 1.0f, 0.0f);
                    line1 = new Color(1.0f, 1.0f, 1.0f, 0.0f);
                    line2 = new Color(1.0f, 1.0f, 1.0f, 0.0f);
                    cursor = 0.0f;
                    //Debug.Log ("test1");
                    if (hasAuthority) { /*						CmdSendOSC (playerID);*/ }
                }
            }
        }

        t.GetComponent<Text>().text = text;
        t1.GetComponent<Text>().text = text1;
        t2.GetComponent<Text>().text = text2;
        l.GetComponent<Image>().color = line;
        l1.GetComponent<Image>().color = line1;
        l2.GetComponent<Image>().color = line2;
        p.GetComponent<Image>().color = panel;
        p1.GetComponent<Image>().color = panel1;
        p2.GetComponent<Image>().color = panel2;
        p3.GetComponent<Image>().color = panel3;
        i4.GetComponent<Image>().color = img4;
        c.GetComponent<Image>().fillAmount = cursor;
        idL.GetComponent<Text>().text = idLabel;

        if (!hasAuthority) return;

        if (lContObj)
        {
            transform.position = lContObj.transform.position;
            transform.rotation = lContObj.transform.rotation;
        }
        else
        {
            //CmdSetAuth();
            Debug.Log("update canvas");
            lContObj = GameObject.FindGameObjectWithTag("controlLeft");
            lContObj.GetComponent<newdbtester>().newCanvasReference = this.gameObject;
        }
    }
    #endregion

    #region PUBLIC_FUNC
    /**
    <summary>
        FD : loadIDStuff(string[], string[], string[], string[], string[], string[], int, string)
        Calls f:CmdLoadIDStuff with Data Array group of variables
        <param name="star4Labels"></param>
        <param name="star4Data"></param>
        <param name="star2Labels"></param>
        <param name="star2Data"></param>
        <param name="gaiaLabels"></param>
        <param name="gaiaData"></param>
        <param name="gID"></param>
        <param name="idStr"></param>
    </summary>
    **/
    public void loadIDStuff(string[] star4Labels, string[] star4Data, string[] star2Labels, string[] star2Data, string[] gaiaLabels, string[] gaiaData, int gID, string idStr)
    {
        CmdLoadIDStuff(star4Labels, star4Data, star2Labels, star2Data, gaiaLabels, gaiaData, gID, idStr);
    }

    /**
    <summary>
        FD : startLoadingFrames
        Calls f:CmdstartLoadingFrames()
    </summary>
    **/
    public void startLoadingFrames()
    {
        CmdstartLoadingFrames();
    }

    /**
    <summary>
        FD : IDStopper()
        Calls f:CMDIDStopper()
    </summary>
    **/
    public void IDStopper()
    {
        CmdIDStopper();
    }

    /**
    <summary>
        FD : GenerateString(int)
        Creates a new string, of given size, of random characters from the Alphabet var
        <param name="size"></param>
    </summary>
    **/
    public string GenerateString(int size)
    {
        char[] chars = new char[size];

        for (int i = 0; i < size; i++) chars[i] = Alphabet[rand.Next(Alphabet.Length)];

        return new string(chars);
    }
    #endregion

    #region PRIVATE_FUNC
    /**
    <summary>
        FD : OnChangePlayerID(int)
        Sets v:playerID to inID
        <param name="inID"></param>
    </summary>
    **/
    void OnChangePlayerID(int oldID, int inID)
    {
        playerID = inID;
    }

    /**
    <summary>
        FD : CmdLoadIDStuff(string[], string[], string[], string[], string[], string[], int, string)
        Calls f:RpcLoadIDStuff with Data Arrays Members and other var
        <param name="star4Labels"></param>
        <param name="star4Data"></param>
        <param name="star2Labels"></param>
        <param name="star2Data"></param>
        <param name="gaiaLabels"></param>
        <param name="gaiaData"></param>
        <param name="gID"></param>
        <param name="idStr"></param>
    </summary>
    **/
    [Command]
    void CmdLoadIDStuff(string[] star4Labels, string[] star4Data, string[] star2Labels, string[] star2Data, string[] gaiaLabels, string[] gaiaData, int gID, string idStr)
    {
        RpcLoadIDStuff(star4Labels, star4Data, star2Labels, star2Data, gaiaLabels, gaiaData, gID, idStr);
    }

    /**
    <summary>
        FD : RpcLoadIDStuff(string[], string[], string[], string[], string[], string[], int, string)
        Sets Data Array members with input values
        Sets graphImgList Array with each GraphImg# var
        Sets i4 sprite, idStr, loadCounter, and loadTimer
        <param name="star4LabelsI"></param>
        <param name="star4DataI"></param>
        <param name="star2LabelsI"></param>
        <param name="star2DataI"></param>
        <param name="gaiaLabelsI"></param>
        <param name="gaiaDataI"></param>
        <param name="gID"></param>
        <param name="idStrI"></param>
    </summary>
    **/
    [ClientRpc]
    void RpcLoadIDStuff(string[] star4LabelsI, string[] star4DataI, string[] star2LabelsI, string[] star2DataI, string[] gaiaLabelsI, string[] gaiaDataI, int gID, string idStrI)
    {
        star4Labels = star4LabelsI;
        star4Data = star4DataI;
        star2Labels = star2LabelsI;
        star2Data = star2DataI;
        gaiaLabels = gaiaLabelsI;
        gaiaData = gaiaDataI;
        graphImgID = gID;

        if (!graphImgList[0])
        {
            graphImgList = new Sprite[16];
            graphImgList[0] = graphImg1;
            graphImgList[1] = graphImg2;
            graphImgList[2] = graphImg3;
            graphImgList[3] = graphImg4;
            graphImgList[4] = graphImg5;
            graphImgList[5] = graphImg6;
            graphImgList[6] = graphImg7;
            graphImgList[7] = graphImg8;
            graphImgList[8] = graphImg9;
            graphImgList[9] = graphImg10;
            graphImgList[10] = graphImg11;
            graphImgList[11] = graphImg12;
            graphImgList[12] = graphImg13;
            graphImgList[13] = graphImg14;
            graphImgList[14] = graphImg15;
            graphImgList[15] = graphImg16;
        }

        i4.GetComponent<Image>().sprite = graphImgList[graphImgID];
        idStr = idStrI;
        loadCounter = 0;
        loadTimer = 0f;
    }

    /**
    <summary>
        FD : CmdstartLoadingFrames()
        Calls f:RpcstartLoadingFrames()
    </summary>
    **/
    [Command]
    void CmdstartLoadingFrames()
    {
        RpcstartLoadingFrames();
    }

    /**
    <summary>
        FD : RpcstartLoadingFrames()
        Sets loadingFrames group members to true
    </summary>
    **/
    [ClientRpc]
    void RpcstartLoadingFrames()
    {
        loadingFrame = true;
        loadingFrame1 = true;
        loadingFrame2 = true;
    }

    /**
    <summary>
        FD : CmdIDStopper()
        Calls f:RPCIDStopper()
    </summary>
    **/
    [Command]
    void CmdIDStopper()
    {
        RpcIDStopper();
    }

    /**
    <summary>
        FD : RpcIDStopper()
        Resets all variables to their version of null
    </summary>
    **/
    [ClientRpc]
    void RpcIDStopper()
    {
        loadingFrame = false;
        loadingFrame1 = false;
        loadingFrame2 = false;
        text = "";
        text1 = "";
        text2 = "";
        line = new Color(1f, 1f, 1f, 0f);
        line1 = new Color(1f, 1f, 1f, 0f);
        line2 = new Color(1f, 1f, 1f, 0f);
        panel = new Color(1f, 1f, 1f, 0f);
        panel1 = new Color(1f, 1f, 1f, 0f);
        panel2 = new Color(1f, 1f, 1f, 0f);
        panel3 = new Color(1f, 1f, 1f, 0f);
        img4 = new Color(1f, 1f, 1f, 0f);
        cursor = 0f;
        idLabel = "";
        loadCounter = 90;
    }
    #endregion

    #region COMMENTED_CODE
    //public GameObject playerRef;

    //private bool authorityFlag = false;

    /*
	[Command]
	void CmdSendOSC(int inID){
		oscPacketBus.GetComponent<OSCPacketBus> ().ifaceDoneTriggerStart (inID);
	}
	*/
    #endregion
}
