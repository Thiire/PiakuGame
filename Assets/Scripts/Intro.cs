using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.Video;
public class Intro : MonoBehaviour {
    public VideoPlayer vid;

    void Start()
    {
        vid.loopPointReached += CheckOver;
    }
 
    void CheckOver(UnityEngine.Video.VideoPlayer vp)
    {
        SceneManager.LoadScene(1);
    }
 
}