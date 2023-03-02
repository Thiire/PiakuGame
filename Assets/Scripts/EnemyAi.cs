using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.VFX;

[RequireComponent(typeof(NavMeshAgent), typeof(Collider), typeof(LineRenderer))]
public class EnemyAi : MonoBehaviour
{
    [Header("General")]
    private NavMeshAgent agent;
    public Transform player;
    public LayerMask whatIsGround, whatIsPlayer;

    [Range(10f, 200f)] public float health = 100f;
    [Range(10f, 90f)] public float coneOfVision = 50f;
    [Range(1f, 4f)] public float rotationSpeed = 2.5f;

    [Header("AggroModification")]
    public Texture initialEmissive;
    public Texture chaseEmissive;
    public Texture aggroEmissive;
    public MeshRenderer objectFrontRenderer;

    [Header("ZoneOfAction")]
    public Collider pattrolZone;
    public Collider attackZone;
    [Range(1f, 50f)] public float pattrolRangeMin = 5f;
    [Range(1f, 25f)] public float pattrolRangeMax = 5f;
    [Range(0f, 5f)] public float pattrolStopTime = 0.5f;
    [Range(1f, 5f)] public float pattrolRotateTime = 2f;
    [Range(0f, 5f)] public float pattrolGoTime = 0.5f;
    private float timeSincePattrol = 0f;
    private bool pattrolStart = false;

    [Header("Patrolling")]
    private bool walkPointSet;
    private Vector3 lookPoint;
    private Quaternion initialRotation;
    private Quaternion toRotate;
    public Vector3 walkPoint { get; private set; }

    [Header("Attacking")]
    [Range(0.5f, 2f)] public float timeBetweenAttacks;
    [Range(0.3f, 1f)] public float timeToAttack;
    [Range(0.1f, 5f)] public float attackVelocity;
    [Range(20f, 100f)] public float attackDamage;
    [Range(2f, 5f)] public float timeToDespawn;
    private float elapsedTimeToAttack = 0f;
    private bool alreadyAttacked;
    public GameObject projectile;

    [Header("States")]
    [Range(1f, 75f)] public float sightRange = 50f;
    [Range(1f, 25f)] public float attackRange = 5f;
    public bool isAttacking { get; private set; } = false;
    public bool isDead { get; private set; } = false;
    public float ennemyOffset = 2.5f;
    public bool playerInSightRange { get; private set; } = false;
    public bool playerInAttackRange { get; private set; } = false;
    public bool playerInAttackZone { get; private set; } = false;

    private Vector3 playerOldPosition;
    private Vector3 playerVelocity;
    private Animator childAnimator;
    private Collider objectCollider;
    private Material objectFrontMaterial;

    private LineRenderer lineRd;
    private Material lineMat;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        lineRd = GetComponent<LineRenderer>();
        lineMat = lineRd.material;
        childAnimator = GetComponentInChildren<Animator>();
        objectCollider = GetComponent<Collider>();
        objectFrontMaterial = objectFrontRenderer.material;

