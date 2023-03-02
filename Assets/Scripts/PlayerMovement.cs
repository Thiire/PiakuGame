using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController _playerController;

    [Header("General")]
    public Camera characterCamera;
    public float speed = 12f;
    public float gravity = -9.81f;
    [Tooltip("Empty reference under character to detect ground")]
    public Transform groundCheck;
    [Tooltip("Reference to groundLayer on actor to step on")]
    public LayerMask groundMask;
    [Tooltip("Reference to MouseView script in character Camera")]
    [Range(0.2f, 0.6f)] public float groundDistance = 0.4f;

    [Header("Jump and air control")]
    [Range(1f, 8f)] public float jumpHeight = 3f;
    [Range(0f, 1f)] public float forwardAirControl = 0.5f;
    [Range(0f, 1f)] public float sideAirControl = 0.5f;

    [Header("Second jump and delay")]
    [Range(1f, 8f)] public float airJumpHeight = 3f;
    [Range(0.5f, 2f)] public float airJumpMaxDelay = 1f;
    [Range(0.1f, 0.5f)] public float airJumpMinDelay = 0.25f;

    [Header("WallRide Jump")]
    [Range(3f, 7f)] public float wallRideJumpHeight = 5f;
    [Tooltip("must be inferior of airJumpMinDelay, if both have the same value no delay to double jump")]
    [Range(0.05f, 0.5f)] public float wallRideSecondJumpDelay = 0.25f;

    public Vector3 _velocity { get; set; }
    public bool _isGrounded { get; private set; }
    public bool isFighting { get; private set; } = false;
    private float _forwardCurrentSpeed;
    private float _sideCurrentSpeed;
    private float _jumpElapsedTime = 0f;
    private bool _isSecondJumpDone = false;

    private WallRide wallRideComponent;
    private PlayerHealth healthPlayer;

    void Start()
    {
        _playerController = GetComponent<CharacterController>();
        healthPlayer = GetComponent<PlayerHealth>();
        wallRideComponent = GetComponent<WallRide>();
        _forwardCurrentSpeed = speed;
        _sideCurrentSpeed = speed;
        healthPlayer.initialSpawnPoint = transform.position;
    }

    void MovePlayer()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 movementForward = transform.forward * moveZ;
        Vector3 movementSide = transform.right * moveX;
        _playerController.Move(movementForward * _forwardCurrentSpeed * Time.deltaTime);
        _playerController.Move(movementSide * _sideCurrentSpeed * Time.deltaTime);
    }

    void GravityAndJump()
    {
        Collider[] allCollider = Physics.OverlapSphere(groundCheck.position, groundDistance, groundMask);
        MovingPlatform movingScript = null;

        if (allCollider.Length != 0)
        {
            foreach (Collider contact in allCollider)
            {
                if (contact.tag == "MovingPlatform")
                {
                    _isGrounded = true;
                    movingScript = contact.GetComponent<MovingPlatform>();
                    if (movingScript != null)
                    {
                        _playerController.Move(movingScript.velocity);
                        break;
                    }
                }
            }
        }
        _isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (Input.GetButtonDown("Jump") && _isGrounded && SkillsManager.SharedInstance.Jump)
        {
            if (((int)Random.Range(0, 2)) == 0)
                SoundManager.SharedInstance.PlaySound("Jump1");
            else
                SoundManager.SharedInstance.PlaySound("Jump3");
            _velocity = new Vector3(0, Mathf.Sqrt(jumpHeight * -2f * gravity), 0); //Jump
            if (movingScript != null)
            {
                _velocity += movingScript.velocity * 70;
            }
            _playerController.stepOffset = 0;
            _forwardCurrentSpeed = speed * forwardAirControl;
            _sideCurrentSpeed = speed * sideAirControl;
            _jumpElapsedTime = 0f;
            _isSecondJumpDone = false;
        }
        else if (_isGrounded && _velocity.y <= 0)
        {
            _velocity = new Vector3(0, -2f, 0); //On ground
            _playerController.stepOffset = 0.35f;
            _forwardCurrentSpeed = speed;
            _sideCurrentSpeed = speed;
            _isSecondJumpDone = false;
        }
        else
        {
            _velocity += Vector3.up * (gravity * Time.deltaTime); //Falling
            _forwardCurrentSpeed = speed * forwardAirControl;
            _sideCurrentSpeed = speed * sideAirControl;
        }
        _playerController.Move(_velocity * Time.deltaTime);
    }

    void JumpInAir()
    {
        if (wallRideComponent.IsWallRunning() && SkillsManager.SharedInstance.WallRide)
        {
            _jumpElapsedTime = (wallRideSecondJumpDelay <= airJumpMinDelay) ? wallRideSecondJumpDelay : airJumpMinDelay;
            _isSecondJumpDone = false;
            if (Input.GetButtonDown("Jump") && SkillsManager.SharedInstance.Jump)
            {
                if (((int)Random.Range(0, 2)) == 0)
                    SoundManager.SharedInstance.PlaySound("Jump1");
                else
                    SoundManager.SharedInstance.PlaySound("Jump3");
                _velocity += wallRideComponent.GetWallJumpDirection() * wallRideJumpHeight;
            }
        }
        else if (!_isGrounded && !_isSecondJumpDone)
        {
            _jumpElapsedTime += Time.deltaTime;
            if ((_jumpElapsedTime <= airJumpMaxDelay && _jumpElapsedTime >= airJumpMinDelay) && Input.GetButtonDown("Jump") && SkillsManager.SharedInstance.DoubleJump)
            {
                if (((int)Random.Range(0, 2)) == 0)
                    SoundManager.SharedInstance.PlaySound("Jump2");
                else
                    SoundManager.SharedInstance.PlaySound("Jump4");
                _velocity = new Vector3(0, Mathf.Sqrt(airJumpHeight * -2f * gravity), 0); //Jump
                _forwardCurrentSpeed = speed * forwardAirControl;
                _sideCurrentSpeed = speed * sideAirControl;
                _playerController.Move(_velocity * Time.deltaTime);
                _jumpElapsedTime = 0f;
                _isSecondJumpDone = true;
            }
            else if (_jumpElapsedTime > airJumpMaxDelay)
            {
                _jumpElapsedTime = 0f;
                _isSecondJumpDone = true;
            }
        }
    }

    public void setFight(bool fight)
    {
        isFighting = fight;
    }

    void Update()
    {
        if (isFighting)
        {
            SoundManager.SharedInstance.PlaySound("Chase");
            SoundManager.SharedInstance.StopSound("Ambiance");
        }
        else
        {
            SoundManager.SharedInstance.PlaySound("Ambiance");
            SoundManager.SharedInstance.StopSound("Chase");
        }

        if (transform.position.y < -50 && !healthPlayer.isDead)
        {
            healthPlayer.killPlayer();
        }
        if (DialogueManager.SharedInstance.isDialogue || healthPlayer.isDead)
            return;
        MovePlayer();
        GravityAndJump();
        JumpInAir();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(groundCheck.transform.position, groundDistance);
    }
}
