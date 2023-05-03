using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New Weapon Preset", menuName = "Weapons/New Weapon Preset")]
public class WeaponPreset : ScriptableObject
{
    [Header("Generic Weapon Stats")]
    [Space(2)]

    [Header("Name")]
    [SerializeField] private string weaponName;
    [Space]

    [Header("Normal Bullet")]
    [SerializeField] private int damage;
    [SerializeField] private float initialBulletSize = 1f;
    [SerializeField] private float bulletDistance;
    [SerializeField] private float bulletSpeed;

    [Header("Direct Hit Bullet")]
    [SerializeField] private int dh_damage;
    [SerializeField] private float dh_initialBulletSize = 1f;
    [SerializeField] private float dh_bulletDistance;
    [SerializeField] private float dh_bulletSpeed;

    public enum FireMode
    {
        Automatic,
        SemiAutomatic
    }
    [Space]
    [Header("Fire Mode")]
    [SerializeField] private FireMode fireMode;
    [Space]

    [Header("Fire Rate")]
    [SerializeField][Range(0f, 20f)] private float firePerSecond;

    [Space(5)]
    [Header("Firearm Stats")]
    [Space(2)]

    [Header("Ammo")]
    [SerializeField] private int maxMagazineAmmo;
    [Space]

    [Header("Bullets")]
    [SerializeField][Range(1, 20)] private int projectilesPerShoot = 1;
    [Space]

    [Header("Reload")]
    [SerializeField] private float timeToFullyReload;

    public enum ReloadMode
    {
        PerMagazine,
        PerBullet
    }
    [Space]

    [Header("Reload Mode")]
    [SerializeField] private ReloadMode reloadMode;
    [Space]

    [Header("Recoil")]
    [SerializeField][Range(0f, 180f)] private float minFiringAngle;
    [SerializeField][Range(0f, 180f)] private float maxFiringAngle;
    [SerializeField][Range(0f, 180f)] private float angleIncrementPerShoot;
    [SerializeField][Range(0f, 180f)] private float angleDecrement;

    #region Property Accesors
    //Generic Weapon Stats
    public string WeaponName { get { return weaponName; } }
    public float FirePerSecond { get { return firePerSecond; } }
    public FireMode WeaponFireMode { get { return fireMode; } }

    //Ammo
    public int MaxMagazineAmmo { get { return maxMagazineAmmo; } }

    //Normal Bullets
    public int Damage { get { return damage; } }
    public float InitialBulletSize { get { return initialBulletSize; } }
    public float BulletDistance { get { return bulletDistance; } }
    public float BulletSpeed { get { return bulletSpeed; } }

    //Direct Hit Bullets
    public int DH_Damage { get { return dh_damage; } }
    public float DH_InitialBulletSize { get { return dh_initialBulletSize; } }
    public float DH_BulletDistance { get { return dh_bulletDistance; } }
    public float DH_BulletSpeed { get { return dh_bulletSpeed; } }

    //Projectiles
    public int ProjectilesPerShoot { get { return projectilesPerShoot; } }

    //Reload
    public float TimeToFullyReload { get { return timeToFullyReload; } }

    //Reload Mode
    public ReloadMode WeaponReloadMode { get { return reloadMode; } }

    //Recoil
    public float MinFiringAngle { get { return minFiringAngle; } }
    public float MaxFiringAngle { get { return maxFiringAngle; } }
    public float AngleIncrementPerShoot { get { return angleIncrementPerShoot; } }
    public float AngleDecrement { get { return angleDecrement; } }
    #endregion
}
