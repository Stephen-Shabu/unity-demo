using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ProjectileComponent : MonoBehaviour
{
    [SerializeField] private GameObject raycastProjectilePrefab;
    [SerializeField] private int projectileCount;
    [SerializeField] private float fireRate;

    private List<RaycastProjectile> projectiles = new List<RaycastProjectile>();
    private Stack<RaycastProjectile> raycastProjects;
    private bool isPoolInitialized;
    private float timeSinceLastshot;

    private void Start()
    {
        isPoolInitialized = false;
        for (int i = 0; i < projectileCount; i++)
        {
            var instance = Instantiate(raycastProjectilePrefab);
            var raycastBullet = instance.GetComponent<RaycastProjectile>();
            raycastBullet.Initialise();
            projectiles.Add(raycastBullet);
        }

        isPoolInitialized = true;
    }

    public void Fire()
    {
        if (Time.time > fireRate + timeSinceLastshot)
        {
            var bullet = projectiles.Find(x => !x.HasFired);
            var muzzlePosition = transform.position + (transform.forward * 2f);
            var muzzleForward = muzzlePosition - transform.position;
            muzzleForward.y = 0;
            if (bullet != null)
            {
                bullet.SetProjectile(muzzlePosition, muzzleForward.normalized);
            }

            timeSinceLastshot = Time.time;
        }
    }

    public void FixedUpdate()
    {
        if (isPoolInitialized)
        {
            for (int i = 0, range = projectiles.Count; i < range; i++)
            {
                projectiles[i].UpdatePosition();
            }
        }
    }
}
