using UnityEngine;

public class AudioComponent : MonoBehaviour
{
    public AudioProfile AudioProfile => profile;

    [SerializeField] private AudioProfile profile; 

    private AudioSource source;

    public void Initialise()
    {
        if (TryGetComponent(out AudioSource src))
        {
            source = src;
        }
        else
        {
            source = new GameObject($"{name} Audio Source").AddComponent<AudioSource>();
        }

        source.spatialBlend = 1f;
        source.playOnAwake = false;
    }

    public void PlayAudio(float volume, float pitchRange, params AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0) return;

        if (clips.Length > 1)
        {
            source.pitch = 1f + Random.Range(-pitchRange, pitchRange);
            source.PlayOneShot(clips[0], volume);
        }
        else
        {
            source.pitch = 1f + Random.Range(-pitchRange, pitchRange);
            source.PlayOneShot(clips[Random.Range(0, clips.Length)], volume);
        }
    }
}
