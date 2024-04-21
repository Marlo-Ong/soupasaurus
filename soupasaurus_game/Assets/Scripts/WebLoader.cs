using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.Events;

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
    public string URI = "https://ml-1-wsl.mango-tone.ts.net";
    public static event UnityAction<ConvoObject, bool> OnMessagePosted;
    public static event UnityAction<string[]> OnNewOptionsGot;
    public static event UnityAction<string> OnUserIDGot;

    public void GetUserID()
    {
        StartCoroutine(ContinueGetUserID());
    }

    public void PostNewMessage()
    {
        StartCoroutine(ContinuePostNewMessage());
    }

    public void GetNewOptions()
    {
        StartCoroutine(ContinueGetNewOptions());
    }

    IEnumerator ContinueGetUserID()
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
            Debug.Log($"USER ID: {c.user_id} / text: {uwr.downloadHandler.text} / url: {uri}");
            UserID = c.user_id;
            OnUserIDGot?.Invoke(c.user_id);
        }
        PostNewMessage();
    }

    IEnumerator ContinuePostNewMessage()
    {
        Debug.Log("Start PostNewMessage");
        bool isFirstMessage = false;

        if (string.IsNullOrEmpty(UserID))
        {
            Debug.LogWarning("No user ID found when trying to POST /conversation");
            yield break;
        }

        string uri;
        if (string.IsNullOrEmpty(ConversationID))
        {
            uri = $"{URI}/conversation?user_id={UserID}";
            isFirstMessage = true;
        }
        else uri = $"{URI}/conversation?user_id={UserID}&conversation_id={ConversationID}";

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
            Debug.Log($"URI: {uri}, first: {isFirstMessage}");
            OnMessagePosted?.Invoke(c, isFirstMessage);
        }
    }

    IEnumerator ContinueGetNewOptions()
    {
        if (string.IsNullOrEmpty(UserID) || string.IsNullOrEmpty(ConversationID))
        {
            Debug.LogWarning("No user ID or convo ID found when trying to POST /new_options");
            yield break;
        }

        // Construct the URI with the score
        string uri = $"{URI}/new_options?user_id={UserID}&conversation_id={ConversationID}";

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
            Debug.Log(uwr.downloadHandler.text);
            ConversationID = c.convo_id;
        }
    }
}