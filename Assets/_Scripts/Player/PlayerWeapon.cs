using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{

    [SerializeField] private WeaponPreset defaultWeapon;
    [SerializeField] private WeaponPreset actualWeapon;

    [SerializeField] private Transform virtualMouse;
    [SerializeField] private LayerMask enemyLayer;

    private int maxAmmo;
    private int currentAmmo;

    private bool isShooting;
    private float nextFireTime;

    private float currentFiringAngle;

    private float nextTimeToReload;
    private bool isReloading;

    //private void OnEnable()
    //{
    //    InputManager.OnFirePerformed += StartShooting;
    //    InputManager.OnFireCanceled += StopShooting;

    //    InputManager.OnReloadPerformed += StartReload;
    //}

    //private void OnDisable()
    //{
    //    InputManager.OnFirePerformed -= StartShooting;
    //    InputManager.OnFireCanceled -= StopShooting;

    //    InputManager.OnReloadPerformed -= StartReload;
    //}

    private void Start()
    {
        SetNewWeapon(defaultWeapon);
    }

    private void Update()
    {
        if (isShooting && !isReloading) Shoot();
        if (isReloading) Reload();
        PassiveDecreaseFiringAngle();
    }

    private void StartShooting()
    {
        if (isReloading) return;
        switch(actualWeapon.WeaponFireMode)
        {
            case WeaponPreset.FireMode.Automatic:
                isShooting = true;
                break;
            case WeaponPreset.FireMode.SemiAutomatic:
                Shoot();
                break;
        }
    }

    private void StopShooting()
    {
        isShooting = false;
    }

    private void Shoot()
    {
        if (currentAmmo <= 0) return;
        if (Time.time < nextFireTime) return;

        float bulletSizeDecrement = actualWeapon.InitialBulletSize / actualWeapon.BulletDistance;

        //Check if is direct hit
        bool isDH = false;
        float distance = Vector2.Distance(transform.position, virtualMouse.position);
        if(distance <= actualWeapon.BulletDistance)
        {
            //Possible direct hit
            Collider2D col = Physics2D.OverlapCircle(virtualMouse.position, distance * bulletSizeDecrement, enemyLayer);
            isDH = (col != null);
        }

        foreach(float angle in GetFiringAngles())
        {
            Vector3 startPos = transform.position;
            Vector3 endPos = startPos + Quaternion.Euler(0, 0, angle) * transform.right * actualWeapon.BulletDistance;

            //Instatiate bullet
            GameObject bullet = BulletPool.Instance.GetBullet();
            if(isDH) //Is Direct Hit
            {
                bulletSizeDecrement = actualWeapon.DH_InitialBulletSize / actualWeapon.DH_BulletDistance;
                bullet.GetComponent<Bullet>().ShootBullet(startPos, endPos, actualWeapon.DH_BulletSpeed, actualWeapon.DH_Damage, bulletSizeDecrement, Team.TeamA, true);
            }
            else //Isn't Direct Hit
            {
                bullet.GetComponent<Bullet>().ShootBullet(startPos, endPos, actualWeapon.BulletSpeed, actualWeapon.Damage, bulletSizeDecrement, Team.TeamA, false);
            }
            bullet.SetActive(true);
        }

        currentAmmo--;

        IncreaseFiringAngleByShoot();
        nextFireTime = Time.time + (1 / actualWeapon.FirePerSecond);
    }

    private List<float> GetFiringAngles()
    {
        List<float> firingAngles = new List<float>();

        for(int i = 0; i < actualWeapon.ProjectilesPerShoot; i++)
        {
            firingAngles.Add(Random.Range(0, currentFiringAngle) - (currentFiringAngle / 2));
        }

        return firingAngles;
    }

    private void IncreaseFiringAngleByShoot()
    {
        currentFiringAngle += actualWeapon.AngleIncrementPerShoot;
        currentFiringAngle = Mathf.Clamp(currentFiringAngle, actualWeapon.MinFiringAngle, actualWeapon.MaxFiringAngle);
    }

    private void PassiveDecreaseFiringAngle()
    {
        currentFiringAngle -= actualWeapon.AngleDecrement * Time.deltaTime;
        currentFiringAngle = Mathf.Clamp(currentFiringAngle, actualWeapon.MinFiringAngle, actualWeapon.MaxFiringAngle);
    }

    private void StartReload()
    {
        isReloading = true;
        nextTimeToReload = actualWeapon.TimeToFullyReload + Time.time;
    }

    private void StopReload()
    {
        isReloading = false;
    }

    private void Reload()
    {
        if (Time.time < nextTimeToReload) return;
        switch(actualWeapon.WeaponReloadMode)
        {
            case WeaponPreset.ReloadMode.PerMagazine:
                currentAmmo = maxAmmo;
                StopReload();
                break;
            case WeaponPreset.ReloadMode.PerBullet:
                currentAmmo += 1;
                if(currentAmmo >= maxAmmo)
                {
                    currentAmmo = maxAmmo;
                    StopReload();
                }
                else nextTimeToReload = actualWeapon.TimeToFullyReload + Time.time;
                break;
        }
    }

    private void SetNewWeapon(WeaponPreset newWeapon)
    {
        if (actualWeapon == newWeapon) return;
        if (isShooting) StopShooting();
        if (isReloading) StopReload();
        
        actualWeapon = newWeapon;
        maxAmmo = newWeapon.MaxMagazineAmmo;
        currentAmmo = maxAmmo;
    }
}
