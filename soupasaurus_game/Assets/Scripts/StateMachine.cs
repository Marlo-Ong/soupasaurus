using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public enum State
{
    StartCutscene,
    Title,
    Menu,
    Questing,
    Fin,
    Friends
}

public class StateMachine : Singleton<StateMachine>
{
    public static event UnityAction<State> OnStateEnter;
    public static event UnityAction<State> OnStateExit;
    public State CurrentState;
    public int NumRounds;
    public List<string> NamesOfDinosMet;
    public GameObject ErrorCanvas;

    void Start()
    {
        NamesOfDinosMet = new();
        StateEnter(State.Title);
    }

    void Update()
    {
        bool isLandscape = Screen.orientation == ScreenOrientation.LandscapeLeft || Screen.orientation == ScreenOrientation.LandscapeRight;
        ErrorCanvas.SetActive(!isLandscape);
    }

    public void StateChange(State s)
    {
        StateExit(CurrentState);
        CurrentState = s;
        StateEnter(s);
        Debug.Log($"{this}'s state was changed to {s}");
    }

    public void StateEnter(State s)
    {
        switch (s)
        {
            case State.StartCutscene:
                break;
            case State.Title:
                break;
            case State.Menu:
                break;
            case State.Questing:
                QuestSceneManager.Instance.InitializeScene();
                break;
            case State.Fin:
                StartCoroutine(StartSoupScene());
                break;
            case State.Friends:
                break;
        }

        OnStateEnter?.Invoke(s);
    }

    public void StateExit(State s)
    {
        switch (s)
        {
            case State.StartCutscene:
                break;
            case State.Title:
                break;
            case State.Menu:
                break;
            case State.Questing:
                break;
        }

        OnStateExit?.Invoke(s);
    }

    public void StartCooking()
    {
        GameObject.Find("Canvas").SetActive(false);
        GameObject.Find("Selection").SetActive(true);
    }

    public void ChooseGameplayLength(int rounds)
    {
        SceneManager.LoadScene(1);
        NumRounds = rounds;
        StateChange(State.StartCutscene);
    }

    IEnumerator StartSoupScene()
    {
        AsyncOperation job = SceneManager.LoadSceneAsync(3);
        while (!job.isDone)
        {
            yield return null;
        }
    }
}
