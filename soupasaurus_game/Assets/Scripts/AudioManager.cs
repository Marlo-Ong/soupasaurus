using System;
using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    public EventReference SFX_UIClick;
    public EventReference SFX_DingBell2;
    public EventReference SFX_DingBell3;
    public EventReference MUS_Background;
    private static Dictionary<string, EventInstance> musicInstances;

    private const string FMODParam_PlayingCutscene = "PlayingCutscene";

    public bool IsCutscenePlaying
    {
        set
        {
            FMODUnity.RuntimeManager.StudioSystem.setParameterByName(FMODParam_PlayingCutscene, value ? 1.0f : 0.0f);
        }
    }

    protected override void Awake()
    {
        base.Awake();

        musicInstances ??= new();

        StartCoroutine(LoadBanksAsync());
    }

    private IEnumerator LoadBanksAsync()
    {
        while (!RuntimeManager.HaveAllBanksLoaded)
        {
            yield return null;
        }

        // Keep yielding the co-routine until all the sample data loading is done
        while (FMODUnity.RuntimeManager.AnySampleDataLoading())
        {
            yield return null;
        }

        Debug.Log("Loaded all banks");

        this.PlayMusic(this.MUS_Background);
    }

    public void PlayMusic(EventReference eventRef)
    {
        PlayMusic(eventRef.Path);
    }

    public void PlayMusic(string path)
    {
        if (IsEventPlaying(path))
            return;

        musicInstances[path] = RuntimeManager.CreateInstance(path);
        musicInstances[path].start();
    }

    public void PlayOneShot(EventReference sound)
    {
        RuntimeManager.PlayOneShot(sound, transform.position);
    }

    public void PlayDinoTheme(string dinoName)
    {
        try
        {
            string dinoTheme = $"event:/Music/Dinos/{dinoName}";
            PlayMusic(dinoTheme);
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }

    private bool IsEventPlaying(string path)
    {
        if (musicInstances.ContainsKey(path))
        {
            musicInstances[path].getPlaybackState(out PLAYBACK_STATE state);
            if (state == PLAYBACK_STATE.PLAYING || state == PLAYBACK_STATE.STARTING)
                return true;
        }

        return false;
    }
}
