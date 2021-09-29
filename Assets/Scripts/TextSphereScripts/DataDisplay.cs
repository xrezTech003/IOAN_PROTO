using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///		CD : DataDisplay
///		Class for displaying data on dome and screens
/// </summary>
[RequireComponent(typeof(Text2TiledTexture))]
public class DataDisplay : MonoBehaviour 
{
	#region PUBLIC_ENUM
	/// <summary>
	///		ENUM : DisplayStyle
	///		VALUES : Spiral, Rings
	/// </summary>
	public enum DisplayStyle { Spiral, Rings }
	#endregion

	#region PUBLIC_VAR
	/// <summary>
	///		VD : displayRate
	///		Charaters (or lines) per sec, less than 0 text is displayed immediatly
	/// </summary>
	[Tooltip("Charaters (or lines) per sec, less than 0 text is displayed immediatly")]
	public float displayRate = 100;

	/// <summary>
	///		VD : maxQueueSize
	///		If text is added to queue faster.  Queue will be drained if over maxQueueSize
	/// </summary>
	[Tooltip("If text is added to queue faster.  Queue will be drained if over maxQueueSize")]
	public int maxQueueSize = 1000;

	/// <summary>
	///		VD : displayRateSpeedUp
	///		If queue is larger than max size displayRate will speed up by this value each frame
	///		Must be between 1.001 and 10
	/// </summary>
	[Tooltip("If queue is larger than max size displayRate will speed up by this value each frame")]
	[Range(1.001f, 10f)]
	public float displayRateSpeedUp = 1.005f;

	/// <summary>
	///		VD : queueSpeedupWindowSize
	///		Float allowed in the queue size before speedup/slowdown as a percentage of maxQueueSize
	///		Must be between 0 and .5
	/// </summary>
	[Tooltip("Float allowed in the queue size before speedup/slowdown as a percentage of maxQueueSize")]
	[Range(0f, .5f)]
	public float queueSpeedupWindowSize = .1f;

	/// <summary>
	///		VD : textStart
	///		Start location for text in percentage of texture
	///		Must be between 0 and 1
	/// </summary>
	[Tooltip("Start location for text in percentage of texture")]
	[Range(0, 1)]
	public float textStart = .1f;

	/// <summary>
	///		VD : textEnd
	///		End location for text in percentage of texture
	///		Must be between 0 and 1
	/// </summary>
	[Tooltip("End location for text in percentage of texture")]
	[Range(0, 1)]
	public float textEnd = .9f;

	/// <summary>
	///		VD : displayStyle
	/// </summary>
	[Tooltip("Display Style")]
	public DisplayStyle displayStyle = DisplayStyle.Spiral;

	/// <summary>
	///		VD : gapX
	///		Space infront of charater being written as a percentage of texture
	///		Must be between 0 and 1
	/// </summary>
	[Tooltip("Space infront of charater being written as a percentage of texture")]
	[Range(0, 1)]
	public float gapX = .1f;
	#endregion

	#region PRIVATE_VAR
	/// <summary>
	///		VD : curDisplayRate
	///		Current Display Rate
	/// </summary>
	float curDisplayRate;

	/// <summary>
	///		VD : textQueueCharaterCount
	///		Count of total characters in all strings in v:textQueue
	/// </summary>
	int textQueueCharaterCount = 0;

	/// <summary>
	///		VD : textQueue
	///		Queue of all strings
	/// </summary>
	Queue<string> textQueue = new Queue<string>();

	/// <summary>
	///		VD : displayedTextQueue
	///		Queue of DisplayedCharacters
	/// </summary>
	Queue<DisplayedCharacter> displayedTextQueue = new Queue<DisplayedCharacter>();

	/// <summary>
	///		VD : lastDisplayTime
	/// </summary>
	float lastDislayTime = -1;

	/// <summary>
	///		VD : curLocation
	///		Current Location on crawl
	/// </summary>
	Vector2Int curLocation = new Vector2Int(0, 0);

	/// <summary>
	///		VD : eraseLoc
	///		Current location of erase on crawl
	/// </summary>
	Vector2Int eraseLoc = new Vector2Int(0, 0);

