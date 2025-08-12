using System.Collections.Generic;
using UnityEngine;

public enum WeaponName
{
    BLASTER,
    WAVE_BEAM,
    POWER_BOMB,
    ICE_MISSLES,
    CHARGE_BLASTER
}

[System.Serializable]
public class WeaponSchema
{
    public WeaponName Name => name;
    public WeaponData Data => data;

    [SerializeField] private WeaponName name;
    [SerializeField] private WeaponData data;
}

[CreateAssetMenu(fileName = "WeaponDatabase", menuName = "Game Config/ Weapon Database", order = 3)]
public class WeaponDatabase : ScriptableObject
{
    public Dictionary<WeaponName, WeaponSchema> Weapons => weaponDatabase;

    [SerializeField] private List<WeaponSchema> weapons = new List<WeaponSchema>();

    private Dictionary<WeaponName, WeaponSchema> weaponDatabase = new Dictionary<WeaponName, WeaponSchema>();

    public void Initialise()
    {
        for (int i = 0; i < weapons.Count; i++)
        {
            weaponDatabase.Add(weapons[i].Name, weapons[i]);
        }
    }
}
