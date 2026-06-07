using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float lifeTime = 3f;

    void OnEnable()
    {
        // 3秒后自动回收（防止卡死）
        Invoke("ReturnToPool", lifeTime);
    }

    void OnDisable()
    {
        CancelInvoke();
    }

    void ReturnToPool()
    {
        BulletPool.Instance.ReturnBullet(gameObject);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            EnemyAI enemy = collision.gameObject.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                enemy.ReturnToPool();
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.OnEnemyKilled();
                }
            }
        }

        ReturnToPool();
    }
}
