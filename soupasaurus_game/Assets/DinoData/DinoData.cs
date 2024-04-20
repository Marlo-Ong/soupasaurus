using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/DinoData", order = 1)]
public class DinoData : ScriptableObject
{
    public Image DinoSprite;
    public string DinoName;
    public string DinoDescription;
}
