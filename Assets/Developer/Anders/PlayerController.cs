using System;
using System.Collections;
using EasingFunctions;
using UnityEngine;
using Avatar = Alteruna.Avatar;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = System.Numerics.Vector2;
using Vector3 = UnityEngine.Vector3;

/// <summary>
/// Handles the movement/physics, materials and important variables of the player
/// Might separate the movement part into a new script (movement component)
/// </summary>
public class PlayerController : MonoBehaviour
{
    public static Action<Avatar> OnPlayerJoined = delegate(Avatar user) { };
    public static Action<Avatar> OnPlayerLeft = delegate (Avatar user) { };

    [HideInInspector] public bool Activated;
    [HideInInspector] public float slowMultiplier = 1f;
    [SerializeField] private float jumpForce = 15;
    [SerializeField] private float gravityForce = 45;
    [SerializeField] private float dashDuration = 0.36f;
    [SerializeField] private float jumpDashDuration = 1.5f;
    [SerializeField] private float jumpDashSpeed = 20f;
    [SerializeField] private float walkSpeed = 6;
    [SerializeField] private Material owningMaterial;
    [SerializeField] private Material otherPlayerMaterial;
    [SerializeField] private Material winningMaterial;
    [SerializeField] private AnimationCurve dashCurve;
    [SerializeField] private AnimationCurve jumpDashCurve;

    private Avatar avatar;
    private CharacterController characterController;
    private MeshRenderer meshRenderer;
    private PlayerTrail trailManager;
    private PlayerInput playerInput;
    private bool isDashing;
    private bool isJumpDashing;
    private float dashProgress;
    private Vector3 velocity;
    private Vector2 previousInputVector = new Vector2(1, 0);

    void Start()
    {
        // Get components
        avatar = GetComponent<Avatar>();
        meshRenderer = GetComponentInChildren<MeshRenderer>();
        characterController = GetComponentInChildren<CharacterController>();
        trailManager = GetComponentInChildren<PlayerTrail>();
        playerInput = GetComponent<PlayerInput>();


        InitializePlayer();
        if (avatar.IsMe)
        {
            if (playerInput)
            {
                playerInput.OnKeyDoubleTapped += OnDoubleTap;
                playerInput.OnJumpAttempt += OnJumpAttempt;
               // playerInput.OnShiftPress += OnShiftPress;
            }
        }
        // player depends on spawn manager and game manager, this ensures that both are present in the scene
        if (!FindObjectOfType<SpawnManager>())
        {
            Instantiate(Resources.Load("SpawnManager") as GameObject);
        }

        if (!FindObjectOfType<GameManager>())
        {
            Instantiate(Resources.Load("GameManager") as GameObject);
        }

        gameObject.name = "Player" + avatar.Possessor.Index;
        OnPlayerJoined.Invoke(avatar);
    }

