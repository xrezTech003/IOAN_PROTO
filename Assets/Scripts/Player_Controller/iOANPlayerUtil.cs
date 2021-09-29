using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// CD: Static class to hold enumerator and dictionary values for clean player code
/// </summary>
public static class iOANPlayerUtil 
{
    /// <summary>
    /// VD: Enumerator for playerID's so we can call them by color
    /// </summary>
    public enum playerID { red, green, blue, server }
    /// <summary>
    /// VD: Enumerator for dial entries so we can call them by color
    /// </summary>
    public enum dialID { cyan, magenta, white, yellow }
    /// <summary>
    /// VD: Dictionary to translate from enumerator to color ::: dial - cmky
    /// </summary>
    public static Dictionary<dialID, Color> dialColor = new Dictionary<dialID, Color>()
    {
        {dialID.cyan, Color.cyan},
        {dialID.magenta, Color.magenta},
        {dialID.white, Color.white},
        {dialID.yellow, Color.yellow}
    };
    /// <summary>
    /// VD: Dictionary to translate from enumerator to color ::: player - rgby
    /// </summary>
    public static Dictionary<playerID, Color> playerColor = new Dictionary<playerID, Color>()
    {
        {playerID.red, Color.red},
        {playerID.green, Color.green},
        {playerID.blue, Color.blue},
        {playerID.server, Color.yellow}
    };
}
