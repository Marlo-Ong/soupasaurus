using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

public class DinoFriendLoader : MonoBehaviour
{
    public List<Image> DinoFriends;
    
    void Start()
    {
        for (int i = 0; i < StateMachine.Instance.NamesOfDinosMet.Count; i++)
        {
            string name = StateMachine.Instance.NamesOfDinosMet[i];
            DinoFriends[i].sprite = StateMachine.DinoSprites[StateMachine.DinoNames.IndexOf(name)];
            DinoFriends[i].gameObject.SetActive(true);
        }
    }

    public void Restart()
    {
        StateMachine.Instance.StateChange(State.Title);
    }
}
