using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class MusicMenu : MonoBehaviour
{
    public Sound[] soundList;
    private AudioSource[] audioSourceList;
    private GameObject[] other;
    private bool NotFirst = false;
    private void Awake()
    {
        other = GameObject.FindGameObjectsWithTag("Music");

        Cursor.lockState = CursorLockMode.None;

        foreach (GameObject oneOther in other)
        {
            if (oneOther.scene.buildIndex == -1)
            {
                NotFirst = true;
            }
        }
        if (NotFirst == true)
        {
            Destroy(gameObject);
        }

        audioSourceList = new AudioSource[soundList.Length];
        for (int i = 0; i < soundList.Length; i++)
        {
            audioSourceList[i] = gameObject.AddComponent<AudioSource>();
            audioSourceList[i].loop = soundList[i].loop;
            audioSourceList[i].playOnAwake = false;
            audioSourceList[i].clip = soundList[i].audioClip;
            audioSourceList[i].volume = soundList[i].audioVolume;
        }
        PlayMusic("Menu");
        DontDestroyOnLoad(gameObject);
    }

    public void PlayMusic(string soundName)
    {
        for (int i = 0; i < soundList.Length; i++)
        {
            if (soundList[i].name == soundName)
            {
                if (audioSourceList[i].isPlaying && soundList[i].loop) return;
                else if (audioSourceList[i].isPlaying) audioSourceList[i].Stop();
                soundList[i].setPlay(true);
                audioSourceList[i].Play();
            }
        }
    }

    public void StopMusic(string soundName)
    {
        for (int i = 0; i < soundList.Length; i++)
        {
            if (soundList[i].name == soundName)
            {
                if (!audioSourceList[i].isPlaying) return;
                soundList[i].setPlay(false);
                if (!soundList[i].loop)
                    audioSourceList[i].Stop();
            }
        }
    }

    void Update()
    {
        for (int i = 0; i < soundList.Length; i++)
        {
            if (soundList[i].loop && ((audioSourceList[i].volume != 0 && !soundList[i].play) || (audioSourceList[i].volume != soundList[i].maxAudioVolume && soundList[i].play)))
            {
                soundList[i].elapsedTime += Time.deltaTime;
                if (soundList[i].play)
                {
                    audioSourceList[i].volume = Mathf.Lerp(0, soundList[i].maxAudioVolume, soundList[i].elapsedTime / 2f);
                }
                else
                {
                    audioSourceList[i].volume = Mathf.Lerp(soundList[i].maxAudioVolume, 0, soundList[i].elapsedTime / 2f);
                }
                if (soundList[i].elapsedTime >= 2f) soundList[i].elapsedTime = 0;
            }
            else if (soundList[i].loop && audioSourceList[i].volume == 0 && !soundList[i].play && audioSourceList[i].isPlaying)
            {
                soundList[i].elapsedTime = 0;
                audioSourceList[i].Stop();
            }
        }
    }
}