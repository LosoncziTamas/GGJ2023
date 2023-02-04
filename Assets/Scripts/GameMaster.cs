using System.Collections.Generic;
using UnityEngine;

public class GameMaster : MonoBehaviour
{
    [SerializeField] private Rect _spawnArea = new(-8.5f, -5f, 17f, 10f);
    [SerializeField] private Enemy _enemyPrefab;
    [SerializeField] private List<Transform> _spawnLocations;

    private void SpawnEnemies()
    {
        var randomLocation = _spawnLocations.GetRandom();
        var enemy = Instantiate(_enemyPrefab, randomLocation.position, Quaternion.identity);
    }

}