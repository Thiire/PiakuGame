using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class PortalActivator : MonoBehaviour
{
    private VisualEffect PortalEffect;
    private bool portalEnabled = false;
    void Start()
    {
        PortalEffect = GetComponent<VisualEffect>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!portalEnabled && other.tag == "Player")
        {
            SoundManager.SharedInstance.PlaySound("Portal");
            portalEnabled = true;
            PortalEffect.Play();
        }
    }

    void Update()
    {

    }
}
