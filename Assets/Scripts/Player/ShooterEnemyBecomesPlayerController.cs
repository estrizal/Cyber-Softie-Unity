using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using System.Collections;

public class ShooterEnemyBecomesPlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float walkSpeed = 5f;
    public float sprintSpeed = 7f;
    public float brakeStrength = 1f;
    
    [Header("Camera Settings")]
    public Transform thirdPersonCameraTransform;
    public Transform isometricCameraTransform;
    public CinemachineCamera thirdPersonCinemachineCamera;
    public CinemachineCamera isometricCinemachineCamera;
    public bool gameIsInThirdPerson;
    [SerializeField] private int basePriority = 10;
    [SerializeField] private int activePriority = 11;

    [Header("Combat Settings")]
    public Transform weaponHolder;
    public float shootCooldown = 0.5f;
    private bool isShooting = false;
    
    [Header("Dash Settings")]
    public float dashForce = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    private bool canDash = true;
    private bool isDashing = false;

    private Rigidbody rb;
    public InputReader InputReader { get; private set; }
    private Animator animator;
    private bool isMoving = false;
    public float animationFactor = 1.2f;
    private GameManager gameManager;
    private Health health;
    public bool isPossessed = false;
    private Gun gun;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        InputReader = GetComponent<InputReader>();
        animator = GetComponent<Animator>();
        health = GetComponent<Health>();
        gun = GetComponentInChildren<Gun>();

        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        
        InputReader.cursorIsLocked = true;
        
        // Set up event listeners
        health.onDeath.AddListener(HandleDeath);
        health.onDamaged.AddListener(HandleDamaged);
    }

    private void OnEnable()
    {
        InputReader.OnDashPerformed += OnDash;
        if (InputReader != null)
        {
            InputReader.OnAttackPerformed += HandleShoot;
        }
    }

    private void OnDisable()
    {
        InputReader.OnDashPerformed -= OnDash;
        if (InputReader != null)
        {
            InputReader.OnAttackPerformed -= HandleShoot;
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
        Cursor.lockState = InputReader.cursorIsLocked ? CursorLockMode.Locked : CursorLockMode.None;

        thirdPersonCinemachineCamera.Priority = gameIsInThirdPerson ? activePriority : basePriority;
        isometricCinemachineCamera.Priority = !gameIsInThirdPerson ? activePriority : basePriority;

        // Update animations
        isMoving = rb.linearVelocity.magnitude > 0.0005f;
        animator.SetBool("isMoving", isMoving);
        
        if (isMoving)
        {
            animator.SetFloat("Speed", rb.linearVelocity.magnitude/moveSpeed*animationFactor);
        }

        // Update weapon aim
        if (gameIsInThirdPerson && weaponHolder != null)
        {
            UpdateWeaponAim();
        }
    }

    private void FixedUpdate()
    {
        if (!isDashing)
        {
            MoveCharacter(gameIsInThirdPerson);
            moveSpeed = InputReader.isSprinting ? sprintSpeed : walkSpeed;
        }
    }

    private void MoveCharacter(bool gameIsInThirdPerson)
    {
        if (InputReader.MoveComposite == Vector2.zero)
        {
            ApplyBrakes(brakeStrength);
            return;
        }

        Vector3 moveDirection;
        if (gameIsInThirdPerson)
        {
            Vector3 cameraForward = thirdPersonCameraTransform.forward;
            cameraForward.y = 0f;
            cameraForward.Normalize();

            Vector3 cameraRight = thirdPersonCameraTransform.right;
            cameraRight.y = 0f;
            cameraRight.Normalize();

            moveDirection = (cameraForward * InputReader.MoveComposite.y +
                           cameraRight * InputReader.MoveComposite.x).normalized;
        }
        else
        {
            Vector3 isoForward = new Vector3(0.707f, 0, 0.707f);
            Vector3 isoRight = new Vector3(0.707f, 0, -0.707f);
            moveDirection = (isoForward * InputReader.MoveComposite.y +
                           isoRight * InputReader.MoveComposite.x).normalized;
        }

        // Increase movement speed by 10x
        rb.AddForce(moveDirection * moveSpeed * 10f, ForceMode.Acceleration);

        // Rotate the GameObject on the Y-axis
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * 10f);
        }
    }

    private void UpdateWeaponAim()
    {
        if (weaponHolder == null || InputReader == null) return;

        // Rotate the weapon holder on the X-axis using LookComposite
        float lookVertical = InputReader.LookComposite.y; // Vertical look input
        Vector3 weaponRotation = weaponHolder.localEulerAngles;
        weaponRotation.x -= lookVertical; // Adjust X rotation
        weaponRotation.x = Mathf.Clamp(weaponRotation.x, -45f, 45f); // Clamp to prevent extreme angles
        weaponHolder.localEulerAngles = weaponRotation;
    }

    private void HandleShoot()
    {
        if (!isShooting && gun != null)
        {
            StartCoroutine(Shoot());
        }
    }

    private IEnumerator Shoot()
    {
        isShooting = true;
        animator.SetTrigger("Shoot");
        
        // Get shooting direction based on camera
        Vector3 shootDirection = gameIsInThirdPerson ? 
            thirdPersonCameraTransform.forward : 
            transform.forward;
            
        gun.Shoot(shootDirection);
        
        yield return new WaitForSeconds(shootCooldown);
        isShooting = false;
    }

    private void ApplyBrakes(float brakeStrength)
    {
        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        rb.AddForce(-horizontalVelocity * brakeStrength, ForceMode.Acceleration);
    }

    private void HandleDeath()
    {
        enabled = false;
        rb.isKinematic = true;
        animator.Play("Death");
    }

    private void HandleDamaged()
    {
        rb.isKinematic = false;
        
        if (rb != null && GameManager.Instance.GetCurrentPossessedEntity() != null)
        {
            Vector3 knockbackDir = transform.position - GameManager.Instance.GetCurrentPossessedEntity().transform.position;
            knockbackDir.y = 0;
            knockbackDir.Normalize();
            rb.AddForce(knockbackDir * 1.5f, ForceMode.Impulse);
        }
    }

    private void OnDash()
    {
        if (canDash && !isDashing && !isShooting)
        {
            StartCoroutine(PerformDash());
        }
    }

    private IEnumerator PerformDash()
    {
        isDashing = true;
        canDash = false;

        Vector3 dashDirection;
        if (InputReader.MoveComposite.magnitude > 0.1f)
        {
            if (gameIsInThirdPerson)
            {
                Vector3 cameraForward = thirdPersonCameraTransform.forward;
                cameraForward.y = 0f;
                Vector3 cameraRight = thirdPersonCameraTransform.right;
                cameraRight.y = 0f;
                dashDirection = (cameraForward * InputReader.MoveComposite.y + 
                               cameraRight * InputReader.MoveComposite.x).normalized;
            }
            else
            {
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

        float startTime = Time.time;
        while (Time.time < startTime + dashDuration)
        {
            rb.linearVelocity = dashDirection * dashForce;
            yield return null;
        }

        rb.linearVelocity = dashDirection * 3.0f;
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
}
