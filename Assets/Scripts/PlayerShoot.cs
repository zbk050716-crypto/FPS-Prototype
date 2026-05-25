using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    [Header("射击设置")]
    public Camera fpsCamera;
    public float shootDistance = 100f;

    [Header("子弹生成点")]
    public float bulletSpeed = 20f;
    public Transform firePoint;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
    }

    void Shoot()
    {
        // 1. 从屏幕中心发射射线（准星位置）
        Ray ray = fpsCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, shootDistance))
        {
            Debug.Log("击中: " + hit.collider.name);

            if (hit.collider.CompareTag("Enemy"))
            {
                Destroy(hit.collider.gameObject);
            }
        }

        // 2. 使用对象池生成子弹（关键修改点）
        GameObject bullet = BulletPool.Instance.GetBullet();

        // 3. 设置子弹位置和方向
        bullet.transform.position = firePoint.position;
        bullet.transform.rotation = firePoint.rotation;

        // 4. 给子弹速度（如果你的Bullet有Rigidbody）
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = firePoint.forward * bulletSpeed;
        }
    }
}