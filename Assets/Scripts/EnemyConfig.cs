using UnityEngine;

[CreateAssetMenu]
public class EnemyConfig : ScriptableObject
{
    [Range(0f, 100f)] public float EnemyMaxSpeed = 2.3f;
    public Vector3 SpawnLocation;
    public Vector2 StartDirection;
    
}