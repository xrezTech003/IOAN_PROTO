using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;
using System;

/// <summary>
///     Class for Animating text and sprite image for activated star
/// </summary>
public class DataTextLoader : MonoBehaviour
{
    private delegate void UpdateEvent();
    private UpdateEvent update;

    #region PUBLIC_VAR
    /// <summary>
    ///     Text Mesh for bold characters
    /// </summary>
    public TextMesh boldMesh;

    /// <summary>
    ///     Text Mesh for regular character
    /// </summary>
    public TextMesh regularMesh;

    /// <summary>
    ///     Text Mesh for centered characters
    /// </summary>
    public TextMesh centerMesh;

    /// <summary>
    ///     Box Renderer for text box
    /// </summary>
    public GameObject boxRenderer;

    /// <summary>
    ///     Text Mesh for bold characters
    /// </summary>
    public TextMesh revBoldMesh;

    /// <summary>
    ///     Text Mesh for regular character
    /// </summary>
    public TextMesh revRegularMesh;

    /// <summary>
    ///     Text Mesh for centered characters
    /// </summary>
    public TextMesh revCenterMesh;

    /// <summary>
    ///     Image Renderer Gameobject for graph sprite
    /// </summary>
    public GameObject frontQuad;

    /// <summary>
    ///     Image Renderer Gameobject for reverse graph sprite
    /// </summary>
    public GameObject backQuad;
    #endregion

    #region PRIVATE_VAR
    /// <summary>
    ///     Sprite Renderer for graph sprite
    /// </summary>
    private MeshRenderer frontImage;

    /// <summary>
    ///     Sprite Renderer for reverse graphe sprite
    /// </summary>
    private MeshRenderer backImage;

    /// <summary>
    ///     Max Size of text renderer
    /// </summary>
    private readonly Vector3 maxScale = new Vector3(1.1f, 0.85f, 1.0f);

    /// <summary>
    ///     Speed of animation
    /// </summary>
    private readonly float scaleSpeed = 0.02f;

    /// <summary>
    ///     Speed at which text renderer animates
    /// </summary>
    private float posSpeed;

    /// <summary>
    ///     Speed at which image is faded in
    /// </summary>
    private float alphaSpeed;

    /// <summary>
    ///     Text to be bolded
    /// </summary>
    private List<string> boldText = new List<string>();

    /// <summary>
    ///     Text to stay stagnant
    /// </summary>
    private List<string> regularText = new List<string>();

    /// <summary>
    ///     Text to be centered
    /// </summary>
    private List<string> centerText = new List<string>();

    /// <summary>
    ///     Is data done loading from server
    /// </summary>
    private bool isLoaded = false;

    /// <summary>
    ///     Display reverse data on server
    /// </summary>
    private bool serverDuplicate = false;
    #endregion

    #region UNITY_FUNC
    /// <summary>
    ///     Set animation units and speed
    /// </summary>
    private void Start()
    {
        boxRenderer.transform.localPosition = Vector3.zero;
        boxRenderer.transform.localScale = new Vector3(maxScale.x, 0.0f, maxScale.z);

        posSpeed = scaleSpeed / maxScale.y;
        alphaSpeed = scaleSpeed / maxScale.y;
    }

    /// <summary>
    ///     Update if needed
    /// </summary>
    private void Update()
    {
        update?.Invoke();
    }

    /// <summary>
    ///     Remove/Free Textures after use
    /// </summary>
    private void OnDestroy()
    {
        if (frontImage.material.GetFloat("_TextureLoaded") > 0) Destroy(frontImage.material.GetTexture("_MainTex"));
        if (backImage && backImage.material.GetFloat("_TextureLoaded") > 0) Destroy(backImage.material.GetTexture("_MainTex"));
    }
    #endregion

    #region PUBLIC_FUNC
    /// <summary>
    ///     Begin Outputting coroutine to animate text boxes
    /// </summary>
    /// <param name="splitVals"></param>
    /// <param name="inGraphURL"></param>
    public void OutputData(string[] splitVals, string inGraphURL, bool duplicate = false)
    {
        serverDuplicate = duplicate;

        StartCoroutine(LoadGraphImg(inGraphURL));

        for (int i = 0; i < splitVals.Count(); i++)
            if (splitVals[i].Contains("\n")) splitVals[i] = splitVals[i].Replace("\n", "");

        List<string> newVals = new List<string>(splitVals);
        newVals.RemoveAll((s) => s == "");

        FormatOne(newVals.ToArray());

        StartCoroutine(StartAnimation());
    }

    /// <summary>
    ///     Controls overall alpha value
    /// </summary>
    /// <param name="a"></param>
    public void SetTotalAlpha(float a)
    {
        if (!isLoaded) return;

        frontImage.material.SetFloat("_Alpha", a);
        boldMesh.gameObject.GetComponent<MeshRenderer>().material.SetFloat("Alpha", a);
        regularMesh.gameObject.GetComponent<MeshRenderer>().material.SetFloat("Alpha", a);
        centerMesh.gameObject.GetComponent<MeshRenderer>().material.SetFloat("Alpha", a);

        if (serverDuplicate)
        {
            backImage.material.SetFloat("_Alpha", a);

            revBoldMesh.gameObject.GetComponent<MeshRenderer>().material.SetFloat("Alpha", a);
            revRegularMesh.gameObject.GetComponent<MeshRenderer>().material.SetFloat("Alpha", a);
            revCenterMesh.gameObject.GetComponent<MeshRenderer>().material.SetFloat("Alpha", a);
        }
    }
    #endregion

