using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Biome", menuName = "ScriptableObjects/BiomeData", order = 2)]
public class BiomeData : ScriptableObject
{
    public string Name;
    public Sprite Floor;
    public Sprite Background;
}
