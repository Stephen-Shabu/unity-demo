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

                    GameEventsEmitter.EmitEvent(EventType.ChangeWeapon, new WeaponChangeEventData { Type = EventType.ChangeWeapon, Name = name });
                }
            }
        }
    }

    public void Fire(bool canFire)
    {
        this.canFire = canFire;
    }

    public void UpdateComponent()
    {
        if (canFire && Time.time > activeWeapon.Data.FireRate + timeSinceLastshot)
        {
            var bulletIdx = projectiles.FindIndex(x => !x.HasFired());
            var bullet = projectiles[bulletIdx];
            var muzzlePosition = transform.position + (transform.forward * .9f);
            var muzzleForward = muzzlePosition - transform.position;
            muzzleForward.y = 0;

            if (bullet != null)
            {
                bullet.SetProjectile(muzzlePosition, muzzleForward.normalized);
            }

            timeSinceLastshot = Time.time;
        }

        if (isPoolInitialized)
        {
            for (int i = 0, range = projectiles.Count; i < range; i++)
            {
                projectiles[i].UpdatePosition();
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
}
