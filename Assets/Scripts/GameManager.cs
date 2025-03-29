using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;
using UnityEngine.AI;

public class GameManager : MonoBehaviour
{
    public CinemachineCamera thirdPersonCinemachineCamera;
    public CinemachineCamera isometricCinemachineCamera;
    public CinemachineCamera shooterMoveCam;
    private bool isShooter = false;
    public static GameManager Instance { get; private set; }
    private bool isIsometricMode = false;

    public Material katanaEnemyMat;

    [Header("Player Settings")]

    public LayerMask possessableLayer;  // Layer for possessable enemies
    public float possessionRange = 2f;   // Range for possession interaction

    public GameObject currentGhost;
    private GameObject _currentPossessedEntity;
    private GhostCharacterController ghostController;
    private EnemyBecomesPlayerController possessedController;
    private ShooterEnemyBecomesPlayerController shooterPossessedController;
    private InputReader ghostInput;
    public int playerRoomNumber;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeGame();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeGame()
    {
        katanaEnemyMat.color = Color.red;
        katanaEnemyMat.SetColor("_EmissionColor", Color.red*0.1f);
        if (currentGhost == null)
        {
            Debug.LogError("No ghost reference set in GameManager!");
            return;
        }

        ghostController = currentGhost.GetComponent<GhostCharacterController>();
        if (ghostController == null)
        {
            Debug.LogError("Ghost object missing GhostCharacterController!");
            return;
        }

        ghostInput = currentGhost.GetComponent<InputReader>();
        if (ghostInput == null)
        {
            Debug.LogError("Ghost object missing InputReader!");
            return;
        }

        // Camera setup
        if (thirdPersonCinemachineCamera && isometricCinemachineCamera)
        {
            thirdPersonCinemachineCamera.Follow = currentGhost.transform;
            isometricCinemachineCamera.Follow = currentGhost.transform;
            thirdPersonCinemachineCamera.gameObject.SetActive(true);
            isometricCinemachineCamera.gameObject.SetActive(false);
            isIsometricMode = false;
        }
        else
        {
            Debug.LogError("Cameras not assigned in GameManager!");
        }

        if (ghostInput != null)
        {
            ghostInput.enabled = true;
            ghostInput.OnInteractPerformed += HandleInteractPerformed;
            ghostInput.OnInteract2Performed += HandleDepossession;
        }
        playerRoomNumber =1;
        _currentPossessedEntity = null;
    }

    private void Start()
    {
        // Remove event subscriptions from Start since we're handling them elsewhere
        if (currentGhost != null)
        {
            ghostInput = currentGhost.GetComponent<InputReader>();
            if (ghostInput == null)
            {
                Debug.LogError("Ghost InputReader not found!");
            }
        }
    }

    private void OnDisable()
    {
        // Clean up event subscription
        if (ghostInput != null)
        {
            ghostInput.OnInteractPerformed -= HandleInteractPerformed;
            ghostInput.OnInteract2Performed -= HandleDepossession;  // Add this line
        }
    }

    private void HandleInteractPerformed()
    {
        if (currentGhost != null && currentGhost.activeSelf)
        {
            TryPossessNearbyEnemy();
        }
    }

    private void HandleDepossession()
    {
        if (_currentPossessedEntity != null)
        {
            Debug.Log("Depossessing current entity");
            UnpossessCurrentEntity();
        }
    }

    private void TryPossessNearbyEnemy()
    {
        Debug.Log("Trying to possess"); // Debug log
        if (currentGhost == null) return;

        Collider[] nearbyEntities = Physics.OverlapSphere(
            currentGhost.transform.position,
            possessionRange,
            possessableLayer
        );

        Debug.Log($"Found {nearbyEntities.Length} possible targets"); // Debug log

        foreach (Collider col in nearbyEntities)
        {
            // Try to get either enemy type controller
            EnemyController katanaEnemy = col.GetComponent<EnemyController>();
            ShooterEnemyCombat shooterEnemy = col.GetComponent<ShooterEnemyCombat>();

            // Check if it's a katana enemy that can be possessed
            if (katanaEnemy != null && !katanaEnemy.isPossessed)
            {
                Debug.Log($"Possessing katana enemy: {col.gameObject.name}");
                PossessEntity(col.gameObject);
                break;
            }
            // Check if it's a shooter enemy that can be possessed
            else if (shooterEnemy != null && !shooterEnemy.isPossessed)
            {
                Debug.Log($"Possessing shooter enemy: {col.gameObject.name}");
                isShooter = true;
                shooterMoveCam.gameObject.SetActive(true);
                PossessEntity(col.gameObject);
                
                break;
            }
        }
    }

