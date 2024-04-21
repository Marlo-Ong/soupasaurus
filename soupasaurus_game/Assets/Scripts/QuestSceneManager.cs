using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class QuestSceneManager : Singleton<QuestSceneManager>
{
    [Header("Panels")]
    public GameObject Panel_Loading;
    public GameObject Ground;
    public GameObject Panel_AISpeechBubble;
    public GameObject Panel_PlayerSpeechBubble;
    public TMP_Text Text_Option1;
    public TMP_Text Text_Option2;

    [Header("Animation Controllers")]
    public AnimationController PlayerDinoAnim;
    public AnimationController AIDinoAnim;

    [Header("Animation Parameters")]
    public float Timing_Delay_AfterLoad;
    public float Timing_Duration_PlayerWalking;
    public float Timing_Duration_BeforeTalk;
    public float Timing_Duration_BeforeGroundRaise;
    public AnimationCurve Curve_GroundRaise;
    public float Timing_Duration_GroundRaise;

    private Vector3 _originalGroundPosition;

    void OnEnable()
    {
        _originalGroundPosition = Ground.transform.localPosition;
        WebLoader.OnMessagePosted += WebLoader_OnMessagePosted;
    }

    public void InitializeScene()
    {
        Debug.Log("Initialized quest scene manager");
        // Change scene
        //SceneManager.LoadSceneAsync(1);
        //StateMachine.Instance.StateChange(State.Questing);

        // Put up loading screen
        Panel_Loading.SetActive(true);

        // Try getting random character
        WebLoader.Instance.GetUserID();
    }

    public void WebLoader_OnMessagePosted(ConvoObject c, bool isFirstMessage)
    {
        // Set speech bubble text
        Panel_AISpeechBubble.GetComponentInChildren<TMP_Text>().text = c.response;

        // Set options text
        Text_Option1.text = c.options[0];
        Text_Option2.text = c.options[1];

        if (isFirstMessage) StartCoroutine(ContinueGameCutscene());
    }

    private IEnumerator ContinueGameCutscene()
    {
        // Close loading screen
        Panel_Loading.SetActive(false);
        yield return new WaitForSeconds(Timing_Delay_AfterLoad);

        // Play dino walking animation
        PlayerDinoAnim.Play();
        yield return new WaitForSeconds(Timing_Duration_PlayerWalking);

        // Stop player walking (sees other dino)
        PlayerDinoAnim.Stop();
        yield return new WaitForSeconds(Timing_Duration_BeforeTalk);

        // Animate AI speech bubble
        Panel_AISpeechBubble.SetActive(true);
        yield return new WaitForSeconds(Timing_Duration_BeforeGroundRaise);

        // Animate ground going up
        float duration = 0;
        while (duration < Timing_Duration_GroundRaise)
        {
            Vector3 offset = new(0, Curve_GroundRaise.Evaluate(duration/Timing_Duration_GroundRaise), 0);
            Ground.transform.localPosition = _originalGroundPosition + offset;
            duration += Time.deltaTime;
            yield return null;
        }
        Debug.Log("ContinueGameCutscene done");
    }
}
