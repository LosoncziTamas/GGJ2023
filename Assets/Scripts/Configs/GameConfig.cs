using UnityEngine;

namespace Configs
{
    [CreateAssetMenu]
    public class GameConfig : ScriptableObject
    {
        [Range(0, 1)] public float TimeBetweenTrailSpawn = 0.02f;
        [Range(0f, 100f)] public float EnemyMaxSpeed = 10f;
        [Range(0f, 100f)] public float PlayerMaxSpeed = 10f;
    }
}