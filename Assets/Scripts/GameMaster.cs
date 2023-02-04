using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMaster : MonoBehaviour
{
    public static GameMaster Instance;
    
    [SerializeField] private Rect _spawnArea = new(-8.5f, -5f, 17f, 10f);
    [SerializeField] private Enemy _enemyPrefab;
    [SerializeField] private List<Transform> _spawnLocations;
    
    public bool Running { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(0.4f);
        Running = true;
        SpawnEnemies(1);
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Restart"))
        {
            LifeLost();
        }
    }

    private void SpawnEnemies(int enemyCount)
    {
        Debug.Assert(enemyCount <= _spawnLocations.Count);
        var usedSpawnLocations = new List<Transform>();
        while (enemyCount > 0)
        {
            var randomLocation = _spawnLocations.GetRandom();
            while (usedSpawnLocations.Contains(randomLocation))
            {
                Debug.Break();
                randomLocation = _spawnLocations.GetRandom();
            }
            usedSpawnLocations.Add(randomLocation);
            var enemy = Instantiate(_enemyPrefab, randomLocation.position, Quaternion.identity);
            enemyCount--;
        }
    }

    public void LifeLost()
    {
        if (!Running)
        {
            return;
        }
        Running = false;
        StartCoroutine(ResetLevel());
    }

    private IEnumerator ResetLevel()
    {
        var allTiles = FindObjectsOfType<Tile>();
        var player = FindObjectOfType<EvenMoreSimplePlayerController>();
        player.ResetToDefault();
        foreach (var tile in allTiles)
        {
            tile.ResetToDefault();
            yield return new WaitForSeconds(0.001f);
        }
        var enemies = FindObjectsOfType<Enemy>();
        foreach (var enemy in enemies)
        {
            enemy.Die();
        }
        yield return new WaitForSeconds(0.4f);
        Running = true;
        SpawnEnemies(1);
    }
}