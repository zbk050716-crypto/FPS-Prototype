using UnityEngine;
using UnityEngine.AI;

public enum EnemyState
{
    Patrol,
    Chase,
    Attack
}

public class EnemyAI : MonoBehaviour
{
    private NavMeshAgent agent;

    [Header("Target")]
    public Transform player;
    private PlayerHealth playerHealth;

    [Header("State")]
    public EnemyState currentState;

    [Header("Distance")]
    public float chaseRange = 10f;
    public float attackRange = 2f;

    [Header("Attack")]
    public int damage = 10;
    public float attackCooldown = 1.5f;
    private float nextAttackTime;

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

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerHealth = playerObj.GetComponent<PlayerHealth>();
        }

        PlayerController pc = playerObj?.GetComponent<PlayerController>();
        baseSpeed = pc != null ? pc.moveSpeed : 5f;

        agent.speed = baseSpeed * speedMultiplier;

        FixHeight();
        InitializePatrol();
    }

    void OnEnable()
    {
        if (agent != null)
        {
            FixHeight();
            InitializePatrol();
        }
    }

    void OnDisable()
    {
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }

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

        float distance = Vector3.Distance(transform.position, player.position);

        UpdateState(distance);
        HandleState();
    }

    // ---------------- STATE SYSTEM ----------------

    void UpdateState(float distance)
    {
        if (distance <= attackRange)
        {
            currentState = EnemyState.Attack;
        }
        else if (distance <= chaseRange)
        {
            currentState = EnemyState.Chase;
        }
        else
        {
            currentState = EnemyState.Patrol;
        }
    }

    void HandleState()
    {
        switch (currentState)
        {
            case EnemyState.Patrol:
                Patrol();
                break;

            case EnemyState.Chase:
                Chase();
                break;

            case EnemyState.Attack:
                AttackState();
                break;
        }
    }

    // ---------------- BEHAVIOUR ----------------

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
        {
            playerHealth.TakeDamage(damage);
        }

        Debug.Log("Enemy Attack Player");
    }

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
        {
            return hit.position;
        }

        return transform.position;
    }

    // ---------------- UTILITY ----------------

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

    public void ReturnToPool()
    {
        EnemyPool.Instance.ReturnEnemy(gameObject);
    }
}