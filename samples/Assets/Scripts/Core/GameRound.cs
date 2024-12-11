using UnityEngine;

[CreateAssetMenu(fileName = "GameRoundConfig", menuName = "Game Config", order = 1)]
public class GameRound : ScriptableObject
{
    public int NumberOfEnemies;
    public float RoundMaxTime;
    public GameObject enemyType;
}
