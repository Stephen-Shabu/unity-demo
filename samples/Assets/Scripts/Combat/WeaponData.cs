using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileData", menuName = "Game Config/ Weapon Data", order = 2)]
public class WeaponData : ScriptableObject
{
    public GameObject ProjectilePrefab => projectilePrefab;
    public GameObject HitVfxPrefab => hitVfxPrefab;
    public float FireRate => fireRate;
    public AudioClip ProjectileSFX => projectileSFX;

    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private GameObject hitVfxPrefab;
    [SerializeField] private float fireRate;
    [SerializeField] private AudioClip projectileSFX;
}
