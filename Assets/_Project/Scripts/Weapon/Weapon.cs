using System.Collections;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    // =========================================
    // CONFIG — fallback values (overridden by WeaponData when assigned)
    // =========================================

    [Header("Data Source")]
    [SerializeField] private WeaponData weaponData;

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
    [SerializeField] private GameObject muzzleFlashPrefab;
    public GameObject hitEffectPrefab;
    public float hitEffectLifetime = 1f;

    [Header("Combat")]
    public LayerMask hitLayers = -1;

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
    public Transform firePoint;

    // =========================================
    // INTERNAL
    // =========================================

    private float lastFireTime;
    private ParticleSystem muzzleFlashInstance;

    // =========================================
    // LIFECYCLE
    // =========================================

    void Start()
    {
        if (weaponData != null)
        {
            weaponName = weaponData.weaponName;
            damage = weaponData.damage;
            magazineSize = weaponData.magazineSize;
            maxMagazineCount = weaponData.maxMagazineCount;
            fireRate = weaponData.fireRate;
            reloadTime = weaponData.reloadTime;
            range = weaponData.range;
        }

        Transform muzzle = transform.Find("MuzzlePoint");
        if (muzzle != null) firePoint = muzzle;

        if (muzzleFlashPrefab != null && firePoint != null)
        {
            GameObject go = Instantiate(muzzleFlashPrefab, firePoint);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            muzzleFlashInstance = go.GetComponent<ParticleSystem>();
        }
    }

    public void Init()
    {
        CurrentAmmo = weaponData.magazineSize;
        ReserveAmmo = weaponData.magazineSize * weaponData.maxMagazineCount;
        IsReloading = false;
        lastFireTime = 0f;
    }

    // =========================================
    // FIRE
    // =========================================

    public bool CanFire()
    {
        if (weaponData.fireRate <= 0f) return false;
        if (IsReloading) return false;
        if (CurrentAmmo <= 0) return false;
        if (Time.time - lastFireTime < 1f / weaponData.fireRate) return false;
        return true;
    }

    public void Fire()
    {
        if (!CanFire()) return;

        CurrentAmmo--;
        lastFireTime = Time.time;
        SpawnMuzzleFlash();

        Ray ray = fpsCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        RaycastHit[] hits = Physics.RaycastAll(ray, weaponData != null ? weaponData.range : range, hitLayers);
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.CompareTag("Ground"))
                continue;

            IDamageable damageable = hit.collider.GetComponent<IDamageable>();
            if (damageable == null) continue;

            damageable.TakeDamage(weaponData.damage);

            if (hitEffectPrefab != null)
            {
                GameObject fx = Instantiate(hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(fx, hitEffectLifetime);
            }
            break;
        }

        SpawnBulletVisual();
    }

    void SpawnMuzzleFlash()
    {
        if (muzzleFlashInstance == null) return;

        muzzleFlashInstance.Stop();
        muzzleFlashInstance.Play();
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
        if (CurrentAmmo >= weaponData.magazineSize) yield break;

        IsReloading = true;
        yield return new WaitForSeconds(weaponData.reloadTime);

        int needed = weaponData.magazineSize - CurrentAmmo;
        int refill = Mathf.Min(needed, ReserveAmmo);
        ReserveAmmo -= refill;
        CurrentAmmo += refill;
        IsReloading = false;
    }
}
