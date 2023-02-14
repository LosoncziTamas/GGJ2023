using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Configs;
using UnityEngine;

public class GameMaster : MonoBehaviour
{
    public static GameMaster Instance;
    
    [SerializeField] private Enemy _enemyPrefab;
    [SerializeField] private List<LevelConfig> _levels;

    private LevelConfig _currentLevelConfig;
    private PlayerController _player;

    private TaskCompletionSource<GameResult> _gameCompletionSource;

    public bool Running { get; private set; } = true;
    public bool Initializing { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private IEnumerator Start()
    {
        _player = FindObjectOfType<PlayerController>();
        yield return new WaitForSeconds(0.3f);
        StartFirstLevel();
    }

    private void StartFirstLevel()
    {
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

    public void LifeLost()
    {
        if (!Running)
        {
            return;
        }
        Running = false;
        _player.Die();
        const float dieDelay = 2.0f;
        StartCoroutine(ClearLevelAndStart(_currentLevelConfig, dieDelay));
    }

    private IEnumerator ClearLevelAndStart(LevelConfig levelConfig, float delay = 0)
    {
        yield return new WaitForSeconds(delay);
        yield return ClearLevel();
        InitLevel(levelConfig);
    }

    private IEnumerator ClearLevel()
    {
        Initializing = true;
        var allTiles = FindObjectsOfType<Tile>();
        _player.ResetToDefault();
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
        Initializing = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            FinishGameWithResult(GameResult.Exited);
        }
    }
    
    public void MoveToNextLevel()
    {
        Running = false;
        var currentLevelIndex = _levels.IndexOf(_currentLevelConfig);
        if (currentLevelIndex < _levels.Count - 1)
        {
            var nextLevel = _levels[currentLevelIndex + 1];
            StartCoroutine(ClearLevelAndStart(nextLevel));
        }
        else
        {
            FinishGameWithResult(GameResult.Completed);
        }
    }

    private void FinishGameWithResult(GameResult gameResult)
    {
        if (_gameCompletionSource.TrySetResult(gameResult))
        {
            StartCoroutine(ClearLevel());
        }
        else
        {
            Debug.LogError("Game already finished.");
        }
    }
}