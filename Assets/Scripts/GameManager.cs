using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;
using UnityEngine.AI;

public class GameManager : MonoBehaviour
{
    public CinemachineCamera thirdPersonCinemachineCamera;
    public CinemachineCamera isometricCinemachineCamera;
    public static GameManager Instance { get; private set; }
    private bool isIsometricMode = false;

    [Header("Player Settings")]

    public LayerMask possessableLayer;  // Layer for possessable enemies
    public float possessionRange = 2f;   // Range for possession interaction

    public GameObject currentGhost;
    private GameObject _currentPossessedEntity;
    private GhostCharacterController ghostController;
    private EnemyBecomesPlayerController possessedController;
    private InputReader ghostInput;

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

        _currentPossessedEntity = null;
    }

    private void Start()
    {
        // Get and store the InputReader once
        if (currentGhost != null)
        {
            ghostInput = currentGhost.GetComponent<InputReader>();
            if (ghostInput != null)
            {
                // Subscribe to the event once
                ghostInput.OnInteractPerformed += HandleInteractPerformed;
            }
        }
    }

    private void OnDisable()
    {
        // Clean up event subscription
        if (ghostInput != null)
        {
            ghostInput.OnInteractPerformed -= HandleInteractPerformed;
        }
    }

    private void HandleInteractPerformed()
    {
        if (currentGhost != null && currentGhost.activeSelf)
        {
            TryPossessNearbyEnemy();
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
            EnemyController enemy = col.GetComponent<EnemyController>();
            if (enemy != null && !enemy.isPossessed)
            {
                Debug.Log($"Possessing {col.gameObject.name}"); // Debug log
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
        possessedController = entity.GetComponent<EnemyBecomesPlayerController>();
        InputReader inputReader = entity.GetComponent<InputReader>();
        NavMeshAgent agent = entity.GetComponent<NavMeshAgent>();

        // Debug component checks
        Debug.Log($"Found components - EnemyController: {enemyController != null}, " +
                  $"PlayerController: {possessedController != null}, " +
                  $"InputReader: {inputReader != null}, " +
                  $"NavMeshAgent: {agent != null}");

        // Handle enemy controller
        if (enemyController != null)
        {
            enemyController.isPossessed = true;
            enemyController.enabled = false;
            if (agent != null)
            {
                agent.enabled = false;
            }
            Debug.Log("Disabled enemy AI and navigation");
        }

        // Handle player controller
        if (possessedController != null)
        {
            possessedController.enabled = true;
            possessedController.isPossessed = true;

            // Setup input system
            if (inputReader != null)
            {
                inputReader.enabled = true;
                Debug.Log("Enabled player controller and input");
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
                Debug.Log("Updated camera targets");
            }
        }
        else
        {
            Debug.LogError("No EnemyBecomesPlayerController found on possessed entity!");
            return;
        }

        // Handle ghost
        if (currentGhost != null)
        {
            currentGhost.SetActive(false);
            ghostController.enabled = false;
            if (ghostInput != null)
            {
                ghostInput.enabled = false;
            }
            Debug.Log("Disabled ghost");
        }

        Debug.Log($"Possession of {entity.name} complete");
    }

    public void UnpossessCurrentEntity()
    {
        if (_currentPossessedEntity != null)
        {
            // Re-enable enemy controller and disable player controller
            EnemyController enemyController = _currentPossessedEntity.GetComponent<EnemyController>();
            InputReader inputReader = _currentPossessedEntity.GetComponent<InputReader>();

            if (enemyController != null)
            {
                enemyController.isPossessed = false;
                enemyController.enabled = true;  // Re-enable enemy AI
                enemyController.GetComponent<NavMeshAgent>().enabled = true;  // Re-enable navigation
            }

            if (possessedController != null && inputReader != null)
            {
                possessedController.enabled = false;
                possessedController.isPossessed = false;
                inputReader.enabled = false;  // Disable input processing
            }

            // Re-enable ghost
            if (currentGhost != null)
            {
                currentGhost.transform.position = _currentPossessedEntity.transform.position + Vector3.up * 2f;
                currentGhost.SetActive(true);
                ghostController.enabled = true;

                var ghostInput = currentGhost.GetComponent<InputReader>();
                if (ghostInput != null)
                {
                    ghostInput.enabled = true;
                }

                // Reset camera targets to ghost
                if (thirdPersonCinemachineCamera && isometricCinemachineCamera)
                {
                    thirdPersonCinemachineCamera.Follow = currentGhost.transform;
                    isometricCinemachineCamera.Follow = currentGhost.transform;
                }
            }

            _currentPossessedEntity = null;
            possessedController = null;
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

    public void SwitchToIsometricCamera()
    {
        if (!isIsometricMode && thirdPersonCinemachineCamera && isometricCinemachineCamera)
        {
            thirdPersonCinemachineCamera.gameObject.SetActive(false);
            isometricCinemachineCamera.gameObject.SetActive(true);

            // Update the follow target based on current state
            Transform targetToFollow = currentGhost != null && currentGhost.activeSelf ?
                currentGhost.transform :
                _currentPossessedEntity?.transform;

            if (targetToFollow != null)
            {
                isometricCinemachineCamera.Follow = targetToFollow;
            }

            isIsometricMode = true;
            Debug.Log("Switched to isometric camera");
        }
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