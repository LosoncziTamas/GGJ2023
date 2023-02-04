using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EvenMoreSimplePlayerController : MonoBehaviour
{
    private const string TileTag = "Tile";

    [SerializeField, Range(0f, 100f)] private float _maxSpeed = 10f;

    private Rect _allowedArea = new(-8.5f, -5f, 17f, 10f);
    private bool _isMoving;    
    private Tile _targetTile;
    private Tile _lastWalkableTile;
    private PolygonBuilder _polygonBuilder;
    private List<Tile> _allTiles;
    private bool IsSliding => _slidingVelocity.HasValue;
    private Vector3? _slidingVelocity;
    private Vector3 _velocity;
    
    private void Start()
    {
        _allTiles = new List<Tile>(FindObjectsOfType<Tile>(includeInactive: true));
        _polygonBuilder = FindObjectOfType<PolygonBuilder>();
    }

    private void OnGUI()
    {
        GUILayout.Label("_velocity " + _velocity);
        GUILayout.Label("_lastPlayerInput " + _lastPlayerInput);
    }

    private void Update()
    {
        if (_isMoving)
        {
            return;
        }
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

    private Vector2 _lastPlayerInput;
    
    private Vector3 VelocityBasedMovement()
    {
        var playerInput = Vector2.zero;
        playerInput.x = Input.GetAxis("Horizontal");
        playerInput.y = Input.GetAxis("Vertical");
        playerInput = Vector2.ClampMagnitude(playerInput, 1f);
        var xDelta = Mathf.Abs(playerInput.x);
        var yDelta = Mathf.Abs(playerInput.y);
        if (xDelta > yDelta)
        {
            playerInput = new Vector2(playerInput.x, 0);
        }
        else if (yDelta > xDelta)
        {
            playerInput = new Vector2(0, playerInput.y);
        }
        else
        {
            playerInput = _lastPlayerInput;
        }
        _lastPlayerInput = playerInput;
        _velocity = new Vector3(playerInput.x, 0f, playerInput.y) * _maxSpeed;
        var desiredDisplacement = _velocity * Time.deltaTime;
        return transform.localPosition + desiredDisplacement;
    }

    private IEnumerator MoveToTile(Tile targetTile)
    {
        if (_isMoving)
        {
            yield break;
        }
        _isMoving = true;
        var targetPosition = targetTile.transform.position;
        while (true)
        {
            var distance = Vector3.Distance(targetPosition, transform.position);
            if (distance < 0.01f)
            {
                transform.position = targetPosition;
                _isMoving = false;
                yield break;
            }
            var newPos = Vector3.MoveTowards(transform.position, targetPosition, _maxSpeed * Time.deltaTime);
            transform.position = newPos;
            yield return null;
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
        if (_isMoving)
        {
            return;
        }
        if (!other.CompareTag(TileTag))
        {
            return;
        }
        var newTile = other.GetComponent<Tile>();
        if (_targetTile == null)
        {
            InitializeTargetTile(newTile);
        }        
        else if (newTile != _targetTile)
        {
            UpdateClosestTile(newTile);
        }
    }

    private void InitializeTargetTile(Tile newTile)
    {
        _targetTile = newTile;
        if (newTile.TileType == TileType.Walkable)
        {
            _slidingVelocity = null;
            _lastWalkableTile = newTile;
        }
        _targetTile.SetHighlightEnabled(true);
        StartCoroutine(MoveToTile(_targetTile));
    }
    
    private void UpdateClosestTile(Tile newTile)
    {
        var selfPos = transform.position;
        var oldTile = _targetTile;
        var oldDistance = Vector3.Distance(oldTile.transform.position, selfPos);
        var newDistance = Vector3.Distance(newTile.transform.position, selfPos);
        if (newDistance < oldDistance)
        {
            var lastTileWasWalkable = oldTile.TileType == TileType.Walkable;
            if (newTile.TileType == TileType.Walkable && IsSliding)
            {
                EndSliding(newTile);
            }            
            else if (newTile.TileType == TileType.Slippery && lastTileWasWalkable)
            {
                BeginSliding();
            }
            InitializeTargetTile(newTile);
            oldTile.SetHighlightEnabled(false);
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