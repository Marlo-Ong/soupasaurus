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
    End,
    Soup
}

public class StateMachine : Singleton<StateMachine>
{
    public static event UnityAction<State> OnStateEnter;
    public static event UnityAction<State> OnStateExit;
    public State CurrentState;

    void Start()
    {
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
        SceneManager.LoadSceneAsync(1);
        StateChange(State.Questing);
    }
}
