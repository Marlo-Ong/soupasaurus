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
    public string conversation_id;
    public string[] options;
}

[Serializable]
public class SoupObject
{
    public string user_id;
    public string[] ingredients;
    public string mtbi;
    public string soup_name;
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
    public string conversation_id;
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
    public static event UnityAction<ConvoObject> OnInitialMessage;
    public static event UnityAction<ConvoObject> OnSubsequentMessage;
    public static event UnityAction<string[]> OnNewOptionsGot;
    public static event UnityAction<string> OnUserIDGot;
    public static event UnityAction<SoupObject> OnGetSoup;

    public void GetUserID()
    {
        StartCoroutine(ContinueGetUserID());
    }

    public void GetSoup()
    {
        StartCoroutine(ContinueGetSoup());
    }

    public void PostInitialMessage()
    {
        StartCoroutine(ContinuePostInitialMessage());
    }

    public void PostSubsequentMessage(int i)
    {
        StartCoroutine(ContinuePostSubsequentMessage(i));
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
    }

    IEnumerator ContinuePostInitialMessage()
    {
        Debug.Log("Start PostInitialMessage");

        if (string.IsNullOrEmpty(UserID))
        {
            Debug.LogWarning("No user ID found when trying to POST /conversation");
            yield break;
        }

        string uri;
        uri = $"{URI}/conversation?user_id={UserID}";

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
            ConversationID = c.conversation_id;
            Debug.Log("Successfully posted initial message.");
            Debug.Log($"Unpacked JSON: {c.conversation_id}, {c.response} / Given JSON: {uwr.downloadHandler.text}");
            OnInitialMessage?.Invoke(c);
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
            Debug.Log($"Successfully got new options: {c.options[0]} / {c.options[1]}");
            OnNewOptionsGot?.Invoke(c.options);
        }
    }

    IEnumerator ContinuePostSubsequentMessage(int selectedOptionIndex)
    {
        Debug.Log("Start PostSubsequentMessage");

        if (string.IsNullOrEmpty(UserID) || string.IsNullOrEmpty(ConversationID))
        {
            Debug.LogWarning("No user ID or convo ID found when trying to POST /conversation");
            yield break;
        }

        string uri;
        uri = $"{URI}/conversation?user_id={UserID}&conversation_id={ConversationID}&selected_option_index={selectedOptionIndex}";

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
            Debug.Log($"Successfully posted subsequent message.");
            OnSubsequentMessage?.Invoke(c);
        }
    }

    IEnumerator ContinueGetSoup()
    {
        if (string.IsNullOrEmpty(UserID) || string.IsNullOrEmpty(ConversationID))
        {
            Debug.LogWarning("No user ID or convo ID found when trying to GET /soup");
            yield break;
        }

        // Construct the URI with the score
        string uri = $"{URI}/soup/{UserID}";

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
            SoupObject c = JsonUtility.FromJson<SoupObject>(uwr.downloadHandler.text);
            Debug.Log($"Successfully got soup: {uwr.downloadHandler.text}");
            OnGetSoup?.Invoke(c);
        }
    }
}