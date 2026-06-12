using UnityEngine;

// DISABLED: This was a legacy shooting system that conflicted with WeaponManager.
// Both scripts fired on mouse click, causing PlayerShoot to bypass the health system
// by calling ReturnToPool() directly, preventing damage and hit effects from working.
// Shooting is now handled by WeaponManager -> Weapon.
public class PlayerShoot : MonoBehaviour
{
}
