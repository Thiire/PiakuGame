using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalManager : MonoBehaviour
{
    [Header("sceneNumber, see build options")]
    public int sceneNumber = 3;
    private bool teleported = false;
    void Start()
    {
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" && !teleported)
        {
            teleported = true;
            SceneManager.LoadScene(sceneNumber);
        }
    }

    void Update()
    {

    }
}
