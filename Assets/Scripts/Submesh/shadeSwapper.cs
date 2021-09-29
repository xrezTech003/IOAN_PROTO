using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
//using UnityOSC;

/// <summary>
/// CD: Heavy lifting class for giving the stars their appearance, speaks with many classes and shaders to get this hard work done ::: NDH thinks it deserves a more fitting name, like, LordCommanderofVisuals.cs
/// </summary>
public class shadeSwapper : MonoBehaviour
{
    public enum HighLightType
    {
        LEFT = 0,
        RIGHT,
        RAY_CAST // make sure RAY_CAST IS LAST
    };
    
    #region PUBLIC_VAR
    /// <summary>
    /// VD: Primary material for High Quality Stars
    /// </summary>
    [Tooltip("Primary material for High Quality Stars")]
    public Material hqMat;

    /// <summary>
    /// VD : Primary material for Medium Quality Stars
    /// </summary>
    [Tooltip("Primary material for Medium Quality Stars")]
    public Material mqMat;

    /// <summary>
    /// VD: Primary material for Low Quality Stars
    /// </summary>
    [Tooltip("Primary material for Medium Quality Stars")]
    public Material lowMat;

    /// <summary>
    /// VD: Engine bock for editing the changing appearance and location of stars 
    /// </summary>
    public MaterialPropertyBlock mMat;

    /// <summary>
    /// Primary shader for stars inside the bounds of the interactive region
    /// </summary>
    [Tooltip("Primary shader for stars inside the bounds of the interactive region")]
    public Shader hqShader;

    /// <summary>
    /// Primary shader for stars between inner and outer bounds
    /// </summary>
    [Tooltip("Primary shader for stars between inner and outer bounds")]
    public Shader mqShader;

    /// <summary>
    /// Primary shader for stars outside the bounds of the interactive region
    /// </summary>
    [Tooltip("Primary shader for stars outside the bounds of the interactive region")]
    public Shader lowShader;

    /// <summary>
    /// Vertical Bounds of cloth
    /// </summary>
    public Vector4 vertBounds = Vector4.zero;
    #endregion

    #region PRIVATE_VAR
    private Vector3[] lControllerPos = new Vector3[4];
    private float[] controllerHeldCDTimer = new float[4];
    private Vector3[] bodyPosList = new Vector3[6];
    private int playerIncrementor = 0;
    private int bodyPosIncrementor;

    /// <summary>
    /// Cloth Y-Position
    /// </summary>
    private float clothHeight = 0f;

    /// <summary>
    /// List for wave epicenter
    /// </summary>
    private Vector3[] wavPosList = new Vector3[8];

    /// <summary>
    /// Radii of each wave
    /// </summary>
    private float[] wavRadList = new float[8];

    /// <summary>
    /// Parameter used to spawn each wave
    /// </summary>
    private int[] wavParamList = new int[8];

    /// <summary>
    /// Intensity of each wave
    /// </summary>
    private float[] wavIntensityList = new float[8];

    /// <summary>
    /// Number of obscured stars
    /// </summary>
    private const int NUM_OBSCURE = 60;

    /// <summary>
    /// List for obscured stars
    /// </summary>
    private List<Vector2> obscurePosList = new List<Vector2>(NUM_OBSCURE);

    /// <summary>
    /// Number of highlighted stars
    /// </summary>
    private const int NUM_HIGHLIGHT = 60;

    /// <summary>
    /// List of highlighted stars
    /// </summary>
    private Vector2[] highlightPosList = new Vector2[NUM_HIGHLIGHT];

    /// <summary>
    /// Highlight value for each star
    /// </summary>
    private float[] highlightValueList = new float[NUM_HIGHLIGHT];

    /// <summary>
    /// Mesh renderers loaded
    /// </summary>
    private bool subMeshRenderersLoaded = false;

    /// <summary>
    /// Flag for late start activations
    /// </summary>
    private bool lateStartFlag = false;

    private ComputeBuffer highlightPosBuff;
    private ComputeBuffer highlightValBuff;
    private ComputeBuffer obscurePosBuff;
    private ComputeBuffer bodyPosBuff;

    private ComputeBuffer wavPosBuff;
    private ComputeBuffer wavRadBuff;
    private ComputeBuffer wavIntBuff;
    private ComputeBuffer wavParamBuff;
    #endregion

    public List<GameObject> NonStarObjects = new List<GameObject>();

