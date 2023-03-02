using UnityEngine;
using System.Linq;
using UnityEngine.Rendering;

[RequireComponent(typeof(PlayerMovement))]
public class WallRide : MonoBehaviour
{
    [Header("Parameter")]
    [Tooltip("Maximum distance to wallRide a wall")]
    [Range(0.5f, 2f)] public float wallMaxDistance = 1;
    [Tooltip("Speed multiplier to apply on player velocity when wallRiding")]
    [Range(0.75f, 2f)] public float wallSpeedMultiplier = 1.2f;
    [Tooltip("Minimum distance to ground to wallRide")]
    [Range(0.5f, 5f)] public float minimumHeight = 1.2f;
    [Tooltip("Maximum angle on which the camera will pivot")]
    [Range(5f, 30f)] public float maxAngleRoll = 20;
    [Tooltip("Min/Max angle for Wall")]
    [Range(0.0f, 1.0f)] public float normalizedAngleThreshold = 0.1f;

    [Header("Player Physics")]
    [Tooltip("Player time to get on max height from a jump (avoid wallRiding while having a positive upward velocity)")]
    [Range(0.25f, 1.5f)] public float playerJumpDuration = 0.5f;
    [Tooltip("Gravity applied to player when wallRiding")]
    [Range(50f, 400f)] public float wallGravityDownForce = 50f;
    [Space]
    [Tooltip("How much the player will be kicked of the wall after the jump")]
    [Range(1f, 3f)] public float wallBouncing = 2;
    [Tooltip("Define how much time needed to pivot the camera during wallRide")]
    [Range(2.5f, 10f)] public float cameraTransitionDuration = 7.5f;

    PlayerMovement m_PlayerMovement;
    Vector3[] directions;
    RaycastHit[] hits;

    bool isWallRunning = false;
    Vector3 lastWallPosition;
    Vector3 lastWallNormal;
    float elapsedTimeSinceJump = 0;
    float elapsedTimeSinceWallAttach = 0;
    float elapsedTimeSinceWallDetatch = 0;
    bool jumping;

    bool isPlayergrounded() => m_PlayerMovement._isGrounded;

    public bool IsWallRunning() => isWallRunning;

    bool CanWallRun()
    {
        float verticalAxis = Input.GetAxisRaw("Vertical");

        return !isPlayergrounded() && verticalAxis > 0 && VerticalCheck();
    }

    bool VerticalCheck()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minimumHeight);
    }


    void Start()
    {
        m_PlayerMovement = GetComponent<PlayerMovement>();

        directions = new Vector3[]{
            Vector3.right,
            Vector3.right + Vector3.forward,
            //Vector3.forward,
            Vector3.left + Vector3.forward,
            Vector3.left
        };
    }


    public void LateUpdate()
    {
        isWallRunning = false;

        if (!SkillsManager.SharedInstance.WallRide)
            return;

        if (Input.GetButtonDown("Jump") && SkillsManager.SharedInstance.Jump)
        {
            jumping = true;
        }

        if (CanAttach())
        {
            hits = new RaycastHit[directions.Length];

            for (int i = 0; i < directions.Length; i++)
            {
                Vector3 dir = transform.TransformDirection(directions[i]);
                Physics.Raycast(transform.position, dir, out hits[i], wallMaxDistance);
                if (hits[i].collider != null)
                {
                    Debug.DrawRay(transform.position, dir * hits[i].distance, Color.green);
                }
                else
                {
                    Debug.DrawRay(transform.position, dir * wallMaxDistance, Color.red);
                }
            }

            if (CanWallRun())
            {
                hits = hits.ToList().Where(h => h.collider != null).OrderBy(h => h.distance).ToArray();
                if (hits.Length > 0 && hits[0].transform.tag == "WallRideable")
                {
                    OnWall(hits[0]);
                    lastWallPosition = hits[0].point;
                    lastWallNormal = hits[0].normal;
                }
            }
        }

        if (isWallRunning)
        {
            elapsedTimeSinceWallDetatch = 0;
            elapsedTimeSinceWallAttach += Time.deltaTime;
            m_PlayerMovement._velocity += Vector3.down * wallGravityDownForce * Time.deltaTime;
        }
        else
        {
            elapsedTimeSinceWallAttach = 0;
            elapsedTimeSinceWallDetatch += Time.deltaTime;
        }
    }

    bool CanAttach()
    {
        if (jumping)
        {
            elapsedTimeSinceJump += Time.deltaTime;
            if (elapsedTimeSinceJump > playerJumpDuration)
            {
                elapsedTimeSinceJump = 0;
                jumping = false;
            }
            return false;
        }

        return true;
    }

    void OnWall(RaycastHit hit)
    {
        float d = Vector3.Dot(hit.normal, Vector3.up);
        if (d >= -normalizedAngleThreshold && d <= normalizedAngleThreshold)
        {
            float vertical = Input.GetAxisRaw("Vertical");
            Vector3 alongWall = transform.TransformDirection(Vector3.forward);

            Debug.DrawRay(transform.position, alongWall.normalized * 10, Color.green);
            Debug.DrawRay(transform.position, lastWallNormal * 10, Color.magenta);

            m_PlayerMovement._velocity = alongWall * vertical * wallSpeedMultiplier;
            isWallRunning = true;
        }
    }

    float CalculateSide()
    {
        if (isWallRunning)
        {
            Vector3 heading = lastWallPosition - transform.position;
            Vector3 perp = Vector3.Cross(transform.forward, heading);
            float dir = Vector3.Dot(perp, transform.up);
            return dir;
        }
        return 0;
    }

    public float GetCameraRoll()
    {
        float dir = CalculateSide();
        float cameraAngle = m_PlayerMovement.characterCamera.transform.eulerAngles.z;
        float targetAngle = 0;
        if (dir != 0)
        {
            targetAngle = Mathf.Sign(dir) * maxAngleRoll;
        }
        return Mathf.LerpAngle(cameraAngle, targetAngle, Mathf.Max(elapsedTimeSinceWallAttach, elapsedTimeSinceWallDetatch) / cameraTransitionDuration);
    }

    public Vector3 GetWallJumpDirection()
    {
        if (isWallRunning)
        {
            return lastWallNormal * wallBouncing + Vector3.up;
        }
        return Vector3.zero;
    }
}
