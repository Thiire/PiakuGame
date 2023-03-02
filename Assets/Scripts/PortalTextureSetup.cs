using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalTextureSetup : MonoBehaviour
{
    [Header("Cameras")]
    public Camera redCamera;
    public Camera blueCamera;
    [Space]
    [Header("Material")]
    public Material PortalMaterial;

    void Start()
    {
        if (redCamera.targetTexture != null)
        {
            redCamera.targetTexture.Release();
        }
        if (blueCamera.targetTexture != null)
        {
            blueCamera.targetTexture.Release();
        }

        redCamera.targetTexture = new RenderTexture(Screen.width, Screen.height, 24);
        blueCamera.targetTexture = new RenderTexture(Screen.width, Screen.height, 24);
        PortalMaterial.SetTexture("_mainBlueTex", redCamera.targetTexture);
        PortalMaterial.SetTexture("_mainRedTex", blueCamera.targetTexture);
    }

}
