using System.Collections.Generic;
using UnityEngine;

public class SimplifiedPlayerController : MonoBehaviour
{
    private const string TileTag = "Tile";
    
    [SerializeField, Range(0f, 1f)] private float _bounciness = 0.5f;
    [SerializeField, Range(0f, 100f)] private float _maxSpeed = 10f;
    
    private Rect _allowedArea = new(-8.5f, -5f, 17f, 10f);
    private Tile _closestTile;
    private Tile _lastWalkableTile;
    private List<Tile> _allTiles;
    private PolygonBuilder _polygonBuilder;
    private Vector3 _velocity;
    private Vector3? _slidingVelocity;

    private bool IsSliding => _slidingVelocity.HasValue;
    
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
        if (_slidingVelocity.HasValue)
        {
            HandleSlidingMovement(playerInput, _slidingVelocity.Value);
        }
        else
        {
            _velocity = new Vector3(playerInput.x, 0f, playerInput.y) * _maxSpeed;
        }
        var desiredDisplacement = _velocity * Time.deltaTime;
        return transform.localPosition + desiredDisplacement;
    }

    private void HandleSlidingMovement(Vector2 playerInput, Vector3 velocity)
    {
        if (Mathf.Approximately(playerInput.magnitude, 0.0f))
        {
            _velocity = velocity;
        }
        else
        {
            var directionChanged = false;
            var slidingLeft = velocity.x < 0;
            var slidingRight = velocity.x > 0;
            var includerOffset = new Vector3();
            if ((slidingLeft || slidingRight) && playerInput.y > 0)
            {
                _velocity = new Vector3(0, 0f, 1.0f) * _maxSpeed;
                includerOffset = new Vector3(0, 0, -0.5f);
                directionChanged = true;
            }
            else if ((slidingLeft || slidingRight) && playerInput.y < 0)
            {
                _velocity = new Vector3(0, 0f, -1.0f) * _maxSpeed;
                includerOffset = new Vector3(0, 0, 0.5f);
                directionChanged = true;
            }
            var slidingUp = velocity.z > 0;
            var slidingDown = velocity.z < 0;
            if ((slidingUp || slidingDown) && playerInput.x > 0)
            {
                _velocity = new Vector3(1.0f,0.0f, 0.0f) * _maxSpeed;
                includerOffset = new Vector3(-0.5f, 0.5f, 0.0f);
                directionChanged = true;
            }
            else if ((slidingUp || slidingDown) && playerInput.x < 0)
            {
                _velocity = new Vector3(-1.0f, 0f, 0.0f) * _maxSpeed;
                includerOffset = new Vector3(0.5f, 0, 0.0f);
                directionChanged = true;
            }
            if (directionChanged)
            {
                _polygonBuilder.Add(_closestTile.transform.position + includerOffset);
                _slidingVelocity = _velocity;
            }
        }
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
        GUILayout.Label("_sliding " + IsSliding);
        GUILayout.Label("_slidingVelocity " + _slidingVelocity);
        
        if (GUILayout.Button("Clear"))
        {
            _polygonBuilder.Clear();
        }
        if (GUILayout.Button("Add"))
        {
            _polygonBuilder.Add(transform.position);
        }
        if (GUILayout.Button("Inside"))
        {
            var result = _polygonBuilder.IsInside(transform.position);
            Debug.Log("inside: " + result);
        }
    }

    private void InitializeClosestTile(Tile newTile)
    {
        _closestTile = newTile;
        if (newTile.TileType == TileType.Walkable)
        {
            _slidingVelocity = null;
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
                if (IsSliding)
                {
                    EndSliding(newTile);
                }
                _slidingVelocity = null;
                _lastWalkableTile = newTile;
            }
            else if (newTile.TileType == TileType.Slippery && lastTileWasWalkable)
            {
                BeginSliding();
            }
            _closestTile.SetHighlightEnabled(true);
        }
    }

    private void BeginSliding()
    {
        _slidingVelocity = _velocity.normalized * _maxSpeed;
        _polygonBuilder.Clear();
        _polygonBuilder.Add(_lastWalkableTile.transform.position);
    }

    private void EndSliding(Tile newTile)
    {
        _polygonBuilder.Add(newTile.transform.position);
        _polygonBuilder.Build();
        var tilesToFlip = GetTilesInPolygon();
        foreach (var tile in tilesToFlip)
        {
            tile.MarkResolved();
        }
        // _polygonBuilder.Clear();
    }
    
    private List<Tile> GetTilesInPolygon()
    {
        var result = new List<Tile>();
        foreach (var tile in _allTiles)
        {
            if (_polygonBuilder.IsInside(tile.transform.position))
            {
                result.Add(tile);
            }
        }
        return result;
    }
}