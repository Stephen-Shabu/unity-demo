using System.Collections;
using UnityEngine;

public class HealthFXComponent : MonoBehaviour
{
    [SerializeField] private Renderer[] meshes;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private HealthComponent health;
    [SerializeField] private GameObject deathVfxPrefab;
    [SerializeField] private float deathImpulse = 10f;
    [SerializeField] private float hitImpulse = 5f;

    private MaterialPropertyBlock[] propertyBlocks;
    private Color[] cachedColours;
    private ParticleSystem deathBurstVfx;
    private IEnumerator hitFlashRoutine;
    private IEnumerator vfxRoutine;

    private static readonly int SHADER_PROP_BASECOLOUR = Shader.PropertyToID("_BaseColor");
    private static readonly int SHADER_PROP_DISSOLVE = Shader.PropertyToID("_DissolveThreshold");

    private YieldInstruction hitFlashYield = new WaitForSeconds(MovementDefines.Character.HIT_FLASH_DURATION);
    private YieldInstruction endOfFrameYield = new WaitForEndOfFrame();

    public void Initialise()
    {
        health.OnDamageTaken += HandleDamageTaken;
        health.OnDeathStarted += HandleDeathStarted;
        health.OnDeathFinished += HandleDeathFinished;

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

    private void HandleDamageTaken(Vector3 hitDir, int newHealth)
    {
        if(hitFlashRoutine != null) StopCoroutine(hitFlashRoutine);

        hitFlashRoutine = ApplyHitFlash(Color.white);
        StartCoroutine(hitFlashRoutine);
    }

    private void HandleDeathStarted(Vector3 hitDir)
    {
        if (vfxRoutine != null) StopCoroutine(vfxRoutine);

        vfxRoutine = PlayDeathVFX();
        StartCoroutine(vfxRoutine);
    }

    private IEnumerator PlayDeathVFX()
    {
        yield return hitFlashYield;

        deathBurstVfx.transform.position = transform.position;
        deathBurstVfx.gameObject.SetActive(true);
        deathBurstVfx.Play();
        health.NotifyDeathComplete();
    }

    private void HandleDeathFinished() { }

    private IEnumerator FadeOutMeshes()
    {
        float flashAnimTime = 0;
        float timeToReachTarget = 1f;

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

            yield return endOfFrameYield;
        }
    }

    private IEnumerator ApplyHitFlash(Color hitColor)
    {
        int length = meshes.Length;
        for (int i = 0; i < length; i++)
        {
            meshes[i].GetPropertyBlock(propertyBlocks[i]);
            propertyBlocks[i].SetColor(SHADER_PROP_BASECOLOUR, hitColor);
            meshes[i].SetPropertyBlock(propertyBlocks[i]);
        }

        yield return hitFlashYield;

        for (int i = 0; i < length; i++)
        {
            propertyBlocks[i].SetColor(SHADER_PROP_BASECOLOUR, cachedColours[i]);
            meshes[i].SetPropertyBlock(propertyBlocks[i]);
        }
    }
}
