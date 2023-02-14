using System.Collections;
using System.Collections.Generic;
using Configs;
using UnityEngine;

public class GameMaster : MonoBehaviour
{
    public static GameMaster Instance;
    
    [SerializeField] private Enemy _enemyPrefab;
    [SerializeField] private List<LevelConfig> _levels;

    private LevelConfig _currentLevelConfig;
    private PlayerController _player;
    private TileSequenceTracker _tileSequenceTracker;
    
    public bool Running { get; private set; } = true;
    public bool Initializing { get; private set; }

    private void Awake()
    {
        Instance = this;
        _tileSequenceTracker = FindObjectOfType<TileSequenceTracker>();
    }

    private void OnEnable()
    {
        _tileSequenceTracker.OnTilesCaptured += OnTilesCaptured;
    }

    private void OnDisable()
    {
        _tileSequenceTracker.OnTilesCaptured -= OnTilesCaptured;
    }
    
    private IEnumerator Start()
    {
        _player = FindObjectOfType<PlayerController>();
        yield return new WaitForSeconds(0.3f);
        StartFirstLevel();
    }

    private void OnTilesCaptured()
    {
        StartCoroutine(CheckCompletion());
    }

    private IEnumerator CheckCompletion()
    {
        const float rootSpawnAnimLength = 1.0f;
        yield return new WaitForSeconds(rootSpawnAnimLength);
        var enemies = Enemy.Instances;
        var levelCompleted = true;
        foreach (var enemy in enemies)
        {
            if (!enemy.Idle)
            {
                levelCompleted = false;
                break;
            }
        }
        if (levelCompleted)
        {
            MoveToNextLevel();
        }
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
        StartCoroutine(ClearLevelAndStart(_currentLevelConfig, dieDelay, resetPlayer: true));
    }

    private IEnumerator ClearLevelAndStart(LevelConfig levelConfig, float delay = 0, bool resetPlayer = true)
    {
        yield return new WaitForSeconds(delay);
        yield return ClearLevel(resetPlayer);
        InitLevel(levelConfig);
    }

    private IEnumerator ClearLevel(bool resetPlayer)
    {
        Initializing = true;
        var allTiles = FindObjectsOfType<Tile>();
        if (resetPlayer)
        {
            _player.ResetToDefault();
        }
        _tileSequenceTracker.DestroyRoots();
        var enemies = Enemy.Instances;
        foreach (var enemy in enemies)
        {
            enemy.ClearFromLevel();
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
            // TODO: modal or exit
        }
    }

    private void MoveToNextLevel()
    {
        Running = false;
        var currentLevelIndex = _levels.IndexOf(_currentLevelConfig);
        if (currentLevelIndex < _levels.Count - 1)
        {
            var nextLevel = _levels[currentLevelIndex + 1];
            StartCoroutine(ClearLevelAndStart(nextLevel, resetPlayer: false));
        }
        else
        {
            Debug.Log("No more levels");
            StartCoroutine(ClearLevel(resetPlayer: false));
        }
    }
}