    #region PRIVATE_FUNC
    /// <summary>
    ///     Coroutine for loading and fading in the graph sprite
    /// </summary>
    /// <param name="inGraphURL"></param>
    /// <returns></returns>
    private IEnumerator LoadGraphImg(string inGraphURL)
    {
        Texture2D tex;

        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(inGraphURL))
        {
            request.SendWebRequest();
            yield return new WaitUntil(() => request.isDone);
            if (request.isNetworkError || request.isHttpError)
            {
                Debug.LogError("Error Loading Image at: " + inGraphURL);
                yield break;
            }

            tex = DownloadHandlerTexture.GetContent(request);
        }

        frontImage = frontQuad.GetComponent<MeshRenderer>();
        frontImage.material.SetTexture("_MainTex", tex);
        frontImage.material.SetFloat("_Alpha", 0f);
        frontImage.material.SetFloat("_TextureLoaded", 1);

        if (serverDuplicate)
        {
            backImage = backQuad.GetComponent<MeshRenderer>();
            backImage.material.SetTexture("_MainTex", tex);
            backImage.material.SetFloat("_Alpha", 0f);
            backImage.material.SetFloat("_TextureLoaded", 1);
        }
    }

    /// <summary>
    ///     Coroutine to begin all animations
    /// </summary>
    /// <returns></returns>
    private IEnumerator StartAnimation()
    {
        yield return new WaitForSeconds(1.25f);

        boxRenderer.SetActive(true);

        update += FadeInImage;
        update += AnimateBox;
    }

    /// <summary>
    ///     Formatting function for data
    /// </summary>
    /// <param name="data"></param>
    private void FormatOne(string[] data)
    {
        int maxStringSize = 0;
        List<string> sanData = new List<string>();

        foreach (string s in data)
        {
            string str = s.TrimEnd(new char[] { ' ' });
            str = new string(str.Where((c) => !char.IsControl(c)).ToArray());

            if (str.Contains(" "))
            {
                string[] splitStr = str.Split(new char[] { ' ' });

                int sizeOne = splitStr[0].Length;

                maxStringSize = Mathf.Max(sizeOne, maxStringSize);
            }

            sanData.Add(str);
        }

        foreach (string s in sanData)
        {
            string str = s;
            string boldTemp = "";
            string regularTemp = "";

            if (!s.Contains(' '))
            {
                boldText.Add("\n");
                boldText.Add("\n");
                regularText.Add("\n");
                regularText.Add("\n");
                centerText.Add("\n");
                centerText.Add(s + "\n");
                continue;
            }

            string[] splitStr = str.Split(new char[] { ' ' });
            int sizeOne = splitStr[0].Length;

            for (int i = 0; i < maxStringSize - sizeOne; i++)
            {
                splitStr[0] = " " + splitStr[0];
            }

            for (int i = 0; i < splitStr[0].Length; i++)
            {
                boldTemp += splitStr[0].ElementAt(i);
                regularTemp += " ";
            }

            boldTemp += " ";
            regularTemp += " ";

            if (splitStr[1] == "Not_Available")
                splitStr[1] = "Unknown";

            for (int i = 0; i < splitStr[1].Length; i++)
            {
                boldTemp += " ";
                regularTemp += splitStr[1].ElementAt(i);
            }

            boldText.Add(boldTemp + "\n");
            regularText.Add(regularTemp + "\n");
            centerText.Add("\n");
        }

        /*
        boldMesh.fontStyle = FontStyle.Bold;
        boldMesh.alignment = TextAlignment.Left;
        boldMesh.anchor = TextAnchor.UpperCenter;

        regularMesh.fontStyle = FontStyle.Normal;
        regularMesh.alignment = TextAlignment.Left;
        regularMesh.anchor = TextAnchor.UpperCenter;

        centerMesh.fontStyle = FontStyle.Bold;
        centerMesh.alignment = TextAlignment.Center;
        centerMesh.anchor = TextAnchor.UpperCenter;
        */
    }

    /// <summary>
    ///     Delegate function to fade in image
    /// </summary>
    private void FadeInImage()
    {
        if (frontImage == null)
            frontImage = frontQuad.GetComponent<MeshRenderer>();

        float alpha = frontImage.material.GetFloat("_Alpha");
        frontImage.material.SetFloat("_Alpha", alpha + alphaSpeed);

        if (serverDuplicate)
        {
            if (backImage == null)
                backImage = backQuad.GetComponent<MeshRenderer>();

            alpha = backImage.material.GetFloat("_Alpha");
            backImage.material.SetFloat("_Alpha", alpha + alphaSpeed);
        }

        if (alpha + alphaSpeed >= 1f)
            update -= FadeInImage;
    }

    /// <summary>
    ///     Index value used for animating
    /// </summary>
    private int index = 0;

    /// <summary>
    ///     Animate text box
    /// </summary>
    private void AnimateBox()
    {
        float yScale = boxRenderer.transform.localScale.y;
        float yPos = boxRenderer.transform.localPosition.y;

        boxRenderer.transform.localScale = new Vector3(maxScale.x, yScale + scaleSpeed, maxScale.z);
        boxRenderer.transform.localPosition = new Vector3(0, yPos + posSpeed, 0);

        float n = boldText.Count();
        yScale = boxRenderer.transform.localScale.y;
        float thresh = (maxScale.y / n) * (index + 1f);

        if (yScale > thresh)
            OutputText();

        if (yScale > maxScale.y)
        {
            update -= AnimateBox;
            isLoaded = true;
        }
    }

    /// <summary>
    ///     Load data to text renderers
    /// </summary>
    private void OutputText()
    {
        boldMesh.text += boldText[index];
        regularMesh.text += regularText[index];
        centerMesh.text += centerText[index];

        if (serverDuplicate)
        {
            revBoldMesh.text += boldText[index];
            revRegularMesh.text += regularText[index];
            revCenterMesh.text += centerText[index];
        }

        index++;
    }
    #endregion
}
