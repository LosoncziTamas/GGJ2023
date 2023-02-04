using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField, Range(0f, 100f)] private float _maxSpeed = 10f;
    [SerializeField] private Trail _trail;

    private Rect _allowedArea = new(-8.5f, -5f, 17f, 10f);
    private bool _isMoving;    
    private Tile _targetTile;
    private Tile _lastWalkableTile;
    private PolygonBuilder _polygonBuilder;
    private List<Tile> _allTiles;
    private Vector3? _slidingVelocity;
    private Vector3 _velocity;
    private Vector2 _lastPlayerInput;
    private Vector3 _startPosition;
    
    private bool IsSliding => _slidingVelocity.HasValue;

    public void ResetToDefault()
    {
        _targetTile = null;
        _lastWalkableTile = null;
        _slidingVelocity = null;
        _velocity = Vector3.zero;
        _lastPlayerInput = Vector2.zero;
        transform.position = _startPosition;
        _isMoving = false;
        _trail.enabled = false;
        StopAllCoroutines();
    }
    
    private void Start()
    {
        _allTiles = new List<Tile>(FindObjectsOfType<Tile>(includeInactive: true));
        _polygonBuilder = FindObjectOfType<PolygonBuilder>();
        _startPosition = transform.position;
    }

#if false
    private void OnGUI()
    {
        GUILayout.Label("_velocity " + _velocity);
        GUILayout.Label("_lastWalkableTile " + _lastWalkableTile.transform.position);
        GUILayout.Label("_targetTile " + _targetTile.transform.position);
        GUILayout.Label("_isMoving " + _isMoving);
    }
#endif

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
    
    private Vector3 VelocityBasedMovement()
    {
        var playerInput = ReadInput();
        _lastPlayerInput = playerInput;
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
            // To make sure tiles are included in the collider.
            // TODO: uncomment for more advanced control
            var includerOffset = new Vector3();
            if ((slidingLeft || slidingRight) && playerInput.y > 0)
            {
                // _velocity = new Vector3(0, 0f, 1.0f) * _maxSpeed;
                includerOffset = new Vector3(0, 0, -0.5f);
                // directionChanged = true;
            }
            else if ((slidingLeft || slidingRight) && playerInput.y < 0)
            {
                // _velocity = new Vector3(0, 0f, -1.0f) * _maxSpeed;
                includerOffset = new Vector3(0, 0, 0.5f);
                // directionChanged = true;
            }
            var slidingUp = velocity.z > 0;
            var slidingDown = velocity.z < 0;
            if ((slidingUp || slidingDown) && playerInput.x > 0)
            {
                // _velocity = new Vector3(1.0f,0.0f, 0.0f) * _maxSpeed;
                includerOffset = new Vector3(-0.5f, 0.5f, 0.0f);
                // directionChanged = true;
            }
            else if ((slidingUp || slidingDown) && playerInput.x < 0)
            {
                // _velocity = new Vector3(-1.0f, 0f, 0.0f) * _maxSpeed;
                includerOffset = new Vector3(0.5f, 0, 0.0f);
                // directionChanged = true;
            }
            if (directionChanged)
            {
                _polygonBuilder.Add(_targetTile.transform.position + includerOffset);
                _slidingVelocity = _velocity;
            }
        }
    }

    private Vector2 ReadInput()
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
        return playerInput;
    }

    private IEnumerator MoveToTile(Tile targetTile)
    {
        if (_isMoving)
        {
            yield break;
        }
        _isMoving = true;
        var targetPosition = new Vector3(targetTile.transform.position.x, targetTile.transform.position.y, transform.position.z);
        while (true)
        {
            var distance = Vector3.Distance(targetPosition, transform.position);
            if (distance < 0.01f)
            {
                transform.position = targetPosition;
                _isMoving = false;
                // TODO: do something about this
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
        if (!other.CompareTag(Tags.TileTag))
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
        _trail.enabled = true;
    }

    private void EndSliding(Tile newTile)
    {
        _trail.enabled = false;
        _polygonBuilder.Add(newTile.transform.position);
        _polygonBuilder.Build();
        var tilesToFlip = GetTilesInPolygon();
        foreach (var tile in tilesToFlip)
        {
            tile.MarkResolved();
        }
        CheckCompletion();
    }

    private void CheckCompletion()
    {
        var walkableCount = 0;
        foreach (var tile in _allTiles)
        {
            if (tile.TileType == TileType.Walkable)
            {
                walkableCount++;
            }
        }
        if (walkableCount == _allTiles.Count)
        {
            GameMaster.Instance.MoveToNextLevel();
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