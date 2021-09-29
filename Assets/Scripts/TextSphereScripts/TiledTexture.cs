using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CD: This class holds the mesh of tiled textures.
/// </summary>
public class TiledTexture
{
    #region PUBLIC_VAR
    public TiledTexture right;
    public TiledTexture down;
    #endregion

    #region PRIVATE_VAR
    private bool isDirty = false;
    private Texture2D destinationTexture;

    private static List<TiledTexture> allTiledTexture = new List<TiledTexture>();

    private int col;
    private int row;

    /// <summary>
    /// VD: curTT: Current TileTexture the class starts on
    /// </summary>
    static int curTT = 0;
    #endregion

    #region PUBLIC_CONS
    /// <summary>
    /// FD: Class Initalizer: Sets correct row and cols and add textures as needed and clears the bg.
    /// </summary>
    /// <param name="size">VD: side length of textures</param>
    /// <param name="bgColor">VD: bg color that gets wiped out</param>
    /// <param name="col">Column position of tile</param>
    /// <param name="row">Row position of tile</param>
    public TiledTexture(int size, Color bgColor, int col, int row)
    {
        this.col = col;
        this.row = row;

        allTiledTexture.Add(this);

        destinationTexture = new Texture2D(size, size, TextureFormat.ARGB32, false, true)
        {
            wrapMode = TextureWrapMode.Clamp
        };

        clearTexture(bgColor);
        apply(); // need to call apply after all texture manipulations
    }
    #endregion

    #region PUBLIC_FUNC
    /// <summary>
    /// FD: toString sends out tile in a form that tells row and column it is on
    /// </summary>
    /// <returns>Returns a string as "T[col,row]"</returns>
	public override string ToString()
    {
		return "T[" + col + ", " + row + "]";
	}

    /// <summary>
    /// FD: Used to get the destination texture.
    /// </summary>
    /// <returns></returns>
	public Texture2D getTexture()
    {
		return destinationTexture;
	}

    /// <summary>
    /// FD: applyToAll(): applies the textures to all tiles that are possible with f:apply.
    /// </summary>
    public static void applyToAll()
    {
        int start = curTT;
        curTT = (curTT + 1) % allTiledTexture.Count;

        while (!allTiledTexture[curTT].apply() && curTT != start)
            curTT = (curTT + 1) % allTiledTexture.Count;
    }

    /// <summary>
    /// FD: apply():  If it is dirty re apply the texture, if not, don't apply
    /// </summary>
    /// <returns>returns if the texture is dirty or not.</returns>
	public bool apply()
    {
        //Apply must be called after
        if (isDirty)
        {
            //			Debug.Log("Applying " + row + " " + col);
            destinationTexture.Apply();
            isDirty = false;

            return true;
        }

        return false;
    }

    /// <summary>
    /// FD: clearTexture: clears the texture.
    /// </summary>
    /// <param name="color"></param>
	public void clearTexture(Color color)
    {
        floodFillArea(destinationTexture.width, destinationTexture.height, color);
    }

    /// <summary>
    /// FD: horizontalLine: 
    /// </summary>
    /// <param name="x">x position of line segment</param>
    /// <param name="y">y position of line segment</param>
    /// <param name="length">VD: length of the line</param>
    /// <param name="c">VD: Color of the line</param>
	public void horizontalLine(int x, int y, int length, Color c)
    {
        //		Debug.Log("horizontalLine: " + x + ", " + y + " - " + length + "   " + this);
        if (x + length > destinationTexture.width)
        {
            int lengthInThisText = destinationTexture.width - x;
            horizontalLine(x, y, lengthInThisText, c);

            if (right != null)
                right.horizontalLine(0, y, length - lengthInThisText, c);
        }
        else if (y > destinationTexture.height)
        {
            if (down != null)
                down.horizontalLine(0, y - destinationTexture.height, length, c);
        }
        else
        {
            isDirty = true;

            for (int offset = 0; offset < length; offset++)
            {
                //				Debug.Log("            SetPixel: " + (x + offset)+","+y);
                destinationTexture.SetPixel(x + offset, y - 1, c);
                destinationTexture.SetPixel(x + offset, y - 2, c);
                destinationTexture.SetPixel(x + offset, y, c);
                destinationTexture.SetPixel(x + offset, y + 1, c);
                destinationTexture.SetPixel(x + offset, y + 2, c);
            }
        }
    }

