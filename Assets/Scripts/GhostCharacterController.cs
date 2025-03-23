using UnityEngine;
using Unity.Cinemachine;

public class GhostCharacterController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 5f;

    [Header("References")]
    public Transform thirdPersonCameraTransform;
    public Transform isometricCameraTransform;
    public Transform body;
    public CinemachineCamera thirdPersonCinemachineCamera;
    public CinemachineCamera isometricCinemachineCamera;
    public bool gameIsInThirdPerson;
    [SerializeField] private int basePriority = 10;
    [SerializeField] private int activePriority = 11;

    [Header("Floating Settings")]
    public float floatHeight = 0.5f;
    public float floatSpeed = 2f;

    [Header("Tilt Settings")]
    public float tiltAmount = 15f;
    public float tiltSpeed = 5f;

    [Header("Ground Check Settings")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayers;

    private Rigidbody rb;
    private InputReader inputReader;
    private Quaternion initialBodyRotation;

    private IInteractable currentInteractable;
    private bool isGrounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        inputReader = GetComponent<InputReader>();

        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        if (body != null)
            initialBodyRotation = body.localRotation;

        inputReader.cursorIsLocked = true;
        //gameIsInThirdPerson = true;

        // Subscribe to interaction and jump input
        inputReader.OnInteractPerformed += HandleInteractAction;
        inputReader.OnJumpPerformed += HandleJumpAction;
    }

    void OnDestroy()
    {
        inputReader.OnInteractPerformed -= HandleInteractAction;
        inputReader.OnJumpPerformed -= HandleJumpAction;
    }

    void Update()
    {
        Cursor.lockState = inputReader.cursorIsLocked ? CursorLockMode.Locked : CursorLockMode.None;

        thirdPersonCinemachineCamera.Priority = gameIsInThirdPerson ? activePriority : basePriority;
        isometricCinemachineCamera.Priority = !gameIsInThirdPerson ? activePriority : basePriority;

        GroundCheck();
    }

    void FixedUpdate()
    {
        MoveCharacter(gameIsInThirdPerson);

        //if (inputReader.MoveComposite == Vector2.zero && isGrounded)
        FloatEffect();

        TiltCharacter();
    }

    private void MoveCharacter(bool gameIsInThirdPerson)
    {
        if (inputReader.MoveComposite == Vector2.zero) return;

        Vector3 moveDirection;

        if (gameIsInThirdPerson)
        {
            Vector3 cameraForward = thirdPersonCameraTransform.forward;
            cameraForward.y = 0f; cameraForward.Normalize();
            Vector3 cameraRight = thirdPersonCameraTransform.right;
            cameraRight.y = 0f; cameraRight.Normalize();

            moveDirection =
                (cameraForward * inputReader.MoveComposite.y + cameraRight * inputReader.MoveComposite.x).normalized;
        }
        else // Isometric mode
        {
            Vector3 isoForward = new Vector3(0.707f, 0, 0.707f);
            Vector3 isoRight = new Vector3(0.707f, 0, -0.707f);

            moveDirection =
                (isoForward * inputReader.MoveComposite.y + isoRight * inputReader.MoveComposite.x).normalized;
        }

        rb.MovePosition(rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime);

        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * 10f);
        }
    }

    private void FloatEffect()
    {
        if (body == null) return;

        float offsetY = Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        body.localPosition = new Vector3(0, offsetY, 0);
    }

    private void TiltCharacter()
    {
        if (body == null) return;

        if (inputReader.MoveComposite != Vector2.zero)
        {
            Vector3 cameraForward = thirdPersonCameraTransform.forward;
            cameraForward.y = 0f; cameraForward.Normalize();
            Vector3 cameraRight = thirdPersonCameraTransform.right;
            cameraRight.y = 0f; cameraRight.Normalize();

            Vector3 moveDirection =
                (cameraForward * inputReader.MoveComposite.y + cameraRight * inputReader.MoveComposite.x).normalized;

            float forwardTiltX =
                Mathf.Clamp(Mathf.Abs(moveDirection.z) * tiltAmount, -tiltAmount, tiltAmount);
            float sidewaysTiltYAsX =
                Mathf.Clamp(Mathf.Abs(moveDirection.x) * tiltAmount, -tiltAmount, tiltAmount);

            float combinedTiltX = forwardTiltX + sidewaysTiltYAsX;

            Quaternion targetTiltRotation =
                Quaternion.Euler(initialBodyRotation.eulerAngles.x + combinedTiltX,
                                 initialBodyRotation.eulerAngles.y,
                                 initialBodyRotation.eulerAngles.z);

            body.localRotation =
                Quaternion.Slerp(body.localRotation, targetTiltRotation, Time.fixedDeltaTime * tiltSpeed);
        }
        else
            body.localRotation =
                Quaternion.Slerp(body.localRotation, initialBodyRotation, Time.fixedDeltaTime * tiltSpeed);
    }

    // Interaction logic using collision triggers
    private void OnTriggerEnter(Collider other)
    {
        var interactable = other.GetComponent<IInteractable>();

        if (interactable != null)
        {
            currentInteractable = interactable;
            InteractionPromptUI.Instance.ShowPrompt(interactable.InteractionPrompt);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var interactable = other.GetComponent<IInteractable>();

        if (interactable != null && interactable == currentInteractable)
        {
            currentInteractable = null;
            InteractionPromptUI.Instance.HidePrompt();
        }
    }

    // Interaction handling
    private void HandleInteractAction()
    {
        currentInteractable?.Interact();
    }

    // Jump handling
    private void HandleJumpAction()
    {
        if (!isGrounded) return;

        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

        Debug.Log("Jumped!");

        // Optional: trigger jump animation or sound here
    }

    // Ground check logic
    private void GroundCheck()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayers);

#if UNITY_EDITOR
        Debug.DrawLine(groundCheck.position,
            groundCheck.position + Vector3.down * groundCheckRadius,
            Color.green);
#endif
    }
}
