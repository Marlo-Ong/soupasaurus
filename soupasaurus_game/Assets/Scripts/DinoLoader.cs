using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DinoLoader : Singleton<DinoLoader>
{
    // Dictionary not serializable in inspector
    public List<string> dinoNames;
    public List<Sprite> dinoSprites;
    public SpriteRenderer OtherDinoSpriteR;
    public List<Image> DinoFriends;

    
    void OnEnable()
    {
        WebLoader.OnInitialMessage += WebLoader_OnMessagePosted;
    }

    private void WebLoader_OnMessagePosted(ConvoObject c)
    {
        OtherDinoSpriteR.sprite = dinoSprites[dinoNames.IndexOf(c.character_name)];
        GetComponent<AnimationController>().Play();
    }
}