    /// <summary>
    /// FD: floodFillArea: This function takes an area of width and height and fills it with a color. This also makes the considered region dirty.
    /// </summary>
    /// <param name="width">VD: width of the area to fill</param>
    /// <param name="height">VD: height of the area to fill</param>
    /// <param name="color">VD: color to fill area with.</param>
	public void floodFillArea(int width, int height, Color color)
    {
        isDirty = true;
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                destinationTexture.SetPixel(x, y, color);
    }


    /// <summary>
    /// FD: writeTexture(): takes the required inputs and apply a texture to a given location. If the location is off the allowed range do what we can in other direction if possible. If completely on the texture consider what we change as dirty.
    /// </summary>
    /// <param name="sourceTexture">VD: The original texture</param>
    /// <param name="locationInDestinationX">VD: x position to put source texture</param>
    /// <param name="locationInDestinationY">VD: y position to put source texture</param>
    /// <param name="locationInSourceX">VD: x position to look into source texture with</param>
    /// <param name="locationInSourceY">VD: y position to look into source texture with</param>
    /// <param name="w">VD: width to write to</param>
    /// <param name="h">VD: height to write too</param>
	public void writeTexture(Texture2D sourceTexture, int locationInDestinationX, int locationInDestinationY, int locationInSourceX, int locationInSourceY, int w, int h)
    {
        /*
            Debug.Log("writeTexture: " +
                "dest(" + locationInDestinationX + ", " +locationInDestinationY+ ")  " +
                "source(" + locationInSourceX + ", " + locationInSourceY +")  " +
                w + "x" + h) ;
        */

        if (locationInDestinationX > destinationTexture.width)
        {
            //completally off the right
            if (right != null)
            {
                right.writeTexture(sourceTexture,
                locationInDestinationX - destinationTexture.width, locationInDestinationY,
                locationInSourceX, locationInSourceY, w, h);
            }
        }
        else if (locationInDestinationY > destinationTexture.height)
        {
            //completally off the bottom
            if (down != null)
            {
                down.writeTexture(sourceTexture,
                locationInDestinationX, locationInDestinationY - destinationTexture.height,
                locationInSourceX, locationInSourceY, w, h);

            }
        }
        else if (locationInDestinationX < 0)
        {
            //off the left
            if (locationInDestinationX + w >= 0)
            {
                //not completally of the left
                writeTexture(sourceTexture, 0, locationInDestinationY, -locationInDestinationX, locationInSourceY, w + locationInDestinationX, h);
            } //else completally off left, nothing to do
        }
        else if (locationInDestinationX + w > destinationTexture.width)
        {
            // left is on texure, right edge is off
            int widthOnText = destinationTexture.width - locationInDestinationX;
            writeTexture(sourceTexture, locationInDestinationX, locationInDestinationY, locationInSourceX, locationInSourceY, widthOnText, h);

            if (right != null)
            {
                int remainderWidth = w - widthOnText;
                right.writeTexture(sourceTexture, 0, locationInDestinationY, locationInSourceX + w - remainderWidth, locationInSourceY, remainderWidth, h);
            }
        }
        else if (locationInDestinationY < 0)
        {
            //off the top
            if (locationInDestinationY + h >= 0)
            {
                //not completally of the top
                writeTexture(sourceTexture, locationInDestinationX, 0, locationInDestinationX, -locationInSourceY, w, h + locationInDestinationY);
            } //else completally off top, nothing to do
        }
        else if (locationInDestinationY + h > destinationTexture.height)
        {
            // top is on texure, bottom edge is off
            int heightOnText = destinationTexture.height - locationInDestinationY;
            writeTexture(sourceTexture, locationInDestinationX, locationInDestinationY, locationInSourceX, locationInSourceY, w, heightOnText);

            if (down != null)
            {
                int remainderHeight = h - heightOnText;
                down.writeTexture(sourceTexture, locationInDestinationX, 0, locationInSourceX, locationInSourceY + h - remainderHeight, w, remainderHeight);
            }
        }
        else
        {
            //entirely on texture
            isDirty = true;
            Color[] pixels = sourceTexture.GetPixels(locationInSourceX, locationInSourceY, w, h);
            destinationTexture.SetPixels(locationInDestinationX, locationInDestinationY, w, h, pixels);
        }
    }
    #endregion
}

//	Color[] pixels = charaterTexture.GetPixels(charX, charY, charWidth, charHeight);
//	destinationTexture.SetPixels(xOffset, yOffset, charWidth, charHeight, pixels);