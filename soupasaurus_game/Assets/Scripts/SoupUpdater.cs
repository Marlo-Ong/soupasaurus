using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class SoupUpdater : MonoBehaviour
{
    public TMP_Text IngredientList;
    public TMP_Text SoupName;
    public TMP_Text MBTIDescription;
    public Dictionary<string, string> MBTIDescriptions;

    void Start()
    {
        MBTIDescriptions = new()
        {
            { "ISTJ", "Responsible, organized, traditional, and methodical. They value stability and order." },
            { "ISFJ", "Compassionate, conscientious, and meticulous. They prioritize harmony and helping others." },
            { "INFJ", "Insightful, empathetic, and visionary. They are deeply introspective and value authenticity." },
            { "INTJ", "Strategic, analytical, and independent. They are driven by logic and long-term vision." },
            { "ISTP", "Adventurous, logical, and spontaneous. They enjoy exploring and solving practical problems." },
            { "ISFP", "Creative, sensitive, and adaptable. They appreciate beauty and enjoy expressing themselves through art." },
            { "INFP", "Idealistic, compassionate, and introspective. They seek meaning and authenticity in life." },
            { "INTP", "Innovative, analytical, and curious. They enjoy exploring ideas and theoretical concepts." },
            { "ESTP", "Energetic, pragmatic, and adventurous. They thrive in dynamic environments and enjoy taking risks." },
            { "ESFP", "Outgoing, spontaneous, and enthusiastic. They enjoy being in the spotlight and making connections." },
            { "ENFP", "Enthusiastic, imaginative, and empathetic. They are driven by their ideals and enjoy inspiring others." },
            { "ENTP", "Inventive, resourceful, and unconventional. They enjoy debating ideas and exploring possibilities." },
            { "ESTJ", "Efficient, practical, and organized. They thrive in structured environments and value tradition." },
            { "ESFJ", "Friendly, conscientious, and supportive. They prioritize harmony and social connections." },
            { "ENFJ", "Charismatic, empathetic, and diplomatic. They inspire and motivate others toward a common goal." },
            { "ENTJ", "Assertive, strategic, and ambitious. They are natural leaders who enjoy tackling challenges head-on." }
        };

        WebLoader.OnGetSoup += WebLoader_OnGetSoup;
        WebLoader.Instance.GetSoup();
    }


    void WebLoader_OnGetSoup(SoupObject s)
    {
        SoupName.text = s.soup_name + $" ({s.mbti})";
        MBTIDescription.text = MBTIDescriptions[s.mbti];
        string t = "";
        foreach (string ing in s.ingredients)
        {
            t += ing + ", ";
        }
        IngredientList.text = t;
    }

    public void NextScene()
    {
        SceneManager.LoadSceneAsync(4);
    }
}
