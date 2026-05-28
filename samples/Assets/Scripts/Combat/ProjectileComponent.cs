using System.Collections.Generic;
using UnityEngine;

public class ProjectileComponent : MonoBehaviour
{
    [SerializeField] private int projectileCount;
    [SerializeField] private WeaponDatabase weaponDb;

    private List<Projectible> projectiles = new List<Projectible>();
    private WeaponSchema activeWeapon;
    private bool isPoolInitialized;
    private float timeSinceLastshot;
    private bool canFire;
    private int currentProjectileIndex;
    private Chargeable chargeableProjectile;

    public void ChangeWeapon(WeaponName name)
    {
        if (activeWeapon == null || !activeWeapon.Name.Equals(name))
        {
            foreach (var projectile in projectiles)
            {
                if (projectile != null)
                {
                    Destroy(projectile.GetGameObject());
                }
            }

            projectiles.Clear();

            if (weaponDb.Weapons.ContainsKey(name))
            {
                if (weaponDb.Weapons.TryGetValue(name, out WeaponSchema data))
                {
                    activeWeapon = data;

                    CreateProjectile();

                    GameEventsEmitter.EmitEvent(EventType.ChangeWeapon, new WeaponChangeEventData { Type = EventType.ChangeWeapon, Name = name, Schema = activeWeapon });
                }
            }
        }
    }

    private void CreateProjectile()
    {
        isPoolInitialized = false;

        for (int i = 0; i < projectileCount; i++)
        {
            var wpnPrjctl = Instantiate(activeWeapon.Data.ProjectilePrefab);

            var projectile = wpnPrjctl.GetComponent<Projectible>();
            projectile.Initialise(activeWeapon.Data);

            projectiles.Add(projectile);
        }

        isPoolInitialized = true;
    }

    public int SetProjectileIndex()
    {
        var bulletIdx = projectiles.FindIndex(x => !x.HasFired());
        currentProjectileIndex = bulletIdx;
        return bulletIdx;
    }

    public void StartCharge()
    {
        var bullet = projectiles[currentProjectileIndex];

        if (bullet != null)
        {
            chargeableProjectile = bullet as Chargeable;
            if (chargeableProjectile != null)
            {
                var muzzlePosition = transform.position + (transform.forward * .9f);
                var muzzleForward = muzzlePosition - transform.position;
                chargeableProjectile.StartCharge(muzzlePosition, muzzleForward.normalized);
            }
        }
    }

    public void Fire(bool canFire)
    {
        this.canFire = canFire;

        if (canFire && Time.time > activeWeapon.Data.FireRate + timeSinceLastshot)
        {
            if (currentProjectileIndex > -1)
            {
                var bullet = projectiles[currentProjectileIndex];
                var muzzlePosition = transform.position + (transform.forward * .9f);
                var muzzleForward = muzzlePosition - transform.position;
                muzzleForward.y = 0;

                if (bullet != null)
                {
                    bullet.SetProjectile(muzzlePosition, muzzleForward.normalized);
                }
            }

            timeSinceLastshot = Time.time;
            chargeableProjectile = null;
        }
    }

    public void UpdateComponent()
    {
        if (isPoolInitialized)
        {
            for (int i = 0, range = projectiles.Count; i < range; i++)
            {
                projectiles[i].UpdatePosition();
            }
        }
    }

    public void UpdateComponent(float chargeTimer)
    {
        if (isPoolInitialized)
        {
            for (int i = 0, range = projectiles.Count; i < range; i++)
            {
                if (chargeableProjectile != null)
                {
                    var muzzlePosition = transform.position + (transform.forward * .9f);
                    var muzzleForward = muzzlePosition - transform.position;
                    chargeableProjectile.UpdateCharge(chargeTimer, muzzlePosition, muzzleForward.normalized);
                }

                projectiles[i].UpdatePosition();
            }
        }
    }
}
