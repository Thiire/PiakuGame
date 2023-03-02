using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagePortal : MonoBehaviour
{
    [Header("PortalMaterial")]
    public Material PortalMat;
    [Header("Portal")]
    public bool PortalOpen = false;
    [Range(0.3f, 1f)] public float timeToOpen = 1f;
    [Range(0f, 3f)] public float delayToOpen = 0.2f;

    private bool isOpenning = false;
    private float timeSinceOpening = 0f;

    void Start()
    {
        PortalMat.SetFloat("_lerpValue", 0);
        if (!PortalOpen)
        {
            PortalMat.SetFloat("_intensity", -2.5f);
        }
        else
        {
            PortalMat.SetFloat("_intensity", 3f);
        }
    }

    void Update()
    {
        if (!SkillsManager.SharedInstance.Teleport)
            return;

        if (Input.GetButtonDown("SwapPortal") && !isOpenning)
        {
            isOpenning = true;
            PortalOpen = !PortalOpen;
        }
        if (isOpenning)
        {
            timeSinceOpening += Time.deltaTime;
            if (timeSinceOpening <= timeToOpen)
            {
                if (PortalOpen)
                {
                    PortalMat.SetFloat("_intensity", Mathf.Lerp(-2.5f, 3f, timeSinceOpening / timeToOpen));
                }
                else
                {
                    PortalMat.SetFloat("_intensity", Mathf.Lerp(3f, -2.5f, timeSinceOpening / timeToOpen));
                }
            }
            else
            {
                if (PortalOpen)
                {
                    PortalMat.SetFloat("_intensity", 3f);
                }
                else
                {
                    PortalMat.SetFloat("_intensity", -2.5f);
                }
                isOpenning = false;
                timeSinceOpening = 0f;
            }
        }
    }
}