    private void PossessEntity(GameObject entity)
    {
        Debug.Log($"Starting possession of {entity.name}");

        if (_currentPossessedEntity != null)
        {
            Debug.Log("Unpossessing current entity first");
            UnpossessCurrentEntity();
        }

        _currentPossessedEntity = entity;

        // Get all required components
        EnemyController enemyController = entity.GetComponent<EnemyController>();
        ShooterEnemyCombat shooterCombat = entity.GetComponent<ShooterEnemyCombat>();
        possessedController = entity.GetComponent<EnemyBecomesPlayerController>();
        shooterPossessedController = entity.GetComponent<ShooterEnemyBecomesPlayerController>();
        InputReader inputReader = entity.GetComponent<InputReader>();
        NavMeshAgent agent = entity.GetComponent<NavMeshAgent>();
        Health health = entity.GetComponent<Health>();

        // Debug component checks
        Debug.Log($"Found components - EnemyController: {enemyController != null}, " +
                  $"ShooterCombat: {shooterCombat != null}, " +
                  $"KatanaController: {possessedController != null}, " +
                  $"ShooterController: {shooterPossessedController != null}, " +
                  $"InputReader: {inputReader != null}, " +
                  $"NavMeshAgent: {agent != null}");

        // Disable AI components
        if (enemyController != null)
        {
            enemyController.isPossessed = true;
            enemyController.enabled = false;
        }
        if (shooterCombat != null)
        {
            shooterCombat.enabled = false;
        }
        if (agent != null)
        {
            agent.enabled = false;
        }

        // Enable appropriate player controller based on enemy type
        bool isShooterEnemy = shooterPossessedController != null;
        
        if (isShooterEnemy)
        {
            if (shooterPossessedController != null)
            {
                shooterPossessedController.enabled = true;
                shooterPossessedController.isPossessed = true;
                shooterPossessedController.gameIsInThirdPerson = !isIsometricMode;
            }
            if (possessedController != null) possessedController.enabled = false;
        }
        else
        {
            if (possessedController != null)
            {
                possessedController.enabled = true;
                possessedController.isPossessed = true;
                possessedController.gameIsInThirdPerson = !isIsometricMode;
            }
            if (shooterPossessedController != null) shooterPossessedController.enabled = false;
        }

        // Setup input system
        if (inputReader != null)
        {
            // Clean up old subscriptions
            inputReader.OnInteract2Performed -= HandleDepossession;
            
            // Reset input system
            inputReader.ResetInput();
            
            // Add new subscriptions and enable
            inputReader.OnInteract2Performed += HandleDepossession;
            inputReader.enabled = true;
            Debug.Log("Reset and enabled possessed entity input system");
        }
        else
        {
            Debug.LogError("No InputReader found on possessed entity!");
            return;
        }

        // Setup camera
        if (thirdPersonCinemachineCamera && isometricCinemachineCamera)
        {
            thirdPersonCinemachineCamera.Follow = entity.transform;
            isometricCinemachineCamera.Follow = entity.transform;
            shooterMoveCam.Follow = entity.transform;
            Debug.Log("Updated camera targets");
        }

        // Handle ghost cleanup
        if (currentGhost != null)
        {
            // Clean up ghost input before disabling
            if (ghostInput != null)
            {
                ghostInput.OnInteractPerformed -= HandleInteractPerformed;
                ghostInput.OnInteract2Performed -= HandleDepossession;
                ghostInput.enabled = false;
            }
            
            currentGhost.SetActive(false);
            ghostController.enabled = false;
            Debug.Log("Cleaned up and disabled ghost");
        }

        // Set common properties
        katanaEnemyMat.color = Color.blue;
        katanaEnemyMat.SetColor("_EmissionColor", Color.blue * 0.1f);
        if (health != null)
        {
            health.maxHealth = 100f;
            health.currentHealth = 100f;
        }
        entity.tag = "Player";
        Debug.Log($"Possession of {entity.name} complete with input reset");
    }

