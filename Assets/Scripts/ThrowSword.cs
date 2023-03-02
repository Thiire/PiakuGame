using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class ThrowSword : MonoBehaviour
{
    [Header("PlayerWeaponRef")]
    public GameObject playerWeaponRef;
    [Header("LayerMask")]
    [Tooltip("Which Layer mask cancel the throw")]
    public LayerMask layerMask;

    public float throwDamage() => playerAttackRef.throwAttackDamage;
    private float throwDistance() => playerAttackRef.throwDistance;
    private float throwTime() => playerAttackRef.throwTime;
    private float catchTime() => playerAttackRef.catchTime;

    private PlayerAttack playerAttackRef;
    public bool isThrown { get; private set; } = false;
    public bool triggerCalled { get; private set; } = false;
    private Vector3 throwDirection;
    private Vector3 throwPoint;
    private Animator swordAnimator;
    private Renderer swordRenderer;
    private TrailRenderer swordTrail;
    private Light pointLight;
    private float elapsedTimeSinceThrow = 0f;
    private float elapsedTimeSinceCatch = 0f;
    void Start()
    {
        swordRenderer = GetComponentInChildren<Renderer>();
        swordAnimator = GetComponent<Animator>();
        swordTrail = GetComponentInChildren<TrailRenderer>();
        pointLight = GetComponentInChildren<Light>();

        playerAttackRef = playerWeaponRef.GetComponent<PlayerAttack>();
        swordRenderer.enabled = false;
        swordTrail.enabled = false;
        pointLight.enabled = false;

        if (!SkillsManager.SharedInstance.Sword)
            gameObject.SetActive(false);
    }

    public void SetSwordPosition(Vector3 directionThrow)
    {
        swordRenderer.enabled = true;
        swordTrail.enabled = true;
        pointLight.enabled = true;
        transform.rotation = playerWeaponRef.transform.rotation;
        transform.position = playerWeaponRef.transform.position;

        throwDirection = directionThrow;
        throwPoint = transform.position + (directionThrow * throwDistance());
        elapsedTimeSinceThrow = 0f;
        elapsedTimeSinceCatch = 0f;
        isThrown = true;
        triggerCalled = false;
        swordAnimator.SetTrigger("StartSpin");
        SoundManager.SharedInstance.PlaySound("Throw");
    }

    public void setNewDestination(Vector3 oldPosition, Vector3 newPosition)
    {
        swordTrail.Clear();
        transform.position = newPosition - (oldPosition - transform.position);
        throwPoint = newPosition - (oldPosition - throwPoint);
        swordTrail.Clear();
    }

    public void CancelThrow()
    {
        swordRenderer.enabled = false;
        pointLight.enabled = false;
        isThrown = false;
        playerAttackRef.CatchWeapon();
    }

    void OnTriggerEnter(Collider other)
    {
        if ((layerMask.value & (1 << other.transform.gameObject.layer)) > 0)
        {
            elapsedTimeSinceThrow = throwTime() + 1;
        }
    }

    void Update()
    {
        if (isThrown)
        {
            if (elapsedTimeSinceThrow >= throwTime())
            {
                if (!triggerCalled)
                {
                    triggerCalled = true;
                    swordTrail.enabled = false;
                    swordAnimator.SetTrigger("StopSpin");
                    SoundManager.SharedInstance.StopSound("Throw");
                }
                elapsedTimeSinceCatch += Time.deltaTime;
                transform.position = Vector3.Lerp(transform.position, playerWeaponRef.transform.position, elapsedTimeSinceCatch / catchTime());
                transform.rotation = Quaternion.Lerp(transform.rotation, playerWeaponRef.transform.rotation, elapsedTimeSinceCatch / catchTime());
                if (Vector3.SqrMagnitude(transform.position - playerWeaponRef.transform.position) < 0.3)
                {
                    CancelThrow();
                }
            }
            else
            {
                elapsedTimeSinceThrow += Time.deltaTime;
                transform.position = Vector3.Lerp(transform.position, throwPoint, elapsedTimeSinceThrow / throwTime());
                //transform.position += throwDirection * (throwTime() / elapsedTimeSinceThrow) * Time.deltaTime * throwDistance();
            }
        }
    }
}
