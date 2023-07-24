using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class OpenAIReply : MonoBehaviour
{
    [System.Serializable]
    public class OpenAISendParameter
    {
        public string model  = "gpt-3.5-turbo";
        public List<OpenAISendMessage> messages = new List<OpenAISendMessage>();
    }

    [System.Serializable]
    public class OpenAISendMessage
    {
        public string role = "user";
        public string content = "";
    }


    /// received message
    /// </summary>
    [System.Serializable]
    public class OpenAIResponse
    {
        public string id;
        public List<OpenAIMessage> choices  = new List<OpenAIMessage>();
        public string finish_reason;
    }

    [System.Serializable]
    public class OpenAIMessage
    {
        public OpenAIChoice message  = new OpenAIChoice();
    }

    [System.Serializable]
    public class OpenAIChoice
    {
        public string role;
        public string content;
    }

    // openai API
    private string m_ApiUrl = "https://api.openai.com/v1/chat/completions";


    public string OpenAI_Key = "";
    public string OpenAI_Model = "gpt-3.5-turbo";

    public ChatUI ui;
    // Start is called before the first frame update
    void Start()
    {
        if (ui != null)
            ui.OnChatMessage += Ui_OnChatMessage;

    }

    private void Ui_OnChatMessage(string text, ChatUI.enumChatMessageType messageType)
    {
        if (messageType == ChatUI.enumChatMessageType.MessageRight)
        {
            if (OpenAI_Key=="")
            {
                ui.AddChatMessage("Please set the OpenAI key before using the function!", ChatUI.enumChatMessageType.MessageLeft);
            }
            else
            {
                SendData(text);
            }
        }
    }


    //send message
    public void SendData(string text)
    {
        if (text.Equals(""))
            return;

        StartCoroutine(GetPostData(text, CallBack));
    }

    //AI response
    private void CallBack(string _callback)
    {
        _callback = _callback.Trim();
        ui.AddChatMessage(_callback, ChatUI.enumChatMessageType.MessageLeft);
    }

    private IEnumerator GetPostData(string _postWord, System.Action<string> _callback)
    {
        using (UnityWebRequest request = new UnityWebRequest(m_ApiUrl, "POST"))
        {
            OpenAISendParameter parameter = new OpenAISendParameter();
            if (OpenAI_Model != "")
                parameter.model = OpenAI_Model;

            OpenAISendMessage sendmsg = new OpenAISendMessage();
            sendmsg.content = _postWord;

            parameter.messages.Add(sendmsg);

            string _jsonText = JsonUtility.ToJson(parameter);


            byte[] data = System.Text.Encoding.UTF8.GetBytes(_jsonText);
            request.uploadHandler = (UploadHandler)new UploadHandlerRaw(data);
            request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", string.Format("Bearer {0}", OpenAI_Key));

            yield return request.SendWebRequest();

            if (request.responseCode == 200)
            {
                string _msg = request.downloadHandler.text;
                OpenAIResponse response = JsonUtility.FromJson(_msg, typeof(OpenAIResponse)) as OpenAIResponse;


                if (response != null && response.choices.Count > 0)
                {
                    _callback(response.choices[0].message.content);
                }

            }
            else
            {
                Debug.Log("Error:" + request.responseCode.ToString());
            }

        }

    }
}
