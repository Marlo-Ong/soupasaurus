using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum Biome
{
    BlueBisque,
    SimmeringSunsets,
    MarsMinestrone
}

public class QuestSceneManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject Panel_Loading;
    public GameObject Ground;
    public GameObject Panel_AISpeechBubble;
    public GameObject Panel_SpeechLoadingDots;
    public List<Button> SceneButtons;
    public TMP_Text Text_Option1;
    public TMP_Text Text_Option2;
    public TMP_Text Text_DinoName;

    [Header("Level Information")]
    public List<BiomeData> Biomes;
    public Image Level_Background;
    public Image Level_Floor;
    private int currentLevelIndex = -1;
    private int maxLevels {get {return StateMachine.Instance.NumRounds;}}

    [Header("Animation Controllers")]
    public AnimationController PlayerDinoAnim;
    public AnimationController AIDinoAnim;

    [Header("Animation Parameters")]
    public float Timing_Delay_AfterLoad;
    public float Timing_Duration_BeforeGroundRaise;
    public AnimationCurve Curve_GroundRaise;
    public float Timing_Duration_GroundRaise;
    public float Timing_Delay_BeforeNextLevel;

    private Vector3 _originalGroundPosition;

    void Start()
    {
        StateMachine.Instance.StateChange(State.Questing);
    }

    void OnEnable()
    {
        StateMachine.OnStateEnter += InitializeScene;
        WebLoader.OnInitialMessage += WebLoader_OnInitialMessage;
        WebLoader.OnSubsequentMessage += WebLoader_OnSubsequentMessage;
        WebLoader.OnNewOptionsGot += WebLoader_OnNewOptionsGot;
        WebLoader.OnUserIDGot += WebLoader_OnUserIDGot;
    }

    void OnDestroy()
    {
        StateMachine.OnStateEnter -= InitializeScene;
        WebLoader.OnInitialMessage -= WebLoader_OnInitialMessage;
        WebLoader.OnSubsequentMessage -= WebLoader_OnSubsequentMessage;
        WebLoader.OnNewOptionsGot -= WebLoader_OnNewOptionsGot;
        WebLoader.OnUserIDGot -= WebLoader_OnUserIDGot;
    }

    public void InitializeScene(State s)
    {
        if (s == State.Questing)
        {
            Debug.Log("Initialized quest scene manager");
            _originalGroundPosition = Ground.transform.localPosition;

            // Initialize Gemini handshake
            WebLoader.Instance.GetUserID();
        }
    }

    public void WebLoader_OnUserIDGot(string _)
    {
        CreateLevel();
    }

    public void CreateLevel()
    {
        currentLevelIndex++;

        if (currentLevelIndex >= 0)
        {
            Level_Background.sprite = Biomes[currentLevelIndex % 3].Background;
            Level_Floor.sprite = Biomes[currentLevelIndex % 3].Floor;
        }

        // Put up loading screen
        Panel_Loading.SetActive(true);
        Panel_AISpeechBubble.SetActive(false);

        // Try getting random character
        WebLoader.Instance.PostInitialMessage();
    }

    #region WebLoader

    public void WebLoader_OnInitialMessage(ConvoObject c)
    {
        WebLoader_OnSubsequentMessage(c);
        StartCoroutine(ContinueGameCutscene());
    }

    public void WebLoader_OnSubsequentMessage(ConvoObject c)
    {
        // Set speech bubble text
        Panel_SpeechLoadingDots.SetActive(false);
        Panel_AISpeechBubble.GetComponentInChildren<TMP_Text>().text = c.response;
        Text_DinoName.text = c.character_name;
        StateMachine.Instance.NamesOfDinosMet.Add(c.character_name);

        if (c.done)
        {
            Text_Option1.text = "";
            Text_Option2.text = "";
            StartCoroutine(NextLevel());
        }

        else
        {
            ToggleAllButtonsActive(true);

            // Set options text
            Text_Option1.text = c.options[0];
            Text_Option2.text = c.options[1];
        }
    }

    public void WebLoader_OnNewOptionsGot(string[] c)
    {
        // Set options text
        Text_Option1.text = c[0];
        Text_Option2.text = c[1];

        ToggleAllButtonsActive(true);
    }

    #endregion

    private IEnumerator ContinueGameCutscene()
    {
        // Close loading screen
        Panel_Loading.SetActive(false);
        yield return new WaitForSeconds(Timing_Delay_AfterLoad);

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

    private IEnumerator NextLevel()
    {
        Debug.Log($"Ending level {currentLevelIndex}");

        // Get ingredient given by dino
        WebLoader.Instance.GetConversation();
        
        // Animate ground going down
        float duration = Timing_Duration_GroundRaise;
        while (duration >= 0)
        {
            Vector3 offset = new(0, Curve_GroundRaise.Evaluate(duration/Timing_Duration_GroundRaise), 0);
            Ground.transform.localPosition = _originalGroundPosition + offset;
            duration -= Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(Timing_Delay_BeforeNextLevel);

        if (currentLevelIndex < maxLevels - 1)
        {
            CreateLevel();
        }

        else
        {
            StateMachine.Instance.StateChange(State.Fin);
        }
    }

    // Called by OptionButtons
    public void OnButtonPressed_OptionOne()
    {
        Panel_AISpeechBubble.GetComponentInChildren<TMP_Text>().text = "";
        Panel_SpeechLoadingDots.SetActive(true);
        ToggleAllButtonsActive(false);
        WebLoader.Instance.PostSubsequentMessage(0);
    }

    public void OnButtonPressed_OptionTwo()
    {
        Panel_AISpeechBubble.GetComponentInChildren<TMP_Text>().text = "";
        Panel_SpeechLoadingDots.SetActive(true);
        ToggleAllButtonsActive(false);
        WebLoader.Instance.PostSubsequentMessage(1);
    }

    public void Refresh()
    {
        ToggleAllButtonsActive(false);
        WebLoader.Instance.GetNewOptions();
    }

    private void ToggleAllButtonsActive(bool on)
    {
        foreach(Button b in SceneButtons)
        {
            b.interactable = on;
        }
    }
}
