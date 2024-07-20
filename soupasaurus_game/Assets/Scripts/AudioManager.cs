using System;
using System.Collections;
using FMODUnity;
using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    public EventReference SFX_UIClick;
    public EventReference MUS_Background;
    private FMOD.Studio.EventInstance MUS_Instance_Background;
    private FMOD.Studio.EventInstance MUS_Instance_DinoTheme;

    private const string FMODParam_PlayingCutscene = "PlayingCutscene";

    public bool IsCutscenePlaying
    {
        set
        {
            FMODUnity.RuntimeManager.StudioSystem.setParameterByName(FMODParam_PlayingCutscene, value ? 1.0f : 0.0f);
        }
    }

    void Start()
    {
        StartCoroutine(LoadBanksAsync());
    }

    private IEnumerator LoadBanksAsync()
    {
        while (!FMODUnity.RuntimeManager.HaveAllBanksLoaded)
        {
            yield return null;
        }

        // Keep yielding the co-routine until all the sample data loading is done
        while (FMODUnity.RuntimeManager.AnySampleDataLoading())
        {
            yield return null;
        }

        Debug.Log("Loaded all banks");

        this.MUS_Instance_Background = RuntimeManager.CreateInstance(MUS_Background);
        this.MUS_Instance_Background.start();
    }

    public void PlayOneShot(EventReference sound)
    {
        RuntimeManager.PlayOneShot(sound, transform.position);
    }

    public void PlayDinoTheme(string dinoName)
    {
        string dinoTheme = $"event:/Music/Dino/{dinoName}";
        try
        {
            this.MUS_Instance_DinoTheme = RuntimeManager.CreateInstance(dinoTheme);
            this.MUS_Instance_DinoTheme.start();
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }
}
