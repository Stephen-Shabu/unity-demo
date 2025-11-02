using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum AudioType
{
    Hit,
    Death,
    UI,
    Projectile
}

[Serializable]
public class AudioData
{
    public AudioType Type => type;
    public AudioClip[] Clips => clips;

    [SerializeField] private AudioType type;
    [SerializeField] private AudioClip[] clips;
}

[CreateAssetMenu(menuName = "Audio/SoundProfile")]
public class AudioProfile : ScriptableObject
{
    public AudioData[] audioDataset;

    private Dictionary<AudioType, AudioData> audioDict = new Dictionary<AudioType, AudioData>();

    private void OnEnable()
    {
        foreach (var set in audioDataset)
        {
            if (!audioDict.ContainsKey(set.Type))
            {
                audioDict.Add(set.Type, set);
            }
        }
    }

    public AudioClip[] GetAudioByType(AudioType type)
    {
        AudioClip[] clips = {};

        return audioDict.TryGetValue(type, out var data) ? data.Clips : null;
    }
}
