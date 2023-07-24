using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatItem
{
    public enum enumChatItemType
    {
        NONE,
        DateTimeTag,
        LeftChatMessage,
        RightChatMessage,
    }

    public enumChatItemType itemType = enumChatItemType.NONE;
    public GameObject gameObject;
    public string text;
    public System.DateTime time;
}
