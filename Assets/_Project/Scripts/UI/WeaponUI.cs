using UnityEngine;
using TMPro;

public class WeaponUI : MonoBehaviour
{
    [Header("UI Text References")]
    [SerializeField] private TMP_Text weaponNameText;
    [SerializeField] private TMP_Text ammoText;
    [SerializeField] private TMP_Text magText;
    [SerializeField] private TMP_Text reloadingText;

    void Start()
    {
        if (weaponNameText == null) Debug.LogWarning("[WeaponUI] Weapon Name Text is not assigned!");
        if (ammoText == null) Debug.LogWarning("[WeaponUI] Ammo Text is not assigned!");
        if (magText == null) Debug.LogWarning("[WeaponUI] Mag Text is not assigned!");
        if (reloadingText == null) Debug.LogWarning("[WeaponUI] Reloading Text is not assigned!");

        if (WeaponManager.Instance != null)
            WeaponManager.Instance.OnWeaponSwitched += OnWeaponSwitched;

        RefreshUI(null);

        if (WeaponManager.Instance != null && WeaponManager.Instance.CurrentWeapon != null)
            OnWeaponSwitched(WeaponManager.Instance.CurrentWeapon);
    }

    void OnDestroy()
    {
        if (WeaponManager.Instance != null)
            WeaponManager.Instance.OnWeaponSwitched -= OnWeaponSwitched;
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing)
            RefreshUI(WeaponManager.Instance != null ? WeaponManager.Instance.CurrentWeapon : null);
        else
            ClearUI();
    }

    void OnWeaponSwitched(Weapon weapon)
    {
        RefreshUI(weapon);
    }

    void RefreshUI(Weapon weapon)
    {
        if (weapon == null)
        {
            ClearUI();
            return;
        }

        if (weaponNameText != null)
            weaponNameText.text = weapon.WeaponName;

        if (ammoText != null)
            ammoText.text = $"{weapon.CurrentAmmo} / {weapon.MagazineSize}";

        if (magText != null)
            magText.text = $" {weapon.ReserveAmmo}";

        if (reloadingText != null)
            reloadingText.text = weapon.IsReloading ? "Reloading..." : "";
    }

    void ClearUI()
    {
        if (weaponNameText != null) weaponNameText.text = "";
        if (ammoText != null) ammoText.text = "";
        if (magText != null) magText.text = "";
        if (reloadingText != null) reloadingText.text = "";
    }
}
