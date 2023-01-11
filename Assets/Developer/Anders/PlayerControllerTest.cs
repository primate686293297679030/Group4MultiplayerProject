using Alteruna;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Alteruna.Trinity;
using Vector2 = System.Numerics.Vector2;
using Vector3 = UnityEngine.Vector3;

public class PlayerControllerTest : MonoBehaviour
{
    [HideInInspector] public bool Activated;
    [SerializeField] private float jumpForce = 15;
    [SerializeField] private float gravityForce = 45;
    [SerializeField] private float dashDuration = 0.36f;
    [SerializeField] private float walkSpeed = 6;
    [SerializeField] private Material owningMaterial;
    [SerializeField] private Material otherPlayerMaterial;
    [SerializeField] private Material winningMaterial;
    [SerializeField] private AnimationCurve dashCurve;

    private Alteruna.Avatar avatar;
    private CharacterController characterController;
    private MeshRenderer meshRenderer;
    private PlayerTrail trailManager;
    private PlayerInput playerInput;
    private bool isDashing;
    private float dashProgress;
    private Vector3 velocity;

    void Start()
    {
        // Get components
        avatar = GetComponent<Alteruna.Avatar>();
        meshRenderer = GetComponentInChildren<MeshRenderer>();
        characterController = GetComponentInChildren<CharacterController>();
        trailManager = GetComponentInChildren<PlayerTrail>();
        playerInput = GetComponent<PlayerInput>();

        if (avatar.IsMe)
        {
            if (owningMaterial)
                meshRenderer.material = owningMaterial;
            Camera myCamera = Camera.main;
            myCamera.gameObject.GetComponent<SmoothCamera>().Target = transform;

            if (playerInput)
            {
                playerInput.OnKeyDoubleTapped += OnDoubleTap;
                playerInput.OnJumpAttempt += OnJumpAttempt;
            }
        }
        else 
        {
            if (otherPlayerMaterial)
                meshRenderer.material = otherPlayerMaterial;
            if (playerInput)
                Destroy(playerInput);
        }

        trailManager.Initialize(avatar.IsMe);
        Activated = true;
    }

    void Update()
    {
        // Only let input affect the avatar if it belongs to me
        if (avatar.IsMe && Activated)
        {
            Vector2 inputVector = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

            // friction/fade out velocity when stopped moving
            if (inputVector == Vector2.Zero && velocity != Vector3.zero)
            {
                velocity.x -= velocity.x * 15f * Time.deltaTime;
                velocity.z -= velocity.z * 15f * Time.deltaTime;
            }
            else if (!isDashing)
                velocity = new Vector3(Input.GetAxis("Horizontal") * walkSpeed, velocity.y, Input.GetAxis("Vertical") * walkSpeed);

            if (!characterController.isGrounded)
            {
                if (velocity.y < 0 && isDashing)
                    velocity.y -= gravityForce * EaseIn(dashProgress,3) * Time.deltaTime; // less gravity force during dash
                else
                    velocity.y -= gravityForce * Time.deltaTime;
            }

            characterController.Move((velocity) * Time.deltaTime);

            // shift based dashing, will probably be removed
            if (Input.GetKeyDown(KeyCode.LeftShift) && !isDashing && inputVector != Vector2.Zero)
            {
                StartCoroutine(DashRoutine(new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized));
            }
        }
        else if (isDashing && !trailManager.IsActive)
        {
            trailManager.ActivateTrails();
        }
    }

    void OnJumpAttempt()
    {
        if (characterController.isGrounded && !isDashing && avatar.IsMe)
        {
            velocity.y = jumpForce;
        }
    }
    void OnDoubleTap(KeyCode key)
    {
        if (isDashing) return;

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
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("WinningBox"))
        {
            meshRenderer.material = winningMaterial;
        }
        GameManager.OnWin(gameObject);
        Debug.Log("Entered trigger");
    }


    private IEnumerator DashRoutine(Vector3 direction)
    {
        float elapsedTime = 0f;
        isDashing = true;
        trailManager.ActivateTrails();
        while (elapsedTime < dashDuration)
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

    // todo: make "EasingFunctions.cs"
    private float EaseOut(float x, int power)
    {
        return 1 - Mathf.Pow(1 - x, power);
    }
    private float EaseIn(float x, int power)
    {
        return Mathf.Pow(x, power);
    }
}