	/// <summary>
	///		VD : text2TiledTexture
	/// </summary>
	Text2TiledTexture text2TiledTexture;

	/// <summary>
	///		VD : udpSender
	///		UDP packet sender
	/// </summary>
	UDPSender udpSender;

	/// <summary>
	///		VD : ipAddress
	///		IP address to send to
	/// </summary>
	string ipAddress;

	/// <summary>
	///		VD : port
	///		Port to send to 
	/// </summary>
	int port;

	/// <summary>
	///		VD : textureFull
	/// </summary>
	bool textureFull = false;

	/// <summary>
	///		VD : charsToDisplay
	/// </summary>
	int charsToDisplay = 0;

	/// <summary>
	///		VD : textEndLoc
	///		End location of text
	/// </summary>
	int textEndLoc;

	/// <summary>
	///		VD : textStartLoc
	///		Start location of text
	/// </summary>
	int textStartLoc;

	/// <summary>
	///		VD : totalTextureWidth
	///		Width of Texture
	/// </summary>
	int totalTextureWidth;

	/// <summary>
	///		VD : totalTextureHeight
	///		Height of Texture
	/// </summary>
	int totalTextureHeight;

	/// <summary>
	///		VD : addLinerTimer
	/// </summary>
	private float addLineTimer = 0f;

	/// <summary>
	///		VD : line
	/// </summary>
	int line = 0;

	/// <summary>
	///		VD : lineDisplayCnt
	/// </summary>
	int lineDisplayCnt = 0;

    float windowSize;
	#endregion

	#region PRIVATE_DELEGATES
	delegate void DisplayCharsDel();

	/// <summary>
	///		VD : displayChars
	///		Delegate variable, doesn't really need to be a delegate
	/// </summary>
	DisplayCharsDel displayChars;
	#endregion

	#region PRIVATE_CLASS
	/**
	<summary>
		CD : DisplayedCharacter
		Struct for holding string and location x, y
	</summary>
	**/
	class DisplayedCharacter
	{
		public string s;
		public int locX;
		public int locY;

		public DisplayedCharacter(string s, int x, int y)
		{
			this.s = s;
			this.locX = x;
			this.locY = y;
		}

	}
	#endregion

	#region UNITY_FUNC
	/**
	<summary>
		FD : Start()
		Get IP and Port from Config
		Set v:udpSender to new UDPSender with ip and port
		Set v:text2TiledTexture to gameObject Text2TiledTexture
		Set v:displayChars to f:Spiral
		Set v:totalTextureWidth and v:totalTextureHeight to v:text2TiledTexture values
		Set v:curLocation x value
		Set v:curDisplayRate to v:displayRate
	</summary>
	**/
	void Start()
	{
        string ip = Config.Instance.Data.textIP;
        int port = Config.Instance.Data.testPort;

		udpSender = new UDPSender(ip, port);

		text2TiledTexture = GetComponent<Text2TiledTexture>();

		displayChars = spiral;
		totalTextureWidth = text2TiledTexture.getTotalWidth();
		totalTextureHeight = text2TiledTexture.getTotalHeight();

		curLocation.x = (int)(gapX * totalTextureWidth);

		curDisplayRate = displayRate;

        textEndLoc = (int)(totalTextureWidth * textEnd);
        textStartLoc = (int)(totalTextureHeight * textStart);

        windowSize = queueSpeedupWindowSize * maxQueueSize;

        routine = StartCoroutine(UpdateRoutine());
    }

	/**
	<summary>
		FD : Update()
		Set v:textEndLoc and v:textStartLoc v:totalTexture(Width|Height) values
		If v:curLocation y value > v:textEndLoc: Set y value to v:textStartLoc
		If v:lastDisplayTime is -1: set v:lastDisplayTime to current Time;
		If v:displayRate > 0
			Get elapsed time between v:lastDisplayTime and current Time
			Init windowSize as v:queueSpeedupWindowSize * v:maxQueueSize
			If v:textQueueCharacterCount > v:maxQueueSize + windowSize: Multiply v:curDisplayRate by v:displayRateSpeedUp
			Else If v:textQueueCharacterCount < v:maxQueueSize - windowSize
				If v:curDisplayRate > v:displayRate
					Divide v:curDisplayRate by v:displayRateSpeedUp
					Set v:curDisplayRate to v:displayRate if it is less than
			If v:curDisplayRate * elapsedTime >= 1
				Set v:charsToDisplay to value
				Call v:displayChars() delegate
				Set v:lastDisplayTime to remainder / displayRate
		Else
			Set v:charsToDisplay to max int
			Call v:displayChars() delegate
	</summary>
	**/
	void Update()
	{
		
	}

