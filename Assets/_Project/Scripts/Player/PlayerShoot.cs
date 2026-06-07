using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerShoot : MonoBehaviour
{
    [Header("射击设置")]
    public Camera fpsCamera;
    public float shootDistance = 100f;

    [Header("子弹生成点")]
    public float bulletSpeed = 20f;
    public Transform firePoint;

    [Header("瞄准线设置")]
    public bool showAimRay = true;
    public Color rayColor = Color.red;
    public float rayDuration = 0.1f;

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
            return;

        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            Shoot();
        }

        DrawAimRay();
    }

    void DrawAimRay()
    {
        if (!showAimRay) return;

        Ray ray = fpsCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Debug.DrawRay(ray.origin, ray.direction * shootDistance, rayColor, rayDuration);
    }

    void Shoot()
    {
        Ray ray = fpsCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, shootDistance))
        {
            if (hit.collider.CompareTag("Enemy"))
            {
                EnemyAI enemy = hit.collider.GetComponent<EnemyAI>();
                if (enemy != null)
                {
                    Debug.Log("击中敌人");
                    enemy.ReturnToPool();
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.OnEnemyKilled();
                    }
                }
            }
        }

        GameObject bullet = BulletPool.Instance.GetBullet();

        bullet.transform.position = firePoint.position;
        bullet.transform.rotation = firePoint.rotation;

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = firePoint.forward * bulletSpeed;
        }
    }
}