using UnityEngine;

public class WeaponController : MonoBehaviour
{
    // =========================================
    // PROPERTIES (delegated to WeaponManager)
    // =========================================

    public Weapon CurrentWeapon
    {
        get
        {
            if (WeaponManager.Instance != null)
                return WeaponManager.Instance.CurrentWeapon;
            return null;
        }
    }

    public int CurrentAmmo
    {
        get
        {
            if (CurrentWeapon != null)
                return CurrentWeapon.CurrentAmmo;
            return 0;
        }
    }

    // modified: ReserveAmmo replaces CurrentMagazineCount
    public int ReserveAmmo
    {
        get
        {
            if (CurrentWeapon != null)
                return CurrentWeapon.ReserveAmmo;
            return 0;
        }
    }

    public bool IsReloading
    {
        get
        {
            if (CurrentWeapon != null)
                return CurrentWeapon.IsReloading;
            return false;
        }
    }

    public string WeaponName
    {
        get
        {
            if (CurrentWeapon != null)
                return CurrentWeapon.WeaponName;
            return "";
        }
    }

    public int MagazineSize
    {
        get
        {
            if (CurrentWeapon != null)
                return CurrentWeapon.MagazineSize;
            return 0;
        }
    }

    // =========================================
    // METHODS
    // =========================================

    public void SwitchWeapon(int index)
    {
        if (WeaponManager.Instance != null)
            WeaponManager.Instance.SwitchWeapon(index);
    }
}