    Coroutine routine;
    void OnDestroy()
    {
        StopCoroutine(routine);
    }

    IEnumerator UpdateRoutine()
    {
        while (true)
        {
            //wrap and overwrite.
            //TODO: What do we want to do when texture is full?

            if (curLocation.y > textEndLoc) curLocation.y = textStartLoc;
            if (lastDislayTime == -1) lastDislayTime = Time.time;

            if (displayRate > 0)
            {
                float elapsedTime = Time.time - lastDislayTime;
                //			Debug.Log("elapsedTime:" + elapsedTime + " = " + Time.time +" - "+ lastDislayTime);

                // ramp up/down speed if full/empty
                if (textQueueCharaterCount > maxQueueSize + windowSize) curDisplayRate *= displayRateSpeedUp;
                else if (textQueueCharaterCount < maxQueueSize - windowSize)
                {
                    if (curDisplayRate > displayRate)
                    {
                        curDisplayRate /= displayRateSpeedUp;
                        if (curDisplayRate < displayRate) curDisplayRate = displayRate;
                    }
                }

                float fltCharatersToDisplay = curDisplayRate * elapsedTime;
                //			Debug.Log("fltCharatersToDisplay:" + fltCharatersToDisplay + " = " + displayRate +" * "+ elapsedTime);

                if (fltCharatersToDisplay >= 1)
                {
                    charsToDisplay = (int)fltCharatersToDisplay;
                    float remainder = fltCharatersToDisplay - charsToDisplay;
                    displayChars();
                    //update lastDislayTime using remainder to keep display rate as even as possible
                    lastDislayTime = Time.time - (remainder / displayRate);
                }
            }
            else
            {
                charsToDisplay = int.MaxValue;
                displayChars();
            }

            //test();

            yield return new WaitForEndOfFrame();
        }
    }
    #endregion

    #region PUBLIC_FUNC
    /**
	<summary>
		FD : addLineUDP(string)
		Send s through udpSender with all newline replaced with spaces
		<param name="s"></param>
	</summary>
	**/
    public void addLineUDP(string s)
	{
		udpSender.Send(s.Replace("\n", " "));
		//Debug.Log ("sending UDP");
	}

	/**
	<summary>
		FD : addLine(string)
		Start Coroutine f:coAddLine with s + a lot of spaces
		<param name="s"></param>
	</summary>
	**/
	public void addLine(string s)
	{
		StartCoroutine(coAddLine(s + "                "));
		//Debug.Log ("sending Text");
	}

    /**
	<summary>
		FD : getNextChar()
		If v:charsToDisplay and v:textQueue count are > 0
			If length of string at top of v:textQueue is <= to v:curChar
				Set v:curChar to 0, Pop v:textQueue, and return recursively
			Else
				Increment v:curChar, Decrement v:charsToDisplay and v:textQueueCharacterCount, and return top string char at v:curChar - 1
		Else return 0 char or null
	</summary>
	**/
    private int curChar = 0;
    public char getNextChar()
	{
		if (charsToDisplay > 0 && textQueue.Count > 0)
		{
			string head = textQueue.Peek();
			if (curChar >= head.Length)
			{
				curChar = 0;
				textQueue.Dequeue();
				return getNextChar();
			}
			else
			{
				curChar++;
				charsToDisplay--;
				textQueueCharaterCount--;
				return head[curChar - 1];
			}
		}
		else return (char)0; //null?
	}

