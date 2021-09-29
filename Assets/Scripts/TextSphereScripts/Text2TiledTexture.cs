using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///		VD : Text2TilesTexture
///		Class for creating texture out of font
/// </summary>
[RequireComponent(typeof(TiledTextureCreator))]
public class Text2TiledTexture : MonoBehaviour 
{
	[HelpBox("How to use:\n" +
	"1) import font as new asset\n" +
	"2) select the font and set font size and ascii default set and press apply\n" +
	"3) click on gear and choose \"make editable copy\"\n" +
	"4) change the texture created by making the font to read/writeable\n" +
	"5) set compression to none\n" +
	"6) create new material and appy to object being textured\n" +
	"7) choose fade or transparent for rendering mode\n" +
	"8) add this script to object\n" +
	"9) drag created font to font property of script\n" +
	"10) drag created texture font texture property\n" +
	"11) check \"reverse serface normals if the camera is inside the object \n"
	, HelpBoxMessageType.Info)]

	#region PUBLIC_VAR
	/// <summary>
	///		VD : font
	/// </summary>
	[Tooltip("Editable copy of a font (see comments in script for details)")]
	public Font font;

	/// <summary>
	///		VD : fontTexture
	/// </summary>
	[Tooltip("readable texture associated with editable copy of a font")]
	public Texture2D fontTexture;

	/// <summary>
	///		VD : typeColor
	/// </summary>
	public Color typeColor;

	/// <summary>
	///		VD : lineSpacing
	/// </summary>
	[Tooltip("Multiplier for standard line spacing")]
	public float lineSpacing = 1;

	[Tooltip("Multiplier for standard letter spacing")]
	public float kerning = 1.0f;
	#endregion

	#region PRIVATE_VAR
	/// <summary>
	///		VD : textureTileCreator
	/// </summary>
	TiledTextureCreator textureTileCreator;

	/// <summary>
	///		VD : charaterTextures
	/// </summary>
	Texture2D[] charaterTextures;

	/// <summary>
	///		VD : eraserTextures
	/// </summary>
	Texture2D[] eraserTextures;

	/// <summary>
	///		VD : charArrayOffset
	/// </summary>
	const int charArrayOffset = (int)' ';
	#endregion

	#region UNITY_FUNC
	/**
	<summary>
		FD : Start()
		Call f:initCharTexture()
	</summary>
	**/
	void Start()
	{
		initCharTextures();
	}
	#endregion

	#region PUBLIC_FUNC
	/**
	<summary>
		FD : writeCharacter(char, int, int)
		Calls f:writeTexture with v:charaterTextures at c and x and y
		<param name="c"></param>
		<param name="x"></param>
		<param name="y"></param>
	</summary>
	**/
	public void writeCharater(char c, int x, int y)
	{
		writeTexture(charaterTextures[(int)c - charArrayOffset], x, y);
	}

	/**
	<summary>
		FD : writeTexture(Texture2D, int, int)
		//writes texture in specifed location (does not consder CharacterInfo)
		<param name="charaterTexture"></param>
		<param name="x"></param>
		<param name="y"></param>
	</summary>
	**/
	public void writeTexture(Texture2D charaterTexture, int x, int y)
	{
		textureTileCreator.writeTexture(charaterTexture, x, y, 0, 0, charaterTexture.width, charaterTexture.height);
	}

	/**
	<summary>
		FD : write(string, ref Vector2Int, bool)
		For every character in string s
			Get either an eraserTexture or charaterTexture 
			Write that texture
		<param name="s"></param>
		<param name="loc"></param>
		<param name="eraseChar">Default at false</param>
	</summary>
	**/
	public void write(string s, ref Vector2Int loc, bool eraseChar = false)
	{
		int curCIndex = 0;
		foreach (char c in s)
		{
			Texture2D charaterTexture;
            int index = (int)c - charArrayOffset;
            if (index > eraserTextures.Length || index < 0) index = (int)' ' - charArrayOffset;

            if (eraseChar) charaterTexture = eraserTextures[index];
            else charaterTexture = charaterTextures[index];

            /*
            try
			{
				if (eraseChar) charaterTexture = eraserTextures[(int)c - charArrayOffset];
				else charaterTexture = charaterTextures[(int)c - charArrayOffset];
			}
			catch (IndexOutOfRangeException)
			{
				if (eraseChar) charaterTexture = eraserTextures[(int)' ' - charArrayOffset];
				else charaterTexture = charaterTextures[(int)' ' - charArrayOffset];
				// this happned too much!
				Debug.LogError("Text2Texture attempting to write non-asci charater:" + c);
			}
            */

			CharacterInfo ci;
			font.GetCharacterInfo(c, out ci, font.fontSize);

			int yOffset = loc.y + font.ascent - charaterTexture.height - ci.minY;
			int xOffset = loc.x + ci.minX;

			writeTexture(charaterTexture, xOffset, yOffset);

			int advance = (int)(ci.advance * kerning);

			loc.x += (advance >= charaterTexture.width) ? advance : charaterTexture.width;
			curCIndex++; ///Somewhat Useless
		}
	}

