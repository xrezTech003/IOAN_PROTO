using UnityEngine;

/// <summary>
///     ENUM : HelpBoxMessageType
///     Values : None, Info, Warning, Error
/// </summary>
public enum HelpBoxMessageType { None, Info, Warning, Error }

/// <summary>
///     CD : HelpBoxAttribute
///     Contains log messages, more of a struct than a class
/// </summary>
public class HelpBoxAttribute : PropertyAttribute
{
    #region PUBLIC_VAR
    /// <summary>
    ///     VD : text
    ///     Message
    /// </summary>
    public string text;

    /// <summary>
    ///     VD : messageType
    ///     Type of message
    /// </summary>
    public HelpBoxMessageType messageType;
    #endregion

    #region CONSTRUCTOR
    /**
    <summary>
        CO : HelpBoxAttribute(string, HelpBoxMessageType)
        <param name="text"/>
        <param name="messageType"/>
    </summary>
    **/
    public HelpBoxAttribute(string text, HelpBoxMessageType messageType = HelpBoxMessageType.None)
    {
        this.text = text;
        this.messageType = messageType;
    }
    #endregion
}
