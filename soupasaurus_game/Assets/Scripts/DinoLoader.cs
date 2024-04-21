using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DinoLoader : Singleton<DinoLoader>
{
    // Dictionary not serializable in inspector
    public List<string> dinoNames;
    public List<Sprite> dinoSprites;
    public SpriteRenderer OtherDinoSpriteR;

    
    void OnEnable()
    {
        WebLoader.OnMessagePosted += WebLoader_OnMessagePosted;
    }

    private void WebLoader_OnMessagePosted(ConvoObject c, bool _)
    {
        OtherDinoSpriteR.sprite = dinoSprites[dinoNames.IndexOf(c.character_name)];
        GetComponent<AnimationController>().Play();
    }
}