    private Initializer init;
    private List<Vector4> uvs = new List<Vector4>();
    private List<Vector4> uvBs = new List<Vector4>();

    /// <summary>
    /// FD: Initialise all the variables, mostly to zero, bodyturner to 9999
    /// </summary>
    void Start()
    {
        init = gameObject.GetComponent<Initializer>();

        //Set Correct Shaders
        hqMat.shader = hqShader;
        mqMat.shader = mqShader;
        lowMat.shader = lowShader;

        mMat = new MaterialPropertyBlock();

        for (int i = 0; i < 8; i++)
        {
            wavPosList[i] = Vector3.zero;
            wavRadList[i] = 0f;
            wavIntensityList[i] = 0f;
            wavParamList[i] = 0;
        }

        for (int i = 0; i < 6; i++)
            bodyPosList[i] = Vector3.one * 99999f;

        for (int i = 0; i < 4; i++)
        {
            controllerHeldCDTimer[i] = 0f;
            lControllerPos[i] = Vector3.zero;
        }

        for (int i = 0; i < NUM_HIGHLIGHT; i++)
        {
            highlightPosList[i] = Vector2.zero;
            highlightValueList[i] = 0.0f;
        }

        highlightPosBuff = new ComputeBuffer(NUM_HIGHLIGHT, 8, ComputeBufferType.Default);
        highlightValBuff = new ComputeBuffer(NUM_HIGHLIGHT, 4, ComputeBufferType.Default);
        obscurePosBuff = new ComputeBuffer(NUM_OBSCURE, 8, ComputeBufferType.Default);
        bodyPosBuff = new ComputeBuffer(6, 12, ComputeBufferType.Default);

        wavPosBuff = new ComputeBuffer(8, 12, ComputeBufferType.Default);
        wavRadBuff = new ComputeBuffer(8, 4, ComputeBufferType.Default);
        wavIntBuff = new ComputeBuffer(8, 4, ComputeBufferType.Default);
        wavParamBuff = new ComputeBuffer(8, 4, ComputeBufferType.Default);

        Shader.SetGlobalBuffer("highlightPosBuff", highlightPosBuff);
        Shader.SetGlobalBuffer("highlightValBuff", highlightValBuff);
        Shader.SetGlobalBuffer("obscurePosBuff", obscurePosBuff);
        Shader.SetGlobalBuffer("bodyPosBuff", bodyPosBuff);

        Shader.SetGlobalBuffer("wavPosBuff", wavPosBuff);
        Shader.SetGlobalBuffer("wavRadBuff", wavRadBuff);
        Shader.SetGlobalBuffer("wavIntBuff", wavIntBuff);
        Shader.SetGlobalBuffer("wavParamBuff", wavParamBuff);
    }

    /// <summary>
    /// GS: FD: Due to this being called by activatedStar I assume this is the bit that makes a star go away when you "activate" it
    /// </summary>
    /// <param name="inID">star to deactivate</param>
    /// <param name="duration">duration for star to say gone</param>
    public void AddObscurePos(int inID)
    {
        string id = inID.ToString();

        float x = (inID > 0) ? Convert.ToSingle(id.Substring(0, 4)) : 0f;
        float y = (inID > 0) ? Convert.ToSingle(id.Substring(4, 4)) : 0f;

        if (obscurePosList.Count() >= NUM_OBSCURE) obscurePosList.Remove(obscurePosList[0]);

        obscurePosList.Add(new Vector2(x, y));
    }

    public void RemoveObscurePos(int inID)
    {
        if (inID == 0) return;

        string id = inID.ToString();

        float x = Convert.ToSingle(id.Substring(0, 4));
        float y = Convert.ToSingle(id.Substring(4, 4));

        obscurePosList.Remove(new Vector2(x, y));
    }

    /// <summary>
    /// GS: FD: called by TapDetector, reolors/reshapes the starmesh in respons to the tap
    /// </summary>
    /// <param name="inID"> starID (coordinate to adjust) </param>
    /// <param name="playerIndex"> (0, 1, or 2) </param>
    /// <param name="type"> Enum = (LEFT, RIGHT, RAYCAST) </param>
    /// <remarks> inID needs to be a uint </remarks>
    public void UpdateHighlightPos(int inID)
    {
        string id = inID.ToString();
        Vector2 vecID = new Vector2(Convert.ToSingle(id.Substring(0, 4)), Convert.ToSingle(id.Substring(4, 4)));

        int index = -1;
        for (int i = 0; i < NUM_HIGHLIGHT; i++)
        {
            if (highlightPosList[i] == vecID)
            {
                index = i;
                break;
            }
        }

        if (index == -1)
        {
            for (int i = 0; i < NUM_HIGHLIGHT; i++)
            {
                if (highlightPosList[i] == Vector2.zero)
                {
                    highlightPosList[i] = vecID;
                    highlightValueList[i] = 1.0f;

                    break;
                }
            }
        }
        else highlightValueList[index] = 1.0f;
    }

