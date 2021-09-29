using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
/**
<summary>
	CD : SensorCam
	// this script assumes an orthographic camera is attached to the controller
	// the camera's near and far clipping panes are sized to fit the controller perfectly
	// it is sized and positioned to extend out below the controller (to make sure it hits the mesh that is generating the geomtery)
	// it renders using a shader that encodes object ids as colors
	// todo: copy all the parameters from regular shader to sensor shader
</summary>
**/
public class SensorCam : MonoBehaviour
{
	#region PUBLIC_VAR
	/// <summary>
	///		VD : cam
	/// </summary>
	public Camera cam;

	/// <summary>
	///		VD : Replacement shader to decrease gpu overhead for sensor cam star finding
	/// </summary>
	public Shader sensorShader;

	/// <summary>
	///		VD : Width of sensorcam screenshot 
	/// </summary>
	public int textureWidth = 3;

	/// <summary>
	///		VD : Height of sensorcam screenshot 
	/// </summary>
	public int textureHeight = 1;

	/// <summary>
	///		VD : controller handednessness boolean
	/// </summary>
	public bool isLeft = false;

	/// <summary>
	///		VD : starID read from getPixel color value reader
	///		//	[ReadOnly]
	/// </summary>
	public uint starID { get; private set; }

	/// <summary>
	///		VD : isClient
	///		// this code should only be run on client machines
	///		// Hetero Player Node should set this varaible to true
	/// </summary>
	public bool isClient = false;
	/// <summary>
	/// VD: reference to the controller this sensorcam is attached to, this script set's it's starID 
	/// </summary>
	public iOANHeteroController controllerNode;
	//private iOANHeteroController.ControllerGrabDetector controllerGrabDetector; //Unused left for reference, this did not work as well as keeping the star ID in the controller node

	#endregion

	#region PRIVATE_VAR
	/// <summary>
	///		VD : copyRect
	/// </summary>
	private Rect copyRect;

    /// <summary>
    ///		VD : renderTex
    /// </summary>
    private RenderTexture renderTex;

	/// <summary>
	///		VD : tex2d
	/// </summary>
	private Texture2D tex2d;
	
	#endregion

	#region UNITY_FUNC
	/**
	<summary>
		FD : Start()
		Set v:cam RaplacementShader to v:sensorShader tag to empty string
		Disable v:cam
		Make new 2D Texture and RenderTexture
		Set v:cam targetTexture to renderTex
	</summary>
	**/
	void Start()
	{
		
        cam.SetReplacementShader(sensorShader, "");
		cam.enabled = false;

		//ideally the render texture dims should be the same ratio as the camera
		//you can set it in the script but i'm not sure what it does to its size in the game
		tex2d = new Texture2D(textureWidth, textureHeight, TextureFormat.ARGB32, false, true);
		renderTex = new RenderTexture(tex2d.width, tex2d.height, 24, UnityEngine.RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);

		//this rect specifies that we'll be getting all the pixels out of the render texture
		//int the tex2d
		copyRect = new Rect(0, 0, tex2d.width, tex2d.height);
		cam.targetTexture = renderTex;
	}
	/// <summary>
	/// FD: Start the Coroutine to render the cameras and get the starID ::: Run left and right on alternating frames if running on a client, or not at all. 
	/// </summary>
	private void Update()
	{
		// only run on client machines
		// the server should not be doing this!
		//if (isLeft) { controllerGrabDetector = grabDetector.left; }
		//else { controllerGrabDetector = grabDetector.right; }
		if (!isClient) return;

		cam.RenderWithShader(sensorShader, "");


		bool isOddFrame = (Time.frameCount % 2) == 0;

		if (isLeft == isOddFrame/* && !controllerNode.grabbing*/)
		{

			StartCoroutine(getStarIDCo());
		}
		//else
		//{
  //          StartCoroutine(readStarPixelsCo());
  //      }

	}
	#endregion


	#region PUBLIC_FUNC
	/**
	<summary>
		FD : colorToUInt(Color)
		Convert Color c to 32-bit number, every 8-bits is a, r, g, or b
		//this is used to confert convert r,g,b value set as a pixel color by a fragment shader into an integer
		<param name="c"></param>
	</summary>
	**/
	public static uint colorToUInt(Color c)
	{
		uint a = ((uint)(c.a * 255)) << 24;
		uint r = ((uint)(c.r * 255)) << 16;
		uint g = ((uint)(c.g * 255)) << 8;
		uint b = ((uint)(c.b * 255));

		return r + g + b + a;
	}


	/**
	<summary>
		FD : getStarID()
		Set current active render texture to v:renderTex
		Apply v:copyRect data to v:tex2d
		Reset current active render texture to original render texture
		Iterate through each pixel and generate a v:starID based on that Pixel
		Return if the ID isn't 0
	</summary>
	**/
	public void getStarID(AsyncGPUReadbackRequest request)
	{

		if (request.hasError)
		{
			Debug.Log("GPU readback error detected.");
			return;
		}

        if (tex2d == null) return;

		tex2d.LoadRawTextureData(request.GetData<uint>());
		tex2d.Apply();

		// check the whole texture starting from the bottom since the camera starts at the bottom of the controller and goes up (incase we go through the star)
		for (int i = 0; i < textureWidth; i++)
		{
			for (int j = textureHeight - 1; j >= 0; j--)
			{
				Color c = tex2d.GetPixel(i, j);
				starID = colorToUInt(c);
				controllerNode.GetComponent<iOANHeteroController>().starID = this.starID; //This line runs regardless of whether or not a star is found with an id != 0 to help with information flow through the grab detector
				if (starID != 0)
				{
					//Debug.Log("SensorCam: STAR FOUND WITH ID OF " + starID);
					//if (isLeft) { left.starID = starID; }
					//else right.starID = starID;
					//controllerGrabDetector = controllerNode.grabDetector;
					
					return;
				}

			}
		}

	}

	//public void readStarPixels()
 //   {
	//	RenderTexture currentRenderTexture = RenderTexture.active;
	//	RenderTexture.active = renderTex;

	//	tex2d.ReadPixels(copyRect, 0, 0); // <---- Lagging Code Immensley
	//	tex2d.Apply();

	//	RenderTexture.active = currentRenderTexture;
	//}
	#endregion

	#region PRIVATE_FUNC
	/**
	<summary>
		FD : getStarIDCo()
		Call f:getStarID() after end of frame
	</summary>
	**/
			public IEnumerator getStarIDCo()
	{
		yield return new WaitForEndOfFrame();
		AsyncGPUReadback.Request(renderTex, 0, TextureFormat.ARGB32, getStarID);
		
	}
	/// <summary>
	/// FD: Dead
	/// </summary>
	/// <returns></returns>
	private IEnumerator readStarPixelsCo()
	{
		yield return new WaitForEndOfFrame();

		//readStarPixels();
	}
	#endregion
}
