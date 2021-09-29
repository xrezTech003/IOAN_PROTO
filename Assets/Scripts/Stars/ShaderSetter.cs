using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// CD: Used to generate the colour and movement on draggableStars and activatedStars
/// </summary>
public class ShaderSetter : MonoBehaviour
{
    private delegate void PulseEvent();
    private PulseEvent pulse;

    public bool trigger = false;

    #region PUBLIC_VAR
    /// <summary>
    /// VD: Public float values for setting shape and colour of wireframe and blob
    /// </summary>
    public float blend, drip, edgeWidth, noiseScale, noiseWeight, poke, speed, rimExpo, alpha;
    /// <summary>
    /// VD: Public float values for setting the colour of wireframe and blob
    /// </summary>
    public Color gradColorA, gradColorB, rimColor, edgeColor;
    /// <summary>
    /// VD: Public temp float values for setting the colour of wireframe and blob
    /// </summary>
    public Color gradColorAOrig, gradColorBOrig, rimColorOrig, edgeColorOrig;

    /// <summary>
    /// VD: Animation duration
    /// </summary>
    public float animDuration = 1.0f;

    /// <summary>
    /// VD: Size of the wireframe
    /// </summary>
    public float exteriorScale = 4.0f;
    /// <summary>
    /// VD: Size of the blob
    /// </summary>
    public float interiorScale = 1.0f;
    /// <summary>
    /// VD: temp for the initial value set to the wireframe's size
    /// </summary>
    public float exteriorScaleOrig;
    /// <summary>
    /// VD: temp for the initial value set to the blob's size
    /// </summary>
    public float interiorScaleOrig;

    public int starID;

    public float noiseScaleMod = 0;
    public float dripMod = 0;
    public float noiseWeightMod = 0;
    public float pokeMod = 0;
    public float speedMod = 0;
    public float interiorMeshScaleMod = 0;

    #endregion

    #region PRIVATE_VAR
    /// <summary>
    /// VD: Object pointer for amorphous blob in the middle of a star object
    /// </summary>
    private GameObject interiorMesh;
    /// <summary>
    /// VD: Object pointer for the wireframe around a star object
    /// </summary>
    private GameObject exteriorMesh;
    /// <summary>
    /// VD: Material pointer for amorphous blob in the middle of a star object
    /// </summary>
    private Material interiorMat;
    /// <summary>
    /// VD: Material pointer for wireframe around a star object
    /// </summary>
    private Material exteriorMat;

    private Transform interiorMeshTransform;

    private Vector3 interiorMeshInitScale;

    #endregion

    #region UNITY_FUNC
    /// <summary>
    /// FD: The start function for shadesetter, will, by default start after the start function for activatedStarScript and draggedSphereScriptNew
    ///  ::: Saves the passed through values from the said script into the "Orig" values for variation in Update
    /// </summary><list><remarks>#1 Find the actual objects to be shade-adjusted </remarks><remarks#2 Find the renderers of the objects to be shade-adjusted </remarks></list>
    private void Start()
    {
        gradColorAOrig = gradColorA;
        gradColorBOrig = gradColorB;
        rimColorOrig = rimColor;
        edgeColorOrig = edgeColor;
        exteriorScaleOrig = exteriorScale;
        interiorScaleOrig = interiorScale;

        //#1
        interiorMesh = transform.Find("icosBlend").gameObject;
        exteriorMesh = transform.Find("icosEdges").gameObject;

        //#2
        interiorMat = interiorMesh.GetComponent<Renderer>().material;
        exteriorMat = exteriorMesh.GetComponent<Renderer>().material;

        //Get transform and inital scale of the intirior mesh
        interiorMeshTransform = interiorMesh.transform;
        interiorMeshInitScale = interiorMeshTransform.localScale;
    }

