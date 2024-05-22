using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class AudioManager : Singleton<AudioManager>
{
    public EventReference SFX_UIClick;

    public void PlayOneShot(EventReference sound)
    {
        RuntimeManager.PlayOneShot(sound, transform.position);
    }
}