	/**
	<summary>
		FD : calcSpiralY(int, int, float)
		If x > v:totalTextureWidth
			Subtract v:totalTextureWidth from x
			Increment line
		Return Calculation
		<param name="line"></param>
		<param name="x"></param>
		<param name="ledding"></param>
	</summary>
	**/
	public int calcSpiralY(int line, int x, float ledding)
	{
		if (x > totalTextureWidth)
		{
			x -= totalTextureWidth;
			line += 1;
		}

		return textStartLoc + (int)(line * ledding + ledding * (x / (float)totalTextureWidth));

	}
	#endregion

	#region PRIVATE_FUNC
	/**
	<summary>
		FD : coAddLine(string)
		Push s to v:textQueue
		Increment v:textQueueCharacterCount by s length
		Wait .00001 seconds
		//Call this to diplsay new text
		<param name="s"></param>
	</summary>
	**/
	IEnumerator coAddLine(string s)
	{
		/*
		for (int i = 0; i < s.Length; i = i + 1) {
			if (s.Substring (i).Length >= 1) {
				textQueue.Enqueue (s.Substring (i, 1));
			} else {
				textQueue.Enqueue (s.Substring (i));
			}
			yield return new WaitForSeconds (0.05f);
		}
		*/
		textQueue.Enqueue(s);
		textQueueCharaterCount += s.Length;
		yield return new WaitForSeconds(0.00001f);
	}

	/**
	<summary>
		FD : Spiral()
		Output Data to text dome
	</summary>
	**/
	void spiral()
	{
		char nextChar = getNextChar();
		float ledding = text2TiledTexture.font.lineHeight * text2TiledTexture.lineSpacing;

		curLocation.y = calcSpiralY(line, curLocation.x, ledding);
		int botOfChar = curLocation.y + (int)ledding;

		while (nextChar != (char)0 && botOfChar <= textEndLoc)
		{
			string next = nextChar.ToString();

			displayedTextQueue.Enqueue(new DisplayedCharacter(next, curLocation.x, curLocation.y));
			//	int xPreWrite = curLocation.x;
			//	Debug.Log(next + ":" + curLocation);
			text2TiledTexture.write(next, ref curLocation);
			//	if(xPreWrite > curLocation.x) {
			//		line++;
			//	}
			curLocation.y = calcSpiralY(line, curLocation.x, ledding);

			botOfChar = curLocation.y + (int)ledding;
			nextChar = getNextChar();

			if (botOfChar > textEndLoc)
			{
				curLocation.x = 0;
				curLocation.y = calcSpiralY(line, curLocation.x, ledding);
				botOfChar = curLocation.y + (int)ledding;
				textureFull = true;
			}

			if (textureFull)
			{
				DisplayedCharacter toErase = displayedTextQueue.Dequeue();
				eraseLoc.x = toErase.locX;
				eraseLoc.y = toErase.locY;
				text2TiledTexture.write(toErase.s, ref eraseLoc, true);
			}
		}
	}
    #endregion

    #region UNREFERENCED
    /**
	<summary>
		FD : test()
		If v:addLineTimer < Time and v:lineDisplayCnt < 900
			Set v:addLineTimer to Time + .25
			Add two new lines of two random substrings and spaces
	</summary>
	**/
    public void test()
    {
        if ((addLineTimer < Time.time) && (lineDisplayCnt < 90000))
        {
            addLineTimer = Time.time + 0.25f;

            for (int i = 0; i < 2; i++)
            {
                string s = ":ABCDEFGHIJKLMNOPQRSTUVWXYZ 0123457890 abcdefghijklmnopqrstuvwxyz "/*.Substring(Random.Range(0, 32))*/;
                s = s + ":ABCDEFGHIJKLMNOPQRSTUVWXYZ 0123457890 abcdefghijklmnopqrstuvwxyz "/*.Substring(Random.Range(0, 32))*/;
                s = s + "                ";
                addLine(lineDisplayCnt++ + s);
            }
        }

    }

    /**
	<summary>
		FD : lineWriter()
		Empty all char into v:text2TiledTexture
	</summary>
	**/
    void lineWriter()
    {
        char nextChar = getNextChar();

        while (nextChar != (char)0 && curLocation.y <= textEndLoc)
        {
            text2TiledTexture.write(nextChar.ToString(), ref curLocation);
            nextChar = getNextChar();
        }
    }
    #endregion
}
