using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class WeaponManager : MonoBehaviour
{
    public static WeaponManager Instance { get; private set; }

    // =========================================
    // CONFIG (modified: weapons must be assigned manually in Inspector)
    // weapons[0] = Rifle, weapons[1] = Pistol, weapons[2] = Shotgun
    // =========================================

    [Header("Weapon List (assign in order: 0=Rifle 1=Pistol 2=Shotgun)")]
    [SerializeField] private Weapon[] weapons;

    [Header("References")]
    [SerializeField] private Camera fpsCamera;
    [SerializeField] private Transform firePoint;

    // =========================================
    // RUNTIME STATE
    // =========================================

    public Weapon CurrentWeapon { get; private set; }
    public int CurrentWeaponIndex { get; private set; } = -1;

    public System.Action<Weapon> OnWeaponSwitched;

    // =========================================
    // LIFECYCLE
    // =========================================

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Debug.Log($"[WeaponManager] Start — weapons array length: {(weapons != null ? weapons.Length.ToString() : "null")}");

        if (weapons == null || weapons.Length == 0)
        {
            Debug.LogWarning("[WeaponManager] No weapons assigned in Inspector, auto-scanning WeaponHolder...");
            AutoPopulateWeapons();
        }

        if (weapons == null || weapons.Length == 0)
        {
            Debug.LogError("[WeaponManager] Still no weapons found! Check WeaponHolder under Player.");
            return;
        }

        foreach (var w in weapons)
        {
            if (w == null) { Debug.LogWarning("[WeaponManager] Skipping null weapon entry"); continue; }
            Debug.Log($"[WeaponManager] Init + deactivate: {w.name}");
            w.fpsCamera = fpsCamera;
            w.firePoint = firePoint;
            w.Init();
            w.gameObject.SetActive(false);
        }

        if (weapons.Length > 0)
        {
            Debug.Log($"[WeaponManager] Switching to weapon 0: {weapons[0].name}");
            SwitchWeapon(0);
        }
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
            return;

        if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchWeapon(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchWeapon(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchWeapon(2);

        if (CurrentWeapon == null) return;

        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            CurrentWeapon.Fire();

        if (Input.GetKeyDown(KeyCode.R))
            StartCoroutine(CurrentWeapon.ReloadRoutine());
    }

    // =========================================
    // SWITCH WEAPON
    // =========================================

    public void SwitchWeapon(int index)
    {
        if (weapons == null || index < 0 || index >= weapons.Length) return;
        if (weapons[index] == null) return;
        if (index == CurrentWeaponIndex) return;

        foreach (var w in weapons)
        {
            if (w != null) w.gameObject.SetActive(false);
        }

        CurrentWeapon = weapons[index];
        CurrentWeaponIndex = index;
        CurrentWeapon.gameObject.SetActive(true);
        Debug.Log($"[WeaponManager] Active weapon: {CurrentWeapon.name}");

        OnWeaponSwitched?.Invoke(CurrentWeapon);
    }

    void AutoPopulateWeapons()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        Transform holder = player.transform.Find("WeaponHolder");
        if (holder == null) return;

        var list = new System.Collections.Generic.List<Weapon>();
        foreach (Transform child in holder)
        {
            Weapon w = child.GetComponent<Weapon>();
            if (w != null) list.Add(w);
        }
        weapons = list.ToArray();
        Debug.Log($"[WeaponManager] Auto-populated {weapons.Length} weapons from WeaponHolder");
    }
}
