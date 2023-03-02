using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    public static SoundManager SharedInstance;
    public bool stopMenuMusic = true;
    private MusicMenu musicHolder;
    void Start()
    {
        Scene currentScene = SceneManager.GetActiveScene();

        SharedInstance = this;
        musicHolder = FindObjectOfType<MusicMenu>();
        if (musicHolder != null)
        {
            if (stopMenuMusic)
            {
                musicHolder.StopMusic("Menu");
                musicHolder.PlayMusic("Ambiance");
            }
            else
            {
                musicHolder.PlayMusic("Menu");
                musicHolder.StopMusic("Ambiance");
                musicHolder.StopMusic("Chase");
            }
        }
    }

    public void PlaySound(string soundName)
    {
        if (musicHolder != null)
        {
            musicHolder.PlayMusic(soundName);
        }
    }

    public void StopSound(string soundName)
    {
        if (musicHolder != null)
        {
            musicHolder.StopMusic(soundName);
        }
    }

    void Update()
    {

    }
}
