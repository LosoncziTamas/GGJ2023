using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMaster : MonoBehaviour
{
    public static GameMaster Instance;
    
    [SerializeField] private Rect _spawnArea = new(-8.5f, -5f, 17f, 10f);
    [SerializeField] private Enemy _enemyPrefab;
    [SerializeField] private List<LevelConfig> _levels;

    private LevelConfig _currentLevelConfig;
    
    public bool Running { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(0.4f);
        Debug.Assert(_levels is {Count: > 0});
        InitLevel(_levels[0]);
    }

    private void InitLevel(LevelConfig levelConfig)
    {
        _currentLevelConfig = levelConfig;
        Running = true;
        foreach (var enemyConfig in levelConfig.Enemies)
        {
            var enemy = Instantiate(_enemyPrefab, enemyConfig.SpawnLocation, Quaternion.identity);
            enemy.Init(enemyConfig);
        }
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Restart"))
        {
            LifeLost();
        }
    }

    public void LifeLost()
    {
        if (!Running)
        {
            return;
        }
        Running = false;
        StartCoroutine(ClearLevelAndStart(_currentLevelConfig));
    }

    private IEnumerator ClearLevelAndStart(LevelConfig levelConfig)
    {
        var allTiles = FindObjectsOfType<Tile>();
        var player = FindObjectOfType<PlayerController>();
        player.ResetToDefault();
        var enemies = FindObjectsOfType<Enemy>();
        foreach (var enemy in enemies)
        {
            enemy.Die();
        }
        foreach (var tile in allTiles)
        {
            tile.ResetToDefault();
            yield return new WaitForSeconds(0.001f);
        }
        InitLevel(levelConfig);
    }

    public void MoveToNextLevel()
    {
        Running = false;
        var currentLevelIndex = _levels.IndexOf(_currentLevelConfig);
        if (currentLevelIndex < _levels.Count)
        {
            var nextLevel = _levels[currentLevelIndex + 1];
            StartCoroutine(ClearLevelAndStart(nextLevel));
        }
        else
        {
            Debug.Log("Congratulations!");
            // TODO: Congrats panel!
        }
    }
}