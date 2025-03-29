using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using Unity.Cinemachine;

public class EnemyBecomesPlayerController : MonoBehaviour
{
    public float moveSpeed = 5f; // Movement speed
    public float walkSpeed = 5f; // Movement speed
    public float sprintSpeed = 7f; // Movement speed
    public Transform thirdPersonCameraTransform; // Reference to the main camera's transform
    public Transform isometricCameraTransform; // Reference to the main camera's transform
    
    public CinemachineCamera thirdPersonCinemachineCamera;
    public CinemachineCamera isometricCinemachineCamera;
    public bool gameIsInThirdPerson;
    [SerializeField] private int basePriority = 10;
    [SerializeField] private int activePriority = 11;
    
    [Header("Dash Settings")]
    public float dashForce = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    private bool canDash = true;
    private bool isDashing = false;

    [Header("Shield Settings")]
    public GameObject shieldObject; // Reference to your existing shield GameObject
    private bool _isBlocking = false;

    public bool isBlocking
    {
        get { return _isBlocking; }
        private set { _isBlocking = value; }
    }

    private Rigidbody rb; // Reference to the Rigidbody component
    public InputReader InputReader { get; private set; } 
    private Quaternion initialBodyRotation; // Stores the initial rotation of the Body
    private Animator animator;
    private bool attacking = false;
    public float attackCooldown = 0.9f;
    
    public float brakeStrength = 1f; // Strength of braking force
    private bool isMoving = false;
    public float animationFactor = 1.2f;
    private GameManager gameManager;
    private Health health;
    public bool isPossessed = false;
    
    private IInteractable currentInteractable;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        InputReader = gameObject.GetComponent<InputReader>();
        
        // Freeze rotation constraints to prevent tipping over
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        InputReader.cursorIsLocked = true;
        
        initialBodyRotation = transform.rotation;
        animator = GetComponent<Animator>();
        health = GetComponent<Health>();
        health.onDeath.AddListener(HandleDeath);
        health.onDamaged.AddListener(HandleDamaged);
        
        // Initialize shield
        if (shieldObject != null)
        {
            shieldObject.SetActive(false); // Ensure shield starts disabled
        }

        // Setup shield
        if (shieldObject != null)
        {
            // Make sure shield has collider and proper tag
            BoxCollider shieldCollider = shieldObject.GetComponent<BoxCollider>();
            if (shieldCollider == null)
            {
                shieldCollider = shieldObject.AddComponent<BoxCollider>();
            }
            shieldCollider.isTrigger = true;
            shieldObject.tag = "Shield";
            shieldObject.SetActive(false);
        }
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
    private void OnEnable()
    {
        InputReader.OnDashPerformed += OnDash;
        if (InputReader != null)
        {
            InputReader.OnAttackPerformed += HandleAttack;
        }
    }

    private void OnDisable()
    {
        InputReader.OnDashPerformed -= OnDash;
        if (InputReader != null)
        {
            InputReader.OnAttackPerformed -= HandleAttack;
        }
    }

    private void Start()
    {
        gameManager = GameManager.Instance;
        
        // Disable controller if not possessed
        if (!isPossessed)
        {
            enabled = false;
            if (InputReader != null)
            {
                InputReader.enabled = false;
            }
        }
        else
        {
            gameManager.SetPossessedEntity(gameObject);
        }
    }
    private void Update()
    {
        // Handle cursor lock without affecting camera activation
        Cursor.lockState = InputReader.cursorIsLocked 
            ? CursorLockMode.Locked 
            : CursorLockMode.None;

        // Set fixed priorities
        thirdPersonCinemachineCamera.Priority = gameIsInThirdPerson 
            ? activePriority 
            : basePriority;
    
        isometricCinemachineCamera.Priority = !gameIsInThirdPerson 
            ? activePriority 
            : basePriority;
        //Animations:
        isMoving = rb.linearVelocity.magnitude > 0.0005f;
        animator.SetBool("isMoving", isMoving);
        
        if (isMoving)
        {
            animator.SetFloat("Speed", rb.linearVelocity.magnitude/moveSpeed*animationFactor);
        }
    }
        
    private void FixedUpdate()
    {
        if (!isDashing)
        {
            MoveCharacter(gameIsInThirdPerson);
            if (InputReader.isSprinting){
                moveSpeed = sprintSpeed; 
            }
            else{
                moveSpeed = walkSpeed;
            }
        }
        if (InputReader.isBlocking) {
            StartBlocking();

        }
        else {
            StopBlocking();
        }
    }

