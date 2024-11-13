using System.Security.Cryptography;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.UIElements;
using System;

public class HealthComponent : MonoBehaviour
{
    public Action OnHealthReachedZero;
    public Action OnDamageRecieved;
    public Action OnDeathComplete;
    public bool IsHealthZero { get; private set; }

    [SerializeField] private Rigidbody attactedRigidBody;
    [SerializeField] private MeshRenderer mesh;

    private int currentHealth = 4;
    private float flashAnimTime;
    private float timeToReachTarget = 1f;

    private MaterialPropertyBlock characterPropertyBlock;
    private const string SHADER_PROP_DISSOLVE = "_DissolveThreshold";
    private const string SHADER_PROP_BASECOLOUR = "_BaseColour";
    private const int FLASH_TIME = 100;

    private void Start()
    {
        characterPropertyBlock = new MaterialPropertyBlock();
    }

    public void ReactToHit(Vector3 hitDirection)
    {

        if (currentHealth > 0)
            currentHealth -= 1;

        if (currentHealth == 0)
        {
            OnHealthReachedZero?.Invoke();
            attactedRigidBody.linearVelocity += hitDirection * 10f;
            UpdateExperienceBarAsync();
            FadeOut();
        }
        else
        {
            OnDamageRecieved?.Invoke();
            UpdateExperienceBarAsync();
            attactedRigidBody.linearVelocity += hitDirection * 5f;
        }
        IsHealthZero = currentHealth == 0;
    }

    private async void FadeOut()
    {
        mesh.GetPropertyBlock(characterPropertyBlock);
        //var maxValue = characterPropertyBlock.GetFloat(SHADER_PROP_DISSOLVE);
        while (Mathf.Abs(flashAnimTime - timeToReachTarget) > 0.01f)
        {
            flashAnimTime += Time.deltaTime;
            var threshold = 0 - (0 - .6f) - flashAnimTime;
            characterPropertyBlock.SetFloat(SHADER_PROP_DISSOLVE, threshold);
            mesh.SetPropertyBlock(characterPropertyBlock);

            await Task.Yield();
        }
        attactedRigidBody.gameObject.SetActive(false);
        OnDeathComplete?.Invoke();
    }

    private async void UpdateExperienceBarAsync()
    {
        mesh.GetPropertyBlock(characterPropertyBlock);

        characterPropertyBlock.SetColor(SHADER_PROP_BASECOLOUR, Color.white);
        mesh.SetPropertyBlock(characterPropertyBlock);

        await Task.Delay(FLASH_TIME);

        characterPropertyBlock.SetColor(SHADER_PROP_BASECOLOUR, Color.gray);
        mesh.SetPropertyBlock(characterPropertyBlock);
    }
}
