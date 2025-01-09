using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ProjectileComponent : MonoBehaviour
{
    [SerializeField] private GameObject raycastProjectilePrefab;
    [SerializeField] private GameObject hitVfxPrefab;
    [SerializeField] private int projectileCount;
    [SerializeField] private float fireRate;

    private List<RaycastProjectile> projectiles = new List<RaycastProjectile>();
    private List<ParticleSystem> hitMarkers = new List<ParticleSystem>();
    private Stack<RaycastProjectile> raycastProjects;
    private bool isPoolInitialized;
    private float timeSinceLastshot;
    private bool canFire;

    public void Initialise()
    {
        isPoolInitialized = false;

        for (int i = 0; i < projectileCount; i++)
        {
            var instance = Instantiate(raycastProjectilePrefab);
            var hit = Instantiate(hitVfxPrefab);
            hit.SetActive(false);

            var raycastBullet = instance.GetComponent<RaycastProjectile>();
            var hitVfx = hit.GetComponent<ParticleSystem>();
            hitMarkers.Add(hitVfx);

            raycastBullet.Initialise();
            var index = i;
            raycastBullet.ProjectileCollided = (RaycastProjectile rp) =>
            {
                hitMarkers[index].gameObject.SetActive(true);
                hitMarkers[index].transform.position = rp.gameObject.transform.position;
                hitMarkers[index].Play();
            };
            projectiles.Add(raycastBullet);
        }

        isPoolInitialized = true;
    }

    public void Fire(bool canFire)
    {
        this.canFire = canFire;
    }

    public void UpdateComponent()
    {
        if (canFire && Time.time > fireRate + timeSinceLastshot)
        {
            var bullet = projectiles.Find(x => !x.HasFired);
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
}
