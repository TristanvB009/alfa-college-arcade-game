using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Random = UnityEngine.Random;

public class SoundEffectLibrary : MonoBehaviour
{
    [SerializeField] private SoundEffectGroup[] soundEffectGroups;
    //the string = name of audio
    private Dictionary<string, List<AudioClip>> _soundDictionary;

    private void Awake()
    {
        InitializeDictionary();
    }

    private void InitializeDictionary()
    {
        _soundDictionary = new Dictionary<string, List<AudioClip>>();
        foreach (SoundEffectGroup soundEffectGroup in soundEffectGroups)
        {
            _soundDictionary[soundEffectGroup.name] = soundEffectGroup.audioClips;
        }
    }

    public AudioClip GetRandomClip(string name)
    {
        if (_soundDictionary.TryGetValue(name, out List<AudioClip> audioClips) && audioClips.Count > 0)
        {
            return audioClips[Random.Range(0, audioClips.Count)];
        }
        return null;
    }

    // Preload SFX into memory to avoid first-play delay for compressed clips
    public void PreloadAll()
    {
        if (soundEffectGroups == null) return;

        // Kick off load for each clip; start a coroutine to wait until loading completes.
        foreach (var group in soundEffectGroups)
        {
            if (group.audioClips == null) continue;
            foreach (var clip in group.audioClips)
            {
                if (clip == null) continue;
                if (clip.loadState == AudioDataLoadState.Unloaded)
                    clip.LoadAudioData(); // request load/decompression
            }
        }

        // Optional: wait until all clips are loaded (runs asynchronously)
        StartCoroutine(WaitForAllLoads());
    }

    private IEnumerator WaitForAllLoads()
    {
        bool anyLoading;
        do
        {
            anyLoading = false;
            foreach (var group in soundEffectGroups)
            {
                if (group.audioClips == null) continue;
                foreach (var clip in group.audioClips)
                {
                    if (clip == null) continue;
                    if (clip.loadState == AudioDataLoadState.Loading)
                    {
                        anyLoading = true;
                        break;
                    }
                }
                if (anyLoading) break;
            }
            yield return null;
        } while (anyLoading);
    }
}

[System.Serializable]
//class to populate the sound effect dictionary
public struct SoundEffectGroup
{
    public string name;
    public List<AudioClip> audioClips;
}
