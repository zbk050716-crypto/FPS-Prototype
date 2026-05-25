using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    private NavMeshAgent agent;

    private Transform player;

    private PlayerHealth playerHealth;

    public float attackDistance = 2f;

    public int damage = 10;

    public float attackCooldown = 1f;

    private float nextAttackTime;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        player = GameObject.FindGameObjectWithTag("Player").transform;

        playerHealth = player.GetComponent<PlayerHealth>();
    }

    void Update()
    {
        if (player == null) return;

        float distance =
            Vector3.Distance(transform.position, player.position);

        if (distance > attackDistance)
        {
            agent.SetDestination(player.position);
        }
        else
        {
            Attack();
        }
    }

    void Attack()
    {
        if (Time.time >= nextAttackTime)
        {
            nextAttackTime =
                Time.time + attackCooldown;

            playerHealth.TakeDamage(damage);

            Debug.Log("敌人攻击玩家");
        }
    }
}
