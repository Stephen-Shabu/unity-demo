using System;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileFXComponent : MonoBehaviour
{
    [SerializeField] private ProjectileComponent projectileComponent;

    private List<ParticleSystem> hitMarkers = new List<ParticleSystem>();

    public void Initialise()
    {
        projectileComponent.OnProjectileCollided -= HandleOnProjectileCollided;
        projectileComponent.OnProjectileCollided += HandleOnProjectileCollided;

        projectileComponent.OnProjectileCreated -= HandleOnProjectileCreated;
        projectileComponent.OnProjectileCreated += HandleOnProjectileCreated;

        projectileComponent.OnProjectilePreCreate -= HandleOnProjectilePreCreate;
        projectileComponent.OnProjectilePreCreate += HandleOnProjectilePreCreate;
    }

    private void HandleOnProjectilePreCreate()
    {
        foreach (var marker in hitMarkers)
        {
            if (marker != null) Destroy(marker.gameObject);
        }

        hitMarkers.Clear();
    }

    private void HandleOnProjectileCreated(int projectileCount, GameObject prefab)
    {
        for (int i = 0; i < projectileCount; i++)
        {
            var wpnHtVfx = Instantiate(prefab);
            wpnHtVfx.SetActive(false);
            hitMarkers.Add(wpnHtVfx.GetComponent<ParticleSystem>());
        }
    }

    private void HandleOnProjectileCollided(int index, Projectible prjctl)
    {
        hitMarkers[index].gameObject.SetActive(true);
        hitMarkers[index].transform.position = prjctl.GetPosition();
        hitMarkers[index].Play();
    }
}
