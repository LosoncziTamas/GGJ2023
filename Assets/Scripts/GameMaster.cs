using System;
using System.Collections;
using System.Collections.Generic;
using Configs;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class GameMaster : MonoBehaviour
{
    public static GameMaster Instance;
    
    [SerializeField] private List<LevelConfig> _levels;

    private LevelConfig _currentLevelConfig;
    private PlayerController _player;
    private TileSequenceTracker _tileSequenceTracker;
    private TileManager _tileManager;
    private Camera _camera;
    private Image _redImage;
    
    public bool Running { get; private set; } = true;
    public bool Initializing { get; private set; }

    private void Awake()
    {
        Instance = this;
        _camera = Camera.main;
        _redImage = _camera.GetComponentInChildren<Image>();
        _tileSequenceTracker = FindObjectOfType<TileSequenceTracker>();
        _tileManager = FindObjectOfType<TileManager>();
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
        else
        {
            var startTileCount = _tileManager.StartSlipperyTileCount;
            var nowTileCount = _tileManager.GetTileByType(TileType.Slippery).Count; 
            var scale = 1.0f - ((float)nowTileCount / startTileCount);
            _player.RaiseSpeed(scale);
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
            var enemy = Instantiate(enemyConfig.EnemyPrefab, enemyConfig.SpawnLocation, Quaternion.identity);
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
        _camera.DOShakePosition(0.3f, 0.5f);
        DOTween.Sequence(_redImage.DOFade(0.1f, 0.3f)).Insert(0.3f, _redImage.DOFade(0.0f, 0.3f));
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
            Application.Quit();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine(ClearLevelAndStart(_levels[0]));
        }
    }

    private void MoveToNextLevel()
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
            Debug.Log("No more levels");
            StartCoroutine(ClearLevel());
        }
    }
}