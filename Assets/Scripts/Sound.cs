using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip audioClip;
    public bool loop = false;
    [Range(0f, 1f)] public float audioVolume;
    [Range(0f, 1f)] public float maxAudioVolume;
    public float elapsedTime = 0;
    public bool play { get; private set; } = false;

    public void setPlay(bool isPlaying)
    {
        play = isPlaying;
    }
}
