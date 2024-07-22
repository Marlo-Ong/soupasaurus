using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/DinoInfo")]
public class DinosaurInfo : ScriptableObject
{
    public List<string> dinoNames;
    public List<Sprite> dinoSprites;    
}
