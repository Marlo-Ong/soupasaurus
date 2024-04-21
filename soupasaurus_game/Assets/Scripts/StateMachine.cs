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

    void Start()
    {
        NamesOfDinosMet = new();
        StateEnter(State.Title);
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
    }

    public void ChooseGameplayLength(int rounds)
    {
        SceneManager.LoadSceneAsync(1);
        NumRounds = rounds;
        StateChange(State.Questing);
    }
}