	/**
	<summary>
		FD : getTotalWidth()
		Set v:textureTileCreator to component if null
		Return tileCols by tileSize
	</summary>
	**/
	public int getTotalWidth()
	{
		if (textureTileCreator == null) textureTileCreator = GetComponent<TiledTextureCreator>();

		return textureTileCreator.tileCols * textureTileCreator.tileSize;
	}

	/**
	<summary>
		FD : getTotalHeight()
		Set v:textureTileCreator to component if null
		Return tileRows by tileSize
	</summary>
	**/
	public int getTotalHeight()
	{
		if (textureTileCreator == null) textureTileCreator = GetComponent<TiledTextureCreator>();

		return textureTileCreator.tileRows * textureTileCreator.tileSize;
	}

	/**
	<summary>
		FD : initCharTextures()
		// create an array of small textures
		// each contaiing on letter
		// so we can just copy letters
		// with get/set pixels
		// instead of iterating through pixels at runtime
	</summary>
	**/
	public void initCharTextures()
	{
		string charsWanted = " `1234567890-=qwertyuiop[]\\asdfghjkl;'zxcvbnm,./~!@#$%^&*()_+QWERTYUIOP{}|ASDFGHJKL:\"ZXCVBNM<>?";
		font.RequestCharactersInTexture(charsWanted, font.fontSize);

		charaterTextures = new Texture2D[126 - charArrayOffset];
		eraserTextures = new Texture2D[126 - charArrayOffset];
		for (int i = (int)' '; i < 126; i++)
		{
			char c = (char)i;
			CharacterInfo ci;
			font.GetCharacterInfo(c, out ci, font.fontSize);

			int fontWidth = -1;
			int fontHeight = -1;

			int acrossDirX;
			if (ci.uvTopRight.x > ci.uvTopLeft.x)
			{
				acrossDirX = 1;
				fontWidth = Mathf.CeilToInt((ci.uvTopRight.x - ci.uvTopLeft.x) * fontTexture.width);

			}
			else if (ci.uvTopRight.x == ci.uvTopLeft.x)
			{
				acrossDirX = 0;
				//if acrossDirX then acrossDirY is 1 or -1
			}
			else
			{
				acrossDirX = -1;
				fontWidth = Mathf.CeilToInt((ci.uvTopLeft.x - ci.uvTopRight.x) * fontTexture.width);
			}


			int acrossDirY;
			if (ci.uvTopRight.y > ci.uvTopLeft.y)
			{
				acrossDirY = 1;
				fontWidth = Mathf.CeilToInt((ci.uvTopRight.y - ci.uvTopLeft.y) * fontTexture.height);
			}
			else if (ci.uvTopRight.y == ci.uvTopLeft.y)
			{
				acrossDirY = 0;
				//if acrossDirY then acrossDirX is 1 or -1
			}
			else
			{
				acrossDirY = -1;
				fontWidth = Mathf.CeilToInt((ci.uvTopLeft.y - ci.uvTopRight.y) * fontTexture.height);
			}

			int downDirX;
			if (ci.uvBottomLeft.x > ci.uvTopLeft.x)
			{
				downDirX = 1;
				fontHeight = (int)((ci.uvBottomLeft.x - ci.uvTopLeft.x) * fontTexture.width);

			}
			else if (ci.uvBottomLeft.x == ci.uvTopLeft.x)
			{
				downDirX = 0;
			}
			else
			{
				downDirX = -1;
				fontHeight = (int)((ci.uvTopLeft.x - ci.uvBottomLeft.x) * fontTexture.width);
			}

			int downDirY;
			if (ci.uvBottomLeft.y > ci.uvTopLeft.y)
			{
				downDirY = 1;
				fontHeight = (int)((ci.uvBottomLeft.y - ci.uvTopLeft.y) * fontTexture.height);
			}
			else if (ci.uvBottomLeft.y == ci.uvTopLeft.y)
			{
				downDirY = 0;
			}
			else
			{
				downDirY = -1;
				fontHeight = (int)((ci.uvTopLeft.y - ci.uvBottomLeft.y) * fontTexture.height);

			}

			Texture2D charTexture = new Texture2D(fontWidth, fontHeight);

			Vector2Int rowStart = new Vector2Int(
			(int)(ci.uvTopLeft.x * fontTexture.width),
			(int)(ci.uvTopLeft.y * fontTexture.height));


			Vector2Int curLoc = new Vector2Int(
			(int)(ci.uvTopLeft.x * fontTexture.width),
			(int)(ci.uvTopLeft.y * fontTexture.height));


			Color baseTypeColor = new Color(typeColor.r, typeColor.g, typeColor.b);
			typeColor.a = 0;

			for (int y = 0; y < charTexture.height; y++)
			{
				for (int x = 0; x < charTexture.width; x++)
				{
					//                    if (c == debugChar) Debug.Log("curLoc " + curLoc);
					Color color = fontTexture.GetPixel(curLoc.x, curLoc.y);
					if (color.a != 0)
					{
						baseTypeColor.a = color.a;
						charTexture.SetPixel(x, y, baseTypeColor);
					}
					else
					{
						charTexture.SetPixel(x, y, typeColor);
					}
					curLoc.x += acrossDirX;
					curLoc.y += acrossDirY;
				}
				//done with a row
				rowStart.x += downDirX;
				rowStart.y += downDirY;
				curLoc.x = rowStart.x;
				curLoc.y = rowStart.y;
			}
			charTexture.Apply();
			charTexture = cropTexture(charTexture);
			charaterTextures[i - charArrayOffset] = charTexture;

			Texture2D eraserText = new Texture2D(charTexture.width, charTexture.height);
			Color clearColor = new Color(typeColor.r, typeColor.g, typeColor.b);
			clearColor.a = 0;
			for (int y = 0; y < eraserText.height; y++)
			{
				for (int x = 0; x < eraserText.width; x++)
				{
					eraserText.SetPixel(x, y, clearColor);

				}
			}
			eraserText.Apply();
			eraserTextures[i - charArrayOffset] = eraserText;

		}
	}