    public void UnpossessCurrentEntity()
    {
        if (_currentPossessedEntity != null)
        {
            // Calculate spawn position with offset
            Vector3 enemyForward = _currentPossessedEntity.transform.forward;
            Vector3 spawnOffset = -enemyForward * 2f;
            spawnOffset.y = 0.5f;
            Vector3 spawnPosition = _currentPossessedEntity.transform.position + spawnOffset;

            // Get all components
            EnemyController enemyController = _currentPossessedEntity.GetComponent<EnemyController>();
            ShooterEnemyCombat shooterCombat = _currentPossessedEntity.GetComponent<ShooterEnemyCombat>();
            InputReader inputReader = _currentPossessedEntity.GetComponent<InputReader>();
            NavMeshAgent enemyAgent = _currentPossessedEntity.GetComponent<NavMeshAgent>();
            Animator animator = _currentPossessedEntity.GetComponent<Animator>();
            Health health = _currentPossessedEntity.GetComponent<Health>();

            // Re-enable AI components
            if (enemyController != null)
            {
                enemyController.isPossessed = false;
                enemyController.enabled = true;
            }
            if (shooterCombat != null)
            {
                shooterCombat.enabled = true;
            }
            if (enemyAgent != null)
            {
                enemyAgent.enabled = true;
            }

            // Disable player controllers
            if (possessedController != null)
            {
                possessedController.enabled = false;
                possessedController.isPossessed = false;
            }
            if (shooterPossessedController != null)
            {
                shooterPossessedController.enabled = false;
                shooterPossessedController.isPossessed = false;
            }

            // Clean up input
            if (inputReader != null)
            {
                inputReader.OnInteract2Performed -= HandleDepossession;
                inputReader.enabled = false;
            }

            if (animator != null)
            {
                animator.SetBool("isMoving", false);
            }
            if (isShooter) {
                shooterMoveCam.gameObject.SetActive(false);
                isShooter = false;
            }
            _currentPossessedEntity.tag = "Room" + playerRoomNumber.ToString();

            // Reset and re-initialize ghost
            if (currentGhost != null)
            {
                // Position and activate ghost
                currentGhost.transform.position = spawnPosition;
                currentGhost.SetActive(true);

                // Reset input system
                if (ghostInput != null)
                {
                    // Clean up old subscriptions
                    ghostInput.OnInteractPerformed -= HandleInteractPerformed;
                    ghostInput.OnInteract2Performed -= HandleDepossession;
                    
                    // Reset input system
                    ghostInput.ResetInput();
                    
                    // Add new subscriptions
                    ghostInput.OnInteractPerformed += HandleInteractPerformed;
                    ghostInput.OnInteract2Performed += HandleDepossession;
                    ghostInput.enabled = true;
                    Debug.Log("Reset ghost input system");
                }

                // Re-enable ghost controller
                ghostController.enabled = true;
                
                // Reset rigidbody
                var ghostRigidbody = currentGhost.GetComponent<Rigidbody>();
                if (ghostRigidbody != null)
                {
                    ghostRigidbody.linearVelocity = Vector3.zero;
                    ghostRigidbody.angularVelocity = Vector3.zero;
                    ghostRigidbody.Sleep(); // Reset physics state
                    ghostRigidbody.WakeUp();
                }

                // Update camera
                if (thirdPersonCinemachineCamera && isometricCinemachineCamera)
                {
                    thirdPersonCinemachineCamera.Follow = currentGhost.transform;
                    isometricCinemachineCamera.Follow = currentGhost.transform;
                }
            }
            health.maxHealth = 30f;
            health.currentHealth = 30f;
            katanaEnemyMat.color = Color.red;
            katanaEnemyMat.SetColor("_EmissionColor", Color.red*0.1f);
            _currentPossessedEntity = null;
            possessedController = null;
            shooterPossessedController = null;
            
            Debug.Log("Depossession complete with input reset");
        }
    }

    public GameObject GetCurrentPossessedEntity()
    {
        return _currentPossessedEntity;
    }

    public void SetPossessedEntity(GameObject entity)
    {
        _currentPossessedEntity = entity;
    }


    private void OnDrawGizmos()
    {
        if (currentGhost != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(currentGhost.transform.position, possessionRange);
        }
    }
}