using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SimplifiedPlayerController : MonoBehaviour
{
    private const string TileTag = "Tile";
    
    [SerializeField, Range(0f, 1f)] private float _bounciness = 0.5f;
    [SerializeField, Range(0f, 100f)] private float _maxSpeed = 10f;
    
    private Rect _allowedArea = new(-8.5f, -5f, 17f, 10f);

    private Tile _closestTile;
    private Tile _lastWalkableTile;
    private bool _sliding;
    private List<Tile> _allTiles;
    private PolygonBuilder _polygonBuilder;
    
    private void Start()
    {
        _allTiles = new List<Tile>(FindObjectsOfType<Tile>(includeInactive: true));
        _polygonBuilder = FindObjectOfType<PolygonBuilder>();
    }

    private void Update()
    {
        var newPosition = VelocityBasedMovement();
        if (!_allowedArea.Contains(new Vector2(newPosition.x, newPosition.z)))
        {
            if (newPosition.x < _allowedArea.xMin) 
            {
                newPosition.x = _allowedArea.xMin;
            }
            else if (newPosition.x > _allowedArea.xMax) 
            {
                newPosition.x = _allowedArea.xMax;
            }
            if (newPosition.z < _allowedArea.yMin) 
            {
                newPosition.z = _allowedArea.yMin;
            }
            else if (newPosition.z > _allowedArea.yMax) 
            {
                newPosition.z = _allowedArea.yMax;
            }
        }
        transform.localPosition = newPosition;
    }
    
    private Vector3 VelocityBasedMovement()
    {
        var playerInput = Vector2.zero;
        playerInput.x = Input.GetAxis("Horizontal");
        playerInput.y = Input.GetAxis("Vertical");
        playerInput = Vector2.ClampMagnitude(playerInput, 1f);
        var velocity = new Vector3(playerInput.x, 0f, playerInput.y) * _maxSpeed;
        var desiredDisplacement = velocity * Time.deltaTime;
        return transform.localPosition + desiredDisplacement;
    }

    private void OnTriggerEnter(Collider other)
    {
        EvaluateTrigger(other);
    }

    private void OnTriggerStay(Collider other)
    {
        EvaluateTrigger(other);
    }
    
    private void EvaluateTrigger(Collider other)
    {
        if (!other.CompareTag(TileTag))
        {
            return;
        }
        var newTile = other.GetComponent<Tile>();
        if (_closestTile == null)
        {
            InitializeClosestTile(newTile);
        }
        else if (newTile != _closestTile)
        {
            UpdateClosestTile(newTile);
        }
    }

    private void OnGUI()
    {
        GUILayout.Label("_sliding " + _sliding);
        if (_closestTile != null)
        {
            GUILayout.Label("_closestTile " + _closestTile.transform.position);
        }
        if (_lastWalkableTile != null)
        {
            GUILayout.Label("_lastWalkableTile " + _lastWalkableTile.transform.position);
        }

        if (GUILayout.Button("Reset"))
        {
            foreach (var tile in _allTiles)
            {
                tile.SetMarkEnabled(false);
            }
        }
        if (GUILayout.Button("Build polygon"))
        {
            _polygonBuilder.Build(new List<Vector3> {Vector3.zero});
        }
    }

    private void InitializeClosestTile(Tile newTile)
    {
        _closestTile = newTile;
        if (newTile.TileType == TileType.Walkable)
        {
            _sliding = false;
            _lastWalkableTile = newTile;
        }
        _closestTile.SetHighlightEnabled(true);
    }

    private void UpdateClosestTile(Tile newTile)
    {
        var selfPos = transform.position;
        var oldDistance = Vector3.Distance(_closestTile.transform.position, selfPos);
        var newDistance = Vector3.Distance(newTile.transform.position, selfPos);
        if (newDistance < oldDistance)
        {
            _closestTile.SetHighlightEnabled(false);
            var lastTileWasWalkable = _closestTile.TileType == TileType.Walkable;
            _closestTile = newTile;
            if (newTile.TileType == TileType.Walkable)
            {
                if (_sliding)
                {
                    var start = _lastWalkableTile.transform.position;
                    var end = _lastWalkableTile.transform.position;
                    _polygonBuilder.Build(new List<Vector3> {start, end});
                    Debug.Log("Slice between " + start + " and " + end);
                    newTile.SetMarkEnabled(true);
                }
                _sliding = false;
                _lastWalkableTile = newTile;
            }
            else if (newTile.TileType == TileType.Slippery && lastTileWasWalkable)
            {
                _sliding = true;
                _lastWalkableTile.SetMarkEnabled(true);
            }
            _closestTile.SetHighlightEnabled(true);
        }
    }
    

    
    private List<Tile> GetTilesInArea(Rect rect)
    {
        var result = new List<Tile>();
        foreach (var tile in _allTiles)
        {
            if (rect.Contains(tile.transform.position))
            {
                result.Add(tile);
            }
        }
        return result;
    }
}