using UnityEngine;
using System.Threading.Tasks;
using System;

public class HealthComponent : MonoBehaviour
{
    public Action OnHealthReachedZero;
    public Action OnDamageRecieved;
    public Action OnDeathComplete;
    public bool IsHealthZero { get; private set; }

    [SerializeField] private Rigidbody attactedRigidBody;
    [SerializeField] private Renderer[] meshes;
    [SerializeField] private GameObject deathVfxPrefab;

    private int currentHealth = 4;
    private float flashAnimTime;
    private float timeToReachTarget = 1f;

    private MaterialPropertyBlock[] propertyBlocks;
    private Color[] cachedColours;
    private const string SHADER_PROP_DISSOLVE = "_DissolveThreshold";
    private const string SHADER_PROP_BASECOLOUR = "_BaseColor";
    private const int FLASH_TIME = 100;
    private ParticleSystem deathBurstVfx;

    public void Initialise()
    {
        deathBurstVfx = Instantiate(deathVfxPrefab).GetComponent<ParticleSystem>();
        deathBurstVfx.gameObject.SetActive(false);

        int meshCount = meshes.Length;
        propertyBlocks = new MaterialPropertyBlock[meshCount];
        cachedColours = new Color[meshCount];

        for (int i = 0; i < meshCount; i++)
        {
            propertyBlocks[i] = new MaterialPropertyBlock();

            meshes[i].GetPropertyBlock(propertyBlocks[i]);
            meshes[i].material.GetColor(SHADER_PROP_BASECOLOUR);

            cachedColours[i] = meshes[i].material.GetColor(SHADER_PROP_BASECOLOUR);
        }
    }

    public void ReactToHit(Vector3 hitDirection)
    {

        if (currentHealth > 0)
            currentHealth -= 1;

        if (currentHealth == 0)
        {
            OnHealthReachedZero?.Invoke();
            attactedRigidBody.linearVelocity += hitDirection * 10f;

            void PlayDeathVFX()
            {
                deathBurstVfx.transform.position = transform.position;
                deathBurstVfx.gameObject.SetActive(true);
                deathBurstVfx.Play();
                attactedRigidBody.gameObject.SetActive(false);
                OnDeathComplete?.Invoke();
            }

            ApplyHitColour(Color.red, PlayDeathVFX);
        }
        else
        {
            OnDamageRecieved?.Invoke();
            ApplyHitColour(Color.white);
            attactedRigidBody.linearVelocity += hitDirection * 5f;
        }
        IsHealthZero = currentHealth == 0;
    }

    private async void FadeOut()
    {
        while (Mathf.Abs(flashAnimTime - timeToReachTarget) > 0.01f)
        {
            flashAnimTime += Time.deltaTime;
            var threshold = 0 - (0 - .6f) - flashAnimTime;

            int length = propertyBlocks.Length;

            for (int i = 0; i < length; i++)
            {
                meshes[i].GetPropertyBlock(propertyBlocks[i]);
                propertyBlocks[i].SetFloat(SHADER_PROP_DISSOLVE, threshold);
                meshes[i].SetPropertyBlock(propertyBlocks[i]);
            }

            await Task.Yield();
        }
        attactedRigidBody.gameObject.SetActive(false);
        OnDeathComplete?.Invoke();
    }

    private async void ApplyHitColour(Color hitColor, Action onComplete = null)
    {
        int length = propertyBlocks.Length;

        for (int i = 0; i < length; i++)
        {
            meshes[i].GetPropertyBlock(propertyBlocks[i]);

            propertyBlocks[i].SetColor(SHADER_PROP_BASECOLOUR, hitColor);
            meshes[i].SetPropertyBlock(propertyBlocks[i]);
        }

        await Task.Delay(FLASH_TIME);

        for (int i = 0; i < length; i++)
        {
            propertyBlocks[i].SetColor(SHADER_PROP_BASECOLOUR, cachedColours[i]);
            meshes[i].SetPropertyBlock(propertyBlocks[i]);
        }

        onComplete?.Invoke();
    }
}
