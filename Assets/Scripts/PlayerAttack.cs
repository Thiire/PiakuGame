using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAttack : MonoBehaviour
{
    [Header("General")]
    public TrailRenderer swordTrail;
    public LayerMask whatIsEnnemy;
    public GameObject playerReference;
    [Header("Attack")]
    [Range(20f, 50f)] public float simpleAttackDamage = 30f;
    [Range(20f, 50f)] public float mediumAttackDamage = 35f;
    [Range(20f, 50f)] public float heavyAttackDamage = 50f;
    [Range(0.1f, 0.5f)] public float longAttackHoldTimer = 0.25f;
    [Range(1f, 5f)] public float attackRange = 2.5f;
    [Range(0.05f, 1f)] public float attackDelay = 0.05f;

    [Space]
    [Header("ThrowAttack")]
    [Range(20f, 50f)] public float throwAttackDamage = 20f;
    [Range(3f, 20f)] public float throwDistance = 15f;
    [Range(2f, 5f)] public float throwTime = 2f;
    [Range(1f, 3f)] public float catchTime = 2f;

    [Header("ThrowableWeapon reference")]
    public ThrowSword swordThrowRef;
    public Transform cameraRef;
    private TeleportPlayer PlayerTeleportScript;
    private Animator swordAnimator;
    private Renderer swordRenderer;
    private bool isAttacking = false;
    private bool isHolding = false;
    private bool isThrown = false;
    private float delaySinceAttack = 0f;
    private float delaySinceClick = 0f;
    private int attackCount = 0;
    private int attackNumber = 0;
    private bool blueSide() => PlayerTeleportScript.blueSide;
    void Start()
    {
        swordAnimator = GetComponent<Animator>();
        swordRenderer = GetComponent<Renderer>();
        PlayerTeleportScript = playerReference.GetComponent<TeleportPlayer>();

        swordTrail.enabled = false;
        if (!SkillsManager.SharedInstance.Sword)
            gameObject.SetActive(false);
    }

    public void SwordAttack()
    {
        attackCount++;
        Collider[] hitColliders = Physics.OverlapSphere(GetComponent<Renderer>().bounds.center, attackRange, whatIsEnnemy);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.tag == "Ennemy")
            {
                SoundManager.SharedInstance.PlaySound("Sword");
                switch (attackCount)
                {
                    case 1:
                        hitCollider.SendMessage("TakeDamage", simpleAttackDamage);
                        break;
                    case 2:
                        hitCollider.SendMessage("TakeDamage", mediumAttackDamage);
                        break;
                    default:
                        hitCollider.SendMessage("TakeDamage", heavyAttackDamage);
                        break;
                }
            }
        }
    }

    public void CatchWeapon()
    {
        swordRenderer.enabled = true;
        isThrown = false;
        swordAnimator.SetTrigger("CatchThrow");
    }

    public void StartSwordTrail()
    {
        swordTrail.Clear();
        swordTrail.enabled = true;
    }

    public void ThrowWeapon()
    {
        swordRenderer.enabled = false;
        swordThrowRef.SetSwordPosition(cameraRef.forward);
    }

    public void StopSwordTrail()
    {
        swordTrail.Clear();
        swordTrail.enabled = false;
    }

    public void PlayAttackSound()
    {
        SoundManager.SharedInstance.PlaySound("SwordAir");
    }

    public void CancelAttackanimation()
    {
        swordAnimator.ResetTrigger("Slash");
        isAttacking = false;
        attackCount = 0;
        attackNumber = 0;
        delaySinceClick = 0f;
        delaySinceAttack = attackDelay;
    }

    void Update()
    {
        delaySinceAttack = (delaySinceAttack > 0) ? delaySinceAttack - Time.deltaTime : 0;
        if (DialogueManager.SharedInstance.isDialogue)
        {
            return;
        }
        if ((Input.GetButton("Fire1") || Input.GetButtonDown("Fire1")) && !isHolding && !isThrown && delaySinceAttack <= 0)
        {
            if (attackNumber == 1 && !SkillsManager.SharedInstance.SwordComboAttack)
            {
            }
            else
            {
                if (isAttacking == false || Input.GetButtonDown("Fire1"))
                {
                    attackNumber++;
                    delaySinceClick = 0f;
                    isAttacking = true;
                    swordAnimator.SetTrigger("Slash");
                }
                if (Input.GetButton("Fire1") && !Input.GetButtonDown("Fire1"))
                {
                    delaySinceClick += Time.deltaTime;
                    if (delaySinceClick >= longAttackHoldTimer)
                    {
                        attackNumber++;
                        delaySinceClick = 0f;
                        isAttacking = true;
                        swordAnimator.SetTrigger("Slash");
                    }
                }
            }
        }
        if (Input.GetButtonDown("Fire2") && !isAttacking && !isHolding && !isThrown && SkillsManager.SharedInstance.SwordThrow)
        {
            isHolding = true;
            swordAnimator.SetTrigger("HoldThrow");
        }
        if (Input.GetButtonUp("Fire2") && !isAttacking && isHolding && !isThrown && SkillsManager.SharedInstance.SwordThrow)
        {
            isHolding = false;
            isThrown = true;
            swordAnimator.SetTrigger("LetThrow");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(GetComponent<Renderer>().bounds.center, attackRange);
    }
}