	/**
	<summary>
		FD : cropTexture(Texture2D)
		Search pixel by pixel of texture to get bounds and crop texture
	</summary>
	**/
	public static Texture2D cropTexture(Texture2D sourceTexture)
	{
		int minX = sourceTexture.height + 1;
		int maxX = -1;
		int minY = sourceTexture.width + 1;
		int maxY = -1;

		for (int x = 0; x < sourceTexture.width; x++)
		{
			for (int y = 0; y < sourceTexture.height; y++)
			{
				Color c = sourceTexture.GetPixel(x, y);
				if (c.a > 0)
				{
					minX = minX < x ? minX : x;
					minY = minY < y ? minY : y;
					maxX = maxX > x ? maxX : x;
					maxY = maxY > y ? maxY : y;
				}
			}
		}

		if (maxX < 0)
		{
			// if all alpha
			return sourceTexture;
		}
		else if ((minX == 0) && (minY == 0) && (maxX == sourceTexture.width - 1) && (minY == sourceTexture.height - 1))
		{
			// if doesn't need cropping
			return sourceTexture;
		}
		else
		{
			Texture2D result = new Texture2D(maxX - minX + 1, maxY - minY + 1);
			for (int x = 0; x < result.width; x++)
			{
				for (int y = 0; y < result.height; y++)
				{
					Color c = sourceTexture.GetPixel(minX + x, minY + y);
					result.SetPixel(x, y, c);
				}
			}
			result.Apply();
			return result;
		}
	}
	#endregion

	#region COMMENTED_CODE
	//	[Tooltip("Bigger textures are slower but look better, text gets smaller")]
	//	public int textureSize = 1024;

	// Update is called once per frame
	void Update()
	{

	}

	//returns new texture cropped as tightly as possible

	#endregion
}