    public void UpdatePlayerPosition(Vector3 pos)
    {
        lControllerPos[playerIncrementor++] = pos;
    }

    /// <summary>
    /// GS: FD: This thin handles the wave that happens when you slam both hands
    /// </summary>
    /// <param name="initPos">Where the cloth got slammed</param>
    /// <param name="intensity">HOw hard it got slammed</param>
    /// <param name="inPadStat">Colour selected on thd left controller circle pad</param>
    public void SpawnWave(Vector3 initPos, float intensity, int inPadStat)
    {
        int minIndex = Array.IndexOf(wavRadList, wavRadList.Max());

        for (int i = 0; i < 8; i++)
        {
            if (wavRadList[i] == 0f)
            {
                minIndex = i;
                break;
            }
        }

        wavPosList[minIndex] = initPos;
        wavRadList[minIndex] = 0.001f;
        wavParamList[minIndex] = inPadStat;
        wavIntensityList[minIndex] = intensity;
    }

    /// <summary>
    /// FD: IV: This is likely garbage. Has a few references tying them in to the compiler, but I'd wager their old
    /// </summary>
    /// <param name="bodyPosIn"></param>
    public void UpdateBodyPosArray(Vector3 bodyPosIn)
    {
        bodyPosList[bodyPosIncrementor++] = bodyPosIn;
    }

