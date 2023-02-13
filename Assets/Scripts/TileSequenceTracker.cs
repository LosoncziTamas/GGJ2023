using System.Collections.Generic;
using UnityEngine;

public class TileSequenceTracker : MonoBehaviour
{
    private readonly List<Tile> _sequence = new List<Tile>();

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
        CaptureSurroundedTiles();
    }

    private void CaptureSurroundedTiles()
    {
        
    }
}