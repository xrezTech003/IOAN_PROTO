using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CD: TiledTextureCreator: This class initalizes the tiled textures toghether.
/// </summary>
public class TiledTextureCreator : MonoBehaviour
{
    #region PUBLIC_VAR
    /// <summary>
    /// VD: tilesize GS: (possible side length of tile)
    /// </summary>
    public int tileSize = 1024;
    /// <summary>
    /// VD: bg color: background color of the texture
    /// </summary>
	public Color bgColor;
    /// <summary>
    /// VD : tileCols: number of columns of all the tiled textures
    /// </summary>
	public int tileCols = 2;
    /// <summary>
    /// VD : tileRows: number of rows of all the tiled textures
    /// </summary>
	public int tileRows = 2;

    /// <summary>
    /// VD: wrapHorizontal: a set of bools to determine which way the tiles wrap.
    /// </summary>
	public bool wrapHorizontal = true;
    /// <summary>
    /// VD: wrapVertical: a set of bools to determine which way the tiles wrap.
    /// </summary>
	public bool wrapVerticle = false;

    /// <summary>
    /// VD: meshname: How we reference these set of tiled textures
    /// </summary>
	public string meshName = "SphereSegment";

    public Shader tileShader;
    #endregion

    #region PRIVATE_VAR
    /// <summary>
    /// VD: TiledTexture: The origin for this mesh of tiled textures.
    /// </summary>
    private TiledTexture upperLeft;
    #endregion

    #region UNITY_FUNC
    /// <summary>
    /// FD: Start(): Starts a parallel routine to flip the object it's attached to, and then sets up the tiles for the tiled texture. it also wraps the textures around the sphere
    /// </summary>
	void Start()
    {
        StartCoroutine(lateFlip());

        TiledTexture[,] tiles = new TiledTexture[tileCols, tileRows];
        for (int i = 0; i < tileCols; i++)
        {
            for (int j = 0; j < tileRows; j++)
            {
                tiles[i, j] = new TiledTexture(tileSize, bgColor, i, j);

                if (i != 0) tiles[i - 1, j].right = tiles[i, j];
                if (j != 0) tiles[i, j - 1].down = tiles[i, j];
            }
        }

        if (wrapHorizontal)
            for (int j = 0; j < tileRows; j++)
                tiles[tileCols - 1, j].right = tiles[0, j];

        if (wrapVerticle)
            for (int i = 0; i < tileCols; i++)
                tiles[i, tileRows - 1].down = tiles[i, 0];

        upperLeft = tiles[0, 0];

        //go across first then down
        int curMesh = 0;
        for (int j = 0; j < tileRows; j++)
        {
            for (int i = 0; i < tileCols; i++)
            {
                //				Debug.Log("seg:" + curMesh + " -> tile:" + i + ","+j );
                GameObject subMesh = GameObject.Find(meshName + curMesh++);

                if (subMesh != null)
                {
                    Material material = subMesh.GetComponent<MeshRenderer>().material;
                    //material.shader = Shader.Find("Unlit/Transparent");
                    material.shader = tileShader;
                    material.SetFloat("_AlphaMod", 0.55f);
                    material.SetFloat("_Flip", (float)i);
                    material.SetFloat("_FlipY", (float)j);
                    material.mainTexture = tiles[i, j].getTexture();
                }
                else Debug.LogWarning("Unable to get " + meshName + (curMesh - 1));
            }
        }

        //			Debug.Log("tiles: 0,0" + " -> right:" + tiles[0,0].right);

        //test -
        /*
		int cnt = 16;
		for(int i = 1; i <= cnt; i++) {
			horizontalLine(0, 200 + 40*i, 128*i, new Color(0,0,0));
		}
		for(int i = 1; i <= cnt; i++) {
			horizontalLine(0, 200+16*40 + 40*i, 128*i, new Color(0,0,0));
		}*/

    }


    // Update is called once per frame
    /// <summary>
    /// Keep applying the new text every frame as needed.
    /// </summary>
    void Update()
    {
        TiledTexture.applyToAll();
        //only does apply if needed
    }
    #endregion

    #region PUBLIC_FUNC
    /// <summary>
    /// FD: writetexture(): writes the appropriate texture of text to the correct tile on the sphere.
    /// </summary>
    /// <param name="sourceTexture">VD: Original text texture</param>
    /// <param name="locationInDestinationX">VD: X position in set of all tiles of tile to put texture on </param>
    /// <param name="locationInDestinationY">VD: Y position in set of all tiles of tile to put texture on</param>
    /// <param name="locationInSourceX">VD: Original X position in origin texture</param>
    /// <param name="locationInSourceY">VD: Original Y position in origin texture</param>
    /// <param name="w">VD: width of texture</param>
    /// <param name="h">VD: height of texture</param>
    public void writeTexture(Texture2D sourceTexture, int locationInDestinationX, int locationInDestinationY, int locationInSourceX, int locationInSourceY, int w, int h)
    {
        upperLeft.writeTexture(sourceTexture, locationInDestinationX, locationInDestinationY, locationInSourceX, locationInSourceY, w, h);
    }

    /// <summary>
    /// Creates a horizontal line at a position of a length
    /// </summary>
    /// <param name="x">VD: x position to place</param>
    /// <param name="y">VD: y position to place</param>
    /// <param name="length">VD: length of the line</param>
    /// <param name="c">VD: color of the line</param>
	public void horizontalLine(int x, int y, int length, Color c)
    {
        upperLeft.horizontalLine(x, y, length, c);
    }
    #endregion

    #region PRIVATE_FUNC
    /// <summary>
    /// FD: lateFlip: Takes the whole object this connected to and flips it 180 degrees.
    /// </summary>
    /// <returns></returns>
    private IEnumerator lateFlip()
    {
		yield return new WaitForSeconds(4.0f);
		transform.rotation = Quaternion.Euler (0f, 0, 180f);
	}
    #endregion
}
