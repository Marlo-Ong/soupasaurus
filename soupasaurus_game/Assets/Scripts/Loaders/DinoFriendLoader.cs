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
        int i = 0;
        foreach(string name in StateMachine.Instance.NamesOfDinosMet.ToHashSet())
        {
            DinoFriends[i].sprite = DinoLoader.Instance.dinoSprites[DinoLoader.Instance.dinoNames.IndexOf(name)];
            DinoFriends[i].gameObject.SetActive(true);
            i++;
        }
    }

    public void Restart()
    {
        SceneManager.LoadScene(0);
    }
}
