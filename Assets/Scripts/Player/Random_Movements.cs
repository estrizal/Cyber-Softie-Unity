using UnityEngine;

public class Random_Movements : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 5f;

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

    [Header("References")]
    public Transform body;

    private Rigidbody rb;
    private Quaternion initialBodyRotation;

    private IInteractable currentInteractable;
    private bool isGrounded;

    private Vector2 simulatedDirection;
    private float moveChangeInterval = 2f;
    private float moveChangeTimer;

    private float jumpCooldown = 3f;
    private float jumpTimer;

    private float interactCooldown = 4f;
    private float interactTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        if (body != null)
            initialBodyRotation = body.localRotation;
    }

    void Update()
    {
        GroundCheck();
        SimulateInput();
    }

    void FixedUpdate()
    {
        MoveCharacter();
        FloatEffect();
        TiltCharacter();
    }

    private void SimulateInput()
    {
        moveChangeTimer += Time.deltaTime;
        jumpTimer += Time.deltaTime;
        interactTimer += Time.deltaTime;

        if (moveChangeTimer >= moveChangeInterval)
        {
            simulatedDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
            moveChangeInterval = Random.Range(1.5f, 3f);
            moveChangeTimer = 0f;
        }

        if (jumpTimer >= jumpCooldown && Random.value > 0.7f && isGrounded)
        {
            Jump();
            jumpTimer = 0f;
        }

        if (interactTimer >= interactCooldown && currentInteractable != null && Random.value > 0.8f)
        {
            currentInteractable.Interact();
            interactTimer = 0f;
        }
    }

    private void MoveCharacter()
    {
        if (simulatedDirection == Vector2.zero) return;

        Vector3 moveDirection = new Vector3(simulatedDirection.x, 0, simulatedDirection.y);

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
        if (body == null || simulatedDirection == Vector2.zero) return;

        Vector3 moveDirection = new Vector3(simulatedDirection.x, 0, simulatedDirection.y);

        float forwardTiltX = Mathf.Clamp(Mathf.Abs(moveDirection.z) * tiltAmount, -tiltAmount, tiltAmount);
        float sidewaysTiltYAsX = Mathf.Clamp(Mathf.Abs(moveDirection.x) * tiltAmount, -tiltAmount, tiltAmount);
        float combinedTiltX = forwardTiltX + sidewaysTiltYAsX;

        Quaternion targetTiltRotation = Quaternion.Euler(
            initialBodyRotation.eulerAngles.x + combinedTiltX,
            initialBodyRotation.eulerAngles.y,
            initialBodyRotation.eulerAngles.z
        );

        body.localRotation = Quaternion.Slerp(body.localRotation, targetTiltRotation, Time.fixedDeltaTime * tiltSpeed);
    }

    private void Jump()
    {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        Debug.Log("NPC Jumped!");
    }

    private void GroundCheck()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayers);

#if UNITY_EDITOR
        Debug.DrawLine(groundCheck.position,
            groundCheck.position + Vector3.down * groundCheckRadius,
            Color.green);
#endif
    }

    private void OnTriggerEnter(Collider other)
    {
        var interactable = other.GetComponent<IInteractable>();
        if (interactable != null)
        {
            currentInteractable = interactable;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var interactable = other.GetComponent<IInteractable>();
        if (interactable != null && interactable == currentInteractable)
        {
            currentInteractable = null;
        }
    }
}
