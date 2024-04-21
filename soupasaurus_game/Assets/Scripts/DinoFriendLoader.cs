using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DinoFriendLoader : MonoBehaviour
{
    public List<Image> DinoFriends;
    
    void Start()
    {
        int i = 0;
        foreach(string name in StateMachine.Instance.NamesOfDinosMet)
        {
            DinoFriends[i++].sprite = DinoLoader.Instance.dinoSprites[DinoLoader.Instance.dinoNames.IndexOf(name)];
            DinoFriends[i].gameObject.SetActive(true);
        }
    }

    public void Restart()
    {
        SceneManager.LoadSceneAsync(0);
    }
}
