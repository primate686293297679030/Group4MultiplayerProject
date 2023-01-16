using System;
using Alteruna;
using System.Collections;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = System.Numerics.Vector2;
using Vector3 = UnityEngine.Vector3;

public class PlayerControllerTest : MonoBehaviour
{
    public static Action<User> OnPlayerJoined = delegate(User user) {  };

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

    private Alteruna.Avatar avatar;
    private CharacterController characterController;
    private MeshRenderer meshRenderer;
    private PlayerTrail trailManager;
    private PlayerInput playerInput;
    private bool isDashing;
    private bool isJumpDashing;
    private float dashProgress;
    private Vector3 velocity;
    private Vector2 previousInputVector = new Vector2(1,0);

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
            
            Transform updateMatTrans = transform.Find("UpdateMaterials");
            if (updateMatTrans != null)
            {
                foreach (var childRenderer in updateMatTrans.GetComponentsInChildren<MeshRenderer>())
                {
                    childRenderer.material = otherPlayerMaterial;
                }
            }

            Destroy(GetComponent<PlayerAbilities>());
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
        trailManager.Initialize(avatar.IsMe);
        Activated = true;
        OnPlayerJoined.Invoke(avatar.Possessor);
    }

    void Update()
    {
        // Only let input affect the avatar if it belongs to me
        if (avatar.IsMe && Activated)
        {
            Vector2 inputVector = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

            if (inputVector != Vector2.Zero) 
                previousInputVector = inputVector;

            
            var targetRotation = Quaternion.LookRotation(new Vector3(previousInputVector.X, 0, previousInputVector.Y), Vector3.up);

            //if (inputVector != Vector2.Zero && !isJumpDashing)
            //    transform.rotation = targetRotation;

            if (!isJumpDashing)
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 33 * Time.deltaTime);

            // friction/fade out velocity when stopped moving
            if (inputVector == Vector2.Zero && velocity != Vector3.zero)
            {
                velocity.x -= velocity.x * 15f * Time.deltaTime;
                velocity.z -= velocity.z * 15f * Time.deltaTime;
            }
            else if (!isDashing && !isJumpDashing)
                velocity = new Vector3(Input.GetAxis("Horizontal") * walkSpeed, velocity.y, Input.GetAxis("Vertical") * walkSpeed);

            if (!characterController.isGrounded)
            {
                if (velocity.y < 0 && isDashing)
                    velocity.y -= gravityForce * EaseIn(dashProgress,3) * Time.deltaTime; // less gravity force during dash
                else
                    velocity.y -= gravityForce * Time.deltaTime;
            }

            characterController.Move((velocity * slowMultiplier) * Time.deltaTime);

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
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("WinningBox"))
        {
            meshRenderer.material = winningMaterial;
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
                    Vector3.down, EaseOut(rotationProgress,2));

            Quaternion newRotation = Quaternion.LookRotation(
                forwardDirection, 
                new Vector3(velocity.x, 
                    Mathf.Abs(velocity.y), velocity.z).normalized);
            transform.rotation = newRotation;

            spinRotation += spinSpeed * (rotationProgress + 1)* Time.deltaTime;
            transform.RotateAround(transform.position, transform.up, spinRotation);

            if (dashProgress > 0.5f && characterController.isGrounded) elapsedTime = jumpDashDuration;
            yield return new WaitForEndOfFrame();
        }

        isJumpDashing = false;
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
    private float easeInOutBack(float x) 
    {
        const float c1 = 1.70158f;
        const float c2 = c1 * 1.525f;

        return x< 0.5
            ? (Mathf.Pow(2 * x, 2) * ((c2 + 1) * 2 * x - c2)) / 2
            : (Mathf.Pow(2 * x - 2, 2) * ((c2 + 1) * (x* 2 - 2) + c2) + 2) / 2;
    }
}
