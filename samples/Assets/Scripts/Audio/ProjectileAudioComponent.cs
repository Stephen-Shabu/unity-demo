using System;
using UnityEngine;
using UnityEngine.Timeline;

public class ProjectileAudioComponent : MonoBehaviour
{
    [SerializeField] private AudioProfile audioProfile;

    private Projectible projectible;
    private AudioSource audioSource;

    private void Awake()
    {
        projectible = GetComponent<Projectible>();

        if (projectible != null)
        {
            projectible.OnCreated -= HandleOnProjectileCreated;
            projectible.OnCreated += HandleOnProjectileCreated;
            projectible.OnFired -= Projectible_OnProjectileFired;
            projectible.OnFired += Projectible_OnProjectileFired;
            projectible.OnCollided -= HandleOnCollided;
            projectible.OnCollided += HandleOnCollided;
        }
        else
        {
            Debug.LogWarning("No Projectile found on this GameObject.");
        }
    }

    private void Projectible_OnProjectileFired(Vector3 startPos, Vector3 direction)
    {
        audioSource.pitch = UnityEngine.Random.Range(0.95f, 1.05f);
        audioSource.pitch = UnityEngine.Random.Range(0.9f, 1.0f);

        AudioClip[] clips = audioProfile.GetAudioByType(AudioType.Projectile);

        if (clips != null && clips.Length > 0)
        {
            audioSource.PlayOneShot(clips[UnityEngine.Random.Range(0, clips.Length)]);
        }
    }

    private void HandleOnProjectileCreated(WeaponData data)
    {
        audioSource = new GameObject($"{data.name} projectile audio Source").AddComponent<AudioSource>();
    }

    private void HandleOnCollided(Projectible projectible)
    {
        audioSource.pitch = UnityEngine.Random.Range(0.95f, 1.05f);
        audioSource.pitch = UnityEngine.Random.Range(0.9f, 1.0f);

        AudioClip[] clips = audioProfile.GetAudioByType(AudioType.Hit);

        if (clips != null && clips.Length > 0)
        {
            audioSource.PlayOneShot(clips[UnityEngine.Random.Range(0, clips.Length)]);
        }
    }

    private void OnDestroy()
    {
        if (audioSource != null)
        {
            projectible.OnCreated -= HandleOnProjectileCreated;
            projectible.OnFired -= Projectible_OnProjectileFired;
            projectible.OnCollided -= HandleOnCollided;

            Destroy(audioSource.gameObject);
        }
    }
}
