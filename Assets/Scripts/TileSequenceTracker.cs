using System.Collections.Generic;
using UnityEngine;

public class TileSequenceTracker : MonoBehaviour
{
    private readonly List<Tile> _sequence = new();

    [SerializeField] private GameObject _singleRootPrefab;
    [SerializeField] private GameObject _doubleRootPrefab;

    private TileManager _tileManager;

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
            Debug.Break();
        }
        foreach (var capturedTile in capturedTiles)
        {
            capturedTile.MarkCaptured();
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
                SpawnDoubleRoot(orientation, currentTile, nextTile);
                i++;
            }
        }
        else
        {
            var firstTile = slipperyTiles[0];
            SpawnSingleRoot(orientation, firstTile);
            for (var i = 1; i < tileCount; i++)
            {
                var currentTile = slipperyTiles[i];
                var nextTile = slipperyTiles[i + 1];
                SpawnDoubleRoot(orientation, currentTile, nextTile);
                i++;
            }
        }
    }

    private void SpawnDoubleRoot(Orientation orientation, Tile currentTile, Tile nextTile)
    {
        if (orientation == Orientation.Horizontal)
        {
            var x = (currentTile.Center.x + nextTile.Center.x) * 0.5f;
            var y = currentTile.Center.y;
            var z = currentTile.Center.z;
            var rootInstance = Instantiate(_doubleRootPrefab);
            rootInstance.transform.position = new Vector3(x, y, z);
            rootInstance.transform.localRotation = Quaternion.Euler(0, 0, 90);
        }
        else
        {
            var x = currentTile.Center.x;
            var y = (currentTile.Center.y + nextTile.Center.y) * 0.5f;
            var z = currentTile.Center.z;
            var rootInstance = Instantiate(_doubleRootPrefab);
            rootInstance.transform.position = new Vector3(x, y, z);
            rootInstance.transform.localRotation = Quaternion.identity;
        }
    }

    private void SpawnSingleRoot(Orientation orientation, Tile currentTile)
    {
        var rootInstance = Instantiate(_singleRootPrefab);
        if (orientation == Orientation.Horizontal)
        {
            rootInstance.transform.position = currentTile.Center;
            rootInstance.transform.localRotation = Quaternion.Euler(0, 0, 90);
        }
        else
        {
            rootInstance.transform.position = currentTile.Center;
            rootInstance.transform.localRotation = Quaternion.identity;
        }
    }

    private bool IsOddNumberOfTiles(List<Tile> tiles)
    {
        return tiles.Count % 2 != 0;
    }

    private bool IsEvenNumberOfTiles(List<Tile> tiles)
    {
        return tiles.Count % 2 == 0;
    }
}