    private void MoveCharacter(bool gameIsInThirdPerson)
    {
        if (gameIsInThirdPerson)
        {
            if (InputReader.MoveComposite == Vector2.zero)
            {
                ApplyBrakes(brakeStrength); // Stop movement if no input
                return;
            }

            // Get camera's forward and right vectors (ignoring vertical axis)
            Vector3 cameraForward = thirdPersonCameraTransform.forward;
            cameraForward.y = 0f;
            cameraForward.Normalize();

            Vector3 cameraRight = thirdPersonCameraTransform.right;
            cameraRight.y = 0f;
            cameraRight.Normalize();

            // Calculate movement direction based on camera orientation and input
            Vector3 moveDirection = (cameraForward * InputReader.MoveComposite.y + cameraRight * InputReader.MoveComposite.x).normalized;

            // Apply force to move the Rigidbody in the calculated direction
            rb.AddForce(moveDirection * moveSpeed, ForceMode.Acceleration);

            // Rotate the Rigidbody to face the movement direction
            if (moveDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * 10f); // Smooth rotation
            }
        }
        else // Isometric mode
        {
            if (InputReader.MoveComposite == Vector2.zero)
            {
                ApplyBrakes(brakeStrength); // Stop movement if no input
                return;
            }

            // For isometric perspective, use fixed 45-degree rotated axes
            Vector3 isoForward = new Vector3(0.707f, 0, 0.707f);  // Normalized (1,0,1)
            Vector3 isoRight = new Vector3(0.707f, 0, -0.707f);   // Normalized (-1,0,1)

            Vector3 moveDirection = (isoForward * InputReader.MoveComposite.y +
                                     isoRight * InputReader.MoveComposite.x).normalized;

            // Apply force to move the Rigidbody in the calculated direction
            rb.AddForce(moveDirection * moveSpeed, ForceMode.Acceleration);

            // Smooth rotation towards movement direction
            if (moveDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation,
                    Time.fixedDeltaTime * 10f);
            }
        }
    }

    private void ApplyBrakes(float brakeStrength)
    {
        // Gradually reduce velocity to stop the Rigidbody when no input is provided
        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        rb.AddForce(-horizontalVelocity * brakeStrength, ForceMode.Acceleration);
    }

    private void HandleAttack()
    {
        if (!attacking && !isBlocking) // Only attack if not blocking
        {
            StartCoroutine(StartAttack());
        }
    }

    IEnumerator StartAttack()
    {
        attacking = true;
        ApplyBrakes(brakeStrength * 70);
        
        // Play attack animation
        
        animator.SetTrigger("Attack");
        
        // Get katana slash component
        KatanaSlash katanaSlash = GetComponentInChildren<KatanaSlash>();
        if (katanaSlash != null)
        {
            yield return new WaitForSeconds(0.1f);
            katanaSlash.ActivateSlash();
        }
        
        // Wait for attack animation to complete
        yield return new WaitForSeconds(attackCooldown);
        attacking = false;
    }

    private void HandleDeath()
    {
        // Disable controls
        enabled = false;
        rb.isKinematic = true;
        animator.Play("Death");
        // Add game over logic here
    }

    private void HandleDamaged()
    {
        // Play hit animation
        //animator.Play("Hit");
        
        // Ensure rigidbody is not kinematic during knockback
        rb.isKinematic = false;
        
        // Apply additional player-specific knockback response
        if (rb != null)
        {
            if (GameManager.Instance.GetCurrentPossessedEntity() != null) {
                Vector3 knockbackDir = transform.position - GameManager.Instance.GetCurrentPossessedEntity().transform.position;
            knockbackDir.y = 0;
            knockbackDir.Normalize();
            
            // Stronger knockback for player
            rb.AddForce(knockbackDir * 1.5f, ForceMode.Impulse);
                }// Get direction from the last attacker
            
        }
    }

    private void OnDash()
    {
        if (canDash && !isDashing)
        {
            if (attacking)
            {
                StopCoroutine(StartAttack());
                attacking = false;
                animator.Play("idle");
            }
            StartCoroutine(PerformDash());
        }
    }

    private IEnumerator PerformDash()
    {
        isDashing = true;
        canDash = false;

        // Get dash direction based on current input and game mode
        Vector3 dashDirection;
        if (InputReader.MoveComposite.magnitude > 0.1f)
        {
            if (gameIsInThirdPerson)
            {
                // Third person dash direction
                Vector3 cameraForward = thirdPersonCameraTransform.forward;
                cameraForward.y = 0f;
                Vector3 cameraRight = thirdPersonCameraTransform.right;
                cameraRight.y = 0f;
                dashDirection = (cameraForward * InputReader.MoveComposite.y + 
                               cameraRight * InputReader.MoveComposite.x).normalized;
            }
            else
            {
                // Isometric dash direction
                Vector3 isoForward = new Vector3(0.707f, 0, 0.707f);
                Vector3 isoRight = new Vector3(0.707f, 0, -0.707f);
                dashDirection = (isoForward * InputReader.MoveComposite.y +
                               isoRight * InputReader.MoveComposite.x).normalized;
            }
        }
        else
        {
            dashDirection = transform.forward;
        }

        // Apply dash force
        float startTime = Time.time;
        while (Time.time < startTime + dashDuration)
        {
            rb.linearVelocity = dashDirection * dashForce;
            yield return null;
        }

        
        rb.linearVelocity = dashDirection*3.0f;
        isDashing = false;

        // Start cooldown
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private void StartBlocking()
    {
        if (!attacking && !isDashing)
        {
            isBlocking = true;
            animator.SetBool("isBlocking", true);
            if (shieldObject != null)
            {
                shieldObject.SetActive(true);
            }
        }
    }

    private void StopBlocking()
    {
        isBlocking = false;
        animator.SetBool("isBlocking", false);
        if (shieldObject != null)
        {
            shieldObject.SetActive(false);
        }
    }
}