        playerOldPosition = player.position;
        lineRd.positionCount = 2;
    }

    private void Update()
    {
        childAnimator.SetFloat("Speed", agent.velocity.magnitude);
        if (!isAttacking)
        {
            playerInSightRange = IsTransformInCone(sightRange + attackRange);
            playerInAttackRange = IsTransformInCone(attackRange);
        }
        else
        {
            playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
            playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);
        }
        playerInAttackZone = attackZone.bounds.Contains(player.position);
        elapsedTimeToAttack -= (alreadyAttacked) ? Time.deltaTime * 2 : Time.deltaTime;
        elapsedTimeToAttack = (elapsedTimeToAttack <= 0) ? 0 : elapsedTimeToAttack;

        if (playerInSightRange && !playerInAttackRange && playerInAttackZone && !isDead) ChasePlayer();
        if (playerInAttackRange && playerInSightRange && playerInAttackZone && !isDead) AttackPlayer();
    }

    private void LateUpdate()
    {
        elapsedTimeToAttack = (elapsedTimeToAttack > timeToAttack) ? timeToAttack : (elapsedTimeToAttack < 0) ? 0 : elapsedTimeToAttack;
        lineMat.SetFloat("_opacity", Mathf.Lerp(0, 1, elapsedTimeToAttack / timeToAttack));
        lineRd.SetPosition(0, transform.position);
        lineRd.SetPosition(1, transform.position + new Vector3(transform.forward.x, transform.forward.y - 0.1f, transform.forward.z) * (Vector3.Magnitude(transform.position - player.transform.position)));
    }

    private void FixedUpdate()
    {
        if (!playerInSightRange && !playerInAttackRange && (!isAttacking || !playerInAttackZone) && !isDead) Patroling();
    }

    bool IsTransformInCone(float Range)
    {
        Vector3 directionTowardPlayer = player.position - transform.position;
        if (directionTowardPlayer.magnitude > Range)
            return false;
        float angleFromConeCenter = Vector3.Angle(directionTowardPlayer, transform.forward);
        return angleFromConeCenter <= coneOfVision;
    }

    private void Patroling()
    {
        if (isAttacking)
            player.SendMessageUpwards("setFight", false);

        objectFrontMaterial.SetTexture("_EmissiveColorMap", initialEmissive);
        isAttacking = false;
        if (!walkPointSet) SearchWalkPoint();

        if (walkPointSet)
        {
            timeSincePattrol += Time.deltaTime;
            if (timeSincePattrol >= pattrolStopTime + pattrolGoTime + pattrolRotateTime)
            {
                if (!agent.SetDestination(walkPoint))
                {
                    walkPointSet = false;
                    return;
                }
                pattrolStart = true;
            }
            else if (timeSincePattrol >= pattrolStopTime)
            {
                transform.rotation = Quaternion.Lerp(initialRotation, toRotate, (timeSincePattrol - pattrolStopTime) / pattrolRotateTime);
            }
            if (pattrolStart)
            {
                Vector3 distanceToWalkPoint = transform.position - walkPoint;
                //Walkpoint reached
                if (distanceToWalkPoint.magnitude - (ennemyOffset) < 1f)
                    walkPointSet = false;
            }
        }

    }
    private void SearchWalkPoint()
    {
        //Calculate random point in range
        RaycastHit tmpHit;
        Vector3 tmpPosition;
        timeSincePattrol = 0f;

        for (int tries = 0; !walkPointSet && tries <= 1000; tries++)
        {
            float randomZ = Random.Range(pattrolZone.bounds.min.z, pattrolZone.bounds.max.z);
            float randomX = Random.Range(pattrolZone.bounds.min.x, pattrolZone.bounds.max.x);

            tmpPosition = new Vector3(randomX, pattrolZone.bounds.max.y, randomZ);
            Vector3 distanceToWalkPoint = transform.position - tmpPosition;
            tries++;
            if (distanceToWalkPoint.magnitude < pattrolRangeMin || distanceToWalkPoint.magnitude > pattrolRangeMin + pattrolRangeMax)
                continue;

            if (Physics.Raycast(tmpPosition, -transform.up, out tmpHit, 3f, whatIsGround) && tmpHit.transform.tag == "Ground")
            {
                walkPointSet = true;
                walkPoint = tmpHit.point;
                lookPoint = new Vector3(tmpHit.point.x, transform.position.y, tmpHit.point.z);
                initialRotation = transform.rotation;
                toRotate = Quaternion.LookRotation(lookPoint - transform.position);
            }
        }
    }

    private void ChasePlayer()
    {
        player.SendMessageUpwards("setFight", true);

        walkPointSet = false;
        isAttacking = true;
        agent.SetDestination(new Vector3(player.position.x, transform.position.y, player.position.z));
        objectFrontMaterial.SetTexture("_EmissiveColorMap", chaseEmissive);
    }

    private void AttackPlayer()
    {
        player.SendMessageUpwards("setFight", true);

        walkPointSet = false;
        isAttacking = true;
        //Make sure enemy doesn't move
        agent.SetDestination(transform.position);

        Vector3 lookPos = player.position - transform.position;
        lookPos.y = 0;
        if (Quaternion.Angle(transform.rotation, Quaternion.LookRotation(lookPos)) > 7.5)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(lookPos), 2.75f * Time.deltaTime);
        }
        else
        {
            if (!alreadyAttacked)
            {
                if (elapsedTimeToAttack == 0)
                {
                    SoundManager.SharedInstance.PlaySound("Laser");
                }
                elapsedTimeToAttack += Time.deltaTime * 2;
            }
            transform.LookAt(player.position);

            playerVelocity = playerOldPosition - player.position;
            playerOldPosition = player.position;
            if (elapsedTimeToAttack >= timeToAttack && !alreadyAttacked)
            {
                alreadyAttacked = true;
                GameObject bullet = PoolManager.SharedInstance.GetPooledEnnemyBullet();
                if (bullet != null)
                {
                    bullet.GetComponent<EnnemyBullet>().startSpawn(transform.position, transform.forward + playerVelocity, attackVelocity, timeToDespawn, attackDamage);
                }
                Invoke(nameof(ResetAttack), timeToAttack / 2 + timeBetweenAttacks);
            }
        }

        objectFrontMaterial.SetTexture("_EmissiveColorMap", aggroEmissive);
    }
    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    public void TakeDamage(float damage)
    {
        health -= (!isAttacking) ? damage * 2 : damage;
        if (health <= 0)
        {
            SoundManager.SharedInstance.PlaySound("Explosion");
            player.SendMessageUpwards("setFight", false);
            childAnimator.SetTrigger("Die");
            isDead = true;
            objectCollider.enabled = false;
            Invoke(nameof(DestroyEnemy), 0.75f);
        }
        else
        {
            childAnimator.SetTrigger("GetAttacked");
        }
        if (!isAttacking)
        {
            GameObject critical = PoolManager.SharedInstance.GetPooledCriticalVFX();
            if (critical != null)
            {
                VisualEffect vfxTmp = critical.GetComponent<VisualEffect>();
                critical.transform.position = transform.position;
                critical.SetActive(true);
                vfxTmp.Play();
            }
        }
        else
        {
            GameObject normal = PoolManager.SharedInstance.GetPooledNormalVFX();
            if (normal != null)
            {
                VisualEffect vfxTmp = normal.GetComponent<VisualEffect>();
                normal.transform.position = transform.position;
                normal.SetActive(true);
                vfxTmp.Play();
            }
        }

        GameObject text = PoolManager.SharedInstance.GetPooledFloatingText();
        if (text != null)
        {
            text.transform.LookAt(new Vector3(player.position.x, text.transform.position.y, player.position.z));
            text.GetComponent<FloatingDamage>().StartDamage((!isAttacking) ? 1 : 0, (!isAttacking) ? (int)damage * 2 : (int)damage, transform.position);
        }
        isAttacking = true;
    }
    private void DestroyEnemy()
    {
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        if (walkPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, walkPoint);
        }
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, pattrolRangeMin);
        Gizmos.DrawWireSphere(transform.position, pattrolRangeMax + pattrolRangeMin);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + (Quaternion.Euler(coneOfVision, 0, 0) * transform.forward) * (sightRange + attackRange));
        Gizmos.DrawLine(transform.position, transform.position + (Quaternion.Euler(coneOfVision, coneOfVision, 0) * transform.forward) * (sightRange + attackRange));
        Gizmos.DrawLine(transform.position, transform.position + (Quaternion.Euler(-coneOfVision, 0, 0) * transform.forward) * (sightRange + attackRange));
        Gizmos.DrawLine(transform.position, transform.position + (Quaternion.Euler(0, coneOfVision, 0) * transform.forward) * (sightRange + attackRange));
        Gizmos.DrawLine(transform.position, transform.position + (Quaternion.Euler(0, -coneOfVision, 0) * transform.forward) * (sightRange + attackRange));
        Gizmos.DrawLine(transform.position, transform.position + (Quaternion.Euler(-coneOfVision, coneOfVision, 0) * transform.forward) * (sightRange + attackRange));
        Gizmos.DrawLine(transform.position, transform.position + (Quaternion.Euler(coneOfVision, -coneOfVision, 0) * transform.forward) * (sightRange + attackRange));
        Gizmos.DrawLine(transform.position, transform.position + (Quaternion.Euler(-coneOfVision, -coneOfVision, 0) * transform.forward) * (sightRange + attackRange));
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + (Quaternion.Euler(coneOfVision, 0, 0) * transform.forward) * attackRange);
        Gizmos.DrawLine(transform.position, transform.position + (Quaternion.Euler(-coneOfVision, 0, 0) * transform.forward) * attackRange);
        Gizmos.DrawLine(transform.position, transform.position + (Quaternion.Euler(0, coneOfVision, 0) * transform.forward) * attackRange);
        Gizmos.DrawLine(transform.position, transform.position + (Quaternion.Euler(0, -coneOfVision, 0) * transform.forward) * attackRange);
        Gizmos.DrawLine(transform.position, transform.position + (Quaternion.Euler(coneOfVision, coneOfVision, 0) * transform.forward) * attackRange);
        Gizmos.DrawLine(transform.position, transform.position + (Quaternion.Euler(-coneOfVision, coneOfVision, 0) * transform.forward) * attackRange);
        Gizmos.DrawLine(transform.position, transform.position + (Quaternion.Euler(coneOfVision, -coneOfVision, 0) * transform.forward) * attackRange);
        Gizmos.DrawLine(transform.position, transform.position + (Quaternion.Euler(-coneOfVision, -coneOfVision, 0) * transform.forward) * attackRange);
    }
}
