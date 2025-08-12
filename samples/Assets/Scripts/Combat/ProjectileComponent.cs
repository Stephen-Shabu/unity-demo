using System.Collections.Generic;
using UnityEngine;

public class ProjectileComponent : MonoBehaviour
{
    [SerializeField] private int projectileCount;
    [SerializeField] private AudioClip imapctSFX;
    [SerializeField] private WeaponDatabase weaponDb;

    private List<Projectible> projectiles = new List<Projectible>();
    private List<ParticleSystem> hitMarkers = new List<ParticleSystem>();
    private List<AudioSource> audioSources = new List<AudioSource>();
    private WeaponSchema activeWeapon;
    private bool isPoolInitialized;
    private float timeSinceLastshot;
    private bool canFire;

    public void ChangeWeapon(WeaponName name)
    {
        if (activeWeapon == null || !activeWeapon.Name.Equals(name))
        {
            OnDestroy();

            projectiles.Clear();
            hitMarkers.Clear();
            audioSources.Clear();

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
                audioSources[bulletIdx].pitch = UnityEngine.Random.Range(0.95f, 1.05f);
                audioSources[bulletIdx].pitch = UnityEngine.Random.Range(0.9f, 1.0f);
                audioSources[bulletIdx].PlayOneShot(activeWeapon.Data.ProjectileSFX);
            }

            timeSinceLastshot = Time.time;
        }

        if (isPoolInitialized)
        {
            for (int i = 0, range = projectiles.Count; i < range; i++)
            {
                projectiles[i].UpdatePosition();
                audioSources[i].transform.position = projectiles[i].GetPosition();
            }
        }
    }

    private void CreateProjectile()
    {
        isPoolInitialized = false;

        for (int i = 0; i < projectileCount; i++)
        {
            var wpnPrjctl = Instantiate(activeWeapon.Data.ProjectilePrefab);
            var wpnHtVfx = Instantiate(activeWeapon.Data.HitVfxPrefab);
            wpnHtVfx.SetActive(false);

            var projectile = wpnPrjctl.GetComponent<Projectible>();
            projectile.Initialise();
            hitMarkers.Add(wpnHtVfx.GetComponent<ParticleSystem>());
            audioSources.Add(new GameObject($"Project {i} audio Source").AddComponent<AudioSource>());

            var index = i;

            projectile.OnProjectileCollided = (prjctl) =>
            {
                hitMarkers[index].gameObject.SetActive(true);
                hitMarkers[index].transform.position = prjctl.GetPosition();
                hitMarkers[index].Play();

                audioSources[index].pitch = UnityEngine.Random.Range(0.95f, 1.05f);
                audioSources[index].pitch = UnityEngine.Random.Range(0.9f, 1.0f);
                audioSources[index].PlayOneShot(imapctSFX);
            };

            projectiles.Add(projectile);
        }

        isPoolInitialized = true;
    }

    private void OnDestroy()
    {
        isPoolInitialized = false;

        foreach (var marker in hitMarkers)
        {
            if(marker != null) Destroy(marker.gameObject);
        }

        foreach (var projectile in projectiles)
        {
            if (projectile != null)  projectile.DestroyProjectile();
        }

        foreach (var source in audioSources)
        {
            if (source != null) Destroy(source.gameObject);
        }
    }
}
