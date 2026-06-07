using UnityEngine;

/// <summary>
/// ScriptableObject that holds all config data for a weapon type.
/// Create via: Assets → Create → Weapon → Weapon Data
/// </summary>
[CreateAssetMenu(fileName = "NewWeaponData", menuName = "Weapon/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Identity")]
    public string weaponName = "New Weapon";

    [Header("Stats")]
    public int damage = 25;
    public int magazineSize = 30;
    public int maxMagazineCount = 3;
    public float fireRate = 10f;
    public float reloadTime = 1.5f;
    public float range = 100f;

    [Header("Visual (Placeholder)")]
    public Sprite weaponIcon;
}
