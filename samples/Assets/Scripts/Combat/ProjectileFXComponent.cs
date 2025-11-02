using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Timeline;

public class ProjectileFXComponent : MonoBehaviour
{
    private Projectible projectible;
    private ParticleSystem hitMarker;

    private void Awake()
    {
        projectible = GetComponent<Projectible>();

        if (projectible != null)
        {
            projectible.OnCreated -= HandleOnProjectileCreated;
            projectible.OnCreated += HandleOnProjectileCreated;
            projectible.OnCollided -= HandleOnCollided;
            projectible.OnCollided += HandleOnCollided;
        }
        else
        {
            Debug.LogWarning("No Projectile found on this GameObject.");
        }
    }

    private void HandleOnProjectileCreated(WeaponData data)
    {
        hitMarker = Instantiate(data.HitVfxPrefab).GetComponent<ParticleSystem>();
        var main = hitMarker.main;
        main.playOnAwake = false;

        hitMarker.gameObject.SetActive(false);
    }

    private void HandleOnCollided(Projectible projectible)
    {
        hitMarker.gameObject.SetActive(true);
        hitMarker.transform.position = projectible.GetPosition();
        hitMarker.Play();
    }

    private void OnDestroy()
    {
        if (hitMarker != null)
        {
            projectible.OnCreated -= HandleOnProjectileCreated;
            projectible.OnCollided -= HandleOnCollided;

            Destroy(hitMarker.gameObject);
        }
    }
}
