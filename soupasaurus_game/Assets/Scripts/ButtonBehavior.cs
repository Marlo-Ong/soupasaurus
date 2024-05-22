using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonBehavior : MonoBehaviour
{
    public void Click()
    {
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.SFX_UIClick);
    }
}
