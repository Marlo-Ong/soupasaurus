using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// Listens for GET ingredient given by dino, adds it to banner
/// </summary>
public class IngredientLoader : MonoBehaviour
{
    private Dictionary<string, Sprite> _ingredientNameToSprite;
    public List<Sprite> IngredientSprites;
    public GameObject IngredientPrefab;
    public GameObject IngredientBanner;

    void Start()
    {
        _ingredientNameToSprite = new()
        {
            { "extroverted", IngredientSprites[0] }, // bold soup base
            { "introverted", IngredientSprites[1] }, // light soup base
            { "sensing", IngredientSprites[2] },     // typical veggie
            { "intuitive", IngredientSprites[3] },   // exotic veggie
            { "thinking", IngredientSprites[4] },    // typical spice
            { "feeling", IngredientSprites[5] },     // exotic spice
            { "judging", IngredientSprites[6] },     // red meat/fish
            { "perceiving", IngredientSprites[7] },  // white meat/fish
        };
        WebLoader.OnGetConvo += WebLoader_OnGetConvo;
    }

    public void WebLoader_OnGetConvo(CompletedConvoObject c)
    {
        GameObject newIngredient = Instantiate(IngredientPrefab);
        newIngredient.GetComponent<SpriteRenderer>().sprite = _ingredientNameToSprite[c.mtbi];
        newIngredient.transform.SetParent(IngredientBanner.transform);
    }
}
