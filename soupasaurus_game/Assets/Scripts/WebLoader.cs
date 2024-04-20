using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class OptionsObject
{
    public string user_id;
    public string convo_id;
    public string[] options;
}

[Serializable]
public class UserIDObject
{
    public string user_id;
}

[Serializable]
public class ConvoObject
{
    public string user_id;
    public string convo_id;
    public string character_name;
    public string response;
    public string[] options;
    public bool done;
}

public class WebLoader : Singleton<WebLoader>
{
    public string UserID;
    public string ConversationID;
    public string URI = "http://dell-wsl.mango-tone.ts.net:8000";
    //public string URI = "localhost:8000";

    void Start()
    {
        Debug.Log("Start");
        StartCoroutine(GetUserID());
    }

    IEnumerator PostNewMessage(string user_id = null, string convo_id = null)
    {
        Debug.Log("Start PostNewMessage");
        if (user_id == null)
        {
            Debug.LogWarning("No user ID found when trying to POST /conversation");
            yield break;
        }

        string uri;
        if (convo_id == null) uri = $"{URI}/conversation?user_id={user_id}";
        else uri = $"{URI}/conversation?user_id={user_id}&conversation_id={convo_id}";

        // Create the UnityWebRequest
        using UnityWebRequest uwr = UnityWebRequest.Post(uri, new WWWForm());

        // Send the request and wait for it to complete
        yield return uwr.SendWebRequest();

        if (uwr.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.LogWarning("Error: " + uwr.error);
        }
        else
        {
            ConvoObject c = JsonUtility.FromJson<ConvoObject>(uwr.downloadHandler.text);
            ConversationID = c.convo_id;
            Debug.Log($"CONVO ID: {c.convo_id} / NAME: {c.character_name} / RESPONSE: {c.response}");
        }
    }

    IEnumerator GetUserID()
    {
        Debug.Log("Start GetUserID");
        // Construct the URI with the score
        string uri = $"{URI}/new_user_id";

        // Create the UnityWebRequest
        using UnityWebRequest uwr = UnityWebRequest.Get(uri);

        // Send the request and wait for it to complete
        yield return uwr.SendWebRequest();

        if (uwr.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.LogWarning("Error: " + uwr.error);
        }
        else
        {
            UserIDObject c = JsonUtility.FromJson<UserIDObject>(uwr.downloadHandler.text);
            Debug.Log($"USER ID: {c.user_id}");
            UserID = c.user_id;
            //StartCoroutine(GetConvoID());
            StartCoroutine(PostNewMessage(UserID));
        }
    }

    IEnumerator GetNewOptions(string user_id = null, string conversation_id = null)
    {
        if (user_id == null || conversation_id == null)
        {
            Debug.LogWarning("No user ID or convo ID found when trying to POST /new_options");
            yield break;
        }

        // Construct the URI with the score
        string uri = $"{URI}/new_options?user_id={user_id}&conversation_id={conversation_id}";

        // Create the UnityWebRequest
        using UnityWebRequest uwr = UnityWebRequest.Get(uri);

        // Send the request and wait for it to complete
        yield return uwr.SendWebRequest();

        if (uwr.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.LogWarning("Error: " + uwr.error);
        }
        else
        {
            OptionsObject c = JsonUtility.FromJson<OptionsObject>(uwr.downloadHandler.text);
            Debug.Log($"CONVO ID: {c.convo_id}");
            ConversationID = c.convo_id;
            StartCoroutine(PostNewMessage(UserID, ConversationID));
        }
    }
}