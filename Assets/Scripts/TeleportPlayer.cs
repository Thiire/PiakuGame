using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class TeleportPlayer : MonoBehaviour
{
    [Header("General")]
    [Tooltip("Camera of the player")]
    public Camera characterCamera;
    public Camera weaponCamera;
    public Material PortalMat;
    public GameObject throwableSword;
    public Light DirectionalLight;
    [Header("FlagsCamera")]
    [Tooltip("Camera of the blueSide")]
    public Camera blueFlagCamera;
    [Tooltip("Camera of the redSide")]
    public Camera redFlagCamera;
    [Header("Sky and PostProcessing Volumes")]
    public Volume skyVolume;
    public Volume postProcessingVolume;
    public CustomPassVolume OutlineVolume;
    [Header("Teleport Settings")]
    [Range(0f, 2f)] public float teleportDelay = 1f;
    [Range(0.2f, 1f)] public float timeSlowed = 0.5f;
    [Range(10, 170)] public float teleportFOV = 50;
    [Space]
    [Range(2f, 20f)] public float teleportFogAttenuation = 2f;
    [Range(0f, 1f)] public float teleportVignetteIntensity = 0.5f;
    [Range(0f, 1f)] public float teleportBloomIntensity = 0.6f;
    [Range(0f, 1f)] public float teleportChromaticAberationIntensity = 0.6f;
    [Range(2f, 50f)] public float teleportDepthIntensity = 3f;
    [Space]
    [Range(4000f, 7000f)] public float teleportLightTemperature = 4000f;
    public Color teleportFogColor;
    public Color teleportHorizonColor;
    [Space]
    [Range(0.1f, 1f)] public float teleportTimeFadeIn = 0.2f;
    [Range(0.1f, 1f)] public float teleportTimeFadeOut = 0.2f;

    private CharacterController playerController;
    public bool blueSide { get; private set; } = true;
    private bool isTeleporting = false;
    private bool hasBeenTeleported = false;
    private bool resetDefault = false;
    private float timeSinceTeleport = 0f;
    private float initalFOV;
    private float initialFogAttenuation;
    private float initialVignetteIntensity;
    private float initialBloomIntensity;
    private float initialChromaticAberationIntensity;
    private float initialDepthIntensity;
    private float initialLightTemperature;
    private Color initialFogColor;
    private Color initialHorizonColor;

    private ThrowSword throwableScript;

    private Fog skyFogAttribute;
    private PhysicallyBasedSky skyPhysicAttribute;
    private ChromaticAberration skyChromaticAberationAttribute;
    private Vignette postProcessingVignetteAttribute;
    private Bloom postProcessingBloomAttribute;
    private DepthOfField postProcessingDepthAttribute;
    private DrawRenderersCustomPass blueOutlineCustomPass;
    private DrawRenderersCustomPass redOutlineCustomPass;
    void Start()
    {
        playerController = GetComponent<CharacterController>();
        if (skyVolume.profile.TryGet<Fog>(out skyFogAttribute))
        {
            initialFogAttenuation = skyFogAttribute.meanFreePath.value;
            initialFogColor = skyFogAttribute.albedo.value;
        }
        if (skyVolume.profile.TryGet<PhysicallyBasedSky>(out skyPhysicAttribute))
        {
            initialHorizonColor = skyPhysicAttribute.horizonTint.value;
        }
        if (skyVolume.profile.TryGet<ChromaticAberration>(out skyChromaticAberationAttribute))
        {
            initialChromaticAberationIntensity = skyChromaticAberationAttribute.intensity.value;
        }
        if (postProcessingVolume.profile.TryGet<Vignette>(out postProcessingVignetteAttribute))
        {
            initialVignetteIntensity = postProcessingVignetteAttribute.intensity.value;
        }
        if (postProcessingVolume.profile.TryGet<Bloom>(out postProcessingBloomAttribute))
        {
            initialBloomIntensity = postProcessingBloomAttribute.intensity.value;
        }
        if (postProcessingVolume.profile.TryGet<DepthOfField>(out postProcessingDepthAttribute))
        {
            initialDepthIntensity = postProcessingDepthAttribute.farFocusStart.value;
        }
        foreach (var pass in OutlineVolume.customPasses)
        {
            if (pass is DrawRenderersCustomPass tmp)
            {
                if (tmp.name == "BlueOutline")
                {
                    blueOutlineCustomPass = tmp;
                }
                else if (tmp.name == "RedOutline")
                {
                    redOutlineCustomPass = tmp;
                }
            }
        }
        initalFOV = characterCamera.fieldOfView;
        initialLightTemperature = DirectionalLight.colorTemperature;

        throwableScript = throwableSword.GetComponent<ThrowSword>();
    }

    public void resetPlayer(Vector3 teleportPoint)
    {
        blueSide = true;
        isTeleporting = false;
        resetDefault = false;
        timeSinceTeleport = -2f;
        StartCoroutine(ResetPosition(teleportPoint, 0.5f));
    }

    IEnumerator ResetPosition(Vector3 teleportPoint, float time)
    {
        float elapsedTime = 0f;
        float tmpVignetteValue = postProcessingVignetteAttribute.intensity.value;
        float tmpSkyValue = skyChromaticAberationAttribute.intensity.value;
        Color tmpFogValue = skyFogAttribute.albedo.value;
        Color tmpHorizonValue = skyPhysicAttribute.horizonTint.value;
        float tmpLightValue = DirectionalLight.colorTemperature;

        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;
            postProcessingVignetteAttribute.intensity.value = Mathf.Lerp(tmpVignetteValue, 0.5f, elapsedTime / time);
            skyChromaticAberationAttribute.intensity.value = Mathf.Lerp(tmpSkyValue, 1, elapsedTime / time);

            skyFogAttribute.albedo.value = Color.Lerp(tmpFogValue, initialFogColor, elapsedTime / time);
            skyPhysicAttribute.horizonTint.value = Color.Lerp(tmpHorizonValue, initialHorizonColor, elapsedTime / time);
            DirectionalLight.colorTemperature = Mathf.Lerp(tmpLightValue, initialLightTemperature, elapsedTime / time);

            Time.timeScale = Mathf.Lerp(1f, timeSlowed, elapsedTime / time);
            yield return null;
        }
        if (characterCamera.cullingMask == (characterCamera.cullingMask | (1 << LayerMask.NameToLayer("RedLayer"))))
        {
            characterCamera.cullingMask ^= 1 << LayerMask.NameToLayer("BlueLayer");
            characterCamera.cullingMask ^= 1 << LayerMask.NameToLayer("RedLayer");
            characterCamera.cullingMask ^= 1 << LayerMask.NameToLayer("BlueEnnemy");
            characterCamera.cullingMask ^= 1 << LayerMask.NameToLayer("RedEnnemy");
            redOutlineCustomPass.layerMask ^= 1 << LayerMask.NameToLayer("RedOutline");
            blueOutlineCustomPass.layerMask ^= 1 << LayerMask.NameToLayer("BlueOutline");
        }

        playerController.enabled = false;
        throwableScript.CancelThrow();
        transform.position = teleportPoint;
        playerController.enabled = true;

        characterCamera.fieldOfView = initalFOV;
        redFlagCamera.fieldOfView = initalFOV;
        blueFlagCamera.fieldOfView = initalFOV;
        weaponCamera.fieldOfView = initalFOV;
        Time.timeScale = 1f;

        skyFogAttribute.meanFreePath.value = initialFogAttenuation;
        skyChromaticAberationAttribute.intensity.value = initialChromaticAberationIntensity;
        postProcessingVignetteAttribute.intensity.value = initialVignetteIntensity;
        postProcessingBloomAttribute.intensity.value = initialBloomIntensity;
        postProcessingDepthAttribute.farFocusStart.value = initialDepthIntensity;

        skyFogAttribute.albedo.value = (blueSide) ? initialFogColor : teleportFogColor;
        skyPhysicAttribute.horizonTint.value = (blueSide) ? initialHorizonColor : teleportHorizonColor;
        DirectionalLight.colorTemperature = (blueSide) ? initialLightTemperature : teleportLightTemperature;

        PortalMat.SetFloat("_lerpValue", (blueSide) ? 0 : 1);
    }

    void Update()
    {
        if (!SkillsManager.SharedInstance.Teleport)
            return;

        timeSinceTeleport += Time.deltaTime;
        if (Input.GetButtonDown("Teleport") && !isTeleporting && timeSinceTeleport >= 0)
        {
            SoundManager.SharedInstance.PlaySound("Portal");
            timeSinceTeleport = 0f;
            isTeleporting = true;
        }
        if (timeSinceTeleport >= teleportDelay + teleportTimeFadeIn + teleportTimeFadeOut)
        {
            timeSinceTeleport = 0f;
            isTeleporting = false;
            hasBeenTeleported = false;
            resetDefault = false;
        }
        if (isTeleporting && !resetDefault)
        {
            if (timeSinceTeleport <= teleportTimeFadeIn)
            {
                characterCamera.fieldOfView = Mathf.Lerp(initalFOV, teleportFOV, timeSinceTeleport / teleportTimeFadeIn);
                redFlagCamera.fieldOfView = Mathf.Lerp(initalFOV, teleportFOV, timeSinceTeleport / teleportTimeFadeIn);
                blueFlagCamera.fieldOfView = Mathf.Lerp(initalFOV, teleportFOV, timeSinceTeleport / teleportTimeFadeIn);
                weaponCamera.fieldOfView = Mathf.Lerp(initalFOV, teleportFOV, timeSinceTeleport / teleportTimeFadeIn);
                Time.timeScale = Mathf.Lerp(1f, timeSlowed, timeSinceTeleport / teleportTimeFadeIn);

                skyFogAttribute.meanFreePath.value = Mathf.Lerp(initialFogAttenuation, teleportFogAttenuation, timeSinceTeleport / teleportTimeFadeIn);
                skyChromaticAberationAttribute.intensity.value = Mathf.Lerp(initialChromaticAberationIntensity, teleportChromaticAberationIntensity, timeSinceTeleport / teleportTimeFadeIn);
                postProcessingVignetteAttribute.intensity.value = Mathf.Lerp(initialVignetteIntensity, teleportVignetteIntensity, timeSinceTeleport / teleportTimeFadeIn);
                postProcessingBloomAttribute.intensity.value = Mathf.Lerp(initialBloomIntensity, teleportBloomIntensity, timeSinceTeleport / teleportTimeFadeIn);
                postProcessingDepthAttribute.farFocusStart.value = Mathf.Lerp(initialDepthIntensity, teleportDepthIntensity, timeSinceTeleport / teleportTimeFadeIn);

                if (blueSide)
                {
                    skyFogAttribute.albedo.value = Color.Lerp(initialFogColor, teleportFogColor, timeSinceTeleport / (teleportTimeFadeOut + teleportTimeFadeIn));
                    skyPhysicAttribute.horizonTint.value = Color.Lerp(initialHorizonColor, teleportHorizonColor, timeSinceTeleport / (teleportTimeFadeOut + teleportTimeFadeIn));
                    DirectionalLight.colorTemperature = Mathf.Lerp(initialLightTemperature, teleportLightTemperature, timeSinceTeleport / (teleportTimeFadeOut + teleportTimeFadeIn));
                    PortalMat.SetFloat("_lerpValue", Mathf.Lerp(0, 1, timeSinceTeleport / (teleportTimeFadeOut + teleportTimeFadeIn)));
                }
                else
                {
                    skyFogAttribute.albedo.value = Color.Lerp(teleportFogColor, initialFogColor, timeSinceTeleport / (teleportTimeFadeOut + teleportTimeFadeIn));
                    skyPhysicAttribute.horizonTint.value = Color.Lerp(teleportHorizonColor, initialHorizonColor, timeSinceTeleport / (teleportTimeFadeOut + teleportTimeFadeIn));
                    DirectionalLight.colorTemperature = Mathf.Lerp(teleportLightTemperature, initialLightTemperature, timeSinceTeleport / (teleportTimeFadeOut + teleportTimeFadeIn));
                    PortalMat.SetFloat("_lerpValue", Mathf.Lerp(1, 0, timeSinceTeleport / (teleportTimeFadeOut + teleportTimeFadeIn)));
                }
            }
            else if (timeSinceTeleport >= teleportTimeFadeIn && timeSinceTeleport <= teleportTimeFadeIn + teleportTimeFadeOut)
            {
                if (!hasBeenTeleported)
                {
                    hasBeenTeleported = true;
                    characterCamera.cullingMask ^= 1 << LayerMask.NameToLayer("BlueLayer");
                    characterCamera.cullingMask ^= 1 << LayerMask.NameToLayer("RedLayer");
                    characterCamera.cullingMask ^= 1 << LayerMask.NameToLayer("BlueEnnemy");
                    characterCamera.cullingMask ^= 1 << LayerMask.NameToLayer("RedEnnemy");
                    redOutlineCustomPass.layerMask ^= 1 << LayerMask.NameToLayer("RedOutline");
                    blueOutlineCustomPass.layerMask ^= 1 << LayerMask.NameToLayer("BlueOutline");
                    playerController.enabled = false;

                    Vector3 newPosition = (blueSide) ? redFlagCamera.transform.position + (transform.position - characterCamera.transform.position) : blueFlagCamera.transform.position + (transform.position - characterCamera.transform.position);
                    throwableScript.setNewDestination(transform.position, newPosition);
                    transform.position = newPosition;
                    playerController.enabled = true;
                }
                characterCamera.fieldOfView = Mathf.Lerp(teleportFOV, initalFOV, (timeSinceTeleport - teleportTimeFadeIn) / teleportTimeFadeOut);
                redFlagCamera.fieldOfView = Mathf.Lerp(teleportFOV, initalFOV, (timeSinceTeleport - teleportTimeFadeIn) / teleportTimeFadeOut);
                blueFlagCamera.fieldOfView = Mathf.Lerp(teleportFOV, initalFOV, (timeSinceTeleport - teleportTimeFadeIn) / teleportTimeFadeOut);
                weaponCamera.fieldOfView = Mathf.Lerp(teleportFOV, initalFOV, (timeSinceTeleport - teleportTimeFadeIn) / teleportTimeFadeOut);
                Time.timeScale = Mathf.Lerp(timeSlowed, 1f, (timeSinceTeleport - teleportTimeFadeIn) / teleportTimeFadeOut);

                skyFogAttribute.meanFreePath.value = Mathf.Lerp(teleportFogAttenuation, initialFogAttenuation, (timeSinceTeleport - teleportTimeFadeIn) / teleportTimeFadeOut);
                skyChromaticAberationAttribute.intensity.value = Mathf.Lerp(teleportChromaticAberationIntensity, initialChromaticAberationIntensity, (timeSinceTeleport - teleportTimeFadeIn) / teleportTimeFadeOut);
                postProcessingVignetteAttribute.intensity.value = Mathf.Lerp(teleportVignetteIntensity, initialVignetteIntensity, (timeSinceTeleport - teleportTimeFadeIn) / teleportTimeFadeOut);
                postProcessingBloomAttribute.intensity.value = Mathf.Lerp(teleportBloomIntensity, initialBloomIntensity, (timeSinceTeleport - teleportTimeFadeIn) / teleportTimeFadeOut);
                postProcessingDepthAttribute.farFocusStart.value = Mathf.Lerp(teleportDepthIntensity, initialDepthIntensity, (timeSinceTeleport - teleportTimeFadeIn) / teleportTimeFadeOut);

                if (blueSide)
                {
                    skyFogAttribute.albedo.value = Color.Lerp(initialFogColor, teleportFogColor, timeSinceTeleport / (teleportTimeFadeOut + teleportTimeFadeIn));
                    skyPhysicAttribute.horizonTint.value = Color.Lerp(initialHorizonColor, teleportHorizonColor, timeSinceTeleport / (teleportTimeFadeOut + teleportTimeFadeIn));
                    DirectionalLight.colorTemperature = Mathf.Lerp(initialLightTemperature, teleportLightTemperature, timeSinceTeleport / (teleportTimeFadeOut + teleportTimeFadeIn));
                    PortalMat.SetFloat("_lerpValue", Mathf.Lerp(0, 1, timeSinceTeleport / (teleportTimeFadeOut + teleportTimeFadeIn)));
                }
                else
                {
                    skyFogAttribute.albedo.value = Color.Lerp(teleportFogColor, initialFogColor, timeSinceTeleport / (teleportTimeFadeOut + teleportTimeFadeIn));
                    skyPhysicAttribute.horizonTint.value = Color.Lerp(teleportHorizonColor, initialHorizonColor, timeSinceTeleport / (teleportTimeFadeOut + teleportTimeFadeIn));
                    DirectionalLight.colorTemperature = Mathf.Lerp(teleportLightTemperature, initialLightTemperature, timeSinceTeleport / (teleportTimeFadeOut + teleportTimeFadeIn));
                    PortalMat.SetFloat("_lerpValue", Mathf.Lerp(1, 0, timeSinceTeleport / (teleportTimeFadeOut + teleportTimeFadeIn)));
                }
            }
            else if (!resetDefault)
            {
                blueSide = !blueSide;
                characterCamera.fieldOfView = initalFOV;
                redFlagCamera.fieldOfView = initalFOV;
                blueFlagCamera.fieldOfView = initalFOV;
                weaponCamera.fieldOfView = initalFOV;
                Time.timeScale = 1f;

                skyFogAttribute.meanFreePath.value = initialFogAttenuation;
                skyChromaticAberationAttribute.intensity.value = initialChromaticAberationIntensity;
                postProcessingVignetteAttribute.intensity.value = initialVignetteIntensity;
                postProcessingBloomAttribute.intensity.value = initialBloomIntensity;
                postProcessingDepthAttribute.farFocusStart.value = initialDepthIntensity;

                skyFogAttribute.albedo.value = (blueSide) ? initialFogColor : teleportFogColor;
                skyPhysicAttribute.horizonTint.value = (blueSide) ? initialHorizonColor : teleportHorizonColor;
                DirectionalLight.colorTemperature = (blueSide) ? initialLightTemperature : teleportLightTemperature;

                PortalMat.SetFloat("_lerpValue", (blueSide) ? 0 : 1);
                resetDefault = true;
            }
        }
    }
}
