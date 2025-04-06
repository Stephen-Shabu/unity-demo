using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static Unity.VisualScripting.Member;

public class ProjectileComponent : MonoBehaviour
{
    [SerializeField] private GameObject raycastProjectilePrefab;
    [SerializeField] private GameObject hitVfxPrefab;
    [SerializeField] private int projectileCount;
    [SerializeField] private float fireRate;
    [SerializeField] private AudioClip projectileSFX;
    [SerializeField] private AudioClip imapctSFX;

    private List<RaycastProjectile> projectiles = new List<RaycastProjectile>();
    private List<ParticleSystem> hitMarkers = new List<ParticleSystem>();
    private List<AudioSource> audioSources = new List<AudioSource>();
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
            audioSources.Add(new GameObject($"Project {i} audio Source").AddComponent<AudioSource>());

            raycastBullet.Initialise();
            var index = i;
            raycastBullet.ProjectileCollided = (RaycastProjectile rp) =>
            {
                hitMarkers[index].gameObject.SetActive(true);
                hitMarkers[index].transform.position = rp.gameObject.transform.position;
                hitMarkers[index].Play();

                audioSources[index].pitch = UnityEngine.Random.Range(0.95f, 1.05f);
                audioSources[index].pitch = UnityEngine.Random.Range(0.9f, 1.0f);
                audioSources[index].PlayOneShot(imapctSFX);
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
            var bulletIdx = projectiles.FindIndex(x => !x.HasFired);
            var bullet = projectiles[bulletIdx];
            var muzzlePosition = transform.position + (transform.forward * .9f);
            var muzzleForward = muzzlePosition - transform.position;
            muzzleForward.y = 0;

            if (bullet != null)
            {
                bullet.SetProjectile(muzzlePosition, muzzleForward.normalized);
                audioSources[bulletIdx].pitch = UnityEngine.Random.Range(0.95f, 1.05f);
                audioSources[bulletIdx].pitch = UnityEngine.Random.Range(0.9f, 1.0f);
                audioSources[bulletIdx].PlayOneShot(projectileSFX);
            }

            timeSinceLastshot = Time.time;
        }

        if (isPoolInitialized)
        {
            for (int i = 0, range = projectiles.Count; i < range; i++)
            {
                projectiles[i].UpdatePosition();
                audioSources[i].transform.position = projectiles[i].transform.position;
            }
        }
    }

    private void OnDestroy()
    {
        foreach (var marker in hitMarkers)
        {
            Destroy(marker.gameObject);
        }

        foreach (var projectile in projectiles)
        {
            Destroy(projectile.gameObject);
        }

        foreach (var source in audioSources)
        {
            Destroy(source.gameObject);
        }
    }
}
