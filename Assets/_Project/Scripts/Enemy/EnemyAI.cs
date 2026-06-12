using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using TMPro;

public enum EnemyState
{
    Patrol,
    Chase,
    Attack
}

public class EnemyAI : MonoBehaviour, IDamageable
{
    private NavMeshAgent agent;

    [Header("Target")]
    public Transform player;
    private PlayerHealth playerHealth;
    private Camera playerCamera;

    [Header("State")]
    public EnemyState currentState;

    [Header("Distance")]
    public float chaseRange = 10f;
    public float attackRange = 2f;

    [Header("Attack")]
    public int damage = 10;
    public float attackCooldown = 1.5f;
    private float nextAttackTime;

    // --- MODIFIED: Health System ---
    [Header("Health")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("Health Bar UI")]
    public Slider hpSlider;
    public Image hpFillImage;
    public TMP_Text hpText;
    public Canvas hpCanvas;
    public float hpBarHeight = 2.5f;

    [Header("VFX")]
    public GameObject deathParticlePrefab;
    public float deathParticleLifetime = 2f;

    [Header("Speed")]
    private float baseSpeed;
    public float speedMultiplier = 0.8f;

    [Header("Height Fix")]
    public float heightOffset = 0.5f;

    [Header("Patrol")]
    public float patrolRadius = 6f;
    public float patrolWaitTime = 2f;
    private Vector3 patrolTarget;
    private float patrolTimer;


    // =========================================
    // EVENTS (decoupled communication)
    // =========================================

    public static System.Action OnEnemyKilled;


    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerHealth = playerObj.GetComponent<PlayerHealth>();
            playerCamera = playerObj.GetComponentInChildren<Camera>();
        }

        PlayerController pc = playerObj?.GetComponent<PlayerController>();
        baseSpeed = pc != null ? pc.moveSpeed : 5f;

        agent.speed = baseSpeed * speedMultiplier;

        FixHeight();
        InitializePatrol();
    }

    void InitHealth()
    {
        currentHealth = maxHealth;
        UpdateHPBar();
    }

    void OnEnable()
    {
        FixHeight();
        if (agent != null)
        {
            agent.Warp(transform.position);
            InitializePatrol();
        }
        InitHealth();
        GameManager.Instance?.RegisterEnemy(this);
    }

    void OnDisable()
    {
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
        GameManager.Instance?.UnregisterEnemy(this);
    }

    // =========================================
    // IDamageable IMPLEMENTATION (modified: multi-hit)
    // =========================================

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        UpdateHPBar();

        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        if (deathParticlePrefab != null)
        {
            GameObject fx = Instantiate(deathParticlePrefab, transform.position, Quaternion.identity);
            Destroy(fx, deathParticleLifetime);
        }

        ReturnToPool();

        OnEnemyKilled?.Invoke();
    }

    void UpdateHPBar()
    {
        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHealth;
            hpSlider.value = currentHealth;
        }

        if (hpText != null)
            hpText.text = $"{currentHealth}/{maxHealth}";

        if (hpFillImage != null)
        {
            float pct = (float)currentHealth / maxHealth;
            hpFillImage.fillAmount = pct;

            if (pct > 0.6f)
                hpFillImage.color = Color.green;
            else if (pct > 0.3f)
                hpFillImage.color = Color.yellow;
            else
                hpFillImage.color = Color.red;
        }
    }

    // =========================================
    // STATE SYSTEM
    // =========================================

    void InitializePatrol()
    {
        patrolTimer = 0;
        nextAttackTime = 0;
        currentState = EnemyState.Patrol;
        patrolTarget = GetRandomPatrolPoint();
        if (agent != null)
        {
            agent.isStopped = false;
            agent.SetDestination(patrolTarget);
        }
    }

    void Update()
    {
        if (player == null) return;

        // --- MODIFIED: HP bar always faces player camera ---
        FaceCamera();

        float distance = Vector3.Distance(transform.position, player.position);

        UpdateState(distance);
        HandleState();
    }

    void FaceCamera()
    {
        if (hpCanvas == null) return;

        Camera cam = playerCamera != null ? playerCamera : Camera.main;
        if (cam == null) return;

        hpCanvas.transform.LookAt(cam.transform);
        hpCanvas.transform.Rotate(0, 180, 0);

        // Keep HP bar above enemy
        Vector3 hpPos = transform.position;
        hpPos.y += hpBarHeight;
        hpCanvas.transform.position = hpPos;
    }

    void UpdateState(float distance)
    {
        if (distance <= attackRange)
            currentState = EnemyState.Attack;
        else if (distance <= chaseRange)
            currentState = EnemyState.Chase;
        else
            currentState = EnemyState.Patrol;
    }

    void HandleState()
    {
        switch (currentState)
        {
            case EnemyState.Patrol: Patrol(); break;
            case EnemyState.Chase:  Chase();  break;
            case EnemyState.Attack: AttackState(); break;
        }
    }

    // =========================================
    // BEHAVIOUR
    // =========================================

    void Patrol()
    {
        agent.isStopped = false;

        if (Vector3.Distance(transform.position, patrolTarget) < 0.6f)
        {
            patrolTimer += Time.deltaTime;

            if (patrolTimer >= patrolWaitTime)
            {
                patrolTarget = GetRandomPatrolPoint();
                agent.SetDestination(patrolTarget);
                patrolTimer = 0;
            }
        }
    }

    void Chase()
    {
        agent.isStopped = false;
        agent.SetDestination(player.position);
    }

    void AttackState()
    {
        agent.isStopped = true;
        transform.LookAt(player);
        TryAttack();
    }

    void TryAttack()
    {
        if (Time.time < nextAttackTime) return;

        nextAttackTime = Time.time + attackCooldown;

        if (playerHealth != null)
            playerHealth.TakeDamage(damage);
    }

    // =========================================
    // UTILITY
    // =========================================

    Vector3 GetRandomPatrolPoint()
    {
        Vector2 randomCircle = Random.insideUnitCircle * patrolRadius;
        Vector3 randomPoint = new Vector3(
            transform.position.x + randomCircle.x,
            transform.position.y,
            transform.position.z + randomCircle.y
        );

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, patrolRadius * 2, NavMesh.AllAreas))
            return hit.position;

        return transform.position;
    }

    void FixHeight()
    {
        Vector3 pos = transform.position;
        pos.y = heightOffset;
        transform.position = pos;
    }

    public void UpdateSpeed(float multiplier)
    {
        speedMultiplier = multiplier;
        agent.speed = baseSpeed * speedMultiplier;
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public void ReturnToPool()
    {
        EnemyPool.Instance.ReturnEnemy(gameObject);
    }
}
