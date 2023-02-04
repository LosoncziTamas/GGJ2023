using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMaster : MonoBehaviour
{
    [SerializeField] private Rect _spawnArea = new(-8.5f, -5f, 17f, 10f);
    [SerializeField] private Enemy _enemyPrefab;
    [SerializeField] private List<Transform> _spawnLocations;

    private readonly List<Transform> _usedLocations = new();

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(0.4f);
        SpawnEnemies(1);
    }

    private void SpawnEnemies(int enemyCount)
    {
        Debug.Assert(enemyCount <= _spawnLocations.Count);
        while (enemyCount > 0)
        {
            var randomLocation = _spawnLocations.GetRandom();
            while (_usedLocations.Contains(randomLocation))
            {
                randomLocation = _spawnLocations.GetRandom();
            }
            _usedLocations.Add(randomLocation);
            var enemy = Instantiate(_enemyPrefab, randomLocation.position, Quaternion.identity);
            enemyCount--;
        }
    }
}