   /// <summary>
   /// Heavy lifting done here - too many functions to summarize but I'll try
   ///  ::: updates vertex's based on input from many class's, moves, hides, and manipulates stars 
   ///  ::: recieves input for editing from slams, taps, and grabs
   ///  ::: also helps subMeshLoader set the correct rendermaterial based on values in globalloadskip for initializer to then set the interactive submesh
   /// </summary>
    void Update()
    {
        ///<remarks>Load the v:clothheight</remarks>
        if (!lateStartFlag)
        {
            clothHeight = init.ClothHeight;
            lateStartFlag = true;
        }

        playerIncrementor = 0;
        bodyPosIncrementor = 0;

        ///<remarks>Load the submesh</remarks>
        if (subMeshRenderersLoaded == false)
        {
            if (init.doneLoading)
            {
                subMeshRenderersLoaded = true;

                Mesh mesh = init.InteractiveSubMesh.GetComponent<MeshFilter>().mesh;
                mesh.GetUVs(0, uvs);
                mesh.GetUVs(1, uvBs);
            }
        }

        ///<remarks>Handles the timers and spread on the waves</remarks>
        for (int i = 0; i < 8; i++)
        {
            if (wavRadList[i] > 0f)
                wavRadList[i] += Time.deltaTime * 4.0f;

            if (wavRadList[i] > 500f)
            {
                wavRadList[i] = 0f;
                wavParamList[i] = 0;
            }

            if (wavIntensityList[i] > 0.0f)
                wavIntensityList[i] -= Time.deltaTime * 0.15f;
        }

        for(int i = 0; i < NUM_HIGHLIGHT; i++)
        {
            if (highlightPosList[i] == Vector2.zero) continue;
            highlightValueList[i] -= 0.01f;

            if (highlightValueList[i] < 0.0f)
            {
                highlightPosList[i] = Vector2.zero;
                highlightValueList[i] = 0.0f;
            }
        }

        if (!subMeshRenderersLoaded) return;
        for (int i = 0; i < NonStarObjects.Count; i++)
        {
            Vector3 pos = NonStarObjects[i].transform.position;
            sphereBlankScript tap = NonStarObjects[i].GetComponent<sphereBlankScript>();
            GoalStarActions goal = NonStarObjects[i].GetComponent<GoalStarActions>();
            bool isTapPanel = (tap != null);

            int starIndex = init.getIndexForStarID(isTapPanel ? tap.starID : uint.Parse(goal.starID));
            float yMod = 0.0f;
            float wavStickTimeModifier = 1E-18f + Mathf.Clamp(Rand(new Vector2(pos.x, pos.z)), 0.2f, 1.0f) * 1E-12f;

            for (int j = 0; j < 8; j++)
            {
                if (wavRadList[j] == 0.0f) continue;

                float dist = Mathf.Sqrt(Mathf.Pow(pos.x - wavPosList[j].x, 2) + Mathf.Pow(pos.z - wavPosList[j].z, 2));
                float temp = Mathf.Max(Mathf.Abs(dist - wavRadList[j]), 0.0000001f);
                float posi = Mathf.Max(Mathf.Abs(dist), 0.0000001f);

                if (posi > wavRadList[j])
                {
                    if (temp < 30.0f) yMod += (wavIntensityList[j] * 0.25f) * Mathf.Sin(2.0f * temp) / (2.0f * temp);
                }
                else
                {
                    bool paramBool = false;
                    switch (wavParamList[j])
                    {
                        case 0: //RMS < 0.15
                            if (DenormalizeToRange(uvBs[starIndex].x, 0.0f, 0.6715103569670999f) < 0.15f) paramBool = true;
                            break;
                        case 1: //SNR > 12
                            if (DenormalizeToRange(uvs[starIndex].w, 0.0f, 26.61842977741538f) > 12.0f) paramBool = true;
                            break;
                        case 2: //Highest Teff in the Grid //SUB IN AstroColor
                            if (DenormalizeToRange(uvs[starIndex].y, 1.231199705120561f, 1.8991459147993115f) < 1.5f) paramBool = true;
                            break;
                        case 3: //Variable Class 3 tier scores
                            if (DenormalizeToRange(uvBs[starIndex].y, 0.0f, 1.0f) == 1.0f) paramBool = true;
                            break;
                    }

                    if (paramBool)
                        yMod += (wavIntensityList[j] * 0.25f) * Mathf.Min(Mathf.Max((1.0f - Mathf.Pow(wavRadList[j] - posi, 10.0f) * wavStickTimeModifier), 0.0f), 1.0f);
                    else if (temp < 30.0f)
                        yMod += (wavIntensityList[j] * 0.25f) * Mathf.Sin(2.0f * temp) / (2.0f * temp);
                }
            }

            if (isTapPanel) tap.waveYMod = yMod;
            else goal.waveYMod = yMod;
        }

        mMat.SetVector("_vertBounds", vertBounds);
        mMat.SetFloat("_clothHeight", clothHeight);

        wavPosBuff.SetData(wavPosList);
        wavRadBuff.SetData(wavRadList);
        wavIntBuff.SetData(wavIntensityList);
        wavParamBuff.SetData(wavParamList);

        mMat.SetFloat("_controllerHeldCDTimer", controllerHeldCDTimer[0]);
        mMat.SetVector("_lControllerPos", lControllerPos[0]);
        mMat.SetFloat("_controllerHeldCDTimerB", controllerHeldCDTimer[1]);
        mMat.SetVector("_lControllerPosB", lControllerPos[1]);
        mMat.SetFloat("_controllerHeldCDTimerC", controllerHeldCDTimer[2]);
        mMat.SetVector("_lControllerPosC", lControllerPos[2]);
        mMat.SetFloat("_controllerHeldCDTimerD", controllerHeldCDTimer[3]);
        mMat.SetVector("_lControllerPosD", lControllerPos[3]);

        bodyPosBuff.SetData(bodyPosList);
        bodyPosList = bodyPosList.Select((v) => Vector3.one * 99999f).ToArray();

        Vector2[] obscure = new Vector2[NUM_OBSCURE];
        obscurePosBuff.SetData(obscure.Select((v, i) => (i < obscurePosList.Count) ? obscurePosList[i] : Vector2.zero).ToList());

        highlightPosBuff.SetData(highlightPosList);
        highlightValBuff.SetData(highlightValueList);
    }

    private void OnDestroy()
    {
        highlightPosBuff.Dispose();
        highlightValBuff.Dispose();
        obscurePosBuff.Dispose();
        bodyPosBuff.Dispose();

        wavPosBuff.Dispose();
        wavRadBuff.Dispose();
        wavIntBuff.Dispose();
        wavParamBuff.Dispose();
    }

    float Rand(Vector2 co)
    {
        float val = Mathf.Sin(Vector2.Dot(co, new Vector2(12.9898f, 78.233f))) * 43758.5453f;
        val -= Mathf.Floor(val);
        return val;
    }

    float DenormalizeToRange(float val, float min, float max)
    {
        return (val * (max - min)) + min;
    }
}
