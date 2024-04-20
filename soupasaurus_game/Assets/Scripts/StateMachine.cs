using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum State
{
    StartCutscene,
    Title,
    Menu,
    Questing
}

public class StateMachine : MonoBehaviour
{
    void Start()
    {
        StateEnter(State.Title);
    }

    public void StateEnter(State s)
    {
        
    }
}
