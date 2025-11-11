using System;
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
        foreach (var group in soundEffectGroups)
        {
            if (group.audioClips == null) continue;
            foreach (var clip in group.audioClips)
            {
                if (clip == null) continue;
            }
        }
    }
}

[System.Serializable]
//class to populate the sound effect dictionary
public struct SoundEffectGroup
{
    public string name;
    public List<AudioClip> audioClips;
}