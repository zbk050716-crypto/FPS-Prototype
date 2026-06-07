using System.Collections;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    // =========================================
    // CONFIG (editable directly in Inspector — no ScriptableObject needed)
    // =========================================

    [Header("Identity")]
    public string weaponName = "New Weapon";

    [Header("Stats")]
    public int damage = 25;
    public int magazineSize = 30;
    public int maxMagazineCount = 3;
    public float fireRate = 10f;
    public float reloadTime = 1.5f;
    public float range = 100f;

    [Header("VFX")]
    public GameObject hitEffectPrefab;
    public float hitEffectLifetime = 1f;

    // =========================================
    // RUNTIME STATE
    // =========================================

    public int CurrentAmmo { get; private set; }
    public int ReserveAmmo { get; private set; }
    public bool IsReloading { get; private set; }

    public int MagazineSize  => magazineSize;
    public string WeaponName => weaponName;

    // =========================================
    // REFERENCES (set by WeaponManager)
    // =========================================

    [System.NonSerialized] public Camera fpsCamera;
    [System.NonSerialized] public Transform firePoint;

    // =========================================
    // INTERNAL
    // =========================================

    private float lastFireTime;

    // =========================================
    // LIFECYCLE
    // =========================================

    public void Init()
    {
        CurrentAmmo = magazineSize;
        ReserveAmmo = magazineSize * maxMagazineCount;
        IsReloading = false;
        lastFireTime = 0f;
    }

    // =========================================
    // FIRE
    // =========================================

    public bool CanFire()
    {
        if (fireRate <= 0f) return false;
        if (IsReloading) return false;
        if (CurrentAmmo <= 0) return false;
        if (Time.time - lastFireTime < 1f / fireRate) return false;
        return true;
    }

    public void Fire()
    {
        if (!CanFire()) return;

        CurrentAmmo--;
        lastFireTime = Time.time;

        Ray ray = fpsCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, range))
        {
            IDamageable damageable = hit.collider.GetComponent<IDamageable>();
            if (damageable != null)
                damageable.TakeDamage(damage);

            // ----- Hit VFX -----
            if (hitEffectPrefab != null)
            {
                GameObject fx = Instantiate(hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(fx, hitEffectLifetime);
            }
        }

        SpawnBulletVisual();
    }

    void SpawnBulletVisual()
    {
        if (BulletPool.Instance == null) return;

        Vector3 origin = firePoint != null
            ? firePoint.position
            : fpsCamera.transform.position + fpsCamera.transform.forward * 0.5f;

        Quaternion rot = firePoint != null
            ? firePoint.rotation
            : fpsCamera.transform.rotation;

        GameObject bullet = BulletPool.Instance.GetBullet();
        bullet.transform.position = origin;
        bullet.transform.rotation = rot;

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
            rb.velocity = rot * Vector3.forward * 20f;
    }

    // =========================================
    // RELOAD
    // =========================================

    public IEnumerator ReloadRoutine()
    {
        if (IsReloading) yield break;
        if (ReserveAmmo <= 0) yield break;
        if (CurrentAmmo >= magazineSize) yield break;

        IsReloading = true;
        yield return new WaitForSeconds(reloadTime);

        int needed = magazineSize - CurrentAmmo;
        int refill = Mathf.Min(needed, ReserveAmmo);
        ReserveAmmo -= refill;
        CurrentAmmo += refill;
        IsReloading = false;
    }
}
