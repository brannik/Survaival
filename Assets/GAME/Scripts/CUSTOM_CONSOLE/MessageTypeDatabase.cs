#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MessageTypeEntry
{
    public string Name;         // Name of the message type
    public Texture2D Icon;      // Icon associated with the message type
    public Color textColor = Color.white;
}

[CreateAssetMenu(fileName = "MessageTypeDatabase", menuName = "Custom Console/MessageTypeDatabase")]
public class MessageTypeDatabase : ScriptableObject
{
    public List<MessageTypeEntry> MessageTypes = new List<MessageTypeEntry>();
}
#endif
