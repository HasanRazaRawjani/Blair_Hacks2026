using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float sprintSpeed = 9f;
    [SerializeField] private float acceleration = 20f;

    [Header("Jump / Gravity")]
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float coyoteTime = 0.12f;
    [SerializeField] private float jumpBufferTime = 0.12f;

    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Slider staminaSlider;

    [Header("Stamina")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaDrain = 20f;
    [SerializeField] private float staminaRegen = 15f;
    [SerializeField] private float sprintThreshold = 10f;

    [Header("Head Bob")]
    [SerializeField] private float walkBobSpeed = 12f;
    [SerializeField] private float walkBobAmount = 0.05f;
    [SerializeField] private float sprintBobSpeed = 16f;
    [SerializeField] private float sprintBobAmount = 0.08f;
    [SerializeField] private float horizontalBobAmount = 0.025f;
    [SerializeField] private float bobSmoothness = 10f;

    [Header("Landing Bob")]
    [SerializeField] private float landingBobAmount = 0.12f;
    [SerializeField] private float landingBobSpeed = 12f;

    [Header("Sprint FOV")]
    [SerializeField] private float normalFOV = 60f;
    [SerializeField] private float sprintFOV = 68f;
    [SerializeField] private float fovSmoothness = 8f;

    private CharacterController controller;
    private Camera playerCamera;

    private Vector3 horizontalVelocity;
    private float verticalVelocity;

    private float lastGroundedTime;
    private float lastJumpPressedTime;

    private float currentStamina;
    private bool isSprinting;

    private Vector3 cameraStartLocalPos;
    private float bobTimer;

    private bool wasGrounded;
    private float landingOffset;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        if (cameraTransform != null)
        {
            cameraStartLocalPos = cameraTransform.localPosition;
            playerCamera = cameraTransform.GetComponent<Camera>();
        }

        currentStamina = maxStamina;

        if (staminaSlider != null)
        {
            staminaSlider.maxValue = maxStamina;
            staminaSlider.value = currentStamina;
        }

        if (playerCamera != null)
        {
            playerCamera.fieldOfView = normalFOV;
        }
    }

    private void Start()
    {
        CursorManager.Lock();
    }

    private void Update()
    {
        HandleCursor();
        HandleJumpInput();
        UpdateGroundedState();
        HandleStamina();
        HandleMovement();
        HandleJump();
        ApplyGravity();

        HandleLandingBob();
        HandleHeadBob();
        HandleFOV();
    }

    private void HandleCursor()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Cursor.lockState == CursorLockMode.Locked)
                CursorManager.Unlock();
            else
                CursorManager.Lock();
        }
    }

    private void UpdateGroundedState()
    {
        if (controller.isGrounded)
        {
            lastGroundedTime = Time.time;
        }
    }

    private void HandleStamina()
    {
        bool tryingToSprint =
            Input.GetKey(KeyCode.LeftShift) &&
            (Input.GetAxisRaw("Horizontal") != 0 ||
             Input.GetAxisRaw("Vertical") != 0);

        if (tryingToSprint && currentStamina > sprintThreshold)
        {
            isSprinting = true;
            currentStamina -= staminaDrain * Time.deltaTime;
        }
        else
        {
            isSprinting = false;
            currentStamina += staminaRegen * Time.deltaTime;
        }

        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);

        if (staminaSlider != null)
        {
            staminaSlider.value = currentStamina;
        }
    }

    private void HandleMovement()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        if (cameraTransform != null)
        {
            forward = cameraTransform.forward;
            right = cameraTransform.right;

            forward.y = 0f;
            right.y = 0f;

            forward.Normalize();
            right.Normalize();
        }

        Vector3 inputDirection =
            (right * x + forward * z).normalized;

        float targetSpeed =
            isSprinting ? sprintSpeed : moveSpeed;

        Vector3 targetVelocity =
            inputDirection * targetSpeed;

        horizontalVelocity = Vector3.MoveTowards(
            horizontalVelocity,
            targetVelocity,
            acceleration * Time.deltaTime
        );

        Vector3 move = new Vector3(
            horizontalVelocity.x,
            0f,
            horizontalVelocity.z
        );

        controller.Move(move * Time.deltaTime);
    }

    private void HandleJumpInput()
    {
        if (Input.GetButtonDown("Jump"))
        {
            lastJumpPressedTime = Time.time;
        }
    }

    private void HandleJump()
    {
        bool canUseCoyoteTime =
            Time.time - lastGroundedTime <= coyoteTime;

        bool bufferedJump =
            Time.time - lastJumpPressedTime <= jumpBufferTime;

        if (bufferedJump && canUseCoyoteTime)
        {
            verticalVelocity =
                Mathf.Sqrt(jumpHeight * -2f * gravity);

            lastJumpPressedTime = -999f;
            lastGroundedTime = -999f;
        }
    }

    private void ApplyGravity()
    {
        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        verticalVelocity += gravity * Time.deltaTime;

        controller.Move(
            Vector3.up * verticalVelocity * Time.deltaTime
        );
    }

    private void HandleLandingBob()
    {
        if (!wasGrounded &&
            controller.isGrounded &&
            verticalVelocity < -5f)
        {
            landingOffset = -landingBobAmount;
        }

        landingOffset = Mathf.Lerp(
            landingOffset,
            0f,
            landingBobSpeed * Time.deltaTime
        );

        wasGrounded = controller.isGrounded;
    }

    private void HandleHeadBob()
    {
        if (cameraTransform == null)
            return;

        bool isMoving =
            controller.isGrounded &&
            (Input.GetAxisRaw("Horizontal") != 0 ||
             Input.GetAxisRaw("Vertical") != 0);

        if (isMoving)
        {
            float speed =
                isSprinting ? sprintBobSpeed : walkBobSpeed;

            float amount =
                isSprinting ? sprintBobAmount : walkBobAmount;

            bobTimer += Time.deltaTime * speed;

            float yOffset =
                Mathf.Sin(bobTimer) * amount;

            float xOffset =
                Mathf.Cos(bobTimer * 0.5f) *
                horizontalBobAmount;

            Vector3 targetPos =
                cameraStartLocalPos +
                new Vector3(
                    xOffset,
                    yOffset + landingOffset,
                    0f
                );

            cameraTransform.localPosition =
                Vector3.Lerp(
                    cameraTransform.localPosition,
                    targetPos,
                    bobSmoothness * Time.deltaTime
                );
        }
        else
        {
            bobTimer = 0f;

            Vector3 targetPos =
                cameraStartLocalPos +
                Vector3.up * landingOffset;

            cameraTransform.localPosition =
                Vector3.Lerp(
                    cameraTransform.localPosition,
                    targetPos,
                    bobSmoothness * Time.deltaTime
                );
        }
    }

    private void HandleFOV()
    {
        if (playerCamera == null)
            return;

        float targetFOV =
            isSprinting ? sprintFOV : normalFOV;

        playerCamera.fieldOfView = Mathf.Lerp(
            playerCamera.fieldOfView,
            targetFOV,
            fovSmoothness * Time.deltaTime
        );
    }
}
