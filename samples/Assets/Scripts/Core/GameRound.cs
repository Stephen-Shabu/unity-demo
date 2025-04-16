using UnityEngine;

[CreateAssetMenu(fileName = "GameRoundData", menuName = "Game Config/ Round Data", order = 1)]
public class GameRound : ScriptableObject
{
    public int NumberOfEnemies;
    public float RoundMaxTime;
    public GameObject enemyType;
}