    ///<summary>
	/// FD: Animates the star by playing the saved squash and squirm animation as well as consistently updating the colour blend
	///</summary>
	void Update()
    {
        //Set Scale
        interiorMeshTransform.localScale = interiorMeshInitScale * (1 + interiorMeshScaleMod);
        //Set Uniforms
        interiorMat.SetFloat("_Drip", drip + dripMod);
        exteriorMat.SetFloat("_Drip", drip + dripMod);

        interiorMat.SetFloat("_EdgeWidth", edgeWidth);
        exteriorMat.SetFloat("_EdgeWidth", 0);

        interiorMat.SetFloat("_NoiseScale", noiseScale + noiseScaleMod);
        exteriorMat.SetFloat("_NoiseScale", noiseScale + noiseScaleMod);

        interiorMat.SetFloat("_NoiseWeight", noiseWeight + noiseWeightMod);
        exteriorMat.SetFloat("_NoiseWeight", noiseWeight + noiseWeightMod);

        interiorMat.SetFloat("_Poke", poke + pokeMod);
        //testing pokeMod effect
        //print("Poke from shadesetter: " + (poke) + " with mod: " + (poke+pokeMod));
        exteriorMat.SetFloat("_Poke", poke + pokeMod);

        interiorMat.SetFloat("_Speed", speed + speedMod);
        exteriorMat.SetFloat("_Speed", speed + speedMod);

        interiorMat.SetFloat("_RimExponent", rimExpo);
        exteriorMat.SetFloat("_RimExponent", rimExpo);

        //Set Colors
        exteriorMat.SetColor("_RimColor", rimColor);
        interiorMat.SetColor("_RimColor", rimColor);

        exteriorMat.SetColor("_GradColor0", gradColorA);
        interiorMat.SetColor("_GradColor0", gradColorA);

        exteriorMat.SetColor("_GradColor1", gradColorB);
        interiorMat.SetColor("_GradColor1", gradColorB);

        exteriorMat.SetColor("_EdgeColor", edgeColor);
        interiorMat.SetColor("_EdgeColor", edgeColor);

        exteriorMat.SetFloat("_Alpha", alpha);
        interiorMat.SetFloat("_Alpha", alpha);

        if (trigger)
        {
            TriggerPulse(5f, 10f);
            trigger = false;
        }

        //Move Pulses
        pulse?.Invoke();
    }
    #endregion

    #region PUBLIC_FUNC
    public void TriggerPulse(float pulseAmp, float pulseFrames)
    {
        if (pulse == null)
        {
           
            origBlend = currentBlend = exteriorMat.GetFloat("_Blend");
            maxBlend = origBlend + pulseAmp;
            if (maxBlend > 10f) maxBlend = 10f;

            pulseStep = (maxBlend - origBlend) / pulseFrames;

            pulse = PulseBlend;
        }
        else
        {
            maxBlend = currentBlend + pulseAmp;
            if (maxBlend > 10f) maxBlend = 10f;

            pulseStep = (maxBlend - currentBlend) / 10f;
        }
    }
    #endregion

    #region PRIVATE_FUNC
    /// <summary>
    /// FD: used by Update to give the star isocos their abstract animation patterns
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    private float ElasticOut(float t)
    {
        return Mathf.Sin(-13.0f * (t + 1.0f) * Mathf.PI * 0.5f) * Mathf.Pow(2.0f, -10.0f * t) + 1.0f;
    }

    float origBlend, maxBlend, currentBlend, pulseStep;
    private void PulseBlend()
    {
        PulseBlend(pulseStep);
    }

    private void PulseBlend(float pulseStep)
    {
        exteriorMat.SetFloat("_Blend", currentBlend);
        interiorMat.SetFloat("_Blend", currentBlend);

        currentBlend += pulseStep;

        if (pulseStep > 0)
        {
            if (currentBlend > maxBlend)
                pulseStep *= -1f;
        }
        else
        {
            if (currentBlend < origBlend)
                pulse = null;
        }
    }
    #endregion
}