    void Update()
    {
        // Only let input affect the avatar if it belongs to me
        if (avatar.IsMe && Activated)
        {
            Vector2 inputVector = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

            if (inputVector != Vector2.Zero)
                previousInputVector = inputVector;


            var targetRotation = Quaternion.LookRotation(new Vector3(previousInputVector.X, 0, previousInputVector.Y),
                Vector3.up);

            if (!isJumpDashing)
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 33 * Time.deltaTime);

            // friction/fade out velocity when stopped moving
            if (inputVector == Vector2.Zero && velocity != Vector3.zero)
            {
                velocity.x -= velocity.x * 15f * Time.deltaTime;
                velocity.z -= velocity.z * 15f * Time.deltaTime;
            }
            else if (!isDashing && !isJumpDashing)
                velocity = new Vector3(Input.GetAxis("Horizontal") * walkSpeed, velocity.y,
                    Input.GetAxis("Vertical") * walkSpeed);

            if (!characterController.isGrounded)
            {
                if (velocity.y < 0 && isDashing)
                    velocity.y -=
                        gravityForce * Ease.In(dashProgress, 3) * Time.deltaTime * 0.66f; // less gravity force during dash
                else
                    velocity.y -= gravityForce * Time.deltaTime;
            }

            characterController.Move((velocity * slowMultiplier) * Time.deltaTime);

            // shift based dashing, will probably be removed
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                if (!isDashing && inputVector != Vector2.Zero && !isJumpDashing)
                StartCoroutine(DashRoutine(new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"))
                    .normalized));
                else
                {
                    StopAllCoroutines();
                    isDashing = false;
                    isJumpDashing = false;
                    velocity.x *= 0.25f;
                    velocity.z *= 0.25f;
                }
            }
        }
        else if (isDashing && !trailManager.IsActive)
        {
            trailManager.ActivateTrails();
        }
    }

    void OnJumpAttempt()
    {
        if (isDashing && characterController.isGrounded)
        {
            StartCoroutine(JumpDashRoutine(velocity.normalized));
        }
        else if (characterController.isGrounded && !isDashing && avatar.IsMe)
        {
            velocity.y = jumpForce;
        }
    }

    void OnDoubleTap(KeyCode key)
    {
        if (isDashing || slowMultiplier < 1f || !Activated) return;

        switch (key)
        {
            case KeyCode.LeftArrow:
            {
                StartCoroutine(DashRoutine(Vector3.left));
                break;
            }
            case KeyCode.RightArrow:
            {
                StartCoroutine(DashRoutine(Vector3.right));
                break;
            }
            case KeyCode.UpArrow:
            {
                StartCoroutine(DashRoutine(Vector3.forward));
                break;
            }
            case KeyCode.DownArrow:
            {
                StartCoroutine(DashRoutine(Vector3.back));
                break;
            }
        }
    }

    //void OnShiftPress()
    //{
    //    if (isDashing || isJumpDashing)
    //    {
    //        StopAllCoroutines();
    //        isDashing = false;
    //        isJumpDashing = false;
    //    }
    //}
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("WinningBox"))
        {
            UpdateMaterials(winningMaterial);
            GameManager.OnWin(gameObject);
        }
    }

    private IEnumerator DashRoutine(Vector3 direction)
    {
        if (isJumpDashing) yield return null;
        float elapsedTime = 0f;
        isDashing = true;
        trailManager.ActivateTrails();
        while (elapsedTime < dashDuration && !isJumpDashing)
        {
            elapsedTime += Time.deltaTime;
            dashProgress = elapsedTime / dashDuration;
            float y = velocity.y;
            velocity = direction * walkSpeed * dashCurve.Evaluate(dashProgress);
            velocity.y = y;
            yield return new WaitForEndOfFrame();
        }

        isDashing = false;
        dashProgress = 0f;
    }

    private IEnumerator JumpDashRoutine(Vector3 direction)
    {
        float elapsedTime = 0f;
        isJumpDashing = true;
        trailManager.ActivateTrails(2);
        float spinRotation = 0;
        float spinSpeed = 750;
        direction = new Vector3(direction.x, 0, direction.z).normalized;
        while (elapsedTime < jumpDashDuration)
        {
            elapsedTime += Time.deltaTime;
            dashProgress = elapsedTime / jumpDashDuration;
            velocity = direction * jumpDashSpeed;
            velocity.y = walkSpeed * jumpDashCurve.Evaluate(dashProgress);


            float rotationProgress = dashProgress < 0.5f ? dashProgress * 2 : 2 - dashProgress * 2;
            Vector3 forwardDirection = Vector3.Lerp(
                new Vector3(velocity.x, 0, velocity.z).normalized,
                Vector3.down, Ease.Out(rotationProgress, 2));

            Quaternion newRotation = Quaternion.LookRotation(
                forwardDirection,
                new Vector3(velocity.x,
                    Mathf.Abs(velocity.y), velocity.z).normalized);
            transform.rotation = newRotation;

            spinRotation += spinSpeed * (rotationProgress + 1) * Time.deltaTime;
            transform.RotateAround(transform.position, transform.up, spinRotation);

            if (dashProgress > 0.5f && characterController.isGrounded) elapsedTime = jumpDashDuration;
            yield return new WaitForEndOfFrame();
        }

        isJumpDashing = false;
        dashProgress = 0f;
    }

    private void UpdateMaterials(Material material, bool updateTrail = true)
    {
        meshRenderer.material = material;
        Transform updateMatTrans = transform.Find("UpdateMaterials");
        if (updateMatTrans != null)
        {
            foreach (var childRenderer in updateMatTrans.GetComponentsInChildren<MeshRenderer>())
            {
                childRenderer.material = material;
            }
        }
        if (updateTrail) 
            trailManager.UpdateTrailColor(material.color);
    }

    public void InitializePlayer()
    {
        if (avatar.IsMe)
        {
            if (owningMaterial)
                UpdateMaterials(owningMaterial, false);
            Camera myCamera = Camera.main;
            myCamera.gameObject.GetComponent<SmoothCamera>().Target = transform;
        }
        else
        {
            if (otherPlayerMaterial)
                UpdateMaterials(otherPlayerMaterial, false);
            if (playerInput)
                Destroy(playerInput);

            if (GetComponent<PlayerAbilities>())
                Destroy(GetComponent<PlayerAbilities>());
        }
        trailManager.Initialize(avatar.IsMe);
        Activated = true;
    }

    // This can be used to force a new location, as simply changing position once will not work on all clients
    public void LockPlayerPosition(Vector3 position, float duration)
    {
        StartCoroutine(LockPlayerRoutine(position, duration));
    }

    private IEnumerator LockPlayerRoutine(Vector3 position, float duration)
    {
        Activated = false;
        float elapsedTime = 0;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            transform.position = position;
            yield return new WaitForEndOfFrame();
        }
        Activated = true;
    }

    void OnDestroy()
    {
        OnPlayerLeft.Invoke(avatar);
    }
}
