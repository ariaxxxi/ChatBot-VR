using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatUI : MonoBehaviour {

    public enum enumChatMessageType
    {
        MessageLeft,
        MessageRight,
    }
    //public float chatInputHeight = 180f;

    RectTransform rootTransform;

    [Header("Controls")]
    public RectTransform contentTransform;
    public InputField inputField;



    [Header("MessageBox")]
    public GameObject dateTimeTag;
    public GameObject noticeTag;
    public GameObject chatLeftMessageBox;
    public AudioClip chatLeftMessageSound;

    public GameObject chatRightMessageBox;
    public AudioClip chatRightMessageSound;

    float contentMinSize = 0;
    float contentHeight = 0;

    Text prevText;

    AudioSource audioSource;

    List<ChatItem> chatItems = new List<ChatItem>();

    DateTime timeTag;

    float MESSAGE_ICONS_WIDTH = 65;
    float MESSAGE_MIN_WIDTH = 130;
    float MESSAGE_MAX_WIDTH = 300;
    float MESSAGE_RIGHT_MAGIN = 10;

    float MESSAGE_SPRITE_BORDER_WIDTH = 25;

    public delegate void chatMessage(string text, enumChatMessageType messageType);
    public event chatMessage OnChatMessage;

    // Use this for initialization
    void Start() {

       // timeTag = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0,0,0);

        rootTransform = GetComponent<RectTransform>();

        //float w = rootTransform.offsetMax.x - rootTransform.offsetMin.x;
        float w = rootTransform.rect.width;
        MESSAGE_MAX_WIDTH = w / 3 * 2;


        //contentMinSize = contentTransform.rect.height;//  contentTransform.offsetMax.y - contentTransform.offsetMin.y;
        //contentMinSize = rootTransform.rect.height;
        contentMinSize = contentTransform.rect.height;

        if (inputField!=null)
        {
            inputField.onEndEdit.AddListener(delegate
            {
                if (inputField.text == "")
                    return;

                AddChatMessage(inputField.text, enumChatMessageType.MessageRight);
                if (OnChatMessage != null)
                    OnChatMessage(inputField.text, enumChatMessageType.MessageRight);
                //AddLeftChatMessageBox(inputField.text);
                inputField.text = "";
                inputField.ActivateInputField();
            });
        }

        if (chatLeftMessageSound!=null || chatRightMessageSound!=null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        InstallContent();
    }

    /// <summary>
    /// Add date time tag
    /// </summary>

    public void AddChatMessage(string text, enumChatMessageType messageType)
    {
        GameObject templateObject = null;
        switch (messageType)
        {
            case enumChatMessageType.MessageLeft:
                templateObject = chatLeftMessageBox;
                break;
            case enumChatMessageType.MessageRight:
                templateObject = chatRightMessageBox;
                break;
        }

        if (templateObject == null)
            return;

     
        //float w = rootTransform.offsetMax.x - rootTransform.offsetMin.x;
        //float h = contentTransform.offsetMax.y - contentTransform.offsetMin.y;
        float w = rootTransform.rect.width;
        float h = contentTransform.rect.height;
        GameObject goChat;

        if (templateObject != null)
        {
            goChat = Instantiate(templateObject);
            RectTransform rectChat = goChat.GetComponent<RectTransform>();
            rectChat.parent = contentTransform;



            //text
            Transform tranText = rectChat.Find("MessageBox/MessageText");
            if (tranText!=null)
            {
                Text txt = tranText.GetComponent<Text>();
                txt.text = text;

                prevText = txt;

                //calculate sentence length

                TextGenerationSettings settings;
                TextGenerator tg;
                tg = txt.cachedTextGeneratorForLayout;
                settings = txt.GetGenerationSettings(new Vector2(MESSAGE_MAX_WIDTH- MESSAGE_ICONS_WIDTH,0));

                Canvas.ForceUpdateCanvases();
                float txtWidth = tg.GetPreferredWidth(txt.text, settings) / txt.pixelsPerUnit;

                //float txtHeight = tg.GetPreferredHeight(txt.text.Substring(0, 1), settings) / txt.pixelsPerUnit;
                float txtHeight = tg.GetPreferredHeight(txt.text, settings) / txt.pixelsPerUnit;


           

                if (txtWidth + MESSAGE_ICONS_WIDTH < MESSAGE_MIN_WIDTH)
                {
                    //set min width
                    rectChat.offsetMax = new Vector2(rectChat.offsetMin.x + MESSAGE_MIN_WIDTH, rectChat.offsetMax.y);

                }

                if (txtWidth + MESSAGE_ICONS_WIDTH > MESSAGE_MIN_WIDTH && txtWidth<MESSAGE_MAX_WIDTH)
                {
                    //single line, set width
                    float wth = txtWidth + 80f;
                    if (messageType == enumChatMessageType.MessageRight) wth += MESSAGE_RIGHT_MAGIN;
                    rectChat.offsetMax = new Vector2(rectChat.offsetMin.x + wth, rectChat.offsetMax.y);
                }

                if (txtWidth + MESSAGE_ICONS_WIDTH > MESSAGE_MAX_WIDTH)
                {
                    //multiple lines, set max width

                    int lineHeight = (int)((txtWidth  )/ MESSAGE_MAX_WIDTH) - 1;
                    if (lineHeight < 1) lineHeight = 1;

                    //float hh = txtHeight* lineHeight;
                    float hh = txtHeight;
                    rectChat.offsetMax = new Vector2(rectChat.offsetMin.x + MESSAGE_MAX_WIDTH, rectChat.offsetMax.y + hh);

                }

            }

            float rectWidth = rectChat.offsetMax.x - rectChat.offsetMin.x;
            float rectHeight = rectChat.offsetMax.y - rectChat.offsetMin.y;

            if (messageType == enumChatMessageType.MessageLeft)
            {
                //align left
                rectChat.localPosition = new Vector3(rectWidth / 2 - w / 2, h / 2 - rectHeight / 2 - contentHeight);
            }
            else
            {
                //align right
                rectChat.localPosition = new Vector3(w / 2- rectWidth /2 - MESSAGE_RIGHT_MAGIN, h / 2 - rectHeight / 2 - contentHeight);
                
            }

            ChatItem item = new ChatItem();
            item.gameObject = goChat;
            item.text = text;
            item.time = DateTime.Now;
            if (messageType == enumChatMessageType.MessageLeft)
                item.itemType = ChatItem.enumChatItemType.LeftChatMessage;
            else
                item.itemType = ChatItem.enumChatItemType.RightChatMessage;

            chatItems.Add(item);


            contentHeight += rectHeight;
            CalcContentHeight();
        }

       // PlaySound(messageType);
    }

 
    public void ButtonSendMessage()
    {
        if (inputField != null)
        {
            AddChatMessage(inputField.text, enumChatMessageType.MessageRight);
        }
        
    }

    void PlaySound(enumChatMessageType messageType)
    {
        if (audioSource == null)
            return;

        AudioClip clip=null;

        switch (messageType)
        {
            case enumChatMessageType.MessageLeft:
                clip = chatLeftMessageSound;
                break;
            case enumChatMessageType.MessageRight:
                clip = chatRightMessageSound;
                break;
        }

        if (clip == null)
            return;

        audioSource.clip = clip;
        audioSource.Play();
    }

    void InstallContent()
    {
        //float h = rootTransform.rect.height; //rootTransform.offsetMax.y - rootTransform.offsetMin.y;

        RectTransform rect = (RectTransform)contentTransform.parent.parent;
        ////float hp = Mathf.Abs(rect.offsetMax.y - rect.offsetMin.y);
        //float hp = rect.rect.height;

        float h = rect.rect.height;
        //h = h - hp;

        contentTransform.offsetMin = new Vector2(0, 0);
        contentTransform.offsetMax = new Vector2(0, h);
        contentTransform.localPosition = new Vector3(0,0);

    }

    //calculate Content height and goes to bottom
    void CalcContentHeight()
    {
        if (contentHeight <= contentMinSize)
        {
            //contentHeight = contentMinSize;
            return;
        }
        //float w = rootTransform.offsetMax.x - rootTransform.offsetMin.x;
        //float h = rootTransform.offsetMax.y - rootTransform.offsetMin.y;
        RectTransform rect = (RectTransform)contentTransform.parent.parent;
        float h = rect.rect.height;

        //RectTransform rect = (RectTransform)contentTransform.parent.parent;
        ////float hp = Mathf.Abs(rect.offsetMax.y - rect.offsetMin.y);
        //float hp = rect.rect.height;
        //h = h - hp;

        contentTransform.offsetMin = new Vector2(0, 0);
        contentTransform.offsetMax = new Vector2(0, contentHeight);
        contentTransform.localPosition = new Vector3(0, contentHeight/2 - h/2);

    }

    public void ClearChatItems()
    {
        foreach(ChatItem item in chatItems)
        {
            Destroy(item.gameObject);
        }
        chatItems.Clear();

        timeTag = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
        contentHeight = 0;
        InstallContent();
    }

    public ChatItem GetChatItem(int index)
    {
        if (index < 0) return null;
        if (index >= chatItems.Count) return null;

        return chatItems[index];
    }

    public List<ChatItem> GetChatItems()
    {
        return chatItems;
    }


}
