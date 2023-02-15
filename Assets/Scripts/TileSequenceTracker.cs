using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class TileSequenceTracker : MonoBehaviour
{
    private readonly List<Tile> _sequence = new();

    [SerializeField] private GameObject _singleRootPrefab;
    [SerializeField] private GameObject _doubleRootPrefab;

    private TileManager _tileManager;
    public event Action OnTilesCaptured;

    private List<GameObject> _roots = new();

    private void Awake()
    {
        _tileManager = FindObjectOfType<TileManager>();
    }

    public bool TileIsInSequence(Tile tile)
    {
        return tile.TileType == TileType.Slippery && _sequence.Contains(tile);
    }
    
    public void BeginTracking(Tile startTile, Tile nextTile)
    {
        _sequence.Clear();
        _sequence.Add(startTile);
        _sequence.Add(nextTile);
    }
    
    public void Add(Tile tile)
    {
        _sequence.Add(tile);
    }

    public void EndTracking(Tile endTile)
    {
        _sequence.Add(endTile);
        SpawnRootsAndCaptureSurroundedTiles();
    }
    
    private void SpawnRootsAndCaptureSurroundedTiles()
    {
        var sequenceLength = _sequence.Count;
        var slipperyTiles = _sequence.GetRange(1, sequenceLength - 1);
        var startTile = _sequence[0];
        var endTile = _sequence[sequenceLength - 1];
        var isHorizontal = startTile.Coordinates.y == endTile.Coordinates.y;
        var isVertical = startTile.Coordinates.x == endTile.Coordinates.x;
        var capturedTiles = default(List<Tile>);
        if (isHorizontal)
        {
            SpawnRoots(Orientation.Horizontal, slipperyTiles);
            capturedTiles = DetermineCapturedTiles(Orientation.Horizontal, startTile.Coordinates.y);
        }
        else if (isVertical)
        {
            SpawnRoots(Orientation.Vertical, slipperyTiles);
            capturedTiles = DetermineCapturedTiles(Orientation.Vertical, startTile.Coordinates.x);
        }
        else
        {
            Debug.LogError($"Neither horizontal or vertical, start: {startTile} end: {endTile}.");
            Debug.Break();
        }
        foreach (var capturedTile in capturedTiles)
        {
            capturedTile.MarkCaptured();
        }
        if (capturedTiles.Count > 0)
        {
            OnTilesCaptured?.Invoke();
        }
    }

    private List<Tile> DetermineCapturedTiles(Orientation orientation, int start)
    {
        var slipperyTiles = _tileManager.GetTileByType(TileType.Slippery);
        if (orientation == Orientation.Horizontal)
        {
            var startY = start;
            var maxY = TileManager.RowCount;
            var minY = 0;
            var tilesAbove = new List<Tile>();
            var tilesBelow = new List<Tile>();
            foreach (var slipperyTile in slipperyTiles)
            {
                var y = slipperyTile.Coordinates.y;
                if (y >= startY && y < maxY)
                {
                    tilesAbove.Add(slipperyTile);
                }
                if (y <= startY && y > minY)
                {
                    tilesBelow.Add(slipperyTile);
                }
            }
            return tilesBelow.Count > tilesAbove.Count ? tilesAbove : tilesBelow;
        }
        else
        {
            var startX = start;
            var maxX = TileManager.ColumnCount;
            var minX = 0;
            var tilesBefore = new List<Tile>();
            var tilesAfter = new List<Tile>();
            foreach (var slipperyTile in slipperyTiles)
            {
                var x = slipperyTile.Coordinates.x;
                if (x >= startX && x < maxX)
                {
                    tilesAfter.Add(slipperyTile);
                }
                if (x <= startX && x > minX)
                {
                    tilesBefore.Add(slipperyTile);
                }
            }
            return tilesBefore.Count > tilesAfter.Count ? tilesAfter : tilesBefore;
        }
    }

    private void SpawnRoots(Orientation orientation, List<Tile> slipperyTiles)
    {
        var tileCount = slipperyTiles.Count;
        if (IsEvenNumberOfTiles(slipperyTiles))
        {
            for (var i = 0; i < tileCount; i++)
            {
                var currentTile = slipperyTiles[i];
                var nextTile = slipperyTiles[i + 1];
                _roots.Add(SpawnDoubleRoot(orientation, currentTile, nextTile));
                i++;
            }
        }
        else
        {
            var firstTile = slipperyTiles[0];
            _roots.Add(SpawnSingleRoot(orientation, firstTile));
            for (var i = 1; i < tileCount; i++)
            {
                var currentTile = slipperyTiles[i];
                var nextTile = slipperyTiles[i + 1];
                _roots.Add(SpawnDoubleRoot(orientation, currentTile, nextTile));
                i++;
            }
        }
        PlayerAnimationAndAudioController.Instance.PlayImpactSound();
    }

    private GameObject SpawnDoubleRoot(Orientation orientation, Tile currentTile, Tile nextTile)
    {
        var randomZOffset = Random.Range(0.1f, 0.4f);
        const float startZOffset = 1.0f;
        GameObject rootInstance;
        if (orientation == Orientation.Horizontal)
        {
            var x = (currentTile.Center.x + nextTile.Center.x) * 0.5f;
            var y = currentTile.Center.y;
            var z = currentTile.Center.z - randomZOffset;
            var startZ = currentTile.Center.z + startZOffset;
            rootInstance = Instantiate(_doubleRootPrefab);
            rootInstance.transform.position = new Vector3(x, y, startZ);
            rootInstance.transform.localRotation = Quaternion.Euler(0, 0, Random.Range(70, 100));
            CreateRootSpawnAnim(rootInstance, z);
        }
        else
        {
            var x = currentTile.Center.x;
            var y = (currentTile.Center.y + nextTile.Center.y) * 0.5f;
            var z = currentTile.Center.z - randomZOffset;
            var startZ = currentTile.Center.z + startZOffset;
            rootInstance = Instantiate(_doubleRootPrefab);
            rootInstance.transform.position = new Vector3(x, y, startZ);
            rootInstance.transform.localRotation = Quaternion.Euler(0, 0, Random.Range(-20, 10));
            CreateRootSpawnAnim(rootInstance, z);
        }
        return rootInstance;
    }

    private Sequence CreateRootSpawnAnim(GameObject rootInstance, float z)
    {
        return DOTween.Sequence(rootInstance.transform.DOMoveZ(z, 0.3f).SetEase(Ease.InOutBack))
            .Insert(0.15f, rootInstance.transform.DOPunchScale(Vector3.one, 0.3f));
    }

    private GameObject SpawnSingleRoot(Orientation orientation, Tile currentTile)
    {
        var randomZOffset = Random.Range(0.1f, 0.4f);
        const float startZOffset = 1.0f;
        var rootInstance = Instantiate(_singleRootPrefab);
        if (orientation == Orientation.Horizontal)
        {
            var startZ = currentTile.Center.z + startZOffset;
            rootInstance.transform.position = new Vector3(currentTile.Center.x, currentTile.Center.y, startZ);
            rootInstance.transform.localRotation = Quaternion.Euler(0, 0, Random.Range(70, 100));
            CreateRootSpawnAnim(rootInstance, currentTile.Center.z - randomZOffset);
        }
        else
        {
            var startZ = currentTile.Center.z + startZOffset;
            rootInstance.transform.position = new Vector3(currentTile.Center.x, currentTile.Center.y, startZ);
            rootInstance.transform.localRotation = Quaternion.Euler(0, 0, Random.Range(-20, 10));
            CreateRootSpawnAnim(rootInstance, currentTile.Center.z - randomZOffset);
        }
        return rootInstance;
    }

    public void DestroyRoots()
    {
        foreach (var root in _roots)
        {
            CreateRootSpawnAnim(root, root.transform.position.z + 1.0f);
            Destroy(root, 0.6f);
        }
        _roots.Clear();
    }

    private bool IsEvenNumberOfTiles(List<Tile> tiles)
    {
        return tiles.Count % 2 == 0;
